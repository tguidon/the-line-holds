"""Run: Blender --background --python Tools/Blender/generate_assets.py -- <repo>.
Original procedural models, metres, +Z forward in Unity. No external assets.
"""
import bpy, math, os, sys
from mathutils import Vector
root = sys.argv[sys.argv.index('--')+1]
out = os.path.join(root, 'TheLineHolds/Assets/TheLineHolds/Art/Models')
os.makedirs(out, exist_ok=True)
materials={}
for name,c in {'Olive':(.25,.29,.16,1),'Dark':(.075,.085,.065,1),'Steel':(.19,.22,.19,1),'Cloth':(.36,.39,.25,1),'Skin':(.59,.43,.3,1),'Sand':(.51,.44,.29,1),'Wood':(.25,.17,.10,1),'Leaf':(.25,.32,.15,1),'Plaster':(.62,.57,.43,1),'Roof':(.27,.24,.20,1)}.items():
 m=bpy.data.materials.new(name); m.diffuse_color=c; materials[name]=m

def part(name,loc,scale,mat='Olive',shape='cube',rot=None):
 if shape=='sphere': bpy.ops.mesh.primitive_uv_sphere_add(segments=12,ring_count=6,location=loc)
 elif shape=='cylinder': bpy.ops.mesh.primitive_cylinder_add(vertices=12,radius=1,depth=2,location=loc)
 elif shape=='cone': bpy.ops.mesh.primitive_cone_add(vertices=9,radius1=1,radius2=.06,depth=2,location=loc)
 else: bpy.ops.mesh.primitive_cube_add(size=2,location=loc)
 o=bpy.context.object; o.name=name; o.scale=scale
 if rot: o.rotation_euler=rot
 o.data.materials.append(materials[mat]); return o

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
 if kind=='MachineGun':
  part('Mount',(0,0,.29),(.04,.04,.26),'Steel','cylinder');part('Receiver',(0,.13,.55),(.1,.24,.09),'Steel');barrel((0,.56,.56),.48,.035)
  part('Ammo',(-.22,.05,.33),(.11,.15,.12),'Olive')
 elif kind=='AntiTank':
  for x in [-.42,.42]:part('Wheel',(x,-.05,.3),(.26,.26,.08),'Dark','cylinder',(0,math.pi/2,0))
  part('Shield',(0,.05,.55),(.4,.055,.29));barrel((0,.65,.6),1.3,.065)
  for x in [-.26,.26]:part('Trail',(x,-.55,.18),(.045,.5,.045),'Steel',rot=(0,0,x))
 else:
  part('Base',(0,0,.08),(.3,.3,.05),'Steel','cylinder');barrel((0,.1,.4),.7,.09,-.65)
  for x in [-.2,.2]:part('Bipod',(x,.3,.25),(.025,.025,.28),'Steel','cylinder',(0,x,0))

def tree():
 part('Trunk',(0,0,.85),(.11,.11,.85),'Wood','cylinder')
 for z,s in [(1.3,.85),(1.85,.64),(2.35,.43)]:part('Pine',(0,0,z),(s,s,.65),'Leaf','cone')

def hut():
 part('Walls',(0,0,.65),(1,.7,.65),'Plaster');part('Door',(.35,.707,.38),(.2,.02,.38),'Wood')
 for x in [-.5,.5]:part('Window',(x,.715,.9),(.16,.025,.16),'Dark')
 for x in [-1,1]:part('Roof',(x*.51,0,1.55),(.62,.83,.07),'Roof',rot=(0,x*-.48,0))
 part('Chimney',(-.5,0,1.85),(.13,.14,.42),'Roof')

def crate():
 part('Crate',(0,0,.24),(.32,.25,.24),'Wood')
 for x in [-.25,.25]:part('Strap',(x,0,.24),(.025,.26,.25),'Sand')

for name,fn in [('Infantry',infantry),('LightTank',tank),('MachineGun',lambda:gun('MachineGun')),('AntiTank',lambda:gun('AntiTank')),('Mortar',lambda:gun('Mortar')),('Pine',tree),('Farmhouse',hut),('Crate',crate),('Sandbags',bags)]:
 scene=bpy.data.scenes.new(name); bpy.context.window.scene=scene; fn()
 bpy.ops.object.select_all(action='SELECT');bpy.ops.object.transform_apply(location=False,rotation=False,scale=True)
 bpy.ops.export_scene.fbx(filepath=os.path.join(out,name+'.fbx'),use_selection=True,axis_forward='-Z',axis_up='Y',apply_unit_scale=True,bake_anim=False,add_leaf_bones=False)
bpy.ops.wm.save_as_mainfile(filepath=os.path.join(root,'Tools/Blender/miniatures.blend'))
print('Generated 9 original miniature models')
