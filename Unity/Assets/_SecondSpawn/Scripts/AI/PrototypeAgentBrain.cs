using System.Collections;
using System.Collections.Generic;
using SecondSpawn.Networking;
using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif

namespace SecondSpawn.AI
{
    /// <summary>
    /// Prototype LLM-style brain for a local NPC actor.
    /// The brain reads bounded character context from the gateway, asks for a
    /// structured decision, then applies only narrow visual intents.
    /// </summary>
    [DisallowMultipleComponent]
    public sealed class PrototypeAgentBrain : MonoBehaviour
    {
        private enum BrainPhase
        {
            Idle,
            Bootstrap,
            Sense,
            Decide,
            Validate,
            Act,
            Reflect,
            Cooldown
        }

#if UNITY_EDITOR
        private const string SharedAnimatorControllerPath =
            "Assets/ExplosiveLLC/RPG Character Mecanim Animation Pack/Animation Controller/RPG-Character-Animation-Controller.controller";
#endif

        [SerializeField] private bool _startOnPlay = true;
        [SerializeField] private string _agentId = "prototype-npc-guide";
        [SerializeField] private string _displayName = "Prototype Guide";
        [SerializeField] private string _zoneId = "prototype-hub";
        [SerializeField] private int _visualVariant = 10;
        [SerializeField] private float _decisionIntervalSeconds = 1.6f;
        [SerializeField] private float _moveSpeed = 2.4f;
        [SerializeField] private float _patrolRadius = 5f;
        [SerializeField] private float _talkIntervalSeconds = 7.5f;
        [SerializeField] private bool _seedSoulOnStart = true;
        [SerializeField] private bool _alignFeetToGround = true;
        [SerializeField] private bool _logPhaseTransitions = true;
        [SerializeField] private int _gatewayFailureErrorThreshold = 3;

        private SecondSpawnGatewayClient _gateway;
        private AgentContextDto _context;
        private PrototypeSpeechBubble _speechBubble;
        private PrototypeVoiceCue _voiceCue;
        private VisualAnimationIntentDriver _intentDriver;
        private Animator _animator;
        private GameObject _visualRoot;
        private Coroutine _brainLoop;
        private Vector3 _homePosition;
        private Vector3 _moveTarget;
        private bool _hasMoveTarget;
        private float _nextTalkAt;
        private int _pendingFootAlignFrames;
        private int _loopSequence;
        private int _consecutiveGatewayFailures;
        private readonly List<string> _phaseTrace = new List<string>();

        private void Awake()
        {
            _homePosition = transform.position;
            _speechBubble = GetOrAdd<PrototypeSpeechBubble>();
            _voiceCue = GetOrAdd<PrototypeVoiceCue>();
            _gateway = FindAnyObjectByType<SecondSpawnGatewayClient>();
            EnsureVisual();
        }

        private void Start()
        {
            if (_startOnPlay)
            {
                StartBrain();
            }
        }

        private void Update()
        {
            TickMovement();
        }

        private void LateUpdate()
        {
            if (_alignFeetToGround && _visualRoot != null && _pendingFootAlignFrames > 0)
            {
                AlignVisualFeetToGround(_visualRoot, transform.position.y);
                _pendingFootAlignFrames--;
            }
        }

        public void StartBrain()
        {
            if (_brainLoop != null)
            {
                return;
            }

            if (_gateway == null)
            {
                _gateway = FindAnyObjectByType<SecondSpawnGatewayClient>();
            }

            if (_gateway == null)
            {
                Debug.LogWarning("[PrototypeAgentBrain] No SecondSpawnGatewayClient found in scene.");
                LogPhase(BrainPhase.Idle, "missing gateway");
                return;
            }

            LogPhase(BrainPhase.Bootstrap, "starting brain loop");
            _brainLoop = StartCoroutine(BrainLoop());
        }

