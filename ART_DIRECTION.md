# Visual direction

Style: A painted Normandy countryside diorama with soft, rounded forms, worn earth, layered hedgerows, and readable military miniatures. This follows the user's requested diorama direction; avoid angular terrain strips, cone trees, and cube foliage.

Palette: meadow moss `#607044`, meadow grass `#85905B`, dry grass `#A4A273`, road earth `#AF9D76`, shadow earth `#514537`, cool backdrop `#61736D`. Existing olive uniforms, steel, sandbags, and muted gold UI remain part of the established palette.

Mood: Warm afternoon sunlight with cool sky fill, soft shadows, restrained color grading, and visible contact between miniatures and ground. The road and build pads take priority over decorative detail.

## Repeatable authoring

Run `Tools/Blender/generate_assets.py` in headless Blender. `--only Pine,Farmhouse,Crate,Sandbags` exports just the environment models. All nine editable model scenes remain in `Tools/Blender/miniatures.blend`.

With Countryside open and Play Mode stopped, run `TheLineHolds.Editor.DioramaBuilder.Map()` through Unity CLI. It replaces its own landscape, preserves controller references and pad objects, generates meadow/road and earth textures, combines small vegetation, and saves the scene. It does not tune combat data. Seed 1944 makes the ground detail repeatable.

`BattleRoute.Ground(x,z)` is shared by terrain, pad placement, range previews, and unit movement. Road corners use short quadratic transitions. The road pigment and wheel ruts are baked into the meadow texture, so there is no visible coplanar road overlay to clip at hills.

The map-wide surface is the base for step 2a. Steps 2b and 2c establish matching tower and enemy targets. Step 3 can add authored local stories and richer variations; steps 4 and 5 can expand motion and combat feedback after visual review.

## Leaf and fortification refinement

Tree crowns and shrubs use individually shaped, ridged leaves over branches, with a shared leaf-vein texture and three related green materials. Avoid smooth cloud-shaped canopy balls. Bush meshes combine by material for efficient drawing. Sandbag walls form full circles with two staggered courses; bags follow the tangent of the wall, and the gun mechanism rotates within the fixed wall. Cannon wheels/trails and mortar supplies fit inside their respective rings.

Architecture uses crisp plaster/stone corners with very small edge breaks. Farmhouse roofs descend from one ridge to matching eaves, with closed gables, coursed slate and seated chimney details. Keep building corners and roof planes more angular than foliage and fabric. Static detail is combined by material.

## Battlefield composition

Gun sites are bare-earth clearings with faint access tracks and low timber markers, with no numbering or raised display pedestals. The original ten placement anchors are preserved. Enemy and defended endpoints use physical farm-gate/checkpoint details and two-sided painted wood signs; their lettering is baked into board textures so it stays attached during camera orbit. Road pigment reaches both map edges. Exposed earth uses three wavy horizons, world-sized grain/stone detail, a muted root mat, and embedded stones. Small crop plots, farm fences and a timber-lined field position give the unused margins a purpose. Keep the whole vehicle corridor free of scenery.
