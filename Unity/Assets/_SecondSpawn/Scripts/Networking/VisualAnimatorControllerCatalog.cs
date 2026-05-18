using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif

namespace SecondSpawn.Networking
{
    public static class VisualAnimatorControllerCatalog
    {
        public const string ProjectSharedControllerPath =
            "Assets/_SecondSpawn/Art/Animations/Controllers/SecondSpawn-Humanoid-Animation-Controller.controller";

        public const string VendorRpgControllerPath =
            "Assets/ExplosiveLLC/RPG Character Mecanim Animation Pack/Animation Controller/RPG-Character-Animation-Controller.controller";

#if UNITY_EDITOR
        public static RuntimeAnimatorController LoadSharedController()
        {
            return AssetDatabase.LoadAssetAtPath<RuntimeAnimatorController>(ProjectSharedControllerPath) ??
                   AssetDatabase.LoadAssetAtPath<RuntimeAnimatorController>(VendorRpgControllerPath);
        }

        public static string GetSharedControllerLookupLabel()
        {
            return $"'{ProjectSharedControllerPath}' or '{VendorRpgControllerPath}'";
        }
#endif
    }
}
