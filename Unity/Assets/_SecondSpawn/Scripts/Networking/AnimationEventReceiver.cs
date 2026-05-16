using UnityEngine;

namespace SecondSpawn.Networking
{
    /// <summary>
    /// Receives optional animation events from local-only character visuals.
    /// Footstep audio/VFX can be wired here later without allowing animation
    /// clips to affect authoritative gameplay state.
    /// </summary>
    [DisallowMultipleComponent]
    public sealed class AnimationEventReceiver : MonoBehaviour
    {
        public void Hit()
        {
        }

        public void Shoot()
        {
        }

        public void FootL()
        {
        }

        public void FootR()
        {
        }

        public void Land()
        {
        }
    }
}
