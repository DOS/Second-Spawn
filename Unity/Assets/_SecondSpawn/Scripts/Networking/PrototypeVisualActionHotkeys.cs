using Fusion;
using UnityEngine;
using UnityEngine.InputSystem;

namespace SecondSpawn.Networking
{
    /// <summary>
    /// Local prototype hotkeys for testing visual animation intents.
    /// This is a dev-only bridge and does not grant gameplay authority.
    /// </summary>
    [DisallowMultipleComponent]
    public sealed class PrototypeVisualActionHotkeys : MonoBehaviour
    {
        private VisualAnimationIntentDriver _driver;
        private NetworkObject _networkObject;

        private void Awake()
        {
            _networkObject = GetComponent<NetworkObject>();
        }

        private void Update()
        {
            if (_networkObject != null && !_networkObject.HasInputAuthority)
            {
                return;
            }

            ResolveDriver();
            if (_driver == null)
            {
                return;
            }

            var keyboard = Keyboard.current;
            if (keyboard == null)
            {
                return;
            }

            if (keyboard.eKey.wasPressedThisFrame) _driver.TryPlay(VisualAnimationIntent.Interact);
            if (keyboard.tKey.wasPressedThisFrame) _driver.TryPlay(VisualAnimationIntent.Talk);
            if (keyboard.yKey.wasPressedThisFrame) _driver.TryPlay(VisualAnimationIntent.Agree);
            if (keyboard.nKey.wasPressedThisFrame) _driver.TryPlay(VisualAnimationIntent.Disagree);
            if (keyboard.jKey.wasPressedThisFrame) _driver.TryPlay(VisualAnimationIntent.Attack);
            if (keyboard.cKey.wasPressedThisFrame) _driver.TryPlay(VisualAnimationIntent.Cast);
            if (keyboard.qKey.wasPressedThisFrame) _driver.TryPlay(VisualAnimationIntent.DodgeLeft);
            if (keyboard.rKey.wasPressedThisFrame) _driver.TryPlay(VisualAnimationIntent.DodgeRight);
            if (keyboard.kKey.wasPressedThisFrame) _driver.TryPlay(VisualAnimationIntent.Death);
            if (keyboard.lKey.wasPressedThisFrame) _driver.TryPlay(VisualAnimationIntent.Revive);
        }

        private void ResolveDriver()
        {
            if (_driver == null)
            {
                _driver = GetComponentInChildren<VisualAnimationIntentDriver>();
            }
        }
    }
}
