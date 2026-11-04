# Verification scripts

Pipeline's `script run` executes `TheLineHolds/AgentScripts/VerifyPrototype.cs` in Edit Mode. It uses real authored assets and the actual simulation to check the economy, placement failures, pause, wave gates, defeat priority, rating thresholds, and squad targeting/damage rules. `CheckBalance.cs` separately verifies two complete winning mixed layouts and three losing strategies (MG-only, lone opening gun, and no reinvestment). Run it with `--file AgentScripts/CheckBalance.cs --entry CheckBalance.All`.

`CheckLifecycle.cs` and `Playthrough.cs` are C# **scripts**, run with `script eval --file` in either Play Mode or a development Player. They exercise real frame-driven controllers. `Playthrough.cs` runs the full match without awarding funds, skipping waves, or altering enemy health. It takes about 4:45 at normal speed, plus preparation.

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

`CheckInput.cs` is an Editor Pipeline `script run` check that injects mouse/keyboard device events across actual Play Mode frames. It verifies the full Input System path through the HUD and world controls without invoking their callbacks directly. It temporarily isolates hardware devices and restores devices/settings in `finally`. Run it after entering Play Mode:

```sh
unity command script run --project-path "$PWD/TheLineHolds" --file AgentScripts/CheckInput.cs --entry CheckInput.All
```
