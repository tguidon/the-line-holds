"""Run: Blender --background --python Tools/Blender/generate_assets.py -- <repo>.
Original procedural models, metres, +Z forward in Unity. No external assets.
"""
import bpy, bmesh, math, os, sys, random
from mathutils import Vector
root = sys.argv[sys.argv.index('--')+1]
out = os.path.join(root, 'TheLineHolds/Assets/TheLineHolds/Art/Models')
os.makedirs(out, exist_ok=True)
materials={}
for name,c in {'Olive':(.25,.29,.16,1),'Dark':(.075,.085,.065,1),'Steel':(.19,.22,.19,1),'Cloth':(.36,.39,.25,1),'Skin':(.59,.43,.3,1),'Sand':(.51,.44,.29,1),'Wood':(.25,.17,.10,1),'Leaf':(.25,.32,.15,1),'Plaster':(.62,.57,.43,1),'Roof':(.27,.24,.20,1),'LeafLight':(.48,.56,.32,1),'LeafDark':(.18,.29,.17,1),'Bark':(.36,.28,.18,1),'Leather':(.28,.21,.13,1),'Edge':(.39,.44,.28,1),'Badge':(.72,.67,.49,1),'BagCloth':(.51,.44,.29,1),'Masonry':(.57,.55,.47,1),'RoofSlate':(.28,.33,.32,1),'RoofSlateLight':(.34,.39,.36,1),'RoofTrim':(.19,.23,.22,1),'WindowGlass':(.13,.2,.22,1),'Shutter':(.27,.35,.26,1),'SignPaint':(.65,.6,.43,1)}.items():
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

only=set(sys.argv[sys.argv.index('--only')+1].split(',')) if '--only' in sys.argv else None
for name,fn in [('Infantry',infantry),('LightTank',tank),('MachineGun',lambda:gun('MachineGun')),('AntiTank',lambda:gun('AntiTank')),('Mortar',lambda:gun('Mortar')),('Pine',tree),('Bush',bush),('Farmhouse',hut),('Crate',crate),('Sandbags',bags),('FieldGate',gate),('Checkpoint',checkpoint),('RoadSign',signpost),('FieldFence',fence),('BuildStake',stake),('Revetment',revetment),('Duckboard',duckboard),('FieldStone',fieldstone)]:
 scene=bpy.data.scenes.new(name); bpy.context.window.scene=scene; fn()
 bpy.ops.object.select_all(action='SELECT');bpy.ops.object.transform_apply(location=False,rotation=False,scale=True)
 if only and name not in only: continue
 bpy.ops.export_scene.fbx(filepath=os.path.join(out,name+'.fbx'),use_selection=True,axis_forward='-Z',axis_up='Y',apply_unit_scale=True,bake_anim=False,add_leaf_bones=False)
bpy.ops.wm.save_as_mainfile(filepath=os.path.join(root,'Tools/Blender/miniatures.blend'))
print('Generated original diorama models', only or 'all')
