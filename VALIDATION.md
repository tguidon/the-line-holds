# Prototype validation

Checked on Apple silicon macOS, November 3, 2026, using Unity 6000.6.7f1, URP 17.6.0, Blender 5.2.3, and Unity Pipeline 0.10.0-exp.1. Editor operations, captures, build commands, and Player verification used the Unity CLI. No computer-use automation is required to reproduce these checks.

## Passed

- Compilation: runtime and Editor scripts compile without errors.
- Actual authored wave counts: 8, 12, 10, 19, 25 units; armor counts 0, 0, 2, 3, 5; 74 total units, with unchanged per-type health across waves.
- Economy: starting 300 funds / 20 lives; occupied and unaffordable placement rejection; sale at floor(50%); duplicate-sale rejection; exactly-once kill rewards and wave rewards; escapes grant no funds.
- Combat: stable furthest-along targeting with ID tie-breaks; splash damage; overlapping lethal projectiles do not award duplicate kills; resolved targets are safe.
- Waves/results: no overlap; all spawns and active enemies must resolve; defeat on zero lives, including the last final-wave escape; terminal states stop advancing; all star thresholds checked.
- Balance: two different sensible layouts clear all five waves with 20 lives. Combat durations are **317 seconds** and **286 seconds**, plus player-controlled preparation time. No-defense play loses at zero lives.
- Standalone macOS, normal speed: a complete five-wave match finished in **317 seconds**, **18,950 rendered frames**, with **3 stars / 20 lives / 74 kills / 0 escapes**. This was followed by an accelerated regression replay on the final separated systems and rating UI.
- Standalone lifecycle: Escape cancellation before pause; movement/spawning/projectiles freeze across real frames; resume advances; retry resets counters, funds, lives, towers, enemies, projectiles; Main Menu clears match objects.
- Standalone UI: screen raycasts reach menu, shop, Start Wave, Pause, and Resume buttons; their pointer-click callbacks perform the expected actions.
- Frame-driven Input System controls in Play Mode: temporary mouse/keyboard devices sent events across actual frames, without calling UI or gameplay callbacks. Verified Play, shop selection, placement, occupied-pad inspection, sale, rejected purchases, right-click cancellation, Escape priority, wheel zoom and both limits, wave start, pause/resume, pause Restart, and Main Menu. Temporary devices/settings were restored afterward. OS focus behavior remains outside this injected-device check.
- Rapid placement regression: moving from a shop button to a build pad and pressing the mouse button in the same input frame places the tower correctly.
- macOS release startup: the release player launches through the documented Unity CLI command, remains running, and logs no startup exceptions or script compilation errors. Full-match evidence is recorded separately above.
- Visual checks: fixed gameplay camera, terrain tiers and ramps, route visibility, pad labels, HUD, range previews, miniature models, and font-independent rating stars. Final representative captures are under `TheLineHolds/Screenshots/`.
- Original Blender source and repeatable exports retained. Font license retained in the imported TMP resources. Git LFS configured before any binary asset commit; caches and builds ignored.

## Builds and platform scope

The final macOS (ARM64) and Windows (x86-64 Mono) release builds are generated from the same Unity project, in separate output folders. Development-only Pipeline instrumentation is excluded from release builds. The macOS QA build retains local authenticated Pipeline access for reproducible checks.

**Windows runtime verification is deferred by the user and does not block prototype completion.** The current evaluation platform is macOS. The Windows cross-build succeeds, but Windows runtime behavior remains unverified. Run `Tools/Validation/windows-checklist.md` when Windows becomes a release goal, before a Windows release or claiming Windows is verified. No Windows machine or runner was connected during this implementation.

The user selected a Windows VM for future testing, when ready. VM setup is a later task. A future result should name the Windows architecture, VM software, virtual GPU, and any x86-64 emulation rather than implying coverage of all Windows PCs.

Build outputs and distributable ZIPs are under `TheLineHolds/Builds/`; those generated folders are intentionally ignored by Git. Compact machine-readable results are recorded in `Tools/Validation/last-run.json`.

## Reproduction and limits

See `README.md` and `Tools/Validation/README.md` for exact build/test commands. The automated playthrough uses the actual frame-driven simulation and actual purchases; it never awards extra funds, skips waves, scales health, or changes damage. The regression replay accelerates Unity time only. Balance remains a prototype baseline and still benefits from human playtesting.

An unfocused standalone Player may discard CLI-synthesized mouse events through Unity input focus handling. UI wiring was checked through EventSystem raycasts and pointer callbacks, and the full Input System path was then checked using temporary devices and queued events across real frames (`CheckInput.cs`). Physical mouse/window focus behavior remains part of the human platform checklist.

The initial runtime log included warnings about stripped, unused URP depth-of-field/Panini shaders. The prototype uses ACES tonemapping, subtle bloom, and vignette; it does not enable those unused effects. No gameplay exceptions occurred. A missing star-font glyph discovered during validation was replaced with uGUI geometry.

The Pipeline documentation consulted matches the installed package: `connectivity.md`, `commands/scripts.md`, `commands/build-and-compilation.md`, `commands/editor-lifecycle-and-observability.md`, `commands/capture.md`, `commands/runtime.md`, and `runtime-setup.md`.

## Workspace state

The project is left in Edit Mode with `Countryside.unity` ready to play. Temporary QA players are closed and auto-tick/background overrides are restored. The macOS release player is left open for the user's playtest. The baseline is recorded in logical Git checkpoints: `b23b134` documents the scope and checkpoint workflow, and `d55d9ec` contains the playable prototype and asset pipeline. Validation and handoff files are committed separately. The pre-task repository commit is `7b4ae17`. The prototype spec's platform acceptance was clarified at the user's request: macOS is the current evaluation platform, while Windows VM testing is deferred until the user is ready. This clarification changes no game code or build artifacts.
