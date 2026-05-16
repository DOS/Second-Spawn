using UnityEngine;

namespace SecondSpawn.AI
{
    [DisallowMultipleComponent]
    public sealed class PrototypeSpeechBubble : MonoBehaviour
    {
        [SerializeField] private Vector3 _localOffset = new(0f, 2.2f, 0f);
        [SerializeField] private float _visibleSeconds = 3.5f;
        [SerializeField] private int _fontSize = 32;
        [SerializeField] private Color _textColor = Color.white;

        private TextMesh _textMesh;
        private float _hideAt;

        private void Awake()
        {
            EnsureTextMesh();
        }

        private void LateUpdate()
        {
            if (_textMesh == null)
            {
                return;
            }

            var meshTransform = _textMesh.transform;
            meshTransform.localPosition = _localOffset;

            var cam = Camera.main;
            if (cam != null)
            {
                meshTransform.rotation = Quaternion.LookRotation(meshTransform.position - cam.transform.position);
            }

            if (_textMesh.gameObject.activeSelf && Time.time >= _hideAt)
            {
                _textMesh.gameObject.SetActive(false);
            }
        }

        public void Show(string text)
        {
            EnsureTextMesh();
            if (_textMesh == null)
            {
                return;
            }

            _textMesh.text = Clamp(text);
            _textMesh.gameObject.SetActive(true);
            _hideAt = Time.time + _visibleSeconds;
        }

        private void EnsureTextMesh()
        {
            if (_textMesh != null)
            {
                return;
            }

            var child = new GameObject("PrototypeSpeechBubble");
            child.transform.SetParent(transform, false);
            child.transform.localPosition = _localOffset;

            _textMesh = child.AddComponent<TextMesh>();
            _textMesh.anchor = TextAnchor.MiddleCenter;
            _textMesh.alignment = TextAlignment.Center;
            _textMesh.fontSize = _fontSize;
            _textMesh.characterSize = 0.04f;
            _textMesh.color = _textColor;
            _textMesh.text = string.Empty;
            child.SetActive(false);
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
    }
}
