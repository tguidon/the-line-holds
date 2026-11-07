"""Run: Blender --background --python Tools/Blender/generate_assets.py -- <repo>.
Original procedural models, metres, +Z forward in Unity. No external assets.
"""
import bpy, math, os, sys, random
from mathutils import Vector
root = sys.argv[sys.argv.index('--')+1]
out = os.path.join(root, 'TheLineHolds/Assets/TheLineHolds/Art/Models')
os.makedirs(out, exist_ok=True)
materials={}
for name,c in {'Olive':(.25,.29,.16,1),'Dark':(.075,.085,.065,1),'Steel':(.19,.22,.19,1),'Cloth':(.36,.39,.25,1),'Skin':(.59,.43,.3,1),'Sand':(.51,.44,.29,1),'Wood':(.25,.17,.10,1),'Leaf':(.25,.32,.15,1),'Plaster':(.62,.57,.43,1),'Roof':(.27,.24,.20,1),'LeafLight':(.48,.56,.32,1),'LeafDark':(.18,.29,.17,1),'Bark':(.36,.28,.18,1),'Leather':(.28,.21,.13,1),'Edge':(.39,.44,.28,1),'Badge':(.72,.67,.49,1),'BagCloth':(.51,.44,.29,1)}.items():
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
  for y in [-.48,-.24,0,.24,.48]:
   part('RoadWheel',(x*1.29,y,.24),(.115,.115,.065),'Steel','cylinder',(0,math.pi/2,0))
   part('Wheel hub',(x*1.44,y,.24),(.045,.045,.02),'Edge','cylinder',(0,math.pi/2,0))
  for y in [-.57,-.4,-.23,-.06,.11,.28,.45,.62]:
   part('Tread shoe',(x,y,.426),(.15,.025,.016),'Steel')
 part('Hull',(0,0,.46),(.43,.64,.21))
 part('Sloped glacis',(0,.46,.57),(.405,.22,.12),'Edge',rot=(.28,0,0))
 part('Turret',(0,.03,.77),(.29,.3,.17),'Olive','cylinder')
 part('Mantlet',(0,.31,.8),(.14,.09,.11),'Edge','sphere')
 part('Hatch',(0,.02,.96),(.15,.15,.025),'Steel','cylinder');barrel((0,.72,.82),1)
 part('Muzzle',(0,1.21,.82),(.055,.07,.055),'Dark')
 part('Hatch handle',(0,.02,1.005),(.07,.025,.018),'Dark')
 for y in [-.53,-.46,-.39,-.32]:part('Engine grille',(0,y,.686),(.25,.012,.009),'Dark')
 for x in [-.24,.24]:
  part('Exhaust',(x,-.63,.54),(.045,.045,.12),'Steel','cylinder',(math.pi/2,0,0))
  part('Headlight',(x,.635,.52),(.052,.025,.052),'Sand','sphere')
 part('Stowage roll',(.28,-.4,.74),(.12,.17,.075),'Cloth','sphere')
 for y in [-.49,-.32]:part('Stowage strap',(.28,y,.74),(.125,.015,.08),'Leather')

def limb(name,objects,pivot):
 node=bpy.data.objects.new(name,None);bpy.context.collection.objects.link(node);node.location=pivot
 bpy.context.view_layer.update()
 for obj in objects:
  matrix=obj.matrix_world.copy();obj.parent=node;obj.matrix_world=matrix

