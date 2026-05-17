import assert from "node:assert/strict";
import fs from "node:fs";
import vm from "node:vm";

const runtime = fs.readFileSync(new URL("../build/index.js", import.meta.url), "utf8");
function loadRuntime() {
  const context = {};
  vm.createContext(context);
  return vm.runInContext(
    `${runtime}
({
  InitModule,
  beforeAuthenticateCustom,
  stableNakamaCustomId,
  stableUsername,
  sanitizeNakamaIdentifier,
  trimTrailingSlash
});`,
    context
  );
}

function createRuntimeHarness(module) {
  const registeredHooks = [];
  const registeredRpcs = new Map();
  const storage = new Map();
  let storageVersion = 0;
  let uuidCounter = 0;
  let conflictOnNextVersionedWrite = false;
  let conflictOnNextCreateOnlyWrite = false;
  const logger = {
    debug: () => {},
    error: (message) => {
      throw new Error(message);
    },
    info: () => {},
  };
  const nk = {
    storageRead: (requests) => requests
      .map((request) => storage.get(storageKey(request.userId, request.collection, request.key)))
      .filter(Boolean),
    storageWrite: (requests) => {
      for (const request of requests) {
        const key = storageKey(request.userId, request.collection, request.key);
        const existing = storage.get(key);
        if (conflictOnNextVersionedWrite && request.version && existing) {
          storageVersion += 1;
          existing.version = `external-version-${storageVersion}`;
          conflictOnNextVersionedWrite = false;
        }
        if (Object.prototype.hasOwnProperty.call(request, "version")) {
          if (request.version === "*") {
            if (conflictOnNextCreateOnlyWrite) {
              storageVersion += 1;
              storage.set(key, {
                ...request,
                value: { external: true },
                version: `external-version-${storageVersion}`,
              });
              conflictOnNextCreateOnlyWrite = false;
              throw new Error("storage create conflict");
            }
            if (existing) {
              throw new Error("storage create conflict");
            }
          } else if (!existing || existing.version !== request.version) {
            throw new Error("storage version conflict");
          }
        }
        storageVersion += 1;
        storage.set(key, {
          ...request,
          version: `test-version-${storageVersion}`,
        });
      }
    },
    uuidv4: () => {
      uuidCounter += 1;
      return `00000000-0000-4000-8000-${String(uuidCounter).padStart(12, "0")}`;
    },
  };

  module.InitModule(
    { env: {} },
    logger,
    nk,
    {
      registerRpc: (name, rpc) => registeredRpcs.set(name, rpc),
      registerBeforeAuthenticateCustom: (hook) => registeredHooks.push(hook),
    }
  );

  return {
    registeredHooks,
    registeredRpcs,
    storage,
    logger,
    nk,
    conflictNextWrite: () => {
      conflictOnNextVersionedWrite = true;
    },
    conflictNextCreateOnlyWrite: () => {
      conflictOnNextCreateOnlyWrite = true;
    },
  };
}

function storageKey(userId, collection, key) {
  return `${userId}:${collection}:${key}`;
}

const module = loadRuntime();

assert.equal(
  module.stableNakamaCustomId("308ebb59-47b7-46fe-835c-5375cd41037d"),
  "supabase-308ebb59-47b7-46fe-835c-5375cd41037d"
);

assert.equal(
  module.stableUsername({ id: "308ebb59-47b7-46fe-835c-5375cd41037d", email: "Founder+Test@example.com" }),
  "foundertest"
);

assert.equal(
  module.stableUsername({ id: "308ebb59-47b7-46fe-835c-5375cd41037d" }),
  "player-308ebb59-47b"
);

const harness = createRuntimeHarness(module);
assert.equal(harness.registeredHooks.length, 1);
assert.equal(harness.registeredRpcs.size, 22);
assert.ok(harness.registeredRpcs.has("secondspawn_health"));
assert.ok(harness.registeredRpcs.has("secondspawn_profile_get"));
assert.ok(harness.registeredRpcs.has("secondspawn_memory_add"));
assert.ok(harness.registeredRpcs.has("secondspawn_soul_update"));
assert.ok(harness.registeredRpcs.has("secondspawn_agent_decide"));
assert.ok(harness.registeredRpcs.has("secondspawn_agent_activity_add"));
assert.ok(harness.registeredRpcs.has("secondspawn_actor_profile_get"));
assert.ok(harness.registeredRpcs.has("secondspawn_actor_memory_add"));
assert.ok(harness.registeredRpcs.has("secondspawn_bodytime_event"));
assert.ok(harness.registeredRpcs.has("secondspawn_reincarnate"));
assert.ok(harness.registeredRpcs.has("secondspawn_openclaw_bind"));
assert.ok(harness.registeredRpcs.has("secondspawn_openclaw_context_get"));
assert.ok(harness.registeredRpcs.has("secondspawn_openclaw_intent_submit"));
assert.ok(harness.registeredRpcs.has("secondspawn_openclaw_heartbeat"));
assert.ok(harness.registeredRpcs.has("secondspawn_chat_send"));
assert.ok(harness.registeredRpcs.has("secondspawn_chat_list"));
assert.ok(harness.registeredRpcs.has("secondspawn_reward_claim"));
assert.ok(harness.registeredRpcs.has("secondspawn_npc_seed"));
assert.ok(harness.registeredRpcs.has("secondspawn_npc_list"));
assert.ok(harness.registeredRpcs.has("secondspawn_npc_interact"));
assert.ok(harness.registeredRpcs.has("secondspawn_npc_context_get"));
assert.ok(harness.registeredRpcs.has("secondspawn_npc_intent_submit"));

const createConflictHarness = createRuntimeHarness(module);
createConflictHarness.conflictNextCreateOnlyWrite();
assert.throws(
  () => createConflictHarness.registeredRpcs.get("secondspawn_profile_get")(
    { userId: "create-race-user", env: {} },
    createConflictHarness.logger,
    createConflictHarness.nk,
    ""
  ),
  /storage create conflict/
);

const actorCreateConflictHarness = createRuntimeHarness(module);
actorCreateConflictHarness.conflictNextCreateOnlyWrite();
const actorCreateRaceProfile = JSON.parse(actorCreateConflictHarness.registeredRpcs.get("secondspawn_actor_profile_get")(
  { userId: "actor-create-race-user", env: {} },
  actorCreateConflictHarness.logger,
  actorCreateConflictHarness.nk,
  JSON.stringify({ actor_id: "npc-race" })
));
assert.equal(actorCreateRaceProfile.actor_id, "npc-race");
assert.equal(actorCreateRaceProfile.body.body_id, "body-npc-race");

const healthPayload = harness.registeredRpcs.get("secondspawn_health")({ userId: "user-1", env: {} }, harness.logger, harness.nk, "");
assert.equal(JSON.parse(healthPayload).service, "second-spawn-nakama");

const sentChat = JSON.parse(harness.registeredRpcs.get("secondspawn_chat_send")(
  { userId: "user-1", env: {} },
  harness.logger,
  harness.nk,
  JSON.stringify({
    channel_id: "prototype-hub",
    sender_display_name: "JOY",
    message: "Hub chat is online.",
  })
));
assert.equal(sentChat.channel_id, "prototype-hub");
assert.equal(sentChat.message.sender_player_id, "user-1");
assert.equal(sentChat.message.sender_display_name, "JOY");
assert.equal(sentChat.message.text, "Hub chat is online.");
assert.equal(sentChat.message.source, "player");

