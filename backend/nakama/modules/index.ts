// SECOND SPAWN Nakama runtime entrypoint.
//
// Nakama is the game backend. This module owns game-backend extensions such as
// health checks, Supabase-backed custom authentication, player profile, soul,
// policy, and compact memory. AI/LLM provider calls stay in api.dos.ai.

var collectionAgent = "secondspawn_agent";
var keyAgentContext = "context";

var rpcIdHealth = "secondspawn_health";
var rpcIdProfileGet = "secondspawn_profile_get";
var rpcIdMemoryAdd = "secondspawn_memory_add";
var rpcIdSoulUpdate = "secondspawn_soul_update";
var rpcIdAgentDecide = "secondspawn_agent_decide";
var rpcIdAgentActivityAdd = "secondspawn_agent_activity_add";
var rpcIdBodyTimeEvent = "secondspawn_bodytime_event";
var rpcIdReincarnate = "secondspawn_reincarnate";
var rpcIdCultivationEvent = "secondspawn_cultivation_event";
var agentActivityLogLimit = 32;
var agentRuntimeMetricMax = 1000000000;
var bodyTimeMaxSeconds = 86400 * 30;
var bodyTimeEarnCapSeconds = 3600;
var bodyTimeSpendCapSeconds = 600;
var bodyTimeDrainCapSeconds = 300;
var bodyTimeEarnCooldownSeconds = 60;
var secondPrototypeMaxBalanceSeconds = 86400 * 365;
var secondPrototypeStartingBalanceSeconds = 86400 * 7;
var secondPrototypeReincarnationCostSeconds = 86400 * 5;
var cultivationPrototypeXpCap = 500;
var cultivationAwakeningToEnhancementXp = 1000;

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
  initializer.registerRpc(rpcIdBodyTimeEvent, rpcBodyTimeEvent);
  initializer.registerRpc(rpcIdReincarnate, rpcReincarnate);
  initializer.registerRpc(rpcIdCultivationEvent, rpcCultivationEvent);
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
  var state = getOrCreateAgentContextState(ctx, nk);
  var context = state.context;
  var request = parseJson(payload || "{}", "agent decision payload");
  var world = request.world_snapshot || {};
  var allowed = request.allowed || ["move", "interact", "say", "stop"];
  var interactTargetId = selectInteractTargetId(world);
  var bodyTime = Number(world.body_time_seconds !== undefined && world.body_time_seconds !== null
    ? world.body_time_seconds
    : context.body.time.remaining_seconds || 0);
  var decision: any;

  if (bodyTime <= context.body.agent_policy.stop_when_body_time_below) {
    decision = {
      action: "stop",
      reason: "body_time_below_policy_threshold",
      confidence: 0.9,
      source: "fallback",
      source_reason: "nakama_body_time_policy"
    };
  } else if (arrayContains(allowed, "move")) {
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
      source_reason: "nakama_prototype_patrol"
    };
  } else if (arrayContains(allowed, "interact") && interactTargetId) {
    decision = {
      action: "interact",
      target_id: interactTargetId,
      reason: "prototype_interact_fallback",
      confidence: 0.55,
      source: "fallback",
      source_reason: "nakama_interact_fallback"
    };
  } else if (arrayContains(allowed, "say")) {
    decision = {
      action: "say",
      say: "I am keeping this body safe until the player returns.",
      reason: "prototype_social_fallback",
      confidence: 0.6,
      source: "fallback",
      source_reason: "nakama_social_fallback"
    };
  } else {
    decision = {
      action: "stop",
      reason: "no_allowed_action",
      confidence: 0.5,
      source: "fallback",
      source_reason: "nakama_no_allowed_action"
    };
  }

  recordAgentDecision(context, decision, nk);
  writeAgentContext(nk, context, state.version);
  return JSON.stringify(decision);
}

function rpcAgentActivityAdd(
  ctx: nkruntime.Context,
  logger: nkruntime.Logger,
  nk: nkruntime.Nakama,
  payload: string
): string {
  var state = getOrCreateAgentContextState(ctx, nk);
  var context = state.context;
  var request = parseJson(payload || "{}", "agent activity payload");
  var activity = normalizeAgentActivity(context, request, nk);

  if (addAgentActivity(context, activity, nk)) {
    applyActivityMetrics(context.body.agent_runtime, request.metrics || {});
    writeAgentContext(nk, context, state.version);
  }
  return JSON.stringify(context);
}

