# Step 2 visual review

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
| UI intentionality | 3/5 | 3/5 | Existing UI preserved. Five logical widths pass overlap checks; pad labels stay readable on level footprints. |
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
