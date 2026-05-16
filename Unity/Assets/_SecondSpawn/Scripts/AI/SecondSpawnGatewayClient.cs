using System;
using System.Collections;
using System.Text;
using UnityEngine;
using UnityEngine.Networking;

namespace SecondSpawn.AI
{
    [DisallowMultipleComponent]
    public sealed class SecondSpawnGatewayClient : MonoBehaviour
    {
        [SerializeField, Tooltip("Public gateway base URL. No provider API keys are stored in Unity.")]
        private string _gatewayBaseUrl = "https://second-spawn-gateway-535583621422.asia-southeast1.run.app";

        [SerializeField, Tooltip("Prototype player id until Supabase auth is wired.")]
        private string _playerId = "dev-player";

        [Header("Nakama Auth")]
        [SerializeField, Tooltip("Try Supabase anonymous auth and Nakama custom auth on scene start.")]
        private bool _authenticateOnStart = true;

        [SerializeField, Tooltip("Supabase project URL. May also come from SECOND_SPAWN_SUPABASE_URL.")]
        private string _supabaseUrl = "";

        [SerializeField, Tooltip("Supabase anon or publishable key. May also come from SECOND_SPAWN_SUPABASE_ANON_KEY.")]
        private string _supabaseAnonKey = "";

        [SerializeField, Tooltip("Nakama HTTP base URL.")]
        private string _nakamaBaseUrl = "http://127.0.0.1:7350";

        [SerializeField, Tooltip("Nakama client/server key. Local dev default is defaultkey; rotate for non-local envs.")]
        private string _nakamaServerKey = "defaultkey";

        [SerializeField, Tooltip("Use Nakama device auth when Supabase is not configured yet. Local prototype only.")]
        private bool _allowNakamaDeviceFallback = true;

        [SerializeField, Tooltip("Create or refresh the Nakama character profile immediately after authentication.")]
        private bool _bootstrapProfileAfterAuth = true;

        [SerializeField, Min(1), Tooltip("Seconds before gateway or Nakama HTTP requests fail fast in Play Mode.")]
        private int _requestTimeoutSeconds = 10;

        private bool _authAttempted;
        private bool _authInProgress;
        private string _supabaseAccessToken;
        private string _nakamaAuthToken;
        private string _nakamaUserId;

        public bool HasNakamaSession => !string.IsNullOrWhiteSpace(_nakamaAuthToken);
        public bool IsAuthReady => !_authInProgress && (_authAttempted || HasNakamaSession);
        public string NakamaAuthToken => _nakamaAuthToken;
        public string NakamaUserId => string.IsNullOrWhiteSpace(_nakamaUserId) ? "" : _nakamaUserId.Trim();
        public string PlayerId => !string.IsNullOrWhiteSpace(NakamaUserId)
            ? NakamaUserId
            : (string.IsNullOrWhiteSpace(_playerId) ? "dev-player" : _playerId.Trim());

        private IEnumerator Start()
        {
            if (_authenticateOnStart)
            {
                yield return Authenticate();
            }
        }

