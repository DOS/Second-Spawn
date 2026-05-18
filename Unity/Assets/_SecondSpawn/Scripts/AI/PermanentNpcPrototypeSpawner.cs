using System;
using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;

namespace SecondSpawn.AI
{
    /// <summary>
    /// Play Mode bridge that makes server-owned permanent NPC Frames visible in
    /// the prototype hub before proper NPC prefabs and Fusion spawning exist.
    /// </summary>
    [DisallowMultipleComponent]
    [RequireComponent(typeof(SecondSpawnGatewayClient))]
    public sealed class PermanentNpcPrototypeSpawner : MonoBehaviour
    {
        [SerializeField] private bool _spawnOnStart = true;
        [SerializeField, Min(0.1f)] private float _authWaitSeconds = 8f;
        [SerializeField, Range(1, 50)] private int _maxNpcMarkers = 10;
        [SerializeField] private Vector3 _spawnOrigin = new Vector3(-8f, 0f, 7f);
        [SerializeField, Min(1)] private int _columns = 5;
        [SerializeField, Min(0.5f)] private float _spacing = 4f;
        [SerializeField, Min(0.5f)] private float _markerHeight = 1.8f;
        [SerializeField, Min(0.1f)] private float _markerRadius = 0.45f;
        [SerializeField, Min(0.5f)] private float _labelHeight = 2.65f;
        [SerializeField, Range(12, 24)] private int _labelFontSize = 18;
        [SerializeField, Range(8, 32)] private int _labelMaxLineLength = 20;
        [SerializeField, Min(96f)] private float _labelMaxWidth = 260f;
        [SerializeField, Min(1f)] private float _labelVisibleDistance = 24f;
        [SerializeField] private Key _refreshKey = Key.F6;
        [SerializeField] private bool _logStatus = true;
        [SerializeField] private string _zoneId = "prototype-hub";
        [SerializeField] private bool _attachAgentBrains = true;
        [SerializeField, Min(0.25f)] private float _agentDecisionIntervalSeconds = 30f;
        [SerializeField, Min(0f)] private float _agentStartStaggerSeconds = 5f;
        [SerializeField, Min(0.5f)] private float _agentPatrolRadius = 5f;
        [SerializeField] private bool _agentPhaseLogs = false;

        private const string RootName = "_PermanentNpcMarkers";

        private SecondSpawnGatewayClient _gateway;
        private Transform _root;
        private Coroutine _spawnRoutine;

        public int SpawnedCount { get; private set; }
        public string LastStatus { get; private set; } = "Not loaded";

        private void Awake()
        {
            _gateway = GetComponent<SecondSpawnGatewayClient>();
            EnsureRoot();
        }

        private void Start()
        {
            if (_spawnOnStart)
            {
                Refresh();
            }
        }

        private void Update()
        {
            if (PrototypeDebugInput.WasPressedThisFrame(_refreshKey, Key.F6))
            {
                Refresh();
            }
        }

        public void Refresh()
        {
            if (_spawnRoutine != null)
            {
                StopCoroutine(_spawnRoutine);
            }

            _spawnRoutine = StartCoroutine(SpawnPermanentNpcs());
        }

        private IEnumerator SpawnPermanentNpcs()
        {
            SpawnedCount = 0;
            LastStatus = "Waiting for Nakama auth";
            yield return WaitForNakamaSession();

            if (!_gateway.HasNakamaSession)
            {
                LastStatus = "No Nakama session. Permanent NPC markers were not spawned.";
                Debug.LogWarning($"[PermanentNpcPrototypeSpawner] {LastStatus}");
                _spawnRoutine = null;
                yield break;
            }

            LastStatus = "Loading permanent NPCs";
            NpcWorldListResponseDto response = null;
            string error = null;
            yield return _gateway.SeedPermanentNpcs(value => response = value, value => error = value);

            if (response == null || response.npcs == null)
            {
                LastStatus = $"Permanent NPC load failed: {error}";
                Debug.LogWarning($"[PermanentNpcPrototypeSpawner] {LastStatus}");
                _spawnRoutine = null;
                yield break;
            }

            ClearRoot();
            var count = Mathf.Min(_maxNpcMarkers, response.npcs.Length);
            for (var index = 0; index < count; index++)
            {
                SpawnNpcMarker(response.npcs[index], index);
            }

            SpawnedCount = count;
            LastStatus = $"Spawned {SpawnedCount} permanent NPC markers";
            if (_logStatus)
            {
                Debug.Log($"[PermanentNpcPrototypeSpawner] {LastStatus}.");
            }

            _spawnRoutine = null;
        }

