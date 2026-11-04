# The Line Holds — Prototype

## Goal

Build one complete, replayable WWII tower defense level to test placement, tower roles, pacing, and the diorama presentation. Aim for a 5–10 minute match. All numbers below are starting values to tune through playtesting.

## Core loop

1. Inspect the map and spend starting funds on defenses.
2. Start the next wave when ready.
3. Defenses fire automatically as enemies advance; build or sell defenses during combat.
4. Earn funds from kills and completed waves, then prepare for the next wave.
5. Survive five waves and receive a rating based on remaining lives, or lose when lives reach zero.

## Single map and camera

- One compact countryside battlefield with a winding road, one enemy entrance, and one defended exit.
- One fixed route, following authored waypoints. Towers never block or change the route.
- Two or three terrain tiers connected by gentle road ramps, with roughly 10 fixed build pads distributed along the route.
- Tiering creates the miniature landscape and placement choices. Height grants no damage or range bonuses in the prototype; terrain and props do not block shots.
- Fixed elevated three-quarter camera with limited zoom. Keep the whole battlefield visible at the default zoom; omit camera rotation and panning initially.
- Keep enemies and build pads readable: avoid trees, cliffs, or buildings obscuring the road from this camera angle.

## Visual direction

- A WWII diorama: believable proportions, muted painted surfaces, earthy colors, and readable miniature silhouettes.
- Follow the supplied references for layered earth, exposed dark base edges, trenches, timber, grass tufts, scattered trees, and small military details.
- Present the battlefield as a self-contained terrain slab against a simple neutral background. No surrounding room or tabletop simulation is needed.
- Use a restrained set of reusable props, soft shadows, and simple muzzle flashes, impacts, and smoke. Keep combat readable over decorative detail.
- Placeholder models are acceptable for the first playable version. Establish a representative diorama pass before judging the visual direction; exact historical units and factions can be chosen later.

## Enemies

Use two ground enemy types. Both follow the same route and have health, movement speed, and a cash reward.

| Enemy | Role | Starting health | Speed | Kill reward |
| --- | --- | --- | --- | --- |
| Infantry | Common, lightly protected target | 40 | Normal | 10 |
| Light tank | Slower, tougher target | 200 | 60% of infantry | 30 |

- No enemy attacks, special abilities, or resistance system initially. Enemies threaten the player by reaching the exit.
- Every escaped enemy costs one life and is removed. Escapes grant no kill reward.
- No flying enemies, bosses, or physics-based vehicle simulation.

## Three defenses

Defenses occupy one build pad each. All three are available from the start, have circular range, and automatically prioritize the enemy furthest along the route within range.

| Defense | Role | Starting cost | Behavior |
| --- | --- | --- | --- |
| Machine-gun nest | Affordable infantry control | 75 | Rapid, low-damage single-target fire; short range |
| Anti-tank gun | Tough single targets | 125 | Slow, high-damage single-target fire; long range |
| Mortar | Groups of enemies | 150 | Slow explosive shots with a small splash radius; medium range |

- Show placement previews, cost, and range before purchase. Reject occupied pads and purchases the player cannot afford without spending funds.
- Let the player select a defense to see its type and range, or sell it for 50% of its purchase cost, rounded down.
- No upgrades, manual aiming, ammunition, friendly fire, or tower destruction.
- Use simple damage rules and visual projectiles; ballistic realism is unnecessary. A destroyed or escaped target must not cause errors or duplicate rewards.

## Economy and five waves

- Start with **300 funds** and **20 lives**. No passive income or spending lives for currency.
- Award **50 funds** after each completed wave. Funds persist only within the match.
- Each wave uses an authored spawn list, with units spaced apart. Later waves increase pressure through numbers, composition, and tighter spacing rather than hidden health scaling.

| Wave | Starting composition | Purpose |
| --- | --- | --- |
| 1 | 8 infantry | Learn placement and basic combat |
| 2 | 12 infantry | Test sustained coverage |
| 3 | 8 infantry, 2 light tanks | Introduce durable enemies |
| 4 | 16 infantry, 3 light tanks | Test mixed defenses and splash damage |
| 5 | 20 infantry, 5 light tanks | Final combined test |

- The first wave waits for player input. Each subsequent wave also waits for a **Start Wave** click, with no preparation timer.
- A wave ends only after its entire spawn list has appeared and every enemy has been killed or escaped. Waves do not overlap.
- Adjust health, costs, fire rates, range, counts, and spacing until multiple sensible defense layouts can finish the map.

## Win, loss, and results

- **Victory:** resolve wave five with at least one life remaining.
- **Defeat:** lives reach zero; stop spawning and end the match immediately. Defeat takes priority if the last escape also ends the final wave.
- Grade a victory by lives remaining: **3 stars for 18–20**, **2 stars for 10–17**, and **1 star for 1–9**.
- Results show victory/defeat, stars, lives remaining, waves completed, enemies killed, and enemies escaped.
- Offer **Retry** and **Main Menu**. Retry resets all match state, including towers, funds, counters, and enemies.

## Menus, HUD, and controls

- Start menu: game title, **Play**, and **Quit**.
- HUD: lives, funds, wave number out of five, enemy count remaining in the current wave, three defense buttons with costs, and **Start Wave**.
- Mouse: select a defense type, click a pad to place it, or click an existing defense to inspect/sell it. Right-click cancels placement; mouse wheel controls limited zoom.
- Escape cancels placement first; otherwise toggles pause. Pause freezes combat, movement, spawning, and projectiles.
- Pause menu: **Resume**, **Restart**, and **Main Menu**.
- Include brief on-screen instructions and distinct placement, firing, impact, escape, and result sounds. A full settings menu and soundtrack can wait.

## Build and extension patterns

- Use the Unity 6.6 URP, C#, Blender, and CLI workflow described in AGENTS.md. Evaluate the current prototype on macOS. Keep the Windows build target and cross-build output, with Windows runtime testing deferred until Windows becomes a release goal.
- Keep enemy, defense, and wave definitions in editable data assets, so tuning and adding content do not require rewriting core systems.
- Separate match state, spawning, route movement, combat, economy, and UI responsibilities. Use reusable prefabs and a small, direct architecture.
- Keep core decisions deterministic where practical: stable targeting, explicit damage, and authored waves. Avoid building a generic framework before the prototype needs it.

## Build order and acceptance

1. **Playable loop:** placeholder map, one route, one enemy, one defense, placement, funds, lives, and restart.
2. **Complete match:** all three defenses, both enemies, five waves, menus, pause, and graded results.
3. **Presentation pass:** tiered terrain, camera, representative assets, materials, effects, and sound.
4. **Playtest and tune:** verify the full loop, address readability and balance, and produce platform builds.

The prototype is ready to evaluate when:

- A new player can launch, understand the basic controls, play all five waves, and retry without editor intervention.
- Good placement can win; neglecting defenses can lose; victory ratings reflect lives remaining correctly.
- Each defense has a useful role, and buying, selling, kills, and escapes update counters exactly once.
- Pause, restart, and returning to the menu leave no stale enemies, projectiles, towers, or match state.
- The tiered diorama is readable from the gameplay camera, and combat remains responsive during the final wave.
- A complete match is checked in a standalone Mac build. Windows runtime verification is deferred and does not block this prototype's completion; check a complete match on Windows before a Windows release or calling that platform verified.

## Outside this prototype

Additional maps, campaigns, progression, unlocks, upgrades, save games, multiplayer, procedural maps, player-created mazes, advanced armor simulation, destructible terrain, and cinematic sequences.
