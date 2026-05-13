using UnityEngine;

namespace SecondSpawn.Networking
{
    /// <summary>
    /// Authoritative networked player state. Spawned by the dedicated
    /// server on join; despawned on disconnect.
    ///
    /// <para>Scaffold: this will become a Fusion 2 <c>NetworkBehaviour</c>
    /// with <c>[Networked]</c> properties post-SDK-install
    /// (see <c>docs/setup/fusion-install.md</c>).</para>
    ///
    /// <para>Owned by the server. Client sees state via Fusion
    /// replication; client never writes <c>[Networked]</c> properties
    /// directly. Visual prediction is OK in the client controller layer
    /// (see <c>SecondSpawn.Gameplay.PlayerController</c>).</para>
    ///
    /// <para>POST-SDK-INSTALL <c>[Networked]</c> property list (per
    /// <c>docs/design/05-networking-architecture.md</c>):</para>
    /// <list type="bullet">
    ///   <item><c>[Networked] public Vector3 Position</c></item>
    ///   <item><c>[Networked] public Quaternion Rotation</c></item>
    ///   <item><c>[Networked] public int CultivationTier</c>
    ///         - persisted across reincarnation per docs/design/04-cultivation-system.md.</item>
    ///   <item><c>[Networked] public float Hp</c></item>
    ///   <item><c>[Networked] public float Stamina</c></item>
    ///   <item><c>[Networked] public NetworkString&lt;_64&gt; CurrentZone</c></item>
    ///   <item><c>[Networked] public bool IsAgentControlled</c>
    ///         - true when offline AI agent is driving (Pillar 1).</item>
    /// </list>
    ///
    /// <para>Server-side flushes to Supabase happen on:</para>
    /// <list type="bullet">
    ///   <item>Player disconnect (final snapshot).</item>
    ///   <item>Zone transition.</item>
    ///   <item>Periodic interval (configurable in
    ///         <c>SecondSpawn.Settings.SecondSpawnConfig</c>).</item>
    ///   <item>Reincarnation - mandatory before despawn so
    ///         cultivation tier carries forward.</item>
    /// </list>
    /// </summary>
    public sealed class NetworkPlayer : MonoBehaviour
    {
        // TODO (post-SDK-install): inherit from Fusion.NetworkBehaviour
        // and add [Networked] properties listed in the summary.
    }
}
