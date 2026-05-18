using Fusion;
using Fusion.Addons.SimpleKCC;
using UnityEngine;

namespace SecondSpawn.Networking
{
    /// <summary>
    /// Replicates compact visual animation state from the networked KCC root
    /// without letting animation own movement authority.
    /// </summary>
    [DisallowMultipleComponent]
    public sealed class NetworkAnimatorBridge : NetworkBehaviour
    {
        [Networked]
        public float NetSpeed { get; set; }

        [Networked]
        public float NetVelocityX { get; set; }

        [Networked]
        public float NetVelocityZ { get; set; }

        [Networked]
        public int NetJumping { get; set; }

        [SerializeField, Tooltip("Animator on the visual child. If empty, the first child Animator is used.")]
        private Animator _animator;

        [SerializeField, Tooltip("Planar speed that maps to normalized animator velocity 1.0.")]
        private float _referenceMoveSpeed = 5f;

        [SerializeField, Tooltip("Animator bool parameter used by RPG Character Mecanim Animation Pack.")]
        private string _movingParameter = "Moving";

        [SerializeField, Tooltip("Animator float parameter used by RPG Character Mecanim Animation Pack.")]
        private string _velocityXParameter = "Velocity X";

        [SerializeField, Tooltip("Animator float parameter used by RPG Character Mecanim Animation Pack.")]
        private string _velocityZParameter = "Velocity Z";

        [SerializeField, Tooltip("Animator float parameter used by ExplosiveLLC free Warrior/Fighter controllers.")]
        private string _velocityParameter = "Velocity";

        [SerializeField, Tooltip("Animator float parameter used by RPG Character Mecanim Animation Pack.")]
        private string _animationSpeedParameter = "AnimationSpeed";

        [SerializeField, Tooltip("Animator float parameter used by ExplosiveLLC free Warrior/Fighter controllers.")]
        private string _animationSpeedSpacedParameter = "Animation Speed";

        [SerializeField, Tooltip("Animator int parameter used by RPG Character Mecanim Animation Pack.")]
        private string _weaponParameter = "Weapon";

        [SerializeField, Tooltip("Default visual locomotion weapon. -1 is Relax, 0 is Unarmed combat.")]
        private int _defaultWeaponValue = -1;

        [SerializeField, Tooltip("Animator int parameter used by RPG Character Mecanim Animation Pack.")]
        private string _jumpingParameter = "Jumping";

        [SerializeField, Tooltip("Animator int parameter used by RPG Character Mecanim Animation Pack.")]
        private string _triggerNumberParameter = "TriggerNumber";

        [SerializeField, Tooltip("Animator int parameter used by ExplosiveLLC free Warrior/Fighter controllers.")]
        private string _triggerNumberSpacedParameter = "Trigger Number";

        [SerializeField, Tooltip("Animator trigger parameter used by RPG Character Mecanim Animation Pack.")]
        private string _triggerParameter = "Trigger";

        [SerializeField, Tooltip("Vertical speed threshold that switches the visual from jump to fall. Positive values start the fall blend just before the physical apex to avoid a held jump pose.")]
        private float _fallVelocityThreshold = 0.8f;

        private bool _hasMovingParameter;
        private bool _hasVelocityXParameter;
        private bool _hasVelocityZParameter;
        private bool _hasVelocityParameter;
        private bool _hasAnimationSpeedParameter;
        private bool _hasAnimationSpeedSpacedParameter;
        private bool _hasWeaponParameter;
        private bool _hasJumpingParameter;
        private bool _hasTriggerNumberParameter;
        private bool _hasTriggerNumberSpacedParameter;
        private bool _hasTriggerParameter;
        private bool _hasAnimatorContract;
        private bool _warnedMissingAnimatorContract;
        private bool _initializedAnimatorDefaults;
        private bool _wasAirborne;
        private int _lastJumpingValue = int.MinValue;
        private SimpleKCC _kcc;
        private NetworkPlayer _networkPlayer;
        private Animator _cachedAnimator;

        private void Awake()
        {
            ResolveAnimator();
        }

        public override void FixedUpdateNetwork()
        {
            if (!HasStateAuthority)
            {
                return;
            }

            _kcc ??= GetComponent<SimpleKCC>();
            if (_kcc == null)
            {
                return;
            }

            var worldVelocity = _kcc.RealVelocity;
            worldVelocity.y = 0f;

            var localVelocity = transform.InverseTransformDirection(worldVelocity);
            var referenceMoveSpeed = Mathf.Max(0.01f, _referenceMoveSpeed);
            NetSpeed = Mathf.Clamp01(worldVelocity.magnitude / referenceMoveSpeed);
            NetVelocityX = Mathf.Clamp(localVelocity.x / referenceMoveSpeed, -1f, 1f);
            NetVelocityZ = Mathf.Clamp(localVelocity.z / referenceMoveSpeed, -1f, 1f);
            NetJumping = GetJumpingFromKcc();
        }

