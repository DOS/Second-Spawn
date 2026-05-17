package character

import (
	"context"
	"errors"
	"fmt"
	"sort"
	"strings"
	"sync"
	"time"
)

var (
	ErrPlayerIDRequired = errors.New("player_id is required")
	ErrMemoryInvalid    = errors.New("memory record is invalid")
)

type Store interface {
	GetOrCreateContext(ctx context.Context, playerID string) (AgentContext, error)
	UpdateSoul(ctx context.Context, playerID string, soul SoulProfile, traits CharacterTraits, policy AgentPolicy) (AgentContext, error)
	AddMemory(ctx context.Context, playerID string, memory MemoryRecord) (AgentContext, error)
}

type MemoryStore struct {
	mu       sync.RWMutex
	profiles map[string]AgentContext
	now      func() time.Time
}

func NewMemoryStore() *MemoryStore {
	return &MemoryStore{
		profiles: make(map[string]AgentContext),
		now:      func() time.Time { return time.Now().UTC() },
	}
}

func (s *MemoryStore) GetOrCreateContext(ctx context.Context, playerID string) (AgentContext, error) {
	if err := ctx.Err(); err != nil {
		return AgentContext{}, err
	}
	playerID = normalizeID(playerID)
	if playerID == "" {
		return AgentContext{}, ErrPlayerIDRequired
	}

	s.mu.Lock()
	defer s.mu.Unlock()

	if existing, ok := s.profiles[playerID]; ok {
		return existing, nil
	}

	profile := NewDefaultAgentContext(playerID, s.now())
	s.profiles[playerID] = profile
	return profile, nil
}

func (s *MemoryStore) UpdateSoul(ctx context.Context, playerID string, soul SoulProfile, traits CharacterTraits, policy AgentPolicy) (AgentContext, error) {
	if err := ctx.Err(); err != nil {
		return AgentContext{}, err
	}
	playerID = normalizeID(playerID)
	if playerID == "" {
		return AgentContext{}, ErrPlayerIDRequired
	}

	s.mu.Lock()
	defer s.mu.Unlock()

	profile, ok := s.profiles[playerID]
	if !ok {
		profile = NewDefaultAgentContext(playerID, s.now())
	}

	profile.Body.Soul = normalizeSoul(soul, profile.Player.DisplayName)
	profile.Body.Characteristics = clampTraits(traits)
	profile.Body.AgentPolicy = normalizePolicy(policy)
	s.profiles[playerID] = profile
	return profile, nil
}

func (s *MemoryStore) AddMemory(ctx context.Context, playerID string, memory MemoryRecord) (AgentContext, error) {
	if err := ctx.Err(); err != nil {
		return AgentContext{}, err
	}
	playerID = normalizeID(playerID)
	if playerID == "" {
		return AgentContext{}, ErrPlayerIDRequired
	}

	s.mu.Lock()
	defer s.mu.Unlock()

	profile, ok := s.profiles[playerID]
	if !ok {
		profile = NewDefaultAgentContext(playerID, s.now())
	}

	now := s.now()
	memory.Kind = normalizeMemoryKind(memory.Kind)
	memory.Summary = strings.TrimSpace(memory.Summary)
	if memory.Summary == "" {
		return AgentContext{}, fmt.Errorf("%w: summary is required", ErrMemoryInvalid)
	}
	if memory.Importance < 1 {
		memory.Importance = 1
	}
	if memory.Importance > 10 {
		memory.Importance = 10
	}

	for i := range profile.Body.Memory {
		existing := &profile.Body.Memory[i]
		if existing.Kind == memory.Kind && strings.EqualFold(strings.TrimSpace(existing.Summary), memory.Summary) {
			if memory.Importance > existing.Importance {
				existing.Importance = memory.Importance
			}
			existing.UpdatedAt = now
			profile.Body.Memory = sortAndBoundMemories(profile.Body.Memory)
			s.profiles[playerID] = profile
			return profile, nil
		}
	}

	if memory.ID == "" {
		memory.ID = fmt.Sprintf("mem-%d", now.UnixNano())
	}
	if memory.CreatedAt.IsZero() {
		memory.CreatedAt = now
	}
	memory.UpdatedAt = now

	profile.Body.Memory = append(profile.Body.Memory, memory)
	profile.Body.Memory = sortAndBoundMemories(profile.Body.Memory)

	s.profiles[playerID] = profile
	return profile, nil
}

func sortAndBoundMemories(memories []MemoryRecord) []MemoryRecord {
	sort.SliceStable(memories, func(i, j int) bool {
		if memories[i].Importance == memories[j].Importance {
			return memories[i].UpdatedAt.After(memories[j].UpdatedAt)
		}
		return memories[i].Importance > memories[j].Importance
	})
	if len(memories) > 64 {
		return memories[:64]
	}
	return memories
}