function rpcBodyTimeEvent(
  ctx: nkruntime.Context,
  logger: nkruntime.Logger,
  nk: nkruntime.Nakama,
  payload: string
): string {
  var state = getOrCreateAgentContextState(ctx, nk);
  var context = state.context;
  var event = normalizeBodyTimeEvent(parseJson(payload || "{}", "body time payload"));

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
  return JSON.stringify(context);
}

function rpcCultivationEvent(
  ctx: nkruntime.Context,
  logger: nkruntime.Logger,
  nk: nkruntime.Nakama,
  payload: string
): string {
  var state = getOrCreateAgentContextState(ctx, nk);
  var context = state.context;
  var event = normalizeCultivationEvent(parseJson(payload || "{}", "cultivation payload"));

  ensureBodyTime(context);
  if (event.id && hasAgentActivityId(context.body.agent_activity || [], event.id)) {
    return JSON.stringify(context);
  }
  if (context.body.lifecycle === "dead") {
    throw new Error("dead bodies cannot progress cultivation before reincarnation");
  }

  applyCultivationEvent(context, event, nk);
  writeAgentContext(nk, context, state.version);
  return JSON.stringify(context);
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
    return {
      context: existing.value,
      version: existing.version
    };
  }

  var context = defaultAgentContext(userId);
  writeAgentContext(nk, context, "");
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
  if (typeof version === "string") {
    write.version = version;
  }
  nk.storageWrite([write]);
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

