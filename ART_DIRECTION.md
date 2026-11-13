# The Line Holds map art guide

Build a historical miniature that a player can read while a wave is moving. At the reset gameplay camera, the eye should find the incoming column, trace its path through the terrain, and land on the defended objective. The surroundings should tell the player what this place is used for and why the objective belongs here.

![Countryside at normal gameplay zoom. A clay-roofed barn and wheat field sit behind the northern bend. A stone cottage frames the foreground, and a patrol passes the brick gate.](TheLineHolds/Screenshots/farmstead-gameplay.png)

Use this frame to compare **miniature scale and the distribution of detail**. Its geography, palette, building kit and path belong to Countryside. Use the patrol frame for the transition from offscreen approach to playable road, and the armor frame for the space between a turning gun and a passing vehicle. [Patrol approaching the gate](TheLineHolds/Screenshots/road-entry-patrol.png) · [Upgraded howitzer during wave nine](TheLineHolds/Screenshots/road-clearance-howitzer.png)

## What carries between levels

| Art reference | Quality to carry forward | How it reads in our setting |
| --- | --- | --- |
| Age of Empires II: Definitive Edition | Recognizable silhouettes from the gameplay camera | The barn has a long ridge and paired doors. The anti-tank gun has a shield and long barrel. Each reads before the player can see individual boards or bolts. |
| Age of Empires III: Definitive Edition | Architectural character and selective color | A clay roof picks out the working farm. Cool slate, lime plaster, painted shutters and masonry sills tie the neighboring buildings together. Detail the roof, gables and sides that the orbit camera reveals. |
| Company of Heroes 2 and 3 | Grounded materials, directional light and evidence of use | Set the stone footing into the slope. Let wheel wear break over firm ground, leave grit on the shoulder, and place supplies where a crew would reach them. |

## Design the path before dressing the map

Author a distinct enemy path for each new level. In a bare-ground sketch, draw the offscreen approach, entrance, bends, objective and proposed gun sites. Let the terrain explain the journey. A switchback around a ridge can bring a column into range twice. A long street can give one gun sustained coverage while a side-on position only catches the turn. Compare this sketch with existing levels. If the same placements cover the same stretches in the same order, revise the geometry until the player has a different choice to make.

Fit the reset camera to that playable layout with the HUD visible. Keep the overview as the widest view and allow closer inspection of individual bends. Show the column entering the frame in motion. Give the arrival landmark and defended objective different silhouettes. Carry the setting beyond the frame so a resize or orbit still shows continuous terrain.

Make the traveled surface legible in the local material. Compacted snow, worn paving and damp sand each record traffic differently. Give legal build sites a consistent visual cue within the level. Turn the longest upgraded gun toward a passing vehicle at the tightest point and check its clearance, targeting range and time on target.

## Density and signs of life

Judge density from the reset camera with units on the path. In the farmstead frame, the barn roof identifies the landmark, the adjoining rows explain its purpose, and the short strip of stubble shows where harvesting stopped. The open ground around the nearby bend lets the player follow a squad past all three details. Use that relationship between landmark, working detail and breathing room in other settings.

- **Cluster by activity.** Pick something people do here. Show where the work happens, where its supplies come from, and a mark it leaves on the ground. Put the group inside the gameplay view. Choose the number and size of props by what remains legible at that distance.
- **Vary the density.** Let the most intricate area surround a landmark, with quieter surfaces behind combat and at bends. A town can feel densely built through a continuous roofline and a few detailed shopfronts. A beach can feel inhabited through two well-used landing points separated by open sand.
- **Use motion sparingly.** A flag at a landing, a little chimney smoke or blowing snow caught against a wall can reveal wind and use. Follow a lead enemy past the effect during a volley. Reduce its contrast, footprint or duration if it competes with the shot or impact.

## Examples for future settings

These are possible level briefs. Choose the terrain, palette, weather, activity, silhouettes and path for each level.

| Setting | A path with its own firing opportunities | Evidence of use visible at gameplay zoom |
| --- | --- | --- |
| Snow | A compacted track doubles back around a rocky spur, then crosses a stream to a signal hut. An inside site covers both legs briefly. | Sled runners polish the snow between a woodpile and the hut. Drifts collect on the sheltered side, leaving the crossing clear. |
| Town | A column enters under a railway arch, follows a long cobbled street, then turns across a market square toward the station. The street and square reward different firing positions. | Cart wear connects the loading doors to the square. Awnings and stacked produce cluster along its far side, where roofs leave the turn visible. |
| Beach | A column arrives along the tide line, crosses an exposed stretch of shingle, then turns inland through a dune gap toward a coast road. The best coverage shifts from the shore to the gap. | Wet sand holds tracks from a beached supply boat to a stack of stores. Rope, crates and a pennant mark the landing while the surf stays clear of the column. |

## Materials and lighting

Keep model scale, coherent material families, grounded foundations, directional shading and legible contact shadows across levels. Set the hue, surface finish, light direction, temperature and atmosphere to suit the place. Read snow as snow even in shadow. Let steel and wet paving catch different highlights. In the lit gameplay frame, check whether an olive figure, a wheel and a gun shield still separate from their backgrounds.

### Countryside reference settings

These authoring colors and generator settings reproduce the farmstead. Use them to match an existing surface when it appears in another map.

