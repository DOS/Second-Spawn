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
          if (request.version === "") {
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
assert.equal(harness.registeredRpcs.size, 7);
assert.ok(harness.registeredRpcs.has("secondspawn_health"));
assert.ok(harness.registeredRpcs.has("secondspawn_profile_get"));
assert.ok(harness.registeredRpcs.has("secondspawn_memory_add"));
assert.ok(harness.registeredRpcs.has("secondspawn_soul_update"));
assert.ok(harness.registeredRpcs.has("secondspawn_agent_decide"));
assert.ok(harness.registeredRpcs.has("secondspawn_agent_activity_add"));
assert.ok(harness.registeredRpcs.has("secondspawn_bodytime_event"));

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

const healthPayload = harness.registeredRpcs.get("secondspawn_health")({ userId: "user-1", env: {} }, harness.logger, harness.nk, "");
assert.equal(JSON.parse(healthPayload).service, "second-spawn-nakama");

const profile = JSON.parse(harness.registeredRpcs.get("secondspawn_profile_get")(
  { userId: "user-1", env: {} },
  harness.logger,
  harness.nk,
  ""
));
assert.equal(profile.player.player_id, "user-1");
assert.equal(profile.body.soul.name, "user-1");
assert.equal(profile.body.memory.length, 1);
assert.equal(profile.body.equipment.primary_weapon, "none");
assert.equal(profile.body.equipment.equipment_visual_id, 0);
assert.equal(profile.body.stats.level, 1);
assert.equal(profile.body.stats.vitality, 10);
assert.equal(profile.body.stats.agility, 8);
assert.equal(profile.body.stats.max_health, 100);
assert.equal(profile.body.stats.attack_power, 10);
assert.equal(profile.body.time.remaining_seconds, 86400);
assert.equal(profile.body.lifecycle, "alive");
assert.equal(profile.body.agent_runtime.decision_count, 0);
assert.equal(profile.body.agent_runtime.fallback_decision_count, 0);
assert.equal(profile.body.agent_activity.length, 1);
assert.equal(profile.body.agent_activity[0].kind, "profile_bootstrap");

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

const storedProfile = harness.storage.get(storageKey("user-1", "secondspawn_agent", "context"));
delete storedProfile.value.body.agent_runtime;
delete storedProfile.value.body.agent_activity;
const migratedProfile = JSON.parse(harness.registeredRpcs.get("secondspawn_profile_get")(
  { userId: "user-1", env: {} },
  harness.logger,
  harness.nk,
  ""
));
assert.equal(migratedProfile.body.agent_runtime.activity_count, 1);
assert.equal(migratedProfile.body.agent_activity.length, 1);
assert.equal(migratedProfile.body.agent_activity[0].kind, "profile_bootstrap");

const updatedMemory = JSON.parse(harness.registeredRpcs.get("secondspawn_memory_add")(
  { userId: "user-1", env: {} },
  harness.logger,
  harness.nk,
  JSON.stringify({ kind: "preference", summary: "Prefers safe farming overnight.", importance: 9 })
));
assert.match(updatedMemory.body.memory[0].id, /^mem-user-1-00000000-0000-4000-8000-000000000001-2$/);
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
