# Prototype Improvement Roadmap

Created November 4, 2026. Step 1 is complete. Step 2 is in progress, split into three visual passes. Steps 3–6 are pending.

Follow the steps in order. Update each status to In progress or Complete as work advances. Record findings, validation results, and commit references in the progress log.

| Step | Status | Focus | Completion criteria |
| --- | --- | --- | --- |
| 1 | Complete | Two focused playtest rounds | Test placement choices, tower roles, wave pressure, pacing, and spending decisions. Adjust between rounds. Record findings and changes from both rounds. |
| 2a | Complete | Terrain and map | Replace hard slopes and angular road edges with natural landforms, worn roads, rounded trees, clustered foliage, and deliberate diorama lighting. Increase the default window size. Assess overview, close-up, and rotated views. |
| 2b | Complete | Tower visual target | Improve the three tower models with rounded sandbags, richer materials, mechanical detail, and distinct silhouettes. Keep prices and combat rules unchanged. |
| 2c | In progress | Enemy visual target | Improve infantry and armor geometry, surface detail, and movement presentation. Keep individual units readable and preserve vehicle spacing. |
| 3 | Pending | Extend the treatment across the map | Apply the chosen terrain, materials, lighting, and prop treatment across the battlefield. Keep the route and build pads readable from different camera angles and zoom levels. |
| 4 | Pending | Improve towers | Improve silhouettes, materials, firing motion, and combat effects. Keep each tower role easy to identify during combat. |
| 5 | Pending | Improve enemies | Improve infantry and tank models, movement, damage feedback, and death effects. Keep squads, armor, and individual targets readable during busy waves. |
| 6 | Pending | Revisit gameplay after the visual passes | Add and playtest tower upgrades so spending decisions continue after all build pads are filled. Revisit upgrade costs, economy, wave pressure, and difficulty. Record fresh playtest findings and resulting changes. |

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
