"""Run: Blender --background --python Tools/Blender/generate_assets.py -- <repo>.
Original procedural models, metres, +Z forward in Unity. No external assets.
"""
import bpy, bmesh, math, os, sys, random
from mathutils import Vector
root = sys.argv[sys.argv.index('--')+1]
out = os.path.join(root, 'TheLineHolds/Assets/TheLineHolds/Art/Models')
os.makedirs(out, exist_ok=True)
materials={}
for name,c in {'Olive':(.25,.29,.16,1),'Dark':(.075,.085,.065,1),'Steel':(.19,.22,.19,1),'Cloth':(.36,.39,.25,1),'Skin':(.59,.43,.3,1),'Sand':(.51,.44,.29,1),'Wood':(.25,.17,.10,1),'Leaf':(.25,.32,.15,1),'Plaster':(.62,.57,.43,1),'Roof':(.27,.24,.20,1),'LeafLight':(.48,.56,.32,1),'LeafDark':(.18,.29,.17,1),'Bark':(.36,.28,.18,1),'Leather':(.28,.21,.13,1),'Edge':(.39,.44,.28,1),'Badge':(.72,.67,.49,1),'BagCloth':(.51,.44,.29,1),'Masonry':(.57,.55,.47,1),'RoofSlate':(.28,.33,.32,1),'RoofSlateLight':(.34,.39,.36,1),'RoofTrim':(.19,.23,.22,1),'WindowGlass':(.13,.2,.22,1),'Shutter':(.27,.35,.26,1),'SignPaint':(.65,.6,.43,1),'RoofClay':(.43,.25,.16,1),'RoofClayLight':(.49,.31,.21,1),'StuccoCream':(.67,.62,.49,1),'ShedPaint':(.28,.37,.32,1)}.items():
 m=bpy.data.materials.new(name); m.diffuse_color=c; materials[name]=m

def part(name,loc,scale,mat='Olive',shape='cube',rot=None,edge=None):
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
  bevel=o.modifiers.new('Soft manufactured edges','BEVEL');bevel.width=min(scale)*.23 if edge is None else edge;bevel.segments=3
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
   wheel=part('RoadWheel',(x*1.29,y,.24),(.115,.115,.065),'Steel','cylinder',(0,math.pi/2,0))
   bolt=part('Wheel bolt',(x*1.45,y+.06,.24),(.018,.018,.018),'Edge','sphere')
   matrix=bolt.matrix_world.copy();bolt.parent=wheel;bolt.matrix_world=matrix
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
 limb('Carry',[o for o in bpy.context.scene.objects if o.type=='MESH' and o.name.startswith(('Sleeve','Hand','Rifle stock','Barrel'))],(0,0,.6))

def scout():
 infantry()
 for o in list(bpy.context.scene.objects):
  if o.type=='MESH' and (o.name.startswith('Pack') or o.name.startswith('Helmet')):bpy.data.objects.remove(o,do_unlink=True)
  elif o.type=='MESH' and o.name.startswith(('Coat','Sleeve','Trouser')):o.data.materials[0]=materials['Sand']
 part('Field cap',(0,0,.95),(.125,.13,.055),'Cloth','sphere')
 part('Cap peak',(0,.12,.93),(.11,.075,.015),'Cloth')

def assault():
 infantry()
 part('Armored vest',(0,.13,.57),(.16,.045,.14),'Steel',edge=.012)
 for x in [-.2,.2]:part('Shoulder pad',(x,.02,.76),(.09,.11,.05),'Olive','sphere')

