using UnityEngine;

namespace SecondSpawn.Networking
{
    /// <summary>
    /// Collects Unity Input System input each Fusion tick and submits it
    /// to the <c>NetworkRunner</c> as a structured intent (NOT a command).
    ///
    /// <para>The server treats every input as a suggestion - it validates
    /// against authoritative state before mutating any
    /// <c>[Networked]</c> property. This is the cornerstone of Pillar 4
    /// (Server-authoritative gameplay) in
    /// <c>docs/design/01-pillars.md</c>.</para>
    ///
    /// <para>Scaffold: the actual Fusion 2 <c>INetworkInput</c>
    /// implementation goes here post-SDK-install
    /// (see <c>docs/setup/fusion-install.md</c>).</para>
    ///
    /// <para>POST-SDK-INSTALL implementation outline:</para>
    /// <list type="number">
    ///   <item>Implement <c>INetworkRunnerCallbacks.OnInput(NetworkRunner, NetworkInput)</c>.</item>
    ///   <item>Define a struct <c>SecondSpawnInput : INetworkInput</c>
    ///         carrying: movement axis, ability slot, target id,
    ///         intent flag (interact / equip / reincarnate).</item>
    ///   <item>In <c>OnInput</c>, read the Unity Input System
    ///         (<c>InputSystem_Actions.inputactions</c>) and populate
    ///         the struct, then <c>input.Set(...)</c>.</item>
    ///   <item>Server consumes the input each tick via
    ///         <c>GetInput()</c>; do NOT trust without validation.</item>
    /// </list>
    ///
    /// <para>The offline AI agent shares this code path: when the player
    /// is offline, the server-side <c>OfflineAgentRunner</c> populates
    /// the same input struct from LLM-derived intents. Inputs from
    /// the agent are rate-limited + capability-capped identically to
    /// inputs from the human (Pillar 1, anti-abuse).</para>
    /// </summary>
    public sealed class NetworkInputProvider : MonoBehaviour
    {
        // TODO (post-SDK-install): implement INetworkRunnerCallbacks +
        // SecondSpawnInput struct per the summary.
    }
}
