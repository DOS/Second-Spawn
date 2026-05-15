using Fusion;
using Fusion.Addons.SimpleKCC;
using UnityEngine;

namespace SecondSpawn.Networking
{
    /// <summary>
    /// Authoritative networked player state.
    /// Spawned by the dedicated server on player join, despawned on disconnect.
    ///
    /// <para>Server-authoritative: client predicts visually but the server
    /// owns every <c>[Networked]</c> property. Per Pillar 4 (Server-authoritative
    /// gameplay) and Hard Rule #2 (LLM never mutates state directly).</para>
    ///
    /// <para>Vertical slice scope: position + rotation + cultivation tier +
    /// HP + agent flag. Inventory, quest progress, NFT lock state are
    /// persisted in Supabase (durable layer), not held in
    /// <c>[Networked]</c> properties (session layer).</para>
    /// </summary>
    [DisallowMultipleComponent]
    [RequireComponent(typeof(SimpleKCC))]
    public sealed class NetworkPlayer : NetworkBehaviour
    {
        [Networked] public int CultivationTier { get; set; }
        [Networked] public float Hp { get; set; }
        [Networked] public float Stamina { get; set; }

        /// <summary>True when the offline AI agent is driving this character (Pillar 1).</summary>
        [Networked] public NetworkBool IsAgentControlled { get; set; }

        [SerializeField, Tooltip("Movement speed in units/second. Simple KCC owns authoritative movement for this spike.")]
        private float _moveSpeed = 5f;

        private SimpleKCC _kcc;

        private void Awake()
        {
            _kcc = GetComponent<SimpleKCC>();
        }

        public override void Spawned()
        {
            _kcc ??= GetComponent<SimpleKCC>();

            if (HasStateAuthority)
            {
                CultivationTier = 1; // Awakening - starting tier per docs/design/04-cultivation-system.md
                Hp = 100f;
                Stamina = 100f;
                IsAgentControlled = false;
            }
        }

        public override void FixedUpdateNetwork()
        {
            if (_kcc == null)
            {
                return;
            }

            var moveVelocity = Vector3.zero;

            // Server-authoritative input application. The client sends
            // INetworkInput suggestions; Simple KCC owns predicted and
            // replicated movement state for the character body.
            if (GetInput(out NetworkInputData input))
            {
                var move = new Vector3(input.HorizontalAxis, 0f, input.VerticalAxis);
                move = Vector3.ClampMagnitude(move, 1f);

                if (move.sqrMagnitude > 0.0001f)
                {
                    _kcc.SetLookRotation(Quaternion.LookRotation(move), preservePitch: false, preserveYaw: false);
                    moveVelocity = move * _moveSpeed;
                }
            }

            _kcc.Move(moveVelocity, jumpImpulse: 0f);
        }
    }
}
