# The Line Holds map art guide

Build a historical miniature that a player can read while a wave is moving. At the reset gameplay camera, the eye should find the incoming column, follow the worn road around each bend, and land on the defended objective. Buildings and fields tell the player what place they are defending.

![Countryside at normal gameplay zoom. A clay-roofed barn and wheat field sit behind the northern bend. A stone cottage frames the foreground, and a patrol passes the brick gate.](TheLineHolds/Screenshots/farmstead-gameplay.png)

Use this frame for **composition and scale**. Use the patrol frame for the transition from offscreen approach to playable road, and the armor frame for the space between a turning gun and a passing vehicle. [Patrol approaching the gate](TheLineHolds/Screenshots/road-entry-patrol.png) · [Upgraded howitzer during wave nine](TheLineHolds/Screenshots/road-clearance-howitzer.png)

## What carries between levels

| Art reference | Quality to carry forward | How it reads in our setting |
| --- | --- | --- |
| Age of Empires II: Definitive Edition | Recognizable silhouettes from the gameplay camera | The barn has a long ridge and paired doors. The anti-tank gun has a shield and long barrel. Each reads before the player can see individual boards or bolts. |
| Age of Empires III: Definitive Edition | Architectural character and selective color | A clay roof picks out the working farm. Cool slate, lime plaster, painted shutters and masonry sills tie the neighboring buildings together. Detail the roof, gables and sides that the orbit camera reveals. |
| Company of Heroes 2 and 3 | Grounded materials, directional light and evidence of use | Set the stone footing into the slope. Let wheel wear break over firm ground, leave grit on the shoulder, and place supplies where a crew would reach them. |

Give each level its own geography, sightlines and civilian activity. Countryside pairs a barn with planted rows, a cut headland, an implement shed and a wagon track. A mill by a crossing could use a mill yard, a loading door and a cart track to explain the same relationships. Find room for that activity inside the normal view. A few related props tell the story more clearly than an even scatter across the meadow.

## Compose from the gameplay camera

1. **Block in the playable ground.** Place the route, approach, objective and legal gun sites. Fit the overview to the playable route and sites with the HUD visible. Keep that fit as the widest view. Let players pan farther when inspecting a bend up close.
2. **Give the road a beginning and an end.** Run the incoming lane beyond the visible frame so the column can march into view. At the entrance, use a landmark traffic can pass through. Give the objective a different outline and function. Countryside uses paired brick gateposts and a parked cart at one end, a roofed sentry box, flag and raised boom at the other.
3. **Separate traffic from firing positions.** Paint a broad traveled center, intermittent wheel marks and feathered shoulders into the same surface the units follow. Make each legal site a tamped clearing with low stakes and a faint access track. On Countryside the worn road is about 1.9 m wide and the closest pad center is about 2.5 m from the road centerline. For a different bend, judge the gap with the largest vehicle alongside a fully upgraded cannon aimed across the road. Check the range ring and actual engagement after moving a pad.
4. **Frame the fight.** Place the civilian landmark in a visible pocket beyond a bend and use lower forms on the near side of the road. Gather crop rows, timber, crates, flowers or walling where work takes place. Move a tree if its canopy hides a turning point or makes an empty pad look occupied. Carry the same turf, leaf scale and connected lanes beyond the playable area so resized windows and rotated views still look inhabited.

## Materials, light and scale

Start with the shared Unity materials. These hex values are authoring colors. Compare the lit result to the gameplay frame, since sun, sky fill and tone mapping affect the final pixels.

| Surface | Countryside colors | Treatment |
| --- | --- | --- |
| Meadow and crops | Hollow green `#526849`, meadow `#839461`, hay `#AFAB73` | Vary growth in patches. Use small grass tufts, branched hedges with shaped leaves, and cultivated rows with a worked-soil edge. |
| Road and yard | Clay `#957958`, chalk `#C2B093`, wheel track `#695942` | Keep the center quiet beneath units. Concentrate stones and debris at edges. Match pigment and terrain height where the battlefield meets the surrounding pasture. |
| Buildings | Oak `#927456`, plaster `#CCBD97`, masonry `#B0AA94`, slate `#526775`, clay tile `#A45741` | Seat structures on foundations. Use crisp gables, tile courses, lintels and shutters. Reserve the strongest roof accent for a landmark. |
| Military miniatures | Olive `#5D6953`, steel `#59656B`, sandbag cloth `#9D9279` | Keep timber and cloth matte, give steel a modest highlight, and use fixed sandbag rings around moving mechanisms. |

