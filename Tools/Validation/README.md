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

November 10 polish: `BuildPolish.All` invokes the repeatable `PlaytestPolishBuilder.Build()` in Edit Mode. `CheckDeselect.All` reproduces the previously ignored ground/repeated clicks; it failed before the fix. `ReviewPolish.Check` verifies upgrade callbacks/costs, selection priority, ambient playback/mute and removal of slab objects in Play Mode. `CheckInput.All` now follows the compact HUD and opens/closes Help for camera buttons, still driving actual injected device events. `CheckOrganic.All` verifies continuous road-bin spread for three seeds. `ProbePasture.All` compares actual saved pigment at the core/apron join; it fails on the clamped field-mask regression.

`NativePolish.cs` verifies native ambience, all nine distinct upgrade prefab references, no whole-vehicle brightening, localized impact feedback and constant explosion emitter count. It captures a temporary funded tier showcase and staged hit/explosion close-up; these are presentation checks, not balance evidence. `NativeCampaignChecks.cs` verifies four real window sizes with worst-case intel and two branch buttons. `CampaignPlaythrough.cs` remains the earned-funds, frame-driven ten-wave check. Its accelerated match and three-second normal-speed sample do not substitute for the user's playtest. The final pasture/fire-texture/debris corrections affect presentation only and are probed separately in a rebuilt player.

The pre-November-10 `CheckUI.cs` and `CheckCampaignUI.cs` retain the old HUD label/layout fixtures for historical reference. Current UI acceptance uses `ReviewPolish.Check`, updated `CheckInput.All`, `NativePolish.cs` and `NativeCampaignChecks.cs` as listed above.

`CheckOverview.All` projects the actual route and endpoint margins across five camera aspects; it failed at 4:3 before the fit calculation. `NativeOverview.cs` checks startup, untouched-overview resize and Reset at five native window sizes, including 1516×1324 (the reported window proportions), and verifies manual pan/orbit/zoom are not reset by resize. `NativeDockIcon.cs` is a macOS-only read of the running NSApplication icon, saved as TIFF for inspection. `resolve_macos_icon.swift <app-path> <png-output>` reads the OS-resolved bundle icon separately; no desktop UI automation is used. Build callbacks register macOS bundle metadata before CLI launch.

### Tactical pause and countryside review (November 10)

Use `CheckTacticalPause.Orders` and `.Disabled` in Edit Mode, and `.Play` plus `CheckInput.All` in Editor Play Mode. These check paused transactions, actual locked UI clicks, camera input and frozen live enemies/projectiles. `CheckCountryside.All` checks road destinations, grass batches, authored buildings and unchanged core anchors; `ProbePasture.All` checks the pigment seam. Re-run `VerifyPrototype.All`, `CheckCampaign.Rules` and `.Balance` for regression coverage. The latest results are in `tactical-pause-scenery.json`.

To rebuild scenery with Countryside open and Play Mode stopped, run `AgentScripts/RefineCountryside.cs --entry RefineCountryside.All` through Pipeline `script run`. The five new Blender types are generated with `--only StoneCottage,VillageInn,Dairy,ToolShed,VillageWell`. `ShowCountryside` provides Panorama/Village/Farm temporary cameras and Reset. `PrepareSilentReview.All` silences only the isolated editor's audio for concurrent player testing.

Build separate review outputs with `unity command build run --target StandaloneOSX --output-path "$PWD/TheLineHolds/Builds/macOS-Review/The Line Holds.app" --options '[]' --confirm true --project-path "$PWD/TheLineHolds"`, and similarly `StandaloneWindows64` to `Builds/Windows-Review/The Line Holds.exe`. Do not overwrite/register/relaunch the active release while the user is testing. The macOS callback deliberately leaves `macOS-Review` unregistered. Current review builds pass but have not been run natively; the historical full native campaign remains separately recorded.

### Desktop releases

Run these commands from the repository root on the signing Mac. Install the project’s Unity editor version, macOS and Windows standalone modules, Xcode command line tools, and GitHub CLI. Sign in to GitHub with release write access. Use a fresh build directory for each version.

#### Build both players

1. Set the version in **Player Settings → Player → Version** and increment the standalone macOS build number. For 0.0.2 these values are `0.0.2` and `4`. Commit the updated `ProjectSettings.asset` with any release tooling. Check the branch and remote tags before selecting a new version.
2. Set the paths below in the same shell. Open the project in Unity and wait for the editor to be ready.

