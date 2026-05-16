using System.Collections;
using SecondSpawn.Networking;
using UnityEngine;
using UnityEngine.InputSystem;

namespace SecondSpawn.AI
{
    [DisallowMultipleComponent]
    [RequireComponent(typeof(NetworkPlayer))]
    public sealed class PrototypeLLMAgentDriver : MonoBehaviour
    {
        [SerializeField] private bool _enableOnStart;
        [SerializeField] private float _decisionIntervalSeconds = 1.25f;
        [SerializeField] private float _moveHoldSeconds = 0.9f;
        [SerializeField] private string _zoneId = "prototype-hub";
        [SerializeField] private bool _allowPrototypeInteract;

        private SecondSpawnGatewayClient _gateway;
        private CharacterMemorySync _memorySync;
        private NetworkPlayer _networkPlayer;
        private PrototypeSpeechBubble _speechBubble;
        private PrototypeVoiceCue _voiceCue;
        private Coroutine _loop;

        private void Awake()
        {
            _networkPlayer = GetComponent<NetworkPlayer>();
            _speechBubble = GetComponent<PrototypeSpeechBubble>();
            if (_speechBubble == null)
            {
                _speechBubble = gameObject.AddComponent<PrototypeSpeechBubble>();
            }

            _voiceCue = GetComponent<PrototypeVoiceCue>();
            if (_voiceCue == null)
            {
                _voiceCue = gameObject.AddComponent<PrototypeVoiceCue>();
            }

            _gateway = FindAnyObjectByType<SecondSpawnGatewayClient>();
            _memorySync = _gateway != null ? _gateway.GetComponent<CharacterMemorySync>() : null;
        }

        private void Start()
        {
            if (_enableOnStart)
            {
                StartAgent();
            }
        }

        private void Update()
        {
            if (Keyboard.current != null && Keyboard.current.pKey.wasPressedThisFrame)
            {
                if (_loop == null)
                {
                    StartAgent();
                }
                else
                {
                    StopAgent();
                }
            }
        }

        public void StartAgent()
        {
            if (!CanDrivePrototypeAgent())
            {
                Debug.LogWarning("[PrototypeLLMAgentDriver] Ignored prototype agent start on a non-authoritative player. Offline agents must run on the server/state authority.");
                return;
            }

            if (_gateway == null)
            {
                Debug.LogWarning("[PrototypeLLMAgentDriver] No SecondSpawnGatewayClient found in scene.");
                return;
            }

            _loop ??= StartCoroutine(DecisionLoop());
        }

        public void StopAgent()
        {
            if (_loop != null)
            {
                StopCoroutine(_loop);
                _loop = null;
            }

            _networkPlayer.ClearPrototypeAgentInput();
        }

        private IEnumerator DecisionLoop()
        {
            while (enabled)
            {
                var request = BuildDecisionRequest();
                AgentDecisionDto decision = null;
                string gatewayError = null;
                yield return _gateway.Decide(request, value => decision = value, error => gatewayError = error);

                if (decision == null && _gateway.HasNakamaSession)
                {
                    yield return _gateway.DecideWithNakamaFallback(request, value => decision = value, Debug.LogWarning);
                }
                else if (decision == null && !string.IsNullOrWhiteSpace(gatewayError))
                {
                    Debug.LogWarning(gatewayError);
                }

                if (decision != null)
                {
                    ApplyDecision(decision);
                }

                yield return new WaitForSeconds(Mathf.Max(0.25f, _decisionIntervalSeconds));
            }
        }

        private bool CanDrivePrototypeAgent()
        {
            return _networkPlayer != null && _networkPlayer.HasStateAuthority;
        }

        private AgentDecisionRequestDto BuildDecisionRequest()
        {
            var position = transform.position;
            var context = _memorySync != null ? _memorySync.Context : null;
            var bodyTime = context?.body?.time?.remaining_seconds ?? 3600;

            return new AgentDecisionRequestDto
            {
                context = context,
                world_snapshot = new WorldSnapshotDto
                {
                    zone_id = _zoneId,
                    position = new Vector2Dto { x = position.x, z = position.z },
                    safe_radius = 8f,
                    body_time_seconds = bodyTime,
                    nearby_objects = System.Array.Empty<WorldObjectDto>()
                },
                allowed = _allowPrototypeInteract
                    ? new[] { "move", "interact", "say", "stop" }
                    : new[] { "move", "say", "stop" }
            };
        }

        private void ApplyDecision(AgentDecisionDto decision)
        {
            if (decision.action == "move" && decision.move != null)
            {
                var target = new Vector3(decision.move.x, transform.position.y, decision.move.z);
                var direction = target - transform.position;
                direction.y = 0f;
                direction = Vector3.ClampMagnitude(direction, 1f);

                _networkPlayer.SetPrototypeAgentInput(new NetworkInputData
                {
                    HorizontalAxis = direction.x,
                    VerticalAxis = direction.z,
                    Run = true
                });
                StartCoroutine(ClearMoveAfterDelay());
            }
            else if (decision.action == "say")
            {
                Debug.Log($"[PrototypeLLMAgentDriver] Agent says: {decision.say}");
                _speechBubble.Show(decision.say);
                _voiceCue.PlayCue(decision.say);
                PlayVisualIntent(VisualAnimationIntent.Talk);
            }
            else if (decision.action == "interact")
            {
                if (_allowPrototypeInteract)
                {
                    PlayVisualIntent(VisualAnimationIntent.Interact);
                }
                else
                {
                    _networkPlayer.ClearPrototypeAgentInput();
                    Debug.Log("[PrototypeLLMAgentDriver] Ignored prototype interact decision. Interact is disabled for patrol mode.");
                }
            }
            else
            {
                _networkPlayer.ClearPrototypeAgentInput();
            }
        }

        private IEnumerator ClearMoveAfterDelay()
        {
            yield return new WaitForSeconds(Mathf.Max(0.1f, _moveHoldSeconds));
            _networkPlayer.ClearPrototypeAgentInput();
        }

        private void PlayVisualIntent(VisualAnimationIntent intent)
        {
            var driver = GetComponentInChildren<VisualAnimationIntentDriver>();
            if (driver != null)
            {
                driver.TryPlay(intent);
            }
        }
    }
}
