using SecondSpawn.AI;
using SecondSpawn.Networking;
using UnityEngine;

namespace SecondSpawn.UI
{
    /// <summary>
    /// Prototype HUD for combat stats, BodyTime, and agent activity surfaces.
    /// The data is read from networked player state that was seeded by the
    /// backend profile. It does not own gameplay authority.
    ///
    /// TODO (slice throughout phases 2-7):
    /// - Replace IMGUI with the production HUD stack.
    /// - Reincarnation flow UI (death -> SECOND token cost -> respawn).
    /// - See deferred templates .claude/templates/_deferred/hud-design.md
    ///   when this work starts.
    /// </summary>
    public sealed class HUDController : MonoBehaviour
    {
        [SerializeField] private bool _showPrototypeStats = true;
        [SerializeField] private bool _showFrameIdentity = true;
        [SerializeField] private bool _showAgentActivity = true;
        [SerializeField] private Vector2 _panelPosition = new Vector2(16f, 16f);
        [SerializeField] private Vector2 _panelSize = new Vector2(360f, 320f);
        [SerializeField] private int _maxStoryCharacters = 110;
        [SerializeField] private int _maxActivityRows = 4;
        [SerializeField] private int _maxActivitySummaryCharacters = 92;

        private NetworkPlayer _cachedPlayer;
        private CharacterMemorySync _cachedMemorySync;
        private GUIStyle _labelStyle;
        private GUIStyle _headingStyle;
        private GUIStyle _mutedStyle;
        private GUIStyle _wrapStyle;
        private float _nextPlayerRefreshAt;
        private float _nextMemorySyncRefreshAt;
        private Vector2 _scrollPosition;

        private void OnGUI()
        {
            if (!_showPrototypeStats)
            {
                return;
            }

            var player = ResolvePlayer();
            EnsureStyles();

            var rect = ResponsiveRect(_panelPosition, _panelSize);
            GUI.Box(rect, "SECOND SPAWN");
            GUILayout.BeginArea(new Rect(rect.x + 12f, rect.y + 24f, rect.width - 24f, rect.height - 32f));
            _scrollPosition = GUILayout.BeginScrollView(_scrollPosition, false, false);
            DrawStats(player);
            if (_showFrameIdentity)
            {
                DrawFrameIdentity();
            }

            if (_showAgentActivity)
            {
                DrawAgentActivity();
            }

            GUILayout.EndScrollView();
            GUILayout.EndArea();
        }

        private void DrawStats(NetworkPlayer player)
        {
            GUILayout.Label("Current Body", _headingStyle);
            if (player == null)
            {
                GUILayout.Label("Waiting for player body...", _mutedStyle);
                return;
            }

            GUILayout.Label($"Level {player.Level}", _labelStyle);
            GUILayout.Label($"HP {player.Hp:0}/{player.MaxHealth} | Energy {player.Stamina:0}/{player.MaxEnergy}", _labelStyle);
            GUILayout.Label($"ATK {player.AttackPower} | DEF {player.DefensePower} | AGI {player.Agility}", _labelStyle);
            GUILayout.Label($"TIME {FormatSeconds(player.BodyTimeRemainingSeconds)} / {FormatSeconds(player.BodyTimeMaxSeconds)}", _labelStyle);
            GUILayout.Label($"Lifecycle {(player.IsBodyDead ? "dead" : "alive")} | Drain {player.BodyTimeDangerDrainRate}s/tick", _labelStyle);
            GUILayout.Label($"SECOND {FormatSeconds(player.SecondBalanceSeconds)} | Reincarnations {player.ReincarnationCount}", _labelStyle);
        }

