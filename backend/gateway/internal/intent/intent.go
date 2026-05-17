// Package intent defines the validated-intent contract between LLM
// outputs and game-state mutations.
//
// Per Hard Rule #2 (CLAUDE.md): "NEVER let LLM mutate authoritative game
// state. Server validates all intent."
//
// The flow:
//
//  1. NPC LLM responds with both natural-language dialogue AND a
//     structured intent (e.g. {"type": "grant_item", "item_id": "x"}).
//  2. The gateway parses the intent, validates it against current game
//     state (does the NPC have authority to grant this item? does the
//     player have a quest that requires this? is the item in inventory
//     limits?).
//  3. Only validated intents are forwarded to the game server (Photon
//     Fusion 2 dedicated) for actual state mutation.
//  4. The Unity client renders the dialogue but never trusts the intent.
//
// This package is the schema + validator. Provider implementations in
// internal/llm extract intent from raw model output; the game server
// applies validated intent.
package intent

// Intent is the structured action proposed by an LLM call.
// All intent types are explicit - we never accept free-form "do thing"
// from the model. New intent types require a code change + ADR.
type Intent struct {
	Type   IntentType        `json:"type"`
	Source string            `json:"source"` // NPC ID or "agent:<player_id>"
	Data   map[string]string `json:"data"`
}

type IntentType string

const (
	// IntentNPCGreet is a no-op intent - dialogue only, no state change.
	IntentNPCGreet IntentType = "npc_greet"

	// IntentNPCOfferQuest proposes that an NPC offers a quest to a player.
	// Validated against: NPC has authority for this quest; player meets
	// prerequisites (level, prior quests done, faction standing).
	IntentNPCOfferQuest IntentType = "npc_offer_quest"

	// IntentNPCGrantItem proposes that an NPC grants an item.
	// Validated against: quest in progress that requires this item;
	// inventory has space; item is on the NPC's grantable list.
	IntentNPCGrantItem IntentType = "npc_grant_item"

	// IntentAgentMove is the offline-AI-agent intent to move the player
	// character. Validated against: server-side pathfinding + zone bounds.
	IntentAgentMove IntentType = "agent_move"

	// IntentAgentAttack is the offline-AI-agent intent to attack a target.
	// Validated against: target in range; agent capability cap; cooldowns.
	IntentAgentAttack IntentType = "agent_attack"
)

// Validator checks whether an Intent is allowed given current game state.
// Implementations live in the game server, NOT in the gateway. The
// gateway only proxies the intent + an opaque world-state token; the
// game server fetches its authoritative state and validates.
type Validator interface {
	Validate(intent *Intent, worldStateToken string) (*ValidationResult, error)
}

type ValidationResult struct {
	Allowed bool
	Reason  string // human-readable rejection reason for logs (NEVER returned to client)
}
