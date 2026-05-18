using UnityEngine;

namespace SecondSpawn.Settings
{
    /// <summary>
    /// Project-level runtime configuration for the Unity client.
    ///
    /// What lives here:
    /// - Public endpoints (Nakama server URL, Supabase URL)
    /// - Public client keys ONLY (Supabase anon key - designed public-safe)
    /// - Photon Fusion 2 App ID (semi-public - meant for client to know)
    /// - Per-environment toggles (dev / staging / prod)
    ///
    /// What does NOT live here (Hard Rule #3 in CLAUDE.md / AGENTS.md):
    /// - Anthropic / OpenAI / Convai API keys
    /// - Supabase service role key
    /// - thirdweb secret key
    /// - DOS Chain signing keys
    ///
    /// Secrets stay in server environments, never compiled into the Unity
    /// client. The client authenticates to Nakama; Nakama calls api.dos.ai
    /// when model-backed decisions are needed.
    ///
    /// One asset instance at Assets/Settings/SecondSpawnConfig.asset is
    /// loaded at startup via Resources or addressables.
    /// </summary>
    [CreateAssetMenu(
        fileName = "SecondSpawnConfig",
        menuName = "Second Spawn/Project Config",
        order = 1)]
    public sealed class SecondSpawnConfig : ScriptableObject
    {
        [Header("Environment")]
        [Tooltip("Affects logging, telemetry, and which endpoints are used.")]
        public BuildEnvironment Environment = BuildEnvironment.Development;

        [Header("Supabase (public-safe values only)")]
        [Tooltip("Supabase project URL, e.g. https://your-project.supabase.co")]
        public string SupabaseUrl = "";

        [Tooltip("Supabase anon key. Public-safe by design - DO NOT put service role key here.")]
        public string SupabaseAnonKey = "";

        [Header("Photon Fusion 2")]
        [Tooltip("Photon App ID from dashboard.photonengine.com (semi-public client identifier).")]
        public string PhotonAppId = "";

        [Header("DOS Chain")]
        [Tooltip("Public DOS Chain RPC endpoint. Signing keys NEVER live here.")]
        public string DosChainRpcUrl = "";

        public enum BuildEnvironment
        {
            Development,
            Staging,
            Production
        }
    }
}