        private void DrawFrameIdentity()
        {
            GUILayout.Space(8f);
            GUILayout.Label("Frame Identity", _headingStyle);

            var context = ResolveMemoryContext();
            var body = context?.body;
            if (body == null)
            {
                GUILayout.Label("Waiting for Nakama profile sync...", _mutedStyle);
                return;
            }

            var inhabitation = body.inhabitation;
            var equipment = body.equipment;
            var identity = body.identity;
            var soul = body.soul;
            var story = body.story;

            GUILayout.Label($"Frame: {Fallback(body.body_id, "unknown")}", _labelStyle);
            GUILayout.Label($"Identity: {Fallback(identity?.public_name, soul?.name, context?.player?.display_name, "unknown")} | {Fallback(identity?.callsign, "no callsign")}", _labelStyle);
            GUILayout.Label($"Source: {Fallback(inhabitation?.source_actor_id, "unassigned")}", _labelStyle);
            GUILayout.Label($"Role: {Fallback(identity?.public_role, inhabitation?.previous_role, story?.role, "unknown")}", _labelStyle);
            GUILayout.Label($"Profession: {Fallback(identity?.profession, "unknown")} | {Fallback(identity?.faction_title, "unaffiliated")}", _labelStyle);
            GUILayout.Label($"Archetype: {Fallback(body.archetype_id, "unknown")} | Visual {body.visual_variant}", _labelStyle);
            GUILayout.Label($"Weapon: {FormatWeapon(equipment)} | {Fallback(equipment?.combat_stance, "relaxed")}", _labelStyle);
            GUILayout.Label($"Caps: {FormatCapabilities(body.animation_capabilities)}", _labelStyle);
            GUILayout.Label($"Soul: {Fallback(soul?.name, context?.player?.display_name, "unknown")}", _labelStyle);

            var storyText = TrimForHud(Fallback(story?.origin, story?.rumor, ""));
            if (!string.IsNullOrWhiteSpace(storyText))
            {
                GUILayout.Label(storyText, _wrapStyle);
            }
        }

        private void DrawAgentActivity()
        {
            GUILayout.Space(8f);
            GUILayout.Label("Agent Runtime", _headingStyle);

            var body = ResolveMemoryContext()?.body;
            if (body == null)
            {
                GUILayout.Label("Waiting for Nakama profile sync...", _mutedStyle);
                return;
            }

            var runtime = body.agent_runtime;
            if (runtime == null)
            {
                GUILayout.Label("No runtime counters recorded yet.", _mutedStyle);
                return;
            }

            GUILayout.Label($"Decisions {runtime.decision_count} | Fallback {runtime.fallback_decision_count} | Activities {runtime.activity_count}", _labelStyle);
            GUILayout.Label($"Intents move:{runtime.move_intent_count} say:{runtime.say_intent_count} stop:{runtime.stop_intent_count} interact:{runtime.interact_intent_count}", _labelStyle);
            GUILayout.Label($"Offline {FormatSeconds(runtime.offline_seconds)} | Last {FormatTimestamp(runtime.last_activity_at)}", _labelStyle);

            var activities = body.agent_activity;
            if (activities == null || activities.Length == 0)
            {
                GUILayout.Label("No recent activity.", _mutedStyle);
                return;
            }

            GUILayout.Label("Recent Activity", _headingStyle);
            var count = Mathf.Min(Mathf.Max(1, _maxActivityRows), activities.Length);
            for (var i = 0; i < count; i++)
            {
                var activity = activities[i];
                if (activity == null)
                {
                    continue;
                }

                var summary = TrimForHud(Fallback(activity.summary, "activity"), _maxActivitySummaryCharacters);
                GUILayout.Label($"{FormatTimestamp(activity.occurred_at)} [{Fallback(activity.kind, "activity")}/{Fallback(activity.source, "unknown")}]", _mutedStyle);
                GUILayout.Label(summary, _wrapStyle);
            }
        }

