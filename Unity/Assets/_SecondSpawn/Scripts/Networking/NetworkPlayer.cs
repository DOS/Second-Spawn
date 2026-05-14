using Fusion;
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
    public sealed class NetworkPlayer : NetworkBehaviour
    {
        [Networked] public Vector3 NetworkedPosition { get; set; }
        [Networked] public Quaternion NetworkedRotation { get; set; }
        [Networked] public int CultivationTier { get; set; }
        [Networked] public float Hp { get; set; }
        [Networked] public float Stamina { get; set; }

        /// <summary>True when the offline AI agent is driving this character (Pillar 1).</summary>
        [Networked] public NetworkBool IsAgentControlled { get; set; }

        [SerializeField, Tooltip("Movement speed in units/second. Will be replaced by Opsive UCC stats in slice phase 2.")]
        private float _moveSpeed = 5f;

        public override void Spawned()
        {
            if (HasStateAuthority)
            {
                NetworkedPosition = transform.position;
                NetworkedRotation = transform.rotation;
                CultivationTier = 1; // Awakening - starting tier per docs/design/04-cultivation-system.md
                Hp = 100f;
                Stamina = 100f;
                IsAgentControlled = false;
            }
        }

        public override void FixedUpdateNetwork()
        {
            // Server-authoritative input application. Client never mutates
            // [Networked] state directly; client only sends INetworkInput
            // suggestions which the server validates here.
            if (GetInput(out NetworkInputData input))
            {
                var move = new Vector3(input.HorizontalAxis, 0f, input.VerticalAxis);
                if (move.sqrMagnitude > 0f)
                {
                    NetworkedPosition += move.normalized * _moveSpeed * Runner.DeltaTime;
                }
            }

            // Apply networked transform to GameObject so all clients see the
            // server-authoritative position.
            transform.position = NetworkedPosition;
            transform.rotation = NetworkedRotation;
        }
    }
}
