package character

import (
	"strings"
	"testing"
	"time"
)

func TestBuildAgentContextPromptSortsAndBoundsMemories(t *testing.T) {
	now := time.Date(2026, 5, 15, 12, 0, 0, 0, time.UTC)
	ctx := AgentContext{
		Player: PlayerProfile{
			PlayerID:    "player-1",
			DisplayName: "JOY",
		},
		Body: BodyProfile{
			BodyID:          "body-1",
			ArchetypeID:     "hunter-default",
			VisualPrefabKey: "rpg-character",
			Equipment: EquipmentLoadout{
				PrimaryWeapon:     "one_hand_sword",
				EquipmentVisualID: 2,
			},
			Stats: CharacterStats{
				Level:        2,
				Vitality:     12,
				Force:        9,
				Agility:      11,
				Focus:        8,
				Resilience:   10,
				MaxHealth:    140,
				MaxEnergy:    60,
				AttackPower:  15,
				DefensePower: 7,
			},
			Lifecycle: BodyLifecycleAlive,
			Time: BodyTimeState{
				RemainingSeconds: 3600,
				MaxSeconds:       7200,
			},
			AgentPolicy: AgentPolicy{
				Enabled:               true,
				Mode:                  "farm_safe_area",
				StopWhenBodyTimeBelow: 900,
			},
			Soul: SoulProfile{
				Name:            "Second Spawn Test Soul",
				CoreDrive:       "survive long enough to regain agency",
				Temperament:     "cautious",
				CombatStyle:     "kite enemies",
				SocialStyle:     "brief and practical",
				LongTermGoals:   []string{"survive the next expedition"},
				MoralBoundaries: []string{"do not betray allies"},
				PlayerNotes:     "avoid unnecessary risk",
			},
			Memory: []MemoryRecord{
				{Kind: MemoryKindCombat, Summary: "Low value memory", Importance: 1, UpdatedAt: now},
				{Kind: MemoryKindQuest, Summary: "Critical quest memory", Importance: 9, UpdatedAt: now.Add(-time.Hour)},
				{Kind: MemoryKindPreference, Summary: "Recent preference memory", Importance: 5, UpdatedAt: now.Add(time.Hour)},
			},
		},
	}

	prompt := BuildAgentContextPrompt(ctx, 2)

	if !strings.Contains(prompt, "player_id: player-1") {
		t.Fatalf("expected player id in prompt, got %s", prompt)
	}
	if !strings.Contains(prompt, "memory_01_summary: Critical quest memory") {
		t.Fatalf("expected highest importance memory first, got %s", prompt)
	}
	if !strings.Contains(prompt, "primary_weapon: one_hand_sword") {
		t.Fatalf("expected equipment in prompt, got %s", prompt)
	}
	if !strings.Contains(prompt, "stats: level=2 vitality=12 force=9 agility=11 focus=8 resilience=10 max_health=140 max_energy=60 attack_power=15 defense_power=7") {
		t.Fatalf("expected body stats in prompt, got %s", prompt)
	}
	if !strings.Contains(prompt, "memory_02_summary: Recent preference memory") {
		t.Fatalf("expected second bounded memory, got %s", prompt)
	}
	if strings.Contains(prompt, "Low value memory") {
		t.Fatalf("expected low value memory to be excluded, got %s", prompt)
	}
}

func TestBuildAgentContextPromptOmitsEmptyFields(t *testing.T) {
	prompt := BuildAgentContextPrompt(AgentContext{
		Player: PlayerProfile{PlayerID: "player-1"},
		Body:   BodyProfile{BodyID: "body-1"},
	}, 5)

	if strings.Contains(prompt, "display_name:") {
		t.Fatalf("expected empty display name to be omitted, got %s", prompt)
	}
	if !strings.Contains(prompt, "player_id: player-1") {
		t.Fatalf("expected non-empty field, got %s", prompt)
	}
}
