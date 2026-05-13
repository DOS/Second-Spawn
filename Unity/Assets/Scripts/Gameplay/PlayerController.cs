using UnityEngine;

namespace SecondSpawn.Gameplay
{
    /// <summary>
    /// Top-down ARPG player controller stub.
    ///
    /// Will eventually wrap Opsive Ultimate Character Controller for combat,
    /// movement, and ability dispatch. Server-authoritative per Pillar 4
    /// (see docs/design/01-pillars.md): the client predicts visually but
    /// the dedicated Photon Fusion 2 server validates every action.
    ///
    /// TODO (slice phase 2):
    /// - Wire Opsive UCC abilities to the server intent schema
    ///   (backend/gateway/internal/intent/intent.go).
    /// - Hook input via Unity Input System (already imported as
    ///   InputSystem_Actions.inputactions).
    /// - Bridge to NetworkRunnerSetup for Networked state authority.
    /// </summary>
    public sealed class PlayerController : MonoBehaviour
    {
    }
}
