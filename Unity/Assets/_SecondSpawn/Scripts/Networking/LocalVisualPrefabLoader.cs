using System.Collections;
using Fusion;
using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif

namespace SecondSpawn.Networking
{
    /// <summary>
    /// Loads an optional local-only visual prefab under the networked player.
    /// The loaded object is cosmetic only: Fusion and Simple KCC remain the
    /// authoritative movement stack.
    /// </summary>
    [DisallowMultipleComponent]
    public sealed class LocalVisualPrefabLoader : MonoBehaviour
    {
#if UNITY_EDITOR
        private const string SharedAnimatorControllerPath =
            "Assets/ExplosiveLLC/RPG Character Mecanim Animation Pack/Animation Controller/RPG-Character-Animation-Controller.controller";
#endif

        [SerializeField, Tooltip("Resources path fallback for a local-only visual prefab. Missing resources leave the committed cube visible.")]
        private string _resourcePath = "SecondSpawn/RPGCharacterVisual";

        [SerializeField, Tooltip("Offset for the loaded visual prefab.")]
        private Vector3 _localPosition = new(0f, -0.5f, 0f);

        [SerializeField, Tooltip("Euler rotation for the loaded visual prefab.")]
        private Vector3 _localEulerAngles;

        [SerializeField, Tooltip("Scale for the loaded visual prefab.")]
        private Vector3 _localScale = Vector3.one;

        [SerializeField, Tooltip("Hide placeholder renderers on this root when the visual prefab loads.")]
        private bool _hideRootRenderers = true;

        [SerializeField, Tooltip("Align the visual renderer bounds bottom to the network root ground plane after loading. This avoids tall/short local asset variants sinking into the floor.")]
        private bool _alignFeetToGround = true;

        [SerializeField, Tooltip("Extra vertical offset after foot-ground alignment.")]
        private float _feetGroundOffset;

        [SerializeField, Tooltip("Optional runtime fallback for assets that fail under the active render pipeline. Generated Second Spawn visuals should already use URP material copies.")]
        private bool _convertMaterialsToUrp;

        [SerializeField, Tooltip("Apply the networked equipment visual and hide all other vendor weapon prop meshes.")]
        private bool _hidePrototypeWeaponProps = true;

        private GameObject _instance;

        private void Start()
        {
            if (Application.isBatchMode || _instance != null)
            {
                return;
            }

            var visualKey = ResolveVisualKey();
            var visualPrefab = LoadVisualPrefab(visualKey);
            if (visualPrefab == null)
            {
                Debug.LogWarning("[LocalVisualPrefabLoader] No local visual prefab found. Keeping placeholder cube.");
                return;
            }

            _instance = Instantiate(visualPrefab, transform);
            _instance.name = visualPrefab.name;
            _instance.transform.SetLocalPositionAndRotation(_localPosition, Quaternion.Euler(_localEulerAngles));
            _instance.transform.localScale = _localScale;

            NormalizeVisualTransform(_instance);
            DisablePhysics(_instance);
            var equipmentVisualId = GetNetworkEquipmentVisualId();
            if (_hidePrototypeWeaponProps)
            {
                EquipmentVisualCatalog.ApplyEquipmentVisual(_instance, equipmentVisualId);
            }

            if (_convertMaterialsToUrp)
            {
                ConfigureMaterials(_instance);
            }

            var animator = ConfigureAnimator(_instance);
            if (_alignFeetToGround)
            {
                AlignVisualFeetToGround(_instance, transform.position.y + _feetGroundOffset);
                StartCoroutine(RealignVisualFeetAfterAnimatorPose());
            }

            if (animator != null && TryGetComponent<NetworkAnimatorBridge>(out var animatorBridge))
            {
                animatorBridge.SetAnimator(animator);
            }

            if (_hideRootRenderers)
            {
                foreach (var renderer in GetComponents<Renderer>())
                {
                    renderer.enabled = false;
                }
            }

            Debug.Log($"[LocalVisualPrefabLoader] Loaded local visual '{visualKey}' with equipment visual {equipmentVisualId}.");
        }

        private IEnumerator RealignVisualFeetAfterAnimatorPose()
        {
            yield return null;
            yield return null;
            if (_instance != null)
            {
                AlignVisualFeetToGround(_instance, transform.position.y + _feetGroundOffset);
            }
        }