const listedChat = JSON.parse(harness.registeredRpcs.get("secondspawn_chat_list")(
  { userId: "user-1", env: {} },
  harness.logger,
  harness.nk,
  JSON.stringify({ channel_id: "prototype-hub", limit: 8 })
));
assert.equal(listedChat.messages.length, 1);
assert.equal(listedChat.messages[0].id, sentChat.message.id);
assert.equal(listedChat.messages[0].text, "Hub chat is online.");

const chatCreateConflictHarness = createRuntimeHarness(module);
chatCreateConflictHarness.conflictNextCreateOnlyWrite();
const chatAfterCreateRace = JSON.parse(chatCreateConflictHarness.registeredRpcs.get("secondspawn_chat_send")(
  { userId: "chat-race-user", env: {} },
  chatCreateConflictHarness.logger,
  chatCreateConflictHarness.nk,
  JSON.stringify({ channel_id: "prototype-hub", message: "Recovered after create race." })
));
assert.equal(chatAfterCreateRace.messages.length, 1);
assert.equal(chatAfterCreateRace.messages[0].text, "Recovered after create race.");

const seededNpcs = JSON.parse(harness.registeredRpcs.get("secondspawn_npc_seed")(
  { userId: "user-1", env: {} },
  harness.logger,
  harness.nk,
  ""
));
assert.equal(seededNpcs.count, 10);
assert.equal(seededNpcs.npcs[0].actor_id, "npc-synthetic-sentinel-0101");
assert.equal(seededNpcs.npcs[0].actor_type, "npc");
assert.equal(seededNpcs.npcs[0].owner_player_id, "user-1");
assert.equal(seededNpcs.npcs[0].body.identity.public_name, "Gate Sentinel 0101");
assert.equal(seededNpcs.npcs[0].body.visual_variant, 7);
assert.equal(seededNpcs.npcs[5].body.visual_variant, 16);
assert.equal(seededNpcs.npcs[6].body.visual_variant, 14);
assert.equal(seededNpcs.npcs[8].body.visual_variant, 15);
assert.ok(harness.storage.get(storageKey("user-1", "secondspawn_actor", "world_profile:npc-synthetic-sentinel-0101")));

const listedNpcs = JSON.parse(harness.registeredRpcs.get("secondspawn_npc_list")(
  { userId: "user-1", env: {} },
  harness.logger,
  harness.nk,
  ""
));
assert.equal(listedNpcs.count, 10);
assert.equal(listedNpcs.npcs[3].actor_id, "npc-scrap-warden-0441");

const npcContext = JSON.parse(harness.registeredRpcs.get("secondspawn_npc_context_get")(
  { userId: "user-1", env: {} },
  harness.logger,
  harness.nk,
  JSON.stringify({
    actor_id: "npc-synthetic-sentinel-0101",
    nearby_actor_ids: ["npc-wasteland-courier-0244"]
  })
));
assert.equal(npcContext.actor.actor_id, "npc-synthetic-sentinel-0101");
assert.equal(npcContext.nearby_actors.length, 1);
assert.equal(npcContext.nearby_actors[0].actor_id, "npc-wasteland-courier-0244");
assert.ok(npcContext.allowed_intents.includes("say"));
assert.equal(npcContext.interaction_rules.max_distance_meters, 12);
assert.ok(npcContext.interaction_rules.soft_prompt_guidance.some((rule) => /affinity/.test(rule)));

const npcIntent = JSON.parse(harness.registeredRpcs.get("secondspawn_npc_intent_submit")(
  { userId: "user-1", env: {} },
  harness.logger,
  harness.nk,
  JSON.stringify({
    id: "npc-intent-1",
    actor_id: "npc-synthetic-sentinel-0101",
    target_actor_id: "npc-wasteland-courier-0244",
    intent: "say",
    source: "llm",
    text: "Route check complete. Keep the eastern gate quiet."
  })
));
assert.equal(npcIntent.accepted, true);
assert.equal(npcIntent.intent.intent, "say");
assert.equal(npcIntent.actor.agent_activity[0].id, "npc-intent-1-actor");
assert.equal(npcIntent.target_actor.agent_activity[0].id, "npc-intent-1-target");
assert.equal(npcIntent.actor.relationships[0].actor_id, "npc-wasteland-courier-0244");
assert.equal(npcIntent.actor.relationships[0].affinity, 4);
assert.equal(npcIntent.actor.relationships[0].familiarity_count, 1);
assert.ok(npcIntent.actor.memory.some((memory) => /Route check complete/.test(memory.summary)));
assert.ok(npcIntent.target_actor.memory.some((memory) => /Route check complete/.test(memory.summary)));
assert.throws(
  () => harness.registeredRpcs.get("secondspawn_npc_intent_submit")(
    { userId: "user-1", env: {} },
    harness.logger,
    harness.nk,
    JSON.stringify({
      actor_id: "npc-synthetic-sentinel-0101",
      target_actor_id: "npc-wasteland-courier-0244",
      intent: "say",
      text: "Too far.",
      distance_meters: 13
    })
  ),
  /too far away/
);
assert.throws(
  () => harness.registeredRpcs.get("secondspawn_npc_intent_submit")(
    { userId: "user-1", env: {} },
    harness.logger,
    harness.nk,
    JSON.stringify({
      actor_id: "npc-synthetic-sentinel-0101",
      intent: "grant_item",
      text: "Give me loot."
    })
  ),
  /NPC intent is not allowed/
);

const npcInteraction = JSON.parse(harness.registeredRpcs.get("secondspawn_npc_interact")(
  { userId: "user-1", env: {} },
  harness.logger,
  harness.nk,
  JSON.stringify({
    id: "npc-talk-1",
    actor_a_id: "npc-synthetic-sentinel-0101",
    actor_b_id: "npc-wasteland-courier-0244",
    topic: "patrol"
  })
));
assert.equal(npcInteraction.interaction.id, "npc-talk-1");
assert.equal(npcInteraction.interaction.actor_a_id, "npc-synthetic-sentinel-0101");
assert.equal(npcInteraction.interaction.actor_b_id, "npc-wasteland-courier-0244");
assert.match(npcInteraction.interaction.actor_a_line, /Route Courier 0244/);
assert.equal(npcInteraction.actor_a.agent_activity[0].id, "npc-talk-1-a");
assert.equal(npcInteraction.actor_b.agent_activity[0].id, "npc-talk-1-b");
assert.ok(npcInteraction.actor_a.memory.some((memory) => memory.kind === "relationship" && /Route Courier 0244/.test(memory.summary)));
assert.ok(npcInteraction.actor_b.memory.some((memory) => memory.kind === "relationship" && /Gate Sentinel 0101/.test(memory.summary)));
assert.throws(
  () => harness.registeredRpcs.get("secondspawn_npc_interact")(
    { userId: "user-1", env: {} },
    harness.logger,
    harness.nk,
    JSON.stringify({
      actor_a_id: "npc-synthetic-sentinel-0101",
      actor_b_id: "npc-synthetic-sentinel-0101"
    })
  ),
  /two different actors/
);

