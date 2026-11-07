# Visual direction

Style: A painted Normandy countryside diorama with soft, rounded forms, worn earth, layered hedgerows, and readable military miniatures. This follows the user's requested diorama direction; avoid angular terrain strips, cone trees, and cube foliage.

Palette: meadow moss `#607044`, meadow grass `#85905B`, dry grass `#A4A273`, road earth `#AF9D76`, shadow earth `#514537`, cool backdrop `#61736D`. Existing olive uniforms, steel, sandbags, and muted gold UI remain part of the established palette.

Mood: Warm afternoon sunlight with cool sky fill, soft shadows, restrained color grading, and visible contact between miniatures and ground. The road and build pads take priority over decorative detail.

## Repeatable authoring

Run `Tools/Blender/generate_assets.py` in headless Blender. `--only Pine,Farmhouse,Crate,Sandbags` exports just the environment models. All nine editable model scenes remain in `Tools/Blender/miniatures.blend`.

With Countryside open and Play Mode stopped, run `TheLineHolds.Editor.DioramaBuilder.Map()` through Unity CLI. It replaces its own landscape, preserves controller references and pad objects, generates meadow/road and earth textures, combines small vegetation, and saves the scene. It does not tune combat data. Seed 1944 makes the ground detail repeatable.

`BattleRoute.Ground(x,z)` is shared by terrain, pad placement, range previews, and unit movement. Road corners use short quadratic transitions. The road pigment and wheel ruts are baked into the meadow texture, so there is no visible coplanar road overlay to clip at hills.

The map-wide surface is the base for step 2a. Steps 2b and 2c establish matching tower and enemy targets. Step 3 can add authored local stories and richer variations; steps 4 and 5 can expand motion and combat feedback after visual review.