def infantry():
 for x,label in [(-.1,'StrideLeft'),(.1,'StrideRight')]:
  boot=part('Boot',(x,.06,.08),(.075,.14,.07),'Dark')
  leg=part('Trouser',(x,0,.27),(.075,.08,.18),'Cloth','sphere')
  limb(label,[boot,leg],(x,0,.43))
 part('Coat',(0,0,.56),(.185,.13,.22),'Cloth','sphere')
 part('Coat hem',(0,0,.43),(.17,.12,.06),'Cloth')
 part('Belt',(0,0,.46),(.185,.135,.022),'Leather')
 part('Buckle',(0,.143,.46),(.035,.012,.025),'Sand')
 part('Head',(0,.01,.85),(.105,.1,.115),'Skin','sphere')
 part('Helmet brim',(0,0,.925),(.15,.14,.025),'Olive','sphere')
 part('Helmet crown',(0,-.015,.96),(.135,.125,.075),'Olive','sphere')
 for x in [-.22,.22]:
  part('Sleeve',(x,.04,.62),(.066,.075,.15),'Cloth','sphere',(-.28,0,-x*.5))
  part('Hand',(x,.1,.52),(.045,.05,.05),'Skin','sphere')
 part('Pack',(0,-.155,.6),(.135,.07,.14),'Sand')
 for x in [-.115,.115]:part('Pack strap',(x,.134,.6),(.018,.014,.14),'Leather',rot=(0,x,0))
 for x in [-.12,.12]:part('Belt pouch',(x,.15,.46),(.05,.033,.05),'Sand')
 part('Rifle stock',(.18,.16,.49),(.03,.16,.035),'Wood');barrel((.18,.32,.57),.48,.023)

def bags(radius=.68,count=14):
 # Complete, staggered courses; long axes follow the circle instead of radiating like petals.
 rng=random.Random(618)
 for row in range(2):
  for i in range(count):
   angle=(i+row*.5)*math.tau/count
   r=radius+rng.uniform(-.008,.008)
   part('Sandbag',(math.cos(angle)*r,math.sin(angle)*r,.10+row*.155),(.163,.115,.09),'BagCloth','sphere',(.025*math.sin(i),.02*math.cos(i),angle+math.pi/2))

def gun(kind):
 bags(.79,16) if kind=='AntiTank' else bags()
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
  for x in [-.2,.2]:part('Trail',(x,-.32,.18),(.045,.24,.045),'Steel',rot=(0,0,x))
  part('Elevation handwheel',(.19,-.2,.52),(.09,.09,.02),'Steel','cylinder',(0,math.pi/2,0))
 else:
  part('Base plate',(0,0,.07),(.3,.3,.045),'Steel','cylinder');barrel((0,.1,.4),.7,.09,-.65)
  part('Tube collar',(0,.22,.57),(.105,.105,.04),'Edge','cylinder',(math.pi/2-.65,0,0))
  for x in [-.2,.2]:part('Bipod',(x,.3,.25),(.025,.025,.28),'Steel','cylinder',(0,x,0))
  part('Sight',(-.13,.14,.5),(.028,.065,.035),'Dark')
  part('Ammo case',(-.34,-.16,.13),(.11,.17,.12),'Wood')
  for y in [-.24,-.1]:part('Mortar round',(-.34,y,.28),(.04,.04,.12),'Sand','sphere')
 # Traverse only the mechanism; the sandbags remain fixed on their emplacement.
 pivot=bpy.data.objects.new('Traverse',None);bpy.context.collection.objects.link(pivot)
 for obj in set(bpy.context.scene.objects)-before:
  if obj!=pivot:obj.parent=pivot

def branch(a,b,radius=.04):
 delta=Vector(b)-Vector(a)
 rotation=delta.to_track_quat('Z','Y').to_euler()
 return part('Branch',(Vector(a)+Vector(b))/2,(radius,radius,delta.length/2),'Bark','cylinder',rotation)