        public IEnumerator Authenticate(Action onSuccess = null, Action<string> onError = null)
        {
            if (_authInProgress)
            {
                while (_authInProgress)
                {
                    yield return null;
                }

                if (HasNakamaSession)
                {
                    onSuccess?.Invoke();
                }
                else
                {
                    onError?.Invoke("Authentication already attempted but no Nakama session is available.");
                }
                yield break;
            }

            _authAttempted = true;
            _authInProgress = true;

            var supabaseUrl = ResolveValue(_supabaseUrl, "SECOND_SPAWN_SUPABASE_URL");
            var supabaseKey = ResolveValue(_supabaseAnonKey, "SECOND_SPAWN_SUPABASE_ANON_KEY", "SECOND_SPAWN_SUPABASE_PUBLISHABLE_KEY");
            if (string.IsNullOrWhiteSpace(supabaseUrl) || string.IsNullOrWhiteSpace(supabaseKey))
            {
                var message = "Supabase URL/key not configured. Falling back to Nakama device auth for local prototype.";
                Debug.Log($"[SecondSpawnGatewayClient] {message}");
                yield return AuthenticateNakamaDeviceFallback(onSuccess, onError);
                yield break;
            }

            SupabaseAnonymousSessionDto supabaseSession = null;
            yield return SignInSupabaseAnonymously(supabaseUrl, supabaseKey, session => supabaseSession = session, error =>
            {
                Debug.LogWarning($"[SecondSpawnGatewayClient] Supabase anonymous auth failed: {error}");
                onError?.Invoke(error);
            });

            if (supabaseSession == null || string.IsNullOrWhiteSpace(supabaseSession.access_token))
            {
                yield return AuthenticateNakamaDeviceFallback(onSuccess, onError);
                yield break;
            }

            _supabaseAccessToken = supabaseSession.access_token;

            NakamaSessionDto nakamaSession = null;
            yield return AuthenticateNakamaCustom(_supabaseAccessToken, session => nakamaSession = session, error =>
            {
                Debug.LogWarning($"[SecondSpawnGatewayClient] Nakama custom auth failed: {error}");
                onError?.Invoke(error);
            });

            if (nakamaSession == null || string.IsNullOrWhiteSpace(nakamaSession.token))
            {
                yield return AuthenticateNakamaDeviceFallback(onSuccess, onError);
                yield break;
            }

            _nakamaAuthToken = nakamaSession.token;
            _nakamaUserId = ExtractJwtStringClaim(_nakamaAuthToken, "uid");
            if (string.IsNullOrWhiteSpace(_nakamaUserId))
            {
                _nakamaUserId = _playerId;
            }

            _authInProgress = false;
            Debug.Log($"[SecondSpawnGatewayClient] Authenticated Nakama user {PlayerId}.");
            yield return BootstrapNakamaProfileAfterAuth("custom_auth");
            onSuccess?.Invoke();
        }

        public IEnumerator GetContext(Action<AgentContextDto> onSuccess, Action<string> onError = null)
        {
            yield return GetContextForPlayer(PlayerId, onSuccess, onError);
        }

        public IEnumerator GetNakamaContext(Action<AgentContextDto> onSuccess, Action<string> onError = null)
        {
            yield return SendNakamaRpc("secondspawn_profile_get", new EmptyPayload(), onSuccess, onError);
        }

        public IEnumerator AddNakamaMemory(MemoryRecordDto memory, Action<AgentContextDto> onSuccess = null, Action<string> onError = null)
        {
            yield return SendNakamaRpc("secondspawn_memory_add", memory, onSuccess, onError);
        }

        public IEnumerator AddNakamaAgentActivity(AgentActivityRecordDto activity, Action<AgentContextDto> onSuccess = null, Action<string> onError = null)
        {
            yield return SendNakamaRpc("secondspawn_agent_activity_add", activity, onSuccess, onError);
        }

        public IEnumerator UpdateNakamaSoul(UpdateSoulRequestDto request, Action<AgentContextDto> onSuccess = null, Action<string> onError = null)
        {
            yield return SendNakamaRpc("secondspawn_soul_update", request, onSuccess, onError);
        }

        public IEnumerator DecideWithNakamaFallback(AgentDecisionRequestDto request, Action<AgentDecisionDto> onSuccess, Action<string> onError = null)
        {
            yield return SendNakamaRpc("secondspawn_agent_decide", request, onSuccess, onError);
        }

        public IEnumerator GetContextForPlayer(string playerId, Action<AgentContextDto> onSuccess, Action<string> onError = null)
        {
            yield return Send<AgentContextDto>(
                UnityWebRequest.Get(BuildUrl($"/v1/characters/{UnityWebRequest.EscapeURL(NormalizePlayerId(playerId))}/context")),
                onSuccess,
                onError);
        }

        public IEnumerator AddMemory(MemoryRecordDto memory, Action<AgentContextDto> onSuccess = null, Action<string> onError = null)
        {
            yield return AddMemoryForPlayer(PlayerId, memory, onSuccess, onError);
        }

        public IEnumerator AddMemoryForPlayer(string playerId, MemoryRecordDto memory, Action<AgentContextDto> onSuccess = null, Action<string> onError = null)
        {
            yield return SendJson(
                "POST",
                $"/v1/characters/{UnityWebRequest.EscapeURL(NormalizePlayerId(playerId))}/memory",
                memory,
                onSuccess,
                onError);
        }

