#if UNITY_EDITOR
using System.IO;
using System.Collections.Generic;
using SecondSpawn.Networking;
using UnityEditor;
using UnityEngine;
using UnityEngine.AI;

namespace SecondSpawn.EditorTools
{
    public static class SecondSpawnVisualPrefabUtility
    {
        [MenuItem("Second Spawn/Art/Rebuild Generated Visual Prefabs")]
        public static void RebuildGeneratedVisualPrefabs()
        {
            EnsureFolder(VisualPrefabCatalog.CleanVisualFolder);
            EnsureFolder(VisualPrefabCatalog.CleanMaterialFolder);
            DeleteExistingGeneratedPrefabs();
            DeleteExistingGeneratedMaterials();

            var generatedCount = 0;
            var materialCache = new Dictionary<Material, Material>();
            for (var i = 0; i < VisualPrefabCatalog.Count; i++)
            {
                var sourcePath = VisualPrefabCatalog.GetSourceAssetPath(i);
                var sourcePrefab = AssetDatabase.LoadAssetAtPath<GameObject>(sourcePath);
                if (sourcePrefab == null)
                {
                    Debug.LogWarning($"[SecondSpawnVisualPrefabUtility] Source visual prefab missing: {sourcePath}");
                    continue;
                }

                var instance = PrefabUtility.InstantiatePrefab(sourcePrefab) as GameObject;
                if (instance == null)
                {
                    instance = Object.Instantiate(sourcePrefab);
                }

                instance.name = Path.GetFileNameWithoutExtension(VisualPrefabCatalog.GetCleanPrefabName(i));
                try
                {
                    if (PrefabUtility.IsPartOfPrefabInstance(instance))
                    {
                        PrefabUtility.UnpackPrefabInstance(instance, PrefabUnpackMode.Completely, InteractionMode.AutomatedAction);
                    }

                    StripGameplayScripts(instance);
                    StripNonVisualRuntimeComponents(instance);
                    PrepareVisualRoot(instance);
                    AssignSharedAnimatorController(instance);
                    ConvertMaterialsToUrp(instance, materialCache);
                    EquipmentVisualCatalog.ApplyEquipmentVisual(instance, EquipmentVisualCatalog.GetDefaultForVisualVariant(i));

                    var cleanPath = VisualPrefabCatalog.GetCleanAssetPath(i);
                    PrefabUtility.SaveAsPrefabAsset(instance, cleanPath);
                    generatedCount++;
                }
                finally
                {
                    Object.DestroyImmediate(instance);
                }
            }

            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
            Debug.Log($"[SecondSpawnVisualPrefabUtility] Generated {generatedCount} clean visual prefab(s).");
        }

        private static void DeleteExistingGeneratedPrefabs()
        {
            var prefabGuids = AssetDatabase.FindAssets("t:Prefab", new[] { VisualPrefabCatalog.CleanVisualFolder });
            foreach (var guid in prefabGuids)
            {
                var prefabPath = AssetDatabase.GUIDToAssetPath(guid);
                if (prefabPath.StartsWith(VisualPrefabCatalog.CleanVisualFolder, System.StringComparison.Ordinal))
                {
                    AssetDatabase.DeleteAsset(prefabPath);
                }
            }
        }

        private static void DeleteExistingGeneratedMaterials()
        {
            if (!AssetDatabase.IsValidFolder(VisualPrefabCatalog.CleanMaterialFolder))
            {
                return;
            }

            var materialGuids = AssetDatabase.FindAssets("t:Material", new[] { VisualPrefabCatalog.CleanMaterialFolder });
            foreach (var guid in materialGuids)
            {
                var materialPath = AssetDatabase.GUIDToAssetPath(guid);
                if (materialPath.StartsWith(VisualPrefabCatalog.CleanMaterialFolder, System.StringComparison.Ordinal))
                {
                    AssetDatabase.DeleteAsset(materialPath);
                }
            }
        }

        private static void StripGameplayScripts(GameObject root)
        {
            foreach (var transform in root.GetComponentsInChildren<Transform>(includeInactive: true))
            {
                var gameObject = transform.gameObject;
                GameObjectUtility.RemoveMonoBehavioursWithMissingScript(gameObject);

                foreach (var behaviour in gameObject.GetComponents<MonoBehaviour>())
                {
                    if (behaviour != null)
                    {
                        Object.DestroyImmediate(behaviour);
                    }
                }
            }
        }

