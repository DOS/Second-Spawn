using System;
using UnityEngine;

namespace SecondSpawn.Networking
{
    public enum VisualAnimationIntent
    {
        None = 0,
        Jump = 1,
        Talk = 2,
        Agree = 3,
        Disagree = 4,
        Interact = 5,
        Attack = 6,
        Cast = 7,
        DodgeLeft = 8,
        DodgeRight = 9,
        DodgeBackward = 10,
        Death = 11,
        Revive = 12,
    }

    /// <summary>
    /// Translates high-level visual intents into optional local Animator
    /// states. Gameplay authority stays on the networked root.
    /// </summary>
    [DisallowMultipleComponent]
    public sealed class VisualAnimationIntentDriver : MonoBehaviour
    {
        [SerializeField, Tooltip("Animator on the local-only visual child.")]
        private Animator _animator;

        [SerializeField, Tooltip("Cross-fade duration for one-shot visual intents.")]
        private float _crossFadeSeconds = 0.08f;

        [SerializeField] private string _triggerParameter = "Trigger";
        [SerializeField] private string _triggerNumberParameter = "TriggerNumber";
        [SerializeField] private string _actionParameter = "Action";
        [SerializeField] private string _talkingParameter = "Talking";
        [SerializeField] private string _weaponParameter = "Weapon";
        [SerializeField] private string _animationSpeedParameter = "AnimationSpeed";

        [SerializeField] private string _talkState = "Base Layer.Relax.Conversation.Relax-Talk1";
        [SerializeField] private string _agreeState = "Base Layer.Relax.Relax-Actions.Relax-Yes";
        [SerializeField] private string _disagreeState = "Base Layer.Relax.Relax-Actions.Relax-No";
        [SerializeField] private string _interactState = "Base Layer.Unarmed.Unarmed-Interact.Unarmed-Pickup";

        private bool _hasTriggerParameter;
        private bool _hasTriggerNumberParameter;
        private bool _hasActionParameter;
        private bool _hasWeaponParameter;
        private bool _hasAnimationSpeedParameter;
        private NetworkPlayer _networkPlayer;

        public bool TryPlay(VisualAnimationIntent intent)
        {
            ResolveAnimator();
            if (_animator == null || intent == VisualAnimationIntent.None)
            {
                return false;
            }

            SetAnimationSpeed(1f);
            return TryPlayThroughAnimatorContract(intent);
        }

        public void Play(string intentName)
        {
            if (Enum.TryParse(intentName, ignoreCase: true, out VisualAnimationIntent intent))
            {
                TryPlay(intent);
            }
        }

        private void Awake()
        {
            ResolveAnimator();
        }

        private void ResolveAnimator()
        {
            if (_animator == null)
            {
                _animator = GetComponent<Animator>();
            }

            if (_animator == null)
            {
                _animator = GetComponentInChildren<Animator>();
            }

            if (_animator != null)
            {
                _animator.applyRootMotion = false;
                _networkPlayer ??= GetComponentInParent<NetworkPlayer>();
                CacheParameters();
            }
        }

        private bool TryPlayThroughAnimatorContract(VisualAnimationIntent intent)
        {
            return intent switch
            {
                VisualAnimationIntent.Jump => false,
                VisualAnimationIntent.Talk => TryPlayTalking(),
                VisualAnimationIntent.Agree => TryCrossFadeState(_agreeState),
                VisualAnimationIntent.Disagree => TryCrossFadeState(_disagreeState),
                VisualAnimationIntent.Interact => TryFireActionTrigger(triggerNumber: 2, action: 2) || TryCrossFadeState(_interactState),
                VisualAnimationIntent.Attack => TryFireActionTrigger(triggerNumber: 4, action: 1),
                VisualAnimationIntent.Cast => TryFireActionTrigger(triggerNumber: 10, action: 1),
                VisualAnimationIntent.DodgeLeft => TryFireActionTrigger(triggerNumber: 13, action: 4),
                VisualAnimationIntent.DodgeRight => TryFireActionTrigger(triggerNumber: 13, action: 2),
                VisualAnimationIntent.DodgeBackward => TryFireActionTrigger(triggerNumber: 13, action: 3),
                VisualAnimationIntent.Death => TryFireTrigger(20),
                VisualAnimationIntent.Revive => TryFireTrigger(21),
                _ => false,
            };
        }

