# Current campaign

November 9, 2026. This expands the initial prototype into a ten-wave battlefield. Final player playtesting is roadmap step 7; this is the baseline to evaluate, not a claim of finished balance.

Start with 300 funds and 20 lives. Build on the ten clearings, click a deployed tower to upgrade it, and call the next wave when ready. Orders work during combat. Each escape costs one life. Wave completion pays 70, 80, …, 160 funds exactly once. A victory scores three stars at 18–20 lives, two at 10–17, and one at 1–9. The 1x/2x control changes game pace; pause freezes combat while tower management and camera controls remain available.

## Enemies

| Type | Base health | Speed, m/s | Reward | Counter |
| --- | ---: | ---: | ---: | --- |
| Riflemen | 68 | 1.20 | 4 | Efficient MG coverage |
| Runners | 38 | 2.05 | 4 | Sustained fire and rear coverage |
| Assault troops | 150 | 1.05 | 7 | Splash damage and suppression |
| Trucks | 200 | 1.85 | 12 | Unarmored; MGs can contribute |
| Armored cars | 270 | 1.90 | 15 | Fast armor; overlapping anti-tank coverage |
| Light tanks | 530 | 1.15 | 22 | Anti-tank guns |
| Heavy tanks | 1,250 | 0.90 | 38 | Upgraded armor specialists |

Individual speeds vary by ±8%. Wave health multipliers rise from 1.0 to 5.8. Later enemies therefore need stronger defenses even when their silhouette is familiar. Vehicles queue at least 4.4 route metres apart and cannot overtake into each other. Infantry follow a separate schedule so they can escort the convoy. Their starting road positions and individual release gaps vary continuously; a gentle lateral drift and varied verge passing avoid repeated three-column formations. The route and vehicle safety gap are unchanged.

## Ten attacks

| Wave | Attack | Formation | Announced composition |
| --- | --- | --- | --- |
| 1 | Probing patrol | Patrol | 18 riflemen |
| 2 | The runners | Stream | 8 riflemen, 28 runners |
| 3 | Platoon advance | Clusters | 30 riflemen, 10 assault troops |
| 4 | Motor reconnaissance | Mixed | 14 runners, 6 trucks, 5 armored cars |
| 5 | Armored spearhead | Convoy | 4 armored cars, 8 light tanks |
| 6 | Crossfire | Mixed | 30 riflemen, 20 runners, 10 assault troops, 3 light tanks |
| 7 | The feint | Stream | 24 runners, 18 assault troops, 6 trucks, 4 light tanks |
| 8 | Infantry surge | Clusters | 54 riflemen, 14 runners, 18 assault troops |
| 9 | Heavy column | Convoy | 6 armored cars, 8 light tanks, 6 heavy tanks |
| 10 | The final push | Mixed | 40 riflemen, 26 runners, 16 assault troops, 6 trucks, 6 armored cars, 10 light tanks, 3 heavy tanks |

There are 459 enemies total. The preview fixes the composition, while seeded formation planning varies group sizes, order, gaps, lanes and individual speeds. Infantry clusters range from 2–8; streams use sections of 4–9 with longer member gaps. Vehicles release singly or in pairs, subject to road clearance. Seeds are replayable in verification; random variation never introduces an unannounced enemy counter.

## Tower progression

Each tower progresses from tier I to veteran tier II, then chooses one of two tier III specializations. Prices below are the additional cost. Specializations are mutually exclusive; rebuilding another branch requires selling. Selling returns half of the actual total investment, rounded down. In-flight shots keep their original profile; upgrading retains cooldown progress and does not grant an instant free shot.

| Base tower | Veteran upgrade | Specialization A | Specialization B |
| --- | --- | --- | --- |
| MG nest, $75 | $90: faster fire and reach | $180 Suppressor: rapid fire, brief single-target slow | $190 AP gun: reach and light-armor damage |
| Anti-tank gun, $125 | $140: heavier shells, faster reload | $250 Tank hunter: heavy armor and long reach | $235 Field howitzer: faster shells with a small blast |
| Mortar, $150 | $160: blast size and loading | $250 Siege mortar: heavy shells and stronger armor damage | $230 Barrage mortar: wide blast and group suppression |

Suppression slows movement rather than blocking the road. The strongest active slow applies; multipliers never multiply into a permanent stop. Amber health bars show the effect. Role-specific damage and targeting remain important: MG-only defenses cannot substitute for anti-tank guns, and narrow single-target fire is inefficient against clustered assaults.

## Implementation and verification

`CampaignBuilder.Build()` authors the definitions, upgrade graph and scene references. Blender generation retains all seven editable actor models and batches static parts by material while preserving moving limbs, carried weapons and wheel details. `WavePlanner` uses a private seeded random generator; presentation never awards damage or funds. Enemy hit/fall/destruction motion stays on miniature children, and retired actors are reused.

The Kingdom Rush reference informed upgrade specializations, complementary counters and readable wave planning. See [Ironhide's original game description](https://www.ironhidegames.com/Games/kingdom-rush). No Kingdom Rush assets or code are used. Exact balance/native results live in `VALIDATION.md` and `Tools/Validation/last-run.json`.

## Playtest interface

The top left shows lives, supplies and the current wave. The top right holds speed, pause/resume, ambience mute, help and a small Options panel for restarting or returning to the menu. Build from the small bottom dock; click a deployed tower to inspect upgrades. Click ground, click the tower again, use the close button, right-click or press Escape to deselect. Escape cancels selection before pausing. Wave intel is available beside Send wave; it is not a persistent briefing. Camera buttons and shortcuts are in Help. All upgrade tiers and specializations now have distinct authored models. Unaffordable upgrades show muted styling, a lock and the missing funds; they cannot be purchased. Starting another wave stays disabled during pause.
