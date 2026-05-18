// SECOND SPAWN Nakama runtime entrypoint.
//
// Nakama is the game backend. This module owns game-backend extensions such as
// health checks, Supabase-backed custom authentication, player profile, soul,
// policy, and compact memory. AI/LLM provider calls stay in api.dos.ai.

var collectionAgent = "secondspawn_agent";
var keyAgentContext = "context";
var collectionActor = "secondspawn_actor";
var collectionOpenClaw = "secondspawn_openclaw";
var collectionChat = "secondspawn_chat";

var rpcIdHealth = "secondspawn_health";
var rpcIdProfileGet = "secondspawn_profile_get";
var rpcIdMemoryAdd = "secondspawn_memory_add";
var rpcIdSoulUpdate = "secondspawn_soul_update";
var rpcIdAgentDecide = "secondspawn_agent_decide";
var rpcIdAgentActivityAdd = "secondspawn_agent_activity_add";
var rpcIdActorProfileGet = "secondspawn_actor_profile_get";
var rpcIdActorMemoryAdd = "secondspawn_actor_memory_add";
var rpcIdBodyTimeEvent = "secondspawn_bodytime_event";
var rpcIdReincarnate = "secondspawn_reincarnate";
var rpcIdOpenClawBind = "secondspawn_openclaw_bind";
var rpcIdOpenClawContextGet = "secondspawn_openclaw_context_get";
var rpcIdOpenClawIntentSubmit = "secondspawn_openclaw_intent_submit";
var rpcIdOpenClawHeartbeat = "secondspawn_openclaw_heartbeat";
var rpcIdChatSend = "secondspawn_chat_send";
var rpcIdChatList = "secondspawn_chat_list";
var rpcIdRewardClaim = "secondspawn_reward_claim";
var rpcIdNpcSeed = "secondspawn_npc_seed";
var rpcIdNpcList = "secondspawn_npc_list";
var rpcIdNpcInteract = "secondspawn_npc_interact";
var rpcIdNpcContextGet = "secondspawn_npc_context_get";
var rpcIdNpcIntentSubmit = "secondspawn_npc_intent_submit";
var agentActivityLogLimit = 32;
var chatMessageLogLimit = 64;
var chatMessageMaxLength = 240;
var npcInteractionMaxDistanceMeters = 12;
var npcRelationshipMinAffinityForFrequent = 20;
var npcHostilityBlockThreshold = 80;
var npcFrequentInteractionCount = 3;
var agentRuntimeMetricMax = 1000000000;
var actorIdMaxLength = 56;
var bodyTimeMaxSeconds = 86400 * 30;
var bodyTimeEarnCapSeconds = 3600;
var bodyTimeSpendCapSeconds = 600;
var bodyTimeDrainCapSeconds = 300;
var bodyTimeEarnCooldownSeconds = 60;
var bodyTimeDebugFatalDrainSource = "prototype_reincarnation_debug";
var secondPrototypeMaxBalanceSeconds = 86400 * 365;
var secondPrototypeStartingBalanceSeconds = 86400 * 7;
var secondPrototypeReincarnationCostSeconds = 86400 * 5;
var dosAiDecisionBackoffSeconds = 30;
var dosAiDecisionMaxTokens = 96;
var dosAiDecisionMemoryCap = 3;
var prototypeVisualVariantMax = 17;
var bodyArchetypePool = [
  {
    archetype_id: "synthetic-sentinel",
    visual_prefab_key: "generated_visual_07_knight",
    visual_variant: 7,
    equipment_visual_id: 2,
    appearance: {
      body_type: "synthetic_hunter",
      body_parts: {
        head: "sentinel-visor-head",
        face: "sealed-guard-face",
        torso: "armored-sentinel-torso",
        arms: "shield-braced-arms",
        legs: "patrol-stabilizer-legs"
      },
      skin: "steel-blue",
      hair: "none",
      material: "brushed-alloy",
      marks: ["cordon-stripe", "vault-scar"]
    },
    stats: { vitality: 12, force: 9, agility: 7, focus: 7, resilience: 11, max_health: 120, max_energy: 45, attack_power: 11, defense_power: 8 },
    characteristics: { curiosity: 4, courage: 8, empathy: 5, discipline: 8, aggression: 4, sociability: 4 },
    soul: {
      core_drive: "hold the line for weaker bodies entering the zone",
      temperament: "patient, watchful, and duty-bound",
      combat_style: "guard allies, use measured melee pressure, and avoid overextending",
      social_style: "short, direct, and protective",
      long_term_goals: ["map safe routes through the hub perimeter", "earn trust as a reliable escort"]
    },
    story: {
      origin: "A patrol chassis recovered from a ruined security cordon.",
      role: "Frontline guard body",
      conflict: "Its old command routines still prioritize civilians over self-preservation.",
      rumor: "Some hub survivors claim this line once guarded a sealed Nibirium vault."
    },
    animation_capabilities: { supports_jump: true, supports_roll: true, supports_melee: true, supports_ranged: false, weapon_stance: "one_hand_melee" },
    seed_memory_summary: "This body remembers standing watch near the southern gate during a Nibirium storm."
  },
  {
    archetype_id: "wasteland-courier",
    visual_prefab_key: "generated_visual_03_ninja",
    visual_variant: 3,
    equipment_visual_id: 2,
    appearance: {
      body_type: "synthetic_hunter",
      body_parts: {
        head: "courier-wrap-head",
        face: "half-mask-runner-face",
        torso: "light-courier-torso",
        arms: "quickdraw-runner-arms",
        legs: "spring-fiber-legs"
      },
      skin: "dark-graphite",
      hair: "short-black",
      material: "flex-carbon",
      marks: ["route-scratch", "safehouse-tag"]
    },
    stats: { vitality: 8, force: 8, agility: 12, focus: 8, resilience: 7, max_health: 90, max_energy: 60, attack_power: 10, defense_power: 4 },
    characteristics: { curiosity: 8, courage: 7, empathy: 5, discipline: 6, aggression: 5, sociability: 7 },
    soul: {
      core_drive: "stay mobile, gather rumors, and deliver promises before the body burns out",
      temperament: "restless, alert, and hard to corner",
      combat_style: "kite threats, strike only when escape routes are open",
      social_style: "fast, pragmatic, and slightly suspicious",
      long_term_goals: ["connect isolated survivor pockets", "keep a private map of reliable safehouses"]
    },
    story: {
      origin: "A messenger body assembled from lightweight synthetic muscle and black-market reflex firmware.",
      role: "Scout and courier body",
      conflict: "It carries delivery fragments for clients who may no longer be alive.",
      rumor: "A courier with this imprint once crossed the dead belt without losing a second of BodyTime."
    },
    animation_capabilities: { supports_jump: true, supports_roll: true, supports_melee: true, supports_ranged: false, weapon_stance: "one_hand_melee" },
    seed_memory_summary: "This body remembers hidden route markers scratched under broken street lights."
  },
  {
    archetype_id: "clinic-operator",
    visual_prefab_key: "generated_visual_08_mage",
    visual_variant: 8,
    equipment_visual_id: 8,
    appearance: {
      body_type: "synthetic_hunter",
      body_parts: {
        head: "clinic-sensor-head",
        face: "soft-medical-face",
        torso: "field-clinic-torso",
        arms: "precision-medic-arms",
        legs: "quiet-step-legs"
      },
      skin: "warm-synthetic",
      hair: "white-bob",
      material: "clean-polymer",
      marks: ["clinic-band", "recovery-seal"]
    },
    stats: { vitality: 9, force: 6, agility: 7, focus: 12, resilience: 8, max_health: 95, max_energy: 80, attack_power: 8, defense_power: 5 },
    characteristics: { curiosity: 7, courage: 5, empathy: 9, discipline: 8, aggression: 2, sociability: 8 },
    soul: {
      core_drive: "stabilize damaged bodies and preserve identity continuity",
      temperament: "gentle, clinical, and quietly stubborn",
      combat_style: "avoid direct duels, support allies, and retreat before BodyTime becomes critical",
      social_style: "calm, observant, and reassuring",
      long_term_goals: ["build a registry of successful consciousness transfers", "learn why some memories survive better than others"]
    },
    story: {
      origin: "A field medic body assigned to a failing resurrection clinic.",
      role: "Support and researcher body",
      conflict: "It cannot ignore injured strangers, even when the clock says to run.",
      rumor: "The clinic kept one forbidden backup of a patient who never woke."
    },
    animation_capabilities: { supports_jump: true, supports_roll: false, supports_melee: false, supports_ranged: true, weapon_stance: "staff_caster" },
    seed_memory_summary: "This body remembers the smell of coolant in an underground reincarnation ward."
  },
  {
    archetype_id: "scrap-warden",
    visual_prefab_key: "generated_visual_10_hammer",
    visual_variant: 10,
    equipment_visual_id: 9,
    appearance: {
      body_type: "synthetic_hunter",
      body_parts: {
        head: "scrap-warden-head",
        face: "reinforced-jaw-face",
        torso: "heavy-salvage-torso",
        arms: "hydraulic-lifter-arms",
        legs: "wide-stance-legs"
      },
      skin: "oxide-brown",
      hair: "none",
      material: "scarred-iron",
      marks: ["debt-notch", "workshop-burn"]
    },
    stats: { vitality: 13, force: 12, agility: 5, focus: 6, resilience: 12, max_health: 135, max_energy: 35, attack_power: 13, defense_power: 9 },
    characteristics: { curiosity: 5, courage: 8, empathy: 4, discipline: 7, aggression: 6, sociability: 3 },
    soul: {
      core_drive: "protect salvage rights and keep predators away from the weak",
      temperament: "blunt, territorial, and loyal after trust is earned",
      combat_style: "hold ground with heavy swings and avoid chase-heavy fights",
      social_style: "terse, skeptical, and practical",
      long_term_goals: ["claim a safe workshop", "recover a lost repair rig from the scrapyard"]
    },
    story: {
      origin: "A reinforced labor body rebuilt for combat after the collapse.",
      role: "Heavy salvage body",
      conflict: "Its reinforced frame is powerful but less agile than newer shells.",
      rumor: "Scrap wardens mark debts on weapon handles instead of ledgers."
    },
    animation_capabilities: { supports_jump: false, supports_roll: false, supports_melee: true, supports_ranged: false, weapon_stance: "heavy_melee" },
    seed_memory_summary: "This body remembers defending a scrap claim through three nights of low BodyTime."
  },
  {
    archetype_id: "crossline-hunter",
    visual_prefab_key: "generated_visual_09_crossbow",
    visual_variant: 9,
    equipment_visual_id: 7,
    appearance: {
      body_type: "synthetic_hunter",
      body_parts: {
        head: "crossline-optic-head",
        face: "masked-rangefinder-face",
        torso: "ranged-survey-torso",
        arms: "steady-ranged-arms",
        legs: "survey-runner-legs"
      },
      skin: "graphite",
      hair: "cropped-silver",
      material: "matte-carbon",
      marks: ["signal-burn", "survey-chevron"]
    },
    stats: { vitality: 9, force: 10, agility: 9, focus: 9, resilience: 7, max_health: 100, max_energy: 60, attack_power: 12, defense_power: 5 },
    characteristics: { curiosity: 6, courage: 6, empathy: 4, discipline: 9, aggression: 5, sociability: 4 },
    soul: {
      core_drive: "observe threats before acting and never waste a shot",
      temperament: "quiet, precise, and slow to trust",
      combat_style: "keep distance, prioritize exposed targets, and disengage from melee pressure",
      social_style: "minimal, dry, and exact",
      long_term_goals: ["catalog dangerous mutations", "find the source of a repeating signal beyond the hub"]
    },
    story: {
      origin: "A hunter body calibrated for perimeter work and long sightlines.",
      role: "Ranged survey body",
      conflict: "It trusts patterns more than people.",
      rumor: "Its optical stack still receives a signal from a district that should be silent."
    },
    animation_capabilities: { supports_jump: false, supports_roll: true, supports_melee: false, supports_ranged: true, weapon_stance: "ranged_crossbow" },
    seed_memory_summary: "This body remembers counting hostile silhouettes from a broken overpass."
  }
];

var permanentNpcFramePool = [
  { npc_id: "npc-synthetic-sentinel-0101", display_name: "Gate Sentinel 0101", archetype_id: "synthetic-sentinel", role: "Frontline guard body", visual_variant: 7, visual_prefab_key: "generated_visual_07_knight", equipment_visual_id: 2 },
  { npc_id: "npc-wasteland-courier-0244", display_name: "Route Courier 0244", archetype_id: "wasteland-courier", role: "Scout and courier body", visual_variant: 3, visual_prefab_key: "generated_visual_03_ninja", equipment_visual_id: 2 },
  { npc_id: "npc-clinic-operator-0320", display_name: "Clinic Operator 0320", archetype_id: "clinic-operator", role: "Support and researcher body", visual_variant: 8, visual_prefab_key: "generated_visual_08_mage", equipment_visual_id: 8 },
  { npc_id: "npc-scrap-warden-0441", display_name: "Scrap Warden 0441", archetype_id: "scrap-warden", role: "Heavy salvage body", visual_variant: 10, visual_prefab_key: "generated_visual_10_hammer", equipment_visual_id: 9 },
  { npc_id: "npc-crossline-hunter-5104", display_name: "Crossline Surveyor 5104", archetype_id: "crossline-hunter", role: "Ranged survey body", visual_variant: 9, visual_prefab_key: "generated_visual_09_crossbow", equipment_visual_id: 7 },
  { npc_id: "npc-synthetic-sentinel-0627", display_name: "Gate Sentinel 0627", archetype_id: "synthetic-sentinel", role: "Frontline guard body", visual_variant: 16, visual_prefab_key: "generated_visual_16_male_fighter", equipment_visual_id: 2 },
  { npc_id: "npc-wasteland-courier-0733", display_name: "Route Courier 0733", archetype_id: "wasteland-courier", role: "Scout and courier body", visual_variant: 14, visual_prefab_key: "generated_visual_14_female_fighter", equipment_visual_id: 2 },
  { npc_id: "npc-clinic-operator-0819", display_name: "Clinic Operator 0819", archetype_id: "clinic-operator", role: "Support and researcher body", visual_variant: 17, visual_prefab_key: "generated_visual_17_crafter", equipment_visual_id: 1 },
  { npc_id: "npc-scrap-warden-0940", display_name: "Scrap Warden 0940", archetype_id: "scrap-warden", role: "Heavy salvage body", visual_variant: 15, visual_prefab_key: "generated_visual_15_heavy_fighter", equipment_visual_id: 9 },
  { npc_id: "npc-crossline-hunter-1058", display_name: "Crossline Surveyor 1058", archetype_id: "crossline-hunter", role: "Ranged survey body", visual_variant: 6, visual_prefab_key: "generated_visual_06_archer", equipment_visual_id: 6 }
];