        private IEnumerator WaitForNakamaSession()
        {
            var deadline = Time.realtimeSinceStartup + _authWaitSeconds;
            while (!_gateway.IsAuthReady && Time.realtimeSinceStartup < deadline)
            {
                yield return null;
            }

            if (_gateway.HasNakamaSession)
            {
                yield break;
            }

            string error = null;
            yield return _gateway.Authenticate(null, value => error = value);
            if (!string.IsNullOrWhiteSpace(error) && _logStatus)
            {
                Debug.LogWarning($"[PermanentNpcPrototypeSpawner] Nakama auth failed before NPC spawn: {error}");
            }
        }

        private void SpawnNpcMarker(ActorProfileDto npc, int index)
        {
            var marker = new GameObject(BuildMarkerName(npc, index));
            marker.name = BuildMarkerName(npc, index);
            marker.transform.SetParent(_root, false);
            marker.transform.localPosition = GridPosition(index);

            var collider = marker.AddComponent<CapsuleCollider>();
            collider.isTrigger = true;
            collider.radius = _markerRadius;
            collider.height = _markerHeight;
            collider.center = new Vector3(0f, _markerHeight * 0.5f, 0f);

            var markerData = marker.AddComponent<PermanentNpcPrototypeMarker>();
            markerData.Bind(npc);
            AddLabel(marker.transform, npc, index);

            if (_attachAgentBrains)
            {
                var brain = marker.AddComponent<PrototypeAgentBrain>();
                brain.ConfigureActorProfile(
                    npc,
                    _zoneId,
                    _agentPatrolRadius,
                    _agentDecisionIntervalSeconds,
                    index * _agentStartStaggerSeconds);
                brain.SetPhaseLogging(_agentPhaseLogs);
                return;
            }

            AddFallbackCapsule(marker.transform, npc, index);
        }

        private Vector3 GridPosition(int index)
        {
            var column = index % Mathf.Max(1, _columns);
            var row = index / Mathf.Max(1, _columns);
            return _spawnOrigin + new Vector3(column * _spacing, 0f, row * _spacing);
        }

        private void AddLabel(Transform marker, ActorProfileDto npc, int index)
        {
            var labelObject = new GameObject("Label");
            labelObject.transform.SetParent(marker, false);
            labelObject.transform.localPosition = new Vector3(0f, _labelHeight, 0f);

            var label = labelObject.AddComponent<PrototypeScreenSpaceLabel>();
            label.Configure(BuildLabel(npc, index), _labelVisibleDistance, _labelFontSize, _labelMaxWidth);
        }

        private void AddFallbackCapsule(Transform marker, ActorProfileDto npc, int index)
        {
            var capsule = GameObject.CreatePrimitive(PrimitiveType.Capsule);
            capsule.name = "FallbackCapsule";
            capsule.transform.SetParent(marker, false);
            capsule.transform.localPosition = new Vector3(0f, _markerHeight * 0.5f, 0f);
            capsule.transform.localScale = new Vector3(_markerRadius, _markerHeight * 0.5f, _markerRadius);

            var collider = capsule.GetComponent<Collider>();
            if (collider != null)
            {
                collider.enabled = false;
            }

            var renderer = capsule.GetComponent<Renderer>();
            if (renderer != null)
            {
                var material = CreateMarkerMaterial(npc, index);
                if (material != null)
                {
                    renderer.material = material;
                }
            }
        }

        private void EnsureRoot()
        {
            if (_root != null)
            {
                return;
            }

            var existing = GameObject.Find(RootName);
            _root = existing != null ? existing.transform : new GameObject(RootName).transform;
            _root.SetParent(null);
        }

        private void ClearRoot()
        {
            EnsureRoot();
            for (var index = _root.childCount - 1; index >= 0; index--)
            {
                Destroy(_root.GetChild(index).gameObject);
            }
        }

        private static Material CreateMarkerMaterial(ActorProfileDto npc, int index)
        {
            var shader = Shader.Find("Universal Render Pipeline/Lit");
            if (shader == null)
            {
                shader = Shader.Find("Standard");
            }

            if (shader == null)
            {
                Debug.LogWarning("[PermanentNpcPrototypeSpawner] No compatible marker shader found.");
                return null;
            }

            var material = new Material(shader)
            {
                color = ColorFor(npc, index)
            };
            return material;
        }

