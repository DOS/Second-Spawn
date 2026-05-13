using UnityEngine;

namespace SecondSpawn.Networking
{
    /// <summary>
    /// Photon Fusion 2 NetworkRunner bootstrap stub.
    ///
    /// Will spawn the NetworkRunner, choose Server Mode (production) vs
    /// Host Mode (dev) per build flag, configure tick rate (60Hz target),
    /// and reference the MetaDOS BR template patterns at
    /// D:\Projects\MetaDOS (read-only) for the boilerplate.
    ///
    /// TODO (slice phase 2):
    /// - Install Photon Fusion 2 SDK via UPM (blocked on JOY App ID).
    /// - Add NetworkRunner component + StartGame call with the Photon
    ///   App ID injected from SecondSpawnConfig (Assets/Settings).
    /// - Implement host migration + interest management per BR200 sample.
    /// - Server-only: never use Host Mode in production builds
    ///   (Hard Rule #4 in CLAUDE.md / AGENTS.md).
    /// </summary>
    public sealed class NetworkRunnerSetup : MonoBehaviour
    {
    }
}
