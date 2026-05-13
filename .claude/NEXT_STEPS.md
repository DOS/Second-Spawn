# Next Steps for SECOND SPAWN

Sequential setup tasks. Run in order. Strike-through means done at the date noted.

> Status legend: ✅ done, ⏳ in progress, 🔜 next, ⏸ blocked / waiting on JOY action.

## 1. ✅ Git LFS init (done 2026-05-13)

```powershell
cd D:\Projects\Second-Spawn
git lfs install
git lfs track  # verify .gitattributes patterns are picked up
```

LFS pointer verified on first binary asset (`Unity/Assets/TutorialInfo/Icons/URP.png`) at commit `c9c8c35`.

## 2. ✅ License (done at repo init)

- **Code:** AGPL-3.0 (`LICENSE`)
- **Assets:** CC-BY-NC 4.0 (`LICENSE-ASSETS`)
- **NFT assets:** Reserved by DOS.AI ecosystem

Rationale: AGPL-3.0 copyleft. Multiplayer game running as network service triggers AGPL on any fork. Combined with CC-BY-NC on assets, blocks commercial "Second Spawn clone" without contributing back.

## 3. ✅ Folder skeleton (done 2026-05-13)

Multi-stack repo: Unity at `Unity/`, Go gateway at `backend/gateway/`, docs at `docs/`, agent context at `.claude/` + `AGENTS.md`. Empty Unity script subfolders (Scripts/Gameplay etc.) were wiped by Unity URP template install at Step 4 - they will be re-created via Unity MCP at Step 6.

## 4. ✅ Unity project init (done 2026-05-14)

Project at `D:\Projects\Second-Spawn\Unity`, name `Second Spawn` (with space, decoupled from repo folder name `Second-Spawn` per JOY decision).

- Unity version: `6000.5.0b7` (Unity 6.5 beta b7). JOY chose beta over Unity 6.0 LTS for newest features. See [docs/adr/0005-unity-6-5-beta.md](../docs/adr/0005-unity-6-5-beta.md).
- Asset Serialization Mode: Force Text (mode 2) - verified in `Unity/ProjectSettings/EditorSettings.asset`.
- Render pipeline: URP 17.5.0.

## 5. 🔜 Photon Fusion 2

Blocked on JOY having a Photon App ID.

