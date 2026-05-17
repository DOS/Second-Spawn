using System.Collections;
using UnityEngine;

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
        [SerializeField] private Vector2 _panelPosition = new Vector2(552f, 280f);
        [SerializeField] private Vector2 _panelSize = new Vector2(420f, 292f);
        [SerializeField] private string _actorAId = "npc-synthetic-sentinel-0101";
        [SerializeField] private string _actorBId = "npc-wasteland-courier-0244";
        [SerializeField] private string _topic = "patrol";

        private SecondSpawnGatewayClient _gateway;
        private GUIStyle _labelStyle;
        private GUIStyle _mutedStyle;
        private bool _busy;
        private string _status = "Ready";
        private ActorProfileDto[] _npcs;
        private NpcInteractionEventDto _lastInteraction;

        private void Awake()
        {
            _gateway = GetComponent<SecondSpawnGatewayClient>();
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

            if (GUILayout.Button("Interact"))
            {
                StartOperation(Interact(), "Interact");
            }
            GUI.enabled = true;

            DrawLastInteraction();
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

        private void ApplyListResponse(NpcWorldListResponseDto response, string error, string label)
        {
            if (response == null)
            {
                _status = $"{label} failed: {error}";
                return;
            }

            _npcs = response.npcs;
            _status = $"{label} {response.count} NPCs";
        }

        private void DrawLastInteraction()
        {
            if (_lastInteraction == null)
            {
                GUILayout.Label("No interaction recorded.", _mutedStyle);
                return;
            }

            GUILayout.Label($"{_lastInteraction.actor_a_name}: {_lastInteraction.actor_a_line}", _labelStyle);
            GUILayout.Label($"{_lastInteraction.actor_b_name}: {_lastInteraction.actor_b_line}", _labelStyle);
        }

        private void DrawNpcSummary()
        {
            if (_npcs == null || _npcs.Length == 0)
            {
                return;
            }

            var count = Mathf.Min(4, _npcs.Length);
            for (var i = 0; i < count; i++)
            {
                var npc = _npcs[i];
                GUILayout.Label($"{npc.actor_id} | {npc.display_name}", _mutedStyle);
            }
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

        private static string SafeValue(string value, string fallback)
        {
            return string.IsNullOrWhiteSpace(value) ? fallback : value.Trim();
        }
    }
}