```sh
version=0.0.2
tag="v$version"
project="$PWD/TheLineHolds"
release="$project/Builds/Release-$version"
app="$release/macOS/The Line Holds.app"
mac_zip="$release/Packages/The-Line-Holds-$version-macOS.zip"
win_zip="$release/Packages/The-Line-Holds-$version-Windows-x64.zip"
unity status --project-path "$project" --until-ready
unity command build run --project-path "$project" --target StandaloneOSX --output-path "$app" --options '[]' --confirm true
unity command build status --project-path "$project" | head -11
```

Wait for the macOS build to report `Succeeded`, then run the Windows build and check its own report. Empty build options produce release players.

```sh
unity command build run --project-path "$project" --target StandaloneWindows64 --output-path "$release/Windows/The Line Holds.exe" --options '[]' --confirm true
unity command build status --project-path "$project" | head -11
```

#### Sign and notarize the macOS app

Use the **Developer ID Application** certificate with its private key in the login keychain. Copy the full identity name from the first command. On a new Mac, use the interactive credentials command once and enter the Apple account details at its prompts. The release script uses the saved Keychain profile.

```sh
security find-identity -v -p codesigning
xcrun notarytool store-credentials TheLineHolds-notary
```

```sh
identity='Developer ID Application: YOUR NAME (TEAMID)'
Tools/Validation/notarize_mac.sh "$app" "$identity" TheLineHolds-notary
```

The script signs nested libraries and plug-ins first, signs the app with hardened runtime and the Unity entitlements, verifies the signature, and submits a ZIP to Apple. Keep the printed submission ID. After Apple accepts the submission, the script staples the ticket to the app, validates the ticket, and checks Gatekeeper distribution policy.

If the connection drops after submission, check the existing submission before resubmitting. An `Invalid` result has a diagnostic log. For an `Accepted` result, staple the same signed app that was submitted. A rebuilt or re-signed app needs a new submission.

```sh
xcrun notarytool info "$submission" --keychain-profile TheLineHolds-notary
xcrun notarytool log "$submission" --keychain-profile TheLineHolds-notary
# After Accepted:
xcrun stapler staple "$app"
xcrun stapler validate "$app"
syspolicy_check distribution "$app"
```

#### Package, verify and publish

Package the stapled app and Windows player. The packager verifies the Mac signature and ticket, preserves bundle metadata, and excludes Windows debug symbols and backup folders. Test the final archives and launch the extracted app. Check version, menu and deployment to a battlefield.

```sh
python3 Tools/Validation/package_builds.py --builds "$release" --version "$version"
unzip -tq "$mac_zip" && unzip -tq "$win_zip"
review_dir=$(mktemp -d)
ditto -x -k "$mac_zip" "$review_dir"
xcrun stapler validate "$review_dir/The Line Holds.app"
syspolicy_check distribution "$review_dir/The Line Holds.app"
open "$review_dir/The Line Holds.app"
shasum -a 256 "$mac_zip" "$win_zip"
```

Tag the exact release commit. Push the source and annotated tag, then create a draft with the two ZIPs attached. Write player-facing changes and installation instructions in a temporary notes file. Replace `$notes` with its path.

```sh
git tag -a "$tag" -m "The Line Holds $version"
git push --atomic origin main "$tag"
gh release create "$tag" "$mac_zip" "$win_zip" --verify-tag --draft --title "The Line Holds $version" --notes-file "$notes"
```

For Windows, the local Mac cross-build and ZIP integrity check are sufficient to publish. Attach that ZIP directly to GitHub Releases. Do not use a GitHub Actions Windows runner for any step. Runtime testing is deferred; leave the Windows player unopened.

Compare the uploaded filenames, byte counts and SHA-256 digests with the local archives before publishing. If a previous attempt created a draft, resume that draft. For a signing-only replacement of an existing Mac release, retain the original source tag and Windows ZIP.

```sh
gh release view "$tag" --json isDraft,assets --jq '{draft:.isDraft,assets:[.assets[]|{name,size,digest}]}'
gh release edit "$tag" --draft=false --latest
gh release view "$tag" --json url,isDraft,tagName,assets --jq '{url,isDraft,tagName,files:[.assets[].name]}'
```
