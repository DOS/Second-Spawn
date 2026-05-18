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
    /// The brain sends bounded context to Nakama, receives a server-validated
    /// model or fallback decision, then applies only narrow visual intents.
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
        [SerializeField, Min(1f)] private float _modelFailureCooldownSeconds = 120f;
        [SerializeField, Min(0f)] private float _decisionCooldownJitterSeconds = 45f;
        [SerializeField] private float _initialDecisionDelaySeconds;
        [SerializeField] private float _moveSpeed = 2.4f;
        [SerializeField] private float _patrolRadius = 5f;
        [SerializeField] private float _socialSenseRadius = 8f;
        [SerializeField] private int _maxNearbySocialActors = 3;
        [SerializeField] private float _talkIntervalSeconds = 7.5f;
        [SerializeField] private bool _seedSoulOnStart = true;
        [SerializeField] private bool _alignFeetToGround = true;
        [SerializeField] private bool _logPhaseTransitions = true;

        [SerializeField] private int _decisionFailureErrorThreshold = 3;
        [SerializeField, Tooltip("Backoff after a model-selected NPC say intent fails Nakama persistence validation.")]
        private float _intentPersistenceFailureBackoffSeconds = 45f;

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
        private float _baseMoveSpeed;
        private bool _hasMoveTarget;
        private float _nextTalkAt;
        private int _pendingFootAlignFrames;
        private int _loopSequence;
        private int _consecutiveDecisionFailures;
        private float _stableDecisionJitterSeconds;
        private float _intentPersistenceBackoffUntil;
        private ActorProfileDto _configuredActorProfile;
        private readonly List<string> _phaseTrace = new List<string>();
        private bool _hasDecisionSlot;
        private static int _activeDecisionRequests;
        private const int MaxConcurrentDecisionRequests = 1;

        public string BrainStatusLabel { get; private set; } = "AI booting";
        public Color BrainStatusColor { get; private set; } = new Color(0.82f, 0.86f, 0.9f);
        public string BrainStatusReason { get; private set; } = "";

        private void Awake()
        {
            _homePosition = transform.position;
            _baseMoveSpeed = _moveSpeed;
            _speechBubble = GetOrAdd<PrototypeSpeechBubble>();
            _voiceCue = GetOrAdd<PrototypeVoiceCue>();
            _gateway = FindAnyObjectByType<SecondSpawnGatewayClient>();
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

            EnsureVisual();
            if (_gateway == null)
            {
                Debug.LogWarning("[PrototypeAgentBrain] No SecondSpawnGatewayClient found in scene.");
                LogPhase(BrainPhase.Idle, "missing Nakama client");
                return;
            }

            _stableDecisionJitterSeconds = StableRange01(_agentId) * _decisionCooldownJitterSeconds;
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

            ReleaseDecisionSlot();
            _hasMoveTarget = false;
            ApplyLocomotion(0f);
            LogPhase(BrainPhase.Idle, "brain stopped");
        }

        private void OnDisable()
        {
            StopBrain();
        }

        public void SetPhaseLogging(bool enabled)
        {
            _logPhaseTransitions = enabled;
        }

        public void ConfigureActorProfile(
            ActorProfileDto profile,
            string zoneId,
            float patrolRadius,
            float decisionIntervalSeconds,
            float initialDecisionDelaySeconds)
        {
            if (profile == null)
            {
                return;
            }

            _configuredActorProfile = profile;
            _agentId = string.IsNullOrWhiteSpace(profile.actor_id) ? _agentId : profile.actor_id.Trim();
            _displayName = string.IsNullOrWhiteSpace(profile.display_name) ? _displayName : profile.display_name.Trim();
            _stableDecisionJitterSeconds = StableRange01(_agentId) * _decisionCooldownJitterSeconds;
            _zoneId = string.IsNullOrWhiteSpace(zoneId) ? _zoneId : zoneId.Trim();
            _patrolRadius = Mathf.Max(0.5f, patrolRadius);
            _decisionIntervalSeconds = Mathf.Max(0.25f, decisionIntervalSeconds);
            _initialDecisionDelaySeconds = Mathf.Max(0f, initialDecisionDelaySeconds);
            _seedSoulOnStart = false;
            _context = BuildContextFromActorProfile(profile);

            var previousVariant = VisualPrefabCatalog.NormalizeVariant(_visualVariant);
            var resolvedVariant = ResolveActorVisualVariant(profile);
            var nextVariant = VisualPrefabCatalog.NormalizeVariant(resolvedVariant);
            _visualVariant = resolvedVariant;
            if (_visualRoot != null && nextVariant != previousVariant)
            {
                ReloadVisual();
            }
            else
            {
                EnsureVisual();
            }

            ApplyContextToPrototypeBody();
        }

        private IEnumerator BrainLoop()
        {
            yield return BootstrapContext();
            if (_initialDecisionDelaySeconds > 0f)
            {
                LogPhase(BrainPhase.Cooldown, $"initial stagger {_initialDecisionDelaySeconds:0.00}s");
                yield return new WaitForSeconds(_initialDecisionDelaySeconds);
            }

            _nextTalkAt = Time.time + 1.5f;

            while (enabled)
            {
                _loopSequence++;
                LogPhase(BrainPhase.Sense, BuildSenseLogDetail());
                SetBrainStatus("AI thinking", new Color(0.72f, 0.82f, 0.95f));
                var request = BuildDecisionRequest();
                LogPhase(BrainPhase.Decide, BuildDecisionRequestLogDetail(request));

                AgentDecisionDto decision = null;
                string decisionError = null;

                if (_activeDecisionRequests >= MaxConcurrentDecisionRequests)
                {
                    SetBrainStatus("AI queued", new Color(0.72f, 0.82f, 0.95f));
                }

                yield return WaitForDecisionSlot();
                _activeDecisionRequests++;
                _hasDecisionSlot = true;
                SetBrainStatus("AI DOS.AI request", new Color(0.72f, 0.82f, 0.95f));
                try
                {
                    yield return _gateway.Decide(request, value => decision = value, error => decisionError = error);
                }
                finally
                {
                    ReleaseDecisionSlot();
                }

                TrackDecisionResult(decisionError);
                UpdateBrainStatus(decision, decisionError);

                LogPhase(BrainPhase.Validate, BuildDecisionLogDetail(decision, decisionError));

                if (decision != null)
                {
                    LogPhase(BrainPhase.Act, BuildDecisionLogDetail(decision, null));
                    yield return ApplyDecision(decision, request);
                }

                LogPhase(BrainPhase.Reflect, "no reflection write in local prototype loop");
                var cooldownSeconds = DecisionCooldownSeconds(decision, decisionError);
                LogPhase(BrainPhase.Cooldown, $"waiting {cooldownSeconds:0.00}s");
                yield return new WaitForSeconds(cooldownSeconds);
            }
        }

        private IEnumerator BootstrapContext()
        {
            if (_configuredActorProfile != null)
            {
                _context = BuildContextFromActorProfile(_configuredActorProfile);
                LogPhase(BrainPhase.Bootstrap, $"actor profile loaded for {_agentId}");
                ApplyContextToPrototypeBody();
                SetBrainStatus("AI ready", new Color(0.82f, 0.86f, 0.9f));
                yield break;
            }

            if (_context == null)
            {
                var seed = BuildSoulSeed();
                if (!_seedSoulOnStart)
                {
                    seed.soul.player_notes = "Local prototype context without durable Nakama actor persistence.";
                }
                _context = BuildLocalPrototypeContext(_agentId, _displayName, _visualVariant, seed);
            }

            _context.body.memory = new[]
            {
                new MemoryRecordDto
                {
                    kind = "system",
                    summary = "Prototype NPC brain patrols the hub, talks through bounded intent, and never mutates game state directly.",
                    importance = 7
                }
            };

            LogPhase(BrainPhase.Bootstrap, _context == null
                ? "context unavailable after bootstrap"
                : "context loaded");
            ApplyContextToPrototypeBody();
            SetBrainStatus(_context == null ? "AI no context" : "AI ready",
                _context == null ? new Color(1f, 0.24f, 0.22f) : new Color(0.82f, 0.86f, 0.9f));
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

        private static AgentContextDto BuildLocalPrototypeContext(
            string agentId,
            string displayName,
            int visualVariant,
            UpdateSoulRequestDto seed)
        {
            var normalizedAgentId = string.IsNullOrWhiteSpace(agentId) ? "prototype-npc-guide" : agentId.Trim();
            var normalizedDisplayName = string.IsNullOrWhiteSpace(displayName) ? normalizedAgentId : displayName.Trim();
            const string role = "Prototype NPC guide";

            return new AgentContextDto
            {
                player = new PlayerProfileDto
                {
                    player_id = normalizedAgentId,
                    display_name = normalizedDisplayName,
                    second_balance_seconds = 0,
                    reincarnation_count = 0
                },
                body = new BodyProfileDto
                {
                    body_id = "body-" + normalizedAgentId,
                    archetype_id = "prototype-npc",
                    visual_prefab_key = $"generated_visual_{VisualPrefabCatalog.NormalizeVariant(visualVariant):00}",
                    visual_variant = VisualPrefabCatalog.NormalizeVariant(visualVariant),
                    stats = new CharacterStatsDto(),
                    characteristics = seed.characteristics,
                    time = new BodyTimeDto { remaining_seconds = 86400, max_seconds = 86400, danger_drain_rate = 1 },
                    lifecycle = "alive",
                    identity = new FrameIdentityDto
                    {
                        public_name = normalizedDisplayName,
                        callsign = normalizedAgentId,
                        public_role = role,
                        profession = role,
                        reputation_summary = "Local prototype context. Durable profile state belongs in Nakama."
                    },
                    story = new BodyStoryDto
                    {
                        origin = "A local prototype context used only when no Nakama actor profile is configured.",
                        role = role,
                        conflict = "It has no durable game-backend state.",
                        rumor = "The real NPC profile should be loaded from Nakama."
                    },
                    animation_capabilities = new AnimationCapabilitiesDto
                    {
                        supports_jump = true,
                        supports_roll = true,
                        supports_melee = true,
                        supports_ranged = false,
                        weapon_stance = "one_hand_melee"
                    },
                    agent_policy = new AgentPolicyDto
                    {
                        enabled = true,
                        mode = "observe_and_keep_safe",
                        preferred_activities = new[] { "talk", "patrol" },
                        forbidden_activities = new[] { "grant_items", "mutate_state", "unsafe_combat" },
                        stop_when_body_time_below = 900
                    },
                    soul = seed.soul
                }
            };
        }

        private AgentDecisionRequestDto BuildDecisionRequest()
        {
            var position = transform.position;
            var shouldTalk = Time.time >= _nextTalkAt;

            var nearbyObjects = BuildNearbyObjects(position);

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
                    nearby_actors = ExtractNearbyActors(nearbyObjects),
                    nearby_objects = nearbyObjects
                },
                allowed = shouldTalk
                    ? new[] { "say", "stop" }
                    : new[] { "move", "stop" }
            };
        }

        private WorldObjectDto[] BuildNearbyObjects(Vector3 position)
        {
            var objects = new List<WorldObjectDto>
            {
                new WorldObjectDto
                {
                    id = "hub-origin",
                    kind = "safe_landmark",
                    display_name = "Hub Origin",
                    role = "safe landmark",
                    distance = Vector3.Distance(position, _homePosition)
                }
            };

            var socialRadius = Mathf.Max(0.5f, _socialSenseRadius);
            var maxActors = Mathf.Max(0, _maxNearbySocialActors);
            if (maxActors <= 0)
            {
                return objects.ToArray();
            }

            var brains = FindObjectsByType<PrototypeAgentBrain>(FindObjectsInactive.Exclude);
            var nearbyActors = new List<WorldObjectDto>();
            foreach (var brain in brains)
            {
                if (brain == null || brain == this || !brain.isActiveAndEnabled)
                {
                    continue;
                }

                var distance = Vector3.Distance(position, brain.transform.position);
                if (distance > socialRadius)
                {
                    continue;
                }

                nearbyActors.Add(new WorldObjectDto
                {
                    id = string.IsNullOrWhiteSpace(brain._agentId) ? brain.name : brain._agentId,
                    kind = "nearby_actor",
                    display_name = string.IsNullOrWhiteSpace(brain._displayName) ? brain.name : brain._displayName,
                    role = string.IsNullOrWhiteSpace(brain._context?.body?.identity?.public_role)
                        ? "nearby Frame actor"
                        : brain._context.body.identity.public_role,
                    affinity = 0,
                    hostility = 0,
                    distance = distance
                });
            }

            nearbyActors.Sort((left, right) => left.distance.CompareTo(right.distance));
            for (var index = 0; index < nearbyActors.Count && index < maxActors; index++)
            {
                objects.Add(nearbyActors[index]);
            }

            return objects.ToArray();
        }

        private static WorldObjectDto[] ExtractNearbyActors(WorldObjectDto[] nearbyObjects)
        {
            if (nearbyObjects == null || nearbyObjects.Length == 0)
            {
                return System.Array.Empty<WorldObjectDto>();
            }

            var actors = new List<WorldObjectDto>();
            foreach (var nearbyObject in nearbyObjects)
            {
                if (nearbyObject != null &&
                    string.Equals(nearbyObject.kind, "nearby_actor", System.StringComparison.OrdinalIgnoreCase))
                {
                    actors.Add(nearbyObject);
                }
            }

            return actors.ToArray();
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

        private static string BuildDecisionLogDetail(AgentDecisionDto decision, string decisionError)
        {
            if (decision == null)
            {
                return string.IsNullOrWhiteSpace(decisionError) ? "decision=none" : $"decision=none, error={decisionError}";
            }

            var degradedSuffix = string.Equals(decision.source, "fallback", System.StringComparison.OrdinalIgnoreCase)
                ? ", degraded=fallback"
                : "";
            if (decision.action == "move" && decision.move != null)
            {
                return $"action=move, target=({decision.move.x:0.00},{decision.move.z:0.00}), confidence={decision.confidence:0.00}{BuildDecisionSourceLogDetail(decision)}{degradedSuffix}";
            }

            if (decision.action == "say")
            {
                var textLength = string.IsNullOrWhiteSpace(decision.say) ? 0 : decision.say.Length;
                return $"action=say, text_length={textLength}, confidence={decision.confidence:0.00}{BuildDecisionSourceLogDetail(decision)}{degradedSuffix}";
            }

            return $"action={decision.action}, confidence={decision.confidence:0.00}{BuildDecisionSourceLogDetail(decision)}{degradedSuffix}";
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

        private float DecisionCooldownSeconds(AgentDecisionDto decision, string decisionError)
        {
            var baseCooldown = Mathf.Max(0.25f, _decisionIntervalSeconds);
            if (!string.IsNullOrWhiteSpace(decisionError))
            {
                return Mathf.Max(baseCooldown, 30f) + _stableDecisionJitterSeconds;
            }

            if (decision != null &&
                string.Equals(decision.source, "fallback", System.StringComparison.OrdinalIgnoreCase) &&
                IsModelBackoffReason(decision.source_reason))
            {
                return Mathf.Max(baseCooldown, _modelFailureCooldownSeconds) + _stableDecisionJitterSeconds;
            }

            return baseCooldown + _stableDecisionJitterSeconds;
        }

        private static IEnumerator WaitForDecisionSlot()
        {
            while (_activeDecisionRequests >= MaxConcurrentDecisionRequests)
            {
                yield return null;
            }
        }

        private void ReleaseDecisionSlot()
        {
            if (!_hasDecisionSlot)
            {
                return;
            }

            _hasDecisionSlot = false;
            _activeDecisionRequests = Mathf.Max(0, _activeDecisionRequests - 1);
        }

        private static bool IsModelBackoffReason(string reason)
        {
            if (string.IsNullOrWhiteSpace(reason))
            {
                return false;
            }

            return reason == "dos_ai_exception" ||
                reason == "dos_ai_timeout" ||
                reason == "dos_ai_circuit_open" ||
                reason == "dos_ai_http_429" ||
                reason == "dos_ai_http_500" ||
                reason == "dos_ai_http_502" ||
                reason == "dos_ai_http_503" ||
                reason == "dos_ai_http_504";
        }

        private void TrackDecisionResult(string decisionError)
        {
            if (string.IsNullOrWhiteSpace(decisionError))
            {
                _consecutiveDecisionFailures = 0;
                return;
            }

            _consecutiveDecisionFailures++;
            Debug.LogWarning($"[PrototypeAgentBrain] Nakama agent decision failed for agent={_agentId}, consecutive_failures={_consecutiveDecisionFailures}: {decisionError}");
            if (_consecutiveDecisionFailures >= Mathf.Max(1, _decisionFailureErrorThreshold))
            {
                Debug.LogError($"[PrototypeAgentBrain] Nakama agent decision failure threshold reached for agent={_agentId}, threshold={_decisionFailureErrorThreshold}: {decisionError}");
            }
        }

        private void UpdateBrainStatus(AgentDecisionDto decision, string decisionError)
        {
            if (decision == null)
            {
                var hasError = !string.IsNullOrWhiteSpace(decisionError);
                SetBrainStatus(
                    hasError ? "AI ERROR" : "AI idle",
                    hasError ? new Color(1f, 0.24f, 0.22f) : new Color(0.82f, 0.86f, 0.9f),
                    ExtractDecisionReason(decisionError));
                return;
            }

            if (string.Equals(decision.source, "model", System.StringComparison.OrdinalIgnoreCase))
            {
                SetBrainStatus("AI DOS.AI", new Color(0.62f, 1f, 0.72f), decision.source_reason);
                return;
            }

            if (string.Equals(decision.source, "fallback", System.StringComparison.OrdinalIgnoreCase) ||
                !string.IsNullOrWhiteSpace(decisionError))
            {
                if (IsModelBackoffReason(decision.source_reason))
                {
                    SetBrainStatus(
                        "AI ERROR",
                        new Color(1f, 0.24f, 0.22f),
                        FirstNonEmpty(decision.source_reason, ExtractDecisionReason(decisionError)));
                    return;
                }

                SetBrainStatus(
                    "AI FALLBACK",
                    new Color(1f, 0.62f, 0.16f),
                    FirstNonEmpty(decision.source_reason, ExtractDecisionReason(decisionError)));
                return;
            }

            SetBrainStatus("AI unknown", new Color(1f, 0.86f, 0.28f), decision.source_reason);
        }

        private void SetBrainStatus(string label, Color color, string reason = "")
        {
            BrainStatusLabel = string.IsNullOrWhiteSpace(label) ? "AI unknown" : label.Trim();
            BrainStatusColor = color;
            BrainStatusReason = FormatBrainStatusReason(reason);
        }

        private static string FormatBrainStatusReason(string reason)
        {
            if (string.IsNullOrWhiteSpace(reason))
            {
                return "";
            }

            var normalized = reason.Trim();
            if (normalized.StartsWith("dos_ai_http_", System.StringComparison.OrdinalIgnoreCase))
            {
                return "DOS.AI HTTP " + normalized["dos_ai_http_".Length..];
            }

            return normalized switch
            {
                "dos_ai_exception" => "DOS.AI timeout",
                "dos_ai_timeout" => "DOS.AI timeout",
                "dos_ai_circuit_open" => "DOS.AI backoff",
                "dos_ai_empty_content" => "DOS.AI empty",
                "dos_ai_validate_error" => "intent rejected",
                "dos_ai_unconfigured" => "DOS.AI off",
                "nakama_body_time_policy" => "BodyTime policy",
                "nakama_prototype_patrol" => "safe patrol",
                "nakama_interact_fallback" => "safe interact",
                "nakama_social_fallback" => "safe social",
                "nakama_no_allowed_action" => "no safe action",
                _ => normalized
            };
        }

        private static string FirstNonEmpty(params string[] values)
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

        private static string ExtractDecisionReason(string error)
        {
            if (string.IsNullOrWhiteSpace(error))
            {
                return "";
            }

            if (error.Contains("daily token budget", System.StringComparison.OrdinalIgnoreCase))
            {
                return "token_budget";
            }

            if (error.Contains("429", System.StringComparison.OrdinalIgnoreCase) ||
                error.Contains("rate", System.StringComparison.OrdinalIgnoreCase))
            {
                return "rate_limited";
            }

            if (error.Contains("502", System.StringComparison.OrdinalIgnoreCase) ||
                error.Contains("bad gateway", System.StringComparison.OrdinalIgnoreCase) ||
                error.Contains("origin_bad_gateway", System.StringComparison.OrdinalIgnoreCase))
            {
                return "provider_502";
            }

            if (error.Contains("timeout", System.StringComparison.OrdinalIgnoreCase) ||
                error.Contains("timed out", System.StringComparison.OrdinalIgnoreCase))
            {
                return "timeout";
            }

            return "decision_error";
        }

        private IEnumerator ApplyDecision(AgentDecisionDto decision, AgentDecisionRequestDto request)
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
                yield return RecordModelNpcIntent(decision, request, text);
                yield break;
            }

            if (decision.action == "move" && decision.move != null)
            {
                var requested = new Vector3(decision.move.x, transform.position.y, decision.move.z);
                _moveTarget = ClampToPatrol(requested);
                _hasMoveTarget = true;
                yield break;
            }

            _hasMoveTarget = false;
            ApplyLocomotion(0f);
            yield break;
        }

        private IEnumerator RecordModelNpcIntent(AgentDecisionDto decision, AgentDecisionRequestDto request, string text)
        {
            if (_gateway == null || decision == null || request == null)
            {
                yield break;
            }

            if (!string.Equals(decision.source, "model", System.StringComparison.OrdinalIgnoreCase))
            {
                yield break;
            }

            if (Time.realtimeSinceStartup < _intentPersistenceBackoffUntil)
            {
                yield break;
            }

            var targetId = string.IsNullOrWhiteSpace(decision.target_id) ? null : decision.target_id.Trim();
            var distanceMeters = ResolveNearbyObjectDistance(request.world_snapshot?.nearby_objects, targetId);
            NpcIntentSubmitResponseDto response = null;
            string error = null;
            yield return _gateway.SubmitPermanentNpcIntent(new NpcIntentSubmitRequestDto
            {
                id = CharacterMemorySync.BuildClientEventId("npc-intent"),
                actor_id = _agentId,
                target_actor_id = targetId,
                intent = "say",
                source = "dos_ai_model",
                text = text,
                reason = decision.reason,
                distance_meters = distanceMeters
            }, value => response = value, value => error = value);

            if (response == null)
            {
                _intentPersistenceBackoffUntil = Time.realtimeSinceStartup + Mathf.Max(_decisionIntervalSeconds, _intentPersistenceFailureBackoffSeconds);
                Debug.LogWarning($"[PrototypeAgentBrain] NPC intent persistence paused for agent={_agentId}, retry_in={Mathf.Max(0f, _intentPersistenceBackoffUntil - Time.realtimeSinceStartup):0}s: {ShortenForLog(error, 220)}");
            }
            else
            {
                _intentPersistenceBackoffUntil = 0f;
            }
        }

        private static float ResolveNearbyObjectDistance(WorldObjectDto[] objects, string targetId)
        {
            if (string.IsNullOrWhiteSpace(targetId) || objects == null)
            {
                return 0f;
            }

            foreach (var nearbyObject in objects)
            {
                if (nearbyObject != null && nearbyObject.id == targetId)
                {
                    return nearbyObject.distance;
                }
            }

            return 0f;
        }

        private static string ShortenForLog(string value, int maxLength)
        {
            if (string.IsNullOrWhiteSpace(value))
            {
                return "unknown";
            }

            var normalized = value.Trim().Replace("\r", " ").Replace("\n", " ");
            return normalized.Length <= maxLength ? normalized : normalized.Substring(0, Mathf.Max(0, maxLength - 3)) + "...";
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
                _moveSpeed = Mathf.Max(0.1f, _baseMoveSpeed * CalculateAgilitySpeedMultiplier(stats.agility));
            }
        }

        private static float CalculateAgilitySpeedMultiplier(int agility)
        {
            return Mathf.Clamp(agility / 8f, 0.75f, 1.4f);
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
            if (prefab == null)
            {
                var sourcePath = VisualPrefabCatalog.GetSourceAssetPath(_visualVariant);
                prefab = AssetDatabase.LoadAssetAtPath<GameObject>(sourcePath);
                if (prefab != null)
                {
                    Debug.LogWarning($"[PrototypeAgentBrain] Generated visual missing for variant {_visualVariant}. Falling back to source asset '{sourcePath}'.");
                }
            }

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
            EquipmentVisualCatalog.ApplyEquipmentVisual(visualRoot, ResolveEquipmentVisualId());
            SanitizeVariantVisuals(visualRoot);
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

        private int ResolveEquipmentVisualId()
        {
            var equipmentId = _context?.body?.equipment?.equipment_visual_id ?? EquipmentVisualCatalog.None;
            return equipmentId != EquipmentVisualCatalog.None
                ? equipmentId
                : EquipmentVisualCatalog.GetDefaultForVisualVariant(_visualVariant);
        }

        private void SanitizeVariantVisuals(GameObject visualRoot)
        {
            if (visualRoot == null)
            {
                return;
            }

            if (VisualPrefabCatalog.NormalizeVariant(_visualVariant) == 14)
            {
                KeepSingleFemaleHairMesh(visualRoot);
            }
        }

        private static void KeepSingleFemaleHairMesh(GameObject visualRoot)
        {
            Renderer selectedHair = null;
            var selectedPriority = -1;
            var hairRenderers = new List<Renderer>();
            foreach (var renderer in visualRoot.GetComponentsInChildren<Renderer>(includeInactive: true))
            {
                if (renderer == null)
                {
                    continue;
                }

                var objectName = renderer.gameObject.name;
                if (string.IsNullOrWhiteSpace(objectName) ||
                    objectName.IndexOf("hair", System.StringComparison.OrdinalIgnoreCase) < 0)
                {
                    continue;
                }

                hairRenderers.Add(renderer);
                var priority = FemaleHairPriority(objectName);
                if (selectedHair == null || priority > selectedPriority)
                {
                    selectedHair = renderer;
                    selectedPriority = priority;
                }
            }

            if (selectedHair == null || hairRenderers.Count <= 1)
            {
                return;
            }

            foreach (var hairRenderer in hairRenderers)
            {
                hairRenderer.enabled = hairRenderer == selectedHair;
            }
        }

        private static int FemaleHairPriority(string objectName)
        {
            if (objectName.Equals("Female-Hair", System.StringComparison.OrdinalIgnoreCase))
            {
                return 3;
            }

            if (objectName.Equals("Mesh-Hair", System.StringComparison.OrdinalIgnoreCase))
            {
                return 2;
            }

            return 1;
        }

        private void ReloadVisual()
        {
            if (_visualRoot != null)
            {
                _visualRoot.name = "PrototypeAgentVisual_Old";
                Destroy(_visualRoot);
                _visualRoot = null;
            }

            _animator = null;
            _intentDriver = null;
            EnsureVisual();
        }

        private static AgentContextDto BuildContextFromActorProfile(ActorProfileDto profile)
        {
            return new AgentContextDto
            {
                player = new PlayerProfileDto
                {
                    player_id = profile.actor_id,
                    display_name = profile.display_name,
                    second_balance_seconds = 0,
                    reincarnation_count = 0
                },
                body = profile.body
            };
        }

        private static int ResolveActorVisualVariant(ActorProfileDto profile)
        {
            if (profile?.body != null && profile.body.visual_variant >= 0)
            {
                return VisualPrefabCatalog.NormalizeVariant(profile.body.visual_variant);
            }

            return StableVisualVariant(profile?.actor_id);
        }

        private static int StableVisualVariant(string seed)
        {
            if (VisualPrefabCatalog.Count <= 0)
            {
                return 0;
            }

            unchecked
            {
                var hash = 2166136261u;
                var value = string.IsNullOrWhiteSpace(seed) ? "prototype-agent" : seed.Trim();
                for (var index = 0; index < value.Length; index++)
                {
                    hash ^= value[index];
                    hash *= 16777619u;
                }

                return (int)(hash % VisualPrefabCatalog.Count);
            }
        }

        private static float StableRange01(string seed)
        {
            unchecked
            {
                var hash = 2166136261u;
                var value = string.IsNullOrWhiteSpace(seed) ? "prototype-agent" : seed.Trim();
                for (var index = 0; index < value.Length; index++)
                {
                    hash ^= value[index];
                    hash *= 16777619u;
                }

                return (hash % 1000u) / 999f;
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
            EnsureSharedController(animator);
            animator.Rebind();
            animator.Update(0f);

            if (animator.GetComponent<AnimationEventReceiver>() == null)
            {
                animator.gameObject.AddComponent<AnimationEventReceiver>();
            }
        }

        private static void EnsureSharedController(Animator animator)
        {
#if UNITY_EDITOR
            if (animator.runtimeAnimatorController != null && ExposesLocomotionContract(animator))
            {
                return;
            }

            var sharedController = AssetDatabase.LoadAssetAtPath<RuntimeAnimatorController>(SharedAnimatorControllerPath);
            if (sharedController != null)
            {
                animator.runtimeAnimatorController = sharedController;
            }
            else
            {
                Debug.LogWarning($"[PrototypeAgentBrain] Shared animator controller not found at '{SharedAnimatorControllerPath}'.");
            }
#endif
        }

        private static bool ExposesLocomotionContract(Animator animator)
        {
            foreach (var parameter in animator.parameters)
            {
                if (parameter.name is "Moving" or "Velocity" or "Velocity X" or "Velocity Z")
                {
                    return true;
                }
            }

            return false;
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
