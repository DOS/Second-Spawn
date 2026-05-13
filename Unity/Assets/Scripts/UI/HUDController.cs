using UnityEngine;

namespace SecondSpawn.UI
{
    /// <summary>
    /// HUD controller stub - combat, cultivation tier, currency,
    /// reincarnation prompt, AI agent activity log entry point.
    ///
    /// TODO (slice throughout phases 2-7):
    /// - Bind cultivation tier display to persisted state.
    /// - Wire combat HUD (HP, stamina, abilities) to PlayerController.
    /// - Reincarnation flow UI (death -> SECOND token cost -> respawn).
    /// - AI agent activity log overlay (visible on player return).
    /// - See deferred templates .claude/templates/_deferred/hud-design.md
    ///   when this work starts.
    /// </summary>
    public sealed class HUDController : MonoBehaviour
    {
    }
}
