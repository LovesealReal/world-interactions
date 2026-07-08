# World Interactions — VPM Listing

This repository hosts the **World Interactions** package for [Club Maul](https://github.com/Club-Maul) and a [VPM](https://vcc.docs.vrchat.com/vpm/) listing so it can be installed and updated through the VRChat Creator Companion (VCC) or ALCOM.

World Interactions is the **world-side counterpart** to the [Staff Scanner V2](https://github.com/Club-Maul/staff-scanner-v2) avatar package: drop one prefab into your VRChat world and, at build time, it generates world-origin contact receivers that react to the scanner's broadcasts — built-in **Slow** and **Rumble** effects (with staff scanner wearers automatically exempt), plus simple hooks to run your own UdonSharp code from any custom contact tag.

## Install via VCC / ALCOM

1. Add this listing to your VCC:

   **[➕ Add to VCC](https://LovesealReal.github.io/world-interactions/)** — or paste the listing URL manually:

   ```
   https://LovesealReal.github.io/world-interactions/index.json
   ```

2. Open your world project, go to **Manage Project**, and add **World Interactions**.
3. See the [package README](Packages/com.clubmaul.world-interactions/README.md) for setup and usage.

## Requirements

- VRChat Worlds SDK 3.7.0+ (includes UdonSharp)

## Repository layout

- `Packages/com.clubmaul.world-interactions/` — the package itself.
- `.github/workflows/` — automation that builds the release `.zip`/`.unitypackage` and publishes the VPM listing to GitHub Pages.
- `Website/` — source for the listing page served at the URL above.

## Releasing a new version

1. Bump `version` in [`Packages/com.clubmaul.world-interactions/package.json`](Packages/com.clubmaul.world-interactions/package.json) and add a `CHANGELOG.md` entry.
2. Commit and push to `main`.
3. Run the **Build Release** workflow (Actions tab → Build Release → Run workflow). It tags the version, builds the artifacts, and publishes a GitHub Release.
4. The **Build Repo Listing** workflow then regenerates the listing and redeploys the Pages site automatically.

> **One-time setup:** create the repository variable `PACKAGE_NAME` = `com.clubmaul.world-interactions` (Settings → Secrets and variables → Actions → Variables) and enable GitHub Pages with the "GitHub Actions" source.

---

Built from VRChat's [template-package](https://github.com/vrchat-community/template-package).