        public void StopBrain()
        {
            if (_brainLoop != null)
            {
                StopCoroutine(_brainLoop);
                _brainLoop = null;
            }

            _hasMoveTarget = false;
            ApplyLocomotion(0f);
            LogPhase(BrainPhase.Idle, "brain stopped");
        }

        private IEnumerator BrainLoop()
        {
            yield return BootstrapContext();
            _nextTalkAt = Time.time + 1.5f;

            while (enabled)
            {
                _loopSequence++;
                LogPhase(BrainPhase.Sense, BuildSenseLogDetail());
                var request = BuildDecisionRequest();
                LogPhase(BrainPhase.Decide, BuildDecisionRequestLogDetail(request));

                AgentDecisionDto decision = null;
                string gatewayError = null;
                yield return _gateway.Decide(request, value => decision = value, error => gatewayError = error);
                TrackGatewayResult(gatewayError);

                LogPhase(BrainPhase.Validate, BuildDecisionLogDetail(decision, gatewayError));

                if (decision != null)
                {
                    LogPhase(BrainPhase.Act, BuildDecisionLogDetail(decision, null));
                    ApplyDecision(decision);
                }

                LogPhase(BrainPhase.Reflect, "no reflection write in local prototype loop");
                var cooldownSeconds = Mathf.Max(0.25f, _decisionIntervalSeconds);
                LogPhase(BrainPhase.Cooldown, $"waiting {cooldownSeconds:0.00}s");
                yield return new WaitForSeconds(cooldownSeconds);
            }
        }

        private IEnumerator BootstrapContext()
        {
            if (_seedSoulOnStart)
            {
                yield return _gateway.UpdateSoulForPlayer(_agentId, BuildSoulSeed(), ctx => _context = ctx, Debug.LogWarning);
            }

            if (_context == null)
            {
                yield return _gateway.GetContextForPlayer(_agentId, ctx => _context = ctx, Debug.LogWarning);
            }

            yield return _gateway.AddMemoryForPlayer(_agentId, new MemoryRecordDto
            {
                kind = "system",
                summary = "Prototype NPC brain patrols the hub, talks through bounded intent, and never mutates game state directly.",
                importance = 7
            }, ctx => _context = ctx, Debug.LogWarning);

            LogPhase(BrainPhase.Bootstrap, _context == null
                ? "context unavailable after bootstrap"
                : "context loaded");
            ApplyContextToPrototypeBody();
        }

        private UpdateSoulRequestDto BuildSoulSeed()
        {
            return new UpdateSoulRequestDto
            {
                soul = new SoulProfileDto
                {
                    name = _displayName,
                    core_drive = "guide the player, preserve safety, and observe the first Second Spawn prototype",
                    temperament = "calm, curious, and careful",
                    combat_style = "avoid combat unless the server explicitly allows it",
                    social_style = "short, practical, and a little uncanny",
                    moral_boundaries = new[] { "do not grant items", "do not spend BodyTime", "do not claim authority over game state" },
                    long_term_goals = new[] { "learn the hub", "help the player understand agent life" },
                    player_notes = "Prototype local NPC brain for testing autonomous character behavior.",
                    reincarnation_lore = "A synthetic guide imprint used to test agent cognition before real NPC lore is authored."
                },
                characteristics = new CharacterTraitsDto
                {
                    curiosity = 8,
                    courage = 4,
                    empathy = 7,
                    discipline = 8,
                    aggression = 1,
                    sociability = 8
                },
                agent_policy = new AgentPolicyDto
                {
                    enabled = true,
                    mode = "prototype_npc_patrol",
                    max_session_seconds = 1800,
                    allow_body_time_spend = false,
                    allow_risky_combat = false,
                    preferred_activities = new[] { "patrol", "talk", "observe" },
                    forbidden_activities = new[] { "grant_items", "spend_body_time", "start_combat" },
                    stop_when_body_time_below = 900
                }
            };
        }