const profile = JSON.parse(harness.registeredRpcs.get("secondspawn_profile_get")(
  { userId: "user-1", env: {} },
  harness.logger,
  harness.nk,
  ""
));
assert.equal(profile.player.player_id, "user-1");
assert.equal(profile.body.soul.name, "user-1");
assert.equal(profile.player.second_balance_seconds, 604800);
assert.equal(profile.player.reincarnation_count, 0);
assert.equal(profile.body.memory.length, 1);
assert.equal(profile.body.archetype_id, "crossline-hunter");
assert.equal(profile.body.visual_prefab_key, "generated_visual_09_crossbow");
assert.equal(profile.body.visual_variant, 9);
assert.equal(profile.body.equipment.primary_weapon, "two_hand_crossbow");
assert.equal(profile.body.equipment.equipment_visual_id, 7);
assert.equal(profile.body.equipment.weapon_visual_key, "crossbow");
assert.equal(profile.body.equipment.weapon_family, "ranged");
assert.equal(profile.body.equipment.combat_stance, "ranged_crossbow");
assert.equal(profile.body.equipment.socket, "hands");
assert.equal(profile.body.appearance.body_type, "synthetic_hunter");
assert.equal(profile.body.appearance.body_parts.head, "crossline-optic-head");
assert.equal(profile.body.appearance.body_parts.arms, "steady-ranged-arms");
assert.equal(profile.body.appearance.body_parts.legs, "survey-runner-legs");
assert.equal(profile.body.appearance.material, "matte-carbon");
assert.ok(profile.body.appearance.marks.includes("signal-burn"));
assert.equal(profile.body.inhabitation.source_actor_id, "npc-crossline-hunter-5104");
assert.equal(profile.body.inhabitation.previous_role, "Ranged survey body");
assert.equal(profile.body.inhabitation.inhabited_by_player, true);
assert.equal(profile.body.animation_capabilities.supports_jump, false);
assert.equal(profile.body.animation_capabilities.supports_roll, true);
assert.equal(profile.body.animation_capabilities.supports_ranged, true);
assert.equal(profile.body.animation_capabilities.weapon_stance, "ranged_crossbow");
assert.equal(profile.body.story.role, "Ranged survey body");
assert.equal(profile.body.identity.public_name, "user-1");
assert.equal(profile.body.identity.callsign, "npc-crossline-hunter-5104");
assert.equal(profile.body.identity.public_role, "Ranged survey body");
assert.equal(profile.body.identity.profession, "Ranged survey body");
assert.match(profile.body.identity.reputation_summary, /Newly inhabited body/);
assert.equal(profile.body.skills.length, 2);
assert.equal(profile.body.skills[0].id, "skill-body-role");
assert.equal(profile.body.skills[0].category, "profession");
assert.equal(profile.body.skills[1].category, "combat");
assert.equal(profile.body.skills[1].name, "crossbow");
assert.equal(profile.body.agents.length, 1);
assert.equal(profile.body.agents[0].mode, "offline_player_agent");
assert.ok(profile.body.agents[0].allowed_activities.includes("safe_farming"));
assert.equal(profile.body.tools.length, 4);
assert.equal(profile.body.tools[0].requires_validation, true);
assert.equal(profile.body.heartbeat.cadence_seconds, 60);
assert.equal(profile.body.heartbeat.offline_session_state, "online");
assert.equal(profile.body.stats.level, 1);
assert.equal(profile.body.stats.vitality, 9);
assert.equal(profile.body.stats.agility, 9);
assert.equal(profile.body.stats.max_health, 100);
assert.equal(profile.body.stats.attack_power, 12);
assert.equal(profile.body.time.remaining_seconds, 86400);
assert.equal(profile.body.lifecycle, "alive");
assert.equal(profile.body.agent_runtime.decision_count, 0);
assert.equal(profile.body.agent_runtime.fallback_decision_count, 0);
assert.equal(profile.body.agent_activity.length, 1);
assert.equal(profile.body.agent_activity[0].kind, "profile_bootstrap");
const assignedSourceActor = harness.storage.get(storageKey("user-1", "secondspawn_actor", "profile:npc-crossline-hunter-5104"));
assert.ok(assignedSourceActor);
assert.equal(assignedSourceActor.value.actor_id, "npc-crossline-hunter-5104");
assert.equal(assignedSourceActor.value.actor_type, "player_body");
assert.equal(assignedSourceActor.value.display_name, "Crossline Surveyor 5104");
assert.equal(assignedSourceActor.value.body.body_id, profile.body.body_id);
assert.equal(assignedSourceActor.value.body.equipment.weapon_visual_key, "crossbow");
assert.equal(assignedSourceActor.value.body.inhabitation.inhabited_by_player, true);
assert.equal(assignedSourceActor.value.body.inhabitation.source_actor_id, "npc-crossline-hunter-5104");
assert.equal(assignedSourceActor.value.body.identity.callsign, "npc-crossline-hunter-5104");
assert.equal(assignedSourceActor.value.body.skills[1].name, "crossbow");
assert.equal(assignedSourceActor.value.body.stats.max_health, profile.body.stats.max_health);
assert.equal(assignedSourceActor.value.body.soul.name, profile.body.soul.name);
assert.ok(assignedSourceActor.value.memory.length > 0);
assert.equal(assignedSourceActor.value.agent_activity[0].kind, "profile_bootstrap");

const spentBodyTime = JSON.parse(harness.registeredRpcs.get("secondspawn_bodytime_event")(
  { userId: "user-1", env: {} },
  harness.logger,
  harness.nk,
  JSON.stringify({
    id: "bodytime-spend-1",
    kind: "spend",
    source: "prototype_service",
    amount_seconds: 600,
    note: "Prototype recovery service."
  })
));
assert.equal(spentBodyTime.body.time.remaining_seconds, 85800);
assert.equal(spentBodyTime.body.lifecycle, "alive");
assert.equal(spentBodyTime.body.agent_activity[0].id, "bodytime-spend-1");
assert.equal(spentBodyTime.body.agent_activity[0].kind, "body_time");
assert.match(spentBodyTime.body.agent_activity[0].summary, /spent 600s/);
assert.match(spentBodyTime.body.heartbeat.last_action_summary, /spent 600s/);
assert.equal(spentBodyTime.body.heartbeat.offline_session_state, "online");

const earnedBodyTime = JSON.parse(harness.registeredRpcs.get("secondspawn_bodytime_event")(
  { userId: "user-1", env: {} },
  harness.logger,
  harness.nk,
  JSON.stringify({
    id: "bodytime-earn-1",
    kind: "earn",
    source: "prototype_safe_farming",
    amount_seconds: 300
  })
));
assert.equal(earnedBodyTime.body.time.remaining_seconds, 86100);
assert.equal(earnedBodyTime.body.agent_activity[0].id, "bodytime-earn-1");

const retriedBodyTimeEarn = JSON.parse(harness.registeredRpcs.get("secondspawn_bodytime_event")(
  { userId: "user-1", env: {} },
  harness.logger,
  harness.nk,
  JSON.stringify({
    id: "bodytime-earn-1",
    kind: "earn",
    source: "prototype_safe_farming",
    amount_seconds: 300
  })
));
assert.equal(retriedBodyTimeEarn.body.time.remaining_seconds, 86100);
assert.equal(retriedBodyTimeEarn.body.agent_activity.filter((activity) => activity.id === "bodytime-earn-1").length, 1);
assert.throws(
  () => harness.registeredRpcs.get("secondspawn_bodytime_event")(
    { userId: "user-1", env: {} },
    harness.logger,
    harness.nk,
    JSON.stringify({
      id: "bodytime-earn-2",
      kind: "earn",
      source: "prototype_safe_farming",
      amount_seconds: 300
    })
  ),
  /earn source is on cooldown/
);

const claimedReward = JSON.parse(harness.registeredRpcs.get("secondspawn_reward_claim")(
  { userId: "user-1", env: {} },
  harness.logger,
  harness.nk,
  JSON.stringify({
    id: "reward-training-1",
    objective_id: "prototype-training-drone"
  })
));
assert.equal(claimedReward.body.time.remaining_seconds, 86220);
assert.equal(claimedReward.body.agent_activity[0].id, "reward-training-1");
assert.equal(claimedReward.body.agent_activity[0].body_time_source, "prototype_reward_prototype-training-drone");
assert.match(claimedReward.body.agent_activity[0].summary, /training drone/);