        public IEnumerator UpdateSoul(UpdateSoulRequestDto request, Action<AgentContextDto> onSuccess = null, Action<string> onError = null)
        {
            yield return UpdateSoulForPlayer(PlayerId, request, onSuccess, onError);
        }

        public IEnumerator UpdateSoulForPlayer(string playerId, UpdateSoulRequestDto request, Action<AgentContextDto> onSuccess = null, Action<string> onError = null)
        {
            yield return SendJson(
                "PUT",
                $"/v1/characters/{UnityWebRequest.EscapeURL(NormalizePlayerId(playerId))}/soul",
                request,
                onSuccess,
                onError);
        }

        public IEnumerator Decide(AgentDecisionRequestDto request, Action<AgentDecisionDto> onSuccess, Action<string> onError = null)
        {
            AgentDecisionDto decision = null;
            string gatewayError = null;
            yield return SendJson<AgentDecisionDto>(
                "POST",
                "/v1/agent/decide",
                GatewayAgentDecisionRequestDto.From(request),
                response => decision = response,
                error => gatewayError = error);

            if (decision == null)
            {
                if (!string.IsNullOrWhiteSpace(gatewayError))
                {
                    onError?.Invoke(gatewayError);
                }
                yield break;
            }

            onSuccess?.Invoke(decision);
            if (HasNakamaSession)
            {
                yield return AddNakamaAgentActivity(BuildGatewayDecisionActivity(decision), null, error =>
                {
                    Debug.LogWarning($"[SecondSpawnGatewayClient] Gateway decision activity write failed: {error}");
                });
            }
        }

        public IEnumerator Chat(NpcChatRequestDto request, Action<NpcChatResponseDto> onSuccess, Action<string> onError = null)
        {
            if (string.IsNullOrWhiteSpace(request.player_id))
            {
                request.player_id = PlayerId;
            }

            yield return SendJson("POST", "/v1/npc/chat", request, onSuccess, onError);
        }

        public IEnumerator GetVoiceSession(Action<VoiceSessionDto> onSuccess, Action<string> onError = null)
        {
            yield return SendJson("POST", "/v1/voice/session", new VoiceSessionRequest(), onSuccess, onError);
        }

        private IEnumerator SendJson<TResponse>(string method, string path, object payload, Action<TResponse> onSuccess, Action<string> onError)
        {
            var json = JsonUtility.ToJson(payload);
            var request = new UnityWebRequest(BuildUrl(path), method)
            {
                uploadHandler = new UploadHandlerRaw(Encoding.UTF8.GetBytes(json)),
                downloadHandler = new DownloadHandlerBuffer()
            };
            request.SetRequestHeader("Content-Type", "application/json; charset=utf-8");
            request.SetRequestHeader("Accept", "application/json");
            yield return Send(request, onSuccess, onError);
        }

        private IEnumerator SendNakamaRpc<TResponse>(string rpcId, object payload, Action<TResponse> onSuccess, Action<string> onError)
        {
            if (!HasNakamaSession)
            {
                yield return Authenticate(null, onError);
            }

            if (!HasNakamaSession)
            {
                onError?.Invoke("Nakama session is unavailable.");
                yield break;
            }

            var json = JsonUtility.ToJson(payload);
            var request = new UnityWebRequest(BuildNakamaUrl($"/v2/rpc/{UnityWebRequest.EscapeURL(rpcId)}?unwrap"), "POST")
            {
                uploadHandler = new UploadHandlerRaw(Encoding.UTF8.GetBytes(json)),
                downloadHandler = new DownloadHandlerBuffer()
            };
            request.SetRequestHeader("Authorization", "Bearer " + _nakamaAuthToken);
            request.SetRequestHeader("Content-Type", "application/json; charset=utf-8");
            request.SetRequestHeader("Accept", "application/json");
            yield return Send(request, onSuccess, onError);
        }