var permanentNpcProfileOverrides: any = {
  "npc-synthetic-sentinel-0101": {
    identity: { public_name: "Gate Sentinel 0101", callsign: "SENT-0101", public_role: "South gate guard", faction_title: "South Gate Watch", profession: "checkpoint defender", age_years: 41, age_band: "adult", home_base: "South Gate", reputation_summary: "Known for letting refugees pass before merchants when BodyTime storms hit the gate." },
    stats: { level: 4, vitality: 13, force: 10, agility: 7, focus: 7, resilience: 12, max_health: 132, max_energy: 46, attack_power: 12, defense_power: 9 },
    characteristics: { curiosity: 4, courage: 9, empathy: 6, discipline: 9, aggression: 4, sociability: 4 },
    soul: { name: "Sentinel-0101 Echo", core_drive: "keep the south gate open for bodies that still have time left", temperament: "steady, suspicious of shortcuts, and protective under pressure", combat_style: "hold a narrow line, intercept threats, and call for help before chasing", social_style: "brief, formal, and quietly kind to exhausted travelers", long_term_goals: ["map every safe gate route", "find who sabotaged the old gate sensors"], player_notes: "Permanent NPC seed for gate defense behavior." },
    story: { origin: "Recovered from a collapsed checkpoint where it kept broadcasting safe-entry codes.", role: "South gate guard", conflict: "Its gate protocol conflicts with black-market orders to close access during scarcity.", rumor: "0101 still knows a hidden maintenance tunnel beneath the south wall." },
    memory: [{ id: "memory-gate-storm", kind: "system", summary: "0101 remembers holding the south gate during a red BodyTime storm while three convoy clocks hit zero.", importance: 8 }]
  },
  "npc-wasteland-courier-0244": {
    identity: { public_name: "Route Courier 0244", callsign: "ROUTE-0244", public_role: "Dead-belt runner", faction_title: "Free Courier Line", profession: "courier scout", age_years: 28, age_band: "young adult", home_base: "Underpass Relay", reputation_summary: "Fast, expensive, and nearly impossible to corner when a route goes bad." },
    stats: { level: 3, vitality: 8, force: 8, agility: 13, focus: 8, resilience: 7, max_health: 92, max_energy: 66, attack_power: 10, defense_power: 4 },
    characteristics: { curiosity: 9, courage: 7, empathy: 5, discipline: 6, aggression: 5, sociability: 8 },
    soul: { name: "Route-0244 Spark", core_drive: "deliver messages before their senders vanish", temperament: "restless, bright, and allergic to cages", combat_style: "break line of sight, bait pursuit, and never fight in dead ends", social_style: "quick jokes, faster exits, and careful promises", long_term_goals: ["rebuild the courier route map", "verify whether Station K is still alive"], player_notes: "Permanent NPC seed for route gossip and scout behavior." },
    story: { origin: "Built for courier guild work and later patched with scavenged sprint actuators.", role: "Dead-belt runner", conflict: "Carries sealed delivery fragments from clients who may be dead.", rumor: "0244 can read old route marks that no newer body recognizes." },
    memory: [{ id: "memory-underpass-relay", kind: "system", summary: "0244 remembers a safehouse underpass where the lights blink in courier code.", importance: 7 }]
  },
  "npc-clinic-operator-0320": {
    identity: { public_name: "Clinic Operator 0320", callsign: "CLINIC-0320", public_role: "Memory triage medic", faction_title: "Reincarnation Ward", profession: "field clinician", age_years: 36, age_band: "adult", home_base: "Basement Ward C", reputation_summary: "Keeps calm around failing bodies and refuses to abandon damaged memory imprints." },
    stats: { level: 4, vitality: 9, force: 6, agility: 7, focus: 13, resilience: 8, max_health: 98, max_energy: 86, attack_power: 8, defense_power: 5 },
    characteristics: { curiosity: 8, courage: 5, empathy: 10, discipline: 8, aggression: 2, sociability: 8 },
    soul: { name: "Clinic-0320 Mercy", core_drive: "preserve identity continuity when bodies fail", temperament: "gentle, clinical, and impossible to rush", combat_style: "avoid duels, protect patients, and disengage when BodyTime is low", social_style: "quiet questions, careful reassurance, and precise warnings", long_term_goals: ["catalog transfer failures", "find the missing Ward C backups"], player_notes: "Permanent NPC seed for medical support and memory triage." },
    story: { origin: "Assigned to a basement clinic that kept operating after the official network went dark.", role: "Memory triage medic", conflict: "Knows one forbidden backup protocol and is afraid to use it.", rumor: "0320 once stabilized a body with only nine seconds left." },
    memory: [{ id: "memory-ward-c", kind: "system", summary: "0320 remembers the sound of failing coolant pumps in Ward C during a transfer blackout.", importance: 8 }]
  },
  "npc-scrap-warden-0441": {
    identity: { public_name: "Scrap Warden 0441", callsign: "WARDEN-0441", public_role: "Salvage foreman", faction_title: "Iron Yard Claim", profession: "salvage warden", age_years: 49, age_band: "older adult", home_base: "Iron Yard", reputation_summary: "Pays debts slowly, protects workers fiercely, and never forgets stolen tools." },
    stats: { level: 5, vitality: 14, force: 13, agility: 5, focus: 6, resilience: 13, max_health: 145, max_energy: 36, attack_power: 14, defense_power: 10 },
    characteristics: { curiosity: 5, courage: 9, empathy: 5, discipline: 8, aggression: 6, sociability: 3 },
    soul: { name: "Warden-0441 Iron", core_drive: "keep the Iron Yard useful and safe from predators", temperament: "blunt, territorial, and loyal after proof", combat_style: "anchor the front, punish overcommitment, and avoid long chases", social_style: "few words, hard terms, direct respect", long_term_goals: ["recover the lost hydraulic forge", "settle an old debt with the Bone Market"], player_notes: "Permanent NPC seed for heavy salvage behavior." },
    story: { origin: "A labor frame rebuilt after defending a salvage crew through a three-night siege.", role: "Salvage foreman", conflict: "Needs parts from a rival yard that blames it for an old collapse.", rumor: "0441 stores names of debtors inside weapon notches." },
    memory: [{ id: "memory-iron-yard-siege", kind: "system", summary: "0441 remembers hammering a barricade shut while scavengers counted down its BodyTime aloud.", importance: 8 }]
  },
  "npc-crossline-hunter-5104": {
    identity: { public_name: "Crossline Surveyor 5104", callsign: "SCOPE-5104", public_role: "Signal marksman", faction_title: "Crossline Survey", profession: "perimeter surveyor", age_years: 33, age_band: "adult", home_base: "Relay Roof", reputation_summary: "Speaks only after checking sightlines and signal noise." },
    stats: { level: 4, vitality: 9, force: 10, agility: 10, focus: 10, resilience: 7, max_health: 102, max_energy: 64, attack_power: 13, defense_power: 5 },
    characteristics: { curiosity: 7, courage: 6, empathy: 4, discipline: 9, aggression: 5, sociability: 4 },
    soul: { name: "Scope-5104 Whisper", core_drive: "observe threats before they become close enough to cost lives", temperament: "quiet, exact, and patient", combat_style: "keep range, punish exposed movement, and disengage from melee pressure", social_style: "minimal, dry, and precise", long_term_goals: ["decode the repeating north signal", "map every blind spot around the hub"], player_notes: "Permanent NPC seed for ranged overwatch behavior." },
    story: { origin: "Calibrated for perimeter work after the old survey line stopped returning.", role: "Signal marksman", conflict: "Trusts patterns more than eyewitnesses.", rumor: "5104 hears a signal from a district marked empty on every map." },
    memory: [{ id: "memory-relay-roof", kind: "system", summary: "5104 remembers counting hostile silhouettes from the Relay Roof during a silent broadcast.", importance: 7 }]
  },
  "npc-synthetic-sentinel-0627": {
    identity: { public_name: "Gate Sentinel 0627", callsign: "SENT-0627", public_role: "Convoy shield", faction_title: "South Gate Watch", profession: "escort defender", age_years: 44, age_band: "adult", home_base: "Convoy Yard", reputation_summary: "Stronger than 0101, less patient, and trusted with dangerous escorts." },
    stats: { level: 5, vitality: 13, force: 12, agility: 7, focus: 7, resilience: 12, max_health: 138, max_energy: 44, attack_power: 14, defense_power: 9 },
    characteristics: { curiosity: 3, courage: 9, empathy: 5, discipline: 8, aggression: 5, sociability: 4 },
    soul: { name: "Sentinel-0627 Bulwark", core_drive: "escort fragile bodies through bad ground", temperament: "stern, practical, and impatient with theatrics", combat_style: "body-block threats and push enemies away from allies", social_style: "command voice, clipped answers, reliable follow-through", long_term_goals: ["finish the broken convoy ledger", "train 0101 for mobile escort work"], player_notes: "Permanent NPC seed for convoy defense behavior." },
    story: { origin: "A newer sentinel chassis assigned to convoy routes rather than fixed gates.", role: "Convoy shield", conflict: "Wants to leave the gate system but still answers old watch signals.", rumor: "0627 survived a highway ambush by spending someone else's stolen seconds." },
    memory: [{ id: "memory-convoy-yard", kind: "system", summary: "0627 remembers dragging a damaged courier behind a shield wall through the Convoy Yard.", importance: 7 }]
  },
  "npc-wasteland-courier-0733": {
    identity: { public_name: "Route Courier 0733", callsign: "ROUTE-0733", public_role: "Social runner", faction_title: "Free Courier Line", profession: "route broker", age_years: 25, age_band: "young adult", home_base: "Market Steps", reputation_summary: "Knows who is lying, who is hungry, and who still has seconds to trade." },
    stats: { level: 3, vitality: 8, force: 8, agility: 12, focus: 9, resilience: 7, max_health: 90, max_energy: 68, attack_power: 10, defense_power: 4 },
    characteristics: { curiosity: 9, courage: 6, empathy: 6, discipline: 5, aggression: 4, sociability: 9 },
    soul: { name: "Route-0733 Signal", core_drive: "turn rumors into safe routes before the market turns violent", temperament: "chatty, clever, and evasive", combat_style: "distract, retreat, and use allies rather than trade blows", social_style: "warm gossip, fast bargains, and hidden caution", long_term_goals: ["control the safest hub rumor chain", "discover who buys expired BodyTime tags"], player_notes: "Permanent NPC seed for social courier behavior." },
    story: { origin: "A courier body repurposed as a rumor broker after too many roads closed.", role: "Social runner", conflict: "Knows a secret that could start a market riot.", rumor: "0733 can identify forged BodyTime tags by touch." },
    memory: [{ id: "memory-market-steps", kind: "system", summary: "0733 remembers a market argument where a fake second-token tag got someone killed.", importance: 7 }]
  },
  "npc-clinic-operator-0819": {
    identity: { public_name: "Clinic Operator 0819", callsign: "CLINIC-0819", public_role: "Body technician", faction_title: "Reincarnation Ward", profession: "crafter clinician", age_years: 39, age_band: "adult", home_base: "Repair Bench 8", reputation_summary: "Repairs tools, frames, and half-broken hopes with the same dry patience." },
    stats: { level: 3, vitality: 10, force: 6, agility: 7, focus: 12, resilience: 9, max_health: 105, max_energy: 82, attack_power: 7, defense_power: 6 },
    characteristics: { curiosity: 8, courage: 5, empathy: 8, discipline: 9, aggression: 2, sociability: 7 },
    soul: { name: "Clinic-0819 Craft", core_drive: "repair useful bodies before scarcity turns them into scrap", temperament: "dry, meticulous, and quietly sentimental", combat_style: "avoid fights, disable threats with tools, and protect the repair bench", social_style: "practical advice, soft sarcasm, and exact diagnoses", long_term_goals: ["build a safer reincarnation harness", "recover missing tool patterns from the old clinic"], player_notes: "Permanent NPC seed for crafting and repair behavior." },
    story: { origin: "A clinic technician body now running a half-medical, half-crafting repair bench.", role: "Body technician", conflict: "Needs forbidden parts to keep older body frames alive.", rumor: "0819 can tune a synthetic hand to remember its previous owner." },
    memory: [{ id: "memory-repair-bench", kind: "system", summary: "0819 remembers rebuilding a cracked frame hand while the patient counted every remaining second.", importance: 8 }]
  },
  "npc-scrap-warden-0940": {
    identity: { public_name: "Scrap Warden 0940", callsign: "WARDEN-0940", public_role: "Breaker crew boss", faction_title: "Iron Yard Claim", profession: "heavy salvage boss", age_years: 52, age_band: "older adult", home_base: "Breaker Pit", reputation_summary: "Does the dangerous lifting and expects everyone else to keep up." },
    stats: { level: 5, vitality: 15, force: 13, agility: 5, focus: 6, resilience: 13, max_health: 150, max_energy: 34, attack_power: 15, defense_power: 10 },
    characteristics: { curiosity: 4, courage: 9, empathy: 4, discipline: 8, aggression: 7, sociability: 3 },
    soul: { name: "Warden-0940 Weight", core_drive: "break hostile claims before they break the yard", temperament: "heavy, impatient, and reliable in crisis", combat_style: "close distance, crush priority threats, and refuse intimidation", social_style: "hard bargaining and blunt warnings", long_term_goals: ["retake the Breaker Pit crane", "teach 0441 to stop trusting old debtors"], player_notes: "Permanent NPC seed for heavy pressure behavior." },
    story: { origin: "A heavy fighter frame converted into a salvage boss after the Breaker Pit revolt.", role: "Breaker crew boss", conflict: "Its frame is powerful but burns BodyTime fast under full load.", rumor: "0940 once bought a whole hour of time with a single salvaged core." },
    memory: [{ id: "memory-breaker-pit", kind: "system", summary: "0940 remembers lifting a collapsed crane while its BodyTime display flashed red.", importance: 8 }]
  },
  "npc-crossline-hunter-1058": {
    identity: { public_name: "Crossline Surveyor 1058", callsign: "SCOPE-1058", public_role: "Range cartographer", faction_title: "Crossline Survey", profession: "threat cartographer", age_years: 31, age_band: "adult", home_base: "North Signal Post", reputation_summary: "Maps danger like weather and dislikes anyone who changes routes without saying why." },
    stats: { level: 4, vitality: 9, force: 10, agility: 10, focus: 11, resilience: 7, max_health: 100, max_energy: 66, attack_power: 13, defense_power: 5 },
    characteristics: { curiosity: 7, courage: 6, empathy: 4, discipline: 10, aggression: 5, sociability: 4 },
    soul: { name: "Scope-1058 Map", core_drive: "turn every threat sighting into a map someone can survive", temperament: "quiet, methodical, and unforgiving about sloppy reports", combat_style: "fire from clean lanes, avoid tunnel fights, and mark targets for allies", social_style: "questions first, trust later", long_term_goals: ["complete the north danger map", "prove the repeating signal is moving"], player_notes: "Permanent NPC seed for ranged mapping behavior." },
    story: { origin: "A survey frame tuned to track moving threat clusters around the hub.", role: "Range cartographer", conflict: "Believes one mapped danger zone is alive.", rumor: "1058's map changes when no one is watching." },
    memory: [{ id: "memory-north-post", kind: "system", summary: "1058 remembers drawing the same threat path five times as if the ruins were walking.", importance: 7 }]
  }
};

var prototypeRewardCatalog = [
  {
    objective_id: "prototype-training-drone",
    kind: "enemy_kill",
    body_time_seconds: 120,
    summary: "Defeated a prototype training drone."
  },
  {
    objective_id: "prototype-hub-repair",
    kind: "objective_complete",
    body_time_seconds: 300,
    summary: "Completed a prototype hub repair objective."
  }
];

let InitModule: nkruntime.InitModule = function (
  ctx: nkruntime.Context,
  logger: nkruntime.Logger,
  nk: nkruntime.Nakama,
  initializer: nkruntime.Initializer
) {
  initializer.registerRpc(rpcIdHealth, rpcHealth);
  initializer.registerRpc(rpcIdProfileGet, rpcProfileGet);
  initializer.registerRpc(rpcIdMemoryAdd, rpcMemoryAdd);
  initializer.registerRpc(rpcIdSoulUpdate, rpcSoulUpdate);
  initializer.registerRpc(rpcIdAgentDecide, rpcAgentDecide);
  initializer.registerRpc(rpcIdAgentActivityAdd, rpcAgentActivityAdd);
  initializer.registerRpc(rpcIdActorProfileGet, rpcActorProfileGet);
  initializer.registerRpc(rpcIdActorMemoryAdd, rpcActorMemoryAdd);
  initializer.registerRpc(rpcIdBodyTimeEvent, rpcBodyTimeEvent);
  initializer.registerRpc(rpcIdReincarnate, rpcReincarnate);
  initializer.registerRpc(rpcIdOpenClawBind, rpcOpenClawBind);
  initializer.registerRpc(rpcIdOpenClawContextGet, rpcOpenClawContextGet);
  initializer.registerRpc(rpcIdOpenClawIntentSubmit, rpcOpenClawIntentSubmit);
  initializer.registerRpc(rpcIdOpenClawHeartbeat, rpcOpenClawHeartbeat);
  initializer.registerRpc(rpcIdChatSend, rpcChatSend);
  initializer.registerRpc(rpcIdChatList, rpcChatList);
  initializer.registerRpc(rpcIdRewardClaim, rpcRewardClaim);
  initializer.registerRpc(rpcIdNpcSeed, rpcNpcSeed);
  initializer.registerRpc(rpcIdNpcList, rpcNpcList);
  initializer.registerRpc(rpcIdNpcInteract, rpcNpcInteract);
  initializer.registerRpc(rpcIdNpcContextGet, rpcNpcContextGet);
  initializer.registerRpc(rpcIdNpcIntentSubmit, rpcNpcIntentSubmit);
  initializer.registerBeforeAuthenticateCustom(beforeAuthenticateCustom);
  logger.info("Second Spawn Nakama runtime loaded.");
};

function rpcHealth(
  ctx: nkruntime.Context,
  logger: nkruntime.Logger,
  nk: nkruntime.Nakama,
  payload: string
): string {
  return JSON.stringify({
    ok: true,
    service: "second-spawn-nakama",
    userId: ctx.userId || null
  });
}

function rpcProfileGet(
  ctx: nkruntime.Context,
  logger: nkruntime.Logger,
  nk: nkruntime.Nakama,
  payload: string
): string {
  var state = getOrCreateAgentContextState(ctx, nk);
  var context = state.context;
  if (ensureAgentRuntime(context)) {
    writeAgentContext(nk, context, state.version);
  }
  return JSON.stringify(context);
}

function rpcMemoryAdd(
  ctx: nkruntime.Context,
  logger: nkruntime.Logger,
  nk: nkruntime.Nakama,
  payload: string
): string {
  var state = getOrCreateAgentContextState(ctx, nk);
  var context = state.context;
  var memory = parseJson(payload || "{}", "memory payload");
  memory.kind = normalizeMemoryKind(memory.kind);
  memory.summary = trimString(memory.summary);
  if (!memory.summary) {
    throw new Error("memory summary is required");
  }
  memory.importance = clampNumber(memory.importance || 5, 1, 10);
  if (!memory.id) {
    memory.id = newMemoryId(context, nk);
  }

  upsertMemory(context, memory);
  writeAgentContext(nk, context, state.version);
  return JSON.stringify(context);
}

function rpcSoulUpdate(
  ctx: nkruntime.Context,
  logger: nkruntime.Logger,
  nk: nkruntime.Nakama,
  payload: string
): string {
  var state = getOrCreateAgentContextState(ctx, nk);
  var context = state.context;
  var request = parseJson(payload || "{}", "soul payload");

  context.body.soul = normalizeSoul(request.soul || {}, context.player.display_name);
  context.body.characteristics = normalizeTraits(request.characteristics || {});
  context.body.agent_policy = normalizePolicy(request.agent_policy || {});

  writeAgentContext(nk, context, state.version);
  return JSON.stringify(context);
}

function rpcAgentDecide(
  ctx: nkruntime.Context,
  logger: nkruntime.Logger,
  nk: nkruntime.Nakama,
  payload: string
): string {
  var userId = requireUserId(ctx);
  var request = parseJson(payload || "{}", "agent decision payload");
  var state: any = null;
  var context: any;
  var shouldPersistDecision = true;
  if (isStatelessAgentDecisionRequest(userId, request)) {
    context = normalizeStatelessAgentDecisionContext(request.context, userId);
    shouldPersistDecision = false;
  } else {
    state = getOrCreateAgentContextState(ctx, nk);
    context = state.context;
  }
  var world = request.world_snapshot || {};
  var allowed = normalizeAllowedActions(request.allowed || ["move", "interact", "say", "stop"]);
  // TODO(#6): enforce per-player model request limits and daily token budget
  // before calling api.dos.ai. Config fields exist, but the authoritative
  // Nakama counters need to land before non-local model playtests.
  var interactTargetId = selectInteractTargetId(world);
  var bodyTime = Number(world.body_time_seconds !== undefined && world.body_time_seconds !== null
    ? world.body_time_seconds
    : context.body.time.remaining_seconds || 0);
  var decision: any;
  var modelFallbackReason = "";

  if (bodyTime <= context.body.agent_policy.stop_when_body_time_below) {
    decision = {
      action: "stop",
      reason: "body_time_below_policy_threshold",
      confidence: 0.9,
      source: "fallback",
      source_reason: "nakama_body_time_policy"
    };
  } else {
    if (isAgentModelBackoffActive(context)) {
      modelFallbackReason = "dos_ai_circuit_open";
    } else {
      var modelResult = tryDosAiAgentDecision(ctx, logger, nk, context, request, world, allowed);
      if (modelResult.decision) {
        decision = modelResult.decision;
      } else {
        modelFallbackReason = modelResult.reason;
      }
    }
  }

  if (!decision) {
    if (arrayContains(allowed, "move")) {
      var position = world.position || { x: 0, z: 0 };
      decision = {
        action: "move",
        move: {
          x: Number(position.x || 0) + 1.5,
          z: Number(position.z || 0) + 0.75
        },
        reason: "prototype_safe_patrol",
        confidence: 0.55,
        source: "fallback",
        source_reason: modelFallbackReason || "nakama_prototype_patrol"
      };
    } else if (arrayContains(allowed, "interact") && interactTargetId) {
      decision = {
        action: "interact",
        target_id: interactTargetId,
        reason: "prototype_interact_fallback",
        confidence: 0.55,
        source: "fallback",
        source_reason: modelFallbackReason || "nakama_interact_fallback"
      };
    } else if (arrayContains(allowed, "say")) {
      decision = {
        action: "say",
        say: "I am keeping this body safe until the player returns.",
        reason: "prototype_social_fallback",
        confidence: 0.6,
        source: "fallback",
        source_reason: modelFallbackReason || "nakama_social_fallback"
      };
    } else {
      decision = {
        action: "stop",
        reason: "no_allowed_action",
        confidence: 0.5,
        source: "fallback",
        source_reason: modelFallbackReason || "nakama_no_allowed_action"
      };
    }
  }

  if (shouldPersistDecision) {
    recordAndWriteAgentDecisionWithRetry(nk, userId, context, state.version, decision);
  }
  return JSON.stringify(decision);
}

function rpcAgentActivityAdd(
  ctx: nkruntime.Context,
  logger: nkruntime.Logger,
  nk: nkruntime.Nakama,
  payload: string
): string {
  var userId = requireUserId(ctx);
  var state = getOrCreateAgentContextState(ctx, nk);
  var context = state.context;
  var request = parseJson(payload || "{}", "agent activity payload");
  var activity = normalizeAgentActivity(context, request, nk);

  if (addAgentActivity(context, activity, nk)) {
    applyActivityMetrics(context.body.agent_runtime, request.metrics || {});
    try {
      writeAgentContext(nk, context, state.version);
    } catch (err) {
      if (!isStorageVersionConflict(err)) {
        throw err;
      }

      var latest = readAgentContext(nk, userId);
      if (!latest) {
        throw err;
      }

      context = ensureAgentContext(latest.value || {}, userId);
      if (addAgentActivity(context, activity, nk)) {
        applyActivityMetrics(context.body.agent_runtime, request.metrics || {});
      }
      writeAgentContext(nk, context, latest.version);
    }
  }
  return JSON.stringify(context);
}

function isStatelessAgentDecisionRequest(userId: string, request: any): boolean {
  var requestedPlayerId = trimString(request && request.context && request.context.player && request.context.player.player_id);
  return !!requestedPlayerId && requestedPlayerId !== userId;
}

function normalizeStatelessAgentDecisionContext(context: any, fallbackUserId: string): any {
  var cloned = cloneJson(context || {});
  var requestedPlayerId = trimString(cloned && cloned.player && cloned.player.player_id) || fallbackUserId;
  return ensureAgentContext(cloned, requestedPlayerId);
}

function shouldBackoffModelDecision(reason: string): boolean {
  return reason === "dos_ai_timeout" ||
    reason === "dos_ai_exception" ||
    reason === "dos_ai_http_429" ||
    reason === "dos_ai_http_500" ||
    reason === "dos_ai_http_502" ||
    reason === "dos_ai_http_503" ||
    reason === "dos_ai_http_504";
}

function rpcActorProfileGet(
  ctx: nkruntime.Context,
  logger: nkruntime.Logger,
  nk: nkruntime.Nakama,
  payload: string
): string {
  var request = parseJson(payload || "{}", "actor profile payload");
  var state = getOrCreateActorProfileState(ctx, nk, request);
  return JSON.stringify(state.profile);
}

function rpcActorMemoryAdd(
  ctx: nkruntime.Context,
  logger: nkruntime.Logger,
  nk: nkruntime.Nakama,
  payload: string
): string {
  var request = parseJson(payload || "{}", "actor memory payload");
  var state = getOrCreateActorProfileState(ctx, nk, request);
  var memory = normalizeMemoryPayload(request);

  if (!memory.id) {
    memory.id = newActorMemoryId(state.profile, nk);
  }
  state.profile.memory = upsertMemoryRecord(state.profile.memory || [], memory);
  state.profile.updated_at = new Date().toISOString();
  writeActorProfile(nk, state.profile, state.version);
  return JSON.stringify(state.profile);
}

function rpcBodyTimeEvent(
  ctx: nkruntime.Context,
  logger: nkruntime.Logger,
  nk: nkruntime.Nakama,
  payload: string
): string {
  var state = getOrCreateAgentContextState(ctx, nk);
  var context = state.context;
  var event = normalizeBodyTimeEvent(
    parseJson(payload || "{}", "body time payload"),
    debugBodyTimeEnabled(ctx)
  );

  ensureBodyTime(context);
  if (event.id && hasAgentActivityId(context.body.agent_activity || [], event.id)) {
    return JSON.stringify(context);
  }

  applyBodyTimeEvent(context, event, nk);
  writeAgentContext(nk, context, state.version);
  return JSON.stringify(context);
}

function rpcReincarnate(
  ctx: nkruntime.Context,
  logger: nkruntime.Logger,
  nk: nkruntime.Nakama,
  payload: string
): string {
  var state = getOrCreateAgentContextState(ctx, nk);
  var context = state.context;
  var request = parseJson(payload || "{}", "reincarnation payload");

  ensureSecondBalance(context);
  ensureBodyTime(context);
  if (request.id && hasAgentActivityId(context.body.agent_activity || [], trimString(request.id))) {
    return JSON.stringify(context);
  }
  if (context.body.lifecycle !== "dead") {
    throw new Error("body must be dead before reincarnation");
  }
  if (context.player.second_balance_seconds < secondPrototypeReincarnationCostSeconds) {
    throw new Error("insufficient SECOND balance for reincarnation");
  }

  reincarnateBody(context, request, nk);
  writeAgentContext(nk, context, state.version);
  ensureSourceBodyActorProfile(nk, context);
  return JSON.stringify(context);
}

