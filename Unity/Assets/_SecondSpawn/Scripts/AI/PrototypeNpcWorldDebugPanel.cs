using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;

namespace SecondSpawn.AI
{
    /// <summary>
    /// Prototype-only IMGUI panel for exercising persistent Nakama-owned NPCs.
    /// </summary>
    [DisallowMultipleComponent]
    [RequireComponent(typeof(SecondSpawnGatewayClient))]
    public sealed class PrototypeNpcWorldDebugPanel : MonoBehaviour
    {
        [SerializeField] private bool _showPanel = true;
        [SerializeField] private Key _toggleKey = Key.F5;
        [SerializeField] private Vector2 _panelPosition = new Vector2(552f, 280f);
        [SerializeField] private Vector2 _panelSize = new Vector2(480f, 420f);
        [SerializeField] private string _actorAId = "npc-synthetic-sentinel-0101";
        [SerializeField] private string _actorBId = "npc-wasteland-courier-0244";
        [SerializeField] private string _topic = "patrol";
        [SerializeField] private string _intentText = "Route check complete. Keep the eastern gate quiet.";
        [SerializeField] private float _distanceMeters = 2f;

        private SecondSpawnGatewayClient _gateway;
        private GUIStyle _labelStyle;
        private GUIStyle _mutedStyle;
        private bool _busy;
        private string _status = "Ready";
        private ActorProfileDto[] _npcs;
        private Vector2 _npcScroll;
        private int _selectedNpcIndex;
        private NpcContextResponseDto _context;
        private NpcIntentSubmitResponseDto _lastIntent;
        private NpcInteractionEventDto _lastInteraction;

        private void Awake()
        {
            _gateway = GetComponent<SecondSpawnGatewayClient>();
        }

        private void Update()
        {
            if (PrototypeDebugInput.WasPressedThisFrame(_toggleKey, Key.F5))
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

            EnsureStyles();
            var rect = new Rect(_panelPosition.x, _panelPosition.y, _panelSize.x, _panelSize.y);
            GUI.Box(rect, "Persistent NPC Debug");
            GUILayout.BeginArea(new Rect(rect.x + 12f, rect.y + 24f, rect.width - 24f, rect.height - 32f));
            GUILayout.Label(_status, _labelStyle);

            GUI.enabled = !_busy;
            GUILayout.BeginHorizontal();
            if (GUILayout.Button("Seed"))
            {
                StartOperation(SeedNpcs(), "Seed");
            }

            if (GUILayout.Button("List"))
            {
                StartOperation(ListNpcs(), "List");
            }
            GUILayout.EndHorizontal();

            _actorAId = LabeledTextField("A", _actorAId);
            _actorBId = LabeledTextField("B", _actorBId);
            _topic = LabeledTextField("Topic", _topic);
            _intentText = LabeledTextField("Say", _intentText);
            _distanceMeters = Mathf.Max(0f, LabeledFloatField("Meters", _distanceMeters));

            GUILayout.BeginHorizontal();
            if (GUILayout.Button("Get Context"))
            {
                StartOperation(GetContext(), "Context");
            }

            if (GUILayout.Button("Submit Say Intent"))
            {
                StartOperation(SubmitIntent(), "Intent");
            }
            GUILayout.EndHorizontal();

            if (GUILayout.Button("Fallback Smoke Tick"))
            {
                StartOperation(Interact(), "Interact");
            }
            GUI.enabled = true;

            DrawLastIntent();
            DrawLastInteraction();
            DrawSelectedNpc();
            DrawNpcSummary();
            GUILayout.EndArea();
        }

        private IEnumerator SeedNpcs()
        {
            NpcWorldListResponseDto response = null;
            string error = null;
            yield return _gateway.SeedPermanentNpcs(value => response = value, value => error = value);
            ApplyListResponse(response, error, "Seeded");
        }

        private IEnumerator ListNpcs()
        {
            NpcWorldListResponseDto response = null;
            string error = null;
            yield return _gateway.ListPermanentNpcs(value => response = value, value => error = value);
            ApplyListResponse(response, error, "Loaded");
        }

