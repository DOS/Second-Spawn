# Install Photon Fusion 2 SDK (JOY manual step)

The Fusion 2 SDK ships as a `.unitypackage` from photonengine.com that requires a Photon Account login to download. Cannot be installed via Unity Package Manager (no Git URL, no OpenUPM mirror, per Photon docs).

Once installed, the AI agent (Claude Code via Coplay Unity MCP) can take over and wire up the networking code per [docs/design/05-networking-architecture.md](../design/05-networking-architecture.md) and [docs/adr/0006-fusion-2-scratch-over-template.md](../adr/0006-fusion-2-scratch-over-template.md).

## Prerequisite

JOY needs a Photon Account. Free for development. Sign up at:

<https://dashboard.photonengine.com/account/signup>

## Step 1: Get a Photon Fusion 2 App ID

1. Log in to <https://dashboard.photonengine.com>
2. Click "Create a new app"
3. Type: **Fusion**
4. Name: `Second Spawn Dev` (we will create a separate `Second Spawn Prod` later)
5. Region: closest to JOY's workstation (HCMC -> Asia/Singapore or Tokyo)
6. After creation, copy the **App ID** (32-char hex)

The free Photon Cloud tier supports 20 CCU concurrent - more than enough for vertical slice development. Production tier decision is deferred (open decision in CLAUDE.md / AGENTS.md).

## Step 2: Download the SDK

1. While logged in, go to <https://doc.photonengine.com/fusion/current/getting-started/sdk-download>
2. Download the **Fusion 2 SDK** `.unitypackage`. Latest stable at time of writing: see Photon release notes for version.
3. Save to `D:\Downloads\` or wherever convenient.

## Step 3: Import into Unity

1. Open Unity Editor at `D:\Projects\Second-Spawn\Unity`.
2. Menu: **Assets > Import Package > Custom Package...**
3. Select the downloaded `Fusion 2 SDK.unitypackage`.
4. In the import dialog, ensure everything is checked (default).
5. Click **Import**.
6. Wait for Unity to compile (~30-90 seconds).

Fusion ships several Plugins/ subfolders + a `Photon/` runtime folder. These contain compiled DLLs and a small amount of Photon's wrapper source. Per ADR 0006, this is OK to commit (it's the SDK, not a template sample); we just do not import the BR200 / Karts / Tanknarok template packages.

## Step 4: Configure the App ID

1. In Unity Project view, navigate to `Assets/Photon/Fusion/Resources/PhotonAppSettings.asset` (path Photon creates).
2. Paste the App ID from Step 1 into the field labeled **App Id Fusion**.
3. Save (Ctrl+S).
4. Also paste the same App ID into `Assets/_SecondSpawn/Settings/SecondSpawnConfig.asset` -> `PhotonAppId` field (once that .asset exists - see "Create SecondSpawnConfig asset" below).

## Step 5: Install Fusion Simple KCC

Fusion Simple KCC is the first controller addon for the player movement spike.

1. Download `fusion-simple-kcc-2.0.15.unitypackage` from Photon.
2. Import it into the Unity project.
3. Expected installed path: `Assets/Photon/FusionAddons/SimpleKCC/`.
4. Confirm `simple_kcc_build_info.txt` reports version `2.0.15`.

This addon is OK to commit with the Photon SDK. It is not a gameplay template sample.

## Step 6: Create the SecondSpawnConfig asset

The ScriptableObject definition was scaffolded in commit `f04aa3b` but the `.asset` instance requires Unity Editor focus to compile the new `SecondSpawn.Settings` assembly. If it still does not exist:

1. Focus Unity Editor and wait for recompile (look for spinner in bottom-right).
2. After compile, menu: **Assets > Create > Second Spawn > Project Config**.
3. Save the new asset at `Assets/_SecondSpawn/Settings/SecondSpawnConfig.asset`.
4. Set the fields in Inspector:
   - **Environment**: Development
   - **GatewayBaseUrl**: `http://localhost:8080` (for now)
   - **SupabaseUrl**: from Step 7 in NEXT_STEPS.md (once Supabase project exists)
   - **SupabaseAnonKey**: same
   - **PhotonAppId**: from Step 1
   - **DosChainRpcUrl**: leave blank for now

## Step 7: Verify it builds

1. Menu: **File > Build Settings**.
2. Target: PC Standalone (Windows or Linux).
3. Click **Build**, save to `D:\Projects\Second-Spawn\Unity\Build\` (gitignored).
4. If the build succeeds with no compile errors, Fusion 2 is installed correctly.

## Step 8: Tell the AI agent

Open a Claude Code session in this repo:

```powershell
cd D:\Projects\Second-Spawn
claude
```

Then tell it: "Fusion 2 SDK installed, App ID configured. Wire up the NetworkRunnerSetup per docs/design/05-networking-architecture.md."

The AI agent will:

1. Verify Fusion assemblies are loaded (`Fusion.Runtime`, `Fusion.Common`, etc).
2. Replace the scaffold code in `Assets/_SecondSpawn/Scripts/Networking/*.cs` with real `NetworkBehaviour` / `[Networked]` property implementations.
3. Hook up `NetworkRunnerSetup` to start a Host Mode session in dev.
4. Create a simple test: spawn a `NetworkPlayer` cube on player join, replicate position at 60Hz.
5. Commit + push.

## Troubleshooting

### "Photon App ID is empty"

You forgot Step 4. Open `Assets/Photon/Fusion/Resources/PhotonAppSettings.asset` and paste the App ID.

### "Cannot connect to Photon Cloud"

- Check network / firewall (Photon uses ports 5055 UDP, 5056 UDP, 443 TCP).
- Verify the App ID matches the one in the dashboard.
- Try the region from Step 1 explicitly (e.g. set `Fixed Region` to `asia` or `jp` in PhotonAppSettings).

### "20 CCU limit reached"

The free Photon Cloud tier caps at 20 concurrent users across all sessions. For local dev with bots + 2-3 players this is plenty. When the vertical slice playtest needs more, upgrade to a paid tier (decision deferred per CLAUDE.md Open Decision Points).

### "BR200 / Tanknarok / Karts import"

DO NOT import these template packages into the Second Spawn project. They are Photon Asset Store samples licensed differently from the Fusion SDK itself, and per ADR 0006 we extract patterns from them in a separate scratch project, not in this repo.

To read the samples for pattern extraction:

```powershell
cd D:\Projects
mkdir fusion-samples-scratch
cd fusion-samples-scratch
# Create a separate Unity project, import the sample, read the code.
# Never copy code from there into Second-Spawn.
```
