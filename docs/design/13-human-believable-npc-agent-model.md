# Human-Believable NPC Agent Model

*Status: Design direction*
*Created: 2026-05-18*
*Author: Codex*

> **Quick reference** - Layer: `AI Agent / Character Design` - Priority: `MVP foundation` - Key deps: `Frame profile`, `NPC agent brain`, `Nakama`, `api.dos.ai`, `Fusion authority`

---

## Purpose

SECOND SPAWN should aim for NPCs that feel as close to human-believable as the
MVP stack can support, not merely functional quest dispensers. The project
should test the real limits of LLM-driven agents through play, while preserving
server authority over movement, combat, inventory, TIME, SECOND, quests, and
any other gameplay mutation.

The goal is not to hardcode behavior. The goal is to give each NPC enough
stable identity, personality, memory, relationships, needs, mood, and world
context for an LLM or external agent harness to choose believable intent inside
validated game limits.

Design target:

```text
Human-believable agent behavior
+ game-owned social and memory state
+ bounded LLM context
+ server-validated intent
```

---

## Research Anchors

This model uses recent LLM-agent and game-AI references, but keeps the result
small enough for a multiplayer ARPG prototype.

The primary anchor is the 2026 SSRN paper on hierarchical memory for
LLM-based game NPCs. It is the closest fit because it directly targets
persistent personality, context-efficient retrieval, and multi-NPC game
deployment. Other references are supporting sources used only where the primary
paper is thin: relationship gameplay, trait vocabulary, symbolic social state,
and broad LLM-agent sanity checks.