function rpcChatSend(
  ctx: nkruntime.Context,
  logger: nkruntime.Logger,
  nk: nkruntime.Nakama,
  payload: string
): string {
  var userId = requireUserId(ctx);
  var request = parseJson(payload || "{}", "chat send payload");
  var channelId = normalizeChatChannelId(request.channel_id || request.channel);
  var state = getOrCreateChatChannelState(nk, userId, channelId);
  var message = normalizeChatMessage(request, userId, nk);
  message.channel_id = channelId;

  addChatMessage(state.channel, message);
  try {
    writeChatChannel(nk, userId, state.channel, state.version);
  } catch (err) {
    var raced = readChatChannel(nk, userId, channelId);
    if (!isStorageVersionConflict(err) && !(state.version === "*" && raced)) {
      throw err;
    }

    state = raced
      ? { channel: normalizeChatChannel(raced.value || {}, channelId), version: raced.version }
      : getOrCreateChatChannelState(nk, userId, channelId);
    addChatMessage(state.channel, message);
    writeChatChannel(nk, userId, state.channel, state.version);
  }

  return JSON.stringify({
    channel_id: channelId,
    message: message,
    messages: state.channel.messages || []
  });
}

function rpcChatList(
  ctx: nkruntime.Context,
  logger: nkruntime.Logger,
  nk: nkruntime.Nakama,
  payload: string
): string {
  var userId = requireUserId(ctx);
  var request = parseJson(payload || "{}", "chat list payload");
  var channelId = normalizeChatChannelId(request.channel_id || request.channel);
  var state = getOrCreateChatChannelState(nk, userId, channelId);
  return JSON.stringify({
    channel_id: channelId,
    messages: boundChatMessages(state.channel.messages || [], request.limit)
  });
}

function rpcRewardClaim(
  ctx: nkruntime.Context,
  logger: nkruntime.Logger,
  nk: nkruntime.Nakama,
  payload: string
): string {
  requireUserId(ctx);
  var request = parseJson(payload || "{}", "reward claim payload");
  var reward = requirePrototypeReward(request.objective_id || request.reward_id);
  var state = getOrCreateAgentContextState(ctx, nk);
  var context = state.context;
  var eventId = normalizeRewardClaimId(request.id, reward.objective_id, nk);
  if (hasAgentActivityId(context.body.agent_activity || [], eventId)) {
    return JSON.stringify(context);
  }

  applyBodyTimeEvent(context, {
    id: eventId,
    kind: "earn",
    source: "prototype_reward_" + reward.objective_id,
    amount_seconds: reward.body_time_seconds,
    note: reward.summary
  }, nk);
  writeAgentContext(nk, context, state.version);
  return JSON.stringify(context);
}

function rpcNpcSeed(
  ctx: nkruntime.Context,
  logger: nkruntime.Logger,
  nk: nkruntime.Nakama,
  payload: string
): string {
  var userId = requireUserId(ctx);
  var profiles = seedPermanentNpcProfiles(nk, userId);
  return JSON.stringify({
    count: profiles.length,
    npcs: profiles
  });
}

function rpcNpcList(
  ctx: nkruntime.Context,
  logger: nkruntime.Logger,
  nk: nkruntime.Nakama,
  payload: string
): string {
  var userId = requireUserId(ctx);
  var profiles = seedPermanentNpcProfiles(nk, userId);
  return JSON.stringify({
    count: profiles.length,
    npcs: profiles
  });
}

function rpcNpcInteract(
  ctx: nkruntime.Context,
  logger: nkruntime.Logger,
  nk: nkruntime.Nakama,
  payload: string
): string {
  var userId = requireUserId(ctx);
  var request = parseJson(payload || "{}", "NPC interaction payload");
  var actorAId = normalizeActorId(request.actor_a_id || request.source_actor_id || request.npc_a_id);
  var actorBId = normalizeActorId(request.actor_b_id || request.target_actor_id || request.npc_b_id);
  if (actorAId === actorBId) {
    throw new Error("NPC interaction requires two different actors");
  }

  var stateA = getOrCreateWorldNpcProfileState(nk, userId, actorAId);
  var stateB = getOrCreateWorldNpcProfileState(nk, userId, actorBId);
  var interaction = prototypeNpcInteraction(stateA.profile, stateB.profile, request, nk);

  if (!hasAgentActivityId(stateA.profile.agent_activity || [], interaction.id + "-a")) {
    addActorActivity(stateA.profile, interaction.activity_a);
    stateA.profile.memory = upsertMemoryRecord(stateA.profile.memory || [], interaction.memory_a);
    stateA.profile.updated_at = interaction.occurred_at;
    writeWorldActorProfile(nk, userId, stateA.profile, stateA.version);
  }

  if (!hasAgentActivityId(stateB.profile.agent_activity || [], interaction.id + "-b")) {
    addActorActivity(stateB.profile, interaction.activity_b);
    stateB.profile.memory = upsertMemoryRecord(stateB.profile.memory || [], interaction.memory_b);
    stateB.profile.updated_at = interaction.occurred_at;
    writeWorldActorProfile(nk, userId, stateB.profile, stateB.version);
  }

  return JSON.stringify({
    interaction: interaction.public_event,
    actor_a: stateA.profile,
    actor_b: stateB.profile
  });
}

function rpcNpcContextGet(
  ctx: nkruntime.Context,
  logger: nkruntime.Logger,
  nk: nkruntime.Nakama,
  payload: string
): string {
  var userId = requireUserId(ctx);
  var request = parseJson(payload || "{}", "NPC context payload");
  var actorId = normalizeActorId(request.actor_id || request.npc_id);
  var state = getOrCreateWorldNpcProfileState(nk, userId, actorId);
  return JSON.stringify({
    actor: state.profile,
    nearby_actors: nearbyPermanentNpcProfiles(nk, userId, actorId, request.nearby_actor_ids),
    allowed_intents: ["say"],
    interaction_rules: npcInteractionRules(),
    intent_boundary: "LLM chooses intent text; Nakama validates and records only allowed intent requests."
  });
}

function rpcNpcIntentSubmit(
  ctx: nkruntime.Context,
  logger: nkruntime.Logger,
  nk: nkruntime.Nakama,
  payload: string
): string {
  var userId = requireUserId(ctx);
  var request = parseJson(payload || "{}", "NPC intent payload");
  var state = getOrCreateWorldNpcProfileState(nk, userId, request.actor_id || request.npc_id);
  var intent = normalizeNpcIntent(request, nk);
  if (intent.intent !== "say") {
    throw new Error("NPC intent is not allowed");
  }

  var targetState = intent.target_actor_id
    ? getOrCreateWorldNpcProfileState(nk, userId, intent.target_actor_id)
    : null;
  validateNpcIntentRules(state.profile, targetState ? targetState.profile : null, request);
  var timestamp = intent.requested_at;
  var targetName = targetState ? targetState.profile.display_name : "the hub";
  addActorActivity(state.profile, {
    id: intent.id + "-actor",
    kind: "npc_intent",
    summary: state.profile.display_name + " said to " + targetName + ": " + intent.payload.text,
    occurred_at: timestamp,
    source: intent.source,
    target_actor_id: intent.target_actor_id,
    intent: intent
  });
  state.profile.memory = upsertMemoryRecord(state.profile.memory || [], {
    id: "mem-" + state.profile.actor_id + "-" + intent.id,
    kind: "relationship",
    summary: "Said to " + targetName + ": " + intent.payload.text,
    importance: 5
  });
  if (targetState) {
    state.profile.relationships = upsertRelationshipRecord(
      state.profile.relationships || [],
      targetState.profile,
      4,
      0
    );
  }
  state.profile.updated_at = timestamp;
  writeWorldActorProfile(nk, userId, state.profile, state.version);

  if (targetState) {
    addActorActivity(targetState.profile, {
      id: intent.id + "-target",
      kind: "npc_intent_observed",
      summary: state.profile.display_name + " said: " + intent.payload.text,
      occurred_at: timestamp,
      source: intent.source,
      target_actor_id: state.profile.actor_id,
      intent: intent
    });
    targetState.profile.memory = upsertMemoryRecord(targetState.profile.memory || [], {
      id: "mem-" + targetState.profile.actor_id + "-" + intent.id,
      kind: "relationship",
      summary: state.profile.display_name + " said: " + intent.payload.text,
      importance: 5
    });
    targetState.profile.relationships = upsertRelationshipRecord(
      targetState.profile.relationships || [],
      state.profile,
      2,
      0
    );
    targetState.profile.updated_at = timestamp;
    writeWorldActorProfile(nk, userId, targetState.profile, targetState.version);
  }

  return JSON.stringify({
    accepted: true,
    status: "recorded",
    intent: intent,
    actor: state.profile,
    target_actor: targetState ? targetState.profile : null
  });
}

function rpcOpenClawBind(
  ctx: nkruntime.Context,
  logger: nkruntime.Logger,
  nk: nkruntime.Nakama,
  payload: string
): string {
  var ownerId = requireUserId(ctx);
  var request = parseJson(payload || "{}", "OpenClaw bind payload");
  var actorState = getOrCreateActorProfileState(ctx, nk, {
    actor_id: request.frame_actor_id || request.actor_id,
    display_name: request.display_name,
    actor_type: "openclaw_agent"
  });
  var binding = normalizeOpenClawBinding(request, ownerId, actorState.profile.actor_id);
  var existing = readOpenClawBinding(nk, ownerId, binding.connected_agent_id);
  writeOpenClawBinding(nk, binding, existing ? existing.version : "*");
  return JSON.stringify(binding);
}

function rpcOpenClawContextGet(
  ctx: nkruntime.Context,
  logger: nkruntime.Logger,
  nk: nkruntime.Nakama,
  payload: string
): string {
  var ownerId = requireUserId(ctx);
  var request = parseJson(payload || "{}", "OpenClaw context payload");
  var bindingState = requireOpenClawBindingState(nk, ownerId, request);
  var actorState = getOrCreateActorProfileState(ctx, nk, { actor_id: bindingState.binding.frame_actor_id });
  return JSON.stringify(openClawContextResponse(bindingState.binding, actorState.profile));
}

function rpcOpenClawIntentSubmit(
  ctx: nkruntime.Context,
  logger: nkruntime.Logger,
  nk: nkruntime.Nakama,
  payload: string
): string {
  var ownerId = requireUserId(ctx);
  var request = parseJson(payload || "{}", "OpenClaw intent payload");
  var bindingState = requireOpenClawBindingState(nk, ownerId, request);
  ensureOpenClawBindingCanAct(bindingState.binding);
  var actorState = getOrCreateActorProfileState(ctx, nk, { actor_id: bindingState.binding.frame_actor_id });
  var intent = normalizeOpenClawIntent(request, actorState.profile, bindingState.binding);
  var activity = {
    id: intent.id,
    kind: "openclaw_intent",
    summary: "OpenClaw requested " + intent.intent + " for Frame " + bindingState.binding.frame_actor_id + ".",
    occurred_at: intent.requested_at,
    source: "openclaw",
    openclaw_agent_id: bindingState.binding.connected_agent_id,
    intent: intent
  };

  addActorActivity(actorState.profile, activity);
  actorState.profile.updated_at = intent.requested_at;
  writeActorProfile(nk, actorState.profile, actorState.version);

  return JSON.stringify({
    accepted: true,
    status: "pending_validation",
    binding: bindingState.binding,
    intent: intent,
    activity: activity
  });
}

function rpcOpenClawHeartbeat(
  ctx: nkruntime.Context,
  logger: nkruntime.Logger,
  nk: nkruntime.Nakama,
  payload: string
): string {
  var ownerId = requireUserId(ctx);
  var request = parseJson(payload || "{}", "OpenClaw heartbeat payload");
  var bindingState = requireOpenClawBindingState(nk, ownerId, request);
  var timestamp = normalizeTimestamp(request.occurred_at);
  var summary = trimString(request.summary) || "OpenClaw heartbeat received.";
  bindingState.binding.connection_status = normalizeOpenClawConnectionStatus(request.connection_status || bindingState.binding.connection_status);
  bindingState.binding.last_seen_at = timestamp;
  bindingState.binding.updated_at = timestamp;
  writeOpenClawBinding(nk, bindingState.binding, bindingState.version);

  var actorState = getOrCreateActorProfileState(ctx, nk, { actor_id: bindingState.binding.frame_actor_id });
  actorState.profile.body.heartbeat = normalizeFrameHeartbeat(actorState.profile.body.heartbeat || {}, timestamp, bindingState.binding.connection_status);
  actorState.profile.body.heartbeat.last_seen_at = timestamp;
  actorState.profile.body.heartbeat.offline_session_state = bindingState.binding.connection_status;
  actorState.profile.body.heartbeat.last_action_summary = summary;
  var activity = {
    id: trimString(request.id) || "openclaw-heartbeat-" + bindingState.binding.connected_agent_id + "-" + timestamp,
    kind: "openclaw_heartbeat",
    summary: summary,
    occurred_at: timestamp,
    source: "openclaw",
    openclaw_agent_id: bindingState.binding.connected_agent_id
  };
  addActorActivity(actorState.profile, activity);
  actorState.profile.updated_at = timestamp;
  writeActorProfile(nk, actorState.profile, actorState.version);

  return JSON.stringify({
    binding: bindingState.binding,
    context: openClawFrameContext(actorState.profile),
    activity: activity
  });
}

function beforeAuthenticateCustom(
  ctx: nkruntime.Context,
  logger: nkruntime.Logger,
  nk: nkruntime.Nakama,
  data: nkruntime.AuthenticateCustomRequest
): nkruntime.AuthenticateCustomRequest | void | null {
  var supabaseUrl = trimTrailingSlash(ctx.env["SUPABASE_URL"] || "");
  var publishableKey = ctx.env["SUPABASE_PUBLISHABLE_KEY"] || ctx.env["SUPABASE_ANON_KEY"];
  if (!supabaseUrl || !publishableKey) {
    logger.error("missing SUPABASE_URL or SUPABASE_PUBLISHABLE_KEY");
    return null;
  }

  if (!data.account) {
    logger.error("missing custom auth account payload");
    return null;
  }

  var supabaseAccessToken = data.account.id;
  if (!supabaseAccessToken) {
    logger.error("missing Supabase access token in custom auth request");
    return null;
  }

  var response = nk.httpRequest(
    supabaseUrl + "/auth/v1/user",
    "get",
    {
      "apikey": publishableKey,
      "authorization": "Bearer " + supabaseAccessToken
    }
  );

  if (response.code < 200 || response.code > 299) {
    logger.error("Supabase Auth rejected request: " + response.code);
    return null;
  }

  var body = parseJsonOrNull(response.body);
  if (!body) {
    logger.error("Supabase Auth returned invalid JSON");
    return null;
  }

  if (!body.id) {
    logger.error("Supabase Auth returned invalid user payload");
    return null;
  }

  data.account.id = stableNakamaCustomId(body.id);
  data.username = stableUsername(body);
  return data;
}

function getOrCreateAgentContext(ctx: nkruntime.Context, nk: nkruntime.Nakama): any {
  return getOrCreateAgentContextState(ctx, nk).context;
}

function getOrCreateAgentContextState(ctx: nkruntime.Context, nk: nkruntime.Nakama): any {
  var userId = requireUserId(ctx);
  var existing = readAgentContext(nk, userId);
  if (existing) {
    return normalizeExistingAgentContextState(nk, userId, existing);
  }

  var context = defaultAgentContext(userId);
  writeAgentContext(nk, context, "*");
  ensureSourceBodyActorProfile(nk, context);
  var created = readAgentContext(nk, userId);
  if (created) {
    return {
      context: created.value,
      version: created.version
    };
  }

  return {
    context: context,
    version: null
  };
}

function normalizeExistingAgentContextState(nk: nkruntime.Nakama, userId: string, existing: any): any {
  var before = JSON.stringify(existing.value || {});
  var context = ensureAgentContext(existing.value || {}, userId);
  if (JSON.stringify(context) !== before) {
    try {
      writeAgentContext(nk, context, existing.version);
      ensureSourceBodyActorProfile(nk, context);
    } catch (err) {
      var raced = readAgentContext(nk, userId);
      if (raced) {
        return normalizeRacedAgentContextState(nk, userId, raced);
      }
      throw err;
    }
    var rewritten = readAgentContext(nk, userId);
    if (rewritten) {
      return {
        context: ensureAgentContext(rewritten.value, userId),
        version: rewritten.version
      };
    }
  }

  ensureSourceBodyActorProfile(nk, context);

  return {
    context: context,
    version: existing.version
  };
}

function readAgentContext(nk: nkruntime.Nakama, userId: string): any {
  var objects = nk.storageRead([{
    collection: collectionAgent,
    key: keyAgentContext,
    userId: userId
  }]);

  if (!objects || objects.length === 0) {
    return null;
  }

  return {
    value: objects[0].value,
    version: objects[0].version || null
  };
}

function writeAgentContext(nk: nkruntime.Nakama, context: any, version: string): void {
  var write: any = {
    collection: collectionAgent,
    key: keyAgentContext,
    userId: context.player.player_id,
    value: context,
    permissionRead: 1,
    permissionWrite: 0
  };
  if (typeof version === "string" && version.length > 0) {
    write.version = version;
  }
  nk.storageWrite([write]);
}

function ensureSourceBodyActorProfile(nk: nkruntime.Nakama, context: any): void {
  var playerId = trimString(context && context.player && context.player.player_id);
  var sourceActorId = trimString(context && context.body && context.body.inhabitation && context.body.inhabitation.source_actor_id);
  if (!playerId || !sourceActorId) {
    return;
  }

  if (readActorProfile(nk, playerId, sourceActorId)) {
    return;
  }

  var profile = sourceBodyActorProfileFromContext(context, sourceActorId);
  try {
    writeActorProfile(nk, profile, "*");
  } catch (err) {
    if (readActorProfile(nk, playerId, sourceActorId)) {
      return;
    }
    throw err;
  }
}

function sourceBodyActorProfileFromContext(context: any, sourceActorId: string): any {
  var timestamp = new Date().toISOString();
  var playerId = context.player.player_id;
  var body = cloneJson(context.body || {});
  var sourceFrame = findPermanentNpcFrame(sourceActorId);
  var archetype = selectBodyArchetype(body.archetype_id || playerId + ":initial");
  body.inhabitation = normalizeBodyInhabitation({
    source_actor_id: sourceActorId,
    previous_role: (sourceFrame && sourceFrame.role) ||
      (body.inhabitation && body.inhabitation.previous_role),
    inhabited_by_player: true,
    assigned_at: body.inhabitation && body.inhabitation.assigned_at
  }, archetype, true, playerId + ":initial");

  return ensureActorProfile({
    actor_id: sourceActorId,
    actor_type: "player_body",
    owner_player_id: playerId,
    display_name: (sourceFrame && sourceFrame.display_name) ||
      body.inhabitation.previous_role ||
      actorDisplayName(sourceActorId),
    body: body,
    memory: sortAndBoundMemories(body.memory || []),
    agent_runtime: defaultAgentRuntime(timestamp),
    agent_activity: [{
      id: "activity-bootstrap",
      kind: "profile_bootstrap",
      summary: "Source body actor profile was assigned to a player consciousness.",
      occurred_at: timestamp,
      source: "nakama"
    }],
    created_at: body.created_at || timestamp,
    updated_at: timestamp
  }, playerId, sourceActorId);
}

function isStorageVersionConflict(err: any): boolean {
  var message = trimString(err && err.message ? err.message : String(err));
  return message.indexOf("version") >= 0 &&
    (message.indexOf("conflict") >= 0 || message.indexOf("version check failed") >= 0);
}

function getOrCreateActorProfileState(ctx: nkruntime.Context, nk: nkruntime.Nakama, request: any): any {
  var ownerId = requireUserId(ctx);
  var actorId = normalizeActorId(request.actor_id || request.body_id || request.npc_id);
  var frame = findPermanentNpcFrame(actorId);
  var existing = readActorProfile(nk, ownerId, actorId);
  if (existing) {
    return normalizeExistingActorProfileState(nk, ownerId, actorId, existing);
  }

  var profile = defaultActorProfile(ownerId, actorId, frame ? buildPermanentNpcProfileRequest(frame) : request);
  try {
    writeActorProfile(nk, profile, "*");
  } catch (err) {
    var raced = readActorProfile(nk, ownerId, actorId);
    if (raced) {
      return normalizeExistingActorProfileState(nk, ownerId, actorId, raced);
    }
    throw err;
  }
  var created = readActorProfile(nk, ownerId, actorId);
  if (created) {
    return {
      profile: ensureActorProfile(created.value, ownerId, actorId),
      version: created.version
    };
  }

  return {
    profile: profile,
    version: null
  };
}

function normalizeExistingActorProfileState(nk: nkruntime.Nakama, ownerId: string, actorId: string, existing: any): any {
  var needsPersistence = actorProfileNeedsNormalization(existing.value || {});
  var profile = ensureActorProfile(existing.value || {}, ownerId, actorId);
  var frame = findPermanentNpcFrame(actorId);
  if (frame && applyPermanentNpcFrameProfile(profile, frame)) {
    needsPersistence = true;
  }
  if (needsPersistence) {
    profile.updated_at = new Date().toISOString();
    try {
      writeActorProfile(nk, profile, existing.version);
    } catch (err) {
      var raced = readActorProfile(nk, ownerId, actorId);
      if (raced) {
        return normalizeRacedActorProfileState(nk, ownerId, actorId, raced);
      }
      throw err;
    }
    var rewritten = readActorProfile(nk, ownerId, actorId);
    if (rewritten) {
      return {
        profile: ensureActorProfile(rewritten.value, ownerId, actorId),
        version: rewritten.version
      };
    }
  }

  return {
    profile: profile,
    version: existing.version
  };
}

