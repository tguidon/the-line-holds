# Verification scripts

Pipeline's `script run` executes `TheLineHolds/AgentScripts/VerifyPrototype.cs` in Edit Mode. It uses real authored assets and the actual simulation to check the economy, placement failures, pause, wave gates, defeat priority, rating thresholds, and squad targeting/damage rules. `CheckBalance.cs` separately verifies three complete winning mixed layouts (including five MG/three AT/two mortar) and three losing strategies (MG-only, lone opening gun, and no reinvestment). Run it with `--file AgentScripts/CheckBalance.cs --entry CheckBalance.All`.

`CheckLifecycle.cs` and `Playthrough.cs` are C# **scripts**, run with `script eval --file` in either Play Mode or a development Player. They exercise real frame-driven controllers. `Playthrough.cs` runs the full match without awarding funds, skipping waves, or altering enemy health. It takes about 5:15 at normal speed, plus preparation.

The runtime Pipeline is enabled for development builds only. Release builds exclude it. Build a QA player with:

```sh
unity command build run --project-path "$PWD/TheLineHolds" --target StandaloneOSX --output-path 'Builds/macOS-QA/The Line Holds.app' --options '["Development"]' --confirm true
unity build run "$PWD/TheLineHolds" --build-path "$PWD/TheLineHolds/Builds/macOS-QA/The Line Holds.app"
unity command script eval --runtime 'The Line Holds' --file "$PWD/TheLineHolds/AgentScripts/CheckLifecycle.cs" --timeout 30
unity command script eval --runtime 'The Line Holds' --file "$PWD/TheLineHolds/AgentScripts/Playthrough.cs" --timeout 600 --detach
```

Record the returned job ID and use `unity job status <job-id> --runtime 'The Line Holds'` to inspect it. The evaluation holds that player's Pipeline command queue until it finishes; other runtime commands can time out behind it. Job status does not enter that queue. The game keeps rendering throughout the asynchronous test.

`ConfigureGameView.cs` sets a fixed 1600×900 Game view through Editor APIs for repeatable screenshots. All inspections and interactions use the Unity CLI; no computer-use automation is required.

`CheckUI.cs` verifies menu/HUD screen raycasts and their pointer-click callbacks. This checks that the overlay, button hierarchy, EventSystem, and button listeners are connected. It does not claim to simulate OS mouse focus. Native pointer injection into an unfocused player can be discarded by Unity's input focus handling.

`CheckInput.cs` is an Editor Pipeline `script run` check that injects mouse/keyboard device events across actual Play Mode frames. It verifies the full Input System path through the HUD, world placement, 360-degree map orbit and Command/Ctrl-drag protection, zoom limits, panning, view reset, and placement from the changed camera without invoking their callbacks directly. It temporarily isolates hardware devices and editor focus routing, then restores devices/settings in `finally`. Run it after entering Play Mode:

```sh
unity command script run --project-path "$PWD/TheLineHolds" --file AgentScripts/CheckInput.cs --entry CheckInput.All
```

`CheckFeedback.cs` checks that the unified road/meadow mesh follows the shared movement surface within 2.5 cm and that the legacy road overlay is absent and five HUD widths. Run its `Geometry` entry in Edit Mode and its `Hud` entry in Play Mode. `CheckTankSpacing.All` checks actual tank dimensions against centre spacing through the entire route, including a compressed-spawn stress case.

The balance check also requires all mixed layouts to fill ten pads before wave 5. Final-wave enemies must reach at least 75% of the route and trigger rear defenses. This prevents a win-only check from accepting a dull front-half clear.

`CheckDiorama.Map` checks terrain density, normals, materials, grounded pad centres, smooth route turns, and default window settings in Edit Mode. `CheckDiorama.Towers` and `CheckDiorama.Units` run in Play Mode and verify imported traverse axes, fixed sandbags, distance-driven stride, and unchanged unit-root position. `ShowTowers.cs` and `ShowUnits.cs` stage temporary visual reviews in Play Mode; stop Play Mode to remove them. They are visual showcases, not gameplay evidence.