| Reference | Role In SECOND SPAWN |
| ---- | ---- |
| [Hierarchical Memory Consolidation and Context-Efficient Retrieval for Persistent Personality in LLM-Based Game NPCs](https://papers.ssrn.com/sol3/papers.cfm?abstract_id=6536138) | **Primary anchor.** Use short-term, episodic, and core memory, plus personality-aware consolidation and graph retrieval. Treat as the closest-fit 2026 game-NPC reference, while remembering it is still a preprint. |
| [Slice of Life: Social Physics with Symbolically Grounded LLM-Based Generative Dialogue](https://www.kmjn.org/publications/SliceOfLife_FDG24-abstract.html) | Supporting source for grounding LLM dialogue in symbolic social state while preserving game design control. |
| [Prom Week](https://ojs.aaai.org/index.php/AIIDE/article/view/12662) / Comme il Faut | Supporting classic source for playable social relationships. Use the lesson that relationship state is game state. Do not use it as the modern LLM brain architecture. |
| [Dwarf Fortress personality facets](https://dwarffortresswiki.org/index.php/DF2014:Personality_facet) | Supporting source for numeric personality scales, values, needs, and emergent social friction. |
| [Crusader Kings III traits](https://ck3.paradoxwikis.com/Traits) | Supporting source for readable roleplay trait naming such as honest, deceitful, brave, ambitious, compassionate, callous, vengeful, and forgiving. |
| [RimWorld traits](https://rimworldwiki.com/index.php?title=Traits) | Supporting source for traits that create routine differences, conflict, attachment, and settlement drama. |
| [Generative Agents: Interactive Simulacra of Human Behavior](https://arxiv.org/abs/2304.03442) | Supporting source for observation, reflection, retrieval, and planning. Do not copy full life-simulation scope. |
| [A Survey on Large Language Model based Autonomous Agents](https://link.springer.com/article/10.1007/s11704-024-40231-1) | Supporting sanity check for modern LLM-agent components such as profile, memory, planning, action, and tool use. |
| [A Survey on the Memory Mechanism of LLM-based Agents](https://arxiv.org/abs/2404.13501) | Supporting sanity check for memory types, retrieval, reflection, and compression choices. |

---

## Design Position

SECOND SPAWN should not define a new backend layer just because an external
agent system has a similar file. The existing Frame layers are enough for MVP:

| Frame Layer | Human-Believable Role |
| ---- | ---- |
| `FrameIdentity` | Public self: name, callsign, profession, faction, reputation, social mask |
| `FrameSoul` | Deep self: drive, fear, desire, values, moral lines, contradictions |
| `FrameBody` | Physical self: current vessel, stats, BodyTime, lifecycle, visual identity |
| `BodyPresentation` | Perceived self: appeal band, visual tags, intimidation tags, and presentation style |
| `CharacterTraits` | Stable numeric tendency vector used by backend and LLM prompts |
| `FrameMemory` | Lived self: short-term memory, episodic memories, core reflections, relationship facts |
| `Relationships` | Per-target social state such as affinity, trust, fear, respect, debt, and hostility |
| `FramePolicy` | Allowed risk, intent limits, moderation, owner constraints, and safety rules |
| `FrameHeartbeat` | Runtime state, current goal, mood, stress, last action, failure and fallback status |
| `FrameTools` | Request schema only. These are not executable tools and never bypass validation. |

If the layers become insufficient later, the first likely additions are:

- `FrameNeeds`: persistent drives such as safety, belonging, recognition, debt
  resolution, curiosity, repair, vengeance, and BodyTime security.
- `FrameEmotion`: short-lived affect state such as calm, anxious, angry,
  ashamed, lonely, proud, protective, or suspicious.
- `FrameRelationship`: a first-class relationship table if relationship facts
  outgrow compact `FrameMemory` records.

For MVP, these can be represented as structured fields inside `CharacterTraits`,
`FrameMemory`, and `FrameHeartbeat` before introducing new storage layers.

---

## Identity, Presentation, and Social Surface

Human-believable NPCs need identity and presentation data, but those fields
should not be mixed into combat stats.

`FrameIdentity` should carry stable public identity:

- name
- callsign
- profession
- faction
- role
- reputation summary
- gender identity
- pronouns
- identity age or soul continuity age when known

`FrameBody` should carry body-specific facts:

- body ID
- visual variant
- apparent age
- chronological body age
- body sex marker or synthetic marker
- lifecycle
- BodyTime
- body-bound story hook

`BodyPresentation` should carry passive social surface:

| Field | Meaning |
| ---- | ---- |
| `appeal_band` | Passive first-impression band such as `low`, `normal`, `notable`, `high`, or `exceptional`. |
| `appeal_tags` | Why the actor is appealing, such as `elegant`, `warm`, `synthetic-perfect`, `cute`, or `mysterious`. |
| `visual_tags` | Objective visual read such as `scarred`, `military-grade`, `clean-silhouette`, or `uncanny`. |
| `intimidation_tags` | Threat read such as `armed`, `massive`, `boss-like`, or `damaged`. |
| `presentation_style` | Broad style such as masculine, feminine, androgynous, military, elegant, or utilitarian. |

`Appeal` is not a core stat and should not be treated as "beauty score".
It is a presentation attribute that can shape initial reactions and LLM flavor
inside backend-approved social rules. It should never replace `charisma`, unlock
rewards by itself, or bypass consent and moderation.

Do not add `presence` as a buildable stat. If the game needs a presence label,
derive it from charisma, reputation, body scale, gear, visual threat, and the
current social context.

---

## Character Trait Vector

`CharacterTraits` are stable personality and action tendencies. They guide LLM
decisions and deterministic fallbacks, but they are not direct stat buffs and
never bypass server-side intent validation.

Use `0..100` for each trait. High and low values should both be meaningful.
The values describe tendencies, not moral labels.

### Recommended MVP Traits

| Trait | Other Common Names | Game Meaning |
| ---- | ---- | ---- |
| `empathy` | compassion, kindness, warmth | How strongly the actor notices and cares about another actor's suffering. |
| `honesty` | integrity, truthfulness, deceit inverse | How likely the actor is to tell the truth, keep promises, and avoid manipulation. |
| `cunning` | guile, intrigue, manipulation, deceit | How likely the actor is to scheme, misdirect, hide motives, or exploit social openings. |
| `loyalty` | devotion, allegiance, honor | How strongly the actor protects a person, faction, oath, crew, or ideal. |
| `ambition` | hunger, aspiration, drive | How strongly the actor seeks status, power, discovery, influence, or escape from their station. |
| `self_preservation` | survival instinct, caution | How strongly the actor protects its own body, BodyTime, and future prospects. |
| `courage` | bravery, valor, nerve | How likely the actor is to face danger despite fear. |
| `discipline` | self-control, diligence, order | How strongly the actor follows routines, plans, duties, and self-restraint. |
| `aggression` | wrath, dominance, hostility | How readily the actor escalates to threats, pressure, combat, or intimidation. |
| `mercy` | forgiveness, restraint, clemency | How likely the actor is to spare, forgive, rescue, or de-escalate when it has advantage. |
| `curiosity` | openness, wonder, knowledge drive | How strongly the actor investigates, asks questions, explores, and tests unknown systems. |
| `sociability` | gregariousness, charisma, expressiveness | How often the actor initiates conversation, joins groups, and maintains social contact. |
| `paranoia` | suspicion, vigilance, distrust | How strongly the actor expects betrayal, traps, hidden motives, and unseen threats. |
| `greed` | acquisitiveness, materialism, hoarding | How strongly the actor seeks resources, BodyTime security, equipment, or leverage. |
| `pragmatism` | realism, utilitarianism, expedience | How willing the actor is to make morally ugly choices for practical outcomes. |
| `vengefulness` | grudge, retribution, wrath | How strongly the actor remembers injury and seeks payback. |

### Why Not Use `good_evil`

Do not add a single `good_evil` axis. It flattens characters and makes social
behavior less believable. Morality should emerge from trait combinations,
memory, relationships, and current pressure.

Examples:

| Pattern | Trait Shape |
| ---- | ---- |
| Principled protector | High empathy, honesty, loyalty, mercy, and discipline |
| Practical savior | High empathy and pragmatism, medium honesty, low mercy toward threats |
| Court schemer | High cunning, ambition, discipline, and paranoia, low honesty |
| Violent survivor | High aggression and self-preservation, low empathy and mercy |
| Tragic loyalist | High loyalty and vengefulness, high discipline, low trust |

### Trait Tags

Use optional `trait_tags` for readable flavor that does not require new numeric
fields. Tags should be useful to writers, prompts, and debug UI.

Examples:

- `oathbound`
- `bodytime-hoarder`
- `debt-ridden`
- `scarred-medic`
- `gate-loyalist`
- `blackout-survivor`
- `memory-broker`
- `failed-protector`
- `rank-hungry`
- `quiet-heretic`

---

## Relationship Ledger

Relationships should be first-class game state once NPC social behavior becomes
important. For MVP, the current affinity and hostility fields are a good start,
but they are not enough for human-believable social behavior.

Recommended per-target relationship fields:

| Field | Range | Meaning |
| ---- | ---- | ---- |
| `affinity` | `-100..100` | General liking, warmth, or personal pull toward the target. |
| `hostility` | `0..100` | Active dislike, resentment, or desire to avoid or harm the target. |
| `trust` | `0..100` | Belief that the target will act honestly or reliably. |
| `fear` | `0..100` | Perceived danger, intimidation, or emotional fear. |
| `respect` | `0..100` | Recognition of competence, rank, sacrifice, or moral force. |
| `debt` | `-100..100` | Social or material obligation. Positive means this actor owes the target. Negative means the target owes this actor. |
| `familiarity` | `0..100` | Amount of shared contact and history. |
| `affection` | `0..100` | Warmth, care, attachment, or romantic pull where content rules allow it. |
| `attachment` | `0..100` | Emotional bond that can survive conflict. |
| `rivalry` | `0..100` | Competitive tension that can coexist with respect or affinity. |
| `last_tone` | enum | Last interaction tone such as `friendly`, `tense`, `hostile`, `intimate`, `transactional`, or `protective`. |
| `tags` | list | Relationship facts such as `mentor`, `rival`, `saved-by-target`, `debtor`, `suspect`, `old-crew`, or `betrayed`. |
| `memory_refs` | list | Memory record IDs that justify the current relationship state. |

MVP minimum:

- `affinity`
- `hostility`
- `trust`
- `fear`
- `respect`
- `debt`
- `familiarity`

Do not collapse this into one `like_score`. A character can love someone they
distrust, respect someone they hate, fear someone they need, or owe a debt to a
rival. Multiple axes make NPC behavior more believable and give the LLM better
bounded context without granting it authority.

Relationship state should be updated only through validated game events and
approved memory updates. LLM output may propose an update, but the backend owns
whether it is accepted.

---

## Memory Shape

Use a three-tier memory model for NPCs and offline player agents.

| Memory Tier | Content | Lifetime | Prompt Use |
| ---- | ---- | ---- | ---- |
| Short-term memory | Recent observations, local speech, immediate action outcomes, nearby actor context | Minutes to one session | High priority and high recency |
| Episodic memory | Compressed events such as fights, favors, betrayals, promises, discoveries, debts, and failed duties | Days to weeks | Retrieved when actor, place, topic, or emotion matches |
| Core memory | Stable self beliefs, traumas, values, contradictions, long-term fears, and durable relationship conclusions | Long-term, crosses reincarnation only by explicit rules | Small, always or often included |

Memory consolidation should be personality-aware. When summarizing an event,
the consolidation prompt should include:

- `CharacterTraits`
- `FrameSoul`
- relationship state
- current role and faction
- whether BodyTime, survival, debt, trust, or moral boundaries were involved

The goal is to prevent personality erosion. A cautious medic and an ambitious
broker should remember the same event differently.

---

## Needs, Mood, And Stress

To push NPCs closer to human-believable behavior, MVP should track lightweight
internal state. These are not new authoritative reward systems. They are
behavior context and observability.

### Needs

Needs are persistent pressures that make NPCs initiate action without a player
prompt.

Recommended need categories:

| Need | Meaning |
| ---- | ---- |
| `safety` | Avoid danger, secure the body, preserve BodyTime. |
| `belonging` | Maintain crew, faction, friendship, and social recognition. |
| `status` | Gain respect, rank, reputation, or leverage. |
| `purpose` | Fulfill role duty, oath, craft, research, or protection. |
| `curiosity` | Investigate unknown zones, bodies, rumors, and system anomalies. |
| `repair` | Fix broken bodies, machines, promises, or social damage. |
| `vengeance` | Seek justice or payback for remembered harm. |
| `atonement` | Reduce guilt from past failure or betrayal. |
| `bodytime_security` | Seek, conserve, steal, earn, or negotiate for TIME measured in SECOND. |

### Mood

Mood is short-lived. It affects tone and initiative, not authority.

Suggested values:

- `calm`
- `anxious`
- `angry`
- `lonely`
- `hopeful`
- `ashamed`
- `proud`
- `protective`
- `suspicious`
- `exhausted`

### Stress

Stress is a numeric pressure value. It can rise from low BodyTime, danger,
betrayal, repeated failure, isolation, or moral conflict. High stress should
increase fallback to safe behavior, defensive speech, mistakes, avoidance, or
urgent social bids.

---

## Proactive Communication

Proactive communication should be selected by the LLM or agent harness using
state, not by hardcoded character scripts.

Decision inputs:

```text
FramePolicy
+ nearby actor list
+ relationship ledger
+ CharacterTraits
+ FrameSoul
+ FrameMemory
+ current needs
+ mood and stress
+ BodyTime state
+ local danger
+ allowed intents
```

Backend hard gates:

- actors must be nearby according to authoritative world state
- the intent must be allowlisted
- the target must be valid
- social cooldown and spam limits must pass
- BodyTime and danger limits must pass
- moderation and consent limits must pass

LLM decision examples:

| State | Likely Human-Believable Intent |
| ---- | ---- |
| High sociability, high affinity, shared memory | Start a warm check-in or mention the shared event. |
| High cunning, high ambition, low trust | Ask a leading question, flatter, bargain, or probe for information. |
| High loyalty, ally in danger | Warn, approach, defend, or ask for retreat. |
| High paranoia, low familiarity, unknown actor nearby | Keep distance, ask a suspicious question, or call for backup. |
| High vengefulness, high hostility, high fear | Avoid direct combat, threaten indirectly, or seek an ally. |
| High empathy, target low BodyTime | Offer help, ask what happened, or suggest a safe route. |
| High greed, low honesty, target has resources | Negotiate, mislead, or request a trade-like interaction within policy. |

The output remains a structured intent such as `say`, `approach`, `warn`,
`avoid`, `request_help`, `trade_request`, `escort_request`, `share_rumor`, or
`stop`. Early MVP can validate only `say`, `move`, `interact`, `attack`, and
`stop`, while preserving richer reason tags for future expansion.

---

## Human-Believability Acceptance Tests

These tests are design targets for playtests and local simulation. They are not
all required before the first vertical slice.

- Same event, different personalities: two NPCs witness the same danger and
  produce different but plausible reactions.
- Same NPC, changed relationship: an NPC speaks differently to a target after a
  favor, betrayal, rescue, or debt.
- Memory continuity: an NPC can reference a prior event without raw transcript
  stuffing.
- Contradiction: an NPC can show inner conflict, such as loyalty to a faction
  while privately distrusting a leader.
- BodyTime pressure: low BodyTime changes priorities and tone without making
  every NPC identical.
- Reincarnation boundary: body-bound memory and durable soul memory separate
  cleanly according to approved carryover rules.
- Social initiative: NPCs sometimes initiate action when no player prompts
  them, but only when policy, relationship, proximity, and world state allow it.
- Validation: no believable speech can directly grant items, TIME, SECOND,
  quest completion, level, or combat outcomes.

---

## MVP Agent Characteristic Sheet

Every important NPC or inhabitable Frame should have a compact characteristic
sheet. The sheet is not a single prompt blob. It is a mix of backend-owned
numbers, bounded text context, and runtime world state.

| Section | Backend Owned Fields | LLM Context Fields |
| ---- | ---- | ---- |
| Identity | `actor_id`, `body_id`, `controller_type`, `zone_id`, `faction_id`, `role_id`, gender identity, pronouns, identity age | public name, callsign, profession, faction summary, reputation, social mask |
| Body | level, HP, energy, stats, BodyTime seconds, lifecycle, visual variant, equipment key, apparent age, body marker | appearance summary, body condition, body origin, inherited role |
| Presentation | appeal band, appearance tags, intimidation tags, presentation style | first-impression cues and visual-social flavor |
| Traits | normalized numeric trait vector | trait tags and a short behavior summary |
| Soul | durable soul record ID and approved version | core drive, core fear, moral boundaries, social style, long-term goals |
| Memory | memory IDs, kinds, importance, timestamps, retrieval tags | short memory summaries, unresolved promises, trauma, rumors, grudges |
| Relationships | affinity, hostility, trust, fear, respect, debt, familiarity, affection, rivalry, cooldowns | relationship notes, known history, last tone, social hooks |
| Policy | allowed intents, denied intents, risk thresholds, rate limits, moderation state | soft guidance such as preferred activities, forbidden topics, role obligations |
| Runtime | current goal ID, mood, stress, last action, failure count, model source | current situation summary and last decision reason |
| World | authoritative position, nearby actors, danger, interactables | local situation summary, zone social rules, event hooks |

Rule of thumb:

- Numbers decide authority, balance, validation, and persistence.
- Text gives the agent a mind and a voice.
- Runtime state explains what is happening right now.
- LLM output remains intent only.

### Proactive Communication Ownership

Proactive communication is not owned only by `Memory` or only by `Soul`.

The intended decision path is:

```text
WorldContext detects nearby actors
-> Relationships rank who matters
-> AgentPolicy decides whether initiating is allowed
-> SoulProfile supplies motive and voice
-> Memory supplies specific content and history
-> LLM selects a structured intent
-> Gateway, Nakama, and Fusion validate
```

Examples:

| State | Expected Agent Choice |
| ---- | ---- |
| High sociability, positive affinity, safe zone | Initiate a short in-character check-in. |
| High empathy, target has low BodyTime | Ask what happened or suggest a safer route. |
| High paranoia, unknown actor nearby | Keep distance or ask a guarded question. |
| High loyalty, ally near danger | Warn, approach, or request retreat. |
| High cunning, low trust | Probe for information without exposing the real motive. |
| High hostility | Avoid, threaten, or request backup depending on policy and danger. |

Hardcoded scripts may provide fallback smoke tests only. They must not become
the primary NPC brain.

## Starter NPC Archetypes

These archetypes are seed material for permanent NPC profiles. They should be
implemented as structured profiles plus prompt context, not as scripted
dialogue trees.

| Archetype | Profession | Trait Shape | Soul Drive | Memory Seed | Relationship Hook |
| ---- | ---- | ---- | ---- | ---- | ---- |
| Gate Sentinel | Perimeter guard | High loyalty, discipline, perception, low trust | Keep the gate standing after a past breach | A friend did not return during a gate failure | Respects couriers, distrusts scavengers |
| Route Courier | Runner between safe nodes | High agility, sociability, pragmatism | Prove that speed is still freedom | Knows abandoned paths and missing names | Builds affinity through route intel |
| Clinic Operator | Field medic and memory triage worker | High empathy, willpower, discipline, low aggression | Save personhood, not only bodies | Keeps fragments from reincarnated patients | Protects damaged bodies, dislikes reckless fighters |
| Scrap Warden | Salvage foreman | High resilience, territorial loyalty, blunt social style | Turn ruins into shelter | Lost a crew to a bad salvage call | Respects useful workers, hates thieves |
| Crossline Surveyor | Boundary mapmaker | High curiosity, perception, self-preservation | Make the world legible before it changes again | Saw a zone boundary move | Trades secrets for protection |
| Signal Marksman | Overwatch and relay | High patience, attack, perception, low sociability | Never let allies die unseen | Watched an ambush unfold too late | Bonds with sentinels and couriers |
| Memory Broker | Rumor and identity-fragment trader | High social, cunning, ambition, low honesty | Own the truth before someone edits it | Knows a secret about a retired body | Creates debt and suspicion |
| Frame Shepherd | Empty-body caretaker | High patience, ritual, empathy, low aggression | Bodies deserve dignity even without occupants | Heard an empty body speak once | Central to reincarnation and body ethics |
| Blackout Mechanic | Power and body-rig repair worker | High willpower, pragmatism, anti-authority | Keep systems running because leaders fail | Survived a station blackout by locking others out | Clashes with authority, helps under pressure |
| Debt Chaplain | BodyTime debt mediator | High social, willpower, honesty, unsettling calm | Make every owed second mean something | Carries final messages from retired bodies | Good trigger for emotional NPC interactions |

---

## MVP Implementation Guidance

Do not implement all research ideas at once. The first practical slice should
add depth where it changes visible NPC behavior.

1. Expand `CharacterTraits` from the current 6 fields toward the recommended
   trait vector.
2. Add a minimal `RelationshipLedger` for persistent NPC relationships:
   affinity, hostility, trust, fear, respect, debt, and familiarity.
3. Add lightweight needs, mood, and stress into runtime or memory context before
   creating new storage layers.
4. Add memory tier metadata: `short_term`, `episodic`, or `core`.
5. Make proactive `say` decisions use nearby actors, relationship state,
   traits, soul, memory, BodyTime, and policy.
6. Keep all gameplay changes server-validated.
7. Measure prompt size and retrieval quality before adding more fields.

---

## Open Questions

Ranked questions for JOY:

1. How far should body-bound personality survive when a player leaves or kills
   a Frame?
2. Should a player-inhabited Frame keep the original NPC soul as a suppressed
   passenger, a merged influence, or a retired identity?
3. Should relationship scores be visible to players, visible only in debug UI,
   or hidden entirely?
4. Should NPCs be allowed to form new relationships with each other while no
   player is nearby?
5. How aggressive should NPCs be about BodyTime scarcity: negotiate, beg,
   steal, betray, or only ask?
6. Should a connected OpenClaw agent be allowed to propose memory and
   relationship updates, or only read them?
7. Which traits should be public character flavor and which should remain
   hidden backend truth?