1. Sign in to [https://dashboard.photonengine.com](https://dashboard.photonengine.com)
2. Create app -> Fusion -> get App ID
3. Install Fusion 2 SDK via Unity Package Manager (Git URL from Photon docs)
4. Configure Photon App ID in `Unity/Assets/Photon/Fusion/Resources/PhotonAppSettings.asset`
5. Read MetaDOS Fusion setup at `D:\Projects\MetaDOS` (read-only) to extract NetworkRunner pattern

Steps 3-5 can be automated via Unity MCP once active in the session (see Step 6).

## 6. ⏸ Coplay unity-mcp (Claude Code <-> Unity Editor bridge)

Setup is partially done by JOY:

- ✅ Coplay relay binary installed at `C:\Users\JOY\.unity\relay\relay_win.exe`
- ✅ Claude Code MCP server config in `~/.claude.json` references the relay
- ✅ JOY has started the Unity-side MCP Server (in Unity Editor)
- ⏸ **Restart Claude Code session** to load `unity-mcp` tools (the relay is registered but the running session pre-dates the bridge being live; tools register at session start, not on the fly)
- ⏳ After restart: verify with a probe command (e.g. list scenes / inspect SampleScene), then proceed with the autonomous Unity setup script in Step 6a below

### 6a. Unity MCP autonomous setup script (run in next session after restart)

Once `unity-mcp` tools appear:

1. Probe: list scenes, list assets root, confirm `SampleScene.unity` is the active scene.
2. Create assembly definition skeleton:
   - `Unity/Assets/Scripts/SecondSpawn.Gameplay.asmdef`
   - `Unity/Assets/Scripts/SecondSpawn.Networking.asmdef`
   - `Unity/Assets/Scripts/SecondSpawn.AI.asmdef` (offline AI agent runtime)
   - `Unity/Assets/Scripts/SecondSpawn.UI.asmdef`
   - `Unity/Assets/Scripts/SecondSpawn.NFT.asmdef`
   - Place 1 placeholder `MonoBehaviour` (or `static class`) per asmdef so Unity tracks the folder + GUID.
3. Create `Assets/Settings/SecondSpawnConfig.cs` ScriptableObject + the corresponding `.asset` instance with placeholder fields for `SupabaseURL`, `SupabaseAnonKey`, `GatewayBaseURL`.
4. Configure URP settings (single Universal Renderer, target 60Hz tick).
5. Remove URP template tutorial assets that aren't needed: `Assets/Readme.asset`, `Assets/TutorialInfo/`. (Keep `InputSystem_Actions.inputactions` - we will use Input System.)
6. Wait for JOY to provide Photon Fusion App ID before installing Fusion SDK (Step 5).
7. Commit each logical chunk separately so the Unity work is reviewable.

## 7. ⏸ Supabase project (waiting JOY to create)

1. Create Supabase project (reuse DOS.Me org)
2. Get URL + anon key + service role key
3. Service role key goes to `backend/gateway/.env` (gitignored), anon key OK in Unity `Unity/Assets/Settings/SecondSpawnConfig.asset` (anon is public-safe)
4. Set up Auth providers (email + DOS Chain wallet sign-in)

## 8. ⏸ GitLab workstation mirror (defer until repo activity warrants)

1. Create empty repo `second-spawn` in GitLab self-hosted
2. Configure pull mirroring from `https://github.com/DOS/Second-Spawn.git` every 15 min
3. Verify mirror sync after 30 min

## 9. ⏸ Backblaze B2 cold backup (defer until first non-trivial LFS volume)

1. Create B2 bucket `second-spawn-backup`
2. Install rclone on workstation
3. Configure rclone remote for B2
4. Add nightly task scheduler: `rclone sync <github-lfs-cache> b2:second-spawn-backup/lfs`

## 10. ✅ Cherry-pick Claude-Code-Game-Studios (done 2026-05-13)

Cloned to `D:\Projects\references\Claude-Code-Game-Studios`. Copied into `.claude/`:

- `templates/` (40 docs - 11 KEEP at top-level, 29 deferred to `_deferred/`. See `.claude/templates/_deferred/README.md` for promotion-back triggers.)
- `skills/` (17 skills - all kept; skills do not auto-load so no noise.)

## 11. ✅ First commit + push (done 2026-05-13, multiple subsequent)

Commits: `e6164f9` (initial setup), `290bf67` (Unity/ folder refactor), `04d7744` (PascalCase rename), `dd03b5a` (template defer), `b456daf` (design docs bootstrap), `c9c8c35` (Unity init), `840f951` (AGENTS.md sync). All on `main`.

## 12. Verify

- [x] Repo public on GitHub: <https://github.com/DOS/Second-Spawn>
- [x] LFS file tracked properly (1 file: `Unity/Assets/TutorialInfo/Icons/URP.png`)
- [x] Unity project opens cleanly (JOY confirmed at Step 4)
- [ ] Photon connects (Step 5)
- [ ] Supabase auth flow tested (Step 7)
- [ ] Coplay MCP bridge active in Claude Code (Step 6 - awaiting session restart)
- [ ] GitLab mirror syncs from GitHub (Step 8 - deferred)
- [ ] B2 nightly backup configured (Step 9 - deferred)
- [x] Backend Go gateway scaffold compiles + tests green (added 2026-05-14)
- [x] CI workflows for backend + markdown lint (added 2026-05-14)
- [x] Unity build CI staged behind workflow_dispatch (added 2026-05-14, needs `UNITY_LICENSE` secret to enable on push)
