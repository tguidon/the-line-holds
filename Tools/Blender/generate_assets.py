"""Run: Blender --background --python Tools/Blender/generate_assets.py -- <repo>.
Original procedural models, metres, +Z forward in Unity. No external assets.
"""
import bpy, math, os, sys, random
from mathutils import Vector
root = sys.argv[sys.argv.index('--')+1]
out = os.path.join(root, 'TheLineHolds/Assets/TheLineHolds/Art/Models')
os.makedirs(out, exist_ok=True)
materials={}
for name,c in {'Olive':(.25,.29,.16,1),'Dark':(.075,.085,.065,1),'Steel':(.19,.22,.19,1),'Cloth':(.36,.39,.25,1),'Skin':(.59,.43,.3,1),'Sand':(.51,.44,.29,1),'Wood':(.25,.17,.10,1),'Leaf':(.25,.32,.15,1),'Plaster':(.62,.57,.43,1),'Roof':(.27,.24,.20,1),'LeafLight':(.48,.56,.32,1),'LeafDark':(.18,.29,.17,1),'Bark':(.36,.28,.18,1),'Leather':(.28,.21,.13,1),'Edge':(.39,.44,.28,1),'Badge':(.72,.67,.49,1)}.items():
 m=bpy.data.materials.new(name); m.diffuse_color=c; materials[name]=m

def part(name,loc,scale,mat='Olive',shape='cube',rot=None):
 if shape=='sphere': bpy.ops.mesh.primitive_uv_sphere_add(segments=20,ring_count=12,location=loc)
 elif shape=='cylinder': bpy.ops.mesh.primitive_cylinder_add(vertices=24,radius=1,depth=2,location=loc)
 elif shape=='cone': bpy.ops.mesh.primitive_cone_add(vertices=9,radius1=1,radius2=.06,depth=2,location=loc)
 else: bpy.ops.mesh.primitive_cube_add(size=2,location=loc)
 o=bpy.context.object; o.name=name; o.scale=scale
 if rot: o.rotation_euler=rot
 o.data.materials.append(materials[mat])
 if name.startswith('Leaf crown') or name=='Crown top':
  for v in o.data.vertices:
   v.co *= 1 + .09*math.sin(v.co.x*11+v.co.y*8)*math.sin(v.co.z*9+v.co.x*4)
 # Apply scale before bevel so metre-sized edges stay consistent across long thin parts.
 bpy.ops.object.transform_apply(location=False,rotation=False,scale=True)
 if shape=='cube':
  bevel=o.modifiers.new('Soft manufactured edges','BEVEL');bevel.width=min(scale)*.23;bevel.segments=3
  bpy.context.view_layer.objects.active=o;bpy.ops.object.modifier_apply(modifier=bevel.name)
  weighted=o.modifiers.new('Weighted corner normals','WEIGHTED_NORMAL');weighted.keep_sharp=True
  bpy.ops.object.modifier_apply(modifier=weighted.name)
 else:
  for polygon in o.data.polygons: polygon.use_smooth=True
 return o

def barrel(loc,length=.8,r=.045,angle=0):
 return part('Barrel',loc,(r,r,length/2),'Steel','cylinder',(math.pi/2+angle,0,0))

def tank():
 for x in [-.45,.45]:
  part('Track',(x,0,.24),(.15,.7,.19),'Dark')
  for y in [-.45,-.15,.15,.45]: part('RoadWheel',(x*1.29,y,.24),(.12,.12,.065),'Steel','cylinder',(0,math.pi/2,0))
 part('Hull',(0,0,.46),(.43,.64,.23)); part('Turret',(0,.06,.78),(.29,.3,.18),'Olive','cylinder')
 part('Hatch',(0,.06,.98),(.16,.16,.035),'Steel','cylinder'); barrel((0,.72,.82),1)
 part('Muzzle',(0,1.21,.82),(.06,.09,.06),'Dark')

def infantry():
 for x in [-.1,.1]:
  part('Boot',(x,.06,.08),(.075,.14,.07),'Dark'); part('Leg',(x,0,.27),(.07,.075,.17),'Cloth')
 part('Coat',(0,0,.56),(.18,.12,.2),'Cloth'); part('Belt',(0,0,.43),(.19,.13,.025),'Wood')
 part('Head',(0,0,.85),(.11,.1,.12),'Skin','sphere'); part('Helmet',(0,0,.94),(.15,.14,.075),'Olive','sphere')
 for x in [-.22,.22]:part('Arm',(x,.03,.61),(.055,.065,.18),'Cloth')
 part('Pack',(0,-.15,.59),(.13,.06,.14),'Sand'); barrel((.18,.24,.57),.55,.025)

def bags():
 for row in range(2):
  for i in range(5): part('Sandbag',((i-2)*.31+(row%2)*.08,.34,row*.16+.12),(.17,.13,.1),'Sand','sphere')
 for x in [-.68,.68]:
  for j in range(2):part('SideBag',(x,.03-j*.25,.13),(.13,.17,.11),'Sand','sphere')