        private AgentDecisionRequestDto BuildDecisionRequest()
        {
            var position = transform.position;
            var shouldTalk = Time.time >= _nextTalkAt;

            return new AgentDecisionRequestDto
            {
                context = _context,
                world_snapshot = new WorldSnapshotDto
                {
                    zone_id = _zoneId,
                    position = new Vector2Dto { x = position.x, z = position.z },
                    safe_radius = _patrolRadius,
                    danger_level = 0,
                    body_time_seconds = _context?.body?.time?.remaining_seconds ?? 3600,
                    nearby_objects = new[]
                    {
                        new WorldObjectDto
                        {
                            id = "hub-origin",
                            kind = "safe_landmark",
                            distance = Vector3.Distance(position, _homePosition)
                        }
                    }
                },
                allowed = shouldTalk
                    ? new[] { "say", "stop" }
                    : new[] { "move", "stop" }
            };
        }

        private string BuildSenseLogDetail()
        {
            var position = transform.position;
            var bodyTime = _context?.body?.time?.remaining_seconds ?? 3600;
            return $"position=({position.x:0.00},{position.z:0.00}), body_time={bodyTime}";
        }

        private static string BuildDecisionRequestLogDetail(AgentDecisionRequestDto request)
        {
            var allowed = request.allowed == null ? "none" : string.Join(",", request.allowed);
            var snapshot = request.world_snapshot;
            if (snapshot == null)
            {
                return $"allowed={allowed}, snapshot=none";
            }

            return $"allowed={allowed}, zone={snapshot.zone_id}, safe_radius={snapshot.safe_radius:0.00}";
        }

        private static string BuildDecisionLogDetail(AgentDecisionDto decision, string gatewayError)
        {
            if (decision == null)
            {
                return string.IsNullOrWhiteSpace(gatewayError)
                    ? "decision=none"
                    : $"decision=none, error={gatewayError}";
            }

            if (decision.action == "move" && decision.move != null)
            {
                return $"action=move, target=({decision.move.x:0.00},{decision.move.z:0.00}), confidence={decision.confidence:0.00}{BuildDecisionSourceLogDetail(decision)}";
            }

            if (decision.action == "say")
            {
                var textLength = string.IsNullOrWhiteSpace(decision.say) ? 0 : decision.say.Length;
                return $"action=say, text_length={textLength}, confidence={decision.confidence:0.00}{BuildDecisionSourceLogDetail(decision)}";
            }

            return $"action={decision.action}, confidence={decision.confidence:0.00}{BuildDecisionSourceLogDetail(decision)}";
        }

        private static string BuildDecisionSourceLogDetail(AgentDecisionDto decision)
        {
            if (decision == null || string.IsNullOrWhiteSpace(decision.source))
            {
                return "";
            }

            return string.IsNullOrWhiteSpace(decision.source_reason)
                ? $", source={decision.source}"
                : $", source={decision.source}, source_reason={decision.source_reason}";
        }

        private void TrackGatewayResult(string gatewayError)
        {
            if (string.IsNullOrWhiteSpace(gatewayError))
            {
                _consecutiveGatewayFailures = 0;
                return;
            }

            _consecutiveGatewayFailures++;
            Debug.LogWarning($"[PrototypeAgentBrain] Gateway decision failed for agent={_agentId}, consecutive_failures={_consecutiveGatewayFailures}: {gatewayError}");
            if (_consecutiveGatewayFailures >= Mathf.Max(1, _gatewayFailureErrorThreshold))
            {
                Debug.LogError($"[PrototypeAgentBrain] Gateway decision failure threshold reached for agent={_agentId}, threshold={_gatewayFailureErrorThreshold}: {gatewayError}");
            }
        }

