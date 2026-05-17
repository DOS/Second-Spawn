namespace SecondSpawn.Networking
{
    public static class VisualPrefabCatalog
    {
        public const string CleanVisualFolder = "Assets/_SecondSpawn/Prefabs/Characters/GeneratedVisualsV2";
        public const string CleanMaterialFolder = "Assets/_SecondSpawn/Materials/GeneratedVisualsV2";

        public static readonly string[] SourceAssetPaths =
        {
            "Assets/ExplosiveLLC/RPG Character Mecanim Animation Pack/Prefabs/Character/RPG-Character.prefab",
            "Assets/ExplosiveLLC/Warrior Pack Bundle 1 FREE/Brute Warrior Mecanim Animation Pack/Prefabs/Brute Warrior.prefab",
            "Assets/ExplosiveLLC/Warrior Pack Bundle 1 FREE/Karate Warrior Mecanim Animation Pack/Prefabs/Karate Warrior.prefab",
            "Assets/ExplosiveLLC/Warrior Pack Bundle 1 FREE/Ninja Warrior Mecanim Animation Pack/Prefabs/Ninja Warrior.prefab",
            "Assets/ExplosiveLLC/Warrior Pack Bundle 1 FREE/Sorceress Warrior Mecanim Animation Pack/Prefabs/Sorceress Warrior.prefab",
            "Assets/ExplosiveLLC/Warrior Pack Bundle 2 FREE/2 Handed Warrior Mecanim Animation Pack/Prefabs/2Handed Warrior.prefab",
            "Assets/ExplosiveLLC/Warrior Pack Bundle 2 FREE/Archer Warrior Mecanim Animation Pack/Prefabs/Archer Warrior.prefab",
            "Assets/ExplosiveLLC/Warrior Pack Bundle 2 FREE/Knight Warrior Mecanim Animation Pack/Prefabs/Knight Warrior.prefab",
            "Assets/ExplosiveLLC/Warrior Pack Bundle 2 FREE/Mage Warrior Mecanim Animation Pack/Prefabs/Mage Warrior.prefab",
            "Assets/ExplosiveLLC/Warrior Pack Bundle 3 FREE/Crossbow Warrior Mecanim Animation Pack/Prefabs/Crossbow Warrior.prefab",
            "Assets/ExplosiveLLC/Warrior Pack Bundle 3 FREE/Hammer Warrior Mecanim Animation Pack/Prefabs/Hammer Warrior.prefab",
            "Assets/ExplosiveLLC/Warrior Pack Bundle 3 FREE/Spearman Warrior Mecanim Animation Pack/Prefabs/Spearman Warrior.prefab",
            "Assets/ExplosiveLLC/Warrior Pack Bundle 3 FREE/Swordsman Warrior Mecanim Animation Pack/Prefabs/Swordsman Warrior.prefab",
            "Assets/ExplosiveLLC/Fighter Pack Bundle FREE/Fighters/Berserker Fighter Mecanim Animation Pack FREE/Prefabs/Berserker.prefab",
            "Assets/ExplosiveLLC/Fighter Pack Bundle FREE/Fighters/Female Fighter Mecanim Animation Pack FREE/Prefabs/Female.prefab",
            "Assets/ExplosiveLLC/Fighter Pack Bundle FREE/Fighters/Heavy Fighter Mecanim Animation Pack FREE/Prefabs/Heavy.prefab",
            "Assets/ExplosiveLLC/Fighter Pack Bundle FREE/Fighters/Male Fighter Mecanim Animation Pack FREE/Prefabs/Male.prefab",
            "Assets/ExplosiveLLC/Warrior Pack Bundle 1 FREE/Sorceress Warrior Mecanim Animation Pack/Prefabs/Crafter FREE.prefab",
        };

        public static int Count => SourceAssetPaths.Length;

        public static string GetSourceAssetPath(int variant)
        {
            return SourceAssetPaths[NormalizeVariant(variant)];
        }

        public static string GetCleanAssetPath(int variant)
        {
            return $"{CleanVisualFolder}/{GetCleanPrefabName(variant)}";
        }

        public static string GetCleanPrefabName(int variant)
        {
            var index = NormalizeVariant(variant);
            var sourcePath = SourceAssetPaths[index];
            var fileNameStart = sourcePath.LastIndexOf('/') + 1;
            var fileNameEnd = sourcePath.LastIndexOf('.');
            var sourceName = fileNameEnd > fileNameStart
                ? sourcePath[fileNameStart..fileNameEnd]
                : $"Visual{index:00}";

            return $"Visual_{index:00}_{SanitizeFileName(sourceName)}.prefab";
        }

        public static int NormalizeVariant(int variant)
        {
            if (Count == 0)
            {
                return 0;
            }

            var result = variant % Count;
            return result < 0 ? result + Count : result;
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
    }
}