def vehicle(kind):
 for x in [-.42,.42]:
  for y in [-.55,.55]:
   wheel=part('Rolling wheel',(x,y,.23),(.18,.18,.09),'Dark','cylinder',(0,math.pi/2,0))
   details=[part('Wheel hub',(x*1.23,y,.23),(.055,.055,.02),'Steel','cylinder',(0,math.pi/2,0))]
   for z in [-.08,.08]:details.append(part('Wheel lug',(x*1.25,y,.23+z),(.02,.02,.02),'Sand','sphere'))
   for detail in details:
    matrix=detail.matrix_world.copy();detail.parent=wheel;detail.matrix_world=matrix
 part('Chassis',(0,0,.31),(.42,.86,.08),'Steel',edge=.015)
 if kind=='Truck':
  part('Cab',(0,.47,.63),(.34,.27,.27),'Olive',edge=.025)
  part('Windshield',(0,.745,.71),(.27,.012,.1),'WindowGlass',edge=.003)
  part('Bonnet',(0,.8,.49),(.34,.15,.12),'Olive',edge=.018)
  part('Canvas bed',(0,-.32,.65),(.37,.48,.28),'Sand',edge=.07)
  for y in [-.7,-.35,0]:part('Canvas rib',(0,y,.91),(.375,.014,.024),'Leather')
  part('Tailgate',(0,-.825,.48),(.38,.03,.14),'Wood')
 else:
  part('Angled hull',(0,0,.53),(.38,.69,.22),'Edge',edge=.04)
  part('Scout turret',(0,-.1,.8),(.22,.22,.13),'Olive','cylinder')
  barrel((0,.3,.83),.65,.035)
  part('Aerial',(-.24,-.42,.98),(.012,.012,.3),'Steel','cylinder')
  part('Hatch',(0,-.1,.94),(.12,.12,.02),'Steel','cylinder')
 for x in [-.25,.25]:part('Headlight',(x,.9,.5),(.045,.025,.045),'Sand','sphere')

def heavy_tank():
 tank()
 part('Turret cheek L',(-.28,.11,.78),(.09,.3,.14),'Steel',edge=.018)
 part('Turret cheek R',(.28,.11,.78),(.09,.3,.14),'Steel',edge=.018)
 part('Heavy muzzle',(0,1.28,.82),(.08,.11,.08),'Steel')
 root=bpy.data.objects.new('Heavy chassis',None);bpy.context.collection.objects.link(root)
 for o in list(bpy.context.scene.objects):
  if o!=root and o.parent is None:o.parent=root
 root.scale=(1.1,1.1,1.1)

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
  part('Base plate',(0,0,.07),(.3,.3,.045),'Steel','cylinder');barrel((0,.1,.4),.7,.09,.65)
  part('Tube collar',(0,.28,.54),(.105,.105,.04),'Edge','cylinder',(math.pi/2+.65,0,0))
  for x in [-.2,.2]:part('Bipod',(x,.3,.25),(.025,.025,.28),'Steel','cylinder',(0,x,0))
  part('Sight',(-.13,.14,.5),(.028,.065,.035),'Dark')
  part('Ammo case',(-.34,-.16,.13),(.11,.17,.12),'Wood')
  for y in [-.24,-.1]:part('Mortar round',(-.34,y,.28),(.04,.04,.12),'Sand','sphere')
 # Traverse only the mechanism; the sandbags remain fixed on their emplacement.
 pivot=bpy.data.objects.new('Traverse',None);bpy.context.collection.objects.link(pivot)
 for obj in set(bpy.context.scene.objects)-before:
  if obj!=pivot:obj.parent=pivot
 # Moving internals are separate from wheels, shield, supports and sandbag walls.
 slide=bpy.data.objects.new('Recoil',None);bpy.context.collection.objects.link(slide);slide.parent=pivot
 prefixes={'MachineGun':('Receiver','Barrel','Heat sleeve','Cooling ring','Stock','Rear sight'),
           'AntiTank':('Barrel','Breech','Recoil sleeve','Muzzle brake'),
           'Mortar':('Barrel','Tube collar')}[kind]
 for obj in list(pivot.children):
  if obj.name.startswith(prefixes):obj.parent=slide
 muzzle=bpy.data.objects.new('Muzzle socket',None);bpy.context.collection.objects.link(muzzle);muzzle.parent=slide
 muzzle.location={'MachineGun':(0,.82,.56),'AntiTank':(0,1.36,.6),'Mortar':(0,.38,.612)}[kind]

