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

        [SerializeField, Min(1), Tooltip("Seconds before Nakama or Supabase HTTP requests fail fast in Play Mode.")]
        private int _requestTimeoutSeconds = 10;

        [SerializeField, Min(1), Tooltip("Seconds before the agent decision RPC fails. DOS.AI model decisions can take longer than normal Nakama calls.")]
        private int _agentDecisionRequestTimeoutSeconds = 135;

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

        public IEnumerator GetNakamaContext(Action<AgentContextDto> onSuccess, Action<string> onError = null)
        {
            yield return SendNakamaRpc("secondspawn_profile_get", new EmptyPayload(), onSuccess, onError);
        }

        public IEnumerator AddNakamaMemory(MemoryRecordDto memory, Action<AgentContextDto> onSuccess = null, Action<string> onError = null)
        {
            yield return SendNakamaRpc("secondspawn_memory_add", memory, onSuccess, onError);
        }

        public IEnumerator GetNakamaActorProfile(string actorId, Action<ActorProfileDto> onSuccess, Action<string> onError = null)
        {
            yield return GetNakamaActorProfile(new ActorProfileRequestDto { actor_id = actorId }, onSuccess, onError);
        }

        public IEnumerator GetNakamaActorProfile(ActorProfileRequestDto request, Action<ActorProfileDto> onSuccess, Action<string> onError = null)
        {
            yield return SendNakamaRpc("secondspawn_actor_profile_get", request, onSuccess, onError);
        }

        public IEnumerator AddNakamaActorMemory(ActorMemoryAddRequestDto memory, Action<ActorProfileDto> onSuccess = null, Action<string> onError = null)
        {
            yield return SendNakamaRpc("secondspawn_actor_memory_add", memory, onSuccess, onError);
        }

        public IEnumerator AddNakamaAgentActivity(AgentActivityRecordDto activity, Action<AgentContextDto> onSuccess = null, Action<string> onError = null)
        {
            yield return SendNakamaRpc("secondspawn_agent_activity_add", activity, onSuccess, onError);
        }

        public IEnumerator ApplyNakamaBodyTimeEvent(BodyTimeEventRequestDto request, Action<AgentContextDto> onSuccess = null, Action<string> onError = null)
        {
            yield return SendNakamaRpc("secondspawn_bodytime_event", request, onSuccess, onError);
        }

        public IEnumerator ReincarnateNakamaBody(ReincarnationRequestDto request, Action<AgentContextDto> onSuccess = null, Action<string> onError = null)
        {
            yield return SendNakamaRpc("secondspawn_reincarnate", request, onSuccess, onError);
        }

        public IEnumerator ClaimNakamaReward(RewardClaimRequestDto request, Action<AgentContextDto> onSuccess = null, Action<string> onError = null)
        {
            yield return SendNakamaRpc("secondspawn_reward_claim", request, onSuccess, onError);
        }

        public IEnumerator UpdateNakamaSoul(UpdateSoulRequestDto request, Action<AgentContextDto> onSuccess = null, Action<string> onError = null)
        {
            yield return SendNakamaRpc("secondspawn_soul_update", request, onSuccess, onError);
        }

        public IEnumerator BindOpenClawAgent(OpenClawBindRequestDto request, Action<OpenClawBindingDto> onSuccess, Action<string> onError = null)
        {
            yield return SendNakamaRpc("secondspawn_openclaw_bind", request, onSuccess, onError);
        }

        public IEnumerator GetOpenClawContext(OpenClawContextRequestDto request, Action<OpenClawContextResponseDto> onSuccess, Action<string> onError = null)
        {
            yield return SendNakamaRpc("secondspawn_openclaw_context_get", request, onSuccess, onError);
        }

        public IEnumerator SubmitOpenClawIntent(OpenClawIntentSubmitRequestDto request, Action<OpenClawIntentSubmitResponseDto> onSuccess, Action<string> onError = null)
        {
            yield return SendNakamaRpc("secondspawn_openclaw_intent_submit", request, onSuccess, onError);
        }

        public IEnumerator SendOpenClawHeartbeat(OpenClawHeartbeatRequestDto request, Action<OpenClawHeartbeatResponseDto> onSuccess, Action<string> onError = null)
        {
            yield return SendNakamaRpc("secondspawn_openclaw_heartbeat", request, onSuccess, onError);
        }

        public IEnumerator SendHubChatMessage(ChatSendRequestDto request, Action<ChatSendResponseDto> onSuccess, Action<string> onError = null)
        {
            yield return SendNakamaRpc("secondspawn_chat_send", request, onSuccess, onError);
        }

        public IEnumerator ListHubChatMessages(ChatListRequestDto request, Action<ChatListResponseDto> onSuccess, Action<string> onError = null)
        {
            yield return SendNakamaRpc("secondspawn_chat_list", request, onSuccess, onError);
        }

        public IEnumerator SeedPermanentNpcs(Action<NpcWorldListResponseDto> onSuccess, Action<string> onError = null)
        {
            yield return SendNakamaRpc("secondspawn_npc_seed", new EmptyPayload(), onSuccess, onError);
        }

        public IEnumerator ListPermanentNpcs(Action<NpcWorldListResponseDto> onSuccess, Action<string> onError = null)
        {
            yield return SendNakamaRpc("secondspawn_npc_list", new EmptyPayload(), onSuccess, onError);
        }

        public IEnumerator InteractPermanentNpcs(NpcInteractionRequestDto request, Action<NpcInteractionResponseDto> onSuccess, Action<string> onError = null)
        {
            yield return SendNakamaRpc("secondspawn_npc_interact", request, onSuccess, onError);
        }

        public IEnumerator GetPermanentNpcContext(NpcContextRequestDto request, Action<NpcContextResponseDto> onSuccess, Action<string> onError = null)
        {
            yield return SendNakamaRpc("secondspawn_npc_context_get", request, onSuccess, onError);
        }

        public IEnumerator SubmitPermanentNpcIntent(NpcIntentSubmitRequestDto request, Action<NpcIntentSubmitResponseDto> onSuccess, Action<string> onError = null)
        {
            yield return SendNakamaRpc("secondspawn_npc_intent_submit", request, onSuccess, onError);
        }

        public IEnumerator Decide(AgentDecisionRequestDto request, Action<AgentDecisionDto> onSuccess, Action<string> onError = null)
        {
            yield return SendNakamaRpc("secondspawn_agent_decide", request, onSuccess, onError, _agentDecisionRequestTimeoutSeconds);
        }

        public IEnumerator Chat(NpcChatRequestDto request, Action<NpcChatResponseDto> onSuccess, Action<string> onError = null)
        {
            request ??= new NpcChatRequestDto();
            if (string.IsNullOrWhiteSpace(request.player_id))
            {
                request.player_id = PlayerId;
            }

            ChatSendResponseDto response = null;
            string error = null;
            yield return SendHubChatMessage(new ChatSendRequestDto
            {
                channel_id = "prototype-hub",
                sender_display_name = request.player_id,
                message = string.IsNullOrWhiteSpace(request.npc_id)
                    ? request.message
                    : $"To {request.npc_id}: {request.message}",
                source = "prototype_npc_chat"
            }, value => response = value, value => error = value);

            if (response == null)
            {
                onError?.Invoke(error);
                yield break;
            }

            onSuccess?.Invoke(new NpcChatResponseDto
            {
                player_id = request.player_id,
                npc_id = string.IsNullOrWhiteSpace(request.npc_id) ? "prototype-hub" : request.npc_id,
                text = response.message == null ? request.message : response.message.text,
                voice_available = false,
                provider = "nakama_hub_chat"
            });
        }

        public IEnumerator GetVoiceSession(Action<VoiceSessionDto> onSuccess, Action<string> onError = null)
        {
            yield return null;
            onSuccess?.Invoke(new VoiceSessionDto
            {
                voice_available = false,
                provider = "not_configured",
                requires_ephemeral_token = true,
                reason = "Voice sessions require a future Nakama RPC that mints an api.dos.ai ephemeral token."
            });
        }

        private IEnumerator SendNakamaRpc<TResponse>(
            string rpcId,
            object payload,
            Action<TResponse> onSuccess,
            Action<string> onError,
            int timeoutSecondsOverride = 0)
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
            yield return Send(BuildNakamaRpcRequest(rpcId, json), onSuccess, error =>
            {
                if (IsNakamaAuthInvalid(error))
                {
                    ClearNakamaSession();
                    Debug.LogWarning($"[SecondSpawnGatewayClient] Nakama session rejected for RPC {rpcId}. Cleared stale session; the next required Nakama RPC will authenticate again.");
                }

                onError?.Invoke(error);
            }, timeoutSecondsOverride);
        }

        private UnityWebRequest BuildNakamaRpcRequest(string rpcId, string json)
        {
            var request = new UnityWebRequest(BuildNakamaUrl($"/v2/rpc/{UnityWebRequest.EscapeURL(rpcId)}?unwrap"), "POST")
            {
                uploadHandler = new UploadHandlerRaw(Encoding.UTF8.GetBytes(json)),
                downloadHandler = new DownloadHandlerBuffer()
            };
            request.SetRequestHeader("Authorization", "Bearer " + _nakamaAuthToken);
            request.SetRequestHeader("Content-Type", "application/json; charset=utf-8");
            request.SetRequestHeader("Accept", "application/json");
            return request;
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

        private IEnumerator Send<TResponse>(
            UnityWebRequest request,
            Action<TResponse> onSuccess,
            Action<string> onError,
            int timeoutSecondsOverride = 0)
        {
            request.timeout = Mathf.Max(1, timeoutSecondsOverride > 0 ? timeoutSecondsOverride : _requestTimeoutSeconds);
            yield return request.SendWebRequest();

            if (request.result != UnityWebRequest.Result.Success)
            {
                onError?.Invoke($"{request.responseCode}: {request.error} {request.downloadHandler.text}");
                yield break;
            }

            var body = request.downloadHandler.text;
            if (string.IsNullOrWhiteSpace(body))
            {
                onError?.Invoke("Server returned an empty response.");
                yield break;
            }

            try
            {
                onSuccess?.Invoke(JsonUtility.FromJson<TResponse>(body));
            }
            catch (Exception ex)
            {
                onError?.Invoke($"Server JSON parse failed: {ex.Message}");
            }
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

        private void ClearNakamaSession()
        {
            _nakamaAuthToken = "";
            _nakamaUserId = "";
        }

        private static bool IsNakamaAuthInvalid(string error)
        {
            if (string.IsNullOrWhiteSpace(error))
            {
                return false;
            }

            return error.Contains("401:", StringComparison.OrdinalIgnoreCase) ||
                   error.Contains("Auth token invalid", StringComparison.OrdinalIgnoreCase);
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