const retriedReward = JSON.parse(harness.registeredRpcs.get("secondspawn_reward_claim")(
  { userId: "user-1", env: {} },
  harness.logger,
  harness.nk,
  JSON.stringify({
    id: "reward-training-1",
    objective_id: "prototype-training-drone"
  })
));
assert.equal(retriedReward.body.time.remaining_seconds, 86220);
assert.equal(retriedReward.body.agent_activity.filter((activity) => activity.id === "reward-training-1").length, 1);
assert.throws(
  () => harness.registeredRpcs.get("secondspawn_reward_claim")(
    { userId: "user-1", env: {} },
    harness.logger,
    harness.nk,
    JSON.stringify({ objective_id: "client-invented-reward" })
  ),
  /unknown prototype reward objective/
);

const storedProfile = harness.storage.get(storageKey("user-1", "secondspawn_agent", "context"));
delete storedProfile.value.body.time;
delete storedProfile.value.body.agent_policy;
delete storedProfile.value.body.agent_runtime;
delete storedProfile.value.body.agent_activity;
delete storedProfile.value.body.appearance;
delete storedProfile.value.body.inhabitation;
delete storedProfile.value.body.equipment.weapon_visual_key;
delete storedProfile.value.body.animation_capabilities.supports_ranged;
delete storedProfile.value.body.animation_capabilities.weapon_stance;
delete storedProfile.value.body.animation_capabilities.supports_roll;
const migratedProfile = JSON.parse(harness.registeredRpcs.get("secondspawn_profile_get")(
  { userId: "user-1", env: {} },
  harness.logger,
  harness.nk,
  ""
));
assert.equal(migratedProfile.body.time.remaining_seconds, 86400);
assert.equal(migratedProfile.body.agent_policy.mode, "observe_and_keep_safe");
assert.equal(migratedProfile.body.agent_runtime.activity_count, 1);
assert.equal(migratedProfile.body.agent_activity.length, 1);
assert.equal(migratedProfile.body.agent_activity[0].kind, "profile_bootstrap");
assert.equal(migratedProfile.body.appearance.body_parts.head, "crossline-optic-head");
assert.equal(migratedProfile.body.inhabitation.source_actor_id, "npc-crossline-hunter-5104");
assert.equal(migratedProfile.body.equipment.weapon_visual_key, "crossbow");
assert.equal(migratedProfile.body.animation_capabilities.supports_roll, true);
assert.equal(migratedProfile.body.animation_capabilities.supports_ranged, true);
assert.equal(migratedProfile.body.animation_capabilities.weapon_stance, "ranged_crossbow");
const normalizedStoredProfile = harness.storage.get(storageKey("user-1", "secondspawn_agent", "context"));
assert.equal(normalizedStoredProfile.value.body.time.remaining_seconds, 86400);
assert.equal(normalizedStoredProfile.value.body.agent_policy.mode, "observe_and_keep_safe");

const npcProfile = JSON.parse(harness.registeredRpcs.get("secondspawn_actor_profile_get")(
  { userId: "user-1", env: {} },
  harness.logger,
  harness.nk,
  JSON.stringify({
    actor_id: "npc-guide",
    actor_type: "npc",
    display_name: "Mira Guide",
    stats: { level: 0, max_health: 0, max_energy: 0, attack_power: 0 },
    characteristics: { curiosity: 8, sociability: 9 },
    time: { remaining_seconds: 0, max_seconds: 0, danger_drain_rate: 0 },
    soul: { core_drive: "help new bodies survive the hub" }
  })
));
assert.equal(npcProfile.actor_id, "npc-guide");
assert.equal(npcProfile.actor_type, "npc");
assert.equal(npcProfile.owner_player_id, "user-1");
assert.equal(npcProfile.display_name, "Mira Guide");
assert.equal(npcProfile.body.soul.name, "Mira Guide");
assert.equal(npcProfile.body.soul.core_drive, "help new bodies survive the hub");
assert.equal(npcProfile.body.stats.level, 1);
assert.equal(npcProfile.body.stats.max_health, 1);
assert.equal(npcProfile.body.stats.max_energy, 0);
assert.equal(npcProfile.body.stats.attack_power, 0);
assert.equal(npcProfile.body.characteristics.sociability, 9);
assert.equal(npcProfile.body.time.remaining_seconds, 0);
assert.equal(npcProfile.body.time.max_seconds, 1);
assert.equal(npcProfile.body.time.danger_drain_rate, 0);
assert.ok(npcProfile.body.story.origin.length > 0);
assert.equal(typeof npcProfile.body.animation_capabilities.supports_jump, "boolean");
assert.equal(npcProfile.body.inhabitation.inhabited_by_player, false);
assert.ok(npcProfile.body.appearance.body_parts.torso.length > 0);
assert.ok(npcProfile.body.equipment.weapon_visual_key.length > 0);
assert.equal(npcProfile.memory.length, 1);

const permanentNpcProfile = JSON.parse(harness.registeredRpcs.get("secondspawn_actor_profile_get")(
  { userId: "user-1", env: {} },
  harness.logger,
  harness.nk,
  JSON.stringify({ actor_id: "npc-clinic-operator-0819" })
));
assert.equal(permanentNpcProfile.actor_id, "npc-clinic-operator-0819");
assert.equal(permanentNpcProfile.actor_type, "npc");
assert.equal(permanentNpcProfile.display_name, "Clinic Operator 0819");
assert.equal(permanentNpcProfile.body.archetype_id, "clinic-operator");
assert.equal(permanentNpcProfile.body.inhabitation.previous_role, "Support and researcher body");
assert.equal(permanentNpcProfile.body.inhabitation.inhabited_by_player, false);

assert.throws(
  () => harness.registeredRpcs.get("secondspawn_actor_profile_get")(
    { userId: "user-1", env: {} },
    harness.logger,
    harness.nk,
    JSON.stringify({ actor_id: "npc-" + "x".repeat(80) })
  ),
  /actor_id is too long/
);

const npcMemory = JSON.parse(harness.registeredRpcs.get("secondspawn_actor_memory_add")(
  { userId: "user-1", env: {} },
  harness.logger,
  harness.nk,
  JSON.stringify({
    actor_id: "npc-guide",
    kind: "relationship",
    summary: "Mira remembers that JOY prefers direct prototype progress.",
    importance: 8
  })
));
assert.equal(npcMemory.memory[0].summary, "Mira remembers that JOY prefers direct prototype progress.");
assert.match(npcMemory.memory[0].id, /^mem-npc-guide-00000000-0000-4000-8000-[0-9]{12}-2$/);

const secondNpcProfile = JSON.parse(harness.registeredRpcs.get("secondspawn_actor_profile_get")(
  { userId: "user-1", env: {} },
  harness.logger,
  harness.nk,
  JSON.stringify({ actor_id: "npc-blacksmith", display_name: "Forge Keeper" })
));
assert.equal(secondNpcProfile.actor_id, "npc-blacksmith");
assert.equal(secondNpcProfile.memory.length, 1);
assert.notEqual(secondNpcProfile.actor_id, npcMemory.actor_id);

const storedNpcProfile = harness.storage.get(storageKey("user-1", "secondspawn_actor", "profile:npc-guide"));
assert.equal(storedNpcProfile.value.actor_id, "npc-guide");

