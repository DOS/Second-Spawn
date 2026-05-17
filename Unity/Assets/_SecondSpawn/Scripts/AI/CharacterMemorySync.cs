using System.Collections;
using Fusion;
using SecondSpawn.Networking;
using UnityEngine;

namespace SecondSpawn.AI
{
    [DisallowMultipleComponent]
    [RequireComponent(typeof(SecondSpawnGatewayClient))]
    public sealed class CharacterMemorySync : MonoBehaviour
    {
        [SerializeField] private bool _syncOnStart = true;
        [SerializeField] private bool _preferNakama = true;
        [SerializeField] private bool _seedPrototypeMemory = true;
        [SerializeField] private bool _applyProfileStatsToLocalPlayer = true;
        [SerializeField] private bool _applyProfileEquipmentToLocalPlayer = true;
        [SerializeField, TextArea] private string _prototypeMemory =
            "JOY wants overnight prototype progress without client-side LLM secrets.";

        private SecondSpawnGatewayClient _gateway;
        private AgentContextDto _context;

        public AgentContextDto Context => _context;

        private void Awake()
        {
            _gateway = GetComponent<SecondSpawnGatewayClient>();
        }

        private IEnumerator Start()
        {
            if (!_syncOnStart)
            {
                yield break;
            }

            yield return Refresh();

            if (_seedPrototypeMemory && !string.IsNullOrWhiteSpace(_prototypeMemory))
            {
                yield return AddMemory(new MemoryRecordDto
                {
                    kind = "preference",
                    summary = _prototypeMemory,
                    importance = 7
                });
            }
        }

        public IEnumerator Refresh()
        {
            if (_preferNakama)
            {
                yield return WaitForAuthAttempt();
                if (_gateway.HasNakamaSession)
                {
                    yield return _gateway.GetNakamaContext(ctx =>
                    {
                        _context = ctx;
                        var soulName = ctx?.body?.soul?.name ?? "unknown";
                        Debug.Log($"[CharacterMemorySync] Loaded Nakama soul '{soulName}'.");
                    }, Debug.LogWarning);
                    yield return ApplyProfileToLocalPlayerWhenAvailable();
                    yield break;
                }
            }

            yield return _gateway.GetContext(ctx =>
            {
                _context = ctx;
                var soulName = ctx?.body?.soul?.name ?? "unknown";
                Debug.Log($"[CharacterMemorySync] Loaded gateway prototype soul '{soulName}'.");
            }, Debug.LogWarning);
            yield return ApplyProfileToLocalPlayerWhenAvailable();
        }

        public IEnumerator ApplyBodyTimeEvent(BodyTimeEventRequestDto request)
        {
            if (!_preferNakama || !_gateway.HasNakamaSession)
            {
                Debug.LogWarning("[CharacterMemorySync] BodyTime events require an authenticated Nakama session.");
                yield break;
            }

            AgentContextDto context = null;
            string error = null;
            yield return _gateway.ApplyNakamaBodyTimeEvent(request, value => context = value, value => error = value);
            if (context == null)
            {
                Debug.LogWarning($"[CharacterMemorySync] BodyTime event failed: {error}");
                yield break;
            }

            _context = context;
            yield return ApplyProfileToLocalPlayerWhenAvailable();
        }

        public IEnumerator ReincarnateCurrentBody(string reason)
        {
            if (!_preferNakama || !_gateway.HasNakamaSession)
            {
                Debug.LogWarning("[CharacterMemorySync] Reincarnation requires an authenticated Nakama session.");
                yield break;
            }

            AgentContextDto context = null;
            string error = null;
            yield return _gateway.ReincarnateNakamaBody(new ReincarnationRequestDto
            {
                id = BuildClientEventId("reincarnation"),
                reason = string.IsNullOrWhiteSpace(reason) ? "Unity prototype debug reincarnation." : reason.Trim()
            }, value => context = value, value => error = value);

            if (context == null)
            {
                Debug.LogWarning($"[CharacterMemorySync] Reincarnation failed: {error}");
                yield break;
            }

            _context = context;
            yield return ApplyProfileToLocalPlayerWhenAvailable();
        }

        private IEnumerator WaitForAuthAttempt()
        {
            const float maxWaitSeconds = 10f;
            var elapsed = 0f;
            while (!_gateway.IsAuthReady && elapsed < maxWaitSeconds)
            {
                elapsed += Time.deltaTime;
                yield return null;
            }
        }

