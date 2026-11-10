# Visual review: steps 2 and 3

November 7, 2026. Scope: terrain/map (2a), tower target (2b), and enemy target (2c). This establishes the next visual baseline for user review. It does not replace the later composition, effects, or gameplay passes.

## Captured evidence

- `TheLineHolds/Screenshots/diorama-map.png`: native 1920×1080 overview.
- `TheLineHolds/Screenshots/diorama-close.png`: native combat close-up with individual infantry, operating guns, health bars, and the worn road.
- `TheLineHolds/Screenshots/diorama-towers.png`: temporary three-tower visual showcase in Editor Play Mode.
- `TheLineHolds/Screenshots/diorama-units.png`: temporary infantry/tank showcase in Editor Play Mode.
- `TheLineHolds/Screenshots/diorama-combat.png`: full-match native combat capture.

Showcases are temporary presentation arrangements, not purchase or balance evidence. Actual match validation uses earned funds and normal controller updates.

## Scoped scorecard

Scores are an internal assessment of these captures, not user approval. Before refers to the previous prototype combat capture. The target here is a coherent, readable indie prototype; further art refinement remains on the roadmap.

| Axis | Before | After | Evidence |
| --- | --- | --- | --- |
| Palette discipline | 3/5 | 3/5 | Muted meadow, earth, olive miniatures, cool backdrop, and established gold UI. No default materials on the touched surface. |
| Style uniformity | 2/5 | 3/5 | Continuous rolling land, worn road, rounded canopy clusters, soft sandbags, and bevelled military miniatures. |
| Lighting depth | 2/5 | 3/5 | Warm sun, cool sky fill, ACES, restrained bloom/vignette, and persisted color grading. |
| Silhouette | 2/5 | 3/5 | Distinct MG, cannon, mortar, infantry, and armor shapes remain readable at overview and close zoom. |
| UI intentionality | 3/5 | 3/5 | Existing UI preserved. Five logical widths pass overlap checks. Step 3 replaces pad labels with ground clearings and low markers. |
| Feedback visibility | 3/5 | 3/5 | Gun traverse/recoil and infantry strides add motion; existing projectiles, health bars, and impact cues remain visible. |

Fix passes addressed washed-out lighting, partly buried pads/labels, incorrect imported traverse axes, non-persistent grade overrides, missing native terrain tangents, and backward-facing imported units. Ground vegetation is combined into three meshes rather than hundreds of scene objects. Terrain uses 12,325 vertices and smooth normals.

## Native shading regression

The first native build showed dark blotches across the meadow despite a clean Editor capture. Disabling terrain shadow casting, ordinary shadows, and ambient occlusion did not resolve them. The exported native texture was correct, and an unlit material rendered correctly. Adding tangents to a clone of the original mesh, retaining its original normals and restoring all lighting/AO, removed the blotches. The durable fix generates and saves tangents for both textured meshes. `CheckDiorama.Map` failed on missing tangents before the fix; `CheckNativeVisuals.cs` also verifies them in the built player.

The native profile has four persisted overrides. Grade-on/off captures differ by approximately 15.94, 13.19, and 15.93 RGB levels on average. Reproduce the comparison with `Tools/Validation/compare_captures.py`; it uses only Python's standard library.

## References used

Skill references: `craft-game-floors/references/art-direction.md`, `scene-lighting.md`, and `visual-scorecard.md`; Unity CLI's `playmode-verification-loop.md`; Pipeline's installed `commands/scripts.md`, `commands/build-and-compilation.md`, `commands/editor-lifecycle-and-observability.md`, and `commands/capture.md`. The diagnosis loop was used for the native rendering defect.

