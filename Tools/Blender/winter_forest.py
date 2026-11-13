"""Blender --background --python Tools/Blender/winter_forest.py -- <repo>
Original metre-scale winter kit. Each FBX has a ground-level placement origin.
"""
import math
import random
import sys
from pathlib import Path

import bpy
from mathutils import Vector

repo = Path(sys.argv[sys.argv.index('--') + 1])
out = repo / 'TheLineHolds/Assets/TheLineHolds/Art/WinterForest/Models'
out.mkdir(parents=True, exist_ok=True)
bpy.ops.wm.read_factory_settings(use_empty=True)
bpy.context.preferences.filepaths.save_version = 0
rng = random.Random(1281)
palette = {
    'Powder': 'E5EEF3', 'BlueIce': 'ABC3CF', 'Needles': '24433F', 'SpruceTips': '45665C',
    'FirBark': '5F5048', 'SawnWood': 'B48B63', 'CampTimber': '846144', 'PineBoards': 'AA8057',
    'RoofIron': '46535A', 'GlassAmber': 'D6AE67', 'SignalRed': 'A64B36', 'Canvas': '86816C',
    'DeepEarth': '473F39', 'Granite': '687780', 'Birch': 'D6D2C5'
}
materials = {}
for key, rgb in palette.items():
    mat = bpy.data.materials.new(key)
    mat.diffuse_color = tuple(int(rgb[i:i+2], 16)/255 for i in (0, 2, 4)) + (1,)
    materials[key] = mat

parts = []
def finish(obj, name, mat):
    obj.name = name
    obj.data.materials.append(materials[mat])
    parts.append(obj)
    return obj

def box(name, loc, size, mat, tilt=(0, 0, 0)):
    bpy.ops.mesh.primitive_cube_add(size=1, location=loc)
    obj = bpy.context.object
    obj.dimensions = size
    obj.rotation_euler = tilt
    return finish(obj, name, mat)

def pole(a, b, ra, rb, mat, name='Timber', sides=9):
    vec = Vector(b)-Vector(a)
    bpy.ops.mesh.primitive_cone_add(vertices=sides, radius1=ra, radius2=rb, depth=vec.length,
                                    location=(Vector(a)+Vector(b))*.5)
    obj = bpy.context.object
    obj.rotation_euler = vec.to_track_quat('Z', 'Y').to_euler()
    return finish(obj, name, mat)

def lump(loc, size, mat):
    bpy.ops.mesh.primitive_ico_sphere_add(subdivisions=1, radius=1, location=loc)
    obj = bpy.context.object
    obj.scale = size
    obj.rotation_euler.z = rng.uniform(0, 6.28)
    return finish(obj, mat, mat)

def mesh(name, verts, faces, mat):
    data = bpy.data.meshes.new(name)
    data.from_pydata(verts, [], faces)
    data.update()
    obj = bpy.data.objects.new(name, data)
    bpy.context.collection.objects.link(obj)
    return finish(obj, name, mat)

def spruce(seed):
    rng.seed(seed)
    pole((0,0,-.13), (.02,0,3.95), .14, .025, 'FirBark')
    # Drooping radial boughs carry separate, discontinuous pillows of snow.
    for tier in range(9):
        h = .93 + tier*.35
        span = 1.10*(1-tier/10)**.8
        count = 10-tier//2
        turn = rng.uniform(0, 6.28)
        for k in range(count):
            angle = turn+k*math.tau/count+rng.uniform(-.1,.1)
            length = span*rng.uniform(.83,1.16)
            direction = Vector((math.cos(angle), math.sin(angle), 0))
            side = Vector((-direction.y, direction.x, 0))
            base = Vector((0,0,h))
            def frond(at, wide, high):
                return tuple(base+direction*(at*length)+side*(wide*length)+Vector((0,0,high*length)))
            coords = [(0,0,.16),(.51,-.29,-.06),(.8,-.18,-.2),(1,-.025,-.34),
                      (.82,.12,-.22),(.62,.28,-.11),(.63,0,.02),(.4,0,-.22)]
            faces = [(0,1,6),(1,2,6),(2,3,6),(3,4,6),(4,5,6),(5,0,6),
                     (1,0,7),(2,1,7),(3,2,7),(4,3,7),(5,4,7),(0,5,7)]
            mesh('Needle fan', [frond(*c) for c in coords], faces,
                 'Needles' if k%3 else 'SpruceTips')
            if rng.random()<.65:
                coat = [(.13,0,.17),(.48,-.22,.015),(.78,-.12,-.12),(.91,-.01,-.23),
                        (.74,.10,-.085),(.54,.21,-.005),(.59,0,.105)]
                width=rng.uniform(.58,.95)
                mesh('Snow mantle', [frond(a,b*width,h) for a,b,h in coat],faces[:6],'Powder' if k%3 else 'BlueIce')
    pole((.02,0,3.66),(.02,0,4.07),.12,0,'Powder')

