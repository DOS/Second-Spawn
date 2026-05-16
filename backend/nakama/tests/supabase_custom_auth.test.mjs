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
        storage.set(storageKey(request.userId, request.collection, request.key), {
          ...request,
          version: "test-version",
        });
      }
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

  return { registeredHooks, registeredRpcs, storage, logger, nk };
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
assert.equal(harness.registeredRpcs.size, 6);
assert.ok(harness.registeredRpcs.has("secondspawn_health"));
assert.ok(harness.registeredRpcs.has("secondspawn_profile_get"));
assert.ok(harness.registeredRpcs.has("secondspawn_memory_add"));
assert.ok(harness.registeredRpcs.has("secondspawn_soul_update"));
assert.ok(harness.registeredRpcs.has("secondspawn_agent_decide"));
assert.ok(harness.registeredRpcs.has("secondspawn_agent_activity_add"));

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
assert.equal(profile.body.agent_runtime.decision_count, 0);
assert.equal(profile.body.agent_runtime.fallback_decision_count, 0);
assert.equal(profile.body.agent_activity.length, 1);
assert.equal(profile.body.agent_activity[0].kind, "profile_bootstrap");

const updatedMemory = JSON.parse(harness.registeredRpcs.get("secondspawn_memory_add")(
  { userId: "user-1", env: {} },
  harness.logger,
  harness.nk,
  JSON.stringify({ kind: "preference", summary: "Prefers safe farming overnight.", importance: 9 })
));
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
assert.equal(activityContext.body.agent_runtime.fallback_decision_count, 4);
assert.equal(activityContext.body.agent_runtime.say_intent_count, 1);
assert.equal(activityContext.body.agent_activity[0].kind, "offline_session");

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
