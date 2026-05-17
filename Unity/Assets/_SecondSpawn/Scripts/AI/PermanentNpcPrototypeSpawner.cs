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
        [SerializeField, Min(0.5f)] private float _spacing = 3f;
        [SerializeField, Min(0.5f)] private float _markerHeight = 1.8f;
        [SerializeField, Min(0.1f)] private float _markerRadius = 0.45f;
        [SerializeField, Min(0.5f)] private float _labelHeight = 2.2f;
        [SerializeField] private Key _refreshKey = Key.F6;
        [SerializeField] private bool _logStatus = true;

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
            var keyboard = Keyboard.current;
            if (keyboard != null && keyboard[_refreshKey].wasPressedThisFrame)
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
            var marker = GameObject.CreatePrimitive(PrimitiveType.Capsule);
            marker.name = BuildMarkerName(npc, index);
            marker.transform.SetParent(_root, false);
            marker.transform.localPosition = GridPosition(index);
            marker.transform.localScale = new Vector3(_markerRadius, _markerHeight * 0.5f, _markerRadius);

            var collider = marker.GetComponent<Collider>();
            if (collider != null)
            {
                collider.isTrigger = true;
            }

            var renderer = marker.GetComponent<Renderer>();
            if (renderer != null)
            {
                var material = CreateMarkerMaterial(npc, index);
                if (material != null)
                {
                    renderer.material = material;
                }
            }

            var markerData = marker.AddComponent<PermanentNpcPrototypeMarker>();
            markerData.Bind(npc);
            AddLabel(marker.transform, npc, index);
        }

        private Vector3 GridPosition(int index)
        {
            var column = index % Mathf.Max(1, _columns);
            var row = index / Mathf.Max(1, _columns);
            return _spawnOrigin + new Vector3(column * _spacing, _markerHeight * 0.5f, row * _spacing);
        }

        private void AddLabel(Transform marker, ActorProfileDto npc, int index)
        {
            var labelObject = new GameObject("Label");
            labelObject.transform.SetParent(marker, false);
            labelObject.transform.localPosition = new Vector3(0f, _labelHeight, 0f);
            labelObject.transform.localRotation = Quaternion.Euler(60f, 0f, 0f);

            var text = labelObject.AddComponent<TextMesh>();
            text.text = BuildLabel(npc, index);
            text.anchor = TextAnchor.MiddleCenter;
            text.alignment = TextAlignment.Center;
            text.fontSize = 42;
            text.characterSize = 0.12f;
            text.color = Color.white;
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

        private static string BuildLabel(ActorProfileDto npc, int index)
        {
            var displayName = string.IsNullOrWhiteSpace(npc?.display_name) ? $"Permanent NPC {index + 1:00}" : npc.display_name.Trim();
            var level = npc?.body?.stats != null ? Mathf.Max(1, npc.body.stats.level) : 1;
            var role = npc?.body?.identity?.public_role;
            if (string.IsNullOrWhiteSpace(role))
            {
                role = npc?.body?.archetype_id;
            }

            return string.IsNullOrWhiteSpace(role)
                ? $"{displayName}\nLv {level}"
                : $"{displayName}\nLv {level} | {role.Trim()}";
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
}
