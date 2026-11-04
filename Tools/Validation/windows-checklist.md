# Windows prototype check

Windows runtime testing is deferred for the current macOS prototype and does not block its completion. The macOS cross-build does not establish Windows runtime compatibility. Run this check on an actual Windows desktop when Windows becomes a release goal, before a Windows release or marking Windows verified.

The agreed next test environment is a Windows VM when the user is ready. Record the VM software, Windows version and architecture, virtual GPU, and whether the x86-64 build ran through emulation. Report a VM pass as validation of that environment; physical Windows hardware coverage can follow before release. No VM installation or setup is required for the current prototype.

1. Copy the entire `TheLineHolds/Builds/Windows/` folder to Windows, including the `.exe`, `UnityPlayer.dll`, and `_Data` folder. Launch `The Line Holds.exe`.
2. Choose **Deploy**. Confirm 300 funds, 20 lives, and 10 empty pads.
3. Choose the machine gun and place it on pads **04** and **05**, then place a mortar on **07**. Funds should reach zero. Right-click to cancel placement.
4. Start wave 1. During combat, confirm firing, impacts, motion, sound, and kills awarding funds. Use Escape to pause. Confirm enemies and projectiles freeze; Resume continues them.
5. Between waves, buy anti-tank guns on **08** and **09** as funds permit, then machine guns on **02** and additional free pads. Each new wave waits for Start Wave.
6. Select a tower and sell it. Confirm half its purchase cost is returned, rounded down. Occupied or unaffordable purchases must not spend funds.
7. Finish all five waves. Check lives, kills + escapes = 74, waves completed = 5, and the correct star rating (18–20 / 10–17 / 1–9 lives).
8. Retry. Confirm 300 funds, 20 lives, no towers, no enemies/projectiles, and counters reset. Return to Main Menu, then Deploy again.
9. Start a match with no defenses. Confirm losing all lives ends the match immediately, stops spawns, and offers Retry and Main Menu.
10. Check wheel zoom, right-click cancellation, Escape cancellation before pause, window resizing at 1280×720 or larger, audio, and Quit.

Record Windows version, CPU/GPU, build version, result, and any issues in `VALIDATION.md`. Player logs are normally under `%USERPROFILE%\AppData\LocalLow\The Line Holds\The Line Holds\Player.log`.