        public override void Render()
        {
            ResolveAnimator();
            if (_animator == null)
            {
                return;
            }

            ApplyMovement(NetSpeed, NetVelocityX, NetVelocityZ);
            ApplyJumping(NetJumping);
        }

        private void ApplyMovement(float normalizedSpeed, float velocityX, float velocityZ)
        {
            if (_hasMovingParameter)
            {
                _animator.SetBool(_movingParameter, normalizedSpeed > 0.02f);
            }

            if (_hasVelocityXParameter)
            {
                _animator.SetFloat(_velocityXParameter, velocityX);
            }

            if (_hasVelocityZParameter)
            {
                _animator.SetFloat(_velocityZParameter, velocityZ);
            }

            if (_hasVelocityParameter)
            {
                _animator.SetFloat(_velocityParameter, normalizedSpeed);
            }

            if (_hasAnimationSpeedParameter)
            {
                _animator.SetFloat(_animationSpeedParameter, 1f);
            }

            if (_hasAnimationSpeedSpacedParameter)
            {
                _animator.SetFloat(_animationSpeedSpacedParameter, 1f);
            }

            if (_hasWeaponParameter)
            {
                _animator.SetInteger(_weaponParameter, GetAnimatorWeaponValue());
            }
        }

        private void ResolveAnimator()
        {
            if (_animator == null)
            {
                _animator = GetComponentInChildren<Animator>();
                if (_animator == null)
                {
                    return;
                }
            }

            _animator.applyRootMotion = false;
            _animator.updateMode = AnimatorUpdateMode.Normal;
            _animator.speed = 1f;
            _kcc ??= GetComponent<SimpleKCC>();
            _networkPlayer ??= GetComponentInParent<NetworkPlayer>();
            if (_cachedAnimator != _animator)
            {
                _cachedAnimator = _animator;
                _initializedAnimatorDefaults = false;
                _lastJumpingValue = int.MinValue;
                CacheParameters();
                InitializeAnimatorDefaults();
            }
        }

        private void CacheParameters()
        {
            _hasMovingParameter = false;
            _hasVelocityXParameter = false;
            _hasVelocityZParameter = false;
            _hasVelocityParameter = false;
            _hasAnimationSpeedParameter = false;
            _hasAnimationSpeedSpacedParameter = false;
            _hasWeaponParameter = false;
            _hasJumpingParameter = false;
            _hasTriggerNumberParameter = false;
            _hasTriggerNumberSpacedParameter = false;
            _hasTriggerParameter = false;
            _hasAnimatorContract = false;

            foreach (var parameter in _animator.parameters)
            {
                if (parameter.name == _movingParameter && parameter.type == AnimatorControllerParameterType.Bool)
                {
                    _hasMovingParameter = true;
                }
                else if (parameter.name == _velocityXParameter && parameter.type == AnimatorControllerParameterType.Float)
                {
                    _hasVelocityXParameter = true;
                }
                else if (parameter.name == _velocityZParameter && parameter.type == AnimatorControllerParameterType.Float)
                {
                    _hasVelocityZParameter = true;
                }
                else if (parameter.name == _velocityParameter && parameter.type == AnimatorControllerParameterType.Float)
                {
                    _hasVelocityParameter = true;
                }
                else if (parameter.name == _animationSpeedParameter && parameter.type == AnimatorControllerParameterType.Float)
                {
                    _hasAnimationSpeedParameter = true;
                }
                else if (parameter.name == _animationSpeedSpacedParameter && parameter.type == AnimatorControllerParameterType.Float)
                {
                    _hasAnimationSpeedSpacedParameter = true;
                }
                else if (parameter.name == _weaponParameter && parameter.type == AnimatorControllerParameterType.Int)
                {
                    _hasWeaponParameter = true;
                }
                else if (parameter.name == _jumpingParameter && parameter.type == AnimatorControllerParameterType.Int)
                {
                    _hasJumpingParameter = true;
                }
                else if (parameter.name == _triggerNumberParameter && parameter.type == AnimatorControllerParameterType.Int)
                {
                    _hasTriggerNumberParameter = true;
                }
                else if (parameter.name == _triggerNumberSpacedParameter && parameter.type == AnimatorControllerParameterType.Int)
                {
                    _hasTriggerNumberSpacedParameter = true;
                }
                else if (parameter.name == _triggerParameter && parameter.type == AnimatorControllerParameterType.Trigger)
                {
                    _hasTriggerParameter = true;
                }
            }

            _hasAnimatorContract = _hasMovingParameter || _hasVelocityXParameter || _hasVelocityZParameter || _hasVelocityParameter;
            if (!_hasAnimatorContract && !_warnedMissingAnimatorContract)
            {
                _warnedMissingAnimatorContract = true;
                Debug.LogWarning($"[NetworkAnimatorBridge] Animator '{_animator.runtimeAnimatorController?.name}' does not expose the expected RPG Character locomotion parameters.");
            }
        }

