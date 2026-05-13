# Next Steps for SECOND SPAWN

Sequential setup tasks after repo init. Run in order.

## 1. Git LFS init

```powershell
cd D:\Projects\Second-Spawn
git lfs install
git lfs track  # verify .gitattributes patterns are picked up
```

## 2. License (decided)

- **Code:** AGPL-3.0 (`LICENSE`)
- **Assets:** CC-BY-NC 4.0 (`LICENSE-ASSETS`)
- **NFT assets:** Reserved by DOS.AI ecosystem

Rationale: AGPL-3.0 is copyleft - any commercial fork must release source under AGPL too. Multiplayer game running as network service triggers AGPL on any fork attempt. Combined with CC-BY-NC on assets, this blocks competitors from creating a "Second Spawn clone" commercially without contributing back to the project.

## 3. Folder skeleton

Create empty folders so the multi-stack repo is consistent. Unity project lives under `unity/` (not at repo root) to keep separation from `backend/`, `docs/`, etc. Run:

```powershell
cd D:\Projects\Second-Spawn
New-Item -ItemType Directory -Force -Path unity, unity\Assets, unity\Assets\Scripts, unity\Assets\Scripts\Gameplay, unity\Assets\Scripts\Networking, unity\Assets\Scripts\AI, unity\Assets\Scripts\UI, unity\Assets\Scripts\NFT, unity\Assets\Art, unity\Assets\Audio, unity\Assets\Prefabs, unity\Assets\Scenes, unity\Assets\Settings, unity\Packages, unity\ProjectSettings, docs\design, backend\gateway, .claude\commands, .claude\templates | Out-Null
```

## 4. Unity project init

1. Open Unity Hub
2. New Project -> 3D (URP) template -> Unity 6 LTS
3. Project location: `D:\Projects\Second-Spawn\unity` (the `unity/` subfolder, NOT the repo root)
4. Project name: `Second-Spawn` (Unity will create `unity/Assets/`, `unity/Packages/`, `unity/ProjectSettings/`, `unity/Library/`)
5. After creation, verify `Edit > Project Settings > Editor > Asset Serialization Mode = Force Text` (default in Unity 6, do NOT change)

## 5. Photon Fusion 2

1. Sign in to https://dashboard.photonengine.com
2. Create app -> Fusion -> get App ID
3. Install Fusion 2 SDK via Unity Package Manager (Git URL from Photon docs)
4. Configure Photon App ID in `unity/Assets/Photon/Fusion/Resources/PhotonAppSettings.asset`
5. Read MetaDOS Fusion setup at `D:\Projects\MetaDOS` (read-only) to extract NetworkRunner pattern

## 6. Coplay unity-mcp (Claude Code <-> Unity Editor bridge)

Follow https://docs.coplay.dev/coplay-mcp/claude-code-guide

Summary:

1. Install MCP for Unity package in Unity project
2. Configure Claude Code MCP server (stdio mode recommended)
3. Restart Claude Code session at `D:\Projects\Second-Spawn`
4. Verify connection: Claude can list scenes, read scripts via MCP

## 7. Supabase project

1. Create Supabase project (reuse DOS.Me org)
2. Get URL + anon key + service role key
3. Service role key goes to backend `.env`, anon key OK in Unity `unity/Assets/Settings/SupabaseConfig.asset` (anon is public-safe)
4. Set up Auth providers (email + DOS Chain wallet sign-in)

## 8. GitLab workstation mirror

On workstation (assumed GitLab self-hosted already running):

1. Create empty repo `second-spawn` in GitLab
2. Configure pull mirroring from `https://github.com/DOS/Second-Spawn.git` every 15 min
3. Verify mirror sync after 30 min

## 9. Backblaze B2 cold backup

1. Create B2 bucket `second-spawn-backup`
2. Install rclone on workstation
3. Configure rclone remote for B2
4. Add nightly task scheduler: `rclone sync <github-lfs-cache> b2:second-spawn-backup/lfs`

## 10. Cherry-pick Claude-Code-Game-Studios

```powershell
git clone https://github.com/Donchitos/Claude-Code-Game-Studios.git D:\Projects\references\Claude-Code-Game-Studios
```

Copy into `D:\Projects\Second-Spawn\.claude\`:

- `templates/` (all 41 doc templates)
- `commands/` selected:
  - `brainstorm.md`
  - `create-architecture.md`
  - `architecture-decision.md`
  - `architecture-review.md`
  - `create-epics.md`
  - `create-stories.md`
  - `dev-story.md`
  - `sprint-plan.md`
  - `perf-profile.md`
  - `scope-check.md`
  - `balance-check.md`
  - `security-audit.md`
  - `code-review.md`
  - `changelog.md`
  - `patch-notes.md`
  - `bug-report.md`
  - `playtest-report.md`

Skip: all `team-*` commands, Director / Lead tier agents, complex hooks.

## 11. First commit + push

```powershell
cd D:\Projects\Second-Spawn
git add .gitattributes .gitignore README.md CONTRIBUTING.md LICENSE LICENSE-ASSETS ROADMAP.md docs .claude
git commit -m "chore: initial repo setup with Unity 6 LFS conventions and architecture docs"
git push origin main
```

## 12. Verify

- [ ] Repo public on GitHub: https://github.com/DOS/Second-Spawn
- [ ] LFS files tracked properly (`git lfs ls-files` after adding first binary)
- [ ] Unity project opens cleanly
- [ ] Photon connects
- [ ] Supabase auth flow tested
- [ ] Coplay MCP bridge active in Claude Code
- [ ] GitLab mirror syncs from GitHub
- [ ] B2 nightly backup configured