`CheckNativeVisuals.cs` verifies final prefab forward axes, persisted terrain tangents, and all four saved Volume overrides in a standalone QA player, then captures grade-on/off, overview combat, and close combat. `compare_captures.py` checks a visible grade difference without external Python dependencies. Native resized views are covered by `CheckResize.cs`.

`CheckFoliage.Assets` checks complete, staggered, tangential sandbag walls and textured leaf meshes. `CheckNativeFoliage.cs` verifies leaf textures and vertex attributes on GPU-only imported meshes in a development player, captures real combat, and measures a three-second frame-rate sample. `ShowFoliage.cs` temporarily disables the camera controller for a 2.8-size close art review; stop Play Mode to remove the override.

`CheckHouses.All` raycasts a temporary clone of the actual imported house to check a high central ridge, symmetric descending slopes, and continuous roof samples. It also limits material-batched renderer count. `ShowHouses.cs` stages two temporary review orientations in Play Mode. `CheckNativeHouses.cs` verifies both real map houses and captures overview/close native views.

`CheckMapComposition.All` checks unchanged tower anchors, removed pedestals/labels, clean field-prop placement roots, textured soil horizons and matching endpoint landmarks. It temporarily adds colliders to actual combined decoration meshes and sweeps three lanes at three heights along the full route; colliders are removed in `finally`. `CheckNativeComposition.cs` verifies the final map in a QA player and captures overview, orbit, entrance, checkpoint and soil close-ups. Its temporary close cameras are restored before returning to the main menu. `CheckResize.cs` restores the original window size after its four-size test.

`CheckWeaponFeedback.Assets` verifies all 21 short mono sound variations, non-silent sample peaks, edge fades and muzzle/recoil anchors. `Events` compares exactly-once typed notifications with legacy fire/impact cues and verifies the simulation origin. `Play` checks all three muzzle directions, elevated mortar tube, barrel-only recoil/recovery/freeze, fixed fortifications/carriage, stable projectile/emitter counts, live voices, pause, the shared gain layer and cleanup. `ShowWeaponFeedback.Stage` creates a temporary staged firing frame; stop Play Mode to remove it.

`CheckNativeWeapons.cs` verifies the sound bank, supported effect materials, shipped attribution, repeated-shot pool stability, actual mixed audio output, pause/reset and native captures. Run it with an absolute `--file` path and `--runtime 'The Line Holds'`. `Playthrough.cs` now records real fire/impact counts for all three roles, peak live particle/voice counts, mixed output peak and fixed projectile allocation through the entire match. Its extra close capture temporarily adjusts and restores the camera; it does not alter the simulation. Keep `Time.timeScale=1` for audio/pacing validation. Signal checks do not replace the user's listening review.

`CheckCampaign.Rules` verifies all ten announced compositions, seven speed roles, deterministic seeded formation variety, upgrade branches/prices/guards/refunds and mixed-speed vehicle spacing. `CheckCampaign.Balance` runs 18 strategy/seed scenarios and asserts that upgraded mixed layouts win and engage rear defenses while static, MG-only and neglected defenses fail. Structured results are in `campaign-balance.json`.

For the current ten-wave campaign, use `CampaignPlaythrough.cs` rather than the historical five-wave `Playthrough.cs`. It uses earned funds with the real frame-driven game, verifies ten-wave completion and all seven archetypes, and records upgrades, final pressure, actor/corpse/voice budgets and audio output. It accelerates time fourfold for verification, with a three-second normal-speed busy-scene sample; it does not constitute human pacing/fun playtesting. `NativeCampaignChecks.cs` covers native actor/root/shader/lifecycle checks and worst-case seven-type intel plus both upgrade choices at four window sizes, restoring the original window and authored wave array afterward. `CheckCampaignUI.All` and `CheckInput.All` cover UI callbacks and frame-driven mouse/keyboard paths separately. All temporary review actors are removed.
