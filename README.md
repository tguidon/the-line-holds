# The Line Holds

A two-map WWII tower defense prototype built with Unity **6000.6.7f1**, URP 17.6, C#, and original Blender miniatures.

The local two-map macOS development build is `TheLineHolds/Builds/macOS-VehicleTravel/The Line Holds.app`. Open it and choose **Countryside** or **Winter Forest**. Windows validation is build-only.

Open `TheLineHolds/` in Unity. The startup scene is `Assets/TheLineHolds/Scenes/MainMenu.unity`. Its field-manual cover shows a photograph of each battlefield. Press Play, select either battlefield from the menu, choose a defense from the build dock and click a cleared gun site. Click a tower to upgrade or sell it. Pausing freezes combat while you build, inspect, upgrade, sell or move the camera. The corner pause control resumes play; Options holds Restart and Main menu. Unaffordable upgrades show a lock and their shortfall. Escape cancels selection first, otherwise pauses. Middle-drag, Command+left-drag on macOS, Ctrl+left-drag on Windows, or Q/E rotates the map; Shift+middle-drag or WASD pans; the wheel zooms; R fits the complete route. Help contains camera buttons and shortcuts.

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

Download desktop builds from [GitHub Releases](https://github.com/tguidon/the-line-holds/releases). Maintainers can follow the [desktop release and Apple notarization guide](Tools/Validation/README.md#desktop-releases).

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

Build commands also appear under **The Line Holds → Build** in Unity. Outputs are separated under `TheLineHolds/Builds/macOS/` and `TheLineHolds/Builds/Windows/`. Windows requires its desktop build module/toolchain. Run gameplay checks on macOS. A successful Windows x64 build is sufficient for release.

Regenerate the models:

```sh
/Applications/Blender.app/Contents/MacOS/Blender --background --python Tools/Blender/generate_assets.py -- "$PWD"
```

Then run **The Line Holds → Rebuild Prototype** to update the material assignments, prefabs, data, and scene. Rebuild resets authored tuning to the defaults in the builder; commit custom tuning first. Runtime restarting creates a fresh match and removes enemies, towers, projectiles, effects, and audio from the preceding attempt.

Git LFS tracks Blender, FBX, audio, font, and image binaries. Unity caches and build output are ignored. No cloud service is needed for the prototype.

Verification scripts and the Windows release checklist are in `Tools/Validation/`. Run `python3 Tools/Validation/package_builds.py` after successful builds to create shareable ZIPs under `TheLineHolds/Builds/Packages/`; Windows backup/symbol folders are excluded. See `VALIDATION.md` for the recorded results and deferred Windows check.

For visual authoring without resetting gameplay data, stop Play Mode and run:

```sh
unity command script eval --project-path "$PWD/TheLineHolds" 'TheLineHolds.Editor.DioramaBuilder.Map();'
```

For a new level, start with the gameplay reference, guidance on distinct routes and scenery density, and examples for snow, town and beach maps in the [map art guide](ART_DIRECTION.md). The repo includes the [battlefield art skill](.agents/skills/battlefield-art/SKILL.md) for authoring and reviewing maps against that guide.

### Winter Forest

The logging track doubles back around a rock outcrop before reaching the radio hut. Foxholes, sleds, wheel ruts and blowing powder dress the map. The ten staked clearings accept the same defenses as Countryside.

To rebuild the scenery, run **The Line Holds → Visuals → Build Winter Forest** in Edit Mode. The build uses the current Countryside campaign references and writes `Scenes/WinterForest.unity` and its own assets under `Art/WinterForest/`. Regenerate the original models with Blender and `Tools/Blender/winter_forest.py`, passing the repository root after `--`. Run `AgentScripts/CheckWinterCampaign.cs` with entry `CheckWinterCampaign.Compare` for the matched-seed balance comparison.

[Opening patrol](TheLineHolds/Screenshots/winter-forest-patrol.png) · [Wave nine armor](TheLineHolds/Screenshots/winter-forest-armor.png)

### Saint-Aubin route prototype

Choose **Saint-Aubin** in the Unity project's main menu to inspect the [town layout](TheLineHolds/Screenshots/saint-aubin-layout.png). The intact houses and roofless plots reserve building footprints for the spring town art pass. The ten round sites are buildable.

In Edit Mode, **The Line Holds → Visuals → Build Saint-Aubin blockout** regenerates its scene and paving from the movement paths. Run `AgentScripts/CheckTownLayout.cs` with entry `CheckTownLayout.All` to check the junction, pad coverage, and wave entrance patterns. Difficulty comparison and final scenery follow route approval.
