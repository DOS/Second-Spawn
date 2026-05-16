using SecondSpawn.Networking;
using UnityEngine;

namespace SecondSpawn.UI
{
    /// <summary>
    /// Prototype HUD for combat stats, BodyTime, and future activity surfaces.
    /// The data is read from networked player state that was seeded by the
    /// backend profile. It does not own gameplay authority.
    ///
    /// TODO (slice throughout phases 2-7):
    /// - Replace IMGUI with the production HUD stack.
    /// - Reincarnation flow UI (death -> SECOND token cost -> respawn).
    /// - AI agent activity log overlay (visible on player return).
    /// - See deferred templates .claude/templates/_deferred/hud-design.md
    ///   when this work starts.
    /// </summary>
    public sealed class HUDController : MonoBehaviour
    {
        [SerializeField] private bool _showPrototypeStats = true;
        [SerializeField] private Vector2 _panelPosition = new Vector2(16f, 16f);
        [SerializeField] private Vector2 _panelSize = new Vector2(280f, 132f);

        private NetworkPlayer _cachedPlayer;
        private GUIStyle _labelStyle;

        private void OnGUI()
        {
            if (!_showPrototypeStats)
            {
                return;
            }

            var player = ResolvePlayer();
            if (player == null)
            {
                return;
            }

            _labelStyle ??= new GUIStyle(GUI.skin.label)
            {
                fontSize = 14,
                normal = { textColor = Color.white }
            };

            var rect = new Rect(_panelPosition.x, _panelPosition.y, _panelSize.x, _panelSize.y);
            GUI.Box(rect, "SECOND SPAWN");
            GUILayout.BeginArea(new Rect(rect.x + 12f, rect.y + 24f, rect.width - 24f, rect.height - 32f));
            GUILayout.Label($"Level {player.Level} | Tier {player.CultivationTier}", _labelStyle);
            GUILayout.Label($"HP {player.Hp:0}/{player.MaxHealth} | Energy {player.Stamina:0}/{player.MaxEnergy}", _labelStyle);
            GUILayout.Label($"ATK {player.AttackPower} | DEF {player.DefensePower} | AGI {player.Agility}", _labelStyle);
            GUILayout.Label($"BodyTime {FormatSeconds(player.BodyTimeRemainingSeconds)} / {FormatSeconds(player.BodyTimeMaxSeconds)}", _labelStyle);
            GUILayout.EndArea();
        }

        private NetworkPlayer ResolvePlayer()
        {
            if (_cachedPlayer != null && _cachedPlayer.isActiveAndEnabled)
            {
                return _cachedPlayer;
            }

            var players = FindObjectsByType<NetworkPlayer>(FindObjectsInactive.Exclude, FindObjectsSortMode.None);
            foreach (var player in players)
            {
                if (player.HasInputAuthority)
                {
                    _cachedPlayer = player;
                    return _cachedPlayer;
                }
            }

            _cachedPlayer = players.Length > 0 ? players[0] : null;
            return _cachedPlayer;
        }

        private static string FormatSeconds(int seconds)
        {
            if (seconds <= 0)
            {
                return "0s";
            }

            var days = seconds / 86400;
            var hours = seconds % 86400 / 3600;
            var minutes = seconds % 3600 / 60;
            if (days > 0)
            {
                return $"{days}d {hours}h";
            }

            return hours > 0 ? $"{hours}h {minutes}m" : $"{minutes}m";
        }
    }
}
