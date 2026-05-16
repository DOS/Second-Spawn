using Fusion;
using UnityEngine;

namespace SecondSpawn.Networking
{
    /// <summary>
    /// Lightweight top-down camera follow for the controller prototype scene.
    /// It follows the local input-authority player in Host Mode and falls back
    /// to the first spawned player when running a server-only smoke test.
    /// </summary>
    [DisallowMultipleComponent]
    public sealed class TopDownCameraFollow : MonoBehaviour
    {
        [SerializeField, Tooltip("Offset from the followed player.")]
        private Vector3 _offset = new(0f, 12f, -9f);

        [SerializeField, Tooltip("Camera follow smoothing. Higher values catch up faster.")]
        private float _followSharpness = 12f;

        [SerializeField, Tooltip("Camera rotation for the prototype top-down view.")]
        private Vector3 _eulerAngles = new(60f, 0f, 0f);

        private Transform _target;

        private void LateUpdate()
        {
            if (_target == null)
            {
                _target = FindTarget();
                if (_target == null)
                {
                    return;
                }
            }

            var desiredPosition = _target.position + _offset;
            var sharpness = Mathf.Max(0.01f, _followSharpness);
            transform.position = Vector3.Lerp(transform.position, desiredPosition, 1f - Mathf.Exp(-sharpness * Time.deltaTime));
            transform.rotation = Quaternion.Euler(_eulerAngles);
        }

        private static Transform FindTarget()
        {
            var players = FindObjectsByType<NetworkPlayer>(FindObjectsInactive.Exclude);
            foreach (var player in players)
            {
                var networkObject = player.Object;
                if (networkObject != null && networkObject.HasInputAuthority)
                {
                    return player.transform;
                }
            }

            return players.Length > 0 ? players[0].transform : null;
        }
    }
}
