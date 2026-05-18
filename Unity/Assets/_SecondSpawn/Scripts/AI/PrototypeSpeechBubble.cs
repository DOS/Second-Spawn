using UnityEngine;

namespace SecondSpawn.AI
{
    [DisallowMultipleComponent]
    public sealed class PrototypeSpeechBubble : MonoBehaviour
    {
        [SerializeField] private Vector3 _localOffset = new(0f, 2.45f, 0f);
        [SerializeField] private float _visibleSeconds = 4f;
        [SerializeField] private int _fontSize = 38;
        [SerializeField] private Color _textColor = Color.white;
        [SerializeField] private Color _bubbleColor = new(0.05f, 0.07f, 0.08f, 0.92f);
        [SerializeField] private Color _borderColor = new(0.72f, 0.9f, 1f, 0.72f);
        [SerializeField] private int _maxLineCharacters = 30;
        [SerializeField] private int _maxLines = 3;

        private GameObject _root;
        private Transform _bodyTransform;
        private Transform _borderTransform;
        private TextMesh _textMesh;
        private float _hideAt;

        private void Awake()
        {
            EnsureBubble();
        }

        private void LateUpdate()
        {
            if (_root == null)
            {
                return;
            }

            var rootTransform = _root.transform;
            rootTransform.localPosition = _localOffset;

            var cam = Camera.main;
            if (cam != null)
            {
                rootTransform.rotation = Quaternion.LookRotation(rootTransform.position - cam.transform.position);
            }

            if (_root.activeSelf && Time.time >= _hideAt)
            {
                _root.SetActive(false);
            }
        }

        public void Show(string text)
        {
            EnsureBubble();
            if (_textMesh == null)
            {
                return;
            }

            var wrapped = Wrap(Clamp(text), _maxLineCharacters, _maxLines);
            _textMesh.text = wrapped.text;
            ResizeBubble(wrapped.longestLineLength, wrapped.lineCount);
            _root.SetActive(true);
            _hideAt = Time.time + _visibleSeconds;
        }

        private void EnsureBubble()
        {
            if (_textMesh != null)
            {
                return;
            }

            _root = new GameObject("PrototypeSpeechBubble");
            _root.transform.SetParent(transform, false);
            _root.transform.localPosition = _localOffset;

            _borderTransform = CreateQuad("BubbleBorder", _borderColor, -0.02f).transform;
            _borderTransform.SetParent(_root.transform, false);

            _bodyTransform = CreateQuad("BubbleBody", _bubbleColor, -0.01f).transform;
            _bodyTransform.SetParent(_root.transform, false);

            var textObject = new GameObject("BubbleText");
            textObject.transform.SetParent(_root.transform, false);
            textObject.transform.localPosition = new Vector3(0f, 0f, -0.03f);

            _textMesh = textObject.AddComponent<TextMesh>();
            _textMesh.anchor = TextAnchor.MiddleCenter;
            _textMesh.alignment = TextAlignment.Center;
            _textMesh.fontSize = _fontSize;
            _textMesh.characterSize = 0.035f;
            _textMesh.color = _textColor;
            _textMesh.text = string.Empty;
            _root.SetActive(false);
        }

        private GameObject CreateQuad(string name, Color color, float localZ)
        {
            var quad = GameObject.CreatePrimitive(PrimitiveType.Quad);
            quad.name = name;
            quad.transform.localPosition = new Vector3(0f, 0f, localZ);

            var collider = quad.GetComponent<Collider>();
            if (collider != null)
            {
                Destroy(collider);
            }

            var renderer = quad.GetComponent<MeshRenderer>();
            renderer.sharedMaterial = CreateBubbleMaterial(name, color);
            return quad;
        }

        private static Material CreateBubbleMaterial(string name, Color color)
        {
            var shader = Shader.Find("Sprites/Default") ??
                Shader.Find("Universal Render Pipeline/Unlit") ??
                Shader.Find("Unlit/Color");
            var material = new Material(shader)
            {
                name = name + "Material",
                color = color
            };
            if (material.HasProperty("_Cull"))
            {
                material.SetInt("_Cull", 0);
            }
            material.renderQueue = 3000;
            return material;
        }

        private void ResizeBubble(int longestLineLength, int lineCount)
        {
            var width = Mathf.Clamp(longestLineLength * 0.075f + 0.48f, 1.25f, 3.25f);
            var height = Mathf.Clamp(lineCount * 0.27f + 0.28f, 0.55f, 1.35f);

            if (_bodyTransform != null)
            {
                _bodyTransform.localScale = new Vector3(width, height, 1f);
            }

            if (_borderTransform != null)
            {
                _borderTransform.localScale = new Vector3(width + 0.08f, height + 0.08f, 1f);
            }
        }

        private static string Clamp(string text)
        {
            if (string.IsNullOrWhiteSpace(text))
            {
                return "...";
            }

            text = text.Trim();
            return text.Length <= 96 ? text : text[..96] + "...";
        }

        private static WrappedText Wrap(string text, int maxLineCharacters, int maxLines)
        {
            maxLineCharacters = Mathf.Clamp(maxLineCharacters, 12, 48);
            maxLines = Mathf.Clamp(maxLines, 1, 5);

            var lines = new System.Collections.Generic.List<string>();
            AppendWrappedLines(lines, text, maxLineCharacters, maxLines);

            if (lines.Count == 0)
            {
                lines.Add("...");
            }

            TruncateLastLineIfNeeded(lines, text);

            return new WrappedText
            {
                text = string.Join("\n", lines),
                lineCount = lines.Count,
                longestLineLength = LongestLineLength(lines)
            };
        }

        private static void AppendWrappedLines(
            System.Collections.Generic.List<string> lines,
            string text,
            int maxLineCharacters,
            int maxLines)
        {
            var current = "";
            var words = text.Split(' ');
            foreach (var word in words)
            {
                if (string.IsNullOrWhiteSpace(word))
                {
                    continue;
                }

                var next = string.IsNullOrWhiteSpace(current) ? word : current + " " + word;
                if (next.Length <= maxLineCharacters)
                {
                    current = next;
                    continue;
                }

                if (!string.IsNullOrWhiteSpace(current))
                {
                    lines.Add(current);
                }

                current = word;
                if (lines.Count >= maxLines)
                {
                    break;
                }
            }

            if (lines.Count < maxLines && !string.IsNullOrWhiteSpace(current))
            {
                lines.Add(current);
            }
        }

        private static void TruncateLastLineIfNeeded(
            System.Collections.Generic.List<string> lines,
            string originalText)
        {
            var lastIndex = lines.Count - 1;
            if (lastIndex < 0 || string.Join(" ", lines).Length >= originalText.Length)
            {
                return;
            }

            if (lines[lastIndex].EndsWith("..."))
            {
                return;
            }

            lines[lastIndex] = lines[lastIndex].Length <= 3
                ? "..."
                : lines[lastIndex][..Mathf.Max(0, lines[lastIndex].Length - 3)] + "...";
        }

        private static int LongestLineLength(System.Collections.Generic.List<string> lines)
        {
            var longest = 0;
            for (var index = 0; index < lines.Count; index++)
            {
                longest = Mathf.Max(longest, lines[index].Length);
            }

            return longest;
        }

        private struct WrappedText
        {
            public string text;
            public int lineCount;
            public int longestLineLength;
        }
    }
}