        private string ResolveVisualKey()
        {
#if UNITY_EDITOR
            var visualVariant = GetNetworkVisualVariant();
            var cleanPath = VisualPrefabCatalog.GetCleanAssetPath(visualVariant);
            if (AssetDatabase.LoadAssetAtPath<GameObject>(cleanPath) != null)
            {
                return cleanPath;
            }

            var sourcePath = VisualPrefabCatalog.GetSourceAssetPath(visualVariant);
            if (!string.IsNullOrWhiteSpace(sourcePath))
            {
                Debug.LogWarning($"[LocalVisualPrefabLoader] Generated visual prefab missing for variant {visualVariant}. Falling back to source asset '{sourcePath}'. Run Second Spawn/Art/Rebuild Generated Visual Prefabs to refresh clean visuals.");
                return sourcePath;
            }

            return _resourcePath;
#else
            return _resourcePath;
#endif
        }

        private GameObject LoadVisualPrefab(string visualKey)
        {
            if (string.IsNullOrWhiteSpace(visualKey))
            {
                return null;
            }

#if UNITY_EDITOR
            if (visualKey.StartsWith("Assets/", System.StringComparison.Ordinal))
            {
                return AssetDatabase.LoadAssetAtPath<GameObject>(visualKey);
            }
#endif

            return Resources.Load<GameObject>(visualKey);
        }

        private int GetNetworkVisualVariant()
        {
            var networkPlayer = GetComponentInParent<NetworkPlayer>();
            if (networkPlayer != null)
            {
                return networkPlayer.VisualVariant;
            }

            return Random.Range(0, int.MaxValue);
        }

        private static void NormalizeVisualTransform(GameObject root)
        {
            root.transform.localRotation = Quaternion.identity;
            root.transform.localScale = Vector3.one;
        }

        private static void AlignVisualFeetToGround(GameObject root, float targetWorldY)
        {
            var renderers = root.GetComponentsInChildren<Renderer>(includeInactive: true);
            if (renderers.Length == 0)
            {
                return;
            }

            var minY = float.PositiveInfinity;
            foreach (var renderer in renderers)
            {
                if (EquipmentVisualCatalog.IsWeaponProp(renderer.transform, root.transform))
                {
                    continue;
                }

                minY = Mathf.Min(minY, renderer.bounds.min.y);
            }

            if (!float.IsFinite(minY))
            {
                return;
            }

            var position = root.transform.position;
            position.y += targetWorldY - minY;
            root.transform.position = position;
        }

        private static void DisablePhysics(GameObject root)
        {
            foreach (var collider in root.GetComponentsInChildren<Collider>(includeInactive: true))
            {
                collider.enabled = false;
            }

            foreach (var rigidbody in root.GetComponentsInChildren<Rigidbody>(includeInactive: true))
            {
                rigidbody.isKinematic = true;
                rigidbody.useGravity = false;
            }
        }

        private int GetNetworkEquipmentVisualId()
        {
            var networkPlayer = GetComponentInParent<NetworkPlayer>();
            if (networkPlayer != null)
            {
                return networkPlayer.EquipmentVisualId != EquipmentVisualCatalog.None
                    ? networkPlayer.EquipmentVisualId
                    : EquipmentVisualCatalog.GetDefaultForVisualVariant(networkPlayer.VisualVariant);
            }

            return EquipmentVisualCatalog.GetDefaultForVisualVariant(GetNetworkVisualVariant());
        }

        private static void ConfigureMaterials(GameObject root)
        {
            var runtimeShader = FindRuntimeShader();
            if (runtimeShader == null)
            {
                Debug.LogWarning("[LocalVisualPrefabLoader] No URP runtime shader found for vendor material conversion.");
                return;
            }

            foreach (var renderer in root.GetComponentsInChildren<Renderer>(includeInactive: true))
            {
                var materials = renderer.materials;

                for (var i = 0; i < materials.Length; i++)
                {
                    materials[i] = CreateRuntimeMaterial(materials[i], runtimeShader);
                }

                renderer.materials = materials;
            }
        }

