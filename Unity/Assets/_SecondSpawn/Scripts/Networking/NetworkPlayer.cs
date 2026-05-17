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
    /// <para>Vertical slice scope: position, rotation, level, combat stats,
    /// BodyTime, HP, and agent flag. Inventory, quest progress, NFT lock state are
    /// persisted in Supabase (durable layer), not held in
    /// <c>[Networked]</c> properties (session layer).</para>
    /// </summary>
    [DisallowMultipleComponent]
    [RequireComponent(typeof(SimpleKCC))]
    public sealed class NetworkPlayer : NetworkBehaviour
    {
        [Networked] public float Hp { get; set; }
        [Networked] public float Stamina { get; set; }
        [Networked] public int Level { get; set; }
        [Networked] public int Vitality { get; set; }
        [Networked] public int Force { get; set; }
        [Networked] public int Agility { get; set; }
        [Networked] public int Focus { get; set; }
        [Networked] public int Resilience { get; set; }
        [Networked] public int MaxHealth { get; set; }
        [Networked] public int MaxEnergy { get; set; }
        [Networked] public int AttackPower { get; set; }
        [Networked] public int DefensePower { get; set; }
        [Networked] public int BodyTimeRemainingSeconds { get; set; }
        [Networked] public int BodyTimeMaxSeconds { get; set; }
        [Networked] public int BodyTimeDangerDrainRate { get; set; }
        [Networked] public NetworkBool IsBodyDead { get; set; }
        [Networked] public int SecondBalanceSeconds { get; set; }
        [Networked] public int ReincarnationCount { get; set; }
        [Networked] public int VisualVariant { get; set; }
        [Networked] public int EquipmentVisualId { get; set; }
        [Networked] public NetworkBool SupportsJumpAnimation { get; set; }

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
        private VisualAnimationIntentDriver _visualIntentDriver;

        private void Awake()
        {
            _kcc = GetComponent<SimpleKCC>();
        }

        public override void Spawned()
        {
            _kcc ??= GetComponent<SimpleKCC>();

            if (HasStateAuthority)
            {
                ApplyDefaultStats();
                if (EquipmentVisualId == EquipmentVisualCatalog.None)
                {
                    EquipmentVisualId = EquipmentVisualCatalog.GetDefaultForVisualVariant(VisualVariant);
                }

                if (!SupportsJumpAnimation)
                {
                    SupportsJumpAnimation = true;
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
                    var speed = input.Run ? GetRunSpeed() : GetWalkSpeed();
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
            if (!HasStateAuthority)
            {
                Debug.LogWarning("[NetworkPlayer] Ignored prototype agent input on a non-authoritative player. Offline agents must be driven by the server/state authority.");
                return;
            }

            _prototypeAgentInput = input;
            _hasPrototypeAgentInput = true;
            IsAgentControlled = true;
        }

        public void ClearPrototypeAgentInput()
        {
            if (!HasStateAuthority)
            {
                return;
            }

            _prototypeAgentInput = default;
            _hasPrototypeAgentInput = false;
            IsAgentControlled = false;
        }

        public bool TryPlayVisualIntent(VisualAnimationIntent intent)
        {
            _visualIntentDriver ??= GetComponentInChildren<VisualAnimationIntentDriver>(includeInactive: true);
            return _visualIntentDriver != null && _visualIntentDriver.TryPlay(intent);
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

        public void ApplyProfileEquipment(int equipmentVisualId)
        {
            if (!HasStateAuthority)
            {
                Debug.LogWarning("[NetworkPlayer] Ignored profile equipment on a non-authoritative player.");
                return;
            }

            EquipmentVisualId = Mathf.Max(EquipmentVisualCatalog.None, equipmentVisualId);
        }

        public void ApplyProfileVisual(int visualVariant, int equipmentVisualId, bool supportsJumpAnimation)
        {
            if (!HasStateAuthority)
            {
                Debug.LogWarning("[NetworkPlayer] Ignored profile visual on a non-authoritative player.");
                return;
            }

            VisualVariant = VisualPrefabCatalog.NormalizeVariant(visualVariant);
            EquipmentVisualId = Mathf.Max(
                EquipmentVisualCatalog.None,
                equipmentVisualId != EquipmentVisualCatalog.None
                    ? equipmentVisualId
                    : EquipmentVisualCatalog.GetDefaultForVisualVariant(VisualVariant));
            SupportsJumpAnimation = supportsJumpAnimation;
        }

        public void ApplyProfileStats(
            int level,
            int vitality,
            int force,
            int agility,
            int focus,
            int resilience,
            int maxHealth,
            int maxEnergy,
            int attackPower,
            int defensePower,
            int bodyTimeRemainingSeconds,
            int bodyTimeMaxSeconds,
            int bodyTimeDangerDrainRate,
            string bodyLifecycle,
            int secondBalanceSeconds,
            int reincarnationCount)
        {
            if (!HasStateAuthority)
            {
                Debug.LogWarning("[NetworkPlayer] Ignored profile stats on a non-authoritative player.");
                return;
            }

            var previousMaxHealth = MaxHealth;
            var previousMaxEnergy = MaxEnergy;
            var profileSaysDead = IsDeadLifecycle(bodyLifecycle) || bodyTimeRemainingSeconds <= 0;
            var wasDead = Hp <= 0f || IsBodyDead;
            var healthRatio = previousMaxHealth > 0 ? Hp / previousMaxHealth : 1f;
            var energyRatio = previousMaxEnergy > 0 ? Stamina / previousMaxEnergy : 1f;

            Level = Mathf.Max(1, level);
            Vitality = Mathf.Clamp(vitality, 1, 999);
            Force = Mathf.Clamp(force, 1, 999);
            Agility = Mathf.Clamp(agility, 1, 999);
            Focus = Mathf.Clamp(focus, 1, 999);
            Resilience = Mathf.Clamp(resilience, 1, 999);
            MaxHealth = Mathf.Max(1, maxHealth);
            MaxEnergy = Mathf.Max(1, maxEnergy);
            AttackPower = Mathf.Max(0, attackPower);
            DefensePower = Mathf.Max(0, defensePower);
            BodyTimeRemainingSeconds = Mathf.Max(0, bodyTimeRemainingSeconds);
            BodyTimeMaxSeconds = Mathf.Max(0, bodyTimeMaxSeconds);
            BodyTimeDangerDrainRate = Mathf.Max(0, bodyTimeDangerDrainRate);
            IsBodyDead = profileSaysDead;
            SecondBalanceSeconds = Mathf.Max(0, secondBalanceSeconds);
            ReincarnationCount = Mathf.Max(0, reincarnationCount);

            Hp = profileSaysDead
                ? 0f
                : wasDead
                ? MaxHealth
                : previousMaxHealth > 0
                ? Mathf.Clamp(Mathf.Round(MaxHealth * healthRatio), 1f, MaxHealth)
                : MaxHealth;
            Stamina = profileSaysDead
                ? 0f
                : previousMaxEnergy > 0
                ? Mathf.Clamp(Mathf.Round(MaxEnergy * energyRatio), 0f, MaxEnergy)
                : MaxEnergy;
        }

        private void ApplyDefaultStats()
        {
            Level = 1;
            Vitality = 10;
            Force = 8;
            Agility = 8;
            Focus = 8;
            Resilience = 8;
            MaxHealth = 100;
            MaxEnergy = 50;
            AttackPower = 10;
            DefensePower = 5;
            BodyTimeRemainingSeconds = 24 * 60 * 60;
            BodyTimeMaxSeconds = 24 * 60 * 60;
            BodyTimeDangerDrainRate = 1;
            VisualVariant = 12;
            SupportsJumpAnimation = true;
            IsBodyDead = false;
            SecondBalanceSeconds = 7 * 24 * 60 * 60;
            ReincarnationCount = 0;
            Hp = MaxHealth;
            Stamina = MaxEnergy;
        }

        private static bool IsDeadLifecycle(string lifecycle)
        {
            return !string.IsNullOrWhiteSpace(lifecycle) &&
                   lifecycle.Trim().Equals("dead", System.StringComparison.OrdinalIgnoreCase);
        }

        private float GetRunSpeed()
        {
            return _moveSpeed * GetAgilitySpeedMultiplier();
        }

        private float GetWalkSpeed()
        {
            return _walkSpeed * GetAgilitySpeedMultiplier();
        }

        private float GetAgilitySpeedMultiplier()
        {
            return Mathf.Clamp(Agility / 8f, 0.75f, 1.4f);
        }
    }
}