        private NetworkPlayer ResolvePlayer()
        {
            if (_cachedPlayer != null && _cachedPlayer.isActiveAndEnabled)
            {
                return _cachedPlayer;
            }

            if (Time.unscaledTime < _nextPlayerRefreshAt)
            {
                return null;
            }

            _nextPlayerRefreshAt = Time.unscaledTime + 0.5f;
            var players = FindObjectsByType<NetworkPlayer>(FindObjectsInactive.Exclude);
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

        private AgentContextDto ResolveMemoryContext()
        {
            if (_cachedMemorySync != null && _cachedMemorySync.isActiveAndEnabled)
            {
                return _cachedMemorySync.Context;
            }

            if (Time.unscaledTime < _nextMemorySyncRefreshAt)
            {
                return null;
            }

            _nextMemorySyncRefreshAt = Time.unscaledTime + 0.5f;
            _cachedMemorySync = FindAnyObjectByType<CharacterMemorySync>(FindObjectsInactive.Exclude);
            return _cachedMemorySync != null ? _cachedMemorySync.Context : null;
        }

        private void EnsureStyles()
        {
            _labelStyle ??= new GUIStyle(GUI.skin.label)
            {
                fontSize = 14,
                normal = { textColor = Color.white }
            };

            _headingStyle ??= new GUIStyle(_labelStyle)
            {
                fontSize = 15,
                fontStyle = FontStyle.Bold,
                normal = { textColor = new Color(0.68f, 0.88f, 1f) }
            };

            _mutedStyle ??= new GUIStyle(_labelStyle)
            {
                normal = { textColor = new Color(0.75f, 0.78f, 0.82f) }
            };

            _wrapStyle ??= new GUIStyle(_mutedStyle)
            {
                wordWrap = true
            };
        }

        private static Rect ResponsiveRect(Vector2 position, Vector2 requestedSize)
        {
            const float margin = 12f;
            var x = Mathf.Clamp(position.x, margin, Mathf.Max(margin, Screen.width - margin));
            var y = Mathf.Clamp(position.y, margin, Mathf.Max(margin, Screen.height - margin));
            var width = Mathf.Clamp(requestedSize.x, 260f, Mathf.Max(260f, Screen.width - x - margin));
            var height = Mathf.Clamp(requestedSize.y, 220f, Mathf.Max(220f, Screen.height - y - margin));
            return new Rect(x, y, width, height);
        }

        private static string FormatSeconds(int seconds)
        {
            return FormatSeconds((long)seconds);
        }

        private static string FormatSeconds(long seconds)
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

            if (hours == 0 && minutes == 0)
            {
                return $"{seconds}s";
            }

            return hours > 0 ? $"{hours}h {minutes}m" : $"{minutes}m";
        }

        private static string FormatTimestamp(string timestamp)
        {
            if (string.IsNullOrWhiteSpace(timestamp))
            {
                return "never";
            }

            var trimmed = timestamp.Trim();
            return trimmed.Length > 16 ? trimmed.Substring(0, 16).Replace('T', ' ') : trimmed.Replace('T', ' ');
        }

        private static string FormatCapabilities(AnimationCapabilitiesDto capabilities)
        {
            if (capabilities == null)
            {
                return "jump, roll, melee";
            }

            return $"jump:{FormatBool(capabilities.supports_jump)} roll:{FormatBool(capabilities.supports_roll)} melee:{FormatBool(capabilities.supports_melee)} ranged:{FormatBool(capabilities.supports_ranged)}";
        }

        private static string FormatWeapon(EquipmentLoadoutDto equipment)
        {
            if (equipment == null)
            {
                return "none";
            }

            var weaponName = Fallback(equipment.weapon_visual_key, equipment.primary_weapon, "none");
            return equipment.equipment_visual_id > 0 ? $"{weaponName} #{equipment.equipment_visual_id}" : weaponName;
        }

        private static string FormatBool(bool value)
        {
            return value ? "Y" : "N";
        }

        private string TrimForHud(string value)
        {
            return TrimForHud(value, _maxStoryCharacters);
        }

        private static string TrimForHud(string value, int maxCharacters)
        {
            if (string.IsNullOrWhiteSpace(value))
            {
                return "";
            }

            var trimmed = value.Trim();
            if (maxCharacters <= 0 || trimmed.Length <= maxCharacters)
            {
                return trimmed;
            }

            return trimmed.Substring(0, Mathf.Max(0, maxCharacters - 3)).TrimEnd() + "...";
        }

        private static string Fallback(params string[] values)
        {
            foreach (var value in values)
            {
                if (!string.IsNullOrWhiteSpace(value))
                {
                    return value.Trim();
                }
            }

            return "";
        }
    }
}
