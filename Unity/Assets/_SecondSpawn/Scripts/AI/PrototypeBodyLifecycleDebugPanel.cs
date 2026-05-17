using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;

namespace SecondSpawn.AI
{
    /// <summary>
    /// Prototype-only IMGUI controls for exercising the BodyTime death and
    /// reincarnation loop against Nakama during Play Mode.
    /// </summary>
    [DisallowMultipleComponent]
    [RequireComponent(typeof(CharacterMemorySync))]
    public sealed class PrototypeBodyLifecycleDebugPanel : MonoBehaviour
    {
        private const string EarnSource = "prototype_safe_farming";
        private const string SpendSource = "prototype_service";
        private const string DrainSource = "danger_zone_tick";
        private const string DebugFatalDrainSource = "prototype_reincarnation_debug";

        [SerializeField] private bool _showPanel = true;
        [SerializeField] private Key _toggleKey = Key.F2;
        [SerializeField] private Vector2 _panelPosition = new Vector2(16f, 212f);
        [SerializeField] private Vector2 _panelSize = new Vector2(320f, 224f);
        [SerializeField] private long _earnSeconds = 300;
        [SerializeField] private long _spendSeconds = 600;
        [SerializeField] private long _dangerDrainSeconds = 300;
        [SerializeField] private string _rewardObjectiveId = "prototype-training-drone";

        private CharacterMemorySync _memorySync;
        private GUIStyle _labelStyle;
        private bool _busy;
        private string _status = "Ready";

        private void Awake()
        {
            _memorySync = GetComponent<CharacterMemorySync>();
        }

        private void Update()
        {
            if (PrototypeDebugInput.WasPressedThisFrame(_toggleKey, Key.F2))
            {
                _showPanel = !_showPanel;
            }
        }

        private void OnGUI()
        {
            if (!_showPanel)
            {
                return;
            }

            _labelStyle ??= new GUIStyle(GUI.skin.label)
            {
                fontSize = 13,
                normal = { textColor = Color.white }
            };

            var context = _memorySync != null ? _memorySync.Context : null;
            var rect = new Rect(_panelPosition.x, _panelPosition.y, _panelSize.x, _panelSize.y);
            GUI.Box(rect, "Body Lifecycle Debug");
            GUILayout.BeginArea(new Rect(rect.x + 12f, rect.y + 24f, rect.width - 24f, rect.height - 32f));
            GUILayout.Label(BuildContextLine(context), _labelStyle);
            GUILayout.Label(_status, _labelStyle);

            GUI.enabled = !_busy;
            if (GUILayout.Button("Refresh Profile"))
            {
                StartOperation(_memorySync.Refresh(), "Refresh");
            }

            GUILayout.BeginHorizontal();
            _rewardObjectiveId = GUILayout.TextField(_rewardObjectiveId);
            if (GUILayout.Button("Claim Reward", GUILayout.Width(112f)))
            {
                StartOperation(_memorySync.ClaimPrototypeReward(SafeRewardObjectiveId()), "Reward");
            }
            GUILayout.EndHorizontal();

            GUILayout.BeginHorizontal();
            if (GUILayout.Button($"+{FormatSeconds(_earnSeconds)}"))
            {
                StartBodyTimeEvent("earn", EarnSource, _earnSeconds, "Prototype safe farming reward.");
            }

            if (GUILayout.Button($"-{FormatSeconds(_spendSeconds)}"))
            {
                StartBodyTimeEvent("spend", SpendSource, _spendSeconds, "Prototype service spend.");
            }

            if (GUILayout.Button($"Drain {FormatSeconds(_dangerDrainSeconds)}"))
            {
                StartBodyTimeEvent("drain", DrainSource, _dangerDrainSeconds, "Prototype danger tick.");
            }
            GUILayout.EndHorizontal();

            GUILayout.BeginHorizontal();
            if (GUILayout.Button("Force Zero Time"))
            {
                var remaining = context?.body?.time?.remaining_seconds ?? 1;
                StartBodyTimeEvent("drain", DebugFatalDrainSource, Mathf.Max(1, ToIntSeconds(remaining)), "Debug fatal drain for reincarnation smoke.");
            }

            if (GUILayout.Button("Reincarnate"))
            {
                StartOperation(_memorySync.ReincarnateCurrentBody("Unity prototype debug reincarnation."), "Reincarnate");
            }
            GUILayout.EndHorizontal();
            GUI.enabled = true;

            GUILayout.Label("Force Zero Time requires SECOND_SPAWN_ENABLE_DEBUG_BODYTIME=true in Nakama.", _labelStyle);
            GUILayout.EndArea();
        }

        private void StartBodyTimeEvent(string kind, string source, long seconds, string note)
        {
            StartOperation(_memorySync.ApplyBodyTimeEvent(new BodyTimeEventRequestDto
            {
                id = CharacterMemorySync.BuildClientEventId($"bodytime-{kind}"),
                kind = kind,
                source = source,
                amount_seconds = seconds,
                note = note
            }), $"BodyTime {kind}");
        }

        private string SafeRewardObjectiveId()
        {
            return string.IsNullOrWhiteSpace(_rewardObjectiveId) ? "prototype-training-drone" : _rewardObjectiveId.Trim();
        }

        private void StartOperation(IEnumerator operation, string label)
        {
            if (_busy || operation == null)
            {
                return;
            }

            StartCoroutine(RunOperation(operation, label));
        }

        private IEnumerator RunOperation(IEnumerator operation, string label)
        {
            _busy = true;
            _status = $"{label} running...";
            yield return operation;
            _status = $"{label} complete";
            _busy = false;
        }

        private static string BuildContextLine(AgentContextDto context)
        {
            if (context == null)
            {
                return "No profile loaded yet.";
            }

            var body = context.body;
            var player = context.player;
            var time = body?.time;
            return $"{body?.lifecycle ?? "unknown"} | BodyTime {FormatSeconds(time?.remaining_seconds ?? 0)} | SECOND {FormatSeconds(player?.second_balance_seconds ?? 0)} | R{player?.reincarnation_count ?? 0}";
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

        private static int ToIntSeconds(long seconds)
        {
            if (seconds <= 0)
            {
                return 0;
            }

            return seconds >= int.MaxValue ? int.MaxValue : (int)seconds;
        }
    }

    internal static class PrototypeDebugInput
    {
        public static bool WasPressedThisFrame(Key serializedKey, Key fallback)
        {
            var keyboard = Keyboard.current;
            if (keyboard == null)
            {
                return false;
            }

            if (!TryNormalizeKey(serializedKey, fallback, keyboard, out var key))
            {
                return false;
            }

            return keyboard[key].wasPressedThisFrame;
        }

        private static bool TryNormalizeKey(Key serializedKey, Key fallback, Keyboard keyboard, out Key key)
        {
            var raw = (int)serializedKey;
            switch (raw)
            {
                case 283:
                    key = Key.F2;
                    return true;
                case 284:
                    key = Key.F3;
                    return true;
                case 285:
                    key = Key.F4;
                    return true;
                case 286:
                    key = Key.F5;
                    return true;
                case 287:
                    key = Key.F6;
                    return true;
            }

            if (IsKeyboardKey(serializedKey, keyboard))
            {
                key = serializedKey;
                return true;
            }

            if (IsKeyboardKey(fallback, keyboard))
            {
                key = fallback;
                return true;
            }

            key = Key.None;
            return false;
        }

        private static bool IsKeyboardKey(Key key, Keyboard keyboard)
        {
            var index = (int)key - 1;
            return index >= 0 && index < keyboard.allKeys.Count;
        }
    }
}
