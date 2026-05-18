using UnityEngine;

namespace SecondSpawn.Networking
{
    public static class EquipmentVisualCatalog
    {
        public const int None = 0;
        public const int Unarmed = 1;
        public const int OneHandSword = 2;
        public const int TwoHandSword = 3;
        public const int TwoHandSpear = 4;
        public const int TwoHandAxe = 5;
        public const int TwoHandBow = 6;
        public const int TwoHandCrossbow = 7;
        public const int Staff = 8;
        public const int Hammer = 9;

        public static int GetDefaultForVisualVariant(int visualVariant)
        {
            return VisualPrefabCatalog.NormalizeVariant(visualVariant) switch
            {
                1 => Unarmed, // Brute
                2 => Unarmed, // Karate
                3 => OneHandSword, // Ninja
                5 => TwoHandSword, // Two-handed warrior
                6 => TwoHandBow, // Archer
                7 => OneHandSword, // Knight, shield deferred until off-hand equipment exists
                8 => Staff, // Mage
                9 => TwoHandCrossbow,
                10 => Hammer,
                11 => TwoHandSpear,
                12 => OneHandSword,
                13 => TwoHandAxe, // Berserker fighter
                14 => OneHandSword, // Female fighter
                15 => Hammer, // Heavy fighter
                16 => OneHandSword, // Male fighter
                17 => Unarmed, // Crafter
                _ => None
            };
        }

        public static int GetAnimatorWeaponValue(int equipmentVisualId)
        {
            return equipmentVisualId switch
            {
                Unarmed => 0,
                TwoHandSword => 1,
                TwoHandSpear => 2,
                TwoHandAxe => 3,
                TwoHandBow => 4,
                TwoHandCrossbow => 5,
                Staff => 6,
                OneHandSword => 7,
                Hammer => 3,
                _ => -1
            };
        }

        public static int GetVisualIdForKey(string weaponVisualKey)
        {
            var key = NormalizeName(weaponVisualKey);
            return key switch
            {
                "unarmed" => Unarmed,
                "sword" or "one_hand_sword" => OneHandSword,
                "two_hand_sword" => TwoHandSword,
                "spear" or "two_hand_spear" => TwoHandSpear,
                "axe" or "two_hand_axe" => TwoHandAxe,
                "bow" or "two_hand_bow" => TwoHandBow,
                "crossbow" or "two_hand_crossbow" => TwoHandCrossbow,
                "staff" => Staff,
                "hammer" => Hammer,
                _ => None
            };
        }

        public static void ApplyEquipmentVisual(GameObject root, int equipmentVisualId)
        {
            var selectedRoot = FindSelectedWeaponPropRoot(root, equipmentVisualId);
            foreach (var renderer in root.GetComponentsInChildren<Renderer>(includeInactive: true))
            {
                var propRoot = FindWeaponPropRoot(renderer.transform, root.transform);
                if (propRoot == null)
                {
                    continue;
                }

                renderer.enabled = selectedRoot != null && IsSameOrChildOf(renderer.transform, selectedRoot);
            }
        }

        public static bool IsWeaponProp(Transform transform, Transform root)
        {
            return FindWeaponPropRoot(transform, root) != null;
        }

        private static Transform FindSelectedWeaponPropRoot(GameObject root, int equipmentVisualId)
        {
            if (equipmentVisualId == None || equipmentVisualId == Unarmed)
            {
                return null;
            }

            foreach (var renderer in root.GetComponentsInChildren<Renderer>(includeInactive: true))
            {
                var propRoot = FindWeaponPropRoot(renderer.transform, root.transform);
                if (propRoot != null && MatchesEquipment(propRoot.name, equipmentVisualId))
                {
                    return propRoot;
                }
            }

            return null;
        }

        private static Transform FindWeaponPropRoot(Transform transform, Transform root)
        {
            var current = transform;
            Transform lastMatch = null;
            while (current != null && current != root)
            {
                if (IsWeaponPropName(current.name))
                {
                    lastMatch = current;
                }

                current = current.parent;
            }

            return lastMatch;
        }

        private static bool MatchesEquipment(string objectName, int equipmentVisualId)
        {
            var name = NormalizeName(objectName);
            return equipmentVisualId switch
            {
                OneHandSword => name is "sword" or "swordl" or "swordr" ||
                    name.Contains("swordsman-weapon") ||
                    name.Contains("ninja-weapon") ||
                    name.Contains("knight-weapon"),
                TwoHandSword => name == "2hand-sword" || name.Contains("twohanded-weapon"),
                TwoHandSpear => name == "2hand-spear" || name == "spear" || name.Contains("spearman-weapon"),
                TwoHandAxe => name == "2hand-axe" ||
                    name.Contains("berserker-weapon") ||
                    name.Contains("heavy-weapon"),
                TwoHandBow => name == "2hand-bow" || name.Contains("archer-weapon"),
                TwoHandCrossbow => name == "2hand-crossbow" || name.Contains("crossbow-weapon"),
                Staff => name == "staff" || name.Contains("mage-weapon"),
                Hammer => name.Contains("hammer-weapon"),
                _ => false
            };
        }

        private static bool IsWeaponPropName(string objectName)
        {
            var name = NormalizeName(objectName);
            if (string.IsNullOrWhiteSpace(name))
            {
                return false;
            }

            if (name == "weapon" || name.Contains("-weapon") || name.Contains("_weapon") || name.Contains(" weapon"))
            {
                return true;
            }

            if (name.StartsWith("2hand-", System.StringComparison.Ordinal))
            {
                return true;
            }

            if (name.EndsWith("-shield", System.StringComparison.Ordinal) ||
                name.EndsWith("-arrow", System.StringComparison.Ordinal))
            {
                return true;
            }

            return name is "pistol" or "dagger" or "knife" or "sword" or "swordl" or "swordr" or
                "shield" or "mace" or "staff" or "spear" or "axe" or "bow" or "rifle" or
                "gun" or "wand" or "club" or "arrow" or "quiver" or "buckler";
        }

        private static bool IsSameOrChildOf(Transform transform, Transform possibleParent)
        {
            var current = transform;
            while (current != null)
            {
                if (current == possibleParent)
                {
                    return true;
                }

                current = current.parent;
            }

            return false;
        }

        private static string NormalizeName(string value)
        {
            return string.IsNullOrWhiteSpace(value)
                ? string.Empty
                : value.Trim().ToLowerInvariant();
        }
    }
}