        private static void StripNonVisualRuntimeComponents(GameObject root)
        {
            foreach (var agent in root.GetComponentsInChildren<NavMeshAgent>(includeInactive: true))
            {
                Object.DestroyImmediate(agent);
            }

            foreach (var controller in root.GetComponentsInChildren<CharacterController>(includeInactive: true))
            {
                Object.DestroyImmediate(controller);
            }

            foreach (var collider in root.GetComponentsInChildren<Collider>(includeInactive: true))
            {
                Object.DestroyImmediate(collider);
            }

            foreach (var joint in root.GetComponentsInChildren<Joint>(includeInactive: true))
            {
                Object.DestroyImmediate(joint);
            }

            foreach (var rigidbody in root.GetComponentsInChildren<Rigidbody>(includeInactive: true))
            {
                Object.DestroyImmediate(rigidbody);
            }
        }

        private static void PrepareVisualRoot(GameObject root)
        {
            foreach (var transform in root.GetComponentsInChildren<Transform>(includeInactive: true))
            {
                var gameObject = transform.gameObject;
                gameObject.tag = "Untagged";
                gameObject.layer = 0;
            }

            root.transform.SetPositionAndRotation(Vector3.zero, Quaternion.identity);
            root.transform.localScale = Vector3.one;
        }

        private static void AssignSharedAnimatorController(GameObject root)
        {
            var sharedController = VisualAnimatorControllerCatalog.LoadSharedController();
            if (sharedController == null)
            {
                Debug.LogWarning($"[SecondSpawnVisualPrefabUtility] Shared animator controller not found at '{VisualAnimatorControllerCatalog.GetSharedControllerLookupLabel()}'.");
                return;
            }

            foreach (var animator in root.GetComponentsInChildren<Animator>(includeInactive: true))
            {
                animator.runtimeAnimatorController = sharedController;
                EditorUtility.SetDirty(animator);
            }
        }

        private static void ConvertMaterialsToUrp(GameObject root, Dictionary<Material, Material> materialCache)
        {
            var urpShader = Shader.Find("Universal Render Pipeline/Simple Lit") ??
                            Shader.Find("Universal Render Pipeline/Lit");
            if (urpShader == null)
            {
                Debug.LogWarning("[SecondSpawnVisualPrefabUtility] URP shader not found. Generated visuals keep source materials.");
                return;
            }

            foreach (var renderer in root.GetComponentsInChildren<Renderer>(includeInactive: true))
            {
                var sourceMaterials = renderer.sharedMaterials;
                var convertedMaterials = new Material[sourceMaterials.Length];
                var changed = false;

                for (var i = 0; i < sourceMaterials.Length; i++)
                {
                    var sourceMaterial = sourceMaterials[i];
                    convertedMaterials[i] = GetOrCreateUrpMaterial(sourceMaterial, urpShader, materialCache);
                    changed |= convertedMaterials[i] != sourceMaterial;
                }

                if (changed)
                {
                    renderer.sharedMaterials = convertedMaterials;
                    EditorUtility.SetDirty(renderer);
                }
            }
        }

        private static Material GetOrCreateUrpMaterial(
            Material sourceMaterial,
            Shader urpShader,
            Dictionary<Material, Material> materialCache)
        {
            if (sourceMaterial == null)
            {
                return null;
            }

            if (sourceMaterial.shader != null && sourceMaterial.shader.name.StartsWith("Universal Render Pipeline/", System.StringComparison.Ordinal))
            {
                return sourceMaterial;
            }

            if (materialCache.TryGetValue(sourceMaterial, out var cachedMaterial))
            {
                return cachedMaterial;
            }

            var materialName = $"{SanitizeFileName(CleanGeneratedAssetName(sourceMaterial.name))}_URP";
            var materialPath = AssetDatabase.GenerateUniqueAssetPath($"{VisualPrefabCatalog.CleanMaterialFolder}/{materialName}.mat");
            var convertedMaterial = new Material(urpShader)
            {
                name = Path.GetFileNameWithoutExtension(materialPath),
                enableInstancing = sourceMaterial.enableInstancing,
                doubleSidedGI = sourceMaterial.doubleSidedGI
            };

            CopyColor(sourceMaterial, convertedMaterial);
            CopyMainTexture(sourceMaterial, convertedMaterial);
            CopyEmission(sourceMaterial, convertedMaterial);

            AssetDatabase.CreateAsset(convertedMaterial, materialPath);
            materialCache[sourceMaterial] = convertedMaterial;
            return convertedMaterial;
        }