def upgraded_gun(kind,branch=''):
 gun(kind)
 pivot=next(o for o in bpy.context.scene.objects if o.name.startswith('Traverse'))
 pivot.scale=(1.14,1.14,1.14) if not branch else (1.3,1.3,1.3)
 before=set(bpy.context.scene.objects)
 if kind=='MachineGun':
  part('Armored receiver shield',(0,.35,.59),(.24,.035,.23),'Olive',edge=.015)
  part('Shield brass strip',(0,.31,.8),(.24,.025,.025),'Sand',edge=.004)
  if branch=='A':
   for x in [-.13,.13]:barrel((x,.62,.6),.65,.045)
   part('Linked ammo box',(-.3,0,.45),(.12,.22,.15),'Olive')
  elif branch=='B':
   barrel((0,.8,.65),.98,.065);part('AP sight',(.12,.18,.88),(.025,.13,.025),'Steel')
 elif kind=='AntiTank':
  part('Reinforced gun shield',(0,.02,.66),(.49,.04,.36),'Steel',edge=.02)
  for x in [-.4,.4]:part('Identification band',(x,-.026,.65),(.025,.018,.23),'Sand')
  if branch=='A':
   barrel((0,.97,.68),1.9,.085);part('Long range optics',(.28,-.1,.94),(.035,.17,.035),'Dark')
  elif branch=='B':
   barrel((0,.61,.7),1,.14);part('Heavy muzzle collar',(0,1.07,.7),(.19,.1,.18),'Dark')
 else:
  part('Elevated base',(0,0,.18),(.36,.36,.12),'Olive','cylinder')
  if branch=='B':
   for x in [-.17,.17]:
    part('Twin mortar tube',(x,.16,.61),(.105,.105,.44),'Steel','cylinder',(math.pi/2+.65,0,0))
  elif branch=='A':part('Siege tube',(0,.18,.65),(.17,.17,.53),'Steel','cylinder',(math.pi/2+.65,0,0))
  else:part('Mortar armored collar',(0,.18,.5),(.15,.15,.11),'Olive','cylinder',(math.pi/2+.65,0,0))
 for obj in set(bpy.context.scene.objects)-before:
  # Kit uses the same authored coordinates as the mechanism, and traverses with it.
  obj.parent=pivot
 muzzle=next(o for o in bpy.context.scene.objects if o.name.startswith('Muzzle socket'))
 if branch:
  muzzle.location=({'MachineGun':(0,.98,.6),'AntiTank':(0,1.94,.68),'Mortar':(0,.52,.94)} if branch=='A' else {'MachineGun':(0,1.31,.65),'AntiTank':(0,1.19,.7),'Mortar':(0,.43,.86)})[kind]
 for x in [-.5,.5]:part('Additional supply crate',(x,-.4,.2),(.12,.16,.17),'Olive')

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
 # Architecture gets millimetre-scale edge breaks, not the broad soft bevel used on miniatures.
 part('Stone footing',(0,0,.12),(1.025,.725,.12),'Masonry',edge=.006)
 part('Plaster walls',(0,0,.75),(1,.7,.6),'Plaster',edge=.008)
 # Solid plaster gables meet the underside of the two roof slopes.
 vertices=[(-1,-.7,1.34),(1,-.7,1.34),(0,-.7,1.99),(-1,.7,1.34),(1,.7,1.34),(0,.7,1.99)]
 mesh=bpy.data.meshes.new('Solid gables');mesh.from_pydata(vertices,[],[(0,2,1),(3,4,5),(0,1,4,3),(1,2,5,4),(2,0,3,5)])
 bm=bmesh.new();bm.from_mesh(mesh);bmesh.ops.recalc_face_normals(bm,faces=bm.faces);bm.to_mesh(mesh);bm.free()
 gables=bpy.data.objects.new('Solid gables',mesh);bpy.context.collection.objects.link(gables);mesh.materials.append(materials['Plaster'])
 # Both wings descend from the same ridge to matching eaves; no inverted valley or floating slabs.
 run,rise,depth=1.14,.75,.86;angle=math.atan2(rise,run);slope=math.hypot(run,rise)
 for sign in [-1,1]:
  part('Roof wing',(sign*run/2,0,2.03-rise/2),(slope/2,depth,.035),'RoofTrim',rot=(0,sign*angle,0),edge=.003)
  for row in range(8):
   t=(row+.5)/8
   for column in range(-1,11):
    lo=max(-depth,-depth+(column+(row%2)*.5)*depth/5)
    hi=min(depth,-depth+(column+1+(row%2)*.5)*depth/5)
    if hi-lo<.03:continue
    normal=Vector((sign*math.sin(angle),0,math.cos(angle)))
    centre=Vector((sign*run*t,(lo+hi)/2,2.03-rise*t))+normal*.043
    part('Slate tile',centre,(slope/16+.008,(hi-lo)/2-.004,.009),'RoofSlateLight' if (row*7+column*3)%9==0 else 'RoofSlate',rot=(0,sign*angle,0),edge=.002)
  part('Eave fascia',(sign*1.14,0,1.27),(.025,.89,.045),'RoofTrim',edge=.003)
 part('Ridge cap',(0,0,2.085),(.07,.89,.025),'RoofSlateLight',edge=.004)
 # Front and back have framed glass and crisp timber shutters.
 for side in [-1,1]:
  y=side*.716
  for x in [-.58,.58]:
   part('Window frame',(x,y,.87),(.205,.018,.225),'Wood',edge=.004)
   part('Glass pane',(x,y+side*.022,.87),(.175,.009,.19),'WindowGlass',edge=.002)
   part('Window mullion',(x,y+side*.034,.87),(.012,.012,.19),'Wood',edge=.002)
   part('Window crossbar',(x,y+side*.036,.87),(.18,.012,.013),'Wood',edge=.002)
   part('Stone sill',(x,y+side*.025,.637),(.23,.055,.025),'Masonry',edge=.003)
   for shutter in [-1,1]:
    sx=x+shutter*.29
    part('Shutter',(sx,y,.87),(.071,.026,.215),'Shutter',edge=.003)
    for z in [.75,.97]:part('Shutter strap',(sx,y+side*.029,z),(.065,.008,.012),'RoofTrim',edge=.001)
 for side in [-1,1]:
  part('Side window frame',(side*1.016,0,.87),(.018,.205,.225),'Wood',edge=.004)
  part('Side glass',(side*1.04,0,.87),(.009,.175,.19),'WindowGlass',edge=.002)
  part('Side mullion',(side*1.052,0,.87),(.012,.012,.19),'Wood',edge=.002)
  part('Side crossbar',(side*1.054,0,.87),(.012,.18,.013),'Wood',edge=.002)
  part('Side sill',(side*1.04,0,.637),(.055,.23,.025),'Masonry',edge=.003)
 part('Door surround',(0,.72,.55),(.225,.025,.43),'Masonry',edge=.004)
 part('Timber door',(0,.754,.54),(.18,.015,.385),'Wood',edge=.004)
 for x in [-.09,0,.09]:part('Door plank joint',(x,.771,.54),(.004,.004,.37),'RoofTrim',edge=.001)
 part('Door lintel',(0,.76,.995),(.25,.05,.035),'Masonry',edge=.003)
 part('Door latch',(.12,.779,.57),(.012,.012,.022),'Steel','sphere')
 part('Doorstep',(0,.83,.065),(.28,.2,.065),'Masonry',edge=.005)
 part('Square chimney',(-.57,-.25,1.94),(.12,.14,.42),'Masonry',edge=.004)
 part('Chimney flashing',(-.57,-.25,1.71),(.17,.2,.015),'RoofTrim',rot=(0,-angle,0),edge=.002)
 for z in [1.75,1.91,2.07,2.23]:part('Chimney mortar joint',(-.57,-.25,z),(.122,.142,.006),'RoofTrim',edge=.001)
 part('Chimney cap',(-.57,-.25,2.385),(.16,.18,.025),'RoofTrim',edge=.003)
 part('Chimney opening',(-.57,-.25,2.413),(.085,.10,.004),'Dark',edge=.001)
 # Keep architectural detail inexpensive: combine static parts by material after authoring.
 groups={}
 for obj in list(bpy.context.scene.objects):
  if obj.type=='MESH':groups.setdefault(obj.data.materials[0].name,[]).append(obj)
 for material,objects in groups.items():
  bpy.ops.object.select_all(action='DESELECT')
  for obj in objects:obj.select_set(True)
  bpy.context.view_layer.objects.active=objects[0];bpy.ops.object.join();bpy.context.object.name='House '+material