        private IEnumerator SignInSupabaseAnonymously(string supabaseUrl, string supabaseKey, Action<SupabaseAnonymousSessionDto> onSuccess, Action<string> onError)
        {
            var request = new UnityWebRequest(TrimTrailingSlash(supabaseUrl) + "/auth/v1/signup", "POST")
            {
                uploadHandler = new UploadHandlerRaw(Encoding.UTF8.GetBytes("{}")),
                downloadHandler = new DownloadHandlerBuffer()
            };
            request.SetRequestHeader("apikey", supabaseKey);
            request.SetRequestHeader("Authorization", "Bearer " + supabaseKey);
            request.SetRequestHeader("Content-Type", "application/json; charset=utf-8");
            request.SetRequestHeader("Accept", "application/json");
            yield return Send(request, onSuccess, onError);
        }

        private IEnumerator AuthenticateNakamaCustom(string supabaseAccessToken, Action<NakamaSessionDto> onSuccess, Action<string> onError)
        {
            var payload = new NakamaCustomAuthRequest { id = supabaseAccessToken };
            var request = new UnityWebRequest(BuildNakamaUrl("/v2/account/authenticate/custom?create=true"), "POST")
            {
                uploadHandler = new UploadHandlerRaw(Encoding.UTF8.GetBytes(JsonUtility.ToJson(payload))),
                downloadHandler = new DownloadHandlerBuffer()
            };
            var basic = Convert.ToBase64String(Encoding.UTF8.GetBytes(ResolveValue(_nakamaServerKey, "SECOND_SPAWN_NAKAMA_SERVER_KEY") + ":"));
            request.SetRequestHeader("Authorization", "Basic " + basic);
            request.SetRequestHeader("Content-Type", "application/json; charset=utf-8");
            request.SetRequestHeader("Accept", "application/json");
            yield return Send(request, onSuccess, onError);
        }

        private IEnumerator AuthenticateNakamaDeviceFallback(Action onSuccess, Action<string> onError)
        {
            if (!_allowNakamaDeviceFallback)
            {
                _authInProgress = false;
                onError?.Invoke("Nakama device fallback is disabled.");
                yield break;
            }

            NakamaSessionDto nakamaSession = null;
            var deviceId = "second-spawn-" + SystemInfo.deviceUniqueIdentifier;
            var username = "local-" + StableSmallHash(deviceId);
            yield return AuthenticateNakamaDevice(deviceId, username, session => nakamaSession = session, error =>
            {
                Debug.LogWarning($"[SecondSpawnGatewayClient] Nakama device fallback failed: {error}");
                onError?.Invoke(error);
            });

            if (nakamaSession == null || string.IsNullOrWhiteSpace(nakamaSession.token))
            {
                _authInProgress = false;
                yield break;
            }

            _nakamaAuthToken = nakamaSession.token;
            _nakamaUserId = ExtractJwtStringClaim(_nakamaAuthToken, "uid");
            if (string.IsNullOrWhiteSpace(_nakamaUserId))
            {
                _nakamaUserId = username;
            }

            _authInProgress = false;
            Debug.Log($"[SecondSpawnGatewayClient] Authenticated Nakama device fallback user {PlayerId}.");
            yield return BootstrapNakamaProfileAfterAuth("device_auth");
            onSuccess?.Invoke();
        }

        private IEnumerator BootstrapNakamaProfileAfterAuth(string authSource)
        {
            if (!_bootstrapProfileAfterAuth || !HasNakamaSession)
            {
                yield break;
            }

            yield return AddNakamaAgentActivity(new AgentActivityRecordDto
            {
                kind = "profile_bootstrap",
                summary = $"Unity client authenticated through {authSource} and confirmed the Nakama character profile.",
                source = "unity"
            }, null, error =>
            {
                Debug.LogWarning($"[SecondSpawnGatewayClient] Nakama profile activity write failed: {error}");
            });
        }

