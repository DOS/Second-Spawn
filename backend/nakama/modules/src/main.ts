const rpcIdHealth = "secondspawn_health";

let InitModule: nkruntime.InitModule = function (
  ctx: nkruntime.Context,
  logger: nkruntime.Logger,
  nk: nkruntime.Nakama,
  initializer: nkruntime.Initializer
) {
  initializer.registerRpc(rpcIdHealth, rpcHealth);
  logger.info("Second Spawn Nakama runtime loaded.");
};

const rpcHealth: nkruntime.RpcFunction = function (
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
};

InitModule.bind(null);
