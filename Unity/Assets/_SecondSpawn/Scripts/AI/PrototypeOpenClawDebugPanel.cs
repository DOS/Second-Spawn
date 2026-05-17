using System;
using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;

namespace SecondSpawn.AI
{
    /// <summary>
    /// Prototype-only IMGUI controls for exercising the OpenClaw Frame bridge
    /// against Nakama during Play Mode.
    /// </summary>
    [DisallowMultipleComponent]
    [RequireComponent(typeof(SecondSpawnGatewayClient))]
    public sealed class PrototypeOpenClawDebugPanel : MonoBehaviour
    {
        [SerializeField] private bool _showPanel = true;
        [SerializeField] private Key _toggleKey = Key.F4;
        [SerializeField] private Vector2 _panelPosition = new Vector2(16f, 424f);
        [SerializeField] private Vector2 _panelSize = new Vector2(360f, 248f);
        [SerializeField] private string _frameActorId = "npc-openclaw-guide";
        [SerializeField] private string _connectedAgentId = "oc-local-debug-agent";
        [SerializeField] private string _displayName = "OpenClaw Debug Guide";
        [SerializeField] private string _sayText = "OpenClaw debug intent reached Nakama.";

        private SecondSpawnGatewayClient _gateway;
        private GUIStyle _labelStyle;
        private GUIStyle _mutedStyle;
        private bool _busy;
        private string _status = "Ready";
        private OpenClawBindingDto _binding;
        private OpenClawContextResponseDto _context;
        private OpenClawIntentSubmitResponseDto _lastIntent;

        private void Awake()
        {
            _gateway = GetComponent<SecondSpawnGatewayClient>();
        }

        private void Update()
        {
            var keyboard = Keyboard.current;
            if (keyboard != null && keyboard[_toggleKey].wasPressedThisFrame)
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
                normal = { textColor = Color.white },
                wordWrap = true
            };
            _mutedStyle ??= new GUIStyle(_labelStyle)
            {
                normal = { textColor = new Color(0.75f, 0.78f, 0.82f) }
            };

            var rect = new Rect(_panelPosition.x, _panelPosition.y, _panelSize.x, _panelSize.y);
            GUI.Box(rect, "OpenClaw Bridge Debug");
            GUILayout.BeginArea(new Rect(rect.x + 12f, rect.y + 24f, rect.width - 24f, rect.height - 32f));
            GUILayout.Label(BuildBindingLine(), _labelStyle);
            GUILayout.Label(BuildContextLine(), _mutedStyle);
            GUILayout.Label(_status, _labelStyle);

            GUI.enabled = !_busy;
            GUILayout.BeginHorizontal();
            if (GUILayout.Button("Bind + Context"))
            {
                StartOperation(BindAndFetchContext(), "Bind + Context");
            }

            if (GUILayout.Button("Refresh"))
            {
                StartOperation(FetchContext(), "Refresh");
            }
            GUILayout.EndHorizontal();

            GUILayout.BeginHorizontal();
            if (GUILayout.Button("Say Intent"))
            {
                StartOperation(SubmitSayIntent(), "Say Intent");
            }

            if (GUILayout.Button("Heartbeat"))
            {
                StartOperation(SendHeartbeat("connected", "Unity debug OpenClaw heartbeat."), "Heartbeat");
            }

            if (GUILayout.Button("Degrade"))
            {
                StartOperation(SendHeartbeat("degraded", "Unity debug marked OpenClaw as degraded."), "Degrade");
            }
            GUILayout.EndHorizontal();
            GUI.enabled = true;

            GUILayout.Label("Intent submit only records pending validation. It does not mutate game state.", _mutedStyle);
            GUILayout.EndArea();
        }

        private IEnumerator BindAndFetchContext()
        {
            OpenClawBindingDto binding = null;
            string error = null;
            yield return _gateway.BindOpenClawAgent(BuildBindRequest(), value => binding = value, value => error = value);
            if (binding == null)
            {
                _status = $"Bind failed: {error}";
                yield break;
            }

            _binding = binding;
            yield return FetchContext();
        }