| Surface | Countryside colors | Treatment |
| --- | --- | --- |
| Meadow and crops | Hollow green `#526849`, meadow `#839461`, hay `#AFAB73` | Vary growth in patches. Use small grass tufts, branched hedges with shaped leaves, and cultivated rows with a worked-soil edge. |
| Road and yard | Clay `#957958`, chalk `#C2B093`, wheel track `#695942` | Keep the center quiet beneath units. Concentrate stones and debris at edges. Match pigment and terrain height where the battlefield meets the surrounding pasture. |
| Buildings | Oak `#927456`, plaster `#CCBD97`, masonry `#B0AA94`, slate `#526775`, clay tile `#A45741` | Seat structures on foundations. Use crisp gables, tile courses, lintels and shutters. Reserve the strongest roof accent for a landmark. |
| Military miniatures | Olive `#5D6953`, steel `#59656B`, sandbag cloth `#9D9279` | Keep timber and cloth matte, give steel a modest highlight, and use fixed sandbag rings around moving mechanisms. |

Countryside uses a warm directional sun, cool sky fill and soft contact shadows. Its sun is `#FFF1DA`, intensity 2.2, Unity rotation (38°, 38°, 0°), with sky fill `#849BA8`, ACES tone mapping and bloom intensity 0.12. Match roof edges, gun shields and wheel shadows from the normal camera before adjusting the grade. [Terrain pigment](TheLineHolds/Assets/TheLineHolds/Editor/FieldPigment.cs) · [Shared material finishes](TheLineHolds/Assets/TheLineHolds/Editor/FoliageMaterials.cs) · [URP lighting setup](TheLineHolds/Assets/TheLineHolds/Editor/DioramaBuilder.cs)

Countryside’s worn road is about 1.9 m wide, and the closest pad center is about 2.5 m from the centerline. Low survey stakes and tamped earth identify its ten sites. Paired brick gateposts frame arriving units. A roofed sentry box and raised boom identify the objective. Use the vehicle and gun silhouettes in the reference frame to judge spacing when authoring another layout.

## Readability during a wave

| Role | Shape at overview | Firing cue |
| --- | --- | --- |
| Machine gun | Compact tripod or nest, receiver and feed drum | Quick receiver kick, short warm flash, tracer and brass |
| Anti-tank gun | Shield, wheels and a prominent aiming barrel | Barrel slides inside the fixed carriage, heavier shell streak, localized impact |
| Mortar | Broad braced bed with an elevated open tube | Launch puff, arcing shell with a short smoke tail, ground-level blast |

For an upgrade, change a major visible component such as a twin mount, heavy tube or armored shield. Keep the muzzle socket with the moving barrel. A runner’s satchel and long stride, a rifleman’s pack, and an assault soldier’s heavier kit should distinguish the infantry in motion. Preserve the differences between a canvas truck, wheeled armored car, tracked light tank and skirted heavy tank. Keep hit sparks, dust and smoke brief enough to follow the lead unit and see which emplacement fired. [Combat reference frame](TheLineHolds/Screenshots/miniatures-wave.png)

## Author a new level

The current project ships one authored map. Its runtime creates the Countryside route and default pads directly. The camera also reads those defaults, and the map builder writes meshes and textures under shared asset names. Duplicating the Unity scene alone still points at Countryside data. For a second playable map, give it its own route, approach, sites, terrain and generated asset paths, then connect the simulation, camera and scenery bake to that same layout. Reuse shared actor prefabs and relevant material finishes. Supply the surface treatment and lighting for the new setting. [Route and terrain](TheLineHolds/Assets/TheLineHolds/Scripts/Core/BattleRoute.cs) · [Match setup](TheLineHolds/Assets/TheLineHolds/Scripts/Presentation/BattleController.cs) · [Camera fit](TheLineHolds/Assets/TheLineHolds/Scripts/Presentation/MapCameraControls.cs)

Author models at metre scale in Blender. Keep the editable source and generation script with the FBX. Match existing prefab forward axes, origins, material assignments and moving parts. For example, the Blender generator accepts `--only Barn` to export the barn FBX while saving the editable scenes. The farm example reserves a footprint for buildings, crops and access tracks. Use the same footprint for ground pigment and vegetation clearance, then batch static detail by material and area. Set the dressing seed so a rebuild reproduces the placement. [Blender generator](Tools/Blender/generate_assets.py) · [Farm layout, pigment and crop geometry](TheLineHolds/Assets/TheLineHolds/Editor/BattlefieldFarm.cs) · [Scenery batching](TheLineHolds/Assets/TheLineHolds/Editor/CountrysideSurroundings.cs)

For the existing Countryside scene, stop Play Mode and use **The Line Holds → Visuals → Build Diorama Map** to rebuild its landscape. **Refresh Historic Miniatures** refreshes its actor prefabs, field dressing, pigment and lighting. Drive the open editor through Unity CLI. [Project commands](README.md#repeatable-commands)

## Review a map in play

- Capture the reset overview with the HUD at 1600×900. Trace the route from entrance to objective, locate each legal site, and identify what happens at the main landmark. Resize to a narrower and a wider window. Countryside was checked at 1024×768 and 1920×800 as well. Watch an arrival after resizing or rotating. [Overview check](TheLineHolds/AgentScripts/CheckOverview.cs) · [Native entrance and clearance check](TheLineHolds/AgentScripts/CheckRoadEntry.cs)
- Place each tower role and inspect an upgrade on a tight bend. Capture a busy wave with a large vehicle passing a firing position. Check silhouettes, aim, range, impact and whether the lead enemy stays visible. Use the same camera, wave, seed and playback speed when comparing before and after. [Model and capture checks](TheLineHolds/AgentScripts/ReviewMiniatures.cs)
- Complete a wave with earned purchases. Compare frame rate in the same encounter and player configuration if the dressing or effects budget changed. Adapt Countryside-specific assertions, including named landmarks and the ten-site count, to the new map. Keep the accepted overview and combat captures alongside the level. Earlier passes and their measured results are in the [visual review history](VISUAL_REVIEW.md).