Engine documentation: [Unity 6.6 Volumes](https://docs.unity.com/en-us/engine/6000.6/manual/post-processing-and-full-screen-effects/urp/volumes-landing-page), [Bloom](https://docs.unity.com/en-us/engine/6000.6/manual/post-processing-and-full-screen-effects/urp/effect-list/bloom), and [defaultScreenWidth](https://docs.unity.com/en-us/engine/6000.6/script-reference/unityeditor/playersettings/defaultscreenwidth). The attempted ambient-lighting documentation link was unavailable; the installed engine API and live probes supplied that check.

No packages, services, external art, or computer-use tools were added. Original Blender source and deterministic editor generators remain in version control. Windows runtime review remains deferred to a future VM.

## Follow-up: leaves and circular sandbags

The user requested readable leaf detail instead of cloud-like bushes/trees and complete circular sandbag walls. `sandbag-rings.png` records all three gun roles; `leaf-detail.png` is a temporary 2.8-orthographic-size material review, closer than the unchanged 5.5 gameplay zoom limit. The temporary camera controller override ends when Play Mode stops. `foliage-overview.png` records the native gameplay view.

Canopies now use individually shaped leaves, visible branches, vein/color texture, and three related greens. The first leaf draft had inverted surface normals; the fix corrected face winding before handoff. The established palette, lighting, UI, and gameplay are retained. Scoped style/silhouette scores remain at least 3/5, now supported by leaf shapes at overview and veins at close review. No smooth canopy spheres remain.

## Follow-up: farmhouse architecture

`houses-detail.png` shows two temporary review orientations of the same revised farmhouse. Broad rounded corners are replaced with small architectural edge breaks. Both roof wings descend from one ridge, with closed gables, slate courses, consistent eaves and a seated chimney. The final fix pass added side windows for the rotated camera. The existing palette and lighting remain in use; the scoped silhouette/style score remains at least 3/5. `CheckHouses.All` raycasts the actual imported roof to verify the ridge is higher than the mid slopes and eaves, symmetric slopes, and a closed surface at each probe. Static parts combine by material. No packages, gameplay changes, or computer-use tools were added.

## Step 3: battlefield composition

The entry uses open farm gates; the defended exit uses a raised checkpoint boom and supplies. Both have physical, two-sided painted signs. Road pigment reaches both slab edges. The same ten build anchors now sit in irregular bare-earth clearings, with narrow access tracks and low timber markers instead of numbered cylinders. Towers sit lower to meet the earth. Farm plots, boundary fencing, supply details and a shallow timber-lined field position refine unused margins.

The cut edge has three irregular soil horizons, world-sized grain/stone texture, a muted root mat and embedded stones. Fix passes reduced overly regular sediment lines and bright tangled roots, cleared shrubs away from the timber position, and preserved single-mesh FBX axis/scale transforms under clean placement roots. This last check restores the proper size and orientation of fences, duckboards and stones.

Scoped scores after the pass: palette discipline 3/5; style uniformity 3/5; lighting depth 3/5; silhouette/readability 3/5; UI intentionality 3/5; feedback visibility 3/5. Established light, palette, UI layout and combat effects are retained. Site/readability and surface-detail fixes address the requested weak areas; this is an internal review, not user approval.

`map-composition.png` is the gameplay overview. Native `map-entry.png`, `map-exit.png`, `map-earth-detail.png`, and `map-rotated.png` record close/orbit views. Close review cameras may be nearer than the unchanged gameplay zoom minimum. The field position is a shallow decorative scrape/revetment, not a navigable trench system. No packages, external art or computer-use tools were added.

References reused for this pass: game-craft art direction, scene lighting and visual scorecard; Unity CLI/Pipeline script, capture and build documentation; uGUI skill for the help-copy update.

## Step 4: weapon operation and sound

The pass retains the painted diorama palette, lighting, map, UI layout and miniature silhouettes. MG receiver kick, short warm flashes/tracers and ejected cartridges differ from cannon barrel recoil/dust and mortar tube launch/arc. Mechanical recoil no longer shifts cannon wheels/shield or mortar supports. Blender source now has muzzle sockets and recoil subassemblies; the mortar tube slopes up toward the target. Presentation uses exact shot notifications; targeting, damage, prices, intervals, hit timing, route and economy are unchanged.

`weapon-feedback.png` is a temporary staged three-role review on real clearings, at the gameplay zoom limit. It is not a purchase/balance capture. The first draft exposed effects that were too faint against the ground. The fix increased flash/puff size and opacity while keeping sub-second lifetimes, then moved the staged guns to clear sites so foliage did not hide their muzzles. Shared emitters and a warmed projectile pool replace per-shot sphere creation. The touched palette, style, lighting, silhouette, UI and feedback axes score 3/5 after the fix; the first draft's feedback visibility was 2/5. Scores are internal, not user acceptance.

Audio now uses 21 edited variations from original gunshot recordings, Kenney material impacts and Spring Spring's explosion. Source recordings and processing remain versioned. Attribution follows the gunshot archive's CC BY 3.0 notice rather than its conflicting CC0 web label, and ships with both platforms. Audio has camera-relative stereo, headroom, modest variation, a 24-voice priority budget and separate soft/armor/shell responses. No AudioMixer was authored; the existing headless gain layer remains in use. No new package or computer-use automation was added.

References read for this pass: game-craft `game-feel.md`, `vfx.md`, `game-audio.md`, `visual-scorecard.md`; Unity CLI/Pipeline `commands/scripts.md`, `build-and-compilation.md`, `capture.md`, and `runtime.md`. Unity 6.6 API pages checked: ParticleSystem.Emit, AudioSource.Play, AudioImporter, ObjectPool, Time.timeScale and Application.runInBackground. The URP 17.6 particle-unlit manual URL was unavailable; the installed matching shader and saved transparent-material state supplied that check. Source/credit links are in `Tools/Audio/README.md` and `ASSET_SOURCES.md`.

Final native review also moved ricochets out of the tank centre and brightened flashes/sparks with a restrained additive material. `weapons-native-launch.png` and `weapons-native-impact.png` show the final staged launch/impact probe with real imported actors. The first probe accidentally captured a stale menu overlay; refreshing the HUD before disabling the temporary controller fixed the capture setup. The surface regression now asserts spark positions remain outside the target centre. `weapons-native-combat.png` records the actual normal-speed final assault before that last visibility-only fix; it is full-match context, not the final effect close-up. The 314-second match held stable pools and unclipped audio with zero runtime errors/warnings. Final native probe and all three builds pass. User listening review remains the subjective quality check.

## Steps 5 and 6: enemy life and the expanded campaign

The seven original miniature archetypes remain within the approved diorama palette. Riflemen, light-capped runners and armored-vest assault troops have distance-driven legs, carried-weapon motion and body sway. Canvas trucks, armored cars and reinforced heavy tanks join the original light tank. Wheel details rotate around their imported axle axes. Static surfaces are combined by material while carry/stride/wheel hierarchies remain separate. Actor roots stay on the shared movement surface; damaged miniatures react and darken, infantry fall, and vehicles flash/smoke/lean before returning to a pool. Retiring actors are capped at 24. Undamaged infantry omit bars; suppression uses amber bars.

The upgrade inspection panel fits its available choices, shows actual invested sale value, and distinguishes mutually exclusive specializations. Gold tier markings, announced wave composition, tactical hints and a 1x/2x pace button extend the existing UI. The field guide explains upgrading and suppression. Variable formations avoid rigid six-person squads, and infantry use verge passing around safely spaced vehicles.

`campaign-roster.png` is a temporary seven-role Editor showcase, not gameplay evidence. Native roster, upgrade/resize and combat captures are recorded separately. Scoped palette/style/lighting/silhouette/UI/feedback scores remain at least 3/5; first-pass fixes addressed sparse feedback, rigid formation timing, the convoy blocking infantry schedules, dense full-health bars, oversized inspection panels and static multipart draw overhead. This is internal review; final user visual/fun playtesting remains roadmap step 7.

References reused: Unity CLI/Pipeline script/build/capture/runtime docs; game-craft character motion, VFX, game feel and visual scorecard; uGUI layout guidance. Unity 6.6 MaterialPropertyBlock/ObjectPool API pages and Ironhide's original Kingdom Rush description were checked. The reference informs complementary counters and upgrade specializations; no external game art/code, new packages or computer-use tools were added.

Final native evidence: `campaign-native-roster.png` shows the seven final miniatures in a temporary showcase. `campaign-native-upgrades.png` shows a real earned-funds inspection; `campaign-native-combat.png` and `campaign-native-victory.png` record the actual ten-wave run. That verification completed with 16 lives, 455 kills/four escapes and 19 upgrades; late enemies reached 99.7% of the route and drew 198 rear shots. A three-second normal-speed busy-scene sample averaged 57.5 fps; the remainder was accelerated for verification. Native UI checks pass four window sizes. QA reports zero errors/one warning, with unused stripped URP depth-of-field/Panini pass notices in the player log. Those effects are not used by the authored grade. Human visual, sound and fun review remains pending step 7.

## November 10: expanded-campaign feedback

The user's six screenshots are the baseline: large persistent text bars, indistinguishable upgrades, regular infantry lanes and a floating slab. The polish pass retains the painted Normandy palette and light. It replaces slab sides with continuous countryside, adds barns/hamlets, widens road pigment and vegetation clearance, makes all nine upgrade profiles distinct miniatures and reduces the default HUD to small resource/pace controls and a build dock. Help and intel are requested when needed. Inspection has a close control; ground, repeated tower clicks, right-click and Escape also dismiss it. Pause/menu/results copy is shortened. Original line icons share one stroke weight; tooltips preserve control labels.

First review fixes: missing CanvasRenderer on icon graphics, excessive field stripes, a field mask that clamped to straw across the apron, and flat destruction fragments. The pigment seam regression now compares actual saved core/outer pixels; terrain heights match along apron edges. Final explosions use soft fire puffs and small 3D fragments. Vehicle whole-model flashes are removed; localized surface sparks/puffs remain. Infantry use continuous seeded lanes/gaps and a slight road drift rather than three repeating columns. The core centreline and all ten sites are retained.

Scoped internal board after fixes (not user approval):

palette discipline: 3/5 — established meadow, olive, earth and warm gold palette.
style uniformity: 3/5 — matched miniature farms, trees and upgrade kits.
lighting depth: 3/5 — retained warm/cool light and authored grade.
silhouette: 4/5 — reinforced shields, long guns and twin/heavy tubes distinguish upgrades.
UI intentionality: 4/5 — compact icons, short copy, requested help/intel and closable inspection.
feedback visibility: 4/5 — upgrade burst/scale recovery, localized armor hits and bounded fire/smoke/debris.

References used: game-craft audio, VFX and visual-scorecard; uGUI and Unity CLI/Pipeline script/build/runtime/capture guidance; Impeccable distill and craft-floor (the mechanical detector reports no findings, but does not understand all Unity layout behavior). Engine API pages checked: [AudioSource](https://docs.unity.com/en-us/engine/6000.6/script-reference/unityengine/audiosource), [ParticleSystem.Emit](https://docs.unity.com/en-us/engine/6000.6/script-reference/unityengine/particlesystem/emit). The attempted UI.Graphic API link was unavailable; the existing StarGraphic and installed uGUI API supplied the CanvasRenderer check. No new package, AudioMixer or computer-use automation was added. The ambient bed is original synthesized wind/foliage/birds, not a licensed song. The app icon uses built-in imagegen; prompt/source details are in `Tools/Blender/APP_ICON.md`.
