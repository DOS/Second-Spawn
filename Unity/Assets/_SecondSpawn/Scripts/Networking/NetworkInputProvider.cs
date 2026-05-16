using System.Collections.Generic;
using Fusion;
using Fusion.Sockets;
using UnityEngine;
using UnityEngine.InputSystem;

namespace SecondSpawn.Networking
{
    /// <summary>
    /// Fusion 2 input contract. The struct is the wire format - it must be
    /// blittable (no reference types). Each field consumes bandwidth every
    /// network tick, so keep this lean.
    ///
    /// <para>Server treats every input as a SUGGESTION, not a command
    /// (Pillar 4 Server-authoritative gameplay). The server validates each
    /// field in <see cref="NetworkPlayer.FixedUpdateNetwork"/> before
    /// mutating <c>[Networked]</c> state.</para>
    /// </summary>
    public struct NetworkInputData : INetworkInput
    {
        public float HorizontalAxis;
        public float VerticalAxis;
        public NetworkBool Run;
        public NetworkBool Jump;
        public NetworkBool Interact;
        public NetworkBool AbilitySlot1;
    }

    /// <summary>
    /// Collects Unity Input System input each Fusion tick and submits it
    /// to the <see cref="NetworkRunner"/> as <see cref="NetworkInputData"/>.
    ///
    /// <para>The same code path is shared with the offline AI agent: when
    /// <see cref="NetworkPlayer.IsAgentControlled"/> is true, the
    /// server-side <c>OfflineAgentRunner</c> populates the input struct from
    /// LLM-derived intents and the rest of the pipeline is identical -
    /// rate-limited + capability-capped per Pillar 1 anti-abuse.</para>
    /// </summary>
    [DisallowMultipleComponent]
    public sealed class NetworkInputProvider : MonoBehaviour, INetworkRunnerCallbacks
    {
        private NetworkRunner _runner;
        private bool _jumpQueued;
        private bool _runToggled = true;

        private void Awake()
        {
            _runner = GetComponent<NetworkRunner>();
            if (_runner != null) _runner.AddCallbacks(this);
        }

        private void OnDestroy()
        {
            if (_runner != null) _runner.RemoveCallbacks(this);
        }

        private void Update()
        {
            var kb = Keyboard.current;
            if (kb == null)
            {
                return;
            }

            if (kb.leftShiftKey.wasPressedThisFrame || kb.rightShiftKey.wasPressedThisFrame)
            {
                _runToggled = !_runToggled;
            }

            if (kb.spaceKey.wasPressedThisFrame)
            {
                _jumpQueued = true;
            }
        }

        public void OnInput(NetworkRunner runner, NetworkInput input)
        {
            // Vertical slice: read raw keyboard via Unity Input System.
            // Slice phase 2 will route through InputSystem_Actions
            // (Assets/InputSystem_Actions.inputactions) for rebinding.
            var kb = Keyboard.current;
            if (kb == null) return;

            var data = new NetworkInputData
            {
                HorizontalAxis = (kb.dKey.isPressed ? 1f : 0f) - (kb.aKey.isPressed ? 1f : 0f),
                VerticalAxis = (kb.wKey.isPressed ? 1f : 0f) - (kb.sKey.isPressed ? 1f : 0f),
                Run = _runToggled,
                Jump = _jumpQueued,
                Interact = kb.eKey.isPressed,
                AbilitySlot1 = kb.digit1Key.isPressed,
            };

            _jumpQueued = false;
            input.Set(data);
        }

        // Unused callbacks - implement so INetworkRunnerCallbacks is satisfied.
        // Real handlers land as slice phase 2+ features need them.
#pragma warning disable UNT0006 // Fusion callbacks intentionally share names with Unity messages but use Fusion-specific signatures.
        public void OnConnectedToServer(NetworkRunner runner) { }
        public void OnConnectFailed(NetworkRunner runner, NetAddress remoteAddress, NetConnectFailedReason reason) { }
        public void OnConnectRequest(NetworkRunner runner, NetworkRunnerCallbackArgs.ConnectRequest request, byte[] token) { }
        public void OnCustomAuthenticationResponse(NetworkRunner runner, Dictionary<string, object> data) { }
        public void OnDisconnectedFromServer(NetworkRunner runner, NetDisconnectReason reason) { }
        public void OnHostMigration(NetworkRunner runner, HostMigrationToken hostMigrationToken) { }
        public void OnInputMissing(NetworkRunner runner, PlayerRef player, NetworkInput input) { }
        public void OnObjectEnterAOI(NetworkRunner runner, NetworkObject obj, PlayerRef player) { }
        public void OnObjectExitAOI(NetworkRunner runner, NetworkObject obj, PlayerRef player) { }
        public void OnPlayerJoined(NetworkRunner runner, PlayerRef player) { }
        public void OnPlayerLeft(NetworkRunner runner, PlayerRef player) { }
        public void OnReliableDataProgress(NetworkRunner runner, PlayerRef player, ReliableKey key, float progress) { }
        public void OnReliableDataReceived(NetworkRunner runner, PlayerRef player, ReliableKey key, System.ReadOnlySpan<byte> data) { }
        public void OnSceneLoadDone(NetworkRunner runner) { }
        public void OnSceneLoadStart(NetworkRunner runner) { }
        public void OnSessionListUpdated(NetworkRunner runner, List<SessionInfo> sessionList) { }
        public void OnShutdown(NetworkRunner runner, ShutdownReason shutdownReason) { }

#pragma warning disable CS0618 // SimulationMessagePtr is obsolete in Fusion 2.1+ but the interface still requires the implementation per Photon/Fusion/release_history.txt line 408.
        public void OnUserSimulationMessage(NetworkRunner runner, SimulationMessagePtr message) { }
#pragma warning restore CS0618
#pragma warning restore UNT0006
    }
}
