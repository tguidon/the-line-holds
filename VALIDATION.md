# Prototype validation

Checked on Apple silicon macOS, November 7, 9 and 10, 2026, with Unity 6000.6.7f1, URP 17.6.0, Unity CLI 1.0.0-beta.17, and Pipeline 0.10.0-exp.1. All authoring, play checks, captures, and builds used Blender scripts and Unity CLI. No computer-use automation was used.

## Current: November 10 tactical pause and countryside refinement

Pause now freezes spawning, movement, health, projectiles and reload time while allowing building, inspection, upgrades, sales and camera movement. The blocking modal is replaced by a small corner indicator; restart/main-menu actions live in the optional Options menu. Unaffordable upgrades are non-interactable with a lock, muted text/price and an exact funds shortfall. Focused simulation checks require live enemies and an in-flight shot before testing freeze. Play Mode pointer/keyboard checks exercise paused sale, rebuilding, a rejected locked upgrade, camera orbit and resume. Core rules, ten-wave rules and all 18 strategy/seed scenarios pass.

The extended scenery now has 16 structures across six types, including original cottages, an inn, dairy, tool sheds and wells. All 22 decorative road chains end at a junction, building yard, battlefield endpoint or outer map edge. Eighty spatial grass batches contain 275,160 vertices; hedges, crop plots, fences and an orchard connect the surrounding landscape visually. Core gameplay data, route and placement anchors are unchanged. Saved pasture seam colors still pass. First visual review identified blank rear facades and smooth new roofs; the final assets have windows on all facades and tiled roof courses. Editor captures are `tactical-pause.png`, `countryside-panorama.png`, `countryside-village.png` and `countryside-farm.png`.

Separate **macOS-Review** and **Windows-Review** release builds pass with zero build warnings/errors. The editor console has zero errors; its history includes deprecated icon-API compiler warnings. No new native player was launched: the user was still playing, so the existing release was neither replaced nor relaunched. Its executable SHA-256 remains unchanged. New native-player acceptance and Windows execution remain pending. Details and repeatable commands are in `Tools/Validation/tactical-pause-scenery.json` and the validation README. Step 7 remains in progress.

## Historical: November 10 startup camera and Dock icon corrections

The previous native overview used one fixed orthographic size, which clipped the entry at 4:3 and narrower window shapes. `CheckOverview.All` failed before the fix with viewport X=0.02 for the entry margin. Startup/Reset now project the route, sites and landmark headroom into the original camera axes and fit both dimensions within HUD-safe margins. Resizing the untouched overview refits it; a manually panned, rotated or zoomed view stays under player control until Reset.

Five projection aspects and five native window sizes pass the complete route/endpoint checks: 1600×900 (size 8.84), 1516×1324 (13.72), 1024×768 (11.79), 1024×1280 (19.64) and 1920×1080 (8.84). The 1516×1324 view matches the reported screenshot proportions. The native check also verifies resize preserves manual camera changes and Reset fits again. Actual injected-input checks pass placement, orbit, modifier drag, pan, zoom, help controls, pause/restart/menu. No gameplay/economy data changed; the previous full-campaign result remains gameplay evidence rather than a new run.

The generated icon was already present in the bundle. The app still carried Unity's template identifier, while direct CLI launches lacked explicit bundle registration. The desktop now has ID `com.thelineholds.game`, version 0.1.1/build 2, explicit Standalone icon slots, and a macOS post-build registration callback. The built ICNS, OS-resolved icon, and actual running NSApplication icon were extracted and inspected: the running gun-and-sandbags image is correct. This uses file/platform API reads through CLI, without desktop UI automation.

Native evidence: `overview-fit-1516x1324.png`, `overview-fit-1600x900.png`, `overview-fit-1024x1280.png` and `running-app-icon.png` under `TheLineHolds/Screenshots/`. QA/macOS/Windows builds pass with zero build errors/warnings; Windows execution remains deferred. Final runtime, package and startup records are in `Tools/Validation/startup-fixes.json`.

## Historical: November 10 player-feedback polish