        private bool TryFireActionTrigger(int triggerNumber, int action)
        {
            if (!_hasActionParameter)
            {
                return false;
            }

            if (_hasWeaponParameter)
            {
                _animator.SetInteger(_weaponParameter, GetAnimatorWeaponValue());
            }

            _animator.SetInteger(_actionParameter, action);
            return TryFireTrigger(triggerNumber);
        }

        private bool TryPlayTalking()
        {
            TrySetInteger(_talkingParameter, 1);
            return TryCrossFadeState(_talkState);
        }

        private bool TryFireTrigger(int triggerNumber)
        {
            if (!_hasTriggerNumberParameter || !_hasTriggerParameter)
            {
                return false;
            }

            _animator.SetInteger(_triggerNumberParameter, triggerNumber);
            _animator.SetTrigger(_triggerParameter);
            return true;
        }

        private bool TryCrossFadeState(string stateName)
        {
            var layerIndex = 0;
            var stateHash = Animator.StringToHash(stateName);
            if (!_animator.HasState(layerIndex, stateHash))
            {
                var shortStateName = GetShortStateName(stateName);
                var shortStateHash = Animator.StringToHash(shortStateName);
                if (!_animator.HasState(layerIndex, shortStateHash))
                {
                    return false;
                }

                stateHash = shortStateHash;
            }

            SetAnimationSpeed(1f);
            _animator.CrossFadeInFixedTime(stateHash, Mathf.Max(0f, _crossFadeSeconds), layerIndex);
            return true;
        }

        private void SetAnimationSpeed(float speed)
        {
            if (_hasAnimationSpeedParameter)
            {
                _animator.SetFloat(_animationSpeedParameter, speed);
            }
        }

        private bool TrySetInteger(string parameterName, int value)
        {
            foreach (var parameter in _animator.parameters)
            {
                if (parameter.name == parameterName && parameter.type == AnimatorControllerParameterType.Int)
                {
                    _animator.SetInteger(parameterName, value);
                    return true;
                }
            }

            return false;
        }

        private void CacheParameters()
        {
            _hasTriggerParameter = false;
            _hasTriggerNumberParameter = false;
            _hasActionParameter = false;
            _hasWeaponParameter = false;
            _hasAnimationSpeedParameter = false;

            foreach (var parameter in _animator.parameters)
            {
                if (parameter.name == _triggerParameter && parameter.type == AnimatorControllerParameterType.Trigger)
                {
                    _hasTriggerParameter = true;
                }
                else if (parameter.name == _triggerNumberParameter && parameter.type == AnimatorControllerParameterType.Int)
                {
                    _hasTriggerNumberParameter = true;
                }
                else if (parameter.name == _actionParameter && parameter.type == AnimatorControllerParameterType.Int)
                {
                    _hasActionParameter = true;
                }
                else if (parameter.name == _weaponParameter && parameter.type == AnimatorControllerParameterType.Int)
                {
                    _hasWeaponParameter = true;
                }
                else if (parameter.name == _animationSpeedParameter && parameter.type == AnimatorControllerParameterType.Float)
                {
                    _hasAnimationSpeedParameter = true;
                }
            }
        }

        private static string GetShortStateName(string stateName)
        {
            var dotIndex = stateName.LastIndexOf('.');
            return dotIndex >= 0 && dotIndex < stateName.Length - 1 ? stateName[(dotIndex + 1)..] : stateName;
        }

        private int GetAnimatorWeaponValue()
        {
            if (_networkPlayer == null || _networkPlayer.EquipmentVisualId == EquipmentVisualCatalog.None)
            {
                return 0;
            }

            return EquipmentVisualCatalog.GetAnimatorWeaponValue(_networkPlayer.EquipmentVisualId);
        }
    }
}
