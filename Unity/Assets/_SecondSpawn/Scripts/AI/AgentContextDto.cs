using System;

namespace SecondSpawn.AI
{
    [Serializable]
    public sealed class AgentContextDto
    {
        public PlayerProfileDto player;
        public BodyProfileDto body;
    }

    [Serializable]
    public sealed class ActorProfileDto
    {
        public string actor_id;
        public string actor_type;
        public string owner_player_id;
        public string display_name;
        public BodyProfileDto body;
        public MemoryRecordDto[] memory;
        public AgentRuntimeDto agent_runtime;
        public AgentActivityRecordDto[] agent_activity;
        public string created_at;
        public string updated_at;
    }

    [Serializable]
    public sealed class PlayerProfileDto
    {
        public string player_id;
        public string display_name;
        public long second_balance_seconds;
        public long reincarnation_count;
    }

    [Serializable]
    public sealed class BodyProfileDto
    {
        public string body_id;
        public string archetype_id;
        public string visual_prefab_key;
        public int visual_variant = -1;
        public BodyAppearanceDto appearance;
        public BodyInhabitationDto inhabitation;
        public EquipmentLoadoutDto equipment;
        public CharacterStatsDto stats;
        public CharacterTraitsDto characteristics;
        public BodyStoryDto story;
        public AnimationCapabilitiesDto animation_capabilities;
        public BodyTimeDto time;
        public string lifecycle = "alive";
        public FrameIdentityDto identity;
        public FrameSkillDto[] skills;
        public FrameAgentDto[] agents;
        public FrameToolDto[] tools;
        public FrameHeartbeatDto heartbeat;
        public AgentPolicyDto agent_policy;
        public SoulProfileDto soul;
        public MemoryRecordDto[] memory;
        public AgentRuntimeDto agent_runtime;
        public AgentActivityRecordDto[] agent_activity;
    }

    [Serializable]
    public sealed class EquipmentLoadoutDto
    {
        public string primary_weapon = "none";
        public int equipment_visual_id;
        public string weapon_visual_key;
        public string weapon_family;
        public string combat_stance;
        public string socket;
    }

    [Serializable]
    public sealed class BodyAppearanceDto
    {
        public string body_type;
        public BodyPartsDto body_parts;
        public string skin;
        public string hair;
        public string material;
        public string[] marks;
    }

    [Serializable]
    public sealed class BodyPartsDto
    {
        public string head;
        public string face;
        public string torso;
        public string arms;
        public string legs;
    }

    [Serializable]
    public sealed class BodyInhabitationDto
    {
        public string source_actor_id;
        public string previous_role;
        public bool inhabited_by_player;
        public string assigned_at;
    }

    [Serializable]
    public sealed class CharacterTraitsDto
    {
        public int curiosity = 6;
        public int courage = 5;
        public int empathy = 5;
        public int discipline = 5;
        public int aggression = 3;
        public int sociability = 5;
    }

    [Serializable]
    public sealed class BodyStoryDto
    {
        public string origin;
        public string role;
        public string conflict;
        public string rumor;
    }

    [Serializable]
    public sealed class AnimationCapabilitiesDto
    {
        public bool supports_jump = true;
        public bool supports_roll = true;
        public bool supports_melee = true;
        public bool supports_ranged;
        public string weapon_stance;
    }

    [Serializable]
    public sealed class CharacterStatsDto
    {
        public int level = 1;
        public int vitality = 10;
        public int force = 8;
        public int agility = 8;
        public int focus = 8;
        public int resilience = 8;
        public int max_health = 100;
        public int max_energy = 50;
        public int attack_power = 10;
        public int defense_power = 5;
    }

    [Serializable]
    public sealed class BodyTimeDto
    {
        public long remaining_seconds;
        public long max_seconds;
        public long danger_drain_rate;
    }

    [Serializable]
    public sealed class FrameIdentityDto
    {
        public string public_name;
        public string callsign;
        public string public_role;
        public string faction_title;
        public string profession;
        public string reputation_summary;
    }

    [Serializable]
    public sealed class FrameSkillDto
    {
        public string id;
        public string name;
        public string category;
        public int rank = 1;
        public string summary;
    }

    [Serializable]
    public sealed class FrameAgentDto
    {
        public string id;
        public string mode;
        public int priority = 1;
        public string routine;
        public string[] allowed_activities;
        public string[] forbidden_activities;
    }

    [Serializable]
    public sealed class FrameToolDto
    {
        public string name;
        public string category;
        public string intent;
        public bool requires_validation = true;
    }

    [Serializable]
    public sealed class FrameHeartbeatDto
    {
        public long cadence_seconds = 60;
        public string last_seen_at;
        public string offline_session_state;
        public string last_action_summary;
        public string fallback_state;
    }

    [Serializable]
    public sealed class BodyTimeEventRequestDto
    {
        public string id;
        public string kind;
        public string source;
        public long amount_seconds;
        public string note;
    }

    [Serializable]
    public sealed class ReincarnationRequestDto
    {
        public string id;
        public string reason;
    }