delete storedNpcProfile.value.body.time;
const normalizedNpcProfile = JSON.parse(harness.registeredRpcs.get("secondspawn_actor_profile_get")(
  { userId: "user-1", env: {} },
  harness.logger,
  harness.nk,
  JSON.stringify({ actor_id: "npc-guide" })
));
assert.equal(normalizedNpcProfile.body.time.remaining_seconds, 86400);
const rewrittenNpcProfile = harness.storage.get(storageKey("user-1", "secondspawn_actor", "profile:npc-guide"));
assert.equal(rewrittenNpcProfile.value.body.time.remaining_seconds, 86400);
assert.notEqual(rewrittenNpcProfile.version, storedNpcProfile.version);

const updatedMemory = JSON.parse(harness.registeredRpcs.get("secondspawn_memory_add")(
  { userId: "user-1", env: {} },
  harness.logger,
  harness.nk,
  JSON.stringify({ kind: "preference", summary: "Prefers safe farming overnight.", importance: 9 })
));
assert.match(updatedMemory.body.memory[0].id, /^mem-user-1-00000000-0000-4000-8000-[0-9]{12}-2$/);
assert.equal(updatedMemory.body.memory[0].summary, "Prefers safe farming overnight.");
assert.equal(updatedMemory.body.memory[0].importance, 9);

const dedupedMemory = JSON.parse(harness.registeredRpcs.get("secondspawn_memory_add")(
  { userId: "user-1", env: {} },
  harness.logger,
  harness.nk,
  JSON.stringify({ kind: "preference", summary: "prefers safe farming overnight.", importance: 3 })
));
assert.equal(dedupedMemory.body.memory.length, 2);
assert.equal(dedupedMemory.body.memory[0].importance, 9);

const updatedSoul = JSON.parse(harness.registeredRpcs.get("secondspawn_soul_update")(
  { userId: "user-1", env: {} },
  harness.logger,
  harness.nk,
  JSON.stringify({
    soul: { name: "JOY Agent", core_drive: "protect body time" },
    characteristics: { curiosity: 99, aggression: -1 },
    agent_policy: { enabled: true, mode: "safe_patrol", stop_when_body_time_below: 600 }
  })
));
assert.equal(updatedSoul.body.soul.name, "JOY Agent");
assert.equal(updatedSoul.body.characteristics.curiosity, 10);
assert.equal(updatedSoul.body.characteristics.aggression, 1);
assert.equal(updatedSoul.body.agent_policy.mode, "safe_patrol");

const decision = JSON.parse(harness.registeredRpcs.get("secondspawn_agent_decide")(
  { userId: "user-1", env: {} },
  harness.logger,
  harness.nk,
  JSON.stringify({
    world_snapshot: { position: { x: 2, z: 3 }, body_time_seconds: 3600 },
    allowed: ["move", "say", "stop"]
  })
));
assert.equal(decision.action, "move");
assert.equal(decision.move.x, 3.5);
const afterMoveDecision = JSON.parse(harness.registeredRpcs.get("secondspawn_profile_get")(
  { userId: "user-1", env: {} },
  harness.logger,
  harness.nk,
  ""
));
assert.equal(afterMoveDecision.body.agent_runtime.decision_count, 1);
assert.equal(afterMoveDecision.body.agent_runtime.move_intent_count, 1);
assert.equal(afterMoveDecision.body.agent_activity[0].kind, "agent_decision");
assert.match(afterMoveDecision.body.agent_activity[0].id, /^act-user-1-00000000-0000-4000-8000-[0-9]{12}-2$/);
assert.equal(afterMoveDecision.body.agent_activity[0].metrics.decisions_made, 1);

const lowTimeDecision = JSON.parse(harness.registeredRpcs.get("secondspawn_agent_decide")(
  { userId: "user-1", env: {} },
  harness.logger,
  harness.nk,
  JSON.stringify({
    world_snapshot: { position: { x: 2, z: 3 }, body_time_seconds: 30 },
    allowed: ["move", "say", "stop"]
  })
));
assert.equal(lowTimeDecision.action, "stop");
const afterStopDecision = JSON.parse(harness.registeredRpcs.get("secondspawn_profile_get")(
  { userId: "user-1", env: {} },
  harness.logger,
  harness.nk,
  ""
));
assert.equal(afterStopDecision.body.agent_runtime.decision_count, 2);
assert.equal(afterStopDecision.body.agent_runtime.fallback_decision_count, 2);
assert.equal(afterStopDecision.body.agent_runtime.stop_intent_count, 1);

const zeroTimeDecision = JSON.parse(harness.registeredRpcs.get("secondspawn_agent_decide")(
  { userId: "user-1", env: {} },
  harness.logger,
  harness.nk,
  JSON.stringify({
    world_snapshot: { position: { x: 2, z: 3 }, body_time_seconds: 0 },
    allowed: ["move", "say", "stop"]
  })
));
assert.equal(zeroTimeDecision.action, "stop");
assert.equal(zeroTimeDecision.reason, "body_time_below_policy_threshold");
const afterZeroTimeDecision = JSON.parse(harness.registeredRpcs.get("secondspawn_profile_get")(
  { userId: "user-1", env: {} },
  harness.logger,
  harness.nk,
  ""
));
assert.equal(afterZeroTimeDecision.body.agent_runtime.decision_count, 3);
assert.equal(afterZeroTimeDecision.body.agent_runtime.fallback_decision_count, 3);
assert.equal(afterZeroTimeDecision.body.agent_runtime.stop_intent_count, 2);

const activityContext = JSON.parse(harness.registeredRpcs.get("secondspawn_agent_activity_add")(
  { userId: "user-1", env: {} },
  harness.logger,
  harness.nk,
  JSON.stringify({
    kind: "offline_session",
    summary: "Agent patrolled the hub while the player was away.",
    metrics: {
      offline_seconds: 45,
      fallback_decisions: 2,
      say_intents: 1
    }
  })
));
assert.equal(activityContext.body.agent_runtime.offline_seconds, 45);
assert.equal(activityContext.body.agent_runtime.fallback_decision_count, 5);
assert.equal(activityContext.body.agent_runtime.say_intent_count, 1);
assert.equal(activityContext.body.agent_activity[0].kind, "offline_session");

const normalizedActivityContext = JSON.parse(harness.registeredRpcs.get("secondspawn_agent_activity_add")(
  { userId: "user-1", env: {} },
  harness.logger,
  harness.nk,
  JSON.stringify({
    kind: "offline_session",
    summary: "Agent attempted to report malformed metrics.",
    occurred_at: "not-a-real-date",
    metrics: {
      offline_seconds: "1e309",
      decisions_made: 9999999999,
      fallback_decisions: -4,
      say_intents: "2.9"
    }
  })
));
assert.notEqual(normalizedActivityContext.body.agent_activity[0].occurred_at, "not-a-real-date");
assert.ok(!Number.isNaN(Date.parse(normalizedActivityContext.body.agent_activity[0].occurred_at)));
assert.equal(normalizedActivityContext.body.agent_runtime.offline_seconds, 45);
assert.equal(normalizedActivityContext.body.agent_runtime.decision_count, 1000000000);
assert.equal(normalizedActivityContext.body.agent_runtime.fallback_decision_count, 5);
assert.equal(normalizedActivityContext.body.agent_runtime.say_intent_count, 3);

