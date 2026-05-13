using UnityEngine;

namespace SecondSpawn.AI
{
    /// <summary>
    /// AI agent stub for offline-player autoplay (USP #1 in
    /// docs/design/00-game-concept.md). The agent runs server-side within
    /// the Photon Fusion 2 server tick - this client-side script is just
    /// the visual indicator + activity log surface for when the player
    /// returns and observes what happened while offline.
    ///
    /// The actual decision loop lives in the dedicated server build:
    ///   pull state from Fusion -> reason via Go LLM gateway
    ///   (backend/gateway) -> emit action intent -> server validates
    ///   per intent.go -> apply mutation.
    ///
    /// TODO (slice phase 7):
    /// - Wire activity log UI (last N agent actions visible to player).
    /// - Subtle visual indicator when nearby player is agent-controlled
    ///   (per docs/design/01-pillars.md Pillar 1 cross-departmental
    ///   table).
    /// - Per-player rate limit + capability cap inheritance.
    /// </summary>
    public sealed class OfflineAgent : MonoBehaviour
    {
    }
}
