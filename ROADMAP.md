# Prototype Improvement Roadmap

Created November 4, 2026. Steps 1, 2a, 2b, 2c, 3, and 4 are complete. Steps 5 and 6 are in progress. Step 7 is pending.

Follow the steps in order. Update each status to In progress or Complete as work advances. Record findings, validation results, and commit references in the progress log.

| Step | Status | Focus | Completion criteria |
| --- | --- | --- | --- |
| 1 | Complete | Two focused playtest rounds | Test placement choices, tower roles, wave pressure, pacing, and spending decisions. Adjust between rounds. Record findings and changes from both rounds. |
| 2a | Complete | Terrain and map | Replace hard slopes and angular road edges with natural landforms, worn roads, rounded trees, clustered foliage, and deliberate diorama lighting. Increase the default window size. Assess overview, close-up, and rotated views. |
| 2b | Complete | Tower visual target | Improve the three tower models with rounded sandbags, richer materials, mechanical detail, and distinct silhouettes. Keep prices and combat rules unchanged. |
| 2c | Complete | Enemy visual target | Improve infantry and armor geometry, surface detail, and movement presentation. Keep individual units readable and preserve vehicle spacing. |
| 3 | Complete | Extend the approved map detail | Build on the map-wide terrain base with authored hedgerows, farm boundaries, field positions, and local prop details. Keep the route and tower sites readable from different camera angles and zoom levels. |
| 4 | Complete | Extend tower presentation | Refine the step 2b target with richer firing motion, operation details, and combat effects. Give weapons and hits distinct sounds. Keep each tower role easy to identify during combat. |
| 5 | In progress | Extend enemy presentation | Build on the step 2c miniatures with richer movement, damage feedback, and death effects. Keep squads, armor, and individual targets readable during busy waves. |
| 6 | In progress | Expand the campaign and gameplay | Add tower upgrades/specializations, seven enemy archetypes, a varied ten-wave campaign, fair formation randomness, useful previews, and tuned economy/pressure. Verify that deployments and upgrades matter. |
| 7 | Pending | Final player playtesting | Play the expanded campaign, assess fun, readability, progression and difficulty, and record feedback for the next tuning pass. |

Fix readability issues throughout all steps. Step 2 establishes the visual target; later passes extend and refine it. Gameplay upgrades and balance remain deferred to step 6.

## Progress log

November 5, 2026: Round 1 feedback implemented in `a12aa99` and `92b5610`. Camera shortcuts, road clipping, UI overlap, tank spacing, round funds, and baseline difficulty are updated. Regression checks cover each change. See [VALIDATION.md](VALIDATION.md) for the evidence.

| Playtest round | Status | Result and follow-up |
| --- | --- | --- |
| 1 | Complete | Feedback implemented and included in round 2. |
| 2 | Complete | User won all five waves with three stars, 20 lives, 216 kills, zero escapes, and 789 funds remaining. Move to visuals next. |

November 7, 2026: The user completed round 2 and reported that the game feels good and the difficulty increased. No enemies escaped. The user accepted the current gameplay for the visual passes and deferred further tuning to step 6.

Round 2 follow-up: Once all pads are filled, the player needs further spending and progression choices. Add tower upgrades in step 6. Reassess difficulty after the visual improvements, using new playtest feedback.

For each update, record the date, step number, work completed, playtest or visual findings, and related commits.

November 7, 2026 — Step 2a: Replaced the five terrain strips with a continuous smooth meadow, softened road bends, baked worn road edges and wheel ruts into the turf, and replaced cone trees/cube grass with rounded crowns and combined foliage clusters. Added textured exposed earth, warm/cool lighting, level emplacement footprints, and a 1920×1080 default window with one-time migration of the old saved size. Core rules and all six balance scenarios pass; mixed layouts still win and neglect loses. The rounded route slightly changes outcomes without changing combat stats. Overview and rotated close-up inspected through Unity CLI.

November 7, 2026 — Step 2b: Refined all three defense models with bevelled mechanical edges, smooth sandbags, sights, ammunition, wheel hubs, shield rivets, and barrel detail. Added independent traverse and restrained recoil. Verified all three imported aiming axes and that sandbags stay fixed. Saved the close-up at `TheLineHolds/Screenshots/diorama-towers.png`. Step 2a checkpoint: `8287e76`.

November 7, 2026 — Step 2c: Refined infantry helmets, clothing, packs, straps, rifles, and rounded limbs. Added a stride driven by simulation distance, which freezes when movement pauses. Refined tank armor, hatches, tracks, wheel hubs, lights, engine grilles, and stowage. Tank spacing passes against the new 1.34 × 0.97 × 2.03 m model: minimum centre separation is 2.99 m. Infantry presentation never changes unit-root position. Tower checkpoint: `849adf9`.

November 7, 2026 — Native visual validation: Confirmed 1920×1080 windowed startup and four persisted grade overrides. A native-only Lit shading fault was traced to missing mesh tangents; adding tangents alone fixed it with shadows and ambient occlusion restored. The regression failed before the fix and passed after it. The fresh QA, macOS release, and Windows cross-build each completed with zero build warnings and errors. Windows runtime verification remains deferred.

November 7, 2026 — Final step 2 handoff: `beccf63` records the enemy target and tangent fix. Corrected the imported infantry/tank forward axis in prefab wrappers, verified it in Play Mode and a rebuilt native player, and passed all four native resize/close-orbit checks. A normal-speed native full match passed in 312 seconds with 18,439 rendered frames, three stars, 19 lives, 215 kills, one escape, and zero runtime errors/warnings. The final prefab-only axis correction does not alter the simulation. Fresh platform builds and package CRC checks pass. See [VISUAL_REVIEW.md](VISUAL_REVIEW.md) and [VALIDATION.md](VALIDATION.md). One updated release copy is open for user visual review before step 3.

