// Package character defines the durable player-character profile contract used
// by the LLM gateway and the authoritative game server.
package character

import (
	"fmt"
	"sort"
	"strings"
	"time"
)

// PlayerProfile is durable account-level identity. It survives body death.
type PlayerProfile struct {
	PlayerID             string    `json:"player_id"`
	DisplayName          string    `json:"display_name"`
	SecondBalanceSeconds int64     `json:"second_balance_seconds"`
	ReincarnationCount   int64     `json:"reincarnation_count"`
	CreatedAt            time.Time `json:"created_at"`
}

// BodyProfile is the current synthetic body. It is replaced on reincarnation.
type BodyProfile struct {
	BodyID                string                `json:"body_id"`
	ArchetypeID           string                `json:"archetype_id"`
	VisualPrefabKey       string                `json:"visual_prefab_key"`
	VisualVariant         int                   `json:"visual_variant"`
	Appearance            BodyAppearance        `json:"appearance"`
	Inhabitation          BodyInhabitation      `json:"inhabitation"`
	Equipment             EquipmentLoadout      `json:"equipment"`
	Stats                 CharacterStats        `json:"stats"`
	Characteristics       CharacterTraits       `json:"characteristics"`
	Story                 BodyStory             `json:"story"`
	AnimationCapabilities AnimationCapabilities `json:"animation_capabilities"`
	Time                  BodyTimeState         `json:"time"`
	Lifecycle             BodyLifecycle         `json:"lifecycle"`
	AgentPolicy           AgentPolicy           `json:"agent_policy"`
	Soul                  SoulProfile           `json:"soul"`
	Memory                []MemoryRecord        `json:"memory"`
	AgentRuntime          AgentRuntime          `json:"agent_runtime"`
	AgentActivity         []AgentActivity       `json:"agent_activity"`
	CreatedAt             time.Time             `json:"created_at"`
}

type EquipmentLoadout struct {
	PrimaryWeapon     string `json:"primary_weapon"`
	EquipmentVisualID int    `json:"equipment_visual_id"`
	WeaponVisualKey   string `json:"weapon_visual_key"`
	WeaponFamily      string `json:"weapon_family"`
	CombatStance      string `json:"combat_stance"`
	Socket            string `json:"socket"`
}

// BodyAppearance is modular visual identity. visual_variant remains a
// prototype fallback while these fields become the durable body description.
type BodyAppearance struct {
	BodyType  string    `json:"body_type"`
	BodyParts BodyParts `json:"body_parts"`
	Skin      string    `json:"skin"`
	Hair      string    `json:"hair"`
	Material  string    `json:"material"`
	Marks     []string  `json:"marks"`
}

type BodyParts struct {
	Head  string `json:"head"`
	Face  string `json:"face"`
	Torso string `json:"torso"`
	Arms  string `json:"arms"`
	Legs  string `json:"legs"`
}

// BodyInhabitation records which NPC-like body the player currently inhabits.
type BodyInhabitation struct {
	SourceActorID     string `json:"source_actor_id"`
	PreviousRole      string `json:"previous_role"`
	InhabitedByPlayer bool   `json:"inhabited_by_player"`
	AssignedAt        string `json:"assigned_at"`
}

type CharacterStats struct {
	Level        int `json:"level"`
	Vitality     int `json:"vitality"`
	Force        int `json:"force"`
	Agility      int `json:"agility"`
	Focus        int `json:"focus"`
	Resilience   int `json:"resilience"`
	MaxHealth    int `json:"max_health"`
	MaxEnergy    int `json:"max_energy"`
	AttackPower  int `json:"attack_power"`
	DefensePower int `json:"defense_power"`
}

// CharacterTraits are stable personality/action tendencies for the LLM agent.
// They guide behavior only. They are not gameplay modifiers and never bypass
// server-side intent validation.
type CharacterTraits struct {
	Curiosity   int `json:"curiosity"`
	Courage     int `json:"courage"`
	Empathy     int `json:"empathy"`
	Discipline  int `json:"discipline"`
	Aggression  int `json:"aggression"`
	Sociability int `json:"sociability"`
}

// BodyStory is a short body-specific hook used for NPC/player onboarding.
type BodyStory struct {
	Origin   string `json:"origin"`
	Role     string `json:"role"`
	Conflict string `json:"conflict"`
	Rumor    string `json:"rumor"`
}

