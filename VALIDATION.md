# Prototype validation

Checked on Apple silicon macOS, November 7, 2026, with Unity 6000.6.7f1, URP 17.6.0, Unity CLI 1.0.0-beta.17, and Pipeline 0.10.0-exp.1. All authoring, play checks, captures, and builds used Blender scripts and Unity CLI. No computer-use automation was used.

## Step 2 visual pass

Steps 2a, 2b, and 2c are implemented. See `ART_DIRECTION.md`, `VISUAL_REVIEW.md`, and `TheLineHolds/Screenshots/diorama-*.png` for the direction, scoped scorecard, and captured evidence.

The terrain is now one smooth, textured meadow with 12,325 vertices. The road is painted into that surface, with worn edges and wheel ruts, so no visible road ribbon can intersect the hills. Road corners use gradual quadratic transitions; the maximum sampled turn is 20.1 degrees. Sampled mesh interiors deviate at most 0.0118 m from the movement surface. Ten level emplacement footprints blend into the hills. Pad labels lie above the stone surface.

Rounded, irregular tree crowns replace cones; clustered blades, shrubs, and stones replace cube grass. Small vegetation combines by material instead of hundreds of separate objects. The exposed earth has a continuous cut edge and pigment layers. Warm sunlight and cool sky fill use four persisted Volume overrides. Native grade-on/off captures differ by about 15.94, 13.19, and 15.93 RGB levels on average.

The default window is 1920×1080, with a fitting size on smaller displays. Existing installations migrate their old saved window size once. Native startup was verified at 1920×1080 in Windowed mode.

Tower models have smooth sandbags, bevelled edges, ammunition and mechanical details, independent traverse, and restrained recoil. Play Mode checks verify all three imported traverse axes and stationary sandbags. Infantry have rounded clothing, helmets, packs, straps, and distance-driven strides; the stride freezes at a fixed distance and never changes the unit root. Tank detail includes armor, tracks, wheel hubs, hatches, lights, grilles, and stowage. Prefab wrappers correct the imported units' backward forward axis without changing movement roots.

A native-only terrain shading defect was caught before handoff. Texture and shadow probes ruled out those causes; adding tangents alone removed the corruption with lighting and ambient occlusion restored. Both textured meshes now retain tangent data. The regression failed before the fix and passes in the saved mesh and native player. Diagnostic overrides were discarded by quitting that QA session and testing a fresh build.

## Gameplay and regressions

Enemy and tower stats, prices, rewards, wave counts, and spacing rules are unchanged. Rounding the route and softening its heights slightly alter travel/coverage; the six-strategy check still accepts mixed defenses and rejects neglect. Tower upgrades and further balance remain deferred to roadmap step 6.

Focused core checks pass for economy, bonuses, invalid orders, sale rounding, pause, wave gates, terminal freeze, defeat priority, ratings, stable targeting, exactly-once rewards, squad formation, armor priority, cluster selection, and actual single-target/splash damage.

| Strategy | Outcome | Lives | Kills / escapes | Combat time | Final-wave route depth | Rear shots |
| --- | --- | --- | --- | --- | --- | --- |
| Mixed bends | Victory | 19 | 215 / 1 | 314 s | 100% | 107 |
| Mixed entrance/middle | Victory | 17 | 213 / 3 | 293 s | 100% | 131 |
| Five MG, three cannons, two mortars | Victory | 12 | 208 / 8 | 309 s | 85% | 26 |
| Machine guns only | Defeat, wave 5 | 0 | 192 / 20 | 313 s | 100% | 220 |
| Lone opening gun | Defeat, wave 2 | 0 | 8 / 20 | 82 s | Not reached | 0 |
| Never reinvest | Defeat, wave 3 | 0 | 64 / 20 | 136 s | Not reached | 0 |

The revised tank measures 1.34×0.97×2.03 m. Minimum measured centre separation is 2.99 m through the entire rounded route, including compressed-spawn stress. The conservative bounding-circle check passes.

Frame-driven input checks pass for placement, inspection, selling, rejected purchases, cancellation, Escape priority, middle/Command drag, pan, zoom limits, reset, camera buttons, rotated placement, UI scroll isolation, wave start, pause, resume, restart, and menu. Temporary devices and focus-routing overrides were restored. Five logical HUD widths (1100, 1200, 1386, 1600, 2000) pass overlap/truncation checks. Standalone UI and lifecycle checks pass.

## Native match and builds

A complete normal-speed standalone match passed in **312 seconds**, with **18,439 rendered frames**, **3 stars / 19 lives / 215 kills / 1 escape**, and **zero runtime errors or warnings**. Final-wave enemies reached the exit and triggered **106 rear-defense shots**. Purchases used earned funds, with no health, income, damage, or wave cheats. The later prefab-only forward-axis correction was checked separately in Play Mode and a fresh native build; it does not change the simulation.

macOS release, Windows cross-build, and macOS QA builds succeed with zero build warnings/errors. Release builds exclude Pipeline's server/compiler assemblies. Packages exclude build backups/symbols and pass ZIP CRC checks. Outputs stay ignored under `TheLineHolds/Builds/`.

Windows runtime verification remains deferred by the user. Use a future Windows VM and `Tools/Validation/windows-checklist.md` before release on that platform.

The editor and QA session opened for validation are closed at handoff; generated font atlas changes are discarded. One updated macOS release copy is left at its main menu for visual review. No live method-reload overrides or temporary input settings remain.

## Checkpoints

- `8287e76`: terrain, road, foliage, lighting, and larger window.
- `849adf9`: refined tower models, independent traverse, recoil, and checks.
- `beccf63`: refined enemy models/stride and native terrain tangent fix.

Roadmap steps 1 and 2a–2c are complete. User visual review comes next, then step 3's authored map composition/detail. Later tower/enemy presentation passes and gameplay upgrades remain pending. Compact results are in `Tools/Validation/last-run.json`; commands are in `Tools/Validation/README.md`.

## November 7 visual-review refinement

Checkpoint `2a3a3c1` replaces open sandbag arcs with complete two-course rings. MG and mortar use 28 bags; cannon uses 32 with a slightly larger radius. Bags follow the wall tangent, alternate between courses, and remain fixed when the guns rotate. Cannon trails and mortar supplies fit inside the rings. `CheckFoliage.Assets` verifies count, radial placement, full-circle coverage, leaf UV/tangent data, and shared vein textures; `CheckDiorama.Towers` passes all three traverse/fixed-wall checks.

Smooth bush/tree crown masses are replaced by individual ridged leaves, visible branches, three green tones, and original leaf-vein textures. Shrubs combine by material. Landscape renderers fell from 269 to 137. Leaf normals were corrected during the draft pass. The native probe uses vertex-attribute metadata for imported GPU-only meshes; it does not require readable duplicate mesh buffers. The corrected probe passes in a fresh standalone session, including a short combat sample at about 60 fps. This is a three-second sample, not a full-match performance benchmark. Runtime gameplay source is unchanged; the earlier full-match evidence remains above.

New review captures: `leaf-detail.png`, `sandbag-rings.png`, `foliage-overview.png`, and `foliage-combat-close.png`. The leaf-detail capture uses a temporary closer review camera; the gameplay zoom limit remains unchanged. No gameplay tuning or new packages were added. Native macOS QA/release and Windows cross-builds are refreshed; Windows runtime verification remains deferred.