November 7, 2026 — Visual-review feedback: Replaced open sandbag arcs with complete, staggered circular walls around the gun mechanisms. MG/mortar walls have two 14-bag courses; the cannon uses two 16-bag courses with room for its wheels and carriage. Replaced smooth tree/bush masses with pointed, ridged leaf geometry, visible branches, shared vein/color textures, and irregular silhouettes. Foliage remains clear of roads and emplacement footprints. This refines steps 2a/2b; steps 3–6 remain pending.

November 7, 2026 — Refinement validation: `2a3a3c1` records the ring/foliage checkpoint. Native combat checks confirm full walls, leaf textures/UV/tangent attributes, and about 60 fps in a three-second sample; fresh QA console has zero errors/warnings. All three platform builds pass without build warnings/errors, packages pass CRC, and runtime gameplay source is unchanged. Removed obsolete cloud-shrub assets and reopened one updated macOS release copy for review.

November 7, 2026 — House refinement: Replaced broad rounded wall bevels with crisp masonry corners. Rebuilt both roof wings to descend from one ridge, closed the gables, and added slate courses, eaves, ridge cap, chimney flashing/cap, stone footing, framed windows, shutters, and a timber door. Architecture is combined by material. The house remains decorative; gameplay and layout are unchanged.

November 7, 2026 — House validation: `eb22840` records the architecture checkpoint. Imported roof raycasts and native views pass; both houses render correctly with ten material groups each. Fresh native console has zero errors/warnings. QA, macOS release and Windows cross-build pass without build warnings/errors; ZIPs pass CRC. Runtime gameplay source is unchanged. One updated release copy is open for review.

November 7, 2026 — Step 3 editor checkpoint: Replaced raised numbered pads with painted earth clearings, faint access tracks, and low timber markers at the same ten anchors. Added an open farm entrance, raised rear checkpoint, physical two-sided stencil signs, crop plots/fences, supply details and a timber-lined field position. Road pigment now reaches both board edges. The earth cut uses three irregular horizons, world-sized grain/stone detail, a muted root mat and embedded stones. A geometry sweep verifies the whole vehicle corridor remains clear. Frame-driven placement/orbit checks and all six balance scenarios pass.

November 7, 2026 — Step 3 complete: `1bfdca5` records the composition checkpoint. Corrected single-mesh field-prop import transforms after native close-up review, then rechecked the actual combined-mesh vehicle corridor. Final native overview/orbit/entry/exit/soil views and four window sizes pass with zero console errors/warnings. A complete four-times-speed native match passed with three stars, 18 lives, 214 kills and two escapes; this verifies operation, not human pacing. Fresh QA/macOS/Windows cross-builds have zero build warnings/errors and packages pass CRC. One updated macOS release is open for user review. Step 4, tower presentation and combat effects, is next; gameplay tuning remains step 6.

November 9, 2026 — Step 4 implementation checkpoint: Added authored muzzle sockets and independent recoil assemblies, smoothed traverse, MG flashes/tracers/ejected brass, cannon recoil/dust/shells and elevated mortar launch/arc effects. Replaced generic combat spheres with five shared emitters and a warmed 32-object projectile pool. Exact shot-context notifications connect presentation without changing targeting, damage, cooldowns, flight time or economy. Twenty-one edited recorded weapon/hit variations use bounded priority voices, camera-relative stereo and pause/reset/gain handling. Source licenses and shipped attribution are retained. Core rules, all six balance strategies and live three-role mechanical/pooling/audio checks pass; native build review follows.

November 9, 2026 — Step 4 complete: `dddf6cc` records the implementation checkpoint. A full normal-speed native match passed in 314 seconds and 18,701 frames: three stars, 18 lives, 214 kills, two escapes, 100% final-wave route depth and 109 rear shots. All three weapon roles fired and impacted; allocation stayed at 32 projectiles, with peak particles 124 and world voices 14/24. Mixed output peak was 0.3265, without clipping; console errors/warnings were zero. The final visual-only fix brightens muzzle flashes, raises mortar smoke and moves sparks onto exposed armor; the fresh native probe passes, including the surface-position regression. QA/macOS/Windows builds and package checks pass; audio credits ship. Step 5, richer enemy movement/damage/death feedback, is next. Gameplay and tower upgrades remain step 6; Windows execution remains deferred.

November 9, 2026 — Step 5 checkpoint: Added distance-driven body sway, wheel motion, warm hit reactions, visible damage wear/smoke, timed infantry falls and vehicle destruction/retirement. Actor models and bars are reused across waves, with at most 24 retiring actors. Read-only hit/kill notifications preserve combat rules. Editor checks pass root isolation, pause, fall timing, reset and 20 reuse cycles; the prior core regression suite passes. Final native review will include the expanded roster from step 6. Player playtesting is now a separate pending step 7.

November 9, 2026 — Step 6 gameplay checkpoint: Added riflemen, runners, assault troops, trucks, armored cars, light tanks and heavy tanks; ten announced wave compositions contain 459 enemies. Seeded patrol/stream/cluster/convoy/mixed formations vary group sizes, timing, lanes and individual speeds. Infantry can escort queued vehicles, whose spacing remains safe through bends. Each tower has a veteran tier and two mutually exclusive specializations, including suppression and armor/area counters. Purchases retain cooldowns and refund half of actual investment on sale. Added next-wave intel, upgrade inspection, visible tier markings, amber suppression bars and 1x/2x pace. Eighteen seeded strategy runs accept upgraded mixed layouts and reject static, single-role and neglected defenses. Core and upgrade/UI checks pass; native campaign verification follows.