        private IEnumerator AuthenticateNakamaDevice(string deviceId, string username, Action<NakamaSessionDto> onSuccess, Action<string> onError)
        {
            var payload = new NakamaDeviceAuthRequest { id = deviceId };
            var request = new UnityWebRequest(BuildNakamaUrl($"/v2/account/authenticate/device?create=true&username={UnityWebRequest.EscapeURL(username)}"), "POST")
            {
                uploadHandler = new UploadHandlerRaw(Encoding.UTF8.GetBytes(JsonUtility.ToJson(payload))),
                downloadHandler = new DownloadHandlerBuffer()
            };
            var basic = Convert.ToBase64String(Encoding.UTF8.GetBytes(ResolveValue(_nakamaServerKey, "SECOND_SPAWN_NAKAMA_SERVER_KEY") + ":"));
            request.SetRequestHeader("Authorization", "Basic " + basic);
            request.SetRequestHeader("Content-Type", "application/json; charset=utf-8");
            request.SetRequestHeader("Accept", "application/json");
            yield return Send(request, onSuccess, onError);
        }

        private IEnumerator Send<TResponse>(UnityWebRequest request, Action<TResponse> onSuccess, Action<string> onError)
        {
            request.timeout = Mathf.Max(1, _requestTimeoutSeconds);
            yield return request.SendWebRequest();

            if (request.result != UnityWebRequest.Result.Success)
            {
                onError?.Invoke($"{request.responseCode}: {request.error} {request.downloadHandler.text}");
                yield break;
            }

            var body = request.downloadHandler.text;
            if (string.IsNullOrWhiteSpace(body))
            {
                onError?.Invoke("Gateway returned an empty response.");
                yield break;
            }

            try
            {
                onSuccess?.Invoke(JsonUtility.FromJson<TResponse>(body));
            }
            catch (Exception ex)
            {
                onError?.Invoke($"Gateway JSON parse failed: {ex.Message}");
            }
        }

        private string BuildUrl(string path)
        {
            var baseUrl = string.IsNullOrWhiteSpace(_gatewayBaseUrl)
                ? "https://second-spawn-gateway-535583621422.asia-southeast1.run.app"
                : _gatewayBaseUrl.TrimEnd('/');
            return baseUrl + path;
        }

        private string BuildNakamaUrl(string path)
        {
            var baseUrl = ResolveValue(_nakamaBaseUrl, "SECOND_SPAWN_NAKAMA_URL");
            if (string.IsNullOrWhiteSpace(baseUrl))
            {
                baseUrl = "http://127.0.0.1:7350";
            }
            return baseUrl.TrimEnd('/') + path;
        }

        private static string NormalizePlayerId(string playerId)
        {
            return string.IsNullOrWhiteSpace(playerId) ? "dev-player" : playerId.Trim();
        }

        private static string ResolveValue(string serializedValue, params string[] envNames)
        {
            if (!string.IsNullOrWhiteSpace(serializedValue))
            {
                return serializedValue.Trim();
            }

            foreach (var envName in envNames)
            {
                var value = Environment.GetEnvironmentVariable(envName);
                if (!string.IsNullOrWhiteSpace(value))
                {
                    return value.Trim();
                }
            }

            return "";
        }

        private static string TrimTrailingSlash(string value)
        {
            return string.IsNullOrWhiteSpace(value) ? "" : value.Trim().TrimEnd('/');
        }

        private static AgentActivityRecordDto BuildGatewayDecisionActivity(AgentDecisionDto decision)
        {
            var action = NormalizeDecisionAction(decision?.action);
            var reason = string.IsNullOrWhiteSpace(decision?.reason) ? "no reason provided" : decision.reason.Trim();
            return new AgentActivityRecordDto
            {
                kind = "agent_decision",
                summary = $"Gateway chose {action}: {reason}",
                source = "unity_gateway",
                metrics = BuildGatewayDecisionMetrics(decision)
            };
        }

        private static AgentActivityMetricsDto BuildGatewayDecisionMetrics(AgentDecisionDto decision)
        {
            var action = NormalizeDecisionAction(decision?.action);
            return new AgentActivityMetricsDto
            {
                decisions_made = 1,
                fallback_decisions = IsFallbackDecision(decision) ? 1 : 0,
                move_intents = action == "move" ? 1 : 0,
                say_intents = action == "say" ? 1 : 0,
                stop_intents = action == "stop" ? 1 : 0,
                interact_intents = action == "interact" ? 1 : 0
            };
        }

        private static bool IsFallbackDecision(AgentDecisionDto decision)
        {
            return string.Equals(decision?.source?.Trim(), "fallback", StringComparison.OrdinalIgnoreCase);
        }