        private IEnumerator Interact()
        {
            NpcInteractionResponseDto response = null;
            string error = null;
            yield return _gateway.InteractPermanentNpcs(new NpcInteractionRequestDto
            {
                id = CharacterMemorySync.BuildClientEventId("npc-talk"),
                actor_a_id = SafeValue(_actorAId, "npc-synthetic-sentinel-0101"),
                actor_b_id = SafeValue(_actorBId, "npc-wasteland-courier-0244"),
                topic = SafeValue(_topic, "patrol")
            }, value => response = value, value => error = value);

            if (response == null)
            {
                _status = $"Interact failed: {error}";
                yield break;
            }

            _lastInteraction = response.interaction;
            _status = "Interaction recorded";
        }

        private IEnumerator GetContext()
        {
            NpcContextResponseDto response = null;
            string error = null;
            yield return _gateway.GetPermanentNpcContext(new NpcContextRequestDto
            {
                actor_id = SafeValue(_actorAId, "npc-synthetic-sentinel-0101"),
                nearby_actor_ids = new[] { SafeValue(_actorBId, "npc-wasteland-courier-0244") }
            }, value => response = value, value => error = value);

            if (response == null)
            {
                _status = $"Context failed: {error}";
                yield break;
            }

            _context = response;
            _status = $"Context loaded for {response.actor?.display_name ?? "NPC"}";
        }

        private IEnumerator SubmitIntent()
        {
            NpcIntentSubmitResponseDto response = null;
            string error = null;
            yield return _gateway.SubmitPermanentNpcIntent(new NpcIntentSubmitRequestDto
            {
                id = CharacterMemorySync.BuildClientEventId("npc-intent"),
                actor_id = SafeValue(_actorAId, "npc-synthetic-sentinel-0101"),
                target_actor_id = SafeValue(_actorBId, "npc-wasteland-courier-0244"),
                intent = "say",
                source = "debug",
                text = SafeValue(_intentText, "Route check complete."),
                reason = "Unity debug panel simulates an LLM-selected NPC intent.",
                distance_meters = _distanceMeters
            }, value => response = value, value => error = value);

            if (response == null)
            {
                _status = $"Intent failed: {error}";
                yield break;
            }

            _lastIntent = response;
            _status = "Say intent recorded";
        }

        private void ApplyListResponse(NpcWorldListResponseDto response, string error, string label)
        {
            if (response == null)
            {
                _status = $"{label} failed: {error}";
                return;
            }

            _npcs = response.npcs;
            if (_npcs != null && _npcs.Length > 0)
            {
                _selectedNpcIndex = Mathf.Clamp(_selectedNpcIndex, 0, _npcs.Length - 1);
            }

            _status = $"{label} {response.count} NPCs";
        }

        private void DrawLastIntent()
        {
            if (_context != null && !string.IsNullOrWhiteSpace(_context.intent_boundary))
            {
                GUILayout.Label(_context.intent_boundary, _mutedStyle);
                if (_context.interaction_rules != null)
                {
                    GUILayout.Label($"Hard limit: {_context.interaction_rules.max_distance_meters:0.#}m", _mutedStyle);
                }
            }

            if (_lastIntent?.intent?.payload?.text == null)
            {
                return;
            }

            var targetName = _lastIntent.target_actor != null ? _lastIntent.target_actor.display_name : "hub";
            GUILayout.Label($"{_lastIntent.actor?.display_name ?? "NPC"} -> {targetName}: {_lastIntent.intent.payload.text}", _labelStyle);
        }

        private void DrawLastInteraction()
        {
            if (_lastInteraction == null)
            {
                GUILayout.Label("Fallback smoke tick has not run.", _mutedStyle);
                return;
            }

            GUILayout.Label("Fallback smoke tick:", _mutedStyle);
            GUILayout.Label($"{_lastInteraction.actor_a_name}: {_lastInteraction.actor_a_line}", _labelStyle);
            GUILayout.Label($"{_lastInteraction.actor_b_name}: {_lastInteraction.actor_b_line}", _labelStyle);
        }

        private void DrawNpcSummary()
        {
            if (_npcs == null || _npcs.Length == 0)
            {
                return;
            }

            GUILayout.Label("Permanent NPCs", _mutedStyle);
            _npcScroll = GUILayout.BeginScrollView(_npcScroll, GUILayout.Height(96f));
            for (var i = 0; i < _npcs.Length; i++)
            {
                var npc = _npcs[i];
                var label = $"{i + 1:00} {SafeText(npc.display_name, "NPC")} | {NpcProfession(npc)} | Lv {NpcLevel(npc)} | V{NpcVisualVariant(npc)}";
                if (GUILayout.Button(label))
                {
                    _selectedNpcIndex = i;
                    _actorAId = SafeText(npc.actor_id, _actorAId);
                    _status = $"Selected {SafeText(npc.display_name, "NPC")}";
                }
            }

            GUILayout.EndScrollView();
        }