// AnimationCapabilities describe visual-only clip availability for this body.
type AnimationCapabilities struct {
	SupportsJump   bool   `json:"supports_jump"`
	SupportsRoll   bool   `json:"supports_roll"`
	SupportsMelee  bool   `json:"supports_melee"`
	SupportsRanged bool   `json:"supports_ranged"`
	WeaponStance   string `json:"weapon_stance"`
}

type BodyTimeState struct {
	RemainingSeconds int64 `json:"remaining_seconds"`
	MaxSeconds       int64 `json:"max_seconds"`
	DangerDrainRate  int64 `json:"danger_drain_rate"`
}

type BodyLifecycle string

const (
	BodyLifecycleAlive         BodyLifecycle = "alive"
	BodyLifecycleDying         BodyLifecycle = "dying"
	BodyLifecycleReincarnating BodyLifecycle = "reincarnating"
	BodyLifecycleDead          BodyLifecycle = "dead"
)

// AgentPolicy is player-controlled. The offline agent must not exceed it.
type AgentPolicy struct {
	Enabled               bool     `json:"enabled"`
	Mode                  string   `json:"mode"`
	MaxSessionSeconds     int64    `json:"max_session_seconds"`
	AllowBodyTimeSpend    bool     `json:"allow_body_time_spend"`
	AllowRiskyCombat      bool     `json:"allow_risky_combat"`
	PreferredActivities   []string `json:"preferred_activities"`
	ForbiddenActivities   []string `json:"forbidden_activities"`
	StopWhenBodyTimeBelow int64    `json:"stop_when_body_time_below"`
}

// SoulProfile is the stable identity layer read by the LLM agent.
// It is not a stat buff container. Gameplay bonuses stay in server-owned state.
type SoulProfile struct {
	Name              string   `json:"name"`
	CoreDrive         string   `json:"core_drive"`
	Temperament       string   `json:"temperament"`
	CombatStyle       string   `json:"combat_style"`
	SocialStyle       string   `json:"social_style"`
	MoralBoundaries   []string `json:"moral_boundaries"`
	LongTermGoals     []string `json:"long_term_goals"`
	PlayerNotes       string   `json:"player_notes"`
	ReincarnationLore string   `json:"reincarnation_lore"`
}

type MemoryKind string

const (
	MemoryKindPreference   MemoryKind = "preference"
	MemoryKindQuest        MemoryKind = "quest"
	MemoryKindRelationship MemoryKind = "relationship"
	MemoryKindCombat       MemoryKind = "combat"
	MemoryKindSystem       MemoryKind = "system"
)

// MemoryRecord is compact RAG input for the offline agent.
type MemoryRecord struct {
	ID         string     `json:"id"`
	Kind       MemoryKind `json:"kind"`
	Summary    string     `json:"summary"`
	Importance int        `json:"importance"`
	CreatedAt  time.Time  `json:"created_at"`
	UpdatedAt  time.Time  `json:"updated_at"`
}

// AgentRuntime tracks operational counters for the offline-agent prototype.
// These values are observability only and do not grant authoritative rewards.
type AgentRuntime struct {
	ProfileBootstrappedAt  string `json:"profile_bootstrapped_at"`
	LastProfileBootstrapAt string `json:"last_profile_bootstrap_at"`
	LastActivityAt         string `json:"last_activity_at"`
	ActivityCount          int64  `json:"activity_count"`
	DecisionCount          int64  `json:"decision_count"`
	FallbackDecisionCount  int64  `json:"fallback_decision_count"`
	MoveIntentCount        int64  `json:"move_intent_count"`
	SayIntentCount         int64  `json:"say_intent_count"`
	StopIntentCount        int64  `json:"stop_intent_count"`
	InteractIntentCount    int64  `json:"interact_intent_count"`
	OfflineSeconds         int64  `json:"offline_seconds"`
}

// AgentActivity is a compact recent activity entry from Nakama.
type AgentActivity struct {
	ID         string               `json:"id"`
	Kind       string               `json:"kind"`
	Summary    string               `json:"summary"`
	OccurredAt string               `json:"occurred_at"`
	Source     string               `json:"source"`
	Metrics    AgentActivityMetrics `json:"metrics"`
}

// AgentActivityMetrics carries optional counters reported with an activity.
type AgentActivityMetrics struct {
	OfflineSeconds    int64 `json:"offline_seconds"`
	DecisionsMade     int64 `json:"decisions_made"`
	FallbackDecisions int64 `json:"fallback_decisions"`
	MoveIntents       int64 `json:"move_intents"`
	SayIntents        int64 `json:"say_intents"`
	StopIntents       int64 `json:"stop_intents"`
	InteractIntents   int64 `json:"interact_intents"`
}