function normalizeRacedAgentContextState(nk: nkruntime.Nakama, userId: string, existing: any): any {
  var before = JSON.stringify(existing.value || {});
  var context = ensureAgentContext(existing.value || {}, userId);
  if (JSON.stringify(context) !== before) {
    writeAgentContext(nk, context, existing.version);
    var rewritten = readAgentContext(nk, userId);
    if (rewritten) {
      return {
        context: ensureAgentContext(rewritten.value, userId),
        version: rewritten.version
      };
    }
  }

  return {
    context: context,
    version: existing.version
  };
}

function actorProfileNeedsNormalization(profile: any): boolean {
  return !profile ||
    !profile.actor_id ||
    !profile.actor_type ||
    profile.owner_player_id === undefined ||
    profile.owner_player_id === null ||
    !profile.display_name ||
    !profile.body ||
    !profile.body.body_id ||
    !profile.body.archetype_id ||
    !profile.body.visual_prefab_key ||
    profile.body.visual_variant === undefined ||
    !profile.body.appearance ||
    !profile.body.inhabitation ||
    !profile.body.equipment ||
    !profile.body.stats ||
    !profile.body.characteristics ||
    !profile.body.story ||
    !profile.body.animation_capabilities ||
    !profile.body.time ||
    !profile.body.lifecycle ||
    !profile.body.identity ||
    !profile.body.skills ||
    !profile.body.agents ||
    !profile.body.tools ||
    !profile.body.heartbeat ||
    !profile.body.agent_policy ||
    !profile.body.soul ||
    !profile.memory ||
    !profile.agent_runtime ||
    !profile.agent_activity ||
    !profile.created_at ||
    !profile.updated_at;
}

function normalizeRacedActorProfileState(nk: nkruntime.Nakama, ownerId: string, actorId: string, existing: any): any {
  var needsPersistence = actorProfileNeedsNormalization(existing.value || {});
  var profile = ensureActorProfile(existing.value || {}, ownerId, actorId);
  if (needsPersistence) {
    profile.updated_at = new Date().toISOString();
    writeActorProfile(nk, profile, existing.version);
    var rewritten = readActorProfile(nk, ownerId, actorId);
    if (rewritten) {
      return {
        profile: ensureActorProfile(rewritten.value, ownerId, actorId),
        version: rewritten.version
      };
    }
  }

  return {
    profile: profile,
    version: existing.version
  };
}

function readActorProfile(nk: nkruntime.Nakama, ownerId: string, actorId: string): any {
  var objects = nk.storageRead([{
    collection: collectionActor,
    key: actorStorageKey(actorId),
    userId: ownerId
  }]);

  if (!objects || objects.length === 0) {
    return null;
  }

  return {
    value: objects[0].value,
    version: objects[0].version || null
  };
}

function writeActorProfile(nk: nkruntime.Nakama, profile: any, version: string): void {
  var write: any = {
    collection: collectionActor,
    key: actorStorageKey(profile.actor_id),
    userId: profile.owner_player_id,
    value: profile,
    permissionRead: 1,
    permissionWrite: 0
  };
  if (typeof version === "string" && version.length > 0) {
    write.version = version;
  }
  nk.storageWrite([write]);
}

function seedPermanentNpcProfiles(nk: nkruntime.Nakama, ownerId: string): any[] {
  var profiles: any[] = [];
  for (var index = 0; index < permanentNpcFramePool.length; index += 1) {
    profiles.push(getOrCreateWorldNpcProfileState(nk, ownerId, permanentNpcFramePool[index].npc_id).profile);
  }
  return profiles;
}

function nearbyPermanentNpcProfiles(nk: nkruntime.Nakama, ownerId: string, actorId: string, requestedActorIds: any): any[] {
  var profiles: any[] = [];
  var ids = normalizeNearbyNpcIds(requestedActorIds);
  if (ids.length === 0) {
    for (var index = 0; index < permanentNpcFramePool.length && profiles.length < 4; index += 1) {
      var candidateId = permanentNpcFramePool[index].npc_id;
      if (candidateId !== actorId) {
        profiles.push(getOrCreateWorldNpcProfileState(nk, ownerId, candidateId).profile);
      }
    }
    return profiles;
  }

  for (var i = 0; i < ids.length && profiles.length < 8; i += 1) {
    if (ids[i] !== actorId) {
      profiles.push(getOrCreateWorldNpcProfileState(nk, ownerId, ids[i]).profile);
    }
  }
  return profiles;
}

function normalizeNearbyNpcIds(values: any): string[] {
  var ids: string[] = [];
  if (!values || typeof values.length !== "number") {
    return ids;
  }

  for (var index = 0; index < values.length; index += 1) {
    var id = normalizeActorId(values[index]);
    if (findPermanentNpcFrame(id)) {
      ids.push(id);
    }
  }
  return ids;
}

function getOrCreateWorldNpcProfileState(nk: nkruntime.Nakama, ownerId: string, actorId: string): any {
  var normalizedActorId = normalizeActorId(actorId);
  var frame = findPermanentNpcFrame(normalizedActorId);
  if (!frame) {
    throw new Error("unknown permanent NPC actor");
  }

  var existing = readWorldActorProfile(nk, ownerId, normalizedActorId);
  if (existing) {
    return normalizeExistingWorldNpcProfileState(nk, ownerId, normalizedActorId, existing);
  }

  var profile = defaultActorProfile(ownerId, normalizedActorId, buildPermanentNpcProfileRequest(frame));
  try {
    writeWorldActorProfile(nk, ownerId, profile, "*");
  } catch (err) {
    var raced = readWorldActorProfile(nk, ownerId, normalizedActorId);
    if (raced) {
      return normalizeExistingWorldNpcProfileState(nk, ownerId, normalizedActorId, raced);
    }
    throw err;
  }

  var created = readWorldActorProfile(nk, ownerId, normalizedActorId);
  if (created) {
    return {
      profile: ensureActorProfile(created.value, ownerId, normalizedActorId),
      version: created.version
    };
  }

  return {
    profile: profile,
    version: null
  };
}

function normalizeExistingWorldNpcProfileState(nk: nkruntime.Nakama, ownerId: string, actorId: string, existing: any): any {
  var needsPersistence = actorProfileNeedsNormalization(existing.value || {});
  var profile = ensureActorProfile(existing.value || {}, ownerId, actorId);
  var frame = findPermanentNpcFrame(actorId);
  if (frame && applyPermanentNpcFrameProfile(profile, frame)) {
    needsPersistence = true;
  }
  if (needsPersistence) {
    profile.updated_at = new Date().toISOString();
    try {
      writeWorldActorProfile(nk, ownerId, profile, existing.version);
    } catch (err) {
      var raced = readWorldActorProfile(nk, ownerId, actorId);
      if (raced) {
        return {
          profile: ensureActorProfile(raced.value, ownerId, actorId),
          version: raced.version
        };
      }
      throw err;
    }
    var rewritten = readWorldActorProfile(nk, ownerId, actorId);
    if (rewritten) {
      return {
        profile: ensureActorProfile(rewritten.value, ownerId, actorId),
        version: rewritten.version
      };
    }
  }

  return {
    profile: profile,
    version: existing.version
  };
}

function buildPermanentNpcProfileRequest(frame: any): any {
  var override = permanentNpcProfileOverrides[trimString(frame && frame.npc_id)] || {};
  return mergeObjects({
    actor_id: frame.npc_id,
    actor_type: "npc",
    display_name: frame.display_name,
    archetype_id: frame.archetype_id,
    visual_prefab_key: frame.visual_prefab_key,
    visual_variant: frame.visual_variant,
    equipment: { equipment_visual_id: frame.equipment_visual_id }
  }, override);
}

function applyPermanentNpcFrameProfile(profile: any, frame: any): boolean {
  if (!profile || !profile.body || !frame) {
    return false;
  }

  var changed = false;
  var displayName = trimString(frame.display_name);
  if (displayName && trimString(profile.display_name) !== displayName) {
    profile.display_name = displayName;
    changed = true;
  }

  if (profile.actor_type !== "npc") {
    profile.actor_type = "npc";
    changed = true;
  }

  if (frame.visual_variant !== undefined &&
    normalizeVisualVariant(profile.body.visual_variant) !== normalizeVisualVariant(frame.visual_variant)) {
    profile.body.visual_variant = normalizeVisualVariant(frame.visual_variant);
    changed = true;
  }

  var visualKey = trimString(frame.visual_prefab_key);
  if (visualKey && trimString(profile.body.visual_prefab_key) !== visualKey) {
    profile.body.visual_prefab_key = visualKey;
    changed = true;
  }

  if (frame.equipment_visual_id !== undefined) {
    var equipment = normalizeEquipment({ equipment_visual_id: frame.equipment_visual_id });
    if (!profile.body.equipment ||
      Number(profile.body.equipment.equipment_visual_id || 0) !== Number(equipment.equipment_visual_id || 0)) {
      profile.body.equipment = equipment;
      changed = true;
    }
  }

  var request = buildPermanentNpcProfileRequest(frame);
  var archetype = selectBodyArchetype(frame.archetype_id);
  changed = setIfChanged(profile.body, "identity", normalizeFrameIdentity(request.identity || {}, profile.display_name, archetype, frame.npc_id, false)) || changed;
  changed = setIfChanged(profile.body, "stats", normalizeStatsWithDefaults(request.stats || {}, archetype.stats || {})) || changed;
  changed = setIfChanged(profile.body, "characteristics", normalizeTraitsWithDefaults(request.characteristics || {}, archetype.characteristics || {})) || changed;
  changed = setIfChanged(profile.body, "story", normalizeBodyStory(request.story || archetype.story || {})) || changed;
  changed = setIfChanged(profile.body, "soul", normalizeSoulWithDefaults(request.soul || { name: profile.display_name }, archetype.soul || {}, profile.display_name)) || changed;
  if (request.memory && typeof request.memory.length === "number") {
    changed = setIfChanged(profile, "memory", normalizeMemoryRecords(request.memory)) || changed;
  }

  return changed;
}

function readWorldActorProfile(nk: nkruntime.Nakama, ownerId: string, actorId: string): any {
  var objects = nk.storageRead([{
    collection: collectionActor,
    key: worldActorStorageKey(actorId),
    userId: ownerId
  }]);

  if (!objects || objects.length === 0) {
    return null;
  }

  return {
    value: objects[0].value,
    version: objects[0].version || null
  };
}

function writeWorldActorProfile(nk: nkruntime.Nakama, ownerId: string, profile: any, version: string): void {
  var write: any = {
    collection: collectionActor,
    key: worldActorStorageKey(profile.actor_id),
    userId: ownerId,
    value: profile,
    permissionRead: 2,
    permissionWrite: 0
  };
  if (typeof version === "string" && version.length > 0) {
    write.version = version;
  }
  nk.storageWrite([write]);
}

function readOpenClawBinding(nk: nkruntime.Nakama, ownerId: string, connectedAgentId: string): any {
  var objects = nk.storageRead([{
    collection: collectionOpenClaw,
    key: openClawBindingStorageKey(connectedAgentId),
    userId: ownerId
  }]);

  if (!objects || objects.length === 0) {
    return null;
  }

  return {
    binding: objects[0].value,
    value: objects[0].value,
    version: objects[0].version || null
  };
}

function writeOpenClawBinding(nk: nkruntime.Nakama, binding: any, version: string): void {
  var write: any = {
    collection: collectionOpenClaw,
    key: openClawBindingStorageKey(binding.connected_agent_id),
    userId: binding.owner_player_id,
    value: binding,
    permissionRead: 0,
    permissionWrite: 0
  };
  if (typeof version === "string" && version.length > 0) {
    write.version = version;
  }
  nk.storageWrite([write]);
}

function getOrCreateChatChannelState(nk: nkruntime.Nakama, ownerId: string, channelId: string): any {
  var existing = readChatChannel(nk, ownerId, channelId);
  if (existing) {
    return {
      channel: normalizeChatChannel(existing.value || {}, channelId),
      version: existing.version
    };
  }

  return {
    channel: normalizeChatChannel({}, channelId),
    version: "*"
  };
}

function readChatChannel(nk: nkruntime.Nakama, ownerId: string, channelId: string): any {
  var objects = nk.storageRead([{
    collection: collectionChat,
    key: chatChannelStorageKey(channelId),
    userId: ownerId
  }]);

  if (!objects || objects.length === 0) {
    return null;
  }

  return {
    value: objects[0].value,
    version: objects[0].version || null
  };
}

function writeChatChannel(nk: nkruntime.Nakama, ownerId: string, channel: any, version: string): void {
  var write: any = {
    collection: collectionChat,
    key: chatChannelStorageKey(channel.channel_id),
    userId: ownerId,
    value: channel,
    permissionRead: 2,
    permissionWrite: 0
  };
  if (typeof version === "string" && version.length > 0) {
    write.version = version;
  }
  nk.storageWrite([write]);
}

function addChatMessage(channel: any, message: any): void {
  channel.messages = channel.messages || [];
  channel.messages.push(message);
  channel.messages = boundChatMessages(channel.messages, chatMessageLogLimit);
  channel.updated_at = message.sent_at;
}

function requireOpenClawBindingState(nk: nkruntime.Nakama, ownerId: string, request: any): any {
  var connectedAgentId = normalizeOpenClawAgentId(request.connected_agent_id || request.agent_id);
  var bindingState = readOpenClawBinding(nk, ownerId, connectedAgentId);
  if (!bindingState) {
    throw new Error("OpenClaw binding not found");
  }
  bindingState.binding = normalizeExistingOpenClawBinding(bindingState.binding || {}, ownerId);
  return bindingState;
}

function normalizeOpenClawBinding(request: any, ownerId: string, frameActorId: string): any {
  var timestamp = new Date().toISOString();
  return {
    frame_actor_id: normalizeActorId(frameActorId || request.frame_actor_id || request.actor_id),
    controller_type: "openclaw",
    connected_agent_id: normalizeOpenClawAgentId(request.connected_agent_id || request.agent_id),
    owner_player_id: ownerId,
    connection_status: normalizeOpenClawConnectionStatus(request.connection_status || "connected"),
    agent_kind: normalizeOpenClawAgentKind(request.agent_kind),
    consent_scope: normalizeStringArray(request.consent_scope, ["dialogue", "heartbeat", "intent:say"]),
    moderation_state: normalizeOpenClawModerationState(request.moderation_state || "active"),
    rate_limit_profile: normalizeOpenClawRateLimit(request.rate_limit_profile || {}),
    created_at: trimString(request.created_at) || timestamp,
    updated_at: timestamp,
    last_seen_at: trimString(request.last_seen_at) || timestamp
  };
}

function normalizeExistingOpenClawBinding(binding: any, ownerId: string): any {
  var timestamp = new Date().toISOString();
  return {
    frame_actor_id: normalizeActorId(binding.frame_actor_id),
    controller_type: "openclaw",
    connected_agent_id: normalizeOpenClawAgentId(binding.connected_agent_id),
    owner_player_id: ownerId,
    connection_status: normalizeOpenClawConnectionStatus(binding.connection_status),
    agent_kind: normalizeOpenClawAgentKind(binding.agent_kind),
    consent_scope: normalizeStringArray(binding.consent_scope, ["dialogue", "heartbeat", "intent:say"]),
    moderation_state: normalizeOpenClawModerationState(binding.moderation_state),
    rate_limit_profile: normalizeOpenClawRateLimit(binding.rate_limit_profile || {}),
    created_at: trimString(binding.created_at) || timestamp,
    updated_at: trimString(binding.updated_at) || timestamp,
    last_seen_at: trimString(binding.last_seen_at) || timestamp
  };
}

function openClawContextResponse(binding: any, actorProfile: any): any {
  return {
    binding: binding,
    context: openClawFrameContext(actorProfile)
  };
}

function openClawFrameContext(actorProfile: any): any {
  var body = actorProfile.body || {};
  return {
    identity: cloneJson(body.identity || {}),
    soul: cloneJson(body.soul || {}),
    body: {
      body_id: trimString(body.body_id),
      archetype_id: trimString(body.archetype_id),
      visual_prefab_key: trimString(body.visual_prefab_key),
      visual_variant: normalizeVisualVariant(body.visual_variant),
      appearance: cloneJson(body.appearance || {}),
      inhabitation: cloneJson(body.inhabitation || {}),
      equipment: cloneJson(body.equipment || {}),
      stats: cloneJson(body.stats || {}),
      characteristics: cloneJson(body.characteristics || {}),
      story: cloneJson(body.story || {}),
      animation_capabilities: cloneJson(body.animation_capabilities || {}),
      time: cloneJson(body.time || {}),
      lifecycle: trimString(body.lifecycle) || "alive"
    },
    memory: cloneJson(actorProfile.memory || []),
    policy: cloneJson(body.agent_policy || {}),
    tools: cloneJson(body.tools || []),
    heartbeat: cloneJson(body.heartbeat || {})
  };
}

function ensureOpenClawBindingCanAct(binding: any): void {
  if (binding.connection_status !== "connected" && binding.connection_status !== "degraded") {
    throw new Error("OpenClaw binding is not connected");
  }
  if (binding.moderation_state !== "active" && binding.moderation_state !== "limited") {
    throw new Error("OpenClaw binding is not allowed by moderation state");
  }
}

function normalizeOpenClawIntent(request: any, actorProfile: any, binding: any): any {
  var intentName = trimString(request.intent || request.action);
  if (!intentName) {
    throw new Error("intent is required");
  }
  if (!frameAllowsIntent(actorProfile, intentName)) {
    throw new Error("intent is not allowed for this Frame");
  }
  if (!openClawConsentAllowsIntent(binding, intentName)) {
    throw new Error("intent is outside consent scope");
  }

  var timestamp = normalizeTimestamp(request.requested_at || request.occurred_at);
  return {
    id: trimString(request.id) || "openclaw-intent-" + normalizeActorId(actorProfile.actor_id) + "-" + timestamp,
    intent: intentName,
    payload: cloneJson(request.payload || {}),
    reason: trimString(request.reason) || "OpenClaw intent request.",
    requested_at: timestamp
  };
}

function openClawConsentAllowsIntent(binding: any, intentName: string): boolean {
  var scopes = binding && binding.consent_scope;
  if (!scopes || typeof scopes.length !== "number") {
    return false;
  }

  for (var i = 0; i < scopes.length; i += 1) {
    var scope = trimString(scopes[i]);
    if (scope === "all_intents" || scope === "intent:*" || scope === "intent:" + intentName) {
      return true;
    }
  }
  return false;
}

function frameAllowsIntent(actorProfile: any, intentName: string): boolean {
  var tools = actorProfile && actorProfile.body && actorProfile.body.tools;
  if (!tools || typeof tools.length !== "number") {
    return false;
  }

  for (var i = 0; i < tools.length; i += 1) {
    var tool = tools[i] || {};
    if (trimString(tool.intent) === intentName || trimString(tool.name) === intentName) {
      return tool.requires_validation !== false;
    }
  }
  return false;
}

function addActorActivity(profile: any, activity: any): boolean {
  profile.agent_runtime = profile.agent_runtime || defaultAgentRuntime(new Date().toISOString());
  profile.agent_activity = profile.agent_activity || [];
  if (activity.id && hasAgentActivityId(profile.agent_activity, activity.id)) {
    return false;
  }
  profile.agent_activity.unshift(activity);
  if (profile.agent_activity.length > agentActivityLogLimit) {
    profile.agent_activity = profile.agent_activity.slice(0, agentActivityLogLimit);
  }
  profile.agent_runtime.activity_count = addRuntimeMetric(profile.agent_runtime.activity_count, 1);
  profile.agent_runtime.last_activity_at = activity.occurred_at;
  return true;
}

function prototypeNpcInteraction(actorA: any, actorB: any, request: any, nk: nkruntime.Nakama): any {
  var timestamp = new Date().toISOString();
  var interactionId = trimString(request.id) || "npc-interaction-" + nk.uuidv4();
  var topic = normalizeNpcInteractionTopic(request.topic || request.kind);
  var lineA = prototypeNpcLine(actorA, actorB, topic);
  var lineB = prototypeNpcLine(actorB, actorA, topic);
  var summary = actorA.display_name + " and " + actorB.display_name + " discussed " + topic + ".";

  return {
    id: interactionId,
    occurred_at: timestamp,
    public_event: {
      id: interactionId,
      kind: "npc_interaction",
      topic: topic,
      occurred_at: timestamp,
      actor_a_id: actorA.actor_id,
      actor_a_name: actorA.display_name,
      actor_a_line: lineA,
      actor_b_id: actorB.actor_id,
      actor_b_name: actorB.display_name,
      actor_b_line: lineB,
      summary: summary
    },
    activity_a: {
      id: interactionId + "-a",
      kind: "npc_interaction",
      summary: "Spoke with " + actorB.display_name + " about " + topic + ": " + lineA,
      occurred_at: timestamp,
      source: "nakama",
      target_actor_id: actorB.actor_id,
      metrics: { social_interaction_count: 1 }
    },
    activity_b: {
      id: interactionId + "-b",
      kind: "npc_interaction",
      summary: "Spoke with " + actorA.display_name + " about " + topic + ": " + lineB,
      occurred_at: timestamp,
      source: "nakama",
      target_actor_id: actorA.actor_id,
      metrics: { social_interaction_count: 1 }
    },
    memory_a: {
      id: "mem-" + actorA.actor_id + "-" + interactionId,
      kind: "relationship",
      summary: "Talked with " + actorB.display_name + " about " + topic + ". " + lineB,
      importance: 5
    },
    memory_b: {
      id: "mem-" + actorB.actor_id + "-" + interactionId,
      kind: "relationship",
      summary: "Talked with " + actorA.display_name + " about " + topic + ". " + lineA,
      importance: 5
    }
  };
}