        private void ApplyDecision(AgentDecisionDto decision)
        {
            if (decision.action == "say")
            {
                var text = string.IsNullOrWhiteSpace(decision.say)
                    ? $"{_displayName} is watching the hub."
                    : decision.say;
                _speechBubble.Show(text);
                _voiceCue.PlayCue(text);
                _intentDriver?.TryPlay(VisualAnimationIntent.Talk);
                _nextTalkAt = Time.time + Mathf.Max(2f, _talkIntervalSeconds);
                return;
            }

            if (decision.action == "move" && decision.move != null)
            {
                var requested = new Vector3(decision.move.x, transform.position.y, decision.move.z);
                _moveTarget = ClampToPatrol(requested);
                _hasMoveTarget = true;
                return;
            }

            _hasMoveTarget = false;
            ApplyLocomotion(0f);
        }

        private void ApplyContextToPrototypeBody()
        {
            var body = _context?.body;
            if (body == null)
            {
                return;
            }

            if (!string.IsNullOrWhiteSpace(body.soul?.name))
            {
                _displayName = body.soul.name.Trim();
            }

            var stats = body.stats;
            if (stats != null)
            {
                _moveSpeed = Mathf.Clamp(_moveSpeed * Mathf.Clamp(stats.agility / 8f, 0.75f, 1.4f), 0.5f, 6f);
            }
        }

        private void LogPhase(BrainPhase phase, string detail)
        {
            if (!_logPhaseTransitions)
            {
                return;
            }

            if (phase == BrainPhase.Sense)
            {
                _phaseTrace.Clear();
            }

            var entry = string.IsNullOrWhiteSpace(detail)
                ? phase.ToString()
                : $"{phase}({detail})";
            _phaseTrace.Add(entry);

            if (phase != BrainPhase.Bootstrap && phase != BrainPhase.Idle && phase != BrainPhase.Cooldown)
            {
                return;
            }

            var trace = string.Join(" -> ", _phaseTrace);
            Debug.Log($"[PrototypeAgentBrain] agent={_agentId}, seq={_loopSequence}, trace={trace}");
        }

        private void TickMovement()
        {
            if (!_hasMoveTarget)
            {
                ApplyLocomotion(0f);
                return;
            }

            var current = transform.position;
            var delta = _moveTarget - current;
            delta.y = 0f;
            if (delta.sqrMagnitude <= 0.04f)
            {
                _hasMoveTarget = false;
                ApplyLocomotion(0f);
                return;
            }

            var direction = delta.normalized;
            transform.position = Vector3.MoveTowards(current, _moveTarget, _moveSpeed * Time.deltaTime);
            transform.rotation = Quaternion.Slerp(transform.rotation, Quaternion.LookRotation(direction), 12f * Time.deltaTime);
            ApplyLocomotion(1f);
        }

        private Vector3 ClampToPatrol(Vector3 target)
        {
            var offset = target - _homePosition;
            offset.y = 0f;
            if (offset.magnitude > _patrolRadius)
            {
                offset = offset.normalized * _patrolRadius;
            }

            return _homePosition + offset;
        }

        private void EnsureVisual()
        {
            if (_animator != null || transform.Find("PrototypeAgentVisual") != null)
            {
                return;
            }

            GameObject visualRoot = null;
#if UNITY_EDITOR
            var cleanPath = VisualPrefabCatalog.GetCleanAssetPath(_visualVariant);
            var prefab = AssetDatabase.LoadAssetAtPath<GameObject>(cleanPath);
            if (prefab != null)
            {
                visualRoot = Instantiate(prefab, transform);
            }
#endif

            if (visualRoot == null)
            {
                visualRoot = GameObject.CreatePrimitive(PrimitiveType.Capsule);
                visualRoot.transform.SetParent(transform, false);
            }

            visualRoot.name = "PrototypeAgentVisual";
            visualRoot.transform.SetLocalPositionAndRotation(Vector3.zero, Quaternion.identity);
            visualRoot.transform.localScale = Vector3.one;
            DisablePhysics(visualRoot);
            if (_alignFeetToGround)
            {
                AlignVisualFeetToGround(visualRoot, transform.position.y);
                _pendingFootAlignFrames = 3;
            }

            _visualRoot = visualRoot;
            _animator = visualRoot.GetComponentInChildren<Animator>(includeInactive: true);
            ConfigureAnimator(_animator);

            _intentDriver = visualRoot.GetComponentInChildren<VisualAnimationIntentDriver>(includeInactive: true);
            if (_intentDriver == null && _animator != null)
            {
                _intentDriver = _animator.gameObject.AddComponent<VisualAnimationIntentDriver>();
            }
        }