// AgentContext is the prompt-safe snapshot passed to an LLM provider.
type AgentContext struct {
	Player PlayerProfile `json:"player"`
	Body   BodyProfile   `json:"body"`
}

// BuildAgentContextPrompt returns a stable, bounded text block for an LLM.
func BuildAgentContextPrompt(ctx AgentContext, maxMemories int) string {
	memories := append([]MemoryRecord(nil), ctx.Body.Memory...)
	sort.SliceStable(memories, func(i, j int) bool {
		if memories[i].Importance == memories[j].Importance {
			return memories[i].UpdatedAt.After(memories[j].UpdatedAt)
		}
		return memories[i].Importance > memories[j].Importance
	})
	if maxMemories >= 0 && len(memories) > maxMemories {
		memories = memories[:maxMemories]
	}

	var b strings.Builder
	writeKV(&b, "player_id", ctx.Player.PlayerID)
	writeKV(&b, "display_name", ctx.Player.DisplayName)
	writeKV(&b, "second_balance_seconds", fmt.Sprintf("%d", ctx.Player.SecondBalanceSeconds))
	writeKV(&b, "reincarnation_count", fmt.Sprintf("%d", ctx.Player.ReincarnationCount))
	writeKV(&b, "body_id", ctx.Body.BodyID)
	writeKV(&b, "archetype_id", ctx.Body.ArchetypeID)
	writeKV(&b, "visual_prefab_key", ctx.Body.VisualPrefabKey)
	writeKV(&b, "visual_variant", fmt.Sprintf("%d", ctx.Body.VisualVariant))
	writeKV(&b, "appearance", formatBodyAppearance(ctx.Body.Appearance))
	writeKV(&b, "inhabitation", formatBodyInhabitation(ctx.Body.Inhabitation))
	writeKV(&b, "equipment", formatEquipment(ctx.Body.Equipment))
	writeKV(&b, "primary_weapon", ctx.Body.Equipment.PrimaryWeapon)
	writeKV(&b, "body_story", formatBodyStory(ctx.Body.Story))
	writeKV(&b, "supports_jump_animation", fmt.Sprintf("%t", ctx.Body.AnimationCapabilities.SupportsJump))
	writeKV(&b, "animation_capabilities", formatAnimationCapabilities(ctx.Body.AnimationCapabilities))
	writeKV(&b, "stats", fmt.Sprintf("level=%d vitality=%d force=%d agility=%d focus=%d resilience=%d max_health=%d max_energy=%d attack_power=%d defense_power=%d",
		ctx.Body.Stats.Level,
		ctx.Body.Stats.Vitality,
		ctx.Body.Stats.Force,
		ctx.Body.Stats.Agility,
		ctx.Body.Stats.Focus,
		ctx.Body.Stats.Resilience,
		ctx.Body.Stats.MaxHealth,
		ctx.Body.Stats.MaxEnergy,
		ctx.Body.Stats.AttackPower,
		ctx.Body.Stats.DefensePower,
	))
	writeKV(&b, "body_lifecycle", string(ctx.Body.Lifecycle))
	writeKV(&b, "body_time_seconds", fmt.Sprintf("%d/%d", ctx.Body.Time.RemainingSeconds, ctx.Body.Time.MaxSeconds))
	writeKV(&b, "traits", fmt.Sprintf("curiosity=%d courage=%d empathy=%d discipline=%d aggression=%d sociability=%d",
		ctx.Body.Characteristics.Curiosity,
		ctx.Body.Characteristics.Courage,
		ctx.Body.Characteristics.Empathy,
		ctx.Body.Characteristics.Discipline,
		ctx.Body.Characteristics.Aggression,
		ctx.Body.Characteristics.Sociability,
	))
	writeKV(&b, "agent_enabled", fmt.Sprintf("%t", ctx.Body.AgentPolicy.Enabled))
	writeKV(&b, "agent_mode", ctx.Body.AgentPolicy.Mode)
	writeKV(&b, "agent_stop_time_threshold", fmt.Sprintf("%d", ctx.Body.AgentPolicy.StopWhenBodyTimeBelow))
	writeKV(&b, "soul_name", ctx.Body.Soul.Name)
	writeKV(&b, "core_drive", ctx.Body.Soul.CoreDrive)
	writeKV(&b, "temperament", ctx.Body.Soul.Temperament)
	writeKV(&b, "combat_style", ctx.Body.Soul.CombatStyle)
	writeKV(&b, "social_style", ctx.Body.Soul.SocialStyle)
	writeKV(&b, "long_term_goals", strings.Join(ctx.Body.Soul.LongTermGoals, "; "))
	writeKV(&b, "moral_boundaries", strings.Join(ctx.Body.Soul.MoralBoundaries, "; "))
	writeKV(&b, "player_notes", ctx.Body.Soul.PlayerNotes)
	writeKV(&b, "memory_count", fmt.Sprintf("%d", len(memories)))

	for i, memory := range memories {
		writeKV(&b, fmt.Sprintf("memory_%02d_kind", i+1), string(memory.Kind))
		writeKV(&b, fmt.Sprintf("memory_%02d_summary", i+1), memory.Summary)
	}

	return strings.TrimSpace(b.String())
}