        private int GetJumpingFromKcc()
        {
            if (_kcc == null)
            {
                return 0;
            }

            if (_kcc.HasJumped && !_wasAirborne)
            {
                _wasAirborne = true;
                return 1;
            }

            if (!_wasAirborne)
            {
                return 0;
            }

            if (!_kcc.IsGrounded)
            {
                if (_kcc.RealVelocity.y <= _fallVelocityThreshold)
                {
                    return 2;
                }

                return 1;
            }

            _wasAirborne = false;
            return 0;
        }

        private void ApplyJumping(int jumping)
        {
            if (_networkPlayer != null && !_networkPlayer.SupportsJumpAnimation)
            {
                if (_hasJumpingParameter)
                {
                    SetJumpingValueOnly(0);
                }

                return;
            }

            if (!_hasJumpingParameter || !HasAnyTriggerNumberParameter() || !_hasTriggerParameter)
            {
                return;
            }

            SetJumping(jumping, triggerTransition: true);
        }

        public void SetAnimator(Animator animator)
        {
            if (_animator == animator)
            {
                return;
            }

            _animator = animator;
            _cachedAnimator = null;
            _initializedAnimatorDefaults = false;
            _wasAirborne = false;
            _lastJumpingValue = int.MinValue;
            ResolveAnimator();
        }

        private void InitializeAnimatorDefaults()
        {
            if (_initializedAnimatorDefaults || _animator == null)
            {
                return;
            }

            _initializedAnimatorDefaults = true;
            _wasAirborne = false;
            if (_hasMovingParameter)
            {
                _animator.SetBool(_movingParameter, false);
            }

            if (_hasVelocityXParameter)
            {
                _animator.SetFloat(_velocityXParameter, 0f);
            }

            if (_hasVelocityZParameter)
            {
                _animator.SetFloat(_velocityZParameter, 0f);
            }

            if (_hasVelocityParameter)
            {
                _animator.SetFloat(_velocityParameter, 0f);
            }

            if (_hasAnimationSpeedParameter)
            {
                _animator.SetFloat(_animationSpeedParameter, 1f);
            }

            if (_hasAnimationSpeedSpacedParameter)
            {
                _animator.SetFloat(_animationSpeedSpacedParameter, 1f);
            }

            if (_hasWeaponParameter)
            {
                _animator.SetInteger(_weaponParameter, GetAnimatorWeaponValue());
            }

            if (_hasJumpingParameter)
            {
                SetJumpingValueOnly(0);
            }
        }

        private void SetJumping(int value)
        {
            SetJumping(value, triggerTransition: true);
        }

        private void SetJumping(int value, bool triggerTransition)
        {
            if (_lastJumpingValue == value)
            {
                return;
            }

            _lastJumpingValue = value;
            _animator.SetInteger(_jumpingParameter, value);
            if (triggerTransition)
            {
                SetTriggerNumber(18);
                _animator.SetTrigger(_triggerParameter);
            }
        }

        private void SetJumpingValueOnly(int value)
        {
            if (_lastJumpingValue == value)
            {
                return;
            }

            _lastJumpingValue = value;
            _animator.SetInteger(_jumpingParameter, value);
        }

        private int GetAnimatorWeaponValue()
        {
            if (_networkPlayer == null || _networkPlayer.EquipmentVisualId == EquipmentVisualCatalog.None)
            {
                return _defaultWeaponValue;
            }

            return EquipmentVisualCatalog.GetAnimatorWeaponValue(_networkPlayer.EquipmentVisualId);
        }

        private bool HasAnyTriggerNumberParameter()
        {
            return _hasTriggerNumberParameter || _hasTriggerNumberSpacedParameter;
        }

        private void SetTriggerNumber(int value)
        {
            if (_hasTriggerNumberParameter)
            {
                _animator.SetInteger(_triggerNumberParameter, value);
            }

            if (_hasTriggerNumberSpacedParameter)
            {
                _animator.SetInteger(_triggerNumberSpacedParameter, value);
            }
        }
    }
}