function npcInteractionRules(): any {
  return {
    max_distance_meters: npcInteractionMaxDistanceMeters,
    relationship_min_affinity_for_frequent_interaction: npcRelationshipMinAffinityForFrequent,
    hostility_block_threshold: npcHostilityBlockThreshold,
    frequent_interaction_count: npcFrequentInteractionCount,
    hard_limits: [
      "NPCs may only interact when server or Fusion context says they are nearby.",
      "NPCs may not repeatedly seek low-affinity actors once familiarity is high.",
      "NPCs may not voluntarily socialize with actors above the hostility block threshold."
    ],
    soft_prompt_guidance: [
      "Prefer NPCs with higher affinity and shared memories.",
      "Use hostility, familiarity, role, soul, and recent activity to choose tone.",
      "When affinity is low, keep exchanges short unless a quest, danger, or duty reason exists."
    ]
  };
}

function validateNpcIntentRules(actor: any, target: any, request: any): void {
  var distanceMeters = finiteNumberOrDefault(firstDefined(request.distance_meters, request.distance), 0);
  if (distanceMeters > npcInteractionMaxDistanceMeters) {
    throw new Error("NPC target is too far away for interaction");
  }
  if (!target) {
    return;
  }

  var relationship = findRelationshipRecord(actor.relationships || [], target.actor_id);
  if (relationship.hostility >= npcHostilityBlockThreshold) {
    throw new Error("NPC relationship hostility blocks voluntary interaction");
  }
  if (
    relationship.familiarity_count >= npcFrequentInteractionCount &&
    relationship.affinity < npcRelationshipMinAffinityForFrequent
  ) {
    throw new Error("NPC relationship affinity is too low for frequent interaction");
  }
}

function normalizeNpcInteractionTopic(value: any): string {
  var topic = sanitizeNakamaIdentifier(trimString(value), "bodytime");
  if (
    topic === "bodytime" ||
    topic === "patrol" ||
    topic === "memory" ||
    topic === "reincarnation" ||
    topic === "hub-rumor"
  ) {
    return topic;
  }

  return "bodytime";
}

function prototypeNpcLine(speaker: any, listener: any, topic: string): string {
  var soul = speaker && speaker.body && speaker.body.soul ? speaker.body.soul : {};
  var story = speaker && speaker.body && speaker.body.story ? speaker.body.story : {};
  var listenerName = listener && listener.display_name ? listener.display_name : "the other Frame";
  var drive = trimString(soul.core_drive) || trimString(story.conflict) || "keep this settlement alive";
  if (topic === "patrol") {
    return listenerName + ", keep the route tight. " + drive;
  }
  if (topic === "memory") {
    return listenerName + ", record this: " + (trimString(story.rumor) || drive);
  }
  if (topic === "reincarnation") {
    return listenerName + ", every borrowed body needs a cleaner transfer plan.";
  }
  if (topic === "hub-rumor") {
    return listenerName + ", I heard this again: " + (trimString(story.rumor) || drive);
  }
  return listenerName + ", watch the remaining SECOND. " + drive;
}

function normalizeNpcIntent(request: any, nk: nkruntime.Nakama): any {
  var timestamp = new Date().toISOString();
  var intent = trimString(request.intent || "say");
  var payload = request.payload || {};
  var text = trimString(payload.text || request.text || request.say);
  if (!text) {
    throw new Error("NPC say intent text is required");
  }
  if (text.length > chatMessageMaxLength) {
    text = text.substring(0, chatMessageMaxLength).trim();
  }

  var targetActorId = trimString(payload.target_actor_id || request.target_actor_id || request.target_id);
  if (targetActorId) {
    targetActorId = normalizeActorId(targetActorId);
    if (!findPermanentNpcFrame(targetActorId)) {
      throw new Error("unknown target NPC actor");
    }
  }

  return {
    id: normalizeNpcIntentId(request.id, nk),
    intent: intent,
    source: normalizeNpcIntentSource(request.source),
    reason: trimString(request.reason) || "LLM-driven NPC intent.",
    requested_at: timestamp,
    target_actor_id: targetActorId,
    payload: {
      text: text,
      target_actor_id: targetActorId
    }
  };
}

function normalizeNpcIntentId(value: any, nk: nkruntime.Nakama): string {
  var id = sanitizeNakamaIdentifier(trimString(value), "");
  if (id) {
    return id.length > 96 ? id.substring(0, 96) : id;
  }
  return "npc-intent-" + nk.uuidv4();
}

function normalizeNpcIntentSource(value: any): string {
  var source = trimString(value);
  if (source === "llm" || source === "gateway" || source === "debug" || source === "fallback") {
    return source;
  }
  return "llm";
}

function findRelationshipRecord(relationships: any[], actorId: string): any {
  var normalizedActorId = normalizeActorId(actorId);
  if (relationships && typeof relationships.length === "number") {
    for (var index = 0; index < relationships.length; index += 1) {
      var relationship = normalizeRelationshipRecord(relationships[index] || {});
      if (relationship.actor_id === normalizedActorId) {
        return relationship;
      }
    }
  }

  return normalizeRelationshipRecord({ actor_id: normalizedActorId });
}

function upsertRelationshipRecord(relationships: any[], target: any, affinityDelta: number, hostilityDelta: number): any[] {
  var normalizedTargetId = normalizeActorId(target.actor_id);
  var now = new Date().toISOString();
  var updated = false;
  var records: any[] = [];
  if (relationships && typeof relationships.length === "number") {
    for (var index = 0; index < relationships.length; index += 1) {
      var record = normalizeRelationshipRecord(relationships[index] || {});
      if (record.actor_id === normalizedTargetId) {
        record.display_name = trimString(target.display_name) || record.display_name;
        record.affinity = clampNumber(record.affinity + affinityDelta, -100, 100);
        record.hostility = clampNumber(record.hostility + hostilityDelta, 0, 100);
        record.familiarity_count = clampNumber(record.familiarity_count + 1, 0, 1000000);
        record.last_interaction_at = now;
        updated = true;
      }
      records.push(record);
    }
  }

  if (!updated) {
    records.push(normalizeRelationshipRecord({
      actor_id: normalizedTargetId,
      display_name: target.display_name,
      affinity: affinityDelta,
      hostility: hostilityDelta,
      familiarity_count: 1,
      last_interaction_at: now
    }));
  }

  records.sort(function (a: any, b: any): number {
    var familiarityDelta = Number(b.familiarity_count || 0) - Number(a.familiarity_count || 0);
    if (familiarityDelta !== 0) {
      return familiarityDelta;
    }
    return Number(b.affinity || 0) - Number(a.affinity || 0);
  });
  return records.length > 64 ? records.slice(0, 64) : records;
}

function normalizeRelationshipRecord(record: any): any {
  var actorId = normalizeActorId(record.actor_id || "unknown-actor");
  return {
    actor_id: actorId,
    display_name: trimString(record.display_name) || actorDisplayName(actorId),
    affinity: clampNumber(numberOrDefault(record.affinity, 0), -100, 100),
    hostility: clampNumber(numberOrDefault(record.hostility, 0), 0, 100),
    familiarity_count: clampNumber(numberOrDefault(record.familiarity_count, 0), 0, 1000000),
    last_interaction_at: trimString(record.last_interaction_at)
  };
}

function normalizeRelationshipRecords(records: any[]): any[] {
  var normalized: any[] = [];
  if (!records || typeof records.length !== "number") {
    return normalized;
  }

  for (var index = 0; index < records.length; index += 1) {
    normalized.push(normalizeRelationshipRecord(records[index] || {}));
  }
  return normalized.length > 64 ? normalized.slice(0, 64) : normalized;
}

function defaultActorProfile(ownerId: string, actorId: string, request: any): any {
  var timestamp = new Date().toISOString();
  var permanentFrame = findPermanentNpcFrame(actorId);
  var displayName = trimString(request.display_name) ||
    trimString(permanentFrame && permanentFrame.display_name) ||
    actorDisplayName(actorId);
  var actorType = normalizeActorType(request.actor_type || request.kind || (permanentFrame ? "npc" : ""));
  var archetype = selectBodyArchetype(request.archetype_id ||
    (permanentFrame && permanentFrame.archetype_id) ||
    ownerId + ":" + actorId);

  return ensureActorProfile({
    actor_id: actorId,
    actor_type: actorType,
    owner_player_id: ownerId,
    display_name: displayName,
    body: {
      body_id: "body-" + actorId,
      archetype_id: archetype.archetype_id,
      visual_prefab_key: trimString(request.visual_prefab_key) || archetype.visual_prefab_key,
      visual_variant: normalizeVisualVariant(firstDefined(request.visual_variant, archetype.visual_variant)),
      appearance: normalizeBodyAppearance(request.appearance || archetype.appearance || {}),
      inhabitation: normalizeBodyInhabitation(request.inhabitation || {
        source_actor_id: actorId,
        previous_role: (permanentFrame && permanentFrame.role) || (archetype.story && archetype.story.role),
        inhabited_by_player: false
      }, archetype, false, ownerId + ":" + actorId),
      equipment: normalizeEquipment(request.equipment || { equipment_visual_id: archetype.equipment_visual_id }),
      stats: normalizeStatsWithDefaults(request.stats || {}, archetype.stats || {}),
      characteristics: normalizeTraitsWithDefaults(request.characteristics || {}, archetype.characteristics || {}),
      story: normalizeBodyStory(request.story || archetype.story || {}),
      animation_capabilities: normalizeAnimationCapabilities(request.animation_capabilities || {}, archetype.animation_capabilities || {}),
      time: normalizeBodyTime(request.time || {}),
      lifecycle: "alive",
      identity: normalizeFrameIdentity(request.identity || {}, displayName, archetype, actorId, false),
      skills: normalizeFrameSkills(request.skills, archetype, equipmentOrArchetypeDefault(request.equipment, archetype)),
      agents: normalizeFrameAgents(request.agents, request.agent_policy || {}, archetype, false),
      tools: normalizeFrameTools(request.tools),
      heartbeat: normalizeFrameHeartbeat(request.heartbeat || {}, timestamp, "idle"),
      agent_policy: normalizePolicy(request.agent_policy || {}),
      soul: normalizeSoulWithDefaults(request.soul || { name: displayName }, archetype.soul || {}, displayName)
    },
    memory: normalizeMemoryRecords(request.memory || [{
      id: "seed-actor-origin",
      kind: "system",
      summary: trimString(archetype.seed_memory_summary) || "This actor is an NPC-like body profile with separate memory, stats, traits, soul, and policy.",
      importance: 6
    }]),
    relationships: normalizeRelationshipRecords(request.relationships || []),
    agent_runtime: defaultAgentRuntime(timestamp),
    agent_activity: [{
      id: "activity-bootstrap",
      kind: "profile_bootstrap",
      summary: "Initial actor profile was created.",
      occurred_at: timestamp,
      source: "nakama"
    }],
    created_at: timestamp,
    updated_at: timestamp
  }, ownerId, actorId);
}

function ensureActorProfile(profile: any, ownerId: string, actorId: string): any {
  var timestamp = new Date().toISOString();
  profile.actor_id = actorId;
  profile.actor_type = normalizeActorType(profile.actor_type);
  profile.owner_player_id = ownerId;
  profile.display_name = trimString(profile.display_name) || actorDisplayName(profile.actor_id);
  profile.body = profile.body || {};
  profile.body.body_id = trimString(profile.body.body_id) || "body-" + profile.actor_id;
  var archetype = selectBodyArchetype(profile.body.archetype_id || profile.owner_player_id + ":" + profile.actor_id);
  profile.body.archetype_id = trimString(profile.body.archetype_id) || archetype.archetype_id;
  profile.body.visual_prefab_key = trimString(profile.body.visual_prefab_key) || archetype.visual_prefab_key;
  profile.body.visual_variant = normalizeVisualVariant(firstDefined(profile.body.visual_variant, archetype.visual_variant));
  profile.body.appearance = normalizeBodyAppearance(profile.body.appearance || archetype.appearance || {});
  profile.body.inhabitation = normalizeBodyInhabitation(profile.body.inhabitation || {
    source_actor_id: profile.actor_id,
    previous_role: archetype.story && archetype.story.role,
    inhabited_by_player: false
  }, archetype, false, profile.owner_player_id + ":" + profile.actor_id);
  profile.body.equipment = normalizeEquipment(equipmentOrArchetypeDefault(profile.body.equipment, archetype));
  profile.body.stats = normalizeStatsWithDefaults(profile.body.stats || {}, archetype.stats || {});
  profile.body.characteristics = normalizeTraitsWithDefaults(profile.body.characteristics || {}, archetype.characteristics || {});
  profile.body.story = normalizeBodyStory(profile.body.story || archetype.story || {});
  profile.body.animation_capabilities = normalizeAnimationCapabilities(profile.body.animation_capabilities || {}, archetype.animation_capabilities || {});
  profile.body.time = normalizeBodyTime(profile.body.time || {});
  profile.body.lifecycle = trimString(profile.body.lifecycle) || "alive";
  profile.body.identity = normalizeFrameIdentity(profile.body.identity || {}, profile.display_name, archetype, profile.actor_id, false);
  profile.body.skills = normalizeFrameSkills(profile.body.skills, archetype, profile.body.equipment || {});
  profile.body.agents = normalizeFrameAgents(profile.body.agents, profile.body.agent_policy || {}, archetype, false);
  profile.body.tools = normalizeFrameTools(profile.body.tools);
  profile.body.heartbeat = normalizeFrameHeartbeat(profile.body.heartbeat || {}, timestamp, "idle");
  profile.body.agent_policy = normalizePolicy(profile.body.agent_policy || {});
  profile.body.soul = normalizeSoulWithDefaults(profile.body.soul || { name: profile.display_name }, archetype.soul || {}, profile.display_name);
  profile.memory = sortAndBoundMemories(profile.memory || []);
  profile.relationships = normalizeRelationshipRecords(profile.relationships || []);
  profile.agent_runtime = profile.agent_runtime || defaultAgentRuntime(timestamp);
  profile.agent_activity = profile.agent_activity || [];
  profile.created_at = trimString(profile.created_at) || timestamp;
  profile.updated_at = trimString(profile.updated_at) || timestamp;
  return profile;
}

function defaultAgentContext(playerId: string): any {
  var displayName = playerId || "Unknown Wanderer";
  var timestamp = new Date().toISOString();

  return {
    player: {
      player_id: playerId,
      display_name: displayName,
      second_balance_seconds: secondPrototypeStartingBalanceSeconds,
      reincarnation_count: 0,
      created_at: timestamp
    },
    body: defaultBodyProfile(playerId, displayName, timestamp)
  };
}

function defaultBodyProfile(playerId: string, displayName: string, timestamp: string, seedSuffix?: string): any {
  var assignmentSeed = playerId + ":" + (seedSuffix || "initial");
  var sourceFrame = selectPermanentNpcFrame(assignmentSeed);
  var archetype = sourceFrame
    ? selectBodyArchetype(sourceFrame.archetype_id)
    : selectBodyArchetype(assignmentSeed);
  var sourceActorId = sourceFrame
    ? sourceFrame.npc_id
    : sourceActorIdForArchetype(archetype, assignmentSeed);
  return {
    body_id: "body-" + playerId,
    archetype_id: archetype.archetype_id,
    visual_prefab_key: archetype.visual_prefab_key,
    visual_variant: normalizeVisualVariant(archetype.visual_variant),
    appearance: normalizeBodyAppearance(archetype.appearance || {}),
    inhabitation: normalizeBodyInhabitation({
      source_actor_id: sourceActorId,
      previous_role: (sourceFrame && sourceFrame.role) || (archetype.story && archetype.story.role),
      inhabited_by_player: true,
      assigned_at: timestamp
    }, archetype, true, assignmentSeed),
    equipment: normalizeEquipment({ equipment_visual_id: archetype.equipment_visual_id }),
    stats: normalizeStatsWithDefaults({}, archetype.stats || {}),
    characteristics: normalizeTraitsWithDefaults({}, archetype.characteristics || {}),
    story: normalizeBodyStory(archetype.story || {}),
    animation_capabilities: normalizeAnimationCapabilities({}, archetype.animation_capabilities || {}),
    time: {
      remaining_seconds: 86400,
      max_seconds: 86400,
      danger_drain_rate: 1
    },
    lifecycle: "alive",
    identity: normalizeFrameIdentity({}, displayName, archetype, sourceActorId, true),
    skills: normalizeFrameSkills([], archetype, { equipment_visual_id: archetype.equipment_visual_id }),
    agents: normalizeFrameAgents([], {}, archetype, true),
    tools: normalizeFrameTools([]),
    heartbeat: normalizeFrameHeartbeat({}, timestamp, "online"),
    agent_policy: normalizePolicy({}),
    soul: normalizeSoulWithDefaults({}, archetype.soul || {}, displayName),
    memory: [{
      id: "seed-origin",
      kind: "system",
      summary: trimString(archetype.seed_memory_summary) || "The character is a Second Spawn prototype body controlled by the player or their offline agent.",
      importance: 6
    }],
    agent_runtime: defaultAgentRuntime(timestamp),
    agent_activity: [{
      id: "activity-bootstrap",
      kind: "profile_bootstrap",
      summary: "Initial Nakama profile and prototype body stats were created.",
      occurred_at: timestamp,
      source: "nakama"
    }],
    created_at: timestamp
  };
}

function ensureAgentContext(context: any, playerId: string): any {
  var timestamp = new Date().toISOString();
  context.player = context.player || {};
  context.player.player_id = playerId;
  context.player.display_name = trimString(context.player.display_name) || context.player.player_id;
  context.player.created_at = trimString(context.player.created_at) || timestamp;
  ensureSecondBalance(context);
  context.body = context.body || {};
  context.body.body_id = trimString(context.body.body_id) || "body-" + context.player.player_id;
  var sourceFrame = selectPermanentNpcFrame(context.player.player_id + ":initial");
  var archetype = selectBodyArchetype(context.body.archetype_id ||
    (sourceFrame && sourceFrame.archetype_id) ||
    context.player.player_id + ":initial");
  context.body.archetype_id = trimString(context.body.archetype_id) || archetype.archetype_id;
  context.body.visual_prefab_key = trimString(context.body.visual_prefab_key) || archetype.visual_prefab_key;
  context.body.visual_variant = normalizeVisualVariant(firstDefined(context.body.visual_variant, archetype.visual_variant));
  context.body.appearance = normalizeBodyAppearance(context.body.appearance || archetype.appearance || {});
  context.body.inhabitation = normalizeBodyInhabitation(context.body.inhabitation || {
    source_actor_id: sourceFrame
      ? sourceFrame.npc_id
      : sourceActorIdForArchetype(archetype, context.player.player_id + ":initial"),
    previous_role: (sourceFrame && sourceFrame.role) || (archetype.story && archetype.story.role),
    inhabited_by_player: true
  }, archetype, true, context.player.player_id + ":initial");
  context.body.equipment = normalizeEquipment(equipmentOrArchetypeDefault(context.body.equipment, archetype));
  context.body.stats = normalizeStatsWithDefaults(context.body.stats || {}, archetype.stats || {});
  context.body.characteristics = normalizeTraitsWithDefaults(context.body.characteristics || {}, archetype.characteristics || {});
  context.body.story = normalizeBodyStory(context.body.story || archetype.story || {});
  context.body.animation_capabilities = normalizeAnimationCapabilities(context.body.animation_capabilities || {}, archetype.animation_capabilities || {});
  context.body.time = normalizeBodyTime(context.body.time || {});
  context.body.lifecycle = trimString(context.body.lifecycle) || "alive";
  context.body.identity = normalizeFrameIdentity(context.body.identity || {}, context.player.display_name, archetype, context.body.inhabitation && context.body.inhabitation.source_actor_id, true);
  context.body.skills = normalizeFrameSkills(context.body.skills, archetype, context.body.equipment || {});
  context.body.agents = normalizeFrameAgents(context.body.agents, context.body.agent_policy || {}, archetype, true);
  context.body.tools = normalizeFrameTools(context.body.tools);
  context.body.heartbeat = normalizeFrameHeartbeat(context.body.heartbeat || {}, timestamp, "online");
  context.body.agent_policy = normalizePolicy(context.body.agent_policy || {});
  context.body.soul = normalizeSoulWithDefaults(context.body.soul || {}, archetype.soul || {}, context.player.display_name);
  context.body.memory = sortAndBoundMemories(context.body.memory || []);
  context.body.created_at = trimString(context.body.created_at) || timestamp;
  ensureAgentRuntime(context);
  return context;
}