def crate():
 part('Crate',(0,0,.24),(.32,.25,.24),'Wood')
 for x in [-.25,.25]:part('Strap',(x,0,.24),(.025,.26,.25),'Sand')

def combine_static(prefix):
 groups={}
 for obj in list(bpy.context.scene.objects):
  if obj.type=='MESH':groups.setdefault(obj.data.materials[0].name,[]).append(obj)
 for material,objects in groups.items():
  bpy.ops.object.select_all(action='DESELECT')
  for obj in objects:obj.select_set(True)
  bpy.context.view_layer.objects.active=objects[0];bpy.ops.object.join();bpy.context.object.name=prefix+' '+material

def gate():
 # Open farm entrance. The 1.8 m corridor accommodates armor without decorative intersections.
 for x in [-1,1]:
  for row in range(3):part('Gate pier',(x*.99,0,.09+row*.17),(.14,.15,.08),'Masonry',edge=.009)
  part('Pier coping',(x*.99,0,.55),(.18,.18,.04),'Masonry',edge=.009)
  part('Gate hinge',(x*.99,0,.77),(.055,.055,.2),'Wood',edge=.006)
  # Gate leaves lie open along the verge, rather than crossing the lane.
  for y in [-.26,-.75]:part('Open gate upright',(x*1.10,y,.39),(.04,.04,.36),'Wood',edge=.005)
  for z in [.22,.62]:part('Open gate rail',(x*1.10,-.5,z),(.035,.38,.035),'Wood',edge=.004)
 combine_static('Farm gate')