        private void ConfigureAnimator(Animator animator)
        {
            if (animator == null)
            {
                return;
            }

            animator.applyRootMotion = false;
            animator.cullingMode = AnimatorCullingMode.AlwaysAnimate;
            animator.updateMode = AnimatorUpdateMode.Normal;
            animator.speed = 1f;
#if UNITY_EDITOR
            if (animator.runtimeAnimatorController == null)
            {
                var sharedController = AssetDatabase.LoadAssetAtPath<RuntimeAnimatorController>(SharedAnimatorControllerPath);
                if (sharedController != null)
                {
                    animator.runtimeAnimatorController = sharedController;
                }
            }
#endif
            animator.Rebind();
            animator.Update(0f);

            if (animator.GetComponent<AnimationEventReceiver>() == null)
            {
                animator.gameObject.AddComponent<AnimationEventReceiver>();
            }
        }

        private void ApplyLocomotion(float speed)
        {
            if (_animator == null)
            {
                return;
            }

            SetBool("Moving", speed > 0.02f);
            SetFloat("Velocity", speed);
            SetFloat("Velocity X", 0f);
            SetFloat("Velocity Z", speed);
            SetFloat("AnimationSpeed", 1f);
            SetFloat("Animation Speed", 1f);
            SetInt("Weapon", -1);
        }

        private void SetBool(string parameterName, bool value)
        {
            foreach (var parameter in _animator.parameters)
            {
                if (parameter.name == parameterName && parameter.type == AnimatorControllerParameterType.Bool)
                {
                    _animator.SetBool(parameterName, value);
                    return;
                }
            }
        }

        private void SetFloat(string parameterName, float value)
        {
            foreach (var parameter in _animator.parameters)
            {
                if (parameter.name == parameterName && parameter.type == AnimatorControllerParameterType.Float)
                {
                    _animator.SetFloat(parameterName, value);
                    return;
                }
            }
        }

        private void SetInt(string parameterName, int value)
        {
            foreach (var parameter in _animator.parameters)
            {
                if (parameter.name == parameterName && parameter.type == AnimatorControllerParameterType.Int)
                {
                    _animator.SetInteger(parameterName, value);
                    return;
                }
            }
        }

        private static void DisablePhysics(GameObject root)
        {
            foreach (var collider in root.GetComponentsInChildren<Collider>(includeInactive: true))
            {
                collider.enabled = false;
            }

            foreach (var rigidbody in root.GetComponentsInChildren<Rigidbody>(includeInactive: true))
            {
                rigidbody.isKinematic = true;
                rigidbody.useGravity = false;
            }
        }

        private static void AlignVisualFeetToGround(GameObject root, float targetWorldY)
        {
            var renderers = root.GetComponentsInChildren<Renderer>(includeInactive: true);
            if (renderers.Length == 0)
            {
                return;
            }

            var minY = float.PositiveInfinity;
            foreach (var renderer in renderers)
            {
                minY = Mathf.Min(minY, renderer.bounds.min.y);
            }

            if (!float.IsFinite(minY))
            {
                return;
            }

            var position = root.transform.position;
            position.y += targetWorldY - minY;
            root.transform.position = position;
        }

        private T GetOrAdd<T>() where T : Component
        {
            var component = GetComponent<T>();
            return component != null ? component : gameObject.AddComponent<T>();
        }
    }
}