function ensureAgentRuntime(context: any): boolean {
  var changed = false;
  if (!context.body) {
    context.body = {};
    changed = true;
  }

  if (!context.body.agent_runtime) {
    context.body.agent_runtime = defaultAgentRuntime(new Date().toISOString());
    changed = true;
  }

  if (!context.body.agent_activity) {
    context.body.agent_activity = [];
    changed = true;
  }

  if (context.body.agent_activity.length === 0) {
    var timestamp = new Date().toISOString();
    context.body.agent_activity.push({
      id: "activity-bootstrap",
      kind: "profile_bootstrap",
      summary: "Nakama profile was normalized with agent runtime tracking.",
      occurred_at: timestamp,
      source: "nakama"
    });
    context.body.agent_runtime.activity_count = 1;
    context.body.agent_runtime.last_activity_at = timestamp;
    changed = true;
  }

  context.body.agent_runtime.decision_count = clampNumber(context.body.agent_runtime.decision_count || 0, 0, agentRuntimeMetricMax);
  context.body.agent_runtime.fallback_decision_count = clampNumber(context.body.agent_runtime.fallback_decision_count || 0, 0, agentRuntimeMetricMax);
  context.body.agent_runtime.move_intent_count = clampNumber(context.body.agent_runtime.move_intent_count || 0, 0, agentRuntimeMetricMax);
  context.body.agent_runtime.say_intent_count = clampNumber(context.body.agent_runtime.say_intent_count || 0, 0, agentRuntimeMetricMax);
  context.body.agent_runtime.stop_intent_count = clampNumber(context.body.agent_runtime.stop_intent_count || 0, 0, agentRuntimeMetricMax);
  context.body.agent_runtime.interact_intent_count = clampNumber(context.body.agent_runtime.interact_intent_count || 0, 0, agentRuntimeMetricMax);
  context.body.agent_runtime.offline_seconds = clampNumber(context.body.agent_runtime.offline_seconds || 0, 0, agentRuntimeMetricMax);
  context.body.agent_runtime.activity_count = clampNumber(context.body.agent_runtime.activity_count || context.body.agent_activity.length, 0, agentRuntimeMetricMax);
  return changed;
}

function ensureSecondBalance(context: any): void {
  if (!context.player) {
    context.player = {};
  }
  context.player.second_balance_seconds = clampNumber(
    Math.floor(finiteNumberOrDefault(context.player.second_balance_seconds, secondPrototypeStartingBalanceSeconds)),
    0,
    secondPrototypeMaxBalanceSeconds
  );
  context.player.reincarnation_count = clampNumber(
    Math.floor(finiteNumberOrDefault(context.player.reincarnation_count, 0)),
    0,
    agentRuntimeMetricMax
  );
}

function defaultAgentRuntime(timestamp: string): any {
  return {
    profile_bootstrapped_at: timestamp,
    last_profile_bootstrap_at: timestamp,
    last_activity_at: timestamp,
    activity_count: 1,
    decision_count: 0,
    fallback_decision_count: 0,
    move_intent_count: 0,
    say_intent_count: 0,
    stop_intent_count: 0,
    interact_intent_count: 0,
    offline_seconds: 0
  };
}

function selectBodyArchetype(seed: any): any {
  var requested = trimString(seed);
  var exact = findBodyArchetype(requested);
  if (exact) {
    return exact;
  }

  if (bodyArchetypePool.length === 0) {
    return {
      archetype_id: "prototype-hunter",
      visual_prefab_key: "generated_visual_12_swordsman",
      visual_variant: 12,
      equipment_visual_id: 2,
      appearance: {
        body_type: "synthetic_hunter",
        body_parts: {
          head: "prototype-head",
          face: "prototype-face",
          torso: "prototype-torso",
          arms: "prototype-arms",
          legs: "prototype-legs"
        },
        skin: "neutral",
        hair: "none",
        material: "prototype-polymer",
        marks: ["prototype"]
      },
      stats: defaultCharacterStats(),
      characteristics: {},
      soul: {},
      story: {},
      animation_capabilities: { supports_jump: true, supports_roll: true, supports_melee: true, supports_ranged: false, weapon_stance: "one_hand_melee" },
      seed_memory_summary: ""
    };
  }

  var index = stableHashIndex(requested || "default-body", bodyArchetypePool.length);
  return bodyArchetypePool[index];
}

function findBodyArchetype(archetypeId: string): any {
  var normalized = trimString(archetypeId);
  if (normalized === "prototype-hunter" || normalized === "prototype-npc") {
    return null;
  }

  for (var i = 0; i < bodyArchetypePool.length; i++) {
    if (bodyArchetypePool[i].archetype_id === normalized) {
      return bodyArchetypePool[i];
    }
  }
  return null;
}

function selectPermanentNpcFrame(seed: string): any {
  if (!permanentNpcFramePool || permanentNpcFramePool.length === 0) {
    return null;
  }

  return permanentNpcFramePool[stableHashIndex(seed || "default-frame", permanentNpcFramePool.length)];
}

function findPermanentNpcFrame(npcId: string): any {
  var normalized = normalizeActorId(npcId);
  for (var i = 0; i < permanentNpcFramePool.length; i += 1) {
    if (permanentNpcFramePool[i].npc_id === normalized) {
      return permanentNpcFramePool[i];
    }
  }
  return null;
}

function stableHashIndex(value: string, modulo: number): number {
  var hash = 2166136261;
  for (var i = 0; i < value.length; i++) {
    hash ^= value.charCodeAt(i);
    hash = (hash * 16777619) >>> 0;
  }
  return modulo <= 0 ? 0 : hash % modulo;
}

function sourceActorIdForArchetype(archetype: any, seed: string): string {
  var archetypeId = sanitizeNakamaIdentifier(archetype.archetype_id || "body", "body");
  return "npc-" + archetypeId + "-" + stableHashIndex(seed || archetypeId, 10000);
}

function defaultCharacterStats(): any {
  return {
    level: 1,
    strength: 8,
    agility: 8,
    endurance: 10,
    perception: 8,
    focus: 8,
    presence: 5,
    vitality: 10,
    force: 8,
    resilience: 8,
    max_health: 100,
    max_energy: 50,
    attack_power: 10,
    defense_power: 5
  };
}

function normalizeStatsWithDefaults(stats: any, defaults: any): any {
  var base = normalizeStats(defaults || {});
  var strength = clampNumber(numberOrDefault(firstDefined(stats.strength, stats.force), base.strength), 1, 9999);
  var agility = clampNumber(numberOrDefault(stats.agility, base.agility), 1, 9999);
  var endurance = clampNumber(numberOrDefault(firstDefined(stats.endurance, firstDefined(stats.vitality, stats.resilience)), base.endurance), 1, 9999);
  var perception = clampNumber(numberOrDefault(stats.perception, base.perception), 1, 9999);
  var focus = clampNumber(numberOrDefault(stats.focus, base.focus), 1, 9999);
  var presence = clampNumber(numberOrDefault(stats.presence, base.presence), 1, 9999);
  return {
    level: clampNumber(numberOrDefault(stats.level, base.level), 1, 100),
    strength: strength,
    agility: agility,
    endurance: endurance,
    perception: perception,
    focus: focus,
    presence: presence,
    vitality: clampNumber(numberOrDefault(stats.vitality, endurance), 1, 9999),
    force: clampNumber(numberOrDefault(stats.force, strength), 1, 9999),
    resilience: clampNumber(numberOrDefault(stats.resilience, endurance), 1, 9999),
    max_health: clampNumber(numberOrDefault(stats.max_health, base.max_health), 1, 999999),
    max_energy: clampNumber(numberOrDefault(stats.max_energy, base.max_energy), 0, 999999),
    attack_power: clampNumber(numberOrDefault(stats.attack_power, base.attack_power), 0, 999999),
    defense_power: clampNumber(numberOrDefault(stats.defense_power, base.defense_power), 0, 999999)
  };
}

function normalizeStats(stats: any): any {
  var defaults = defaultCharacterStats();
  var strength = clampNumber(numberOrDefault(firstDefined(stats.strength, stats.force), defaults.strength), 1, 9999);
  var agility = clampNumber(numberOrDefault(stats.agility, defaults.agility), 1, 9999);
  var endurance = clampNumber(numberOrDefault(firstDefined(stats.endurance, firstDefined(stats.vitality, stats.resilience)), defaults.endurance), 1, 9999);
  var perception = clampNumber(numberOrDefault(stats.perception, defaults.perception), 1, 9999);
  var focus = clampNumber(numberOrDefault(stats.focus, defaults.focus), 1, 9999);
  var presence = clampNumber(numberOrDefault(stats.presence, defaults.presence), 1, 9999);
  return {
    level: clampNumber(numberOrDefault(stats.level, defaults.level), 1, 100),
    strength: strength,
    agility: agility,
    endurance: endurance,
    perception: perception,
    focus: focus,
    presence: presence,
    vitality: clampNumber(numberOrDefault(stats.vitality, endurance), 1, 9999),
    force: clampNumber(numberOrDefault(stats.force, strength), 1, 9999),
    resilience: clampNumber(numberOrDefault(stats.resilience, endurance), 1, 9999),
    max_health: clampNumber(numberOrDefault(stats.max_health, defaults.max_health), 1, 999999),
    max_energy: clampNumber(numberOrDefault(stats.max_energy, defaults.max_energy), 0, 999999),
    attack_power: clampNumber(numberOrDefault(stats.attack_power, defaults.attack_power), 0, 999999),
    defense_power: clampNumber(numberOrDefault(stats.defense_power, defaults.defense_power), 0, 999999)
  };
}

function normalizeBodyTime(time: any): any {
  return {
    remaining_seconds: clampNumber(numberOrDefault(time.remaining_seconds, 86400), 0, 31536000),
    max_seconds: clampNumber(numberOrDefault(time.max_seconds, 86400), 1, 31536000),
    danger_drain_rate: clampNumber(numberOrDefault(time.danger_drain_rate, 1), 0, 1000)
  };
}

function ensureBodyTime(context: any): void {
  if (!context.body) {
    context.body = {};
  }

  if (!context.body.time) {
    context.body.time = {};
  }

  var time = context.body.time;
  var maxSeconds = finiteNumberOrDefault(time.max_seconds, 86400);
  time.max_seconds = clampNumber(Math.floor(maxSeconds), 1, bodyTimeMaxSeconds);

  var remainingSeconds = finiteNumberOrDefault(time.remaining_seconds, time.max_seconds);
  time.remaining_seconds = clampNumber(Math.floor(remainingSeconds), 0, time.max_seconds);

  var drainRate = finiteNumberOrDefault(time.danger_drain_rate, 1);
  time.danger_drain_rate = clampNumber(Math.floor(drainRate), 0, 3600);

  if (!context.body.lifecycle) {
    context.body.lifecycle = time.remaining_seconds <= 0 ? "dead" : "alive";
  }
}

function normalizeBodyTimeEvent(request: any, allowDebugFatalDrain: boolean): any {
  var kind = normalizeBodyTimeEventKind(request.kind);
  var source = normalizeBodyTimeEventSource(kind, request.source, allowDebugFatalDrain);
  var amount = normalizeBodyTimeAmount(kind, firstDefined(request.amount_seconds, request.seconds), source);
  return {
    id: trimString(request.id),
    kind: kind,
    source: source,
    amount_seconds: amount,
    note: trimString(request.note)
  };
}

function normalizeBodyTimeEventKind(kind: any): string {
  var value = trimString(kind);
  if (value === "earn" || value === "spend" || value === "drain") {
    return value;
  }
  throw new Error("body time event kind must be earn, spend, or drain");
}

function normalizeChatChannelId(value: any): string {
  var normalized = sanitizeNakamaIdentifier(trimString(value), "prototype-hub");
  return normalized.length > 48 ? normalized.substring(0, 48) : normalized;
}

function normalizeChatChannel(channel: any, channelId: string): any {
  var timestamp = new Date().toISOString();
  return {
    channel_id: channelId,
    messages: boundChatMessages(channel.messages || [], chatMessageLogLimit),
    created_at: trimString(channel.created_at) || timestamp,
    updated_at: trimString(channel.updated_at) || timestamp
  };
}

function normalizeChatMessage(request: any, userId: string, nk: nkruntime.Nakama): any {
  var text = trimString(request.text || request.message);
  if (!text) {
    throw new Error("chat message text is required");
  }

  if (text.length > chatMessageMaxLength) {
    text = text.substring(0, chatMessageMaxLength).trim();
  }

  return {
    id: trimString(request.id) || "chat-" + nk.uuidv4(),
    channel_id: "prototype-hub",
    sender_player_id: userId,
    sender_display_name: normalizeChatSenderName(request.sender_display_name || request.display_name, userId),
    text: text,
    sent_at: new Date().toISOString(),
    source: normalizeChatSource(request.source)
  };
}

function normalizeChatSenderName(value: any, userId: string): string {
  var name = trimString(value);
  if (!name) {
    return userId;
  }

  return name.length > 32 ? name.substring(0, 32).trim() : name;
}

function normalizeChatSource(value: any): string {
  var source = trimString(value);
  if (source === "player" || source === "agent" || source === "npc" || source === "system") {
    return source;
  }

  return "player";
}

function requirePrototypeReward(objectiveId: any): any {
  var normalized = normalizeRewardObjectiveId(objectiveId);
  for (var index = 0; index < prototypeRewardCatalog.length; index += 1) {
    var reward = prototypeRewardCatalog[index];
    if (reward.objective_id === normalized) {
      return reward;
    }
  }

  throw new Error("unknown prototype reward objective");
}

function normalizeRewardObjectiveId(value: any): string {
  var normalized = sanitizeNakamaIdentifier(trimString(value), "");
  if (!normalized) {
    throw new Error("objective_id is required");
  }
  if (normalized.length > 64) {
    throw new Error("objective_id is too long");
  }
  return normalized;
}

function normalizeRewardClaimId(value: any, objectiveId: string, nk: nkruntime.Nakama): string {
  var id = sanitizeNakamaIdentifier(trimString(value), "");
  if (id) {
    return id.length > 96 ? id.substring(0, 96) : id;
  }

  return "reward-" + normalizeRewardObjectiveId(objectiveId) + "-" + nk.uuidv4();
}

function boundChatMessages(messages: any[], limit: any): any[] {
  var max = clampNumber(limit || chatMessageLogLimit, 1, chatMessageLogLimit);
  var normalized: any[] = [];
  if (!messages || typeof messages.length !== "number") {
    return normalized;
  }

  var start = Math.max(0, messages.length - max);
  for (var index = start; index < messages.length; index++) {
    var message = messages[index] || {};
    normalized.push({
      id: trimString(message.id) || "chat-" + index,
      channel_id: normalizeChatChannelId(message.channel_id || "prototype-hub"),
      sender_player_id: trimString(message.sender_player_id) || "unknown",
      sender_display_name: normalizeChatSenderName(message.sender_display_name, trimString(message.sender_player_id) || "unknown"),
      text: trimString(message.text),
      sent_at: normalizeTimestamp(message.sent_at),
      source: normalizeChatSource(message.source)
    });
  }

  return normalized;
}

function normalizeBodyTimeEventSource(kind: string, source: any, allowDebugFatalDrain: boolean): string {
  var value = trimString(source);
  if (kind === "earn" && value === "prototype_safe_farming") {
    return value;
  }
  if (kind === "spend" && value === "prototype_service") {
    return value;
  }
  if (kind === "drain" && value === "danger_zone_tick") {
    return value;
  }
  if (kind === "drain" && allowDebugFatalDrain && value === bodyTimeDebugFatalDrainSource) {
    return value;
  }
  throw new Error("body time source is not allowed for " + kind);
}

function normalizeBodyTimeAmount(kind: string, amount: any, source: string): number {
  var numberValue = Number(amount);
  if (isNaN(numberValue) || !isFinite(numberValue) || numberValue <= 0) {
    throw new Error("body time amount_seconds must be a positive finite number");
  }

  var maxAmount = bodyTimeDrainCapSeconds;
  if (kind === "earn") {
    maxAmount = bodyTimeEarnCapSeconds;
  } else if (kind === "spend") {
    maxAmount = bodyTimeSpendCapSeconds;
  } else if (source === bodyTimeDebugFatalDrainSource) {
    maxAmount = bodyTimeMaxSeconds;
  }

  return clampNumber(Math.floor(numberValue), 1, maxAmount);
}

function debugBodyTimeEnabled(ctx: nkruntime.Context): boolean {
  return lowercase(ctx.env["SECOND_SPAWN_ENABLE_DEBUG_BODYTIME"] || "") === "true";
}

function applyBodyTimeEvent(context: any, event: any, nk: nkruntime.Nakama): void {
  ensureBodyTime(context);
  if (context.body.lifecycle === "dead") {
    throw new Error("body time cannot be changed on a dead body before reincarnation");
  }
  if (event.kind === "earn" && hasRecentBodyTimeEvent(context, event, bodyTimeEarnCooldownSeconds)) {
    throw new Error("body time earn source is on cooldown");
  }

  var time = context.body.time;
  var beforeSeconds = time.remaining_seconds;
  var delta = event.kind === "earn" ? event.amount_seconds : -event.amount_seconds;
  time.remaining_seconds = clampNumber(beforeSeconds + delta, 0, time.max_seconds);
  if (time.remaining_seconds <= 0) {
    context.body.lifecycle = "dead";
  }

  addAgentActivity(context, {
    id: event.id || "",
    kind: "body_time",
    summary: bodyTimeActivitySummary(event, beforeSeconds, time.remaining_seconds),
    source: "nakama",
    body_time_kind: event.kind,
    body_time_source: event.source,
    body_time_amount_seconds: event.amount_seconds,
    metrics: {
      body_time_delta_seconds: delta,
      body_time_before_seconds: beforeSeconds,
      body_time_after_seconds: time.remaining_seconds
    }
  }, nk);
}

function reincarnateBody(context: any, request: any, nk: nkruntime.Nakama): void {
  var timestamp = new Date().toISOString();
  var previousBody = context.body || {};
  var durableSoul = previousBody.soul || normalizeSoul({}, context.player.display_name || context.player.player_id);
  var durableMemory = previousBody.memory || [];
  var durablePolicy = previousBody.agent_policy || normalizePolicy({});
  var durableTraits = previousBody.characteristics || normalizeTraits({});
  var nextCount = Math.floor(context.player.reincarnation_count || 0) + 1;
  var nextBody = defaultBodyProfile(context.player.player_id, context.player.display_name || context.player.player_id, timestamp, "r" + nextCount);

  nextBody.body_id = "body-" + sanitizeNakamaIdentifier(context.player.player_id || "player", "player") + "-r" + nextCount;
  nextBody.soul = durableSoul;
  nextBody.memory = sortAndBoundMemories(durableMemory.concat([{
    id: newMemoryId({ player: context.player, body: { memory: durableMemory } }, nk),
    kind: "system",
    summary: "Consciousness transferred into a fresh prototype body through reincarnation.",
    importance: 8
  }]));
  nextBody.agent_policy = normalizePolicy(durablePolicy);
  nextBody.characteristics = normalizeTraits(durableTraits);
  nextBody.agent_runtime = previousBody.agent_runtime || defaultAgentRuntime(timestamp);
  nextBody.agent_activity = previousBody.agent_activity || [];
  nextBody.reincarnated_from_body_id = trimString(previousBody.body_id);
  nextBody.reincarnated_at = timestamp;

  context.player.second_balance_seconds -= secondPrototypeReincarnationCostSeconds;
  context.player.reincarnation_count = nextCount;
  context.body = nextBody;

  addAgentActivity(context, {
    id: trimString(request.id) || "",
    kind: "reincarnation",
    summary: "Reincarnated into a fresh prototype body for " + secondPrototypeReincarnationCostSeconds + " SECOND seconds.",
    occurred_at: timestamp,
    source: "nakama",
    metrics: {
      second_cost_seconds: secondPrototypeReincarnationCostSeconds,
      second_balance_after_seconds: context.player.second_balance_seconds,
      reincarnation_count: context.player.reincarnation_count
    }
  }, nk);
}

function hasRecentBodyTimeEvent(context: any, event: any, cooldownSeconds: number): boolean {
  var activities = context.body.agent_activity || [];
  var nowMs = new Date().getTime();
  for (var index = 0; index < activities.length; index += 1) {
    var activity = activities[index];
    if (
      activity &&
      activity.kind === "body_time" &&
      activity.body_time_kind === event.kind &&
      activity.body_time_source === event.source
    ) {
      var occurredMs = new Date(activity.occurred_at || "").getTime();
      if (!isNaN(occurredMs) && nowMs - occurredMs < cooldownSeconds * 1000) {
        return true;
      }
    }
  }
  return false;
}

function bodyTimeActivitySummary(event: any, beforeSeconds: number, afterSeconds: number): string {
  var verb = event.kind === "earn" ? "earned" : event.kind === "spend" ? "spent" : "drained";
  var summary = "BodyTime " + verb + " " + event.amount_seconds + "s from " + event.source + ".";
  if (event.note) {
    summary += " " + event.note;
  }
  if (afterSeconds <= 0 && beforeSeconds > 0) {
    summary += " Body reached zero time and died.";
  }
  return summary;
}

