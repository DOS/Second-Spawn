using UnityEngine;

namespace SecondSpawn.Networking
{
    /// <summary>
    /// Owns the Photon Fusion 2 <c>NetworkRunner</c> lifecycle. Singleton.
    ///
    /// <para>This is a scaffold: the actual Fusion API surface is gated
    /// behind installing the SDK (see <c>docs/setup/fusion-install.md</c>).
    /// Until that happens the script compiles clean as a plain
    /// <c>MonoBehaviour</c> placeholder so the rest of the project can
    /// reference <c>SecondSpawn.Networking</c> assembly without a
    /// hard dependency.</para>
    ///
    /// <para>Design references:</para>
    /// <list type="bullet">
    ///   <item><c>docs/adr/0001-photon-fusion-2.md</c> - why Fusion 2.</item>
    ///   <item><c>docs/adr/0006-fusion-2-scratch-over-template.md</c>
    ///         - why scratch + extract-pattern rather than BR200 drop-in.</item>
    ///   <item><c>docs/design/05-networking-architecture.md</c> - full
    ///         networking GDD.</item>
    /// </list>
    ///
    /// <para>Mode selection (per <c>docs/design/05-networking-architecture.md</c>):</para>
    /// <list type="bullet">
    ///   <item><c>Application.isBatchMode</c> true (Linux headless server build)
    ///         -> <c>GameMode.Server</c> (dedicated, production canonical).</item>
    ///   <item><c>Application.isBatchMode</c> false (editor / standalone client)
    ///         -> <c>GameMode.Host</c> (Photon Cloud free 20 CCU, DEV ONLY).</item>
    /// </list>
    ///
    /// <para>Hard Rule #4 (CLAUDE.md / AGENTS.md):
    /// production builds MUST use Server Mode dedicated. CI workflow
    /// <c>.github/workflows/unity-build.yml</c> includes
    /// <c>-batchmode -nographics -server</c> for the Linux dedicated
    /// server target to enforce this at build time.</para>
    ///
    /// <para>POST-SDK-INSTALL implementation outline:</para>
    /// <list type="number">
    ///   <item>Add <c>using Fusion;</c></item>
    ///   <item>Add <c>NetworkRunner _runner;</c> field.</item>
    ///   <item>In <c>Start()</c>, instantiate the runner via
    ///         <c>gameObject.AddComponent&lt;NetworkRunner&gt;()</c>.</item>
    ///   <item>Read Photon App ID from <c>SecondSpawnConfig</c> singleton
    ///         (<c>Resources.Load&lt;SecondSpawn.Settings.SecondSpawnConfig&gt;("SecondSpawnConfig")</c>).</item>
    ///   <item>Choose mode from <c>Application.isBatchMode</c>.</item>
    ///   <item>Call <c>_runner.StartGame(new StartGameArgs { GameMode = mode, SessionName = "SecondSpawn-Zone-Default", PlayerCount = 20 })</c>.</item>
    ///   <item>Wire <c>INetworkInputProvider</c> from
    ///         <c>NetworkInputProvider</c>.</item>
    /// </list>
    ///
    /// <para>Reference samples (read locally, never copied into this repo
    /// per ADR 0006):</para>
    /// <list type="bullet">
    ///   <item>BR200 - <c>NetworkRunner</c> startup + dedicated mode.</item>
    ///   <item>Fusion Starter - simplest possible runner setup.</item>
    ///   <item>Tanknarok - top-down player spawn pattern, useful for
    ///         ARPG-style instance entry.</item>
    /// </list>
    /// </summary>
    public sealed class NetworkRunnerSetup : MonoBehaviour
    {
        // TODO (post-SDK-install): replace with real Fusion 2 wiring.
        // Currently a placeholder so the SecondSpawn.Networking assembly
        // compiles and other assemblies can reference it.
    }
}
