using Fusion;
using UnityEngine;

namespace SecondSpawn.Networking
{
    /// <summary>
    /// Owns the Fusion 2 <see cref="NetworkRunner"/> lifecycle. Singleton.
    ///
    /// <para>Mode selection (per <c>docs/design/05-networking-architecture.md</c>):</para>
    /// <list type="bullet">
    ///   <item><c>Application.isBatchMode</c> true (Linux headless server build)
    ///         -> <see cref="GameMode.Server"/> dedicated, production canonical.</item>
    ///   <item><c>Application.isBatchMode</c> false (editor / standalone client)
    ///         -> <see cref="GameMode.Host"/> on Photon Cloud free 20 CCU, DEV ONLY.</item>
    /// </list>
    ///
    /// <para>Hard Rule #4 (CLAUDE.md / AGENTS.md): production builds MUST use
    /// Server Mode dedicated. CI workflow
    /// <c>.github/workflows/unity-build.yml</c> includes
    /// <c>-batchmode -nographics -server</c> for the Linux dedicated server
    /// target to enforce this at build time.</para>
    ///
    /// <para>Photon App ID is read from <c>PhotonAppSettings</c>
    /// (Resources singleton at <c>Assets/Photon/Fusion/Resources/PhotonAppSettings.asset</c>),
    /// which is auto-loaded by Fusion at runtime.</para>
    /// </summary>
    [DisallowMultipleComponent]
    public sealed class NetworkRunnerSetup : MonoBehaviour
    {
        [SerializeField, Tooltip("Default zone session name. Production will derive this from zone instance routing.")]
        private string _sessionName = "SecondSpawn-Zone-Default";

        [SerializeField, Tooltip("Max players per zone instance. Vertical slice target: 4-20.")]
        private int _maxPlayersPerZone = 20;

        private NetworkRunner _runner;

        private async void Start()
        {
            if (_runner != null)
            {
                Debug.LogWarning("[NetworkRunnerSetup] Runner already started; ignoring duplicate Start().");
                return;
            }

            _runner = gameObject.AddComponent<NetworkRunner>();
            _runner.ProvideInput = !Application.isBatchMode;

            var mode = Application.isBatchMode ? GameMode.Server : GameMode.Host;
            Debug.Log($"[NetworkRunnerSetup] Starting {mode} session '{_sessionName}' (max {_maxPlayersPerZone} players).");

            var startArgs = new StartGameArgs
            {
                GameMode = mode,
                SessionName = _sessionName,
                PlayerCount = _maxPlayersPerZone,
                SceneManager = gameObject.AddComponent<NetworkSceneManagerDefault>(),
            };

            var result = await _runner.StartGame(startArgs);
            if (!result.Ok)
            {
                Debug.LogError($"[NetworkRunnerSetup] StartGame failed: {result.ShutdownReason} - {result.ErrorMessage}");
                return;
            }

            Debug.Log($"[NetworkRunnerSetup] {mode} session ready. SessionInfo={_runner.SessionInfo?.Name} IsServer={_runner.IsServer} IsSharedModeMasterClient={_runner.IsSharedModeMasterClient}");
        }

        private void OnDestroy()
        {
            if (_runner != null && _runner.IsRunning)
            {
                _ = _runner.Shutdown();
            }
        }
    }
}
