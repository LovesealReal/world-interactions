# World Interactions — VPM Listing

This repository hosts the **World Interactions** package and a [VPM](https://vcc.docs.vrchat.com/vpm/) listing so it can be installed and updated through the VRChat Creator Companion (VCC) or ALCOM.

World Interactions lets avatars talk to your VRChat world through contact broadcasts: drop one prefab into your world and, at build time, it generates world-origin contact receivers — built-in **Slow** and **Rumble** effects (with presence-beacon wearers like staff automatically exempt), plus simple hooks to run your own UdonSharp code from any custom contact tag. The full avatar/world convention is documented in [PROTOCOL.md](Packages/com.loveseal.world-interactions/PROTOCOL.md), so any avatar system can implement it.

## Packages

| Package | What it is |
|---|---|
| [`com.loveseal.world-interactions`](Packages/com.loveseal.world-interactions/) | The core system. Community-neutral: standard `WI/*` tags out of the box, everything configurable. |
| [`com.clubmaul.world-interactions`](Packages/com.clubmaul.world-interactions/) | Thin [Club Maul](https://github.com/Club-Maul) preset: depends on the core and ships a prefab pre-configured for the [Staff Scanner V2](https://github.com/Club-Maul/staff-scanner-v2) ecosystem (`ClubMaul/*` tags, staff exemption). |

## Install via VCC / ALCOM

1. Add this listing to your VCC:

   **[➕ Add to VCC](https://LovesealReal.github.io/world-interactions/)** — or paste the listing URL manually:

   ```
   https://LovesealReal.github.io/world-interactions/index.json
   ```

2. Open your world project, go to **Manage Project**, and add **World Interactions** (or **Club Maul World Interactions** for the Club Maul preset).
3. See the [package README](Packages/com.loveseal.world-interactions/README.md) for setup and usage.

## Requirements

- VRChat Worlds SDK 3.7.0+ (includes UdonSharp)

## Repository layout

- `Packages/com.loveseal.world-interactions/` — the core package.
- `Packages/com.clubmaul.world-interactions/` — the Club Maul preset package (staged here; intended to move to the Club-Maul org with its own listing).
- `.github/workflows/` — automation that builds the release `.zip`/`.unitypackage` and publishes the VPM listing to GitHub Pages.
- `Website/` — source for the listing page served at the URL above.

## Releasing a new version

1. Bump `version` in [`Packages/com.loveseal.world-interactions/package.json`](Packages/com.loveseal.world-interactions/package.json) and add a `CHANGELOG.md` entry.
2. Commit and push to `main`.
3. Run the **Build Release** workflow (Actions tab → Build Release → Run workflow). It tags the version, builds the artifacts, and publishes a GitHub Release.
4. The **Build Repo Listing** workflow then regenerates the listing and redeploys the Pages site automatically.

> **One-time setup:** create the repository variable `PACKAGE_NAME` = `com.loveseal.world-interactions` (Settings → Secrets and variables → Actions → Variables) and enable GitHub Pages with the "GitHub Actions" source. The workflows release the core package only; the Club Maul preset gets its own repo/listing under the Club-Maul org.

---

Built from VRChat's [template-package](https://github.com/vrchat-community/template-package).