const idempotencyHarness = createRuntimeHarness(module);
idempotencyHarness.registeredRpcs.get("secondspawn_profile_get")(
  { userId: "idempotent-user", env: {} },
  idempotencyHarness.logger,
  idempotencyHarness.nk,
  ""
);
const retryActivityPayload = JSON.stringify({
  id: "activity-retry-1",
  kind: "offline_session",
  summary: "Retried activity should only count once.",
  metrics: {
    offline_seconds: 12,
    decisions_made: 2
  }
});
idempotencyHarness.registeredRpcs.get("secondspawn_agent_activity_add")(
  { userId: "idempotent-user", env: {} },
  idempotencyHarness.logger,
  idempotencyHarness.nk,
  retryActivityPayload
);
idempotencyHarness.registeredRpcs.get("secondspawn_agent_activity_add")(
  { userId: "idempotent-user", env: {} },
  idempotencyHarness.logger,
  idempotencyHarness.nk,
  retryActivityPayload
);
const afterRetriedActivity = JSON.parse(idempotencyHarness.registeredRpcs.get("secondspawn_profile_get")(
  { userId: "idempotent-user", env: {} },
  idempotencyHarness.logger,
  idempotencyHarness.nk,
  ""
));
assert.equal(afterRetriedActivity.body.agent_runtime.offline_seconds, 12);
assert.equal(afterRetriedActivity.body.agent_runtime.decision_count, 2);
assert.equal(afterRetriedActivity.body.agent_runtime.activity_count, 2);
assert.equal(afterRetriedActivity.body.agent_activity.filter((activity) => activity.id === "activity-retry-1").length, 1);

const bodyTimeDeathHarness = createRuntimeHarness(module);
bodyTimeDeathHarness.registeredRpcs.get("secondspawn_profile_get")(
  { userId: "bodytime-death-user", env: {} },
  bodyTimeDeathHarness.logger,
  bodyTimeDeathHarness.nk,
  ""
);
const deathStoredProfile = bodyTimeDeathHarness.storage.get(storageKey("bodytime-death-user", "secondspawn_agent", "context"));
deathStoredProfile.value.body.time.remaining_seconds = 120;
const drainedBodyTime = JSON.parse(bodyTimeDeathHarness.registeredRpcs.get("secondspawn_bodytime_event")(
  { userId: "bodytime-death-user", env: {} },
  bodyTimeDeathHarness.logger,
  bodyTimeDeathHarness.nk,
  JSON.stringify({
    id: "bodytime-drain-1",
    kind: "drain",
    source: "danger_zone_tick",
    amount_seconds: 300
  })
));
assert.equal(drainedBodyTime.body.time.remaining_seconds, 0);
assert.equal(drainedBodyTime.body.lifecycle, "dead");
assert.match(drainedBodyTime.body.agent_activity[0].summary, /died/);
assert.throws(
  () => bodyTimeDeathHarness.registeredRpcs.get("secondspawn_bodytime_event")(
    { userId: "bodytime-death-user", env: {} },
    bodyTimeDeathHarness.logger,
    bodyTimeDeathHarness.nk,
    JSON.stringify({
      kind: "earn",
      source: "prototype_safe_farming",
      amount_seconds: 60
    })
  ),
  /dead body/
);
assert.throws(
  () => bodyTimeDeathHarness.registeredRpcs.get("secondspawn_bodytime_event")(
    { userId: "bodytime-death-user", env: {} },
    bodyTimeDeathHarness.logger,
    bodyTimeDeathHarness.nk,
    JSON.stringify({
      kind: "drain",
      source: "danger_zone_tick",
      amount_seconds: 60
    })
  ),
  /dead body/
);
assert.throws(
  () => bodyTimeDeathHarness.registeredRpcs.get("secondspawn_bodytime_event")(
    { userId: "bodytime-death-user", env: {} },
    bodyTimeDeathHarness.logger,
    bodyTimeDeathHarness.nk,
    JSON.stringify({
      kind: "earn",
      source: "unknown_source",
      amount_seconds: 60
    })
  ),
  /source is not allowed/
);

const debugDrainHarness = createRuntimeHarness(module);
debugDrainHarness.registeredRpcs.get("secondspawn_profile_get")(
  { userId: "debug-drain-user", env: {} },
  debugDrainHarness.logger,
  debugDrainHarness.nk,
  ""
);
assert.throws(
  () => debugDrainHarness.registeredRpcs.get("secondspawn_bodytime_event")(
    { userId: "debug-drain-user", env: {} },
    debugDrainHarness.logger,
    debugDrainHarness.nk,
    JSON.stringify({
      id: "debug-drain-rejected",
      kind: "drain",
      source: "prototype_reincarnation_debug",
      amount_seconds: 86400
    })
  ),
  /source is not allowed/
);
const debugDrainedBody = JSON.parse(debugDrainHarness.registeredRpcs.get("secondspawn_bodytime_event")(
  {
    userId: "debug-drain-user",
    env: { SECOND_SPAWN_ENABLE_DEBUG_BODYTIME: "true" }
  },
  debugDrainHarness.logger,
  debugDrainHarness.nk,
  JSON.stringify({
    id: "debug-drain-accepted",
    kind: "drain",
    source: "prototype_reincarnation_debug",
    amount_seconds: 86400,
    note: "Debug fatal drain."
  })
));
assert.equal(debugDrainedBody.body.time.remaining_seconds, 0);
assert.equal(debugDrainedBody.body.lifecycle, "dead");
assert.equal(debugDrainedBody.body.agent_activity[0].body_time_source, "prototype_reincarnation_debug");

const reincarnationHarness = createRuntimeHarness(module);
reincarnationHarness.registeredRpcs.get("secondspawn_profile_get")(
  { userId: "reincarnation-user", env: {} },
  reincarnationHarness.logger,
  reincarnationHarness.nk,
  ""
);
const reincarnationStoredProfile = reincarnationHarness.storage.get(storageKey("reincarnation-user", "secondspawn_agent", "context"));
reincarnationStoredProfile.value.body.time.remaining_seconds = 60;
reincarnationHarness.registeredRpcs.get("secondspawn_bodytime_event")(
  { userId: "reincarnation-user", env: {} },
  reincarnationHarness.logger,
  reincarnationHarness.nk,
  JSON.stringify({
    id: "reincarnation-drain-1",
    kind: "drain",
    source: "danger_zone_tick",
    amount_seconds: 120
  })
);
const reincarnatedProfile = JSON.parse(reincarnationHarness.registeredRpcs.get("secondspawn_reincarnate")(
  { userId: "reincarnation-user", env: {} },
  reincarnationHarness.logger,
  reincarnationHarness.nk,
  JSON.stringify({
    id: "reincarnation-1",
    reason: "prototype zero-time recovery"
  })
));
assert.equal(reincarnatedProfile.player.second_balance_seconds, 172800);
assert.equal(reincarnatedProfile.player.reincarnation_count, 1);
assert.equal(reincarnatedProfile.body.body_id, "body-reincarnation-user-r1");
assert.equal(reincarnatedProfile.body.lifecycle, "alive");
assert.equal(reincarnatedProfile.body.time.remaining_seconds, 86400);
assert.ok(reincarnatedProfile.body.archetype_id.length > 0);
assert.ok(reincarnatedProfile.body.visual_variant >= 0);
assert.ok(reincarnatedProfile.body.story.role.length > 0);
assert.equal(reincarnatedProfile.body.agent_activity[0].id, "reincarnation-1");
assert.equal(reincarnatedProfile.body.agent_activity[0].kind, "reincarnation");
assert.match(reincarnatedProfile.body.memory[0].summary, /Consciousness transferred/);
const reincarnatedSourceActor = reincarnationHarness.storage.get(storageKey(
  "reincarnation-user",
  "secondspawn_actor",
  `profile:${reincarnatedProfile.body.inhabitation.source_actor_id}`
));
assert.ok(reincarnatedSourceActor);
assert.equal(reincarnatedSourceActor.value.actor_type, "player_body");
assert.equal(reincarnatedSourceActor.value.body.body_id, "body-reincarnation-user-r1");
assert.equal(reincarnatedSourceActor.value.body.inhabitation.inhabited_by_player, true);

