# Prototype validation

Checked on Apple silicon macOS, November 7 and 9, 2026, with Unity 6000.6.7f1, URP 17.6.0, Unity CLI 1.0.0-beta.17, and Pipeline 0.10.0-exp.1. All authoring, play checks, captures, and builds used Blender scripts and Unity CLI. No computer-use automation was used.

## Step 2 visual pass

Steps 2a, 2b, and 2c are implemented. See `ART_DIRECTION.md`, `VISUAL_REVIEW.md`, and `TheLineHolds/Screenshots/diorama-*.png` for the direction, scoped scorecard, and captured evidence.

The terrain is now one smooth, textured meadow with 12,325 vertices. The road is painted into that surface, with worn edges and wheel ruts, so no visible road ribbon can intersect the hills. Road corners use gradual quadratic transitions; the maximum sampled turn is 20.1 degrees. Sampled mesh interiors deviate at most 0.0118 m from the movement surface. Ten level emplacement footprints blend into the hills. Step 3 replaces the initial labelled stone pads with painted earth clearings.

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

The step 2 normal-speed standalone match passed in **312 seconds**, with **18,439 rendered frames**, **3 stars / 19 lives / 215 kills / 1 escape**, and **zero runtime errors or warnings**. Final-wave enemies reached the exit and triggered **106 rear-defense shots**. Purchases used earned funds, with no health, income, damage, or wave cheats. The later prefab-only forward-axis correction was checked separately in Play Mode and a fresh native build; it does not change the simulation. The current step 4 full-match result is recorded below.

macOS release, Windows cross-build, and macOS QA builds succeed with zero build warnings/errors. Release builds exclude Pipeline's server/compiler assemblies. Packages exclude build backups/symbols and pass ZIP CRC checks. Outputs stay ignored under `TheLineHolds/Builds/`.

Windows runtime verification remains deferred by the user. Use a future Windows VM and `Tools/Validation/windows-checklist.md` before release on that platform.

The editor and QA session opened for validation are closed at handoff; generated font atlas changes are discarded. One updated macOS release copy is left at its main menu for visual review. No live method-reload overrides or temporary input settings remain.

## Checkpoints

- `8287e76`: terrain, road, foliage, lighting, and larger window.
- `849adf9`: refined tower models, independent traverse, recoil, and checks.
- `beccf63`: refined enemy models/stride and native terrain tangent fix.

Roadmap steps 1, 2a–2c, 3 and 4 are complete. User visual/listening review comes next; step 5 extends enemy presentation. Gameplay upgrades remain step 6. Compact results are in `Tools/Validation/last-run.json`; commands are in `Tools/Validation/README.md`.

## November 7 visual-review refinement

Checkpoint `2a3a3c1` replaces open sandbag arcs with complete two-course rings. MG and mortar use 28 bags; cannon uses 32 with a slightly larger radius. Bags follow the wall tangent, alternate between courses, and remain fixed when the guns rotate. Cannon trails and mortar supplies fit inside the rings. `CheckFoliage.Assets` verifies count, radial placement, full-circle coverage, leaf UV/tangent data, and shared vein textures; `CheckDiorama.Towers` passes all three traverse/fixed-wall checks.

Smooth bush/tree crown masses are replaced by individual ridged leaves, visible branches, three green tones, and original leaf-vein textures. Shrubs combine by material. Landscape renderers fell from 269 to 137. Leaf normals were corrected during the draft pass. The native probe uses vertex-attribute metadata for imported GPU-only meshes; it does not require readable duplicate mesh buffers. The corrected probe passes in a fresh standalone session, including a short combat sample at about 60 fps. This is a three-second sample, not a full-match performance benchmark. Runtime gameplay source is unchanged; the earlier full-match evidence remains above.

New review captures: `leaf-detail.png`, `sandbag-rings.png`, `foliage-overview.png`, and `foliage-combat-close.png`. The leaf-detail capture uses a temporary closer review camera; the gameplay zoom limit remains unchanged. No gameplay tuning or new packages were added. Native macOS QA/release and Windows cross-builds are refreshed; Windows runtime verification remains deferred.

## November 7 farmhouse refinement

Checkpoint `eb22840` replaces the broad rounded building bevel with millimetre-scale architectural edge breaks and rebuilds the roof around one shared ridge. Both wings now descend toward the eaves; solid gables fill their ends. Slate courses, fascia, ridge cap, chimney flashing/cap, stone footing/sills, window frames/shutters and a timber door add definition. The final fix pass adds side windows for rotated views.