function defaultBodyProfile(playerId: string, displayName: string, timestamp: string): any {
  return {
    body_id: "body-" + playerId,
    archetype_id: "prototype-hunter",
    visual_prefab_key: "prototype-random",
    equipment: normalizeEquipment({}),
    stats: {
      level: 1,
      vitality: 10,
      force: 8,
      agility: 8,
      focus: 8,
      resilience: 8,
      max_health: 100,
      max_energy: 50,
      attack_power: 10,
      defense_power: 5
    },
    characteristics: normalizeTraits({}),
    time: {
      remaining_seconds: 86400,
      max_seconds: 86400,
      danger_drain_rate: 1
    },
    cultivation: {
      tier: "Awakening",
      progress_xp: 0
    },
    lifecycle: "alive",
    agent_policy: normalizePolicy({}),
    soul: normalizeSoul({}, displayName),
    memory: [{
      id: "seed-origin",
      kind: "system",
      summary: "The character is a Second Spawn prototype body controlled by the player or their offline agent.",
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

function normalizeBodyTimeEvent(request: any): any {
  var kind = normalizeBodyTimeEventKind(request.kind);
  var source = normalizeBodyTimeEventSource(kind, request.source);
  var amount = normalizeBodyTimeAmount(kind, firstDefined(request.amount_seconds, request.seconds));
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

function normalizeBodyTimeEventSource(kind: string, source: any): string {
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
  throw new Error("body time source is not allowed for " + kind);
}

function normalizeBodyTimeAmount(kind: string, amount: any): number {
  var numberValue = Number(amount);
  if (isNaN(numberValue) || !isFinite(numberValue) || numberValue <= 0) {
    throw new Error("body time amount_seconds must be a positive finite number");
  }

  var maxAmount = bodyTimeDrainCapSeconds;
  if (kind === "earn") {
    maxAmount = bodyTimeEarnCapSeconds;
  } else if (kind === "spend") {
    maxAmount = bodyTimeSpendCapSeconds;
  }

  return clampNumber(Math.floor(numberValue), 1, maxAmount);
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
  var nextBody = defaultBodyProfile(context.player.player_id, context.player.display_name || context.player.player_id, timestamp);

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

function normalizeCultivationEvent(request: any): any {
  var source = trimString(request.source);
  if (source !== "prototype_nibirium_absorb") {
    throw new Error("cultivation source is not allowed");
  }

  var amount = Number(firstDefined(request.amount_xp, request.xp));
  if (isNaN(amount) || !isFinite(amount) || amount <= 0) {
    throw new Error("cultivation amount_xp must be a positive finite number");
  }

  return {
    id: trimString(request.id),
    source: source,
    amount_xp: clampNumber(Math.floor(amount), 1, cultivationPrototypeXpCap),
    note: trimString(request.note)
  };
}

function applyCultivationEvent(context: any, event: any, nk: nkruntime.Nakama): void {
  ensureCultivation(context);
  var cultivation = context.body.cultivation;
  var beforeTier = cultivation.tier;
  var beforeXp = cultivation.progress_xp;
  var promoted = false;

  if (cultivation.tier === "Awakening") {
    cultivation.progress_xp = clampNumber(cultivation.progress_xp + event.amount_xp, 0, cultivationAwakeningToEnhancementXp);
    if (cultivation.progress_xp >= cultivationAwakeningToEnhancementXp) {
      cultivation.tier = "Enhancement";
      cultivation.progress_xp = 0;
      promoted = true;
    }
  } else {
    cultivation.tier = "Enhancement";
    cultivation.progress_xp = clampNumber(cultivation.progress_xp + event.amount_xp, 0, cultivationAwakeningToEnhancementXp);
  }

  addAgentActivity(context, {
    id: event.id || "",
    kind: "cultivation",
    summary: cultivationActivitySummary(event, beforeTier, beforeXp, cultivation, promoted),
    source: "nakama",
    cultivation_source: event.source,
    cultivation_xp: event.amount_xp,
    metrics: {
      cultivation_xp: event.amount_xp,
      cultivation_before_xp: beforeXp,
      cultivation_after_xp: cultivation.progress_xp,
      cultivation_promoted: promoted ? 1 : 0
    }
  }, nk);
}

function ensureCultivation(context: any): void {
  if (!context.body) {
    context.body = {};
  }
  if (!context.body.cultivation) {
    context.body.cultivation = {};
  }

  var tier = trimString(context.body.cultivation.tier);
  if (tier !== "Enhancement") {
    tier = "Awakening";
  }
  context.body.cultivation.tier = tier;
  context.body.cultivation.progress_xp = clampNumber(
    Math.floor(finiteNumberOrDefault(context.body.cultivation.progress_xp, 0)),
    0,
    cultivationAwakeningToEnhancementXp
  );
}

function cultivationActivitySummary(event: any, beforeTier: string, beforeXp: number, cultivation: any, promoted: boolean): string {
  var summary = "Cultivation gained " + event.amount_xp + " XP from " + event.source + ".";
  if (promoted) {
    summary += " Tier advanced from " + beforeTier + " to Enhancement.";
  } else {
    summary += " Progress moved from " + beforeXp + " to " + cultivation.progress_xp + " XP in " + cultivation.tier + ".";
  }
  if (event.note) {
    summary += " " + event.note;
  }
  return summary;
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
    value === "cultivation" ||
    value === "memory_sync" ||
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
  var memories = context.body.memory || [];
  for (var i = 0; i < memories.length; i++) {
    var existing = memories[i];
    if (existing.kind === memory.kind && lowercase(trimString(existing.summary)) === lowercase(memory.summary)) {
      if (memory.importance > existing.importance) {
        existing.importance = memory.importance;
      }
      context.body.memory = sortAndBoundMemories(memories);
      return;
    }
  }

  memories.push(memory);
  context.body.memory = sortAndBoundMemories(memories);
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
      "reach Enhancement",
      "build trusted relationships with NPCs"
    ]),
    player_notes: trimString(soul.player_notes) || "prototype default soul",
    reincarnation_lore: trimString(soul.reincarnation_lore) || "a synthetic body carrying a persistent consciousness imprint"
  };
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

function normalizeEquipment(equipment: any): any {
  var equipmentVisualId = clampNumber(equipment.equipment_visual_id || 0, 0, 9);
  return {
    primary_weapon: trimString(equipment.primary_weapon) || primaryWeaponName(equipmentVisualId),
    equipment_visual_id: equipmentVisualId
  };
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