def birch():
    for ox, oy, height in [(-.13,0,3.2),(.14,.13,2.65)]:
        top=(ox+.18,oy,height)
        pole((ox,oy,-.15),top,.105,.034,'Birch')
        for j in range(10):
            h=(j+.4)*height/10
            x=ox+.18*h/height
            lump((x,oy-.055,h),(.074,.046,.025),'FirBark')
            if j<3: continue
            a=rng.uniform(0,6.28)
            elbow=(x+math.cos(a)*.42,oy+math.sin(a)*.42,h+.22)
            tip=(x+math.cos(a)*.75,oy+math.sin(a)*.75,h+.70)
            pole((x,oy,h),elbow,.035,.022,'Birch')
            pole(elbow,tip,.023,.003,'FirBark')
            pole(elbow,(elbow[0]+.12,elbow[1]-.18,elbow[2]+.36),.016,.003,'FirBark')

def cabin():
    box('Stone footing',(0,0,.06),(2.45,1.82,.32),'Granite')
    for row in range(10):
        z=.23+row*.143
        for y in (-.84,.84):
            pole((-1.27,y,z),(1.27,y,z),.102,.102,'CampTimber',sides=12)
        for x in (-1.13,1.13):
            pole((x,-.98,z+.06),(x,.98,z+.06),.10,.10,'PineBoards',sides=12)
    for x in (-1.14,1.14):
        mesh('Plank gable',[(x,-.86,1.56),(x,.86,1.56),(x,0,2.19)],[(0,1,2),(2,1,0)],'CampTimber')
    for sign in (-1,1):
        tilt=(-sign*.605,0,0)
        box('Standing seam roof',(0,sign*.52,1.91),(2.82,1.33,.09),'RoofIron',tilt)
        box('Roof snow',(0,sign*.5,1.997),(2.78,1.28,.095),'Powder',tilt)
        for k in range(13):
            lump((-1.28+k*.21,sign*1.02,1.65),(.15,.15,rng.uniform(.045,.087)),'Powder')
    for x in (-.71,.76):
        box('Window casing',(x,-.943,.98),(.56,.092,.63),'SawnWood')
        box('Window glass',(x,-1.001,.99),(.43,.035,.47),'GlassAmber')
        box('Window sash',(x,-1.027,1),(.041,.031,.50),'RoofIron')
        box('Window rail',(x,-1.029,1),(.45,.03,.042),'RoofIron')
    box('Door',(0,-.963,.71),(.53,.09,1.12),'SignalRed')
    box('Latch',(.15,-1.017,.69),(.087,.04,.027),'SawnWood')
    box('Porch step',(0,-1.14,.18),(.85,.38,.14),'CampTimber')
    for side in (-1,1):
        for j in range(18):
            x=rng.uniform(-1.30,1.30); y=rng.uniform(.08,.84)*side
            cap=lump((x,y,2.36-abs(y)*.69),(.19,.21,rng.uniform(.03,.053)),'Powder' if j%4 else 'BlueIce')
            cap.rotation_euler=(-side*.605,0,0)
    pole((-.67,.23,1.94),(-.67,.23,2.66),.096,.096,'RoofIron')
    pole((-.67,.23,2.66),(-.67,.23,2.74),.15,.07,'RoofIron')
    for a in (-1,1):
        for b in (-1,1):
            pole((1.6+a*.25,b*.25,.04),(1.6,0,3.06),.033,.018,'RoofIron')
    for z in (.52,1.13,1.75,2.35):
        w=(3.06-z)*.085
        pole((1.6-w,-w,z),(1.6+w,w,z+.48),.019,.016,'RoofIron')
    pole((1.6,0,2.98),(1.6,0,3.65),.022,.012,'RoofIron')
    pole((1.24,0,3.43),(1.98,0,3.43),.021,.021,'RoofIron')