def checkpoint():
 for x in [-1,1]:
  part('Checkpoint timber',(x*1.04,0,.47),(.065,.07,.47),'Wood',edge=.006)
  part('Timber pale tip',(x*1.04,0,.94),(.08,.085,.025),'Sand',edge=.003)
  for i in range(4):part('Verge sandbag',(x*(1.04+(i%2)*.19),-.16-i//2*.22,.10),(.12,.17,.09),'BagCloth','sphere')
 # Raised boom stays above the road, with its support on the verge.
 part('Raised boom',(-1.04,0,1.10),(.035,.035,.55),'Sand',rot=(0,-.6,0),edge=.004)
 part('Boom hinge',(-1.04,0,.7),(.1,.09,.08),'Steel',edge=.008)
 combine_static('Road checkpoint')

def signpost():
 part('Sign post',(0,0,.6),(.045,.045,.6),'Wood',edge=.004)
 part('Sign board',(0,0,1.13),(.5,.045,.17),'Wood',edge=.006)
 # Two printed faces; fixed geometry cannot detach as the player orbits the camera.
 for side in [-1,1]:
  verts=[(-.47,side*.047,.985),(.47,side*.047,.985),(.47,side*.047,1.275),(-.47,side*.047,1.275)]
  m=bpy.data.meshes.new('Sign face');m.from_pydata(verts,[],[(0,1,2,3)] if side<0 else [(3,2,1,0)])
  uv=m.uv_layers.new(name='SignUV');coords=[(0,0),(1,0),(1,1),(0,1)]
  if side>0:coords=[(1,0),(0,0),(0,1),(1,1)]
  for loop in m.loops:uv.data[loop.index].uv=coords[loop.vertex_index]
  obj=bpy.data.objects.new('Sign face',m);bpy.context.collection.objects.link(obj);m.materials.append(materials['SignPaint'])
 for x in [-.43,.43]:part('Board nail',(x,-.052,1.13),(.012,.008,.012),'Steel','sphere')
 combine_static('Road sign')

def fence():
 for x in [-.65,.65]:part('Fence post',(x,0,.35),(.045,.055,.35),'Wood',edge=.005)
 for z in [.24,.53]:part('Fence rail',(0,0,z),(.69,.03,.035),'Wood',edge=.004)
 part('Fence brace',(0,0,.37),(.65,.022,.025),'Wood',rot=(0,-.22,0),edge=.003)
 combine_static('Farm fence')

def stake():
 part('Survey stake',(0,0,.065),(.035,.035,.095),'Wood',edge=.003)
 part('Pale stake cap',(0,0,.146),(.036,.036,.015),'Sand',edge=.002)

def revetment():
 for i in range(12):
  part('Trench plank',((i-5.5)*.13,0,.14),(.062,.035,.14),'Wood',rot=(0,(i%3-1)*.025,0),edge=.003)
 for x in [-.6,.6]:part('Trench support',(x,-.04,.12),(.035,.055,.16),'Bark',edge=.004)
 combine_static('Field revetment')

def duckboard():
 for i in range(12):part('Duckboard',((i-5.5)*.13,0,.018),(.055,.22,.016),'Wood',edge=.003)
 combine_static('Duckboards')

def fieldstone():
 bpy.ops.mesh.primitive_ico_sphere_add(subdivisions=1,radius=1)
 o=bpy.context.object;o.name='Cut stone';o.data.materials.append(materials['Masonry'])
 for v in o.data.vertices:v.co*=1+.12*math.sin(v.co.x*5+v.co.z*7)
 for p in o.data.polygons:p.use_smooth=True

def barn():
 part('Barn stone base',(0,0,.12),(1.25,1.75,.12),'Masonry',edge=.012)
 part('Barn timber body',(0,0,1),(1.18,1.68,.88),'Wood',edge=.012)
 for side in [-1,1]:
  for y in [-1.5,-1,-.5,0,.5,1,1.5]:part('Barn siding',(side*1.19,y,1),(.018,.025,.86),'Bark',edge=.002)
  part('Barn doors',(side*.47,-1.695,.89),(.45,.025,.77),'Shutter',edge=.006)
  part('Door brace',(side*.47,-1.73,.88),(.025,.025,.7),'Sand',rot=(0,side*.47,0),edge=.003)
  part('Roof pitch',(side*.62,0,2.12),(.77,1.87,.06),'RoofSlate',rot=(0,side*.5,0),edge=.008)
  verts=[(-1.18,side*1.68,1.88),(1.18,side*1.68,1.88),(0,side*1.68,2.53)]
  m=bpy.data.meshes.new('Barn gable');m.from_pydata(verts,[],[(0,1,2)] if side<0 else [(2,1,0)])
  o=bpy.data.objects.new('Barn gable',m);bpy.context.collection.objects.link(o);m.materials.append(materials['Wood'])
 part('Barn ridge',(0,0,2.51),(.07,1.9,.055),'RoofTrim',edge=.008)
 combine_static('Barn')

def roof_gable(hx,hy,eave,rise,mat='RoofSlate'):
 angle=math.atan2(rise,hx);slope=math.hypot(hx,rise)/2
 for side in [-1,1]:
  part('Sloping roof',(side*hx/2,0,eave+rise/2),(slope+.1,hy+.16,.045),mat,rot=(0,side*angle,0),edge=.008)
 for side in [-1,1]:
  m=bpy.data.meshes.new('Closed gable');m.from_pydata([(-hx,side*hy,eave),(hx,side*hy,eave),(0,side*hy,eave+rise)],[],[(0,1,2)] if side<0 else [(2,1,0)])
  o=bpy.data.objects.new('Closed gable',m);bpy.context.collection.objects.link(o);m.materials.append(materials['StuccoCream'])
 # Small overlapping courses echo the original farmhouse rather than a smooth toy roof.
 for side in [-1,1]:
  for row in range(6):
   t=(row+.5)/6
   for col in range(8):
    y=-hy+(col+.5)*hy/4;normal=Vector((side*math.sin(angle),0,math.cos(angle)));centre=Vector((side*hx*t,y,eave+rise*(1-t)))+normal*.052
    light=(row*7+col*3)%9==0
    tile=('RoofClayLight' if light else 'RoofClay') if mat=='RoofClay' else ('RoofSlateLight' if light else 'RoofSlate')
    part('Roof tile',centre,(math.hypot(hx,rise)/12+.006,hy/8-.006,.01),tile,rot=(0,side*angle,0),edge=.002)
 part('Roof ridge',(0,0,eave+rise),(.055,hy+.18,.045),'RoofTrim',edge=.008)

def country_building(kind):
 if kind=='ToolShed':
  part('Shed footing',(0,0,.08),(1.2,.85,.08),'Masonry',edge=.012)
  for x in [-1.1,1.1]:
   for y in [-.74,.74]:part('Exposed shed post',(x,y,.79),(.055,.055,.75),'Wood',edge=.005)
  for i in range(15):part('Rear siding',(-1.05+i*.15,.77,.8),(.07,.035,.7),'ShedPaint',edge=.003)
  part('Lean roof',(0,0,1.58),(1.3,.98,.045),'RoofSlate',rot=(.12,0,0),edge=.006)
  for x in [-.68,0,.68]:part('Stored timber',(x,.31,.26),(.22,.31,.18),'Wood',edge=.006)
  part('Shed brace',(0,.72,1.1),(1.12,.04,.045),'Wood',rot=(0,.2,0),edge=.004)
  combine_static('Open tool shed');return
 hx,hy,height=(1.1,1.28,1.28) if kind=='StoneCottage' else (1.72,1.03,2.35) if kind=='VillageInn' else (1.85,.9,1.25)
 part('Stone footing',(0,0,.13),(hx+.08,hy+.08,.13),'Masonry',edge=.014)
 part('Plaster walls',(0,0,height/2+.15),(hx,hy,height/2),'StuccoCream',edge=.006)
 roof_gable(hx,hy,height+.15,.7 if kind!='Dairy' else .5,'RoofClay' if kind=='VillageInn' else 'RoofSlate')
 # Hand-authored facade: different floor count, window spacing, doorway and trim.
 rows=[.85,1.87] if kind=='VillageInn' else [.83]
 for level in rows:
  for x in [-hx*.64,hx*.64]:
   part('Window dark inset',(x,-hy-.014,level),(.22,.026,.25),'WindowGlass',edge=.003)
   for dx in [-.24,.24]:part('Window stone jamb',(x+dx,-hy-.045,level),(.03,.026,.29),'Masonry',edge=.003)
   part('Window sill',(x,-hy-.055,level-.27),(.28,.065,.025),'Masonry',edge=.004)
   part('Window mullion',(x,-hy-.05,level),(.018,.02,.24),'Sand',edge=.003)
   if kind=='StoneCottage':
    for side in [-1,1]:part('Open blue green shutter',(x+side*.36,-hy-.045,level),(.1,.025,.26),'Shutter',edge=.003)
 # The orbit camera sees every facade. Give the rear and both gables real openings too.
 for level in rows:
  for x in [-hx*.64,hx*.64]:
   part('Rear window frame',(x,hy+.024,level),(.24,.024,.28),'Wood',edge=.004)
   part('Rear window glass',(x,hy+.055,level),(.205,.012,.24),'WindowGlass',edge=.002)
   part('Rear window crossbar',(x,hy+.072,level),(.205,.014,.015),'Wood',edge=.002)
   part('Rear window mullion',(x,hy+.072,level),(.015,.014,.24),'Wood',edge=.002)
   part('Rear sill',(x,hy+.07,level-.29),(.29,.07,.025),'Masonry',edge=.004)
   if kind=='StoneCottage':
    for side in [-1,1]:part('Rear shutter',(x+side*.36,hy+.04,level),(.1,.025,.27),'Shutter',edge=.003)
  for side in [-1,1]:
   part('Gable window frame',(side*(hx+.024),0,level),(.024,.24,.28),'Wood',edge=.004)
   part('Gable glass',(side*(hx+.055),0,level),(.012,.205,.24),'WindowGlass',edge=.002)
   part('Gable crossbar',(side*(hx+.072),0,level),(.014,.205,.015),'Wood',edge=.002)
   part('Gable mullion',(side*(hx+.072),0,level),(.014,.015,.24),'Wood',edge=.002)
   part('Gable sill',(side*(hx+.07),0,level-.29),(.07,.29,.025),'Masonry',edge=.004)
 part('Rear doorway surround',(0,hy+.024,.64),(.29,.04,.53),'Masonry',edge=.004)
 part('Rear plank door',(0,hy+.07,.62),(.235,.02,.48),'Wood',edge=.004)
 for x in [-.12,0,.12]:part('Rear plank joint',(x,hy+.096,.62),(.005,.005,.46),'RoofTrim',edge=.001)
 part('Rear doorstep',(0,hy+.24,.13),(.38,.26,.1),'Masonry',edge=.01)
 if kind=='VillageInn':
  part('Upper floor timber band',(0,hy+.032,1.36),(hx,.03,.035),'Wood',edge=.004)
  for side in [-1,1]:part('Side floor band',(side*(hx+.032),0,1.36),(.03,hy,.035),'Wood',edge=.004)
 part('Timber door',(0,-hy-.022,.62),(.23,.04,.48),'Wood',edge=.005)
 part('Door step',(0,-hy-.24,.13),(.4,.27,.1),'Masonry',edge=.012)
 if kind=='VillageInn':
  for x in [-1.52,1.52]:part('Porch post',(x,-hy-.64,.83),(.055,.055,.78),'Wood',edge=.005)
  part('Porch canopy',(0,-hy-.4,1.62),(1.65,.5,.05),'RoofClay',rot=(.14,0,0),edge=.006)
  part('Inn beam',(0,-hy-.69,1.5),(1.65,.04,.055),'Wood',edge=.005)
 elif kind=='Dairy':
  for x in [-1.28,1.28]:part('Roof ventilator',(x,0,height+.72),(.13,.15,.17),'Steel')
  for x in [-.55,.55]:
   part('Milk churn',(x,-hy-.3,.35),(.12,.12,.22),'Steel','cylinder')
   part('Milk churn lid',(x,-hy-.3,.59),(.14,.14,.025),'Steel','cylinder')
 else:
  part('Cottage chimney',(-.57,.35,height+.67),(.16,.2,.52),'Masonry',edge=.008)
  part('Chimney cap',(-.57,.35,height+1.2),(.2,.24,.04),'RoofTrim',edge=.007)
 combine_static(kind)

def village_well():
 for row in range(3):
  for i in range(12):
   a=(i+row*.5)*math.tau/12;part('Well stone',(math.cos(a)*.47,math.sin(a)*.47,.12+row*.18),(.145,.13,.085),'Masonry',rot=(0,0,a+math.pi/2),edge=.022)
 for x in [-.65,.65]:part('Well upright',(x,0,.83),(.065,.065,.83),'Wood',edge=.005)
 part('Windlass',(0,0,1.2),(.055,.055,.75),'Wood','cylinder',(0,math.pi/2,0))
 part('Rope',(0,0,.77),(.012,.012,.42),'Sand','cylinder')
 part('Bucket',(0,0,.44),(.15,.15,.19),'Wood','cylinder')
 roof_gable(.81,.65,1.65,.39,'RoofClay')
 combine_static('Village well')

def batch_actor():
 # Keep moving wheel/leg hierarchies and muzzle/rifle landmarks; batch static surfaces by material.
 groups={}
 for obj in list(bpy.context.scene.objects):
  if obj.type!='MESH' or obj.name.startswith(('RoadWheel','Rolling wheel','Muzzle','Rifle stock','Barrel')):continue
  key=(obj.parent,obj.data.materials[0].name)
  groups.setdefault(key,[]).append(obj)
 for (parent,mat),objects in groups.items():
  if len(objects)<2:continue
  bpy.ops.object.select_all(action='DESELECT')
  for obj in objects:obj.select_set(True)
  bpy.context.view_layer.objects.active=objects[0];bpy.ops.object.join();objects[0].name='Miniature '+mat

only=set(sys.argv[sys.argv.index('--only')+1].split(',')) if '--only' in sys.argv else None
for name,fn in [('Infantry',infantry),('Scout',scout),('AssaultInfantry',assault),('Truck',lambda:vehicle('Truck')),('ArmoredCar',lambda:vehicle('ArmoredCar')),('HeavyTank',heavy_tank),('LightTank',tank),('MachineGun',lambda:gun('MachineGun')),('AntiTank',lambda:gun('AntiTank')),('Mortar',lambda:gun('Mortar')),('MachineGunII',lambda:upgraded_gun('MachineGun')),('MachineGunRapid',lambda:upgraded_gun('MachineGun','A')),('MachineGunAP',lambda:upgraded_gun('MachineGun','B')),('AntiTankII',lambda:upgraded_gun('AntiTank')),('TankHunter',lambda:upgraded_gun('AntiTank','A')),('FieldHowitzer',lambda:upgraded_gun('AntiTank','B')),('MortarII',lambda:upgraded_gun('Mortar')),('SiegeMortar',lambda:upgraded_gun('Mortar','A')),('BarrageMortar',lambda:upgraded_gun('Mortar','B')),('Pine',tree),('Bush',bush),('Farmhouse',hut),('Barn',barn),('StoneCottage',lambda:country_building('StoneCottage')),('VillageInn',lambda:country_building('VillageInn')),('Dairy',lambda:country_building('Dairy')),('ToolShed',lambda:country_building('ToolShed')),('VillageWell',village_well),('Crate',crate),('Sandbags',bags),('FieldGate',gate),('Checkpoint',checkpoint),('RoadSign',signpost),('FieldFence',fence),('BuildStake',stake),('Revetment',revetment),('Duckboard',duckboard),('FieldStone',fieldstone)]:
 scene=bpy.data.scenes.new(name); bpy.context.window.scene=scene; fn()
 if name in {'Infantry','Scout','AssaultInfantry','Truck','ArmoredCar','HeavyTank','LightTank'}:batch_actor()
 bpy.ops.object.select_all(action='SELECT');bpy.ops.object.transform_apply(location=False,rotation=False,scale=True)
 if only and name not in only: continue
 bpy.ops.export_scene.fbx(filepath=os.path.join(out,name+'.fbx'),use_selection=True,axis_forward='-Z',axis_up='Y',apply_unit_scale=True,bake_anim=False,add_leaf_bones=False)
bpy.ops.wm.save_as_mainfile(filepath=os.path.join(root,'Tools/Blender/miniatures.blend'))
print('Generated original diorama models', only or 'all')