        private static Color ColorFor(ActorProfileDto npc, int index)
        {
            var archetype = npc?.body?.archetype_id ?? "";
            if (Contains(archetype, "sentinel"))
            {
                return new Color(0.18f, 0.72f, 0.95f);
            }

            if (Contains(archetype, "courier"))
            {
                return new Color(0.92f, 0.72f, 0.24f);
            }

            if (Contains(archetype, "clinic"))
            {
                return new Color(0.32f, 0.86f, 0.52f);
            }

            if (Contains(archetype, "scrap"))
            {
                return new Color(0.82f, 0.42f, 0.25f);
            }

            if (Contains(archetype, "hunter"))
            {
                return new Color(0.75f, 0.48f, 0.92f);
            }

            var hue = Mathf.Repeat(index * 0.137f, 1f);
            return Color.HSVToRGB(hue, 0.65f, 0.92f);
        }

        private static string BuildMarkerName(ActorProfileDto npc, int index)
        {
            var displayName = string.IsNullOrWhiteSpace(npc?.display_name) ? $"Permanent NPC {index + 1:00}" : npc.display_name.Trim();
            return $"NPC_{index + 1:00}_{SanitizeName(displayName)}";
        }

        private static bool Contains(string value, string needle)
        {
            return !string.IsNullOrWhiteSpace(value)
                && value.IndexOf(needle, StringComparison.OrdinalIgnoreCase) >= 0;
        }

        private string BuildLabel(ActorProfileDto npc, int index)
        {
            var displayName = string.IsNullOrWhiteSpace(npc?.display_name) ? $"Permanent NPC {index + 1:00}" : npc.display_name.Trim();
            var level = npc?.body?.stats != null ? Mathf.Max(1, npc.body.stats.level) : 1;
            var role = npc?.body?.identity?.public_role;
            if (string.IsNullOrWhiteSpace(role))
            {
                role = npc?.body?.archetype_id;
            }

            var title = $"#{index + 1:00} {Shorten(displayName, _labelMaxLineLength)}";
            return string.IsNullOrWhiteSpace(role)
                ? $"{title}\nLv {level}"
                : $"{title}\nLv {level} | {Shorten(role.Trim(), _labelMaxLineLength)}";
        }

        private static string Shorten(string value, int maxLength)
        {
            if (string.IsNullOrWhiteSpace(value))
            {
                return "";
            }

            var normalized = value.Trim();
            if (normalized.Length <= maxLength)
            {
                return normalized;
            }

            return maxLength <= 3 ? normalized.Substring(0, maxLength) : normalized.Substring(0, maxLength - 3) + "...";
        }

        private static string SanitizeName(string value)
        {
            if (string.IsNullOrWhiteSpace(value))
            {
                return "Permanent_NPC";
            }

            var chars = value.Trim().ToCharArray();
            for (var i = 0; i < chars.Length; i++)
            {
                var c = chars[i];
                if (!(char.IsLetterOrDigit(c) || c == '-' || c == '_'))
                {
                    chars[i] = '_';
                }
            }

            return new string(chars);
        }
    }

    /// <summary>
    /// Runtime-only marker component that keeps the loaded server actor profile
    /// attached to a visible prototype NPC object.
    /// </summary>
    [DisallowMultipleComponent]
    public sealed class PermanentNpcPrototypeMarker : MonoBehaviour
    {
        public ActorProfileDto Profile { get; private set; }
        public string ActorId => string.IsNullOrWhiteSpace(Profile?.actor_id) ? "" : Profile.actor_id.Trim();

        /// <summary>
        /// Stores the Nakama actor profile used to render this prototype marker.
        /// </summary>
        public void Bind(ActorProfileDto profile)
        {
            Profile = profile;
        }
    }

    [DisallowMultipleComponent]
    public sealed class PrototypeScreenSpaceLabel : MonoBehaviour
    {
        private const float ScreenPadding = 8f;
        private const float StatusRefreshSeconds = 0.25f;

        private static int s_cachedCameraFrame = -1;
        private static Camera s_cachedMainCamera;

        private string _text = "";
        private float _visibleDistance = 24f;
        private int _fontSize = 18;
        private float _maxWidth = 260f;
        private PrototypeAgentBrain _brain;
        private GUIContent _content;
        private GUIStyle _labelStyle;
        private GUIStyle _shadowStyle;
        private string _cachedDisplayText = "";
        private Color _cachedLabelColor = new Color(0.92f, 0.94f, 0.96f);
        private float _cachedHeight;
        private float _nextStatusRefreshAt;

