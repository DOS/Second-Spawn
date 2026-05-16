using System;
using System.Collections;
using UnityEngine;

namespace SecondSpawn.AI
{
    /// <summary>
    /// Binds one scene NPC-like body to one Nakama actor profile.
    /// Each bound actor has its own body, stats, traits, soul, memory, policy,
    /// runtime counters, and activity log.
    /// </summary>
    [DisallowMultipleComponent]
    public sealed class ActorProfileBinder : MonoBehaviour
    {
        [SerializeField] private bool _loadOnStart = true;
        [SerializeField] private string _actorId = "npc-guide";
        [SerializeField] private string _actorType = "npc";
        [SerializeField] private string _displayName = "Guide";
        [SerializeField] private string _archetypeId = "prototype-npc";
        [SerializeField] private string _visualPrefabKey = "prototype-npc";
        [SerializeField] private bool _seedMemoryOnStart = true;
        [SerializeField] private string _seedMemoryKind = "system";
        [SerializeField, TextArea] private string _seedMemorySummary =
            "This NPC-like body has an independent actor profile and memory.";
        [SerializeField, Range(1, 10)] private int _seedMemoryImportance = 6;

        private SecondSpawnGatewayClient _gateway;
        private Coroutine _loadRoutine;

        public ActorProfileDto CurrentProfile { get; private set; }
        public bool IsLoading => _loadRoutine != null;
        public bool IsReady => CurrentProfile != null;
        public string ActorId => ResolveActorId();

        public event Action<ActorProfileDto> ProfileLoaded;

        private void Awake()
        {
            _gateway = FindAnyObjectByType<SecondSpawnGatewayClient>();
        }

        private void Start()
        {
            if (_loadOnStart)
            {
                Refresh();
            }
        }

        public void Refresh()
        {
            if (_loadRoutine != null)
            {
                StopCoroutine(_loadRoutine);
            }

            _loadRoutine = StartCoroutine(LoadActorProfile());
        }

        public IEnumerator LoadActorProfile()
        {
            if (_gateway == null)
            {
                _gateway = FindAnyObjectByType<SecondSpawnGatewayClient>();
            }

            if (_gateway == null)
            {
                Debug.LogWarning($"[ActorProfileBinder] No gateway client found for actor {ActorId}.");
                _loadRoutine = null;
                yield break;
            }

            ActorProfileDto profile = null;
            string profileError = null;
            yield return _gateway.GetNakamaActorProfile(BuildProfileRequest(), value => profile = value, error => profileError = error);
            if (profile == null)
            {
                Debug.LogWarning($"[ActorProfileBinder] Actor profile load failed for {ActorId}: {profileError}");
                _loadRoutine = null;
                yield break;
            }

            CurrentProfile = profile;
            ApplyProfile(profile);

            if (_seedMemoryOnStart && !string.IsNullOrWhiteSpace(_seedMemorySummary))
            {
                ActorProfileDto memoryProfile = null;
                string memoryError = null;
                yield return _gateway.AddNakamaActorMemory(new ActorMemoryAddRequestDto
                {
                    actor_id = profile.actor_id,
                    kind = string.IsNullOrWhiteSpace(_seedMemoryKind) ? "system" : _seedMemoryKind.Trim(),
                    summary = _seedMemorySummary.Trim(),
                    importance = Mathf.Clamp(_seedMemoryImportance, 1, 10)
                }, value => memoryProfile = value, error => memoryError = error);

                if (memoryProfile != null)
                {
                    CurrentProfile = memoryProfile;
                    ApplyProfile(memoryProfile);
                }
                else if (!string.IsNullOrWhiteSpace(memoryError))
                {
                    Debug.LogWarning($"[ActorProfileBinder] Actor memory seed failed for {ActorId}: {memoryError}");
                }
            }

            ProfileLoaded?.Invoke(CurrentProfile);
            _loadRoutine = null;
        }

        private ActorProfileRequestDto BuildProfileRequest()
        {
            return new ActorProfileRequestDto
            {
                actor_id = ResolveActorId(),
                actor_type = string.IsNullOrWhiteSpace(_actorType) ? "npc" : _actorType.Trim(),
                display_name = string.IsNullOrWhiteSpace(_displayName) ? gameObject.name : _displayName.Trim(),
                archetype_id = string.IsNullOrWhiteSpace(_archetypeId) ? "prototype-npc" : _archetypeId.Trim(),
                visual_prefab_key = string.IsNullOrWhiteSpace(_visualPrefabKey) ? "prototype-npc" : _visualPrefabKey.Trim()
            };
        }

        private void ApplyProfile(ActorProfileDto profile)
        {
            if (profile == null)
            {
                return;
            }

            if (!string.IsNullOrWhiteSpace(profile.actor_id))
            {
                _actorId = profile.actor_id.Trim();
            }

            if (!string.IsNullOrWhiteSpace(profile.display_name))
            {
                _displayName = profile.display_name.Trim();
                gameObject.name = _displayName;
            }
        }

        private string ResolveActorId()
        {
            if (!string.IsNullOrWhiteSpace(_actorId))
            {
                return _actorId.Trim();
            }

            return string.IsNullOrWhiteSpace(gameObject.name) ? "npc-body" : gameObject.name.Trim();
        }
    }
}