Imported-geometry raycasts pass: ridge 2.11 m, mid slopes 1.83/1.83 m, and eaves 1.40/1.40 m, with no missing roof sample. Static parts combine into ten material groups per house. Native checks verify both existing map houses, live frames, and supported materials. `houses-detail.png` is a temporary two-orientation art review; `houses-native-close.png` and `houses-native-overview.png` show actual map instances. Gameplay source, placement and balance are unchanged. All three builds and package integrity are checked again.

## Step 3 composition validation

The ten placement anchors, route, enemy/tower data and wave/economy rules are preserved. Pedestal renderers and numbered/floating map labels are removed; physical signs, ground pigment and low timber markers replace them. Help text refers to cleared gun sites. Tower view height is adjusted to sit on earth; simulation shot origins and rules are unchanged.

`CheckMapComposition.All` verifies anchor positions, removal of pedestals/legacy labels, three textured/tangent-bearing soil horizons, matching endpoint landmarks and a full corridor sweep. It temporarily adds mesh colliders to the actual combined details and probes three lateral lanes/heights along the route; no decoration intersects the vehicle corridor. Temporary colliders are removed in `finally`. Clean placement roots preserve FBX axis/scale conversion for the new single-mesh field kit.

Frame-driven input checks pass placement, selling, invalid orders, camera shortcuts, rotation, zoom, rotated placement, pause/restart/menu and fixed physical landmarks during orbit. Core checks and all six balance strategies pass with unchanged outcomes. Native checks cover ground sites, soil materials/tangents, physical signs and overview/orbit/endpoint/soil close views. An accelerated four-times-speed native match resolves all five waves using earned funds and normal rules; this is a verification run, not a new human pacing test.

That match completed in 79 real seconds and 4,711 rendered frames: three stars, 18 lives, 214 kills, two escapes, 100% final-wave route depth and 108 rear-defense shots. The final field-prop-only transform fix was subsequently checked in a fresh native player and by the corridor sweep; combat source is unchanged. The earlier 312-second normal-speed run is historical step 2 evidence. Four native window sizes pass again; the resize script now restores the user's original size. Final QA console counts are zero errors and zero warnings. QA/macOS/Windows builds pass without build warnings/errors, release server/compiler assemblies are absent, and both packages pass CRC (macOS 42.6 MiB; Windows 52.2 MiB). Windows execution remains deferred.

## Step 4 weapon and audio validation — November 9

Checkpoint `dddf6cc` adds muzzle sockets and recoil subassemblies to the three Blender weapon models. Live checks verify correct muzzle direction, an elevated mortar tube, recoil recovery/freeze and fixed sandbags/supports. Five shared emitters and 32 warmed projectile objects replace per-shot primitive creation. `CheckWeaponFeedback.Play` repeats 30 fire/impact cycles without growing either pool, checks active audio, pause/unpause, shared SFX gain and reset/menu cleanup. `Events` compares 57 typed fires/impacts with 57 legacy cues and verifies unchanged simulation origins. Core rules and all six balance layouts pass with the same results as step 3. Frame-driven input checks pass again.

The bank contains 21 short mono 48 kHz PCM variations (994,524 WAV bytes including headers), with edge fades and normalized peaks below 0.8. It separates three weapon reports from soft, metal, shell and mortar impacts. The archive's gunshot attribution requirement is retained despite a conflicting web license label; credits ship in StreamingAssets and both ZIPs. Native output probing confirms actual nonzero mixed audio. The headless SFX gain layer is retained; no mixer or new package was installed.

A normal-speed native full match passed in **314 seconds / 18,701 rendered frames**, with **three stars, 18 lives, 214 kills and two escapes**. Final-wave depth reached 100%, with 109 rear-defense shots. Real fire counts were MG 1,760 / cannon 164 / mortar 124; impacts were 1,760 / 163 / 124. The final pending cannon shot was cleared by normal wave completion. Peak live particles were 124, peak world audio voices 14 of 24, mixed output sample peak 0.3265, and projectile allocation stayed at 32. Console errors and warnings were both zero.

The final visibility-only refinement moves ricochets from the simulation's unit centre to the exposed hull edge, raises mortar smoke slightly, and uses additive warm flashes/sparks. Native close-up review checks this separately; it does not alter the earlier match's combat or audio logic. The full-match source hash and final source hash are recorded separately in `last-run.json`. Native screenshots distinguish staged launch/impact probes from real match captures. Signal checks do not replace the user's subjective listening review. Windows runtime testing remains deferred.

Final QA/macOS/Windows builds pass with zero build errors/warnings. Final native probe, including exposed armor sparks, passes with zero console errors/warnings and nonzero unclipped mixed output. ZIP CRC and included audio credits pass (macOS 43.5 MiB; Windows 53.1 MiB). The editor and QA session are closed, generated font cache changes discarded, and one updated macOS release is left at the main menu.
