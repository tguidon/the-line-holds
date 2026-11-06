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
- Elevated three-quarter overview with full 360-degree orbit, orthographic zoom from 5.5 to 18 (default 13.5), and bounded ground-plane panning. Keep the whole battlefield visible at the default view; provide a reset action. Labels and health bars remain readable while rotating.
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
| Infantry | Six-soldier squads; each soldier has its own health | 80 | 1.5 m/s | 4 |
| Light tank | Armored target; resists light weapons and mortar fragments | 650 | 1.05 m/s | 16 |

- Simple armor multipliers distinguish weapon roles. No enemy attacks or special abilities. Enemies threaten the player by reaching the exit.
- Every escaped enemy costs one life and is removed. Escapes grant no kill reward.
- Tank sections use 5.5-second intervals. A minimum 4.8 m route gap also delays vehicles across section boundaries, so tank models stay separate through bends. Extend the wave rather than spawning overlapping vehicles.
- No flying enemies, bosses, or physics-based vehicle simulation.

## Three defenses

Defenses occupy one build pad each. All three are available from the start, have circular range, and use deterministic targeting within circular range. Equal priorities favor the enemy furthest along the route, then its stable ID.

| Defense | Role | Starting cost | Behavior |
| --- | --- | --- | --- |
| Machine-gun nest | Affordable infantry control | 75 | 8 damage every 0.32 s; 3.3 m range; single-target; 15% damage against armor |
| Anti-tank gun | Tough single targets | 125 | 95 damage every 2 s; 5.8 m range; prioritizes armor; 60% damage against infantry |
| Mortar | Groups of enemies | 150 | 42 damage every 3.8 s; 4.8 m range; targets the densest predicted cluster; 1.4 m splash; 35% damage against armor |

- Show placement previews, cost, and range before purchase. Reject occupied pads and purchases the player cannot afford without spending funds.
- Let the player select a defense to see its type and range, or sell it for 50% of its purchase cost, rounded down.
- No upgrades, manual aiming, ammunition, friendly fire, or tower destruction.
- Use simple damage rules and visual projectiles; ballistic realism is unnecessary. A destroyed or escaped target must not cause errors or duplicate rewards.

## Economy and five waves

- Start with **300 funds** and **20 lives**. No passive income or spending lives for currency.
- Award **70, 80, 90, 100, and 110 funds** after waves 1–5. This is 60 plus 10 times the completed wave number. Funds persist only within the match.
- Each wave uses an authored squad/vehicle list. Six infantry enter in two lanes, 0.2 seconds apart; every member is independently targetable and costs one life if it escapes. Mortars can damage multiple members; guns hit one member per shot. Squads are separated by short gaps, shrinking from 5 to 0.8 seconds. Later waves increase pressure through numbers, composition, and tighter spacing rather than hidden health scaling.

| Wave | Starting composition | Purpose |
| --- | --- | --- |
| 1 | 3 squads (18 infantry) | Establish overlapping coverage |
| 2 | 5 squads (30 infantry) | Reward splash and sustained coverage |
| 3 | 6 squads, 4 light tanks | Introduce armor priority |
| 4 | 8 squads, 8 light tanks | Demand layered mixed defenses |
| 5 | 10 squads, 12 light tanks | Final combined assault |

- The first wave waits for player input. Each subsequent wave also waits for a **Start Wave** click, with no preparation timer.
- A wave ends only after its entire spawn list has appeared and every enemy has been killed or escaped. Waves do not overlap.
- Multiple mixed layouts must win. A lone opening gun, an all-machine-gun layout, and a starting layout that never reinvests must lose. Fully deployed mixed layouts must still engage rear defenses, with final-wave enemies reaching at least 75% of the route. Keep these outcomes in a repeatable balance check. This is the single baseline difficulty; easier and harder modes come later.

## Win, loss, and results

- **Victory:** resolve wave five with at least one life remaining.
- **Defeat:** lives reach zero; stop spawning and end the match immediately. Defeat takes priority if the last escape also ends the final wave.
- Grade a victory by lives remaining: **3 stars for 18–20**, **2 stars for 10–17**, and **1 star for 1–9**.
- Results show victory/defeat, stars, lives remaining, waves completed, enemies killed, and enemies escaped.
- Offer **Retry** and **Main Menu**. Retry resets all match state, including towers, funds, counters, and enemies.

## Menus, HUD, and controls

- Start menu: game title, **Play**, and **Quit**.
- HUD: lives, funds, wave number out of five, enemy count remaining in the current wave, three defense buttons with costs, and **Start Wave**.
- Mouse: select a defense type, click a pad to place it, or click an existing defense to inspect/sell it. Right-click cancels placement; mouse wheel zooms. Middle-drag, Command+left-drag on macOS, Ctrl+left-drag on Windows, or Q/E rotates; Shift+middle-drag or WASD pans; R restores the overview. On-screen rotate, zoom, and reset buttons provide mouse-only access. Scrolling over UI does not move the camera.
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