        private IEnumerator FetchContext()
        {
            OpenClawContextResponseDto context = null;
            string error = null;
            yield return _gateway.GetOpenClawContext(new OpenClawContextRequestDto
            {
                connected_agent_id = SafeAgentId()
            }, value => context = value, value => error = value);

            if (context == null)
            {
                _status = $"Context failed: {error}";
                yield break;
            }

            _context = context;
            _binding = context.binding;
            _status = "Context loaded";
        }

        private IEnumerator SubmitSayIntent()
        {
            OpenClawIntentSubmitResponseDto response = null;
            string error = null;
            yield return _gateway.SubmitOpenClawIntent(new OpenClawIntentSubmitRequestDto
            {
                connected_agent_id = SafeAgentId(),
                id = BuildClientEventId("openclaw-say"),
                intent = "say",
                reason = "Unity OpenClaw debug panel smoke.",
                payload = new OpenClawIntentPayloadDto { text = SafeSayText() }
            }, value => response = value, value => error = value);

            if (response == null)
            {
                _status = $"Intent failed: {error}";
                yield break;
            }

            _lastIntent = response;
            _binding = response.binding;
            _status = $"Intent {response.status}";
            yield return FetchContext();
        }

        private IEnumerator SendHeartbeat(string state, string summary)
        {
            OpenClawHeartbeatResponseDto response = null;
            string error = null;
            yield return _gateway.SendOpenClawHeartbeat(new OpenClawHeartbeatRequestDto
            {
                connected_agent_id = SafeAgentId(),
                connection_status = state,
                summary = summary
            }, value => response = value, value => error = value);

            if (response == null)
            {
                _status = $"Heartbeat failed: {error}";
                yield break;
            }

            _binding = response.binding;
            _status = $"Heartbeat {response.binding?.connection_status ?? "unknown"}";
            yield return FetchContext();
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

        private OpenClawBindRequestDto BuildBindRequest()
        {
            return new OpenClawBindRequestDto
            {
                frame_actor_id = SafeFrameActorId(),
                connected_agent_id = SafeAgentId(),
                display_name = string.IsNullOrWhiteSpace(_displayName) ? "OpenClaw Debug Guide" : _displayName.Trim(),
                agent_kind = "companion",
                connection_status = "connected",
                moderation_state = "active",
                consent_scope = new[] { "dialogue", "heartbeat", "intent:say" },
                rate_limit_profile = new OpenClawRateLimitProfileDto
                {
                    requests_per_minute = 20,
                    intents_per_minute = 10,
                    tokens_per_day = 50000
                }
            };
        }

        private string BuildBindingLine()
        {
            var binding = _binding;
            var frame = binding?.frame_actor_id ?? SafeFrameActorId();
            var agent = binding?.connected_agent_id ?? SafeAgentId();
            var state = binding?.connection_status ?? "unbound";
            var intent = _lastIntent != null ? $" | last {(_lastIntent.status ?? "unknown")}" : "";
            return $"Frame {frame} | Agent {agent} | {state}{intent}";
        }

        private string BuildContextLine()
        {
            var context = _context?.context;
            if (context == null)
            {
                return "No OpenClaw context loaded.";
            }

            var identity = context.identity;
            var body = context.body;
            var toolCount = context.tools?.Length ?? 0;
            var memoryCount = context.memory?.Length ?? 0;
            var heartbeat = context.heartbeat?.offline_session_state ?? "unknown";
            return $"{identity?.public_name ?? "Unnamed"} | {identity?.public_role ?? "no role"} | {body?.body_id ?? "no body"} | tools {toolCount} | memories {memoryCount} | {heartbeat}";
        }

        private string SafeFrameActorId()
        {
            return string.IsNullOrWhiteSpace(_frameActorId) ? "npc-openclaw-guide" : _frameActorId.Trim();
        }

        private string SafeAgentId()
        {
            return string.IsNullOrWhiteSpace(_connectedAgentId) ? "oc-local-debug-agent" : _connectedAgentId.Trim();
        }

        private string SafeSayText()
        {
            return string.IsNullOrWhiteSpace(_sayText) ? "OpenClaw debug intent reached Nakama." : _sayText.Trim();
        }

        private static string BuildClientEventId(string prefix)
        {
            return $"{prefix}-{Guid.NewGuid():N}";
        }
    }
}
