# Prototype validation

Checked on Apple silicon macOS, November 5, 2026, using Unity 6000.6.7f1, URP 17.6.0, Unity CLI 1.0.0-beta.17, and Unity Pipeline 0.10.0-exp.1. All editor operations, input checks, captures, and builds used the Unity CLI.

## Round 1 feedback and fixes

- Road geometry: the regression sampled triangle interiors and found road sections 0.0484 m below the terrain. Road triangles now split at each terrain slope boundary. Minimum sampled clearance is 0.0450 m throughout the mesh. Native resize, close zoom, and 135-degree orbit captures show a continuous road without the exposed terrain patches.
- HUD overlap: the original fixed widths made counters overlap Pause and wave text overlap Start Wave at narrower widths. Header and footer content now occupy separate responsive regions. Checks pass at logical canvas widths 1100, 1200, 1386, 1600, and 2000, with readable counters and briefings.
- Camera: middle-drag remains available. Command+left-drag rotates on macOS; Ctrl+left-drag is the Windows binding. Queued macOS Input System events confirm rotation, preservation of build selection, and no purchase when dragging over an empty pad. Windows runtime input remains unverified.
- Tank spacing: model bounds are 1.29 m wide and 2.00 m long. The old minimum centre separation was 0.73 m. Vehicle intervals now use 5.5 seconds, with a 4.8 m route-spacing guard across section boundaries. The guard delays the column rather than releasing a catch-up burst. Minimum measured centre separation is 2.77 m through the full route, including a compressed-spawn stress case. A conservative bounding-circle test also covers rotation at bends.
- Funds: completed waves award 70, 80, 90, 100, and 110 funds. The first bonus adds 20 funds to the previous baseline and makes a cannon affordable after a clean first wave. The HUD reports the completed round bonus.
- Difficulty: stronger armor, faster infantry, more later squads, and longer vehicle columns form the single baseline. Easier and harder modes remain future work.

## Combat and balance

Waves contain 18, 30, 40, 56, and 72 units (216 total), including 0, 0, 4, 8, and 12 tanks. Six-soldier squads retain individual health, targeting, rewards, and escape accounting. Infantry have 80 health and move at 1.5 m/s. Tanks have 650 health and move at 1.05 m/s. Per-type health stays constant across waves.

Focused checks pass for squad totals and formation, armor damage and target priority, mortar cluster selection, actual single-target versus six-target splash hits, economy and round bonuses, invalid/occupied purchases, sale rounding, pause, wave gates, terminal freeze, rating boundaries, final-escape defeat priority, stable ties, and exactly-once rewards.

`CheckBalance.All` uses actual purchases and earned funds. Strong strategies can reinvest during combat. Mixed layouts fill all ten pads before wave 5. Each must send enemies beyond 75% of the route and fire the rear defenses at least five times.

| Strategy | Outcome | Lives | Kills / escapes | Combat time | Final-wave route depth | Rear shots in wave 5 |
| --- | --- | --- | --- | --- | --- | --- |
| Mixed bends | Victory | 20 | 216 / 0 | 315 s | 92% | 87 |
| Mixed entrance and middle | Victory | 19 | 215 / 1 | 296 s | 100% | 106 |
| Five MG, three cannons, two mortars | Victory | 3 | 199 / 17 | 314 s | 86% | 28 |
| Machine guns only | Defeat, wave 5 | 0 | 188 / 20 | 310 s | 100% | 177 |
| Lone opening machine gun | Defeat, wave 2 | 0 | 8 / 20 | 84 s | Not reached | 0 |
| Initial defenses, never reinvest | Defeat, wave 3 | 0 | 65 / 20 | 138 s | Not reached | 0 |

Times exclude player-controlled preparation. Defeat stops immediately, so losing runs can leave unresolved units. Human playtest round 2 remains pending; these automated checks do not judge whether the revised pacing feels fun.

## Input, resizing, and live checks

`CheckInput.All` injects temporary mouse/keyboard events across real Play Mode frames. It passes placement/inspection/selling, rejected purchases, same-frame shop-to-pad placement, cancellation, Escape priority, wheel limits, middle-drag, Command-drag, pan, reset, camera buttons, and placement after rotation/zoom. Temporary devices, background behavior, and editor-only input routing are restored in `finally`.

The editor's default focus routing initially discarded virtual mouse events. The harness now temporarily routes device input to the Game view. Product input configuration remains unchanged.

Standalone `CheckResize.cs` passes actual native window sizes 1600×900, 1200×1000, 1024×768, and 1920×1080. Each size also checks a 5.5 close zoom and 135-degree orbit. The narrow HUD and close road captures were visually inspected.

Standalone UI raycasts/callbacks and lifecycle checks pass for menus, shops, camera controls, wave start, pause/resume, restart, cancellation priority, freeze, and Main Menu cleanup. Physical OS mouse/window focus is outside the injected-device check. No computer-use automation was used.

## Standalone match

The complete standalone Mac match passed at normal speed in **315 seconds**, with **18,837 rendered frames**, **3 stars / 20 lives / 216 kills / 0 escapes**, and **zero runtime error entries**. Final-wave enemies reached **92.1% of the route** and triggered **85 rear-defense shots**. The real-time controller used only actual purchases and earned funds, including reinforcements during combat. No health, damage, income, or wave cheats were used. Current combat and victory captures are retained.

## Builds and platform scope

The updated Mac release is open for round 2 and logs no startup exceptions or script compilation errors. The QA player and the editor opened for this task are closed. Automation overrides are restored, and generated font atlas changes are discarded.

The macOS ARM64 release, Windows x86-64 Mono cross-build, and Mac QA build succeed with zero warnings and errors. Development-only Pipeline instrumentation remains excluded from release builds. Outputs and ZIPs remain ignored under `TheLineHolds/Builds/`.

Windows runtime verification is deferred by the user. Use a Windows VM when ready, following `Tools/Validation/windows-checklist.md`. No Windows runtime test or VM setup was performed in this revision.

See `README.md` and `Tools/Validation/README.md` for reproduction commands. Compact results are in `Tools/Validation/last-run.json`. Representative captures are under `TheLineHolds/Screenshots/`.

The installed Pipeline documentation consulted includes `commands/scripts.md`, `commands/build-and-compilation.md`, `commands/editor-lifecycle-and-observability.md`, `commands/capture.md`, `commands/runtime.md`, and `runtime-setup.md`.

## Checkpoints and roadmap

- `a12aa99`: camera shortcut and purchase protection, responsive HUD regions, road surface repair, and regression checks.
- `92b5610`: round funds, harder baseline, tank spacing, full-map pressure checks, and playthrough updates.

Roadmap step 1 remains in progress until the user's next hands-on round. Steps 2–5 remain pending. Earlier baselines and their validation remain in Git history.
