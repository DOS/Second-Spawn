using UnityEngine;

namespace SecondSpawn.Gameplay
{
    /// <summary>
    /// Top-down ARPG player controller stub.
    ///
    /// Starts as a project-owned movement contract. Fusion Simple KCC and
    /// Opsive Ultimate Character Controller can be evaluated against this
    /// baseline later. Server-authoritative per Pillar 4 (see
    /// docs/design/01-pillars.md): the client predicts visually but the
    /// dedicated Photon Fusion 2 server validates every action.
    ///
    /// TODO (slice phase 2):
    /// - Spike Fusion Simple KCC based on Photon Pirate Adventure patterns.
    /// - Evaluate Opsive UCC abilities only after the smaller Fusion-native
    ///   movement path is proven.
    /// - Hook input via Unity Input System (already imported as
    ///   InputSystem_Actions.inputactions).
    /// - Bridge to NetworkRunnerSetup for Networked state authority.
    /// </summary>
    public sealed class PlayerController : MonoBehaviour
    {
    }
}
