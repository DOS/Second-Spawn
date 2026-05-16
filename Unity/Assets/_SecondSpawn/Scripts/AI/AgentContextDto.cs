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
    public sealed class PlayerProfileDto
    {
        public string player_id;
        public string display_name;
    }

    [Serializable]
    public sealed class BodyProfileDto
    {
        public string body_id;
        public string archetype_id;
        public string visual_prefab_key;
        public EquipmentLoadoutDto equipment;
        public CharacterStatsDto stats;
        public CharacterTraitsDto characteristics;
        public BodyTimeDto time;
        public CultivationDto cultivation;
        public string lifecycle = "alive";
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
    public sealed class BodyTimeEventRequestDto
    {
        public string id;
        public string kind;
        public string source;
        public long amount_seconds;
        public string note;
    }

    [Serializable]
    public sealed class CultivationDto
    {
        public string tier;
        public long progress_xp;
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