func NewDefaultAgentContext(playerID string, now time.Time) AgentContext {
	displayName := playerID
	if displayName == "" {
		displayName = "Unknown Wanderer"
	}

	return AgentContext{
		Player: PlayerProfile{
			PlayerID:             playerID,
			DisplayName:          displayName,
			SecondBalanceSeconds: 7 * 24 * 60 * 60,
			ReincarnationCount:   0,
			CreatedAt:            now,
		},
		Body: BodyProfile{
			BodyID:          "body-" + playerID,
			ArchetypeID:     "prototype-hunter",
			VisualPrefabKey: "prototype-random",
			Equipment: EquipmentLoadout{
				PrimaryWeapon:     "none",
				EquipmentVisualID: 0,
			},
			Stats: CharacterStats{
				Level:        1,
				Vitality:     10,
				Force:        8,
				Agility:      8,
				Focus:        8,
				Resilience:   8,
				MaxHealth:    100,
				MaxEnergy:    50,
				AttackPower:  10,
				DefensePower: 5,
			},
			Characteristics: CharacterTraits{
				Curiosity:   6,
				Courage:     5,
				Empathy:     5,
				Discipline:  5,
				Aggression:  3,
				Sociability: 5,
			},
			Time: BodyTimeState{
				RemainingSeconds: 24 * 60 * 60,
				MaxSeconds:       24 * 60 * 60,
				DangerDrainRate:  1,
			},
			Lifecycle: BodyLifecycleAlive,
			AgentPolicy: AgentPolicy{
				Enabled:               true,
				Mode:                  "observe_and_keep_safe",
				MaxSessionSeconds:     30 * 60,
				AllowBodyTimeSpend:    false,
				AllowRiskyCombat:      false,
				PreferredActivities:   []string{"explore", "talk", "safe_farming"},
				ForbiddenActivities:   []string{"spend_body_time", "start_pvp", "trade_items"},
				StopWhenBodyTimeBelow: 15 * 60,
			},
			Soul: SoulProfile{
				Name:              displayName,
				CoreDrive:         "survive, learn the zone, and preserve agency for the player",
				Temperament:       "careful but curious",
				CombatStyle:       "avoid risky fights, kite when threatened",
				SocialStyle:       "brief, grounded, and helpful",
				MoralBoundaries:   []string{"do not betray allies", "do not spend scarce resources without permission"},
				LongTermGoals:     []string{"survive the next expedition", "build trusted relationships with NPCs"},
				PlayerNotes:       "prototype default soul",
				ReincarnationLore: "a synthetic body carrying a persistent consciousness imprint",
			},
			Memory: []MemoryRecord{
				{
					ID:         "seed-origin",
					Kind:       MemoryKindSystem,
					Summary:    "The character is a Second Spawn prototype body controlled by the player or their offline agent.",
					Importance: 6,
					CreatedAt:  now,
					UpdatedAt:  now,
				},
			},
			CreatedAt: now,
		},
	}
}

func normalizeID(value string) string {
	return strings.TrimSpace(value)
}

func normalizeSoul(soul SoulProfile, fallbackName string) SoulProfile {
	if strings.TrimSpace(soul.Name) == "" {
		soul.Name = fallbackName
	}
	if strings.TrimSpace(soul.CoreDrive) == "" {
		soul.CoreDrive = "survive and protect player agency"
	}
	return soul
}

func normalizePolicy(policy AgentPolicy) AgentPolicy {
	if strings.TrimSpace(policy.Mode) == "" {
		policy.Mode = "observe_and_keep_safe"
	}
	if policy.MaxSessionSeconds <= 0 {
		policy.MaxSessionSeconds = 30 * 60
	}
	if policy.StopWhenBodyTimeBelow <= 0 {
		policy.StopWhenBodyTimeBelow = 15 * 60
	}
	return policy
}

func clampTraits(traits CharacterTraits) CharacterTraits {
	traits.Curiosity = clampTrait(traits.Curiosity)
	traits.Courage = clampTrait(traits.Courage)
	traits.Empathy = clampTrait(traits.Empathy)
	traits.Discipline = clampTrait(traits.Discipline)
	traits.Aggression = clampTrait(traits.Aggression)
	traits.Sociability = clampTrait(traits.Sociability)
	return traits
}

func clampTrait(value int) int {
	if value < 1 {
		return 1
	}
	if value > 10 {
		return 10
	}
	return value
}

func normalizeMemoryKind(kind MemoryKind) MemoryKind {
	switch kind {
	case MemoryKindPreference, MemoryKindQuest, MemoryKindRelationship, MemoryKindCombat, MemoryKindSystem:
		return kind
	default:
		return MemoryKindSystem
	}
}
