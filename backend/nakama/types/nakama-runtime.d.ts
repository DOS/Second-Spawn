declare namespace nkruntime {
  interface Context {
    env: { [key: string]: string | undefined };
    userId?: string;
  }

  interface Logger {
    debug(message: string): void;
    error(message: string): void;
    info(message: string): void;
  }

  interface Nakama {
    httpRequest(
      url: string,
      method: string,
      headers: { [key: string]: string },
      body?: string
    ): HttpResponse;
    storageRead(requests: StorageReadRequest[]): StorageObject[];
    storageWrite(requests: StorageWriteRequest[]): void;
    uuidv4(): string;
  }

  interface HttpResponse {
    code: number;
    body: string;
  }

  interface Initializer {
    registerRpc(name: string, fn: RpcFunction): void;

    registerBeforeAuthenticateCustom(
      fn: BeforeHookFunction<AuthenticateCustomRequest>
    ): void;
  }

  type RpcFunction = (
    ctx: Context,
    logger: Logger,
    nk: Nakama,
    payload: string
  ) => string;

  type InitModule = (
    ctx: Context,
    logger: Logger,
    nk: Nakama,
    initializer: Initializer
  ) => void;

  type BeforeHookFunction<T> = (
    ctx: Context,
    logger: Logger,
    nk: Nakama,
    data: T
  ) => T | void | null;

  interface AuthenticateCustomRequest {
    account?: {
      id: string;
    };
    username?: string;
  }

  interface StorageReadRequest {
    collection: string;
    key: string;
    userId: string;
  }

  interface StorageWriteRequest {
    collection: string;
    key: string;
    userId: string;
    value: any;
    version?: string;
    permissionRead: number;
    permissionWrite: number;
  }

  interface StorageObject {
    collection: string;
    key: string;
    userId: string;
    value: any;
    version: string;
    permissionRead: number;
    permissionWrite: number;
  }
}