def foxhole():
    # A recessed floor and timber lining meet the matching depression in the Unity heightfield.
    lump((0,0,-.27),(.95,.57,.07),'DeepEarth')
    for k in range(16):
        a=k*math.tau/16
        x,y=.92*math.cos(a),.58*math.sin(a)
        pole((x*.82,y*.82,-.40),(x,y,.105),.084,.09,'CampTimber')
        if k in (11,12): continue
        q=lump((x*1.02,y*1.02,.095),(.31,.195,.15),'Canvas')
        q.rotation_euler.z=a+math.pi/2
        q=lump((x*1.02,y*1.02,.17),(.28,.16,.097),'Powder')
        q.rotation_euler.z=a+math.pi/2
    for k in range(4):
        box('Recessed duckboard',(-.43+k*.23,0,-.20),(.17,.67,.044),'PineBoards')

def wood_sledge():
    for x in (-.31,.31):
        pole((x,-.93,.075),(x,.79,.075),.055,.055,'RoofIron')
        pole((x,.79,.075),(x,1,.26),.052,.047,'RoofIron')
    for j in range(7):
        box('Sled bed',(0,-.7+j*.22,.22),(.81,.19,.095),'SawnWood')
    for i in range(5):
        x=(i%3-1)*.23;z=.37+(i//3)*.19
        pole((x,-.78,z),(x,.73,z),.132,.12,'FirBark')
        pole((x,-.792,z),(x,-.784,z),.112,.112,'SawnWood')
    for y in (-.5,.45):
        box('Canvas strap',(0,y,.64),(.66,.047,.032),'Canvas')

def waymarker():
    pole((0,0,-.1),(.025,0,1.13),.075,.062,'CampTimber')
    box('Direction board',(0,-.05,.91),(.83,.1,.25),'SignalRed')
    box('Snow on sill',(0,-.055,1.05),(.87,.15,.05),'Powder')
    for x in (-.09,.13):
        for sign in (-1,1):
            box('Painted chevron',(x,-.111,.91+sign*.046),(.18,.016,.035),'Powder',(0,sign*.55,0))

def rock():
    lump((0,0,.25),(1.14,.68,.59),'Granite')
    lump((-.08,.02,.45),(.99,.61,.43),'Powder')
    lump((.79,.14,.12),(.51,.44,.32),'Granite')
    lump((.74,.14,.26),(.45,.4,.22),'BlueIce')

kits=[('SnowSpruce',lambda:spruce(41)),('SilverBirch',birch),('SignalHut',cabin),
      ('Foxhole',foxhole),('WoodSledge',wood_sledge),('SnowyOutcrop',rock),('TrailMarker',waymarker)]
for index,(name,build) in enumerate(kits):
    parts.clear()
    build()
    bpy.ops.object.select_all(action='DESELECT')
    for obj in parts: obj.select_set(True)
    bpy.context.view_layer.objects.active=parts[0]
    bpy.ops.object.join()
    obj=bpy.context.object
    obj.name=name
    bpy.context.scene.cursor.location=(0,0,0)
    bpy.ops.object.origin_set(type='ORIGIN_CURSOR')
    bpy.ops.object.transform_apply(location=False,rotation=True,scale=True)
    bpy.ops.export_scene.fbx(filepath=str(out/(name+'.fbx')),use_selection=True,
                             axis_forward='-Z',axis_up='Y',apply_unit_scale=True,bake_anim=False)
    obj.location.x=index*4
bpy.ops.wm.save_as_mainfile(filepath=str(repo/'Tools/Blender/winter_forest.blend'),compress=True)