        private static string NormalizeDecisionAction(string action)
        {
            return string.IsNullOrWhiteSpace(action) ? "unknown" : action.Trim().ToLowerInvariant();
        }

        [Serializable]
        private sealed class GatewayAgentDecisionRequestDto
        {
            public GatewayAgentContextDto context;
            public WorldSnapshotDto world_snapshot;
            public string[] allowed;

            public static GatewayAgentDecisionRequestDto From(AgentDecisionRequestDto request)
            {
                return new GatewayAgentDecisionRequestDto
                {
                    context = GatewayAgentContextDto.From(request?.context),
                    world_snapshot = request?.world_snapshot,
                    allowed = request?.allowed
                };
            }
        }

        [Serializable]
        private sealed class GatewayAgentContextDto
        {
            public PlayerProfileDto player;
            public GatewayBodyProfileDto body;

            public static GatewayAgentContextDto From(AgentContextDto context)
            {
                return new GatewayAgentContextDto
                {
                    player = context?.player,
                    body = GatewayBodyProfileDto.From(context?.body)
                };
            }
        }

        [Serializable]
        private sealed class GatewayBodyProfileDto
        {
            public string body_id;
            public string archetype_id;
            public string visual_prefab_key;
            public EquipmentLoadoutDto equipment;
            public CharacterStatsDto stats;
            public CharacterTraitsDto characteristics;
            public BodyTimeDto time;
            public CultivationDto cultivation;
            public AgentPolicyDto agent_policy;
            public SoulProfileDto soul;
            public MemoryRecordDto[] memory;

            public static GatewayBodyProfileDto From(BodyProfileDto body)
            {
                if (body == null)
                {
                    return null;
                }

                return new GatewayBodyProfileDto
                {
                    body_id = body.body_id,
                    archetype_id = body.archetype_id,
                    visual_prefab_key = body.visual_prefab_key,
                    equipment = body.equipment,
                    stats = body.stats,
                    characteristics = body.characteristics,
                    time = body.time,
                    cultivation = body.cultivation,
                    agent_policy = body.agent_policy,
                    soul = body.soul,
                    memory = body.memory
                };
            }
        }

        private static string ExtractJwtStringClaim(string jwt, string claimName)
        {
            if (string.IsNullOrWhiteSpace(jwt))
            {
                return "";
            }

            var parts = jwt.Split('.');
            if (parts.Length < 2)
            {
                return "";
            }

            try
            {
                var payload = parts[1].Replace('-', '+').Replace('_', '/');
                switch (payload.Length % 4)
                {
                    case 2:
                        payload += "==";
                        break;
                    case 3:
                        payload += "=";
                        break;
                }

                var json = Encoding.UTF8.GetString(Convert.FromBase64String(payload));
                var claims = JsonUtility.FromJson<NakamaJwtClaimsDto>(json);
                return claimName switch
                {
                    "uid" => claims?.uid ?? "",
                    _ => ""
                };
            }
            catch (Exception)
            {
                return "";
            }
        }

        private static string StableSmallHash(string value)
        {
            unchecked
            {
                uint hash = 2166136261;
                for (var i = 0; i < value.Length; i++)
                {
                    hash ^= value[i];
                    hash *= 16777619;
                }
                return hash.ToString("x8");
            }
        }

        [Serializable]
        private sealed class VoiceSessionRequest
        {
        }

        [Serializable]
        private sealed class EmptyPayload
        {
        }

        [Serializable]
        private sealed class SupabaseAnonymousSessionDto
        {
            public string access_token;
            public SupabaseUserDto user;
        }

        [Serializable]
        private sealed class SupabaseUserDto
        {
            public string id;
        }

        [Serializable]
        private sealed class NakamaCustomAuthRequest
        {
            public string id;
        }

        [Serializable]
        private sealed class NakamaDeviceAuthRequest
        {
            public string id;
        }

        [Serializable]
        private sealed class NakamaSessionDto
        {
            public string token;
            public string refresh_token;
        }

        [Serializable]
        private sealed class NakamaJwtClaimsDto
        {
            public string uid;
        }
    }
}