Implementation checkpoint `346797c` adds the compact icon-led HUD, independent ambience mute, continuous countryside, wider road pigment, nine distinct tower upgrade models, upgrade purchase feedback, organic infantry lanes/gaps, closable inspection, local vehicle hits and pooled destruction. Ground clicks, repeated tower clicks and Escape deselection are locked down by a check that failed before the controller fix. Real injected-input checks pass placement/selling, UI scroll isolation, the camera/help controls, Cmd drag, pause/resume/restart/menu. Upgrade cost and branch callbacks, ambient playback/mute, and all 18 seeded balance strategies pass. Continuous infantry spread covers 17, 16 and 17 distinct 0.1 m road bins for seeds 1944, 42 and 2026, replacing three repeated lanes. Vehicle minimum centre separation remains 2.76 m.

The native full campaign passes at four-times time acceleration: **216 real seconds / 12,825 frames, ten waves, 17 lives, 456 kills, three escapes, 20 upgrades and all seven archetypes**. Final-wave route depth reaches 99.7%, with 198 rear shots. The three-second normal-speed busy-scene sample averages **59.4 fps**. Peak actors alive: 39; retiring: 10/24; created/reused: 81; world voices: 24/24; sampled audio peak: 0.5330, below clipping. Projectile allocation remains 32. These measurements verify operation; fun and full normal-speed pacing remain a user playtest.

Final native review then corrects only the apron mask and fire texture/debris appearance. The saved pigment seam check passes (core RGB 0.506/0.545/0.353; adjoining pasture 0.489/0.531/0.340). A rebuilt native probe passes all nine distinct upgrade references, ambience/mute, absence of whole-vehicle flashes, local impact particles and constant explosion-emitter count. Four real window sizes pass counters, worst-case wave intel and two branch controls. Final QA console reports zero errors/warnings; the startup log still includes notices for unused stripped URP depth-of-field/Panini passes, which the authored grade does not use. Full-match and final-source hashes are recorded separately in `last-run.json`.

QA, macOS release and Windows cross-builds all succeed without build errors/warnings. ZIP CRC, shipped credits and exclusion of Pipeline server/compiler/backups/symbols pass (macOS about 102.1 MiB; Windows 110.5 MiB). Windows execution remains deferred. One updated macOS release is opened after closing QA/editor and discarding the generated font cache. No computer-use automation, new package or AudioMixer was used.

Native `polish-native-map.png`, `polish-native-upgrades.png`, `polish-native-explosion.png` and `polish-native-ui-1024x768.png` show the final presentation. The funded upgrade and explosion close-ups are temporary showcases. The UI stress capture deliberately substitutes one final-wave fixture, so its 0/1 header is not campaign data. `polish-native-combat.png` and `polish-native-victory.png` are the actual earned-funds campaign before the last presentation-only correction. The imagegen app icon and complete final prompt are recorded in `Tools/Blender/APP_ICON.md`.

## Historical November 9: steps 5 and 6

Steps 5 and 6 are complete. Step 7 is the pending human playtest. The current campaign contains ten waves and 459 enemies across riflemen, runners, assault troops, trucks, armored cars, light tanks and heavy tanks. See `GAMEPLAY.md` for stats, compositions, counters and incremental upgrade prices. Historical five-wave results below describe earlier checkpoints.

- Compilation and focused core rules pass, including upgrade costs/guards/refunds, suppression, exactly-once accounting, seeded formation replay and mixed-speed vehicle spacing. Minimum measured vehicle centre separation is 2.76 m; infantry ease onto the verge when passing vehicles.
- Eighteen strategy/seed scenarios pass. Upgraded mixed layouts win with 16–20 lives and final-wave rear engagement; fixed base towers lose on waves 9–10, MG-only upgrades lose on wave 6, and neglected defenses lose on wave 2. Exact results are in `Tools/Validation/campaign-balance.json`. These checks establish a useful baseline; they do not measure player enjoyment.
- Enemy lifecycle checks cover gait/root isolation, pause, hit/death/reset and 20 reuse cycles. Native checks cover all seven models, supported actor shaders, ten-wave data and worst-case intel/upgrade layouts at 1024×768, 1200×1000, 1600×900 and 1920×1080. Original window/data are restored afterward. UI callbacks and frame-driven placement, upgrades, selling, camera, pause/restart/menu input pass separately.
- The real native campaign passes in **217 real seconds / 12,422 frames**, using earned funds and normal rules: **10 waves, 16 lives, 455 kills, 4 escapes, 19 upgrades and all seven enemy types**. Final-wave route depth is **99.7%**, with **198 rear-defense shots**. Time acceleration is 4x for verification; a three-second 1x busy-scene sample averages **57.5 fps**. This is not a normal-speed human pacing test.
- Peak live enemies: 38; retiring actors: 7 (cap 24); actors created/reused: 80; world audio voices: 24 (cap 24). Projectile allocation stays at 32 and corpses clear after victory. Sampled mixed audio peaks at 0.5199, below clipping. Signal checks do not replace subjective listening.
- QA console reports zero errors and one warning. The player log includes URP notices for unused stripped depth-of-field/Panini passes; those effects are not part of the authored grade. No gameplay exception occurs. Retained bloom/color grading and native actor materials were reviewed. Build reports themselves have zero warnings/errors.
- Final QA, macOS release and Windows cross-builds succeed; ZIP CRC, included audio credits and exclusion of Pipeline server/compiler/debug output pass. Windows runtime testing remains deferred to a future VM. Generated font cache changes are discarded after closing the editor. One fresh release copy is opened for player review.

