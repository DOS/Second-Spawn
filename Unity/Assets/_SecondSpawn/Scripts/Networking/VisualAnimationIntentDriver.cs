using System;
using System.Collections.Generic;
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
        PickUpItem = 13,
        TakeItem = 14,
        ReceiveItem = 15,
        HandoffItem = 16,
        PutDownItem = 17,
        DropItem = 18,
        BeltItem = 19,
        BackItem = 20,
        Eat = 21,
        Drink = 22,
        Water = 23,
        Plant = 24,
        Gather = 25,
        Bored = 26,
        ChopStart = 27,
        ChopVertical = 28,
        ChopHorizontal = 29,
        ChopDiagonal = 30,
        ChopGround = 31,
        ChopCeiling = 32,
        ChopFinish = 33,
        DigStart = 34,
        DigScoop = 35,
        DigFinish = 36,
        FishCast = 37,
        FishReel = 38,
        SawStart = 39,
        SawFinish = 40,
        HammerWall = 41,
        HammerTable = 42,
        SickleUse = 43,
        RakeUse = 44,
        ChairSit = 45,
        ChairTalk = 46,
        ChairEat = 47,
        ChairDrink = 48,
        ChairStand = 49,
        ClimbStart = 50,
        ClimbOffBottom = 51,
        ClimbUp = 52,
        ClimbDown = 53,
        ClimbOffTop = 54,
        ClimbOnTop = 55,
        PushPullStart = 56,
        PushPullRelease = 57,
        CarryPickup = 58,
        CarryReceive = 59,
        CarryHandoff = 60,
        CarryPutdown = 61,
        PullUpItem = 62,
        BeltAwayItem = 63,
        BackAwayItem = 64,
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
        [SerializeField] private string _triggerNumberSpacedParameter = "Trigger Number";
        [SerializeField] private string _actionParameter = "Action";
        [SerializeField] private string _talkingParameter = "Talking";
        [SerializeField] private string _weaponParameter = "Weapon";
        [SerializeField] private string _animationSpeedParameter = "AnimationSpeed";
        [SerializeField] private string _animationSpeedSpacedParameter = "Animation Speed";

        [SerializeField] private string _talkState = "Base Layer.Relax.Conversation.Relax-Talk1";
        [SerializeField] private string _agreeState = "Base Layer.Relax.Relax-Actions.Relax-Yes";
        [SerializeField] private string _disagreeState = "Base Layer.Relax.Relax-Actions.Relax-No";
        [SerializeField] private string _interactState = "Base Layer.Unarmed.Unarmed-Interact.Unarmed-Pickup";

        private bool _hasTriggerParameter;
        private bool _hasTriggerNumberParameter;
        private bool _hasTriggerNumberSpacedParameter;
        private bool _hasActionParameter;
        private bool _hasTalkingParameter;
        private bool _hasWeaponParameter;
        private bool _hasAnimationSpeedParameter;
        private bool _hasAnimationSpeedSpacedParameter;
        private NetworkPlayer _networkPlayer;
        private RuntimeAnimatorController _cachedController;
        private readonly HashSet<string> _availableTriggerNames = new HashSet<string>();

        public bool TryPlay(VisualAnimationIntent intent)
        {
            ResolveAnimator();
            if (_animator == null || intent == VisualAnimationIntent.None)
            {
                return false;
            }

            if (!IsSupportedByBody(intent))
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
                CacheParametersIfNeeded();
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
                _ => TryFireNamedTrigger(GetNamedTrigger(intent)),
            };
        }

        private static string GetNamedTrigger(VisualAnimationIntent intent)
        {
            return intent switch
            {
                VisualAnimationIntent.PickUpItem => "ItemPickupTrigger",
                VisualAnimationIntent.TakeItem => "ItemTakeTrigger",
                VisualAnimationIntent.ReceiveItem => "ItemRecieveTrigger",
                VisualAnimationIntent.HandoffItem => "ItemHandoffTrigger",
                VisualAnimationIntent.PutDownItem => "ItemPutdownTrigger",
                VisualAnimationIntent.DropItem => "ItemDropTrigger",
                VisualAnimationIntent.BeltItem => "ItemBeltTrigger",
                VisualAnimationIntent.BackItem => "ItemBackTrigger",
                VisualAnimationIntent.PullUpItem => "ItemPullUpTrigger",
                VisualAnimationIntent.BeltAwayItem => "ItemBeltAwayTrigger",
                VisualAnimationIntent.BackAwayItem => "ItemBackAwayTrigger",
                VisualAnimationIntent.Eat => "ItemEatTrigger",
                VisualAnimationIntent.Drink => "ItemDrinkTrigger",
                VisualAnimationIntent.Water => "ItemWaterTrigger",
                VisualAnimationIntent.Plant => "ItemPlantTrigger",
                VisualAnimationIntent.Gather => "GatherTrigger",
                VisualAnimationIntent.Bored => "Bored1Trigger",
                VisualAnimationIntent.ChopStart => "ChoppingStartTrigger",
                VisualAnimationIntent.ChopVertical => "ChopVerticalTrigger",
                VisualAnimationIntent.ChopHorizontal => "ChopHorizontalTrigger",
                VisualAnimationIntent.ChopDiagonal => "ChopDiagonalTrigger",
                VisualAnimationIntent.ChopGround => "ChopGroundTrigger",
                VisualAnimationIntent.ChopCeiling => "ChopCeilingTrigger",
                VisualAnimationIntent.ChopFinish => "ChopFinishTrigger",
                VisualAnimationIntent.DigStart => "DiggingStartTrigger",
                VisualAnimationIntent.DigScoop => "DiggingScoopTrigger",
                VisualAnimationIntent.DigFinish => "DiggingFinishTrigger",
                VisualAnimationIntent.FishCast => "FishingCastTrigger",
                VisualAnimationIntent.FishReel => "FishingReelTrigger",
                VisualAnimationIntent.SawStart => "SawStartTrigger",
                VisualAnimationIntent.SawFinish => "SawFinishTrigger",
                VisualAnimationIntent.HammerWall => "HammerWallTrigger",
                VisualAnimationIntent.HammerTable => "HammerTableTrigger",
                VisualAnimationIntent.SickleUse => "ItemSickleUse",
                VisualAnimationIntent.RakeUse => "ItemRakeUse",
                VisualAnimationIntent.ChairSit => "ChairSitTrigger",
                VisualAnimationIntent.ChairTalk => "ChairTalk1Trigger",
                VisualAnimationIntent.ChairEat => "ChairEatTrigger",
                VisualAnimationIntent.ChairDrink => "ChairDrinkTrigger",
                VisualAnimationIntent.ChairStand => "ChairStandTrigger",
                VisualAnimationIntent.ClimbStart => "ClimbStartTrigger",
                VisualAnimationIntent.ClimbOffBottom => "ClimbOffBottomTrigger",
                VisualAnimationIntent.ClimbUp => "ClimbUpTrigger",
                VisualAnimationIntent.ClimbDown => "ClimbDownTrigger",
                VisualAnimationIntent.ClimbOffTop => "ClimbOffTopTrigger",
                VisualAnimationIntent.ClimbOnTop => "ClimbOnTopTrigger",
                VisualAnimationIntent.PushPullStart => "PushPullStartTrigger",
                VisualAnimationIntent.PushPullRelease => "PushPullReleaseTrigger",
                VisualAnimationIntent.CarryPickup => "CarryPickupTrigger",
                VisualAnimationIntent.CarryReceive => "CarryRecieveTrigger",
                VisualAnimationIntent.CarryHandoff => "CarryHandoffTrigger",
                VisualAnimationIntent.CarryPutdown => "CarryPutdownTrigger",
                _ => "",
            };
        }

        private bool IsSupportedByBody(VisualAnimationIntent intent)
        {
            if (_networkPlayer == null)
            {
                return true;
            }

            return intent switch
            {
                VisualAnimationIntent.Jump => _networkPlayer.SupportsJumpAnimation,
                VisualAnimationIntent.Attack => _networkPlayer.SupportsMeleeAnimation || _networkPlayer.SupportsRangedAnimation,
                VisualAnimationIntent.Cast => _networkPlayer.SupportsMeleeAnimation || _networkPlayer.SupportsRangedAnimation,
                VisualAnimationIntent.DodgeLeft or
                    VisualAnimationIntent.DodgeRight or
                    VisualAnimationIntent.DodgeBackward => _networkPlayer.SupportsRollAnimation,
                _ => true
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
            if (_hasTalkingParameter)
            {
                _animator.SetInteger(_talkingParameter, 1);
            }

            return TryCrossFadeState(_talkState);
        }

        private bool TryFireTrigger(int triggerNumber)
        {
            if (!HasAnyTriggerNumberParameter() || !_hasTriggerParameter)
            {
                return false;
            }

            SetTriggerNumber(triggerNumber);
            _animator.SetTrigger(_triggerParameter);
            return true;
        }

        private bool TryFireNamedTrigger(string triggerName)
        {
            if (string.IsNullOrWhiteSpace(triggerName) || !_availableTriggerNames.Contains(triggerName))
            {
                return false;
            }

            _animator.SetTrigger(triggerName);
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

            if (_hasAnimationSpeedSpacedParameter)
            {
                _animator.SetFloat(_animationSpeedSpacedParameter, speed);
            }
        }

        private void CacheParametersIfNeeded()
        {
            var controller = _animator.runtimeAnimatorController;
            if (_cachedController == controller)
            {
                return;
            }

            _cachedController = controller;
            CacheParameters();
        }

        private void CacheParameters()
        {
            _hasTriggerParameter = false;
            _hasTriggerNumberParameter = false;
            _hasTriggerNumberSpacedParameter = false;
            _hasActionParameter = false;
            _hasTalkingParameter = false;
            _hasWeaponParameter = false;
            _hasAnimationSpeedParameter = false;
            _hasAnimationSpeedSpacedParameter = false;
            _availableTriggerNames.Clear();

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
                else if (parameter.name == _triggerNumberSpacedParameter && parameter.type == AnimatorControllerParameterType.Int)
                {
                    _hasTriggerNumberSpacedParameter = true;
                }
                else if (parameter.name == _actionParameter && parameter.type == AnimatorControllerParameterType.Int)
                {
                    _hasActionParameter = true;
                }
                else if (parameter.name == _talkingParameter && parameter.type == AnimatorControllerParameterType.Int)
                {
                    _hasTalkingParameter = true;
                }
                else if (parameter.name == _weaponParameter && parameter.type == AnimatorControllerParameterType.Int)
                {
                    _hasWeaponParameter = true;
                }
                else if (parameter.name == _animationSpeedParameter && parameter.type == AnimatorControllerParameterType.Float)
                {
                    _hasAnimationSpeedParameter = true;
                }
                else if (parameter.name == _animationSpeedSpacedParameter && parameter.type == AnimatorControllerParameterType.Float)
                {
                    _hasAnimationSpeedSpacedParameter = true;
                }

                if (parameter.type == AnimatorControllerParameterType.Trigger)
                {
                    _availableTriggerNames.Add(parameter.name);
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