func formatBodyAppearance(appearance BodyAppearance) string {
	parts := []string{}
	appendKV := func(key string, value string) {
		if strings.TrimSpace(value) != "" {
			parts = append(parts, key+"="+strings.TrimSpace(value))
		}
	}
	appendKV("type", appearance.BodyType)
	appendKV("head", appearance.BodyParts.Head)
	appendKV("face", appearance.BodyParts.Face)
	appendKV("torso", appearance.BodyParts.Torso)
	appendKV("arms", appearance.BodyParts.Arms)
	appendKV("legs", appearance.BodyParts.Legs)
	appendKV("skin", appearance.Skin)
	appendKV("hair", appearance.Hair)
	appendKV("material", appearance.Material)
	if len(appearance.Marks) > 0 {
		parts = append(parts, "marks="+strings.Join(appearance.Marks, ", "))
	}
	return strings.Join(parts, "; ")
}

func formatBodyInhabitation(inhabitation BodyInhabitation) string {
	parts := []string{}
	if strings.TrimSpace(inhabitation.SourceActorID) != "" {
		parts = append(parts, "source_actor_id="+strings.TrimSpace(inhabitation.SourceActorID))
	}
	if strings.TrimSpace(inhabitation.PreviousRole) != "" {
		parts = append(parts, "previous_role="+strings.TrimSpace(inhabitation.PreviousRole))
	}
	parts = append(parts, fmt.Sprintf("inhabited_by_player=%t", inhabitation.InhabitedByPlayer))
	return strings.Join(parts, "; ")
}

func formatEquipment(equipment EquipmentLoadout) string {
	parts := []string{}
	appendKV := func(key string, value string) {
		if strings.TrimSpace(value) != "" {
			parts = append(parts, key+"="+strings.TrimSpace(value))
		}
	}
	appendKV("primary_weapon", equipment.PrimaryWeapon)
	parts = append(parts, fmt.Sprintf("visual_id=%d", equipment.EquipmentVisualID))
	appendKV("visual_key", equipment.WeaponVisualKey)
	appendKV("family", equipment.WeaponFamily)
	appendKV("stance", equipment.CombatStance)
	appendKV("socket", equipment.Socket)
	return strings.Join(parts, "; ")
}

func formatBodyStory(story BodyStory) string {
	parts := []string{}
	if strings.TrimSpace(story.Origin) != "" {
		parts = append(parts, "origin="+strings.TrimSpace(story.Origin))
	}
	if strings.TrimSpace(story.Role) != "" {
		parts = append(parts, "role="+strings.TrimSpace(story.Role))
	}
	if strings.TrimSpace(story.Conflict) != "" {
		parts = append(parts, "conflict="+strings.TrimSpace(story.Conflict))
	}
	if strings.TrimSpace(story.Rumor) != "" {
		parts = append(parts, "rumor="+strings.TrimSpace(story.Rumor))
	}
	return strings.Join(parts, "; ")
}

func formatAnimationCapabilities(capabilities AnimationCapabilities) string {
	stance := strings.TrimSpace(capabilities.WeaponStance)
	parts := []string{
		fmt.Sprintf("jump=%t", capabilities.SupportsJump),
		fmt.Sprintf("roll=%t", capabilities.SupportsRoll),
		fmt.Sprintf("melee=%t", capabilities.SupportsMelee),
		fmt.Sprintf("ranged=%t", capabilities.SupportsRanged),
	}
	if stance != "" {
		parts = append(parts, "stance="+stance)
	}
	return strings.Join(parts, "; ")
}

func writeKV(b *strings.Builder, key string, value string) {
	if strings.TrimSpace(value) == "" {
		return
	}
	b.WriteString(key)
	b.WriteString(": ")
	b.WriteString(strings.TrimSpace(value))
	b.WriteByte('\n')
}