def foliage(name,clusters,count,seed):
 rng=random.Random(seed);vertices=[];faces=[];uvs=[];shades=[]
 # Ridged, pointed leaf geometry. Each leaf gets the same UV motif so veins retain real scale.
 outline=[(.5,0),(.16,.16),(0,.4),(.13,.73),(.5,1),(.87,.73),(1,.4),(.84,.16)]
 for centre,radii in clusters:
  for i in range(count):
   direction=Vector((rng.uniform(-1,1),rng.uniform(-1,1),rng.uniform(-1,1))).normalized()
   radius=rng.random()**(1/3)
   position=Vector(centre)+Vector((direction.x*radii[0],direction.y*radii[1],direction.z*radii[2]))*radius
   normal=(direction+Vector((0,0,.35))).normalized()
   rotation=normal.to_track_quat('Z','Y')
   twist=rng.random()*math.tau
   length=rng.uniform(.14,.24);width=length*rng.uniform(.5,.68)
   start=len(vertices)
   for u,v in outline+[(.5,.46)]:
    x=(u-.5)*width;y=(v-.46)*length
    z=.012 if (u,v)==(.5,.46) else -.006*abs(v-.46)
    local=Vector((x*math.cos(twist)-y*math.sin(twist),x*math.sin(twist)+y*math.cos(twist),z))
    vertices.append(position+rotation@local);uvs.append((u,v))
   shade=rng.choices([0,1,2],[.5,.3,.2])[0]
   for j in range(8):faces.append((start+8,start+(j+1)%8,start+j));shades.append(shade)
 mesh=bpy.data.meshes.new(name);mesh.from_pydata(vertices,[],faces);mesh.update()
 layer=mesh.uv_layers.new(name='LeafUV')
 for polygon in mesh.polygons:
  polygon.material_index=shades[polygon.index]
  for index in polygon.loop_indices:layer.data[index].uv=uvs[mesh.loops[index].vertex_index]
 obj=bpy.data.objects.new(name,mesh);bpy.context.collection.objects.link(obj)
 for material in ['Leaf','LeafLight','LeafDark']:mesh.materials.append(materials[material])
 return obj

def tree():
 part('Trunk',(0,0,.67),(.095,.105,.67),'Bark','cylinder',(.025,.04,0))
 clusters=[]
 for i in range(5):
  angle=i*2.39996
  endpoint=(math.cos(angle)*.5,math.sin(angle)*.5,1.55+(i%3)*.22)
  branch((0,0,.7),endpoint,.045)
  branch(endpoint,(endpoint[0]*1.45,endpoint[1]*1.45,endpoint[2]+.25),.018)
  clusters.append(((endpoint[0],endpoint[1],endpoint[2]+.25),(.55,.51,.53)))
 clusters.append(((0,0,2.22),(.48,.45,.45)))
 foliage('Broadleaf canopy',clusters,125,74)

def bush():
 for i in range(5):
  angle=i*2.39996
  branch((0,0,.02),(math.cos(angle)*.26,math.sin(angle)*.23,.35+(i%2)*.12),.012)
 foliage('Shrub leaves',[((-.16,0,.27),(.35,.29,.27)),((.17,.06,.36),(.33,.28,.3)),((.02,-.12,.2),(.28,.3,.21))],85,93)

def hut():
 part('Walls',(0,0,.65),(1,.7,.65),'Plaster');part('Door',(.35,.707,.38),(.2,.02,.38),'Wood')
 for x in [-.5,.5]:part('Window',(x,.715,.9),(.16,.025,.16),'Dark')
 for x in [-1,1]:part('Roof',(x*.51,0,1.55),(.62,.83,.07),'Roof',rot=(0,x*-.48,0))
 part('Chimney',(-.5,0,1.85),(.13,.14,.42),'Roof')

def crate():
 part('Crate',(0,0,.24),(.32,.25,.24),'Wood')
 for x in [-.25,.25]:part('Strap',(x,0,.24),(.025,.26,.25),'Sand')

only=set(sys.argv[sys.argv.index('--only')+1].split(',')) if '--only' in sys.argv else None
for name,fn in [('Infantry',infantry),('LightTank',tank),('MachineGun',lambda:gun('MachineGun')),('AntiTank',lambda:gun('AntiTank')),('Mortar',lambda:gun('Mortar')),('Pine',tree),('Bush',bush),('Farmhouse',hut),('Crate',crate),('Sandbags',bags)]:
 scene=bpy.data.scenes.new(name); bpy.context.window.scene=scene; fn()
 bpy.ops.object.select_all(action='SELECT');bpy.ops.object.transform_apply(location=False,rotation=False,scale=True)
 if only and name not in only: continue
 bpy.ops.export_scene.fbx(filepath=os.path.join(out,name+'.fbx'),use_selection=True,axis_forward='-Z',axis_up='Y',apply_unit_scale=True,bake_anim=False,add_leaf_bones=False)
bpy.ops.wm.save_as_mainfile(filepath=os.path.join(root,'Tools/Blender/miniatures.blend'))
print('Generated original diorama models', only or 'all')
