# Visual direction

Style: A painted Normandy countryside diorama with soft, rounded forms, worn earth, layered hedgerows, and readable military miniatures. This follows the user's requested diorama direction; avoid angular terrain strips, cone trees, and cube foliage.

Palette: meadow moss `#607044`, meadow grass `#85905B`, dry grass `#A4A273`, road earth `#AF9D76`, shadow earth `#514537`, cool backdrop `#61736D`. Existing olive uniforms, steel, sandbags, and muted gold UI remain part of the established palette.

Mood: Warm afternoon sunlight with cool sky fill, soft shadows, restrained color grading, and visible contact between miniatures and ground. The road and build pads take priority over decorative detail.

## Repeatable authoring

Run `Tools/Blender/generate_assets.py` in headless Blender. `--only Pine,Farmhouse,Crate,Sandbags` exports just those environment models. All editable model scenes remain in `Tools/Blender/miniatures.blend`.

With Countryside open and Play Mode stopped, run `TheLineHolds.Editor.DioramaBuilder.Map()` through Unity CLI. It replaces its own landscape, preserves controller references and pad objects, generates meadow/road and earth textures, combines small vegetation, and saves the scene. It does not tune combat data. Seed 1944 makes the ground detail repeatable.

`BattleRoute.Ground(x,z)` is shared by terrain, pad placement, range previews, and unit movement. Road corners use short quadratic transitions. The road pigment and wheel ruts are baked into the meadow texture, so there is no visible coplanar road overlay to clip at hills.

The map-wide surface is the base for step 2a. Steps 2b and 2c establish matching tower and enemy targets. Step 3 can add authored local stories and richer variations; steps 4 and 5 can expand motion and combat feedback after visual review.

## Leaf and fortification refinement

Tree crowns and shrubs use individually shaped, ridged leaves over branches, with a shared leaf-vein texture and three related green materials. Avoid smooth cloud-shaped canopy balls. Bush meshes combine by material for efficient drawing. Sandbag walls form full circles with two staggered courses; bags follow the tangent of the wall, and the gun mechanism rotates within the fixed wall. Cannon wheels/trails and mortar supplies fit inside their respective rings.

Architecture uses crisp plaster/stone corners with very small edge breaks. Farmhouse roofs descend from one ridge to matching eaves, with closed gables, coursed slate and seated chimney details. Keep building corners and roof planes more angular than foliage and fabric. Static detail is combined by material.

## Battlefield composition

Gun sites are bare-earth clearings with faint access tracks and low timber markers, with no numbering or raised display pedestals. The original ten placement anchors are preserved. Enemy and defended endpoints use physical farm-gate/checkpoint details and two-sided painted wood signs; their lettering is baked into board textures so it stays attached during camera orbit. Road pigment reaches both map edges. Exposed earth uses three wavy horizons, world-sized grain/stone detail, a muted root mat, and embedded stones. Small crop plots, farm fences and a timber-lined field position give the unused margins a purpose. Keep the whole vehicle corridor free of scenery.

## Weapon operation and effects

Machine guns use quick warm flashes, short tracers, restrained receiver kick and ejected brass. Cannon barrels slide independently of their fixed shield/wheels, recover more slowly, and raise a little dust. Mortar tubes point upward toward the target, with a launch puff and an olive shell travelling along an arc. Weapon effects start at authored muzzle sockets rather than the simulation's generic shot origin. Short earth-colored puffs, warm armor sparks and restrained mortar smoke preserve target/road readability. No camera shake, hit-stop or combat-stat changes are part of this pass.

Five shared world-space particle emitters and warmed projectile objects keep transient effects bounded. Pause freezes effects and mechanical recovery with the match. Edited recorded sounds give each role its own report and hit response; soft impacts and armor strikes stay quieter than heavy weapons. Audio uses the existing CLI-only gain layer, a bounded priority voice pool and camera-relative stereo. Rebuild these assets with `CombatPresentationBuilder.Build()` after model generation. Keep the shipped audio attribution file.

## Enemy life and campaign readability

Infantry have distance-driven body/leg movement and a small carried-weapon sway. Vehicles roll their wheel details around the imported axle axis and sway subtly on the road. Hit reactions and damage wear affect only miniature children; gameplay roots stay on the route. Infantry fall briefly; vehicles flash, smoke, lean and retire. Actor models/bars are reused, with a 24-corpse budget and clean reset. Suppressed enemies show amber bars; undamaged infantry omit health bars to reduce clutter.

The seven archetypes retain the established military palette: light field caps for runners, armored vests for assault infantry, canvas-backed trucks, compact armored cars and a larger reinforced tank. Higher tower tiers gain small gold markings. Upgrade inspection adapts to the available choices and uses the same command-panel style. Next-wave intel fixes composition while formation timing varies. No new package, external art, camera shake or computer-use workflow is part of this pass.

## November 10 playtest polish

The miniature palette is retained, but the cutaway slab is removed from the scene. Four continuous pasture aprons extend 90 metres in each direction, with original barns, farmhouses, loose trees, bushes, farm fences and supply yards. Core route and build anchors stay fixed. Meadow pigment matches across the boundary; fields use normalized soft masks. Scenery combines by material and spatial sector for useful culling. The worn road is about 1.9 m wide before its feathered edges, versus 1.28 m previously. Vegetation clears the wider corridor. The default overview is closer (10.8 orthographic size).

The HUD uses compact icon-led resources and controls, a three-choice build dock, on-demand intel/help, short transient notices and a small closable upgrade inspector. Pause, menu and scoring copy is shortened. Hand-authored line icons use CanvasRenderer geometry; no icon font is required. Upgrade models have reinforced shields, larger mechanisms, distinct long/twin/heavy barrels, supply crates and gold identification details, with a short purchase burst and scale recovery. Original Blender scenes retain every tier/branch model.

Infantry use continuous seeded lateral positions, varied individual release gaps and a gentle distance-driven lateral drift. Verge passing has per-unit width variation. Vehicles keep the existing safe longitudinal gap. Armor hits use localized sparks/puffs at the facing hull edge; whole-vehicle brightening is disabled. Destruction uses pooled fire puffs, smoke and small 3D debris. Audio adds an original quiet wind/leaf/bird bed with independent mute and combat/pause ducking through the existing gain layer. The new app icon is imagegen artwork; its final prompt and saved path are in `Tools/Blender/APP_ICON.md`.