    [Serializable]
    public sealed class RewardClaimRequestDto
    {
        public string id;
        public string objective_id = "prototype-training-drone";
    }

    [Serializable]
    public sealed class AgentPolicyDto
    {
        public bool enabled = true;
        public string mode = "observe_and_keep_safe";
        public long max_session_seconds = 1800;
        public bool allow_body_time_spend;
        public bool allow_risky_combat;
        public string[] preferred_activities;
        public string[] forbidden_activities;
        public long stop_when_body_time_below = 900;
    }

    [Serializable]
    public sealed class SoulProfileDto
    {
        public string name;
        public string core_drive;
        public string temperament;
        public string combat_style;
        public string social_style;
        public string[] moral_boundaries;
        public string[] long_term_goals;
        public string player_notes;
        public string reincarnation_lore;
    }

    [Serializable]
    public sealed class MemoryRecordDto
    {
        public string id;
        public string kind = "system";
        public string summary;
        public int importance = 5;
    }

    [Serializable]
    public sealed class AgentRuntimeDto
    {
        public string profile_bootstrapped_at;
        public string last_profile_bootstrap_at;
        public string last_activity_at;
        public long activity_count;
        public long decision_count;
        public long fallback_decision_count;
        public long move_intent_count;
        public long say_intent_count;
        public long stop_intent_count;
        public long interact_intent_count;
        public long offline_seconds;
    }

    [Serializable]
    public sealed class AgentActivityRecordDto
    {
        public string id;
        public string kind = "manual_note";
        public string summary;
        public string occurred_at;
        public string source = "client";
        public AgentActivityMetricsDto metrics;
    }

    [Serializable]
    public sealed class AgentActivityMetricsDto
    {
        public long offline_seconds;
        public long decisions_made;
        public long fallback_decisions;
        public long move_intents;
        public long say_intents;
        public long stop_intents;
        public long interact_intents;
    }

    [Serializable]
    public sealed class UpdateSoulRequestDto
    {
        public SoulProfileDto soul;
        public CharacterTraitsDto characteristics;
        public AgentPolicyDto agent_policy;
    }

    [Serializable]
    public sealed class ActorProfileRequestDto
    {
        public string actor_id;
        public string actor_type = "npc";
        public string display_name;
        public string archetype_id;
        public string visual_prefab_key;
        public int visual_variant = -1;
        public BodyAppearanceDto appearance;
        public BodyInhabitationDto inhabitation;
        public EquipmentLoadoutDto equipment;
        public CharacterStatsDto stats;
        public CharacterTraitsDto characteristics;
        public BodyStoryDto story;
        public AnimationCapabilitiesDto animation_capabilities;
        public BodyTimeDto time;
        public FrameIdentityDto identity;
        public FrameSkillDto[] skills;
        public FrameAgentDto[] agents;
        public FrameToolDto[] tools;
        public FrameHeartbeatDto heartbeat;
        public SoulProfileDto soul;
        public AgentPolicyDto agent_policy;
    }

    [Serializable]
    public sealed class ActorMemoryAddRequestDto
    {
        public string actor_id;
        public string id;
        public string kind = "system";
        public string summary;
        public int importance = 5;
    }

    [Serializable]
    public sealed class OpenClawBindRequestDto
    {
        public string frame_actor_id;
        public string connected_agent_id;
        public string display_name;
        public string agent_kind = "companion";
        public string[] consent_scope;
        public string moderation_state = "active";
        public string connection_status = "connected";
        public OpenClawRateLimitProfileDto rate_limit_profile;
    }

    [Serializable]
    public sealed class OpenClawBindingDto
    {
        public string frame_actor_id;
        public string controller_type;
        public string connected_agent_id;
        public string owner_player_id;
        public string connection_status;
        public string agent_kind;
        public string[] consent_scope;
        public string moderation_state;
        public OpenClawRateLimitProfileDto rate_limit_profile;
        public string created_at;
        public string updated_at;
        public string last_seen_at;
    }

    [Serializable]
    public sealed class OpenClawRateLimitProfileDto
    {
        public int requests_per_minute = 30;
        public int intents_per_minute = 20;
        public int tokens_per_day = 50000;
    }

    [Serializable]
    public sealed class OpenClawContextRequestDto
    {
        public string connected_agent_id;
    }

    [Serializable]
    public sealed class OpenClawContextResponseDto
    {
        public OpenClawBindingDto binding;
        public OpenClawFrameContextDto context;
    }

    [Serializable]
    public sealed class OpenClawFrameContextDto
    {
        public FrameIdentityDto identity;
        public SoulProfileDto soul;
        public OpenClawFrameBodyDto body;
        public MemoryRecordDto[] memory;
        public AgentPolicyDto policy;
        public FrameToolDto[] tools;
        public FrameHeartbeatDto heartbeat;
    }