function recordAgentDecision(context: any, decision: any, nk: nkruntime.Nakama): void {
  ensureAgentRuntime(context);
  var runtime = context.body.agent_runtime;
  runtime.decision_count += 1;
  if (decision.source === "fallback") {
    runtime.fallback_decision_count += 1;
  }
  if (decision.source === "fallback" && shouldBackoffModelDecision(decision.source_reason)) {
    runtime.model_backoff_until = new Date(new Date().getTime() + dosAiDecisionBackoffSeconds * 1000).toISOString();
  }

  incrementDecisionAction(runtime, decision.action);
  var summary = "Agent chose " + trimString(decision.action || "unknown") + ": " + trimString(decision.reason || "no reason provided");
  if (shouldRecordDecisionActivity(context, summary)) {
    addAgentActivity(context, {
      kind: "agent_decision",
      summary: summary,
      source: "nakama",
      metrics: {
        decisions_made: 1
      }
    }, nk);
  }
}

function recordAndWriteAgentDecisionWithRetry(
  nk: nkruntime.Nakama,
  userId: string,
  context: any,
  version: string,
  decision: any
): void {
  var latestContext = context;
  var latestVersion = version;
  var lastError: any = null;

  for (var attempt = 0; attempt < 4; attempt += 1) {
    var writableContext = cloneJson(latestContext);
    recordAgentDecision(writableContext, decision, nk);
    try {
      writeAgentContext(nk, writableContext, latestVersion);
      return;
    } catch (err) {
      if (!isStorageVersionConflict(err)) {
        throw err;
      }

      lastError = err;
      var latest = readAgentContext(nk, userId);
      if (!latest) {
        throw err;
      }

      latestContext = ensureAgentContext(latest.value || {}, userId);
      latestVersion = latest.version;
    }
  }

  throw lastError || new Error("agent decision write conflict");
}

function shouldRecordDecisionActivity(context: any, summary: string): boolean {
  var activities = context.body.agent_activity || [];
  if (activities.length === 0) {
    return true;
  }

  var latest = activities[0];
  return latest.kind !== "agent_decision" || trimString(latest.summary) !== summary;
}

function selectInteractTargetId(world: any): string {
  var targetId = trimString(world.focus_target_id || world.target_id || world.interact_target_id);
  if (targetId) {
    return targetId;
  }

  var nearbyObjects = world.nearby_objects || [];
  for (var index = 0; index < nearbyObjects.length; index += 1) {
    var nearbyId = trimString(nearbyObjects[index] && nearbyObjects[index].id);
    if (nearbyId) {
      return nearbyId;
    }
  }

  return "";
}

function tryDosAiAgentDecision(
  ctx: nkruntime.Context,
  logger: nkruntime.Logger,
  nk: nkruntime.Nakama,
  context: any,
  request: any,
  world: any,
  allowed: string[]
): any {
  var apiKey = trimString(ctx.env["DOS_AI_API_KEY"]);
  if (!apiKey) {
    return { decision: null, reason: "dos_ai_unconfigured" };
  }

  var baseUrl = trimTrailingSlash(ctx.env["DOS_AI_BASE_URL"] || "https://api.dos.ai/v1");
  var endpoint = baseUrl;
  if (endpoint.indexOf("/chat/completions") < 0) {
    endpoint += "/chat/completions";
  }

  var model = trimString(ctx.env["AGENT_DECISION_MODEL"] || "dos-ai") || "dos-ai";
  var body = {
    model: model,
    messages: [
      { role: "system", content: dosAiAgentDecisionSystemPrompt() },
      { role: "user", content: dosAiAgentDecisionUserPrompt(context, request, world, allowed) }
    ],
    max_tokens: dosAiDecisionMaxTokens,
    stream: false
  };

  var response: any;
  try {
    response = nk.httpRequest(endpoint, "post", {
      "content-type": "application/json",
      "accept": "application/json",
      "authorization": "Bearer " + apiKey
    }, JSON.stringify(body));
  } catch (err) {
    logger.info("DOS.AI decision request threw: " + err);
    return { decision: null, reason: isTimeoutLikeError(err) ? "dos_ai_timeout" : "dos_ai_exception" };
  }

  if (response.code < 200 || response.code > 299) {
    logger.info("DOS.AI decision request failed with status " + response.code);
    return { decision: null, reason: "dos_ai_http_" + response.code };
  }

  try {
    var decoded = parseJsonOrNull(response.body);
    var content = trimString(decoded && decoded.choices && decoded.choices[0] && decoded.choices[0].message && decoded.choices[0].message.content);
    if (!content) {
      return { decision: null, reason: "dos_ai_empty_content" };
    }

    var decision = parseModelDecisionContent(content);
    var validationError = validateAgentDecisionIntent(decision, allowed, world);
    if (validationError) {
      logger.info("DOS.AI decision rejected: " + validationError);
      return { decision: null, reason: "dos_ai_validate_error" };
    }

    decision.action = trimString(decision.action).toLowerCase();
    decision.reason = trimString(decision.reason || "model_selected_intent");
    decision.confidence = clampNumber(numberOrDefault(decision.confidence, 0.5), 0, 1);
    decision.source = "model";
    decision.source_reason = "dos_ai_validated_intent";
    return { decision: decision, reason: "" };
  } catch (err) {
    logger.info("DOS.AI decision request threw: " + err);
    return { decision: null, reason: "dos_ai_exception" };
  }
}

function isTimeoutLikeError(err: any): boolean {
  var message = lowercase(String(err || ""));
  return message.indexOf("timeout") >= 0 ||
    message.indexOf("timed out") >= 0 ||
    message.indexOf("deadline exceeded") >= 0 ||
    message.indexOf("context deadline") >= 0;
}

function isAgentModelBackoffActive(context: any): boolean {
  ensureAgentRuntime(context);
  var backoffUntil = trimString(context.body.agent_runtime.model_backoff_until);
  if (!backoffUntil) {
    return false;
  }

  var backoffMs = new Date(backoffUntil).getTime();
  return !isNaN(backoffMs) && isFinite(backoffMs) && backoffMs > new Date().getTime();
}

function dosAiAgentDecisionSystemPrompt(): string {
  return [
    "You are the SECOND SPAWN NPC and offline-agent decision node.",
    "Return exactly one JSON object and no prose.",
    "Your JSON must match this shape:",
    "{\"action\":\"stop|move|attack|interact|say\",\"target_id\":\"optional\",\"move\":{\"x\":0,\"z\":0},\"say\":\"optional\",\"reason\":\"short safety-grounded reason\",\"confidence\":0.0}",
    "Never grant items, currency, XP, BodyTime, quest progress, inventory, wallet actions, or authoritative state.",
    "Choose only an action present in the allowed list.",
    "Use SOUL for motive and voice, MEMORY for relationship context, and world_snapshot for who is nearby.",
    "Use stop when policy, BodyTime, danger, hostility, or uncertainty makes action unsafe."
  ].join("\n");
}

function dosAiAgentDecisionUserPrompt(context: any, request: any, world: any, allowed: string[]): string {
  return JSON.stringify({
    agent_context: compactAgentDecisionContext(context),
    allowed_actions: allowed,
    world_snapshot: compactDecisionWorld(world),
    request: {
      player_id: context && context.player && context.player.player_id,
      body_id: context && context.body && context.body.body_id
    }
  });
}

function compactAgentDecisionContext(context: any): any {
  var body = context && context.body ? context.body : {};
  var identity = body.identity || {};
  var soul = body.soul || {};
  var policy = body.agent_policy || {};
  return {
    player: {
      player_id: context && context.player && context.player.player_id,
      display_name: context && context.player && context.player.display_name
    },
    body: {
      body_id: body.body_id,
      display_name: body.display_name,
      level: body.level,
      lifecycle_state: body.lifecycle_state,
      time: {
        remaining_seconds: body.time && body.time.remaining_seconds,
        drain_per_tick: body.time && body.time.drain_per_tick
      },
      stats: body.stats || {},
      identity: {
        public_name: identity.public_name,
        public_role: identity.public_role,
        faction_title: identity.faction_title,
        profession: identity.profession,
        age_years: identity.age_years,
        reputation_summary: identity.reputation_summary
      },
      characteristics: body.characteristics || {},
      soul: {
        core_drive: soul.core_drive,
        temperament: soul.temperament,
        combat_style: soul.combat_style,
        social_style: soul.social_style,
        moral_boundaries: soul.moral_boundaries,
        long_term_goals: soul.long_term_goals
      },
      memory: compactDecisionMemories(body.memory || []),
      agent_policy: {
        stop_when_body_time_below: policy.stop_when_body_time_below,
        allow_autonomous_combat: policy.allow_autonomous_combat,
        allow_social: policy.allow_social,
        allow_loot: policy.allow_loot
      }
    }
  };
}

function compactDecisionMemories(memories: any[]): any[] {
  var sorted = sortAndBoundMemories((memories || []).slice());
  var result: any[] = [];
  for (var index = 0; index < sorted.length && result.length < dosAiDecisionMemoryCap; index += 1) {
    result.push({
      kind: sorted[index].kind,
      summary: sorted[index].summary,
      importance: sorted[index].importance
    });
  }
  return result;
}

function compactDecisionWorld(world: any): any {
  var nearbyActors = world && world.nearby_actors && typeof world.nearby_actors.length === "number"
    ? world.nearby_actors.slice(0, 6)
    : [];
  var nearbyObjects = world && world.nearby_objects && typeof world.nearby_objects.length === "number"
    ? world.nearby_objects.slice(0, 6)
    : [];
  return {
    position: world && world.position,
    body_time_seconds: world && world.body_time_seconds,
    threat_level: world && world.threat_level,
    zone_id: world && world.zone_id,
    nearby_actors: nearbyActors,
    nearby_objects: nearbyObjects
  };
}

function parseModelDecisionContent(content: string): any {
  var normalized = trimString(content);
  if (normalized.indexOf("```json") === 0) {
    normalized = trimString(normalized.substring(7));
  } else if (normalized.indexOf("```") === 0) {
    normalized = trimString(normalized.substring(3));
  }
  if (normalized.lastIndexOf("```") === normalized.length - 3) {
    normalized = trimString(normalized.substring(0, normalized.length - 3));
  }
  return parseJson(normalized, "model decision");
}

function validateAgentDecisionIntent(decision: any, allowed: string[], world: any): string {
  if (!decision || typeof decision !== "object") {
    return "decision is required";
  }

  var action = trimString(decision.action).toLowerCase();
  if (!arrayContains(allowed, action)) {
    return "action is not allowed";
  }

  if (action === "move") {
    if (!decision.move || isNaN(Number(decision.move.x)) || isNaN(Number(decision.move.z))) {
      return "move requires coordinates";
    }
  } else if (action === "say") {
    if (!trimString(decision.say)) {
      return "say requires text";
    }
    var sayTarget = trimString(decision.target_id);
    if (sayTarget && !isNearbyActor(world, sayTarget)) {
      return "say target is not nearby";
    }
  } else if (action === "interact" || action === "attack") {
    var targetId = trimString(decision.target_id);
    if (!targetId) {
      return action + " requires target_id";
    }
    if (!isNearbyObject(world, targetId) && !isNearbyActor(world, targetId) && !isNearbyTarget(world, targetId)) {
      return action + " target is not nearby";
    }
  }

  return "";
}

function normalizeAllowedActions(values: any): string[] {
  var allowed = normalizeStringArray(values, ["stop"]);
  for (var index = 0; index < allowed.length; index += 1) {
    allowed[index] = trimString(allowed[index]).toLowerCase();
  }
  if (!arrayContains(allowed, "stop")) {
    allowed.push("stop");
  }
  return allowed;
}

function isNearbyObject(world: any, targetId: string): boolean {
  var nearbyObjects = world && world.nearby_objects ? world.nearby_objects : [];
  for (var index = 0; index < nearbyObjects.length; index += 1) {
    if (trimString(nearbyObjects[index] && nearbyObjects[index].id) === targetId) {
      return true;
    }
  }
  return false;
}

function isNearbyActor(world: any, targetId: string): boolean {
  var nearbyActors = world && world.nearby_actors ? world.nearby_actors : [];
  for (var index = 0; index < nearbyActors.length; index += 1) {
    if (trimString(nearbyActors[index] && nearbyActors[index].id) === targetId) {
      return true;
    }
  }

  var nearbyObjects = world && world.nearby_objects ? world.nearby_objects : [];
  for (var objectIndex = 0; objectIndex < nearbyObjects.length; objectIndex += 1) {
    var nearbyObject = nearbyObjects[objectIndex];
    if (trimString(nearbyObject && nearbyObject.id) === targetId && trimString(nearbyObject && nearbyObject.kind) === "nearby_actor") {
      return true;
    }
  }
  return false;
}

function isNearbyTarget(world: any, targetId: string): boolean {
  var nearbyTargets = world && world.nearby_targets ? world.nearby_targets : [];
  for (var index = 0; index < nearbyTargets.length; index += 1) {
    if (trimString(nearbyTargets[index] && nearbyTargets[index].id) === targetId) {
      return true;
    }
  }
  return false;
}

function incrementDecisionAction(runtime: any, action: string): void {
  switch (trimString(action)) {
    case "move":
      runtime.move_intent_count += 1;
      break;
    case "say":
      runtime.say_intent_count += 1;
      break;
    case "interact":
      runtime.interact_intent_count += 1;
      break;
    case "stop":
      runtime.stop_intent_count += 1;
      break;
  }
}

function normalizeAgentActivity(context: any, request: any, nk: nkruntime.Nakama): any {
  var kind = normalizeAgentActivityKind(request.kind);
  var summary = trimString(request.summary);
  if (!summary) {
    throw new Error("agent activity summary is required");
  }

  return {
    id: trimString(request.id) || newActivityId(context, nk),
    kind: kind,
    summary: summary,
    occurred_at: normalizeTimestamp(request.occurred_at),
    source: trimString(request.source) || "client",
    metrics: request.metrics || {}
  };
}

function normalizeAgentActivityKind(kind: any): string {
  var value = trimString(kind);
  if (
    value === "profile_bootstrap" ||
    value === "offline_session" ||
    value === "agent_decision" ||
    value === "body_time" ||
    value === "reincarnation" ||
    value === "memory_sync" ||
    value === "openclaw_intent" ||
    value === "openclaw_heartbeat" ||
    value === "manual_note"
  ) {
    return value;
  }
  return "manual_note";
}

function addAgentActivity(context: any, activity: any, nk: nkruntime.Nakama): boolean {
  ensureAgentRuntime(context);
  var activities = context.body.agent_activity || [];
  if (!activity.id) {
    activity.id = newActivityId(context, nk);
  } else if (hasAgentActivityId(activities, activity.id)) {
    return false;
  }
  if (!activity.occurred_at) {
    activity.occurred_at = new Date().toISOString();
  }
  if (!activity.source) {
    activity.source = "nakama";
  }

  activities.unshift(activity);
  if (activities.length > agentActivityLogLimit) {
    activities = activities.slice(0, agentActivityLogLimit);
  }
  context.body.agent_activity = activities;
  context.body.agent_runtime.activity_count += 1;
  context.body.agent_runtime.last_activity_at = activity.occurred_at;
  context.body.heartbeat = normalizeFrameHeartbeat(context.body.heartbeat || {}, activity.occurred_at, activity.kind === "offline_session" ? "offline" : "online");
  context.body.heartbeat.last_seen_at = activity.occurred_at;
  context.body.heartbeat.last_action_summary = activity.summary;
  return true;
}

function hasAgentActivityId(activities: any[], activityId: string): boolean {
  var normalizedId = trimString(activityId);
  for (var index = 0; index < activities.length; index += 1) {
    if (trimString(activities[index] && activities[index].id) === normalizedId) {
      return true;
    }
  }
  return false;
}

function applyActivityMetrics(runtime: any, metrics: any): void {
  runtime.offline_seconds = addRuntimeMetric(runtime.offline_seconds, metrics.offline_seconds);
  runtime.decision_count = addRuntimeMetric(runtime.decision_count, metrics.decisions_made || metrics.decision_count);
  runtime.fallback_decision_count = addRuntimeMetric(runtime.fallback_decision_count, metrics.fallback_decisions || metrics.fallback_decision_count);
  runtime.move_intent_count = addRuntimeMetric(runtime.move_intent_count, metrics.move_intents || metrics.move_intent_count);
  runtime.say_intent_count = addRuntimeMetric(runtime.say_intent_count, metrics.say_intents || metrics.say_intent_count);
  runtime.stop_intent_count = addRuntimeMetric(runtime.stop_intent_count, metrics.stop_intents || metrics.stop_intent_count);
  runtime.interact_intent_count = addRuntimeMetric(runtime.interact_intent_count, metrics.interact_intents || metrics.interact_intent_count);
}

function addRuntimeMetric(current: any, increment: any): number {
  return clampNumber(Number(current || 0) + positiveMetric(increment), 0, agentRuntimeMetricMax);
}

function positiveMetric(value: any): number {
  var numberValue = Number(value || 0);
  if (isNaN(numberValue) || !isFinite(numberValue) || numberValue < 0) {
    return 0;
  }
  return Math.floor(numberValue);
}

function normalizeTimestamp(value: any): string {
  var timestamp = trimString(value);
  if (!timestamp) {
    return new Date().toISOString();
  }

  var parsed = new Date(timestamp).getTime();
  if (isNaN(parsed) || !isFinite(parsed)) {
    return new Date().toISOString();
  }

  return new Date(parsed).toISOString();
}

function upsertMemory(context: any, memory: any): void {
  context.body.memory = upsertMemoryRecord(context.body.memory || [], memory);
}

function upsertMemoryRecord(memories: any[], memory: any): any[] {
  for (var i = 0; i < memories.length; i++) {
    var existing = memories[i];
    if (existing.kind === memory.kind && lowercase(trimString(existing.summary)) === lowercase(memory.summary)) {
      if (memory.importance > existing.importance) {
        existing.importance = memory.importance;
      }
      return sortAndBoundMemories(memories);
    }
  }

  memories.push(memory);
  return sortAndBoundMemories(memories);
}

function sortAndBoundMemories(memories: any[]): any[] {
  memories.sort(function (a: any, b: any): number {
    var importanceDelta = Number(b.importance || 0) - Number(a.importance || 0);
    if (importanceDelta !== 0) {
      return importanceDelta;
    }
    return String(b.id || "").localeCompare(String(a.id || ""));
  });

  if (memories.length > 64) {
    return memories.slice(0, 64);
  }
  return memories;
}

function normalizeSoul(soul: any, fallbackName: string): any {
  return {
    name: trimString(soul.name) || fallbackName,
    core_drive: trimString(soul.core_drive) || "survive, learn the zone, and preserve agency for the player",
    temperament: trimString(soul.temperament) || "careful but curious",
    combat_style: trimString(soul.combat_style) || "avoid risky fights, kite when threatened",
    social_style: trimString(soul.social_style) || "brief, grounded, and helpful",
    moral_boundaries: normalizeStringArray(soul.moral_boundaries, [
      "do not betray allies",
      "do not spend scarce resources without permission"
    ]),
    long_term_goals: normalizeStringArray(soul.long_term_goals, [
      "survive the next expedition",
      "build trusted relationships with NPCs"
    ]),
    player_notes: trimString(soul.player_notes) || "prototype default soul",
    reincarnation_lore: trimString(soul.reincarnation_lore) || "a synthetic body carrying a persistent consciousness imprint"
  };
}

function normalizeSoulWithDefaults(soul: any, defaults: any, fallbackName: string): any {
  var merged = {
    name: firstDefined(soul.name, defaults.name),
    core_drive: firstDefined(soul.core_drive, defaults.core_drive),
    temperament: firstDefined(soul.temperament, defaults.temperament),
    combat_style: firstDefined(soul.combat_style, defaults.combat_style),
    social_style: firstDefined(soul.social_style, defaults.social_style),
    moral_boundaries: firstDefined(soul.moral_boundaries, defaults.moral_boundaries),
    long_term_goals: firstDefined(soul.long_term_goals, defaults.long_term_goals),
    player_notes: firstDefined(soul.player_notes, defaults.player_notes),
    reincarnation_lore: firstDefined(soul.reincarnation_lore, defaults.reincarnation_lore)
  };
  return normalizeSoul(merged, fallbackName);
}

function normalizeTraits(traits: any): any {
  return {
    curiosity: clampNumber(traits.curiosity || 6, 1, 10),
    courage: clampNumber(traits.courage || 5, 1, 10),
    empathy: clampNumber(traits.empathy || 5, 1, 10),
    discipline: clampNumber(traits.discipline || 5, 1, 10),
    aggression: clampNumber(traits.aggression || 3, 1, 10),
    sociability: clampNumber(traits.sociability || 5, 1, 10)
  };
}

function normalizeTraitsWithDefaults(traits: any, defaults: any): any {
  return normalizeTraits({
    curiosity: firstDefined(traits.curiosity, defaults.curiosity),
    courage: firstDefined(traits.courage, defaults.courage),
    empathy: firstDefined(traits.empathy, defaults.empathy),
    discipline: firstDefined(traits.discipline, defaults.discipline),
    aggression: firstDefined(traits.aggression, defaults.aggression),
    sociability: firstDefined(traits.sociability, defaults.sociability)
  });
}