        private IEnumerator AddMemory(MemoryRecordDto memory)
        {
            if (_preferNakama && _gateway.HasNakamaSession)
            {
                yield return _gateway.AddNakamaMemory(memory, ctx => _context = ctx, Debug.LogWarning);
                yield break;
            }

            yield return _gateway.AddMemory(memory, ctx => _context = ctx, Debug.LogWarning);
        }

        private IEnumerator ApplyProfileToLocalPlayerWhenAvailable()
        {
            if (!_applyProfileEquipmentToLocalPlayer && !_applyProfileStatsToLocalPlayer)
            {
                yield break;
            }

            var body = _context?.body;
            if (body == null)
            {
                yield break;
            }

            const float maxWaitSeconds = 10f;
            const float retryIntervalSeconds = 0.25f;
            var elapsed = 0f;
            while (elapsed < maxWaitSeconds)
            {
                if (TryApplyProfileBody(body))
                {
                    yield break;
                }

                elapsed += retryIntervalSeconds;
                yield return new WaitForSeconds(retryIntervalSeconds);
            }

            Debug.LogWarning("[CharacterMemorySync] No local state-authority player was ready for profile body sync.");
        }

        private bool TryApplyProfileBody(BodyProfileDto body)
        {
            var players = Object.FindObjectsByType<NetworkPlayer>(FindObjectsInactive.Exclude);
            foreach (var player in players)
            {
                if (!IsLocalAuthoritativePlayer(player))
                {
                    continue;
                }

                if (_applyProfileStatsToLocalPlayer)
                {
                    ApplyStats(player, body);
                }

                if (_applyProfileEquipmentToLocalPlayer)
                {
                    ApplyEquipment(player, body);
                }

                Debug.Log($"[CharacterMemorySync] Applied profile body stats and visuals to local player '{player.name}'.");
                return true;
            }

            return false;
        }

        private void ApplyStats(NetworkPlayer player, BodyProfileDto body)
        {
            var stats = body.stats ?? new CharacterStatsDto();
            var time = body.time ?? new BodyTimeDto();
            var account = _context?.player ?? new PlayerProfileDto();
            player.ApplyProfileStats(
                stats.level,
                stats.vitality,
                stats.force,
                stats.agility,
                stats.focus,
                stats.resilience,
                stats.max_health,
                stats.max_energy,
                stats.attack_power,
                stats.defense_power,
                ToNetworkSeconds(time.remaining_seconds),
                ToNetworkSeconds(time.max_seconds),
                ToNetworkSeconds(time.danger_drain_rate),
                body.lifecycle,
                ToNetworkSeconds(account.second_balance_seconds),
                ToNetworkSeconds(account.reincarnation_count));
        }

        public static string BuildClientEventId(string prefix)
        {
            return $"{prefix}-{System.Guid.NewGuid():N}";
        }

        private static void ApplyEquipment(NetworkPlayer player, BodyProfileDto body)
        {
            var equipmentVisualId = body.equipment?.equipment_visual_id ?? EquipmentVisualCatalog.None;
            if (equipmentVisualId == EquipmentVisualCatalog.None)
            {
                return;
            }

            player.ApplyProfileEquipment(equipmentVisualId);
            var loaders = player.GetComponentsInChildren<LocalVisualPrefabLoader>(includeInactive: true);
            foreach (var loader in loaders)
            {
                loader.ApplyEquipmentVisual(equipmentVisualId);
            }
        }

        private static int ToNetworkSeconds(long seconds)
        {
            if (seconds <= 0)
            {
                return 0;
            }

            return seconds >= int.MaxValue ? int.MaxValue : (int)seconds;
        }

        private static bool IsLocalAuthoritativePlayer(NetworkPlayer player)
        {
            if (player == null || !player.HasStateAuthority)
            {
                return false;
            }

            if (player.Object == null || player.Runner == null)
            {
                return true;
            }

            if (player.Runner.IsServer)
            {
                return true;
            }

            if (!player.Runner.IsSharedModeMasterClient)
            {
                return false;
            }

            return player.Object.InputAuthority == PlayerRef.None ||
                   player.Object.InputAuthority == player.Runner.LocalPlayer;
        }
    }
}
