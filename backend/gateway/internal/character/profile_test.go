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
			VisualVariant:   9,
			Appearance: BodyAppearance{
				BodyType: "synthetic_hunter",
				BodyParts: BodyParts{
					Head:  "crossline-optic-head",
					Face:  "masked-rangefinder-face",
					Torso: "ranged-survey-torso",
					Arms:  "steady-ranged-arms",
					Legs:  "survey-runner-legs",
				},
				Skin:     "graphite",
				Material: "matte-carbon",
				Marks:    []string{"signal-burn", "survey-chevron"},
			},
			Inhabitation: BodyInhabitation{
				SourceActorID:     "npc-crossline-hunter-4445",
				PreviousRole:      "Ranged survey body",
				InhabitedByPlayer: true,
			},
			Equipment: EquipmentLoadout{
				PrimaryWeapon:     "one_hand_sword",
				EquipmentVisualID: 2,
				WeaponVisualKey:   "sword",
				WeaponFamily:      "melee",
				CombatStance:      "one_hand_melee",
				Socket:            "right_hand",
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
			Story: BodyStory{
				Origin:   "A recovered perimeter hunter body.",
				Role:     "Ranged survey body",
				Conflict: "It trusts patterns more than people.",
				Rumor:    "Its optics still receive a signal from a silent district.",
			},
			AnimationCapabilities: AnimationCapabilities{
				SupportsJump:   false,
				SupportsRoll:   true,
				SupportsMelee:  true,
				SupportsRanged: false,
				WeaponStance:   "one_hand_melee",
			},
			Lifecycle: BodyLifecycleAlive,
			Time: BodyTimeState{
				RemainingSeconds: 3600,
				MaxSeconds:       7200,
			},
			Identity: FrameIdentity{
				PublicName:        "Crossline Surveyor 4445",
				Callsign:          "npc-crossline-hunter-4445",
				PublicRole:        "Ranged survey body",
				FactionTitle:      "Relay Runner",
				Profession:        "Perimeter scout",
				AgeYears:          34,
				AgeBand:           "adult",
				HomeBase:          "Gate Seraph Hub",
				ReputationSummary: "Known for safe route work.",
			},
			Skills: []FrameSkill{
				{ID: "skill-scout", Name: "Perimeter Scout", Category: "profession", Rank: 2, Summary: "Can read threat patterns around the hub."},
			},
			Agents: []FrameAgent{
				{
					ID:                  "agent-offline-player",
					Mode:                "offline_player_agent",
					Priority:            1,
					Routine:             "Scout low-risk routes and preserve BodyTime.",
					AllowedActivities:   []string{"explore", "safe_farming"},
					ForbiddenActivities: []string{"start_pvp"},
				},
			},
			Tools: []FrameTool{
				{Name: "move", Category: "intent", Intent: "move", RequiresValidation: true},
			},
			Heartbeat: FrameHeartbeat{
				CadenceSeconds:      60,
				OfflineSessionState: "online",
				LastActionSummary:   "Standing near the hub gate.",
				FallbackState:       "none",
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
	if !strings.Contains(prompt, "home_base=Gate Seraph Hub") {
		t.Fatalf("expected home base in prompt, got %s", prompt)
	}
	if !strings.Contains(prompt, "visual_variant: 9") {
		t.Fatalf("expected visual variant in prompt, got %s", prompt)
	}
	if !strings.Contains(prompt, "role=Ranged survey body") {
		t.Fatalf("expected body story in prompt, got %s", prompt)
	}
	if !strings.Contains(prompt, "appearance: type=synthetic_hunter; head=crossline-optic-head; face=masked-rangefinder-face; torso=ranged-survey-torso; arms=steady-ranged-arms; legs=survey-runner-legs; skin=graphite; material=matte-carbon; marks=signal-burn, survey-chevron") {
		t.Fatalf("expected modular appearance in prompt, got %s", prompt)
	}
	if !strings.Contains(prompt, "inhabitation: source_actor_id=npc-crossline-hunter-4445; previous_role=Ranged survey body; inhabited_by_player=true") {
		t.Fatalf("expected body inhabitation in prompt, got %s", prompt)
	}
	if !strings.Contains(prompt, "equipment: primary_weapon=one_hand_sword; visual_id=2; visual_key=sword; family=melee; stance=one_hand_melee; socket=right_hand") {
		t.Fatalf("expected equipment metadata in prompt, got %s", prompt)
	}
	if !strings.Contains(prompt, "supports_jump_animation: false") {
		t.Fatalf("expected animation capability in prompt, got %s", prompt)
	}
	if !strings.Contains(prompt, "animation_capabilities: jump=false; roll=true; melee=true; ranged=false; stance=one_hand_melee") {
		t.Fatalf("expected expanded animation capabilities in prompt, got %s", prompt)
	}
	if !strings.Contains(prompt, "stats: level=2 vitality=12 force=9 agility=11 focus=8 resilience=10 max_health=140 max_energy=60 attack_power=15 defense_power=7") {
		t.Fatalf("expected body stats in prompt, got %s", prompt)
	}
	if !strings.Contains(prompt, "frame_identity: public_name=Crossline Surveyor 4445; callsign=npc-crossline-hunter-4445; role=Ranged survey body; faction_title=Relay Runner; profession=Perimeter scout; age_years=34; age_band=adult; home_base=Gate Seraph Hub; reputation=Known for safe route work.") {
		t.Fatalf("expected frame identity in prompt, got %s", prompt)
	}
	if !strings.Contains(prompt, "frame_skills: Perimeter Scout(profession,r2): Can read threat patterns around the hub.") {
		t.Fatalf("expected frame skills in prompt, got %s", prompt)
	}
	if !strings.Contains(prompt, "frame_agents: offline_player_agent priority=1 routine=Scout low-risk routes and preserve BodyTime.") {
		t.Fatalf("expected frame agents in prompt, got %s", prompt)
	}
	if !strings.Contains(prompt, "frame_tools: move->move validation=true") {
		t.Fatalf("expected frame tools in prompt, got %s", prompt)
	}
	if !strings.Contains(prompt, "frame_heartbeat: cadence=60s; state=online; last_action=Standing near the hub gate.; fallback=none") {
		t.Fatalf("expected frame heartbeat in prompt, got %s", prompt)
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