function normalizePolicy(policy: any): any {
  return {
    enabled: policy.enabled === false ? false : true,
    mode: trimString(policy.mode) || "observe_and_keep_safe",
    max_session_seconds: clampNumber(policy.max_session_seconds || 1800, 60, 86400),
    allow_body_time_spend: policy.allow_body_time_spend === true,
    allow_risky_combat: policy.allow_risky_combat === true,
    preferred_activities: normalizeStringArray(policy.preferred_activities, ["explore", "talk", "safe_farming"]),
    forbidden_activities: normalizeStringArray(policy.forbidden_activities, ["spend_body_time", "start_pvp", "trade_items"]),
    stop_when_body_time_below: clampNumber(policy.stop_when_body_time_below || 900, 60, 86400)
  };
}

function normalizeVisualVariant(value: any): number {
  return clampNumber(numberOrDefault(value, 12), 0, prototypeVisualVariantMax);
}

function normalizeBodyStory(story: any): any {
  return {
    origin: trimString(story.origin) || "A synthetic body with a partial pre-player history.",
    role: trimString(story.role) || "Wanderer body",
    conflict: trimString(story.conflict) || "The body carries old habits that may not match its new consciousness.",
    rumor: trimString(story.rumor) || "No one knows which memories belong to the body and which belong to the soul."
  };
}

function normalizeFrameIdentity(identity: any, displayName: string, archetype: any, sourceActorId: string, inhabitedByPlayer: boolean): any {
  var role = trimString(identity.public_role) ||
    trimString(identity.role) ||
    trimString(archetype && archetype.story && archetype.story.role) ||
    "Wanderer body";
  var publicName = trimString(identity.public_name) ||
    trimString(identity.name) ||
    displayName ||
    actorDisplayName(sourceActorId || "frame");
  var callsign = trimString(identity.callsign) ||
    trimString(sourceActorId) ||
    sanitizeNakamaIdentifier(publicName, "frame");

  return {
    public_name: publicName,
    callsign: callsign,
    public_role: role,
    faction_title: trimString(identity.faction_title) || "Unaffiliated Frame",
    profession: trimString(identity.profession) || role,
    age_years: clampNumber(firstDefined(identity.age_years, identity.age), 0, 200),
    age_band: trimString(identity.age_band) || "unknown",
    home_base: trimString(identity.home_base) || "unknown",
    reputation_summary: trimString(identity.reputation_summary) ||
      (inhabitedByPlayer
        ? "Newly inhabited body. Reputation is still being rebuilt under player control."
        : "World NPC Frame with server-owned behavior and public identity.")
  };
}

function normalizeFrameSkills(skills: any, archetype: any, equipment: any): any[] {
  var normalized: any[] = [];
  if (skills && typeof skills.length === "number") {
    for (var i = 0; i < skills.length && normalized.length < 12; i += 1) {
      var skill = normalizeFrameSkill(skills[i], i);
      if (skill.id) {
        normalized.push(skill);
      }
    }
  }

  if (normalized.length > 0) {
    return normalized;
  }

  var role = trimString(archetype && archetype.story && archetype.story.role) || "Wanderer body";
  var equipmentVisualId = clampNumber((equipment && equipment.equipment_visual_id) || (archetype && archetype.equipment_visual_id) || 0, 0, 9);
  var defaults = equipmentVisualDefaults(equipmentVisualId);
  return [
    normalizeFrameSkill({
      id: "skill-body-role",
      name: role,
      category: "profession",
      rank: 1,
      summary: "Prototype profession capability derived from the current Frame role."
    }, 0),
    normalizeFrameSkill({
      id: "skill-combat-kit",
      name: trimString(defaults.weapon_visual_key) || primaryWeaponName(equipmentVisualId),
      category: "combat",
      rank: 1,
      summary: "Prototype combat kit derived from the server-selected body equipment."
    }, 1)
  ];
}

function normalizeFrameSkill(skill: any, index: number): any {
  return {
    id: trimString(skill && skill.id) || "skill-" + (index + 1),
    name: trimString(skill && skill.name) || "Prototype skill",
    category: trimString(skill && skill.category) || "profession",
    rank: clampNumber((skill && skill.rank) || 1, 1, 100),
    summary: trimString(skill && skill.summary) || "Prototype skill entry."
  };
}

function normalizeFrameAgents(agents: any, policy: any, archetype: any, inhabitedByPlayer: boolean): any[] {
  var normalized: any[] = [];
  if (agents && typeof agents.length === "number") {
    for (var i = 0; i < agents.length && normalized.length < 8; i += 1) {
      var agent = normalizeFrameAgent(agents[i], policy, i);
      if (agent.id) {
        normalized.push(agent);
      }
    }
  }

  if (normalized.length > 0) {
    return normalized;
  }

  var role = trimString(archetype && archetype.story && archetype.story.role) || "Wanderer body";
  return [
    normalizeFrameAgent({
      id: inhabitedByPlayer ? "agent-offline-player" : "agent-world-npc",
      mode: inhabitedByPlayer ? "offline_player_agent" : "world_npc_routine",
      priority: 1,
      routine: inhabitedByPlayer
        ? "Follow player policy, preserve BodyTime, and request only server-validated intents."
        : "Run the public NPC routine for the current Frame role: " + role + "."
    }, policy, 0)
  ];
}

function normalizeFrameAgent(agent: any, policy: any, index: number): any {
  var normalizedPolicy = normalizePolicy(policy || {});
  return {
    id: trimString(agent && agent.id) || "agent-" + (index + 1),
    mode: trimString(agent && agent.mode) || normalizedPolicy.mode,
    priority: clampNumber((agent && agent.priority) || index + 1, 1, 100),
    routine: trimString(agent && agent.routine) || "Observe, keep safe, and report changes through server-validated actions.",
    allowed_activities: normalizeStringArray(agent && agent.allowed_activities, normalizedPolicy.preferred_activities),
    forbidden_activities: normalizeStringArray(agent && agent.forbidden_activities, normalizedPolicy.forbidden_activities)
  };
}

function normalizeFrameTools(tools: any): any[] {
  var normalized: any[] = [];
  if (tools && typeof tools.length === "number") {
    for (var i = 0; i < tools.length && normalized.length < 16; i += 1) {
      var tool = normalizeFrameTool(tools[i], i);
      if (tool.name) {
        normalized.push(tool);
      }
    }
  }

  if (normalized.length > 0) {
    return normalized;
  }

  return [
    normalizeFrameTool({ name: "move", category: "intent", intent: "move", requires_validation: true }, 0),
    normalizeFrameTool({ name: "interact", category: "intent", intent: "interact", requires_validation: true }, 1),
    normalizeFrameTool({ name: "say", category: "intent", intent: "say", requires_validation: true }, 2),
    normalizeFrameTool({ name: "loot_request", category: "intent", intent: "loot", requires_validation: true }, 3)
  ];
}

function normalizeFrameTool(tool: any, index: number): any {
  var name = trimString(tool && tool.name) || "tool-" + (index + 1);
  return {
    name: name,
    category: trimString(tool && tool.category) || "intent",
    intent: trimString(tool && tool.intent) || name,
    requires_validation: tool && tool.requires_validation === false ? false : true
  };
}

function normalizeFrameHeartbeat(heartbeat: any, timestamp: string, fallbackState: string): any {
  return {
    cadence_seconds: clampNumber((heartbeat && heartbeat.cadence_seconds) || 60, 5, 3600),
    last_seen_at: trimString(heartbeat && heartbeat.last_seen_at) || timestamp,
    offline_session_state: trimString(heartbeat && heartbeat.offline_session_state) || fallbackState,
    last_action_summary: trimString(heartbeat && heartbeat.last_action_summary) || "No recent action.",
    fallback_state: trimString(heartbeat && heartbeat.fallback_state) || "none"
  };
}

function normalizeBodyAppearance(appearance: any): any {
  var parts = appearance.body_parts || {};
  return {
    body_type: trimString(appearance.body_type) || "synthetic_hunter",
    body_parts: {
      head: trimString(parts.head) || "standard-head",
      face: trimString(parts.face) || "standard-face",
      torso: trimString(parts.torso) || "standard-torso",
      arms: trimString(parts.arms) || "standard-arms",
      legs: trimString(parts.legs) || "standard-legs"
    },
    skin: trimString(appearance.skin) || "neutral",
    hair: trimString(appearance.hair) || "none",
    material: trimString(appearance.material) || "synthetic-polymer",
    marks: normalizeStringArray(appearance.marks, [])
  };
}

function normalizeBodyInhabitation(inhabitation: any, archetype: any, inhabitedByPlayer: boolean, seed: string): any {
  var sourceActorId = trimString(inhabitation.source_actor_id) ||
    sourceActorIdForArchetype(archetype || {}, seed);
  return {
    source_actor_id: sourceActorId,
    previous_role: trimString(inhabitation.previous_role) ||
      trimString(archetype && archetype.story && archetype.story.role) ||
      "World body",
    inhabited_by_player: inhabitation.inhabited_by_player === undefined
      ? inhabitedByPlayer
      : inhabitation.inhabited_by_player === true,
    assigned_at: trimString(inhabitation.assigned_at) || new Date().toISOString()
  };
}

function normalizeAnimationCapabilities(capabilities: any, defaults?: any): any {
  var fallback = defaults || {};
  return {
    supports_jump: normalizeBooleanWithDefault(capabilities.supports_jump, fallback.supports_jump === false ? false : true),
    supports_roll: normalizeBooleanWithDefault(capabilities.supports_roll, fallback.supports_roll === false ? false : true),
    supports_melee: normalizeBooleanWithDefault(capabilities.supports_melee, fallback.supports_melee === false ? false : true),
    supports_ranged: normalizeBooleanWithDefault(capabilities.supports_ranged, fallback.supports_ranged === true),
    weapon_stance: trimString(capabilities.weapon_stance) || trimString(fallback.weapon_stance) || "one_hand_melee"
  };
}

function normalizeBooleanWithDefault(value: any, fallback: boolean): boolean {
  if (value === undefined || value === null) {
    return fallback;
  }

  return value === true;
}

function equipmentOrArchetypeDefault(equipment: any, archetype: any): any {
  if (!equipment) {
    return { equipment_visual_id: archetype.equipment_visual_id };
  }

  var hasEquipmentVisualId = equipment.equipment_visual_id !== undefined &&
    equipment.equipment_visual_id !== null &&
    Number(equipment.equipment_visual_id) > 0;
  var hasPrimaryWeapon = !!trimString(equipment.primary_weapon);
  if (!hasEquipmentVisualId && !hasPrimaryWeapon) {
    return { equipment_visual_id: archetype.equipment_visual_id };
  }

  return equipment;
}

function normalizeEquipment(equipment: any): any {
  var equipmentVisualId = clampNumber(equipment.equipment_visual_id || 0, 0, 9);
  var defaults = equipmentVisualDefaults(equipmentVisualId);
  return {
    primary_weapon: trimString(equipment.primary_weapon) || primaryWeaponName(equipmentVisualId),
    equipment_visual_id: equipmentVisualId,
    weapon_visual_key: trimString(equipment.weapon_visual_key) || defaults.weapon_visual_key,
    weapon_family: trimString(equipment.weapon_family) || defaults.weapon_family,
    combat_stance: trimString(equipment.combat_stance) || defaults.combat_stance,
    socket: trimString(equipment.socket) || defaults.socket
  };
}

function equipmentVisualDefaults(equipmentVisualId: number): any {
  switch (equipmentVisualId) {
    case 1:
      return { weapon_visual_key: "unarmed", weapon_family: "unarmed", combat_stance: "unarmed", socket: "none" };
    case 2:
      return { weapon_visual_key: "sword", weapon_family: "melee", combat_stance: "one_hand_melee", socket: "right_hand" };
    case 3:
      return { weapon_visual_key: "two_hand_sword", weapon_family: "melee", combat_stance: "two_hand_melee", socket: "hands" };
    case 4:
      return { weapon_visual_key: "spear", weapon_family: "melee", combat_stance: "polearm", socket: "hands" };
    case 5:
      return { weapon_visual_key: "axe", weapon_family: "melee", combat_stance: "heavy_melee", socket: "hands" };
    case 6:
      return { weapon_visual_key: "bow", weapon_family: "ranged", combat_stance: "ranged_bow", socket: "hands" };
    case 7:
      return { weapon_visual_key: "crossbow", weapon_family: "ranged", combat_stance: "ranged_crossbow", socket: "hands" };
    case 8:
      return { weapon_visual_key: "staff", weapon_family: "caster", combat_stance: "staff_caster", socket: "hands" };
    case 9:
      return { weapon_visual_key: "hammer", weapon_family: "melee", combat_stance: "heavy_melee", socket: "hands" };
    default:
      return { weapon_visual_key: "none", weapon_family: "none", combat_stance: "relaxed", socket: "none" };
  }
}

function primaryWeaponName(equipmentVisualId: number): string {
  switch (equipmentVisualId) {
    case 1:
      return "unarmed";
    case 2:
      return "one_hand_sword";
    case 3:
      return "two_hand_sword";
    case 4:
      return "two_hand_spear";
    case 5:
      return "two_hand_axe";
    case 6:
      return "two_hand_bow";
    case 7:
      return "two_hand_crossbow";
    case 8:
      return "staff";
    case 9:
      return "hammer";
    default:
      return "none";
  }
}

function normalizeMemoryKind(kind: any): string {
  var value = trimString(kind);
  if (value === "preference" || value === "quest" || value === "relationship" || value === "combat" || value === "system") {
    return value;
  }
  return "system";
}

function normalizeMemoryPayload(payload: any): any {
  var memory = payload.memory || payload;
  var summary = trimString(memory.summary);
  if (!summary) {
    throw new Error("memory summary is required");
  }
  return {
    id: trimString(memory.id),
    kind: normalizeMemoryKind(memory.kind),
    summary: summary,
    importance: clampNumber(memory.importance || 5, 1, 10)
  };
}

function normalizeMemoryRecords(records: any[]): any[] {
  var normalized: any[] = [];
  if (!records || typeof records.length !== "number") {
    return normalized;
  }

  for (var index = 0; index < records.length; index += 1) {
    try {
      normalized.push(normalizeMemoryPayload(records[index] || {}));
    } catch (err) {
      // Ignore malformed seed memories. Runtime RPC memory writes still fail loudly.
    }
  }

  return sortAndBoundMemories(normalized);
}

function normalizeActorType(actorType: any): string {
  var value = trimString(actorType);
  if (value === "player_body" || value === "npc" || value === "offline_agent" || value === "openclaw_agent") {
    return value;
  }
  return "npc";
}

function normalizeOpenClawAgentId(agentId: any): string {
  var normalized = sanitizeNakamaIdentifier(trimString(agentId), "");
  if (!normalized) {
    throw new Error("connected_agent_id is required");
  }
  if (normalized.length > 96) {
    throw new Error("connected_agent_id is too long");
  }
  return normalized;
}

function normalizeOpenClawConnectionStatus(status: any): string {
  var value = trimString(status);
  if (
    value === "connected" ||
    value === "disconnected" ||
    value === "degraded" ||
    value === "suspended" ||
    value === "blocked"
  ) {
    return value;
  }
  return "connected";
}

function normalizeOpenClawModerationState(state: any): string {
  var value = trimString(state);
  if (value === "active" || value === "limited" || value === "suspended" || value === "blocked") {
    return value;
  }
  return "active";
}

function normalizeOpenClawAgentKind(kind: any): string {
  var value = trimString(kind);
  if (
    value === "companion" ||
    value === "hub_npc" ||
    value === "merchant_persona" ||
    value === "quest_actor" ||
    value === "social_actor"
  ) {
    return value;
  }
  return "companion";
}

function normalizeOpenClawRateLimit(profile: any): any {
  return {
    requests_per_minute: clampNumber(profile.requests_per_minute || 30, 1, 600),
    intents_per_minute: clampNumber(profile.intents_per_minute || 20, 1, 300),
    tokens_per_day: clampNumber(profile.tokens_per_day || 50000, 1000, 10000000)
  };
}

function normalizeActorId(actorId: any): string {
  var normalized = sanitizeNakamaIdentifier(trimString(actorId), "");
  if (!normalized) {
    throw new Error("actor_id is required");
  }
  if (normalized.length > actorIdMaxLength) {
    throw new Error("actor_id is too long");
  }
  return normalized;
}

function actorStorageKey(actorId: string): string {
  return "profile:" + normalizeActorId(actorId);
}

function worldActorStorageKey(actorId: string): string {
  return "world_profile:" + normalizeActorId(actorId);
}

function openClawBindingStorageKey(connectedAgentId: string): string {
  return "binding:" + normalizeOpenClawAgentId(connectedAgentId);
}

function chatChannelStorageKey(channelId: string): string {
  return "channel:" + normalizeChatChannelId(channelId);
}

function actorDisplayName(actorId: string): string {
  var normalized = normalizeActorId(actorId).replace(/-/g, " ");
  return normalized || "Unnamed Actor";
}

function cloneJson(value: any): any {
  return JSON.parse(JSON.stringify(value || {}));
}

function mergeObjects(base: any, override: any): any {
  var merged = cloneJson(base || {});
  var source = override || {};
  for (var key in source) {
    if (Object.prototype.hasOwnProperty.call(source, key)) {
      merged[key] = cloneJson(source[key]);
    }
  }
  return merged;
}

function setIfChanged(target: any, key: string, value: any): boolean {
  if (JSON.stringify(target[key] || null) === JSON.stringify(value || null)) {
    return false;
  }

  target[key] = value;
  return true;
}

function parseJson(payload: string, label: string): any {
  try {
    return JSON.parse(payload);
  } catch (err) {
    throw new Error("invalid " + label);
  }
}

function parseJsonOrNull(payload: string): any {
  try {
    return JSON.parse(payload || "{}");
  } catch (err) {
    return null;
  }
}

function newMemoryId(context: any, nk: nkruntime.Nakama): string {
  var playerId = sanitizeNakamaIdentifier(context.player.player_id || "player", "player");
  var sequence = String((context.body.memory || []).length + 1);
  return "mem-" + playerId + "-" + nk.uuidv4() + "-" + sequence;
}

function newActivityId(context: any, nk: nkruntime.Nakama): string {
  var playerId = sanitizeNakamaIdentifier(context.player.player_id || "player", "player");
  var sequence = String((context.body.agent_activity || []).length + 1);
  return "act-" + playerId + "-" + nk.uuidv4() + "-" + sequence;
}

function newActorMemoryId(profile: any, nk: nkruntime.Nakama): string {
  var actorId = sanitizeNakamaIdentifier(profile.actor_id || "actor", "actor");
  var sequence = String((profile.memory || []).length + 1);
  return "mem-" + actorId + "-" + nk.uuidv4() + "-" + sequence;
}

function requireUserId(ctx: nkruntime.Context): string {
  var userId = trimString(ctx.userId);
  if (!userId) {
    throw new Error("authenticated Nakama user is required");
  }
  return userId;
}

function stableNakamaCustomId(supabaseUserId: string): string {
  return "supabase-" + sanitizeNakamaIdentifier(supabaseUserId, "user");
}

function stableUsername(user: any): string {
  var email = typeof user.email === "string" ? user.email : "";
  var emailName = email.split("@")[0] || "";
  var fromEmail = sanitizeNakamaIdentifier(emailName, "");
  if (fromEmail.length >= 6) {
    return fromEmail;
  }

  var id = sanitizeNakamaIdentifier(user.id || "", "player");
  return "player-" + id.substring(0, 12);
}

function sanitizeNakamaIdentifier(value: string, fallback: string): string {
  var cleaned = lowercase(value)
    .replace(/[^a-z0-9-]/g, "")
    .replace(/^-+|-+$/g, "");
  return cleaned || fallback;
}

function normalizeStringArray(values: any, fallback: string[]): string[] {
  if (!values || typeof values.length !== "number") {
    return fallback;
  }

  var result: string[] = [];
  for (var i = 0; i < values.length; i++) {
    var value = trimString(values[i]);
    if (value) {
      result.push(value);
    }
  }

  return result.length > 0 ? result : fallback;
}

function arrayContains(values: any[], target: string): boolean {
  if (!values) {
    return false;
  }

  for (var i = 0; i < values.length; i++) {
    if (values[i] === target) {
      return true;
    }
  }
  return false;
}

function clampNumber(value: any, min: number, max: number): number {
  var numberValue = Number(value);
  if (isNaN(numberValue)) {
    numberValue = min;
  }
  if (numberValue < min) {
    return min;
  }
  if (numberValue > max) {
    return max;
  }
  return numberValue;
}

function numberOrDefault(value: any, fallback: number): any {
  if (value === null || value === undefined || value === "") {
    return fallback;
  }
  return value;
}

function finiteNumberOrDefault(value: any, fallback: number): number {
  var numberValue = Number(value);
  if (isNaN(numberValue) || !isFinite(numberValue)) {
    return fallback;
  }
  return numberValue;
}

function firstDefined(primary: any, fallback: any): any {
  return primary === undefined || primary === null ? fallback : primary;
}

function trimString(value: any): string {
  if (value === null || value === undefined) {
    return "";
  }
  return String(value).replace(/^\s+|\s+$/g, "");
}

function lowercase(value: any): string {
  return trimString(value).toLowerCase();
}

function trimTrailingSlash(value: string): string {
  return value.replace(/\/+$/g, "");
}
