# Prototype validation

Checked on Apple silicon macOS, November 4, 2026, using Unity 6000.6.7f1, URP 17.6.0, Unity CLI 1.0.0-beta.16, and Unity Pipeline 0.10.0-exp.1. All editor operations, input checks, captures, and builds used the Unity CLI.

## Combat and balance

The opening-gun regression reproduced the reported issue on the previous baseline: 8 kills, 0 escapes, peak concurrency 1, and only 1.6 m travelled along a 50.9 m route. With the revised authored data, that gun kills 4 infantry and lets 14 escape; peak concurrency is 15.

- Waves contain 18, 24, 28, 43, and 58 units (171 total), including 0, 0, 4, 7, and 10 tanks. Six-soldier squads enter in two lanes with separate health, targeting, rewards, and escape accounting.
- Machine guns hit individual infantry and deal 15% damage to armor. Anti-tank guns prioritize armor and deal 60% damage to infantry. Mortars lead moving clusters, hit multiple soldiers, and deal 35% damage to armor. No hidden per-wave health scaling is used.
- Focused checks pass for squad spawn totals and compact formation, armor damage and target priority, mortar cluster selection, and actual single-target versus six-target splash hits.
- Existing economy, invalid/occupied purchases, sale rounding, pause freeze, wave gates, terminal freeze, all rating boundaries, final-escape defeat priority, stable tie-breaking, and exactly-once kill/escape/wave accounting checks pass.

`CheckBalance.All` runs these strategies at fixed simulation steps with actual purchases and earned funds:

| Strategy | Outcome | Lives | Kills / escapes | Combat time |
| --- | --- | --- | --- | --- |
| Mixed coverage at the bends | Victory | 18 | 169 / 2 | 283 s |
| Mixed entrance and middle coverage | Victory | 20 | 171 / 0 | 243 s |
| Machine guns only | Defeat, wave 5 | 0 | 150 / 20 | 336 s |
| Lone opening machine gun | Defeat, wave 2 | 0 | 9 / 20 | 94 s |
| Initial three defenses, never reinvest | Defeat, wave 5 | 0 | 135 / 20 | 314 s |

Victory times exclude player-controlled preparation. Defeat stops immediately, so unresolved units remain in the losing runs. These are regression bounds for useful choices, not a substitute for further human balance feedback.

## Camera, input, and visual checks

`CheckInput.All` injects temporary Input System mouse/keyboard events across real Play Mode frames, without calling gameplay or button callbacks. It passes placement/inspection/selling, rejected purchases, same-frame shop-to-pad placement, right-click cancellation, Escape priority, close zoom limits (5.5–18), Q/E and middle-drag orbit, Shift+middle-drag and WASD pan, R/button reset, on-screen rotation and zoom, UI scroll isolation, and placement after rotating/zooming. Temporary devices/settings are restored in `finally`.

World labels follow camera orbit and health bars face the view. The overview and a 135-degree rotated close view were visually inspected. A wrapped reset-button label was corrected. Live formation capture showed 11 concurrent individual infantry and advancing frames.

Standalone UI raycasts and callbacks pass for Deploy, shop, camera rotation/zoom/reset, Start Wave, Pause, and Resume. Standalone lifecycle checks pass for cancellation priority, pause freeze/resume, retry reset, and Main Menu cleanup.

Physical OS mouse/window focus is outside the injected-device check. No computer-use automation was used.

## Standalone match

A complete five-wave standalone Mac match at normal speed finished in **283 seconds**, with **16,883 rendered frames**, **3 stars / 18 lives / 169 kills / 2 escapes**. The two escapes were tanks in wave 3. The final mixed assault was held after reinvesting earned funds; no funds, damage, health, or wave cheats were used. Runtime error entries: **0**. The current combat and victory captures are saved under `TheLineHolds/Screenshots/`.

## Builds and platform scope

The Mac release was launched through the CLI, remains open for playtesting, and logs no startup exceptions or script compilation errors. The QA player and the editor opened for this task were closed; editor automation overrides were restored. Generated font atlas cache changes were discarded.

The refreshed macOS ARM64 release, Windows x86-64 Mono cross-build, and macOS QA build all succeed with zero build warnings and zero errors. Development-only Pipeline instrumentation is excluded from release builds. Build outputs and distributable ZIPs remain ignored under `TheLineHolds/Builds/`.

Windows runtime verification remains deferred by the user. Use a Windows VM when ready, following `Tools/Validation/windows-checklist.md`, before treating Windows as verified. No VM setup or Windows runtime test was performed in this revision.

See `README.md` and `Tools/Validation/README.md` for reproduction commands. Machine-readable results are in `Tools/Validation/last-run.json`. Representative gameplay images are under `TheLineHolds/Screenshots/`.

The installed Pipeline documentation consulted includes `commands/scripts.md`, `commands/build-and-compilation.md`, `commands/editor-lifecycle-and-observability.md`, `commands/capture.md`, `commands/runtime.md`, and `runtime-setup.md`.

## Checkpoints

- `b011b67`: squad combat, tower roles, economy, authored pressure, and balance regression checks.
- `c5d943e`: map orbit, close zoom, pan, view reset, readable labels, and camera input checks.

The November 3 baseline and its validation remain available in Git history. The current evidence above supersedes its sparse-wave balance results.