def gun(kind):
 bags()
 before=set(bpy.context.scene.objects)
 if kind=='MachineGun':
  part('Mount',(0,0,.29),(.045,.045,.26),'Steel','cylinder')
  part('Receiver',(0,.13,.55),(.095,.24,.08),'Steel');barrel((0,.56,.56),.48,.035)
  part('Heat sleeve',(0,.49,.56),(.052,.052,.16),'Dark','cylinder',(math.pi/2,0,0))
  for y in [.39,.45,.51,.57,.63]:part('Cooling ring',(0,y,.56),(.057,.057,.012),'Steel','cylinder',(math.pi/2,0,0))
  part('Stock',(0,-.21,.53),(.065,.16,.055),'Wood')
  part('Ammo',(-.23,.05,.29),(.11,.15,.12),'Olive');part('Ammo lid',(-.23,.05,.42),(.12,.16,.02),'Edge')
  for i in range(5):part('Belt round',(-.15-i*.035,.13,.54),(.014,.04,.014),'Sand','cylinder',(math.pi/2,0,0))
  part('Rear sight',(0,.03,.66),(.015,.025,.03),'Dark')
 elif kind=='AntiTank':
  for x in [-.42,.42]:
   part('Rubber tire',(x,-.05,.3),(.26,.26,.08),'Dark','cylinder',(0,math.pi/2,0))
   part('Wheel rim',(x*1.2,-.05,.3),(.19,.19,.018),'Edge','cylinder',(0,math.pi/2,0))
   part('Axle hub',(x*1.25,-.05,.3),(.065,.065,.035),'Steel','cylinder',(0,math.pi/2,0))
  part('Shield',(0,.05,.55),(.4,.045,.27),'Olive',rot=(.13,0,0));barrel((0,.65,.6),1.3,.065)
  part('Breech',(0,-.15,.6),(.13,.24,.11),'Steel')
  part('Recoil sleeve',(0,.32,.6),(.085,.085,.32),'Edge','cylinder',(math.pi/2,0,0))
  part('Muzzle brake',(0,1.26,.6),(.085,.09,.075),'Dark')
  for x in [-.32,.32]:
   for z in [.4,.68]:part('Shield rivet',(x,.108,z),(.018,.014,.018),'Edge','sphere')
  for x in [-.26,.26]:part('Trail',(x,-.55,.18),(.045,.5,.045),'Steel',rot=(0,0,x))
  part('Elevation handwheel',(.19,-.2,.52),(.09,.09,.02),'Steel','cylinder',(0,math.pi/2,0))
 else:
  part('Base plate',(0,0,.07),(.3,.3,.045),'Steel','cylinder');barrel((0,.1,.4),.7,.09,-.65)
  part('Tube collar',(0,.22,.57),(.105,.105,.04),'Edge','cylinder',(math.pi/2-.65,0,0))
  for x in [-.2,.2]:part('Bipod',(x,.3,.25),(.025,.025,.28),'Steel','cylinder',(0,x,0))
  part('Sight',(-.13,.14,.5),(.028,.065,.035),'Dark')
  part('Ammo case',(-.47,-.22,.13),(.14,.23,.12),'Wood')
  for y in [-.3,-.15]:part('Mortar round',(-.47,y,.28),(.04,.04,.12),'Sand','sphere')
 # Traverse only the mechanism; the sandbags remain fixed on their emplacement.
 pivot=bpy.data.objects.new('Traverse',None);bpy.context.collection.objects.link(pivot)
 for obj in set(bpy.context.scene.objects)-before:
  if obj!=pivot:obj.parent=pivot

def tree():
 rng=random.Random(74)
 part('Root flare',(0,0,.2),(.2,.17,.2),'Bark','sphere')
 part('Trunk',(0,0,.85),(.095,.095,.8),'Bark','cylinder',(.035,.05,0))
 for i in range(5):
  angle=i*math.tau/5
  part('Branch',(math.cos(angle)*.23,math.sin(angle)*.23,1.3),(.055,.055,.48),'Bark','cylinder',(math.sin(angle)*.6,math.cos(angle)*.6,0))
 # Broadleaf bocage crowns: irregular overlapping masses rather than stacked cones.
 for i in range(19):
  angle=i*2.39996;r=.65*math.sqrt(rng.random());z=1.7+rng.random()*.7
  part('Leaf crown',(math.cos(angle)*r,math.sin(angle)*r,z),(.42+rng.random()*.21,.4+rng.random()*.2,.38+rng.random()*.22),['Leaf','LeafLight','LeafDark'][i%3],'sphere')
 part('Crown top',(.07,0,2.45),(.5,.47,.43),'LeafLight','sphere')

def hut():
 part('Walls',(0,0,.65),(1,.7,.65),'Plaster');part('Door',(.35,.707,.38),(.2,.02,.38),'Wood')
 for x in [-.5,.5]:part('Window',(x,.715,.9),(.16,.025,.16),'Dark')
 for x in [-1,1]:part('Roof',(x*.51,0,1.55),(.62,.83,.07),'Roof',rot=(0,x*-.48,0))
 part('Chimney',(-.5,0,1.85),(.13,.14,.42),'Roof')

def crate():
 part('Crate',(0,0,.24),(.32,.25,.24),'Wood')
 for x in [-.25,.25]:part('Strap',(x,0,.24),(.025,.26,.25),'Sand')

only=set(sys.argv[sys.argv.index('--only')+1].split(',')) if '--only' in sys.argv else None
for name,fn in [('Infantry',infantry),('LightTank',tank),('MachineGun',lambda:gun('MachineGun')),('AntiTank',lambda:gun('AntiTank')),('Mortar',lambda:gun('Mortar')),('Pine',tree),('Farmhouse',hut),('Crate',crate),('Sandbags',bags)]:
 scene=bpy.data.scenes.new(name); bpy.context.window.scene=scene; fn()
 bpy.ops.object.select_all(action='SELECT');bpy.ops.object.transform_apply(location=False,rotation=False,scale=True)
 if only and name not in only: continue
 bpy.ops.export_scene.fbx(filepath=os.path.join(out,name+'.fbx'),use_selection=True,axis_forward='-Z',axis_up='Y',apply_unit_scale=True,bake_anim=False,add_leaf_bones=False)
bpy.ops.wm.save_as_mainfile(filepath=os.path.join(root,'Tools/Blender/miniatures.blend'))
print('Generated original diorama models', only or 'all')