        private static Shader FindRuntimeShader()
        {
            return Shader.Find("Universal Render Pipeline/Lit") ??
                   Shader.Find("Universal Render Pipeline/Simple Lit") ??
                   Shader.Find("Universal Render Pipeline/Unlit");
        }

        private static Material CreateRuntimeMaterial(Material source, Shader runtimeShader)
        {
            var material = new Material(runtimeShader)
            {
                name = source != null ? $"{source.name}_RuntimeLit" : "RuntimeLit"
            };

            var sourceColor = Color.white;
            if (source != null)
            {
                if (source.HasProperty("_BaseColor"))
                {
                    sourceColor = source.GetColor("_BaseColor");
                }
                else if (source.HasProperty("_Color"))
                {
                    sourceColor = source.GetColor("_Color");
                }

                Texture mainTexture = null;
                if (source.HasProperty("_BaseMap"))
                {
                    mainTexture = source.GetTexture("_BaseMap");
                }
                else if (source.HasProperty("_MainTex"))
                {
                    mainTexture = source.GetTexture("_MainTex");
                }

                if (mainTexture != null)
                {
                    if (material.HasProperty("_BaseMap"))
                    {
                        material.SetTexture("_BaseMap", mainTexture);
                        material.SetTextureScale("_BaseMap", source.GetTextureScale(source.HasProperty("_BaseMap") ? "_BaseMap" : "_MainTex"));
                        material.SetTextureOffset("_BaseMap", source.GetTextureOffset(source.HasProperty("_BaseMap") ? "_BaseMap" : "_MainTex"));
                    }
                    else if (material.HasProperty("_MainTex"))
                    {
                        material.SetTexture("_MainTex", mainTexture);
                        material.SetTextureScale("_MainTex", source.GetTextureScale(source.HasProperty("_BaseMap") ? "_BaseMap" : "_MainTex"));
                        material.SetTextureOffset("_MainTex", source.GetTextureOffset(source.HasProperty("_BaseMap") ? "_BaseMap" : "_MainTex"));
                    }
                }
            }

            if (material.HasProperty("_BaseColor"))
            {
                material.SetColor("_BaseColor", sourceColor);
            }
            else if (material.HasProperty("_Color"))
            {
                material.SetColor("_Color", sourceColor);
            }

            return material;
        }

        private static Animator ConfigureAnimator(GameObject root)
        {
            var animator = root.GetComponentInChildren<Animator>(includeInactive: true);
            if (animator == null)
            {
                return null;
            }

            animator.applyRootMotion = false;
            animator.cullingMode = AnimatorCullingMode.AlwaysAnimate;
            animator.updateMode = AnimatorUpdateMode.Normal;
            animator.speed = 1f;
            EnsureSharedController(animator);
            animator.Rebind();
            animator.Update(0f);

            if (animator.GetComponent<AnimationEventReceiver>() == null)
            {
                animator.gameObject.AddComponent<AnimationEventReceiver>();
            }

            if (animator.GetComponent<VisualAnimationIntentDriver>() == null)
            {
                animator.gameObject.AddComponent<VisualAnimationIntentDriver>();
            }

            return animator;
        }

        private static void EnsureSharedController(Animator animator)
        {
#if UNITY_EDITOR
            if (animator.runtimeAnimatorController != null && ExposesLocomotionContract(animator))
            {
                return;
            }

            var sharedController = AssetDatabase.LoadAssetAtPath<RuntimeAnimatorController>(SharedAnimatorControllerPath);
            if (sharedController != null)
            {
                animator.runtimeAnimatorController = sharedController;
            }
            else
            {
                Debug.LogWarning($"[LocalVisualPrefabLoader] Shared animator controller not found at '{SharedAnimatorControllerPath}'.");
            }
#endif
        }

        private static bool ExposesLocomotionContract(Animator animator)
        {
            foreach (var parameter in animator.parameters)
            {
                if (parameter.name is "Moving" or "Velocity" or "Velocity X" or "Velocity Z")
                {
                    return true;
                }
            }

            return false;
        }

        public void ApplyEquipmentVisual(int equipmentVisualId)
        {
            if (!_hidePrototypeWeaponProps || _instance == null)
            {
                return;
            }

            EquipmentVisualCatalog.ApplyEquipmentVisual(_instance, equipmentVisualId);
        }
    }
}