Use one warm directional sun, cool sky fill, and soft contact shadows. Countryside starts from sun `#FFF1DA`, intensity 2.2, Unity rotation (38°, 38°, 0°), with sky fill `#849BA8`, ACES tone mapping and bloom intensity 0.12. Match roof edges, gun shields and wheel shadows from the normal camera before adjusting the grade. [Terrain pigment](TheLineHolds/Assets/TheLineHolds/Editor/FieldPigment.cs) · [Shared material finishes](TheLineHolds/Assets/TheLineHolds/Editor/FoliageMaterials.cs) · [URP lighting setup](TheLineHolds/Assets/TheLineHolds/Editor/DioramaBuilder.cs)

## Readability during a wave

| Role | Shape at overview | Firing cue |
| --- | --- | --- |
| Machine gun | Compact tripod or nest, receiver and feed drum | Quick receiver kick, short warm flash, tracer and brass |
| Anti-tank gun | Shield, wheels and a prominent aiming barrel | Barrel slides inside the fixed carriage, heavier shell streak, localized impact |
| Mortar | Broad braced bed with an elevated open tube | Launch puff, arcing shell with a short smoke tail, ground-level blast |

For an upgrade, change a major visible component such as a twin mount, heavy tube or armored shield. Keep the muzzle socket with the moving barrel. A runner’s satchel and long stride, a rifleman’s pack, and an assault soldier’s heavier kit should distinguish the infantry in motion. Preserve the differences between a canvas truck, wheeled armored car, tracked light tank and skirted heavy tank. Keep hit sparks, dust and smoke brief enough to follow the lead unit and see which emplacement fired. [Combat reference frame](TheLineHolds/Screenshots/miniatures-wave.png)

## Author a new level

The current project ships one authored map. Its runtime creates the Countryside route and default pads directly. The camera also reads those defaults, and the map builder writes meshes and textures under shared asset names. Duplicating the Unity scene alone still points at Countryside data. For a second playable map, give it its own route, approach, sites, terrain and generated asset paths, then connect the simulation, camera and scenery bake to that same layout. Reuse the shared prefabs, materials, pigment functions and lighting. [Route and terrain](TheLineHolds/Assets/TheLineHolds/Scripts/Core/BattleRoute.cs) · [Match setup](TheLineHolds/Assets/TheLineHolds/Scripts/Presentation/BattleController.cs) · [Camera fit](TheLineHolds/Assets/TheLineHolds/Scripts/Presentation/MapCameraControls.cs)

Author models at metre scale in Blender. Keep the editable source and generation script with the FBX. Match existing prefab forward axes, origins, material assignments and moving parts. For example, the Blender generator accepts `--only Barn` to export the barn FBX while saving the editable scenes. The farm example reserves a footprint for buildings, crops and access tracks. Use the same footprint for ground pigment and vegetation clearance, then batch static detail by material and area. Set the dressing seed so a rebuild reproduces the placement. [Blender generator](Tools/Blender/generate_assets.py) · [Farm layout, pigment and crop geometry](TheLineHolds/Assets/TheLineHolds/Editor/BattlefieldFarm.cs) · [Scenery batching](TheLineHolds/Assets/TheLineHolds/Editor/CountrysideSurroundings.cs)

For the existing Countryside scene, stop Play Mode and use **The Line Holds → Visuals → Build Diorama Map** to rebuild its landscape. **Refresh Historic Miniatures** refreshes its actor prefabs, field dressing, pigment and lighting. Drive the open editor through Unity CLI. [Project commands](README.md#repeatable-commands)

## Review a map in play

- Capture the reset overview with the HUD at 1600×900. Trace the route from entrance to objective, locate each legal site, and identify the civilian landmark. Resize to a narrower and a wider window. Countryside was checked at 1024×768 and 1920×800 as well. Watch an arrival after resizing or rotating. [Overview check](TheLineHolds/AgentScripts/CheckOverview.cs) · [Native entrance and clearance check](TheLineHolds/AgentScripts/CheckRoadEntry.cs)
- Place each tower role and inspect an upgrade on a tight bend. Capture a busy wave with a large vehicle passing a firing position. Check silhouettes, aim, range, impact and whether the lead enemy stays visible. Use the same camera, wave, seed and playback speed when comparing before and after. [Model and capture checks](TheLineHolds/AgentScripts/ReviewMiniatures.cs)
- Complete a wave with earned purchases. Compare frame rate in the same encounter and player configuration if the dressing or effects budget changed. Adapt Countryside-specific assertions, including named landmarks and the ten-site count, to the new map. Keep the accepted overview and combat captures alongside the level. Earlier passes and their measured results are in the [visual review history](VISUAL_REVIEW.md).