Native evidence: `campaign-native-roster.png` is a staged roster; `campaign-native-upgrades.png`, `campaign-native-combat.png` and `campaign-native-victory.png` are actual campaign captures under `TheLineHolds/Screenshots/`. Reproduction and measured results are in `Tools/Validation/README.md` and `last-run.json`.

## Historical step 2 visual pass

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

Windows runtime testing is deferred and does not block publishing the Windows ZIP. Follow the [Windows release checklist](Tools/Validation/windows-checklist.md).

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

## Winter Forest — November 12, 2026

The bare route was playable before scenery was added. At seed 42 the accelerated Play Mode run bought all defenses from earned funds, completed ten waves, and finished with 19 lives. The dressed run finished with 18 lives, 457 kills, two escapes, 20 upgrades and 176 final-wave shots from the rear positions. Both runs accounted for all 459 enemies.

| Matched-seed simulation | Countryside | Winter Forest |
| --- | --- | --- |
| Route length | 52.7 m | 56.9 m |
| Closest pad to road center | 2.49 m | 2.47 m |
| Shortest base MG road coverage | 5.5 m | 4.6 m |
| Mixed defenses, lives at seeds 42 / 1944 / 2026 | 19 / 20 / 20 | 18 / 20 / 20 |
| MG only, defeat wave at the same seeds | 7 / 7 / 9 | 7 / 6 / 7 |

The macOS development player ran the opening encounter at 57.5 FPS on each map in separate four-second samples at 1600×900. After native resizes to 1024×768 and 1920×800, the Winter Forest patrol spawned 13 m and 24 m before the entrance and walked through the marker. The editor input check exercised mouse selection, purchases, upgrades' affordability, sales, zoom/orbit/pan, pause, restart and return to the menu. The winter camera check also covered a 110° orbit and verified that both weather emitters were running. Windows runtime validation is pending.

Repeat with `CheckWinterCampaign.Compare` in Edit Mode, `PlayWinterForest.cs` and `CheckWinterView.cs` in Play Mode, and `CheckWinterPlayer.cs` against the development player. The accepted gameplay frames are the [opening patrol](TheLineHolds/Screenshots/winter-forest-patrol.png) and [wave nine armor](TheLineHolds/Screenshots/winter-forest-armor.png).

## Saint-Aubin, November 13

The two 54.68 m routes share their last 39.91 m. All ten sites have at least 2.56 m of centerline clearance and at least 5.4 m of base MG coverage on a reachable route. Narrow (4:3) and wide (2.4:1) camera fits and a 75-degree orbit kept both arrival points outside the frustum.

`CheckWinterCampaign.Compare` used the same authored waves, prices, role order, upgrade rules and seeds on all three maps, with placements chosen for each layout. With mixed defenses, Saint-Aubin finished with 18, 19 and 18 lives on seeds 42, 1944 and 2026. Countryside and Winter Forest each finished with 19, 20 and 20. With only MGs, Saint-Aubin lost on wave 6 in all three runs, Winter Forest on wave 7, and Countryside on waves 7, 7 and 9.

The menu-driven Mac Editor playthrough on the dressed scene (seed 42) completed all ten waves with 19 lives, 458 kills, one escape and 20 earned upgrades. Both single entrances and the split entrance occurred. The armored column and upgraded towers are visible in the [wave-nine capture](TheLineHolds/Screenshots/saint-aubin-armor.png).
