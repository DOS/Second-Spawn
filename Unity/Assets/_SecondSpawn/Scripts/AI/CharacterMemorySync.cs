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
                    yield return ApplyProfileEquipmentWhenAvailable();
                    yield break;
                }
            }

            yield return _gateway.GetContext(ctx =>
            {
                _context = ctx;
                var soulName = ctx?.body?.soul?.name ?? "unknown";
                Debug.Log($"[CharacterMemorySync] Loaded gateway prototype soul '{soulName}'.");
            }, Debug.LogWarning);
            yield return ApplyProfileEquipmentWhenAvailable();
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

        private IEnumerator ApplyProfileEquipmentWhenAvailable()
        {
            if (!_applyProfileEquipmentToLocalPlayer)
            {
                yield break;
            }

            var equipmentVisualId = _context?.body?.equipment?.equipment_visual_id ?? EquipmentVisualCatalog.None;
            if (equipmentVisualId == EquipmentVisualCatalog.None)
            {
                yield break;
            }

            const float maxWaitSeconds = 10f;
            var elapsed = 0f;
            while (elapsed < maxWaitSeconds)
            {
                if (TryApplyProfileEquipment(equipmentVisualId))
                {
                    yield break;
                }

                elapsed += Time.deltaTime;
                yield return null;
            }

            Debug.LogWarning($"[CharacterMemorySync] No local state-authority player was ready for equipment visual {equipmentVisualId}.");
        }

        private static bool TryApplyProfileEquipment(int equipmentVisualId)
        {
            var players = Object.FindObjectsByType<NetworkPlayer>(FindObjectsInactive.Exclude, FindObjectsSortMode.None);
            foreach (var player in players)
            {
                if (!IsLocalAuthoritativePlayer(player))
                {
                    continue;
                }

                player.EquipmentVisualId = equipmentVisualId;
                var loaders = player.GetComponentsInChildren<LocalVisualPrefabLoader>(includeInactive: true);
                foreach (var loader in loaders)
                {
                    loader.ApplyEquipmentVisual(equipmentVisualId);
                }

                Debug.Log($"[CharacterMemorySync] Applied profile equipment visual {equipmentVisualId} to local player.");
                return true;
            }

            return false;
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

            return player.Object.InputAuthority == PlayerRef.None ||
                   player.Object.InputAuthority == player.Runner.LocalPlayer;
        }
    }
}
