# The Line Holds

A single-map WWII tower defense prototype built with Unity **6000.6.7f1**, URP 17.6, C#, and original Blender miniatures.

To play without the Unity Editor, unzip `TheLineHolds/Builds/Packages/The-Line-Holds-macOS.zip` and open **The Line Holds.app**. The unpackaged local app is also available at `TheLineHolds/Builds/macOS/The Line Holds.app`. Choose **Deploy** to begin. The diorama pass adds rolling terrain, a worn road, leafy vegetation, refined miniatures, independent gun traverse, and infantry strides. The default window is now 1920×1080; smaller displays use a fitting size, and an existing saved window size migrates once. This is the current macOS prototype; Windows testing will use a Windows VM when you are ready.

Open `TheLineHolds/` in Unity. The playable scene is `Assets/TheLineHolds/Scenes/Countryside.unity`. Press Play, then **Deploy**. Buy a defense from the supply bar and click a cleared gun site. Start each wave when ready. Click an occupied emplacement to inspect its range or sell it for half its price. Right-click cancels placement; Escape cancels placement first, otherwise pauses; the mouse wheel zooms from a close view to the full map. Middle-drag, Command+left-drag on macOS, Ctrl+left-drag on Windows, or Q/E rotates through 360 degrees; Shift+middle-drag or WASD pans; R restores the overview. The view strip also has rotate, zoom, and reset buttons.

Ten attacks mix irregular patrols, runner streams, infantry clusters, vehicle columns and combined assaults. Seven enemy types have distinct speeds and durability: riflemen, runners, assault troops, trucks, armored cars, light tanks and heavy tanks. The next attack's composition and tactical hint are shown before you call it. Formations and individual speeds vary within that announced plan. Vehicles retain safe spacing through bends.

Click a deployed tower to buy its veteran upgrade, then choose one of two specializations. Machine guns can suppress infantry or use AP rounds; anti-tank guns can become tank hunters or field howitzers; mortars can specialize in heavy shells or group suppression. Amber health bars mark suppressed enemies. Selling returns half of the actual investment. Use the header's 1x/2x control to change pace.

The match starts with 300 funds and 20 lives. Kill rewards depend on enemy type; completed waves pay 70–160 funds. Reinvest in overlapping coverage and counter the announced attack. Survive all ten waves to win. Every escape costs one life. Victory earns three stars at 18–20 lives, two at 10–17, and one at 1–9. See [GAMEPLAY.md](GAMEPLAY.md) for the current campaign and upgrade tables.

## Project structure

- `Scripts/Core`: editable unit/wave definitions, authored route, and deterministic match simulation. Small `Economy`, `WaveSpawner`, `RouteMovement`, and `CombatSystem` classes handle their respective rules; `MatchState` coordinates their order and owns outcomes.
- `Scripts/Presentation`: world views, mouse controls, uGUI/TMP menus/HUD, and sound cues.
- `Data`: enemy, defense, and spawn-list assets; tune these directly in the Inspector.
- `Prefabs` and `Art/Models`: reusable miniatures and Blender FBX exports, imported at metre scale.
- `Editor/PrototypeBuilder.cs`: repeatable scene, asset, sound, and platform build generation. `DioramaBuilder`, `DioramaSurface`, and `DioramaVegetation` generate the current map treatment.
- `AgentScripts/VerifyPrototype.cs`: focused checks against the actual simulation and authored data.
- `Tools/Blender/generate_assets.py` and `miniatures.blend`: original editable model sources.

## Repeatable commands

From the repository root, with Unity CLI installed:

```sh
unity open "$PWD/TheLineHolds"
unity status --until-ready --project-path "$PWD/TheLineHolds"
unity command editor set_auto_tick --project-path "$PWD/TheLineHolds" --enabled true
unity command script eval --project-path "$PWD/TheLineHolds" 'TheLineHolds.Editor.PrototypeBuilder.Build();'
unity command script run --project-path "$PWD/TheLineHolds" --file AgentScripts/VerifyPrototype.cs --entry VerifyPrototype.All
unity command script run --project-path "$PWD/TheLineHolds" --file AgentScripts/CheckBalance.cs --entry CheckBalance.All
unity command script eval --project-path "$PWD/TheLineHolds" 'TheLineHolds.Editor.PrototypeBuilder.BuildMac();'
unity command script eval --project-path "$PWD/TheLineHolds" 'TheLineHolds.Editor.PrototypeBuilder.BuildWindows();'
```

Launch the existing Mac release without rebuilding:

```sh
unity build run "$PWD/TheLineHolds" --build-path "$PWD/TheLineHolds/Builds/macOS/The Line Holds.app"
```

Build commands also appear under **The Line Holds → Build** in Unity. Outputs are separated under `TheLineHolds/Builds/macOS/` and `TheLineHolds/Builds/Windows/`. Windows requires its desktop build module/toolchain. The current prototype evaluation platform is macOS; Windows runtime testing is deferred and does not block prototype completion. Confirm a full match on Windows before a Windows release or treating that platform as verified.

Regenerate the models:

```sh
/Applications/Blender.app/Contents/MacOS/Blender --background --python Tools/Blender/generate_assets.py -- "$PWD"
```

Then run **The Line Holds → Rebuild Prototype** to update the material assignments, prefabs, data, and scene. Rebuild resets authored tuning to the defaults in the builder; commit custom tuning first. Runtime restarting creates a fresh match and removes enemies, towers, projectiles, effects, and audio from the preceding attempt.

Git LFS tracks Blender, FBX, audio, font, and image binaries. Unity caches and build output are ignored. No cloud service is needed for the prototype.

Verification scripts and the Windows playtest checklist are in `Tools/Validation/`. Run `python3 Tools/Validation/package_builds.py` after successful builds to create shareable ZIPs under `TheLineHolds/Builds/Packages/`; Windows backup/symbol folders are excluded. See `VALIDATION.md` for the recorded results and deferred Windows check.

For visual authoring without resetting gameplay data, stop Play Mode and run:

```sh
unity command script eval --project-path "$PWD/TheLineHolds" 'TheLineHolds.Editor.DioramaBuilder.Map();'
```

See `ART_DIRECTION.md` for palette, lighting, generator details, and the visual target. Steps 2a–2c are implemented; review their captures under `TheLineHolds/Screenshots/diorama-*.png` before the next composition pass.
