using System.Collections.Generic;
using Fusion;
using Fusion.Sockets;
using UnityEngine;

namespace SecondSpawn.Networking
{
    /// <summary>
    /// Server-authoritative player spawner. On
    /// <see cref="OnPlayerJoined"/>, the server spawns the configured
    /// player prefab with input authority assigned to the joining player
    /// and reparents the spawned NetworkObject under
    /// <see cref="_spawnRoot"/> (the scene's _DynamicObjects bucket).
    ///
    /// <para>Per <c>docs/design/05-networking-architecture.md</c> + Pillar
    /// 4 (Server-authoritative gameplay), only the server creates
    /// NetworkObjects. Clients receive replicated spawn callbacks via
    /// Fusion's standard pipeline.</para>
    ///
    /// <para>Vertical slice scope: spawns a unit cube as a placeholder for
    /// the future Hunter NFT skin (slice phase 2). Spawn positions form a
    /// ring around origin so multiple players are visually separated for
    /// the smoke test.</para>
    /// </summary>
    [DisallowMultipleComponent]
    public sealed class PlayerSpawner : MonoBehaviour, INetworkRunnerCallbacks
    {
        [SerializeField, Tooltip("Player_NetworkCube prefab spawned per joining player. Must have NetworkObject + NetworkPlayer components.")]
        private NetworkObject _playerPrefab;

        [SerializeField, Tooltip("Scene Transform that owns spawned player objects (e.g. _DynamicObjects).")]
        private Transform _spawnRoot;

        [SerializeField, Tooltip("Radius of the spawn ring around origin (units).")]
        private float _spawnRingRadius = 3f;

        [SerializeField, Tooltip("Vertical lift so the cube sits visibly on the ground plane.")]
        private float _spawnYOffset = 0.5f;

        private NetworkRunner _runner;
        private int _spawnCounter;

        private void Awake()
        {
            _runner = GetComponent<NetworkRunner>();
            if (_runner != null) _runner.AddCallbacks(this);
        }

        private void OnDestroy()
        {
            if (_runner != null) _runner.RemoveCallbacks(this);
        }

        public void OnPlayerJoined(NetworkRunner runner, PlayerRef player)
        {
            if (!runner.IsServer) return;
            if (_playerPrefab == null)
            {
                Debug.LogError("[PlayerSpawner] _playerPrefab not assigned; cannot spawn for " + player);
                return;
            }

            var spawnPos = ComputeSpawnPosition(_spawnCounter);
            var spawnRoot = _spawnRoot;
            runner.Spawn(_playerPrefab, spawnPos, Quaternion.identity, player,
                onBeforeSpawned: (r, obj) =>
                {
                    // TODO(phase-B follow-up): documented Fusion 2 pattern for
                    // pre-Spawned() reparent. Observed under Fusion 2.1.1 to
                    // NOT stick - the NetworkObject still ends up at scene
                    // root. Likely Fusion moves the GO into a runner-managed
                    // scene after this callback; investigate Spawn flow.
                    if (spawnRoot != null) obj.transform.SetParent(spawnRoot, worldPositionStays: true);
                });
            _spawnCounter++;
            Debug.Log($"[PlayerSpawner] Spawned player cube for {player} at {spawnPos}");
        }

        public void OnPlayerLeft(NetworkRunner runner, PlayerRef player)
        {
            if (!runner.IsServer) return;
            foreach (var no in runner.GetAllNetworkObjects())
            {
                if (no.InputAuthority == player)
                {
                    runner.Despawn(no);
                    break;
                }
            }
        }

        private Vector3 ComputeSpawnPosition(int slot)
        {
            const float twoPi = Mathf.PI * 2f;
            float angle = slot * (twoPi / 8f);
            return new Vector3(
                Mathf.Cos(angle) * _spawnRingRadius,
                _spawnYOffset,
                Mathf.Sin(angle) * _spawnRingRadius);
        }

        // Unused INetworkRunnerCallbacks members - implement to satisfy interface.
        public void OnConnectedToServer(NetworkRunner runner) { }
        public void OnConnectFailed(NetworkRunner runner, NetAddress remoteAddress, NetConnectFailedReason reason) { }
        public void OnConnectRequest(NetworkRunner runner, NetworkRunnerCallbackArgs.ConnectRequest request, byte[] token) { }
        public void OnCustomAuthenticationResponse(NetworkRunner runner, Dictionary<string, object> data) { }
        public void OnDisconnectedFromServer(NetworkRunner runner, NetDisconnectReason reason) { }
        public void OnHostMigration(NetworkRunner runner, HostMigrationToken hostMigrationToken) { }
        public void OnInput(NetworkRunner runner, NetworkInput input) { }
        public void OnInputMissing(NetworkRunner runner, PlayerRef player, NetworkInput input) { }
        public void OnObjectEnterAOI(NetworkRunner runner, NetworkObject obj, PlayerRef player) { }
        public void OnObjectExitAOI(NetworkRunner runner, NetworkObject obj, PlayerRef player) { }
        public void OnReliableDataProgress(NetworkRunner runner, PlayerRef player, ReliableKey key, float progress) { }
        public void OnReliableDataReceived(NetworkRunner runner, PlayerRef player, ReliableKey key, System.ReadOnlySpan<byte> data) { }
        public void OnSceneLoadDone(NetworkRunner runner) { }
        public void OnSceneLoadStart(NetworkRunner runner) { }
        public void OnSessionListUpdated(NetworkRunner runner, List<SessionInfo> sessionList) { }
        public void OnShutdown(NetworkRunner runner, ShutdownReason shutdownReason) { }

#pragma warning disable CS0618 // SimulationMessagePtr is obsolete in Fusion 2.1+ but the interface still requires the implementation per Photon/Fusion/release_history.txt line 408.
        public void OnUserSimulationMessage(NetworkRunner runner, SimulationMessagePtr message) { }
#pragma warning restore CS0618
    }
}
