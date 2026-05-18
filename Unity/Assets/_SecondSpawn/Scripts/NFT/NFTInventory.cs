using UnityEngine;

namespace SecondSpawn.NFT
{
    /// <summary>
    /// NFT inventory stub - Hunter skin, weapon, pet equipment from
    /// DOS Chain via thirdweb-api MCP. Server-authoritative escrow:
    /// lock on equip, release on unequip / reincarnation.
    ///
    /// API keys for thirdweb / DOS Chain NEVER live in this client
    /// (per Hard Rule #3). All chain interactions go through the Go
    /// server-side Nakama or wallet service which signs and submits transactions.
    ///
    /// TODO (slice phase 6):
    /// - Wire wallet auth (sign-message via Supabase + DOS Chain).
    /// - Optimistic UI: show equip success client-side, reconcile on
    ///   chain confirmation failure.
    /// - Cache lock state in Supabase for cheap reads.
    /// - 1 equip slot for pets per docs/design/00-game-concept.md
    ///   gameplay arch.
    /// </summary>
    public sealed class NFTInventory : MonoBehaviour
    {
    }
}