        private static void CopyColor(Material sourceMaterial, Material convertedMaterial)
        {
            var color = Color.white;
            if (sourceMaterial.HasProperty("_BaseColor"))
            {
                color = sourceMaterial.GetColor("_BaseColor");
            }
            else if (sourceMaterial.HasProperty("_Color"))
            {
                color = sourceMaterial.GetColor("_Color");
            }

            if (convertedMaterial.HasProperty("_BaseColor"))
            {
                convertedMaterial.SetColor("_BaseColor", color);
            }
            else if (convertedMaterial.HasProperty("_Color"))
            {
                convertedMaterial.SetColor("_Color", color);
            }
        }

        private static void CopyMainTexture(Material sourceMaterial, Material convertedMaterial)
        {
            var textureProperty = sourceMaterial.HasProperty("_BaseMap") ? "_BaseMap" : "_MainTex";
            Texture mainTexture = null;
            if (sourceMaterial.HasProperty(textureProperty))
            {
                mainTexture = sourceMaterial.GetTexture(textureProperty);
            }

            if (mainTexture == null)
            {
                return;
            }

            var targetProperty = convertedMaterial.HasProperty("_BaseMap") ? "_BaseMap" : "_MainTex";
            if (!convertedMaterial.HasProperty(targetProperty))
            {
                return;
            }

            convertedMaterial.SetTexture(targetProperty, mainTexture);
            convertedMaterial.SetTextureScale(targetProperty, sourceMaterial.GetTextureScale(textureProperty));
            convertedMaterial.SetTextureOffset(targetProperty, sourceMaterial.GetTextureOffset(textureProperty));
        }

        private static void CopyEmission(Material sourceMaterial, Material convertedMaterial)
        {
            Texture emissionTexture = null;
            if (sourceMaterial.HasProperty("_EmissionMap"))
            {
                emissionTexture = sourceMaterial.GetTexture("_EmissionMap");
            }
            else if (sourceMaterial.HasProperty("_Illum"))
            {
                emissionTexture = sourceMaterial.GetTexture("_Illum");
            }

            if (emissionTexture == null || !convertedMaterial.HasProperty("_EmissionMap"))
            {
                return;
            }

            convertedMaterial.SetTexture("_EmissionMap", emissionTexture);
            if (convertedMaterial.HasProperty("_EmissionColor"))
            {
                convertedMaterial.SetColor("_EmissionColor", Color.white);
            }

            convertedMaterial.EnableKeyword("_EMISSION");
        }

        private static void EnsureFolder(string assetFolder)
        {
            var segments = assetFolder.Split('/');
            var current = segments[0];
            for (var i = 1; i < segments.Length; i++)
            {
                var next = $"{current}/{segments[i]}";
                if (!AssetDatabase.IsValidFolder(next))
                {
                    AssetDatabase.CreateFolder(current, segments[i]);
                }

                current = next;
            }
        }

        private static string SanitizeFileName(string value)
        {
            var chars = value.ToCharArray();
            for (var i = 0; i < chars.Length; i++)
            {
                var c = chars[i];
                if (!(char.IsLetterOrDigit(c) || c == '-' || c == '_'))
                {
                    chars[i] = '_';
                }
            }

            return new string(chars);
        }

        private static string CleanGeneratedAssetName(string value)
        {
            var normalized = value.Trim();
            const string freeSuffix = " FREE";
            return normalized.EndsWith(freeSuffix, System.StringComparison.OrdinalIgnoreCase)
                ? normalized[..^freeSuffix.Length]
                : normalized;
        }
    }
}
#endif