const retriedReincarnation = JSON.parse(reincarnationHarness.registeredRpcs.get("secondspawn_reincarnate")(
  { userId: "reincarnation-user", env: {} },
  reincarnationHarness.logger,
  reincarnationHarness.nk,
  JSON.stringify({
    id: "reincarnation-1",
    reason: "retry should not spend twice"
  })
));
assert.equal(retriedReincarnation.player.second_balance_seconds, 172800);
assert.equal(retriedReincarnation.player.reincarnation_count, 1);
assert.equal(retriedReincarnation.body.agent_activity.filter((activity) => activity.id === "reincarnation-1").length, 1);
assert.throws(
  () => reincarnationHarness.registeredRpcs.get("secondspawn_reincarnate")(
    { userId: "reincarnation-user", env: {} },
    reincarnationHarness.logger,
    reincarnationHarness.nk,
    JSON.stringify({
      id: "reincarnation-2",
      reason: "alive bodies cannot reincarnate"
    })
  ),
  /body must be dead/
);

const insufficientReincarnationHarness = createRuntimeHarness(module);
insufficientReincarnationHarness.registeredRpcs.get("secondspawn_profile_get")(
  { userId: "insufficient-second-user", env: {} },
  insufficientReincarnationHarness.logger,
  insufficientReincarnationHarness.nk,
  ""
);
const insufficientProfile = insufficientReincarnationHarness.storage.get(storageKey("insufficient-second-user", "secondspawn_agent", "context"));
insufficientProfile.value.player.second_balance_seconds = 100;
insufficientProfile.value.body.lifecycle = "dead";
insufficientProfile.value.body.time.remaining_seconds = 0;
assert.throws(
  () => insufficientReincarnationHarness.registeredRpcs.get("secondspawn_reincarnate")(
    { userId: "insufficient-second-user", env: {} },
    insufficientReincarnationHarness.logger,
    insufficientReincarnationHarness.nk,
    JSON.stringify({
      id: "reincarnation-insufficient-1"
    })
  ),
  /insufficient SECOND balance/
);

const interactHarness = createRuntimeHarness(module);
interactHarness.registeredRpcs.get("secondspawn_profile_get")(
  { userId: "interact-user", env: {} },
  interactHarness.logger,
  interactHarness.nk,
  ""
);
const interactDecision = JSON.parse(interactHarness.registeredRpcs.get("secondspawn_agent_decide")(
  { userId: "interact-user", env: {} },
  interactHarness.logger,
  interactHarness.nk,
  JSON.stringify({
    world_snapshot: {
      position: { x: 2, z: 3 },
      body_time_seconds: 3600,
      nearby_objects: [{ id: "cache-1", kind: "supply_cache", distance: 1.2 }]
    },
    allowed: ["interact"]
  })
));
assert.equal(interactDecision.action, "interact");
assert.equal(interactDecision.target_id, "cache-1");
const afterInteractDecision = JSON.parse(interactHarness.registeredRpcs.get("secondspawn_profile_get")(
  { userId: "interact-user", env: {} },
  interactHarness.logger,
  interactHarness.nk,
  ""
));
assert.equal(afterInteractDecision.body.agent_runtime.decision_count, 1);
assert.equal(afterInteractDecision.body.agent_runtime.interact_intent_count, 1);
assert.match(afterInteractDecision.body.agent_activity[0].summary, /Agent chose interact/);

const missingInteractTargetDecision = JSON.parse(interactHarness.registeredRpcs.get("secondspawn_agent_decide")(
  { userId: "interact-user", env: {} },
  interactHarness.logger,
  interactHarness.nk,
  JSON.stringify({
    world_snapshot: {
      position: { x: 2, z: 3 },
      body_time_seconds: 3600,
      nearby_objects: []
    },
    allowed: ["interact"]
  })
));
assert.equal(missingInteractTargetDecision.action, "stop");
assert.equal(missingInteractTargetDecision.source_reason, "nakama_no_allowed_action");

const dedupeHarness = createRuntimeHarness(module);
dedupeHarness.registeredRpcs.get("secondspawn_profile_get")(
  { userId: "dedupe-user", env: {} },
  dedupeHarness.logger,
  dedupeHarness.nk,
  ""
);
const repeatedMovePayload = JSON.stringify({
  world_snapshot: { position: { x: 2, z: 3 }, body_time_seconds: 3600 },
  allowed: ["move", "stop"]
});
dedupeHarness.registeredRpcs.get("secondspawn_agent_decide")(
  { userId: "dedupe-user", env: {} },
  dedupeHarness.logger,
  dedupeHarness.nk,
  repeatedMovePayload
);
dedupeHarness.registeredRpcs.get("secondspawn_agent_decide")(
  { userId: "dedupe-user", env: {} },
  dedupeHarness.logger,
  dedupeHarness.nk,
  repeatedMovePayload
);
const afterRepeatedMove = JSON.parse(dedupeHarness.registeredRpcs.get("secondspawn_profile_get")(
  { userId: "dedupe-user", env: {} },
  dedupeHarness.logger,
  dedupeHarness.nk,
  ""
));
assert.equal(afterRepeatedMove.body.agent_runtime.decision_count, 2);
assert.equal(afterRepeatedMove.body.agent_runtime.move_intent_count, 2);
assert.equal(afterRepeatedMove.body.agent_activity.filter((activity) => activity.kind === "agent_decision").length, 1);

const activityConflictHarness = createRuntimeHarness(module);
activityConflictHarness.registeredRpcs.get("secondspawn_profile_get")(
  { userId: "activity-conflict-user", env: {} },
  activityConflictHarness.logger,
  activityConflictHarness.nk,
  ""
);
activityConflictHarness.conflictNextWrite();
const activityConflictResponse = JSON.parse(activityConflictHarness.registeredRpcs.get("secondspawn_agent_activity_add")(
  { userId: "activity-conflict-user", env: {} },
  activityConflictHarness.logger,
  activityConflictHarness.nk,
  JSON.stringify({
    kind: "agent_decision",
    summary: "This append-only activity should be retried after a stale version.",
    metrics: { decisions_made: 1 }
  })
));
assert.equal(activityConflictResponse.body.agent_runtime.decision_count, 1);
assert.equal(
  activityConflictResponse.body.agent_activity[0].summary,
  "This append-only activity should be retried after a stale version."
);

const conflictHarness = createRuntimeHarness(module);
conflictHarness.registeredRpcs.get("secondspawn_profile_get")(
  { userId: "conflict-user", env: {} },
  conflictHarness.logger,
  conflictHarness.nk,
  ""
);
conflictHarness.conflictNextWrite();
assert.throws(
  () => conflictHarness.registeredRpcs.get("secondspawn_memory_add")(
    { userId: "conflict-user", env: {} },
    conflictHarness.logger,
    conflictHarness.nk,
    JSON.stringify({ kind: "preference", summary: "This write should detect a stale version." })
  ),
  /storage version conflict/
);

const actorConflictHarness = createRuntimeHarness(module);
actorConflictHarness.registeredRpcs.get("secondspawn_actor_profile_get")(
  { userId: "actor-conflict-user", env: {} },
  actorConflictHarness.logger,
  actorConflictHarness.nk,
  JSON.stringify({ actor_id: "npc-conflict" })
);
actorConflictHarness.conflictNextWrite();
assert.throws(
  () => actorConflictHarness.registeredRpcs.get("secondspawn_actor_memory_add")(
    { userId: "actor-conflict-user", env: {} },
    actorConflictHarness.logger,
    actorConflictHarness.nk,
    JSON.stringify({
      actor_id: "npc-conflict",
      kind: "relationship",
      summary: "This actor memory write should detect a stale version."
    })
  ),
  /storage version conflict/
);

