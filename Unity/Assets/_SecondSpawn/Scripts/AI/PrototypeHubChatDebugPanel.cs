using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;

namespace SecondSpawn.AI
{
    /// <summary>
    /// Prototype-only IMGUI panel for exercising the Nakama-backed hub chat log.
    /// This is a bounded RPC storage channel, not the final realtime chat socket.
    /// </summary>
    [DisallowMultipleComponent]
    [RequireComponent(typeof(SecondSpawnGatewayClient))]
    public sealed class PrototypeHubChatDebugPanel : MonoBehaviour
    {
        [SerializeField] private bool _showPanel = true;
        [SerializeField] private Key _toggleKey = Key.F3;
        [SerializeField] private Vector2 _panelPosition = new Vector2(552f, 16f);
        [SerializeField] private Vector2 _panelSize = new Vector2(420f, 248f);
        [SerializeField] private string _channelId = "prototype-hub";
        [SerializeField] private string _displayName = "JOY";
        [SerializeField] private string _draftMessage = "Hub chat is online.";
        [SerializeField] private int _messageLimit = 8;

        private SecondSpawnGatewayClient _gateway;
        private GUIStyle _labelStyle;
        private GUIStyle _mutedStyle;
        private bool _busy;
        private string _status = "Ready";
        private ChatMessageDto[] _messages;

        private void Awake()
        {
            _gateway = GetComponent<SecondSpawnGatewayClient>();
        }

        private void Update()
        {
            if (PrototypeDebugInput.WasPressedThisFrame(_toggleKey, Key.F3))
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
            GUI.Box(rect, "Hub Chat Debug");
            GUILayout.BeginArea(new Rect(rect.x + 12f, rect.y + 24f, rect.width - 24f, rect.height - 32f));
            GUILayout.Label($"Channel {SafeChannelId()} | {_status}", _labelStyle);

            GUI.enabled = !_busy;
            GUILayout.BeginHorizontal();
            _displayName = GUILayout.TextField(_displayName, GUILayout.Width(96f));
            _draftMessage = GUILayout.TextField(_draftMessage);
            GUILayout.EndHorizontal();

            GUILayout.BeginHorizontal();
            if (GUILayout.Button("Send"))
            {
                StartOperation(SendMessage(), "Send");
            }

            if (GUILayout.Button("Refresh"))
            {
                StartOperation(RefreshMessages(), "Refresh");
            }
            GUILayout.EndHorizontal();
            GUI.enabled = true;

            DrawMessages();
            GUILayout.EndArea();
        }

        private IEnumerator SendMessage()
        {
            ChatSendResponseDto response = null;
            string error = null;
            yield return _gateway.SendHubChatMessage(new ChatSendRequestDto
            {
                channel_id = SafeChannelId(),
                sender_display_name = SafeDisplayName(),
                message = SafeDraftMessage(),
                source = "player"
            }, value => response = value, value => error = value);

            if (response == null)
            {
                _status = $"Send failed: {error}";
                yield break;
            }

            _messages = response.messages;
            _status = "Message sent";
        }

        private IEnumerator RefreshMessages()
        {
            ChatListResponseDto response = null;
            string error = null;
            yield return _gateway.ListHubChatMessages(new ChatListRequestDto
            {
                channel_id = SafeChannelId(),
                limit = Mathf.Max(1, _messageLimit)
            }, value => response = value, value => error = value);

            if (response == null)
            {
                _status = $"Refresh failed: {error}";
                yield break;
            }

            _messages = response.messages;
            _status = "Messages loaded";
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

        private void DrawMessages()
        {
            if (_messages == null || _messages.Length == 0)
            {
                GUILayout.Label("No messages loaded.", _mutedStyle);
                return;
            }

            var start = Mathf.Max(0, _messages.Length - Mathf.Max(1, _messageLimit));
            for (var i = start; i < _messages.Length; i++)
            {
                var message = _messages[i];
                if (message == null)
                {
                    continue;
                }

                GUILayout.Label($"{FormatTimestamp(message.sent_at)} {Fallback(message.sender_display_name, message.sender_player_id, "unknown")}: {Fallback(message.text, "")}", _labelStyle);
            }
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

        private string SafeChannelId()
        {
            return string.IsNullOrWhiteSpace(_channelId) ? "prototype-hub" : _channelId.Trim();
        }

        private string SafeDisplayName()
        {
            return string.IsNullOrWhiteSpace(_displayName) ? _gateway.PlayerId : _displayName.Trim();
        }

        private string SafeDraftMessage()
        {
            return string.IsNullOrWhiteSpace(_draftMessage) ? "Hub chat is online." : _draftMessage.Trim();
        }

        private static string FormatTimestamp(string timestamp)
        {
            if (string.IsNullOrWhiteSpace(timestamp))
            {
                return "--:--";
            }

            var trimmed = timestamp.Trim();
            return trimmed.Length > 16 ? trimmed.Substring(11, 5) : trimmed;
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