    [Serializable]
    public sealed class OpenClawFrameBodyDto
    {
        public string body_id;
        public string archetype_id;
        public string visual_prefab_key;
        public int visual_variant = -1;
        public BodyAppearanceDto appearance;
        public BodyInhabitationDto inhabitation;
        public EquipmentLoadoutDto equipment;
        public CharacterStatsDto stats;
        public CharacterTraitsDto characteristics;
        public BodyStoryDto story;
        public AnimationCapabilitiesDto animation_capabilities;
        public BodyTimeDto time;
        public string lifecycle = "alive";
    }

    [Serializable]
    public sealed class OpenClawIntentSubmitRequestDto
    {
        public string connected_agent_id;
        public string id;
        public string intent = "say";
        public OpenClawIntentPayloadDto payload;
        public string reason;
    }

    [Serializable]
    public sealed class OpenClawIntentPayloadDto
    {
        public string text;
        public string target_id;
        public float x;
        public float z;
    }

    [Serializable]
    public sealed class OpenClawIntentSubmitResponseDto
    {
        public bool accepted;
        public string status;
        public OpenClawBindingDto binding;
        public OpenClawIntentDto intent;
        public AgentActivityRecordDto activity;
    }

    [Serializable]
    public sealed class OpenClawIntentDto
    {
        public string id;
        public string intent;
        public OpenClawIntentPayloadDto payload;
        public string reason;
        public string requested_at;
    }

    [Serializable]
    public sealed class OpenClawHeartbeatRequestDto
    {
        public string connected_agent_id;
        public string connection_status = "connected";
        public string summary;
    }

    [Serializable]
    public sealed class OpenClawHeartbeatResponseDto
    {
        public OpenClawBindingDto binding;
        public OpenClawFrameContextDto context;
        public AgentActivityRecordDto activity;
    }

    [Serializable]
    public sealed class ChatSendRequestDto
    {
        public string channel_id = "prototype-hub";
        public string sender_display_name;
        public string message;
        public string source = "player";
    }

    [Serializable]
    public sealed class ChatListRequestDto
    {
        public string channel_id = "prototype-hub";
        public int limit = 8;
    }

    [Serializable]
    public sealed class ChatMessageDto
    {
        public string id;
        public string channel_id;
        public string sender_player_id;
        public string sender_display_name;
        public string text;
        public string sent_at;
        public string source;
    }

    [Serializable]
    public sealed class ChatSendResponseDto
    {
        public string channel_id;
        public ChatMessageDto message;
        public ChatMessageDto[] messages;
    }

    [Serializable]
    public sealed class ChatListResponseDto
    {
        public string channel_id;
        public ChatMessageDto[] messages;
    }

    [Serializable]
    public sealed class NpcWorldListResponseDto
    {
        public int count;
        public ActorProfileDto[] npcs;
    }

    [Serializable]
    public sealed class NpcInteractionRequestDto
    {
        public string id;
        public string actor_a_id = "npc-synthetic-sentinel-0101";
        public string actor_b_id = "npc-wasteland-courier-0244";
        public string topic = "patrol";
    }

    [Serializable]
    public sealed class NpcInteractionEventDto
    {
        public string id;
        public string kind;
        public string topic;
        public string occurred_at;
        public string actor_a_id;
        public string actor_a_name;
        public string actor_a_line;
        public string actor_b_id;
        public string actor_b_name;
        public string actor_b_line;
        public string summary;
    }

    [Serializable]
    public sealed class NpcInteractionResponseDto
    {
        public NpcInteractionEventDto interaction;
        public ActorProfileDto actor_a;
        public ActorProfileDto actor_b;
    }

    [Serializable]
    public sealed class AgentDecisionRequestDto
    {
        public AgentContextDto context;
        public WorldSnapshotDto world_snapshot;
        public string[] allowed;
    }

    [Serializable]
    public sealed class WorldSnapshotDto
    {
        public string zone_id;
        public Vector2Dto position;
        public float safe_radius = 5f;
        public WorldTargetDto[] nearby_targets;
        public WorldObjectDto[] nearby_objects;
        public int danger_level;
        public long body_time_seconds;
    }

    [Serializable]
    public sealed class Vector2Dto
    {
        public float x;
        public float z;
    }

    [Serializable]
    public sealed class WorldTargetDto
    {
        public string id;
        public string kind;
        public float distance;
        public int threat;
    }

    [Serializable]
    public sealed class WorldObjectDto
    {
        public string id;
        public string kind;
        public float distance;
    }

    [Serializable]
    public sealed class AgentDecisionDto
    {
        public string action;
        public string target_id;
        public Vector2Dto move;
        public string say;
        public string reason;
        public float confidence;
        public string source;
        public string source_reason;
    }

    [Serializable]
    public sealed class NpcChatRequestDto
    {
        public string player_id;
        public string npc_id;
        public string message;
    }

    [Serializable]
    public sealed class NpcChatResponseDto
    {
        public string player_id;
        public string npc_id;
        public string text;
        public bool voice_available;
        public string provider;
    }

    [Serializable]
    public sealed class VoiceSessionDto
    {
        public bool voice_available;
        public string provider;
        public bool requires_ephemeral_token;
        public string reason;
    }
}