const openClawHarness = createRuntimeHarness(module);
const openClawActor = JSON.parse(openClawHarness.registeredRpcs.get("secondspawn_actor_profile_get")(
  { userId: "openclaw-owner", env: {} },
  openClawHarness.logger,
  openClawHarness.nk,
  JSON.stringify({
    actor_id: "npc-openclaw-guide",
    display_name: "OpenClaw Guide"
  })
));
assert.equal(openClawActor.actor_id, "npc-openclaw-guide");

const openClawBinding = JSON.parse(openClawHarness.registeredRpcs.get("secondspawn_openclaw_bind")(
  { userId: "openclaw-owner", env: {} },
  openClawHarness.logger,
  openClawHarness.nk,
  JSON.stringify({
    frame_actor_id: "npc-openclaw-guide",
    connected_agent_id: "oc-agent-guide-1",
    agent_kind: "companion",
    consent_scope: ["dialogue", "heartbeat", "intent:say"],
    rate_limit_profile: {
      requests_per_minute: 12,
      intents_per_minute: 6,
      tokens_per_day: 1234
    }
  })
));
assert.equal(openClawBinding.frame_actor_id, "npc-openclaw-guide");
assert.equal(openClawBinding.connected_agent_id, "oc-agent-guide-1");
assert.equal(openClawBinding.controller_type, "openclaw");
assert.equal(openClawBinding.owner_player_id, "openclaw-owner");
assert.equal(openClawBinding.connection_status, "connected");
assert.equal(openClawBinding.moderation_state, "active");
assert.equal(openClawBinding.rate_limit_profile.requests_per_minute, 12);
assert.equal(openClawBinding.rate_limit_profile.intents_per_minute, 6);
assert.equal(openClawBinding.rate_limit_profile.tokens_per_day, 1234);

const openClawContext = JSON.parse(openClawHarness.registeredRpcs.get("secondspawn_openclaw_context_get")(
  { userId: "openclaw-owner", env: {} },
  openClawHarness.logger,
  openClawHarness.nk,
  JSON.stringify({ connected_agent_id: "oc-agent-guide-1" })
));
assert.equal(openClawContext.binding.frame_actor_id, "npc-openclaw-guide");
assert.equal(openClawContext.context.identity.public_name, "OpenClaw Guide");
assert.equal(openClawContext.context.body.body_id, "body-npc-openclaw-guide");
assert.equal(openClawContext.context.body.lifecycle, "alive");
assert.ok(openClawContext.context.body.stats.max_health > 0);
assert.ok(openClawContext.context.body.time.remaining_seconds > 0);
assert.ok(openClawContext.context.tools.some((tool) => tool.intent === "say"));
assert.equal(openClawContext.context.skills, undefined);
assert.equal(openClawContext.context.agents, undefined);

const openClawIntent = JSON.parse(openClawHarness.registeredRpcs.get("secondspawn_openclaw_intent_submit")(
  { userId: "openclaw-owner", env: {} },
  openClawHarness.logger,
  openClawHarness.nk,
  JSON.stringify({
    connected_agent_id: "oc-agent-guide-1",
    id: "oc-intent-1",
    intent: "say",
    payload: { text: "The gate is clear for now." },
    reason: "player asked for hub status"
  })
));
assert.equal(openClawIntent.accepted, true);
assert.equal(openClawIntent.status, "pending_validation");
assert.equal(openClawIntent.intent.intent, "say");
assert.equal(openClawIntent.activity.kind, "openclaw_intent");
assert.match(openClawIntent.activity.summary, /say/);

assert.throws(
  () => openClawHarness.registeredRpcs.get("secondspawn_openclaw_intent_submit")(
    { userId: "openclaw-owner", env: {} },
    openClawHarness.logger,
    openClawHarness.nk,
    JSON.stringify({
      connected_agent_id: "oc-agent-guide-1",
      id: "oc-intent-2",
      intent: "attack",
      payload: { target_id: "npc-target" }
    })
  ),
  /intent is not allowed for this Frame/
);

assert.throws(
  () => openClawHarness.registeredRpcs.get("secondspawn_openclaw_intent_submit")(
    { userId: "openclaw-owner", env: {} },
    openClawHarness.logger,
    openClawHarness.nk,
    JSON.stringify({
      connected_agent_id: "oc-agent-guide-1",
      id: "oc-intent-3",
      intent: "move",
      payload: { x: 1, z: 1 }
    })
  ),
  /intent is outside consent scope/
);

const openClawHeartbeat = JSON.parse(openClawHarness.registeredRpcs.get("secondspawn_openclaw_heartbeat")(
  { userId: "openclaw-owner", env: {} },
  openClawHarness.logger,
  openClawHarness.nk,
  JSON.stringify({
    connected_agent_id: "oc-agent-guide-1",
    connection_status: "degraded",
    summary: "External agent is online but model latency is high."
  })
));
assert.equal(openClawHeartbeat.binding.connection_status, "degraded");
assert.equal(openClawHeartbeat.context.heartbeat.offline_session_state, "degraded");
assert.equal(openClawHeartbeat.context.heartbeat.last_action_summary, "External agent is online but model latency is high.");
assert.equal(openClawHeartbeat.activity.kind, "openclaw_heartbeat");

const storedOpenClawActor = openClawHarness.storage.get(storageKey("openclaw-owner", "secondspawn_actor", "profile:npc-openclaw-guide"));
assert.equal(storedOpenClawActor.value.agent_activity[0].kind, "openclaw_heartbeat");
assert.equal(storedOpenClawActor.value.agent_activity[1].kind, "openclaw_intent");
assert.equal(storedOpenClawActor.value.body.heartbeat.offline_session_state, "degraded");

const calls = [];
const response = harness.registeredHooks[0](
  {
    env: {
      SUPABASE_URL: "https://project.supabase.co/",
      SUPABASE_PUBLISHABLE_KEY: "sb_publishable_test",
    },
  },
  { error: (message) => calls.push(["error", message]), info: () => {}, debug: () => {} },
  {
    httpRequest: (url, method, headers) => {
      calls.push(["http", url, method, headers]);
      return {
        code: 200,
        body: JSON.stringify({
          id: "308ebb59-47b7-46fe-835c-5375cd41037d",
          email: "joy@example.com",
        }),
      };
    },
  },
  { account: { id: "supabase-access-token" } }
);

assert.equal(response.account.id, "supabase-308ebb59-47b7-46fe-835c-5375cd41037d");
assert.equal(response.username, "player-308ebb59-47b");
assert.equal(calls[0][0], "http");
assert.equal(calls[0][1], "https://project.supabase.co/auth/v1/user");
assert.equal(calls[0][2], "get");
assert.equal(calls[0][3].apikey, "sb_publishable_test");
assert.equal(calls[0][3].authorization, "Bearer supabase-access-token");

const rejected = harness.registeredHooks[0](
  {
    env: {
      SUPABASE_URL: "https://project.supabase.co",
      SUPABASE_PUBLISHABLE_KEY: "sb_publishable_test",
    },
  },
  { error: () => {}, info: () => {}, debug: () => {} },
  {
    httpRequest: () => ({ code: 401, body: "invalid token" }),
  },
  { account: { id: "bad-token" } }
);
assert.equal(rejected, null);

console.log("supabase_custom_auth tests passed");