        public void Configure(string text, float visibleDistance, int fontSize, float maxWidth)
        {
            _text = string.IsNullOrWhiteSpace(text) ? "NPC" : text.Trim();
            _visibleDistance = Mathf.Max(1f, visibleDistance);
            _fontSize = Mathf.Clamp(fontSize, 12, 24);
            _maxWidth = Mathf.Max(96f, maxWidth);
            _cachedDisplayText = _text;
            _cachedLabelColor = new Color(0.92f, 0.94f, 0.96f);
            _cachedHeight = _fontSize * 2.25f;
            _nextStatusRefreshAt = 0f;
            _content ??= new GUIContent();
            _content.text = _cachedDisplayText;
        }

        private void OnGUI()
        {
            if (Event.current.type != EventType.Repaint)
            {
                return;
            }

            var cam = GetMainCameraCached();
            if (cam == null || string.IsNullOrWhiteSpace(_text))
            {
                return;
            }

            var distance = Vector3.Distance(cam.transform.position, transform.position);
            if (distance > _visibleDistance)
            {
                return;
            }

            var screenPoint = cam.WorldToScreenPoint(transform.position);
            if (screenPoint.z <= 0f)
            {
                return;
            }

            if (screenPoint.x < -_maxWidth || screenPoint.x > Screen.width + _maxWidth ||
                screenPoint.y < -_maxWidth || screenPoint.y > Screen.height + _maxWidth)
            {
                return;
            }

            EnsureStyles();
            RefreshDisplayCache(force: false);
            var content = _content ??= new GUIContent(_cachedDisplayText);
            _labelStyle.normal.textColor = _cachedLabelColor;
            var height = _cachedHeight;
            var x = Mathf.Clamp(screenPoint.x - _maxWidth * 0.5f, ScreenPadding, Screen.width - _maxWidth - ScreenPadding);
            var y = Mathf.Clamp(Screen.height - screenPoint.y - height * 0.5f, ScreenPadding, Screen.height - height - ScreenPadding);
            var rect = new Rect(x, y, _maxWidth, height);

            GUI.Label(new Rect(rect.x + 1f, rect.y + 1f, rect.width, rect.height), content, _shadowStyle);
            GUI.Label(rect, content, _labelStyle);
        }

        private static Camera GetMainCameraCached()
        {
            if (s_cachedCameraFrame == Time.frameCount)
            {
                return s_cachedMainCamera;
            }

            s_cachedCameraFrame = Time.frameCount;
            s_cachedMainCamera = Camera.main;
            return s_cachedMainCamera;
        }

        private void RefreshBrainReference()
        {
            if (_brain != null)
            {
                return;
            }

            _brain = GetComponentInParent<PrototypeAgentBrain>();
        }

        private string BuildDisplayText()
        {
            if (_brain == null || string.IsNullOrWhiteSpace(_brain.BrainStatusLabel))
            {
                return _text;
            }

            var reason = string.IsNullOrWhiteSpace(_brain.BrainStatusReason)
                ? ""
                : $" {_brain.BrainStatusReason.Trim()}";
            return $"{_text}\n{_brain.BrainStatusLabel}{reason}";
        }

        private Color ResolveLabelColor()
        {
            return _brain == null ? new Color(0.92f, 0.94f, 0.96f) : _brain.BrainStatusColor;
        }

        private void RefreshDisplayCache(bool force)
        {
            if (!force && Time.unscaledTime < _nextStatusRefreshAt)
            {
                return;
            }

            _nextStatusRefreshAt = Time.unscaledTime + StatusRefreshSeconds;
            RefreshBrainReference();
            var nextText = BuildDisplayText();
            var nextColor = ResolveLabelColor();
            if (!force && nextText == _cachedDisplayText && nextColor == _cachedLabelColor)
            {
                return;
            }

            EnsureStyles();
            _cachedDisplayText = nextText;
            _cachedLabelColor = nextColor;
            _content ??= new GUIContent();
            _content.text = _cachedDisplayText;
            _cachedHeight = _labelStyle.CalcHeight(_content, _maxWidth);
        }

        private void EnsureStyles()
        {
            if (_labelStyle != null)
            {
                return;
            }

            _labelStyle = new GUIStyle(GUI.skin.label)
            {
                alignment = TextAnchor.MiddleCenter,
                fontSize = _fontSize,
                fontStyle = FontStyle.Bold,
                wordWrap = true,
                normal = { textColor = new Color(0.92f, 0.94f, 0.96f) }
            };

            _shadowStyle = new GUIStyle(_labelStyle)
            {
                normal = { textColor = new Color(0f, 0f, 0f, 0.72f) }
            };
        }
    }
}