        private void DrawSelectedNpc()
        {
            var npc = SelectedNpc();
            if (npc == null)
            {
                return;
            }

            GUILayout.Label("Selected NPC", _mutedStyle);
            GUILayout.Label($"{SafeText(npc.display_name, "NPC")} | {SafeText(npc.body?.identity?.callsign, npc.actor_id)}", _labelStyle);
            GUILayout.Label($"{NpcProfession(npc)} | {NpcAge(npc)} | {SafeText(npc.body?.identity?.home_base, "unknown base")}", _labelStyle);
            GUILayout.Label($"Lv {NpcLevel(npc)} | HP {NpcStats(npc)?.max_health ?? 0} | ATK {NpcStats(npc)?.attack_power ?? 0} | DEF {NpcStats(npc)?.defense_power ?? 0} | V{NpcVisualVariant(npc)}", _labelStyle);
            GUILayout.Label($"Soul: {SafeText(npc.body?.soul?.name, "unknown")}", _labelStyle);
            GUILayout.Label(Shorten(SafeText(npc.memory != null && npc.memory.Length > 0 ? npc.memory[0].summary : "", "No seed memory."), 120), _mutedStyle);
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
            _busy = false;
        }

        private void EnsureStyles()
        {
            _labelStyle ??= new GUIStyle(GUI.skin.label)
            {
                fontSize = 13,
                normal = { textColor = Color.white },
                wordWrap = true
            };
            _mutedStyle ??= new GUIStyle(_labelStyle)
            {
                normal = { textColor = new Color(0.75f, 0.78f, 0.82f) }
            };
        }

        private static string LabeledTextField(string label, string value)
        {
            GUILayout.BeginHorizontal();
            GUILayout.Label(label, GUILayout.Width(44f));
            var next = GUILayout.TextField(value);
            GUILayout.EndHorizontal();
            return next;
        }

        private static float LabeledFloatField(string label, float value)
        {
            GUILayout.BeginHorizontal();
            GUILayout.Label(label, GUILayout.Width(44f));
            var text = GUILayout.TextField(value.ToString("0.##"));
            GUILayout.EndHorizontal();
            return float.TryParse(text, out var parsed) ? parsed : value;
        }

        private static string SafeValue(string value, string fallback)
        {
            return string.IsNullOrWhiteSpace(value) ? fallback : value.Trim();
        }

        private ActorProfileDto SelectedNpc()
        {
            if (_npcs == null || _npcs.Length == 0)
            {
                return null;
            }

            _selectedNpcIndex = Mathf.Clamp(_selectedNpcIndex, 0, _npcs.Length - 1);
            return _npcs[_selectedNpcIndex];
        }

        private static CharacterStatsDto NpcStats(ActorProfileDto npc)
        {
            return npc?.body?.stats;
        }

        private static int NpcLevel(ActorProfileDto npc)
        {
            return Mathf.Max(1, NpcStats(npc)?.level ?? 1);
        }

        private static int NpcVisualVariant(ActorProfileDto npc)
        {
            return npc?.body?.visual_variant ?? -1;
        }

        private static string NpcProfession(ActorProfileDto npc)
        {
            return SafeText(npc?.body?.identity?.profession, npc?.body?.story?.role, "unknown role");
        }

        private static string NpcAge(ActorProfileDto npc)
        {
            var identity = npc?.body?.identity;
            if (identity == null || identity.age_years <= 0)
            {
                return SafeText(identity?.age_band, "age unknown");
            }

            var band = SafeText(identity.age_band, "age");
            return $"{identity.age_years}y {band}";
        }

        private static string SafeText(params string[] values)
        {
            for (var i = 0; i < values.Length; i++)
            {
                if (!string.IsNullOrWhiteSpace(values[i]))
                {
                    return values[i].Trim();
                }
            }

            return "";
        }

        private static string Shorten(string value, int maxLength)
        {
            if (string.IsNullOrWhiteSpace(value))
            {
                return "";
            }

            var normalized = value.Trim();
            if (normalized.Length <= maxLength)
            {
                return normalized;
            }

            return maxLength <= 3 ? normalized.Substring(0, maxLength) : normalized.Substring(0, maxLength - 3) + "...";
        }
    }
}
