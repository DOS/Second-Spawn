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
        [Networked] public int VisualVariant { get; set; }
        [Networked] public int EquipmentVisualId { get; set; }

        /// <summary>True when the offline AI agent is driving this character (Pillar 1).</summary>
        [Networked] public NetworkBool IsAgentControlled { get; set; }

        [SerializeField, Tooltip("Run speed in units/second. Simple KCC owns authoritative movement for this spike.")]
        private float _moveSpeed = 5f;

        [SerializeField, Tooltip("Walk speed in units/second. Shift toggles between walk and run during the prototype.")]
        private float _walkSpeed = 2.2f;

        [SerializeField, Tooltip("Authoritative jump impulse applied by Fusion Simple KCC.")]
        private float _jumpImpulse = 7.5f;

        private SimpleKCC _kcc;
        private NetworkInputData _prototypeAgentInput;
        private bool _hasPrototypeAgentInput;

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
                if (EquipmentVisualId == EquipmentVisualCatalog.None)
                {
                    EquipmentVisualId = EquipmentVisualCatalog.GetDefaultForVisualVariant(VisualVariant);
                }

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
            var jumpImpulse = 0f;

            // Server-authoritative input application. The client sends
            // INetworkInput suggestions; Simple KCC owns predicted and
            // replicated movement state for the character body.
            if (TryGetAuthoritativeInput(out NetworkInputData input))
            {
                var move = new Vector3(input.HorizontalAxis, 0f, input.VerticalAxis);
                move = Vector3.ClampMagnitude(move, 1f);

                if (move.sqrMagnitude > 0.0001f)
                {
                    _kcc.SetLookRotation(Quaternion.LookRotation(move), preservePitch: false, preserveYaw: false);
                    var speed = input.Run ? _moveSpeed : _walkSpeed;
                    moveVelocity = move * speed;
                }

                if (input.Jump)
                {
                    jumpImpulse = _jumpImpulse;
                }
            }

            _kcc.Move(moveVelocity, jumpImpulse);
        }

        public void SetPrototypeAgentInput(NetworkInputData input)
        {
            _prototypeAgentInput = input;
            _hasPrototypeAgentInput = true;
            if (HasStateAuthority)
            {
                IsAgentControlled = true;
            }
        }

        public void ClearPrototypeAgentInput()
        {
            _prototypeAgentInput = default;
            _hasPrototypeAgentInput = false;
            if (HasStateAuthority)
            {
                IsAgentControlled = false;
            }
        }

        private bool TryGetAuthoritativeInput(out NetworkInputData input)
        {
            if (_hasPrototypeAgentInput && IsAgentControlled)
            {
                input = _prototypeAgentInput;
                return true;
            }

            return GetInput(out input);
        }
    }
}
