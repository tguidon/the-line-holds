"""Generate the metre-scale Saint-Aubin town kit. Blender -b -P saint_aubin.py -- <repo>."""
import math
import random
import sys
from pathlib import Path
import bpy
from mathutils import Vector

repo = Path(sys.argv[sys.argv.index('--') + 1])
art = repo / 'TheLineHolds/Assets/TheLineHolds/Art/SaintAubin'
out = art / 'Models'
out.mkdir(parents=True, exist_ok=True)
bpy.ops.wm.read_factory_settings(use_empty=True)
bpy.context.preferences.filepaths.save_version = 0
rng = random.Random(1944)
palette = dict(
    plaster='D5C5A6', chalk='E0D7BB', ochre='BD9D75', grey='9F9F90', stone='ADA995',
    cream='E1CFAB', mortar='817C70', ash='716B62', coal='4D4B43', brick='A1654D',
    brick2='BA8262', slate='516475', slate2='5B6E7A', slate3='4B5F6B', tile='A35E47',
    tile2='AF6A50', tile3='975740', zinc='7E908F', dark='394044', glass='586B70',
    glint='9BA9A3', wood='7B6049', plank='A38461', gold='C8A356', green='687D72',
    sage='93A287', wine='8A4B42', iron='464F4D', linen='D9CBA8', bark='685843',
    leaf='728751', shade='526B46', tip='96A464', petal='DDD0BE', blush='C9AFA8',
    yellow='D3AE56', foliage='7B905E', flagblue='556D89', flagred='A45650')
keys = list(palette)
# One UV swatch per pigment. Tint variations are built into the stone, slates and foliage meshes.
image = bpy.data.images.new('Saint-Aubin pigments', width=64, height=64, alpha=False)
pixels = []
for y in range(64):
    for x in range(64):
        color = palette[keys[min(len(keys)-1, (y//8)*8+x//8)]]
        rgb = [int(color[k:k+2],16)/255 for k in (0,2,4)]
        pixels.extend(rgb+[1])
image.pixels[:] = pixels
image.filepath_raw = str(art/'Town pigments.png')
image.file_format = 'PNG'
image.save()
image.pack()
mat = bpy.data.materials.new('SaintAubinPigments')
mat.use_nodes = True
node = mat.node_tree.nodes.new('ShaderNodeTexImage')
node.image = image
mat.node_tree.links.new(node.outputs['Color'], mat.node_tree.nodes['Principled BSDF'].inputs['Base Color'])

verts, faces, pigments = [], [], []
def poly(points, loops, color):
    start = len(verts)
    verts.extend(points)
    for loop in loops:
        faces.append(tuple(start+i for i in loop))
        pigments.append(color)

def box(x,y,z,w,d,h,c,angle=0, pitch=0):
    points=[]
    for a,b,k in [(-1,-1,-1),(1,-1,-1),(1,1,-1),(-1,1,-1),(-1,-1,1),(1,-1,1),(1,1,1),(-1,1,1)]:
        u,v,t=a*w/2,b*d/2,k*h/2
        v,t=v*math.cos(pitch)-t*math.sin(pitch),v*math.sin(pitch)+t*math.cos(pitch)
        points.append((x+u*math.cos(angle)-v*math.sin(angle),y+u*math.sin(angle)+v*math.cos(angle),z+t))
    poly(points,[(0,3,2,1),(4,5,6,7),(0,1,5,4),(1,2,6,5),(2,3,7,6),(3,0,4,7)],c)

def beam(a,b,r,s,c,n=7):
    a,b=Vector(a),Vector(b)
    axis=(b-a).normalized()
    side=axis.cross(Vector((0,1,0)) if abs(axis.y)<.9 else Vector((1,0,0))).normalized()
    across=axis.cross(side)
    points=[tuple(p+radius*(side*math.cos(i*math.tau/n)+across*math.sin(i*math.tau/n))) for p,radius in [(a,r),(b,s)] for i in range(n)]
    poly(points,[tuple(reversed(range(n))),tuple(range(n,n*2))]+[(i,(i+1)%n,(i+1)%n+n,i+n) for i in range(n)],c)

def roof(x,y,z,w,d,rise,clay=False):
    style=['tile','tile2','tile3'] if clay else ['slate','slate2','slate3']
    angle=math.atan2(rise,d/2)
    for sign in [-1,1]:
        box(x,y+sign*d/4,z+rise/2,w,math.hypot(d/2,rise),.055,style[0],pitch=-sign*angle)
        # Overlapping slate courses catch the sun. Offset the joints in alternate rows.
        for row in range(6):
            t=(row+.5)/6
            n=max(3,round(w/.23))
            for col in range(n+1):
                l=max(0,(col-(row%2)*.5)/n)
                r=min(1,(col+1-(row%2)*.5)/n)
                if r<=l: continue
                box(x+w*((l+r)/2-.5),y+sign*t*d/2,z+(1-t)*rise+.029,w*(r-l)*.975,math.hypot(d/2,rise)/6+.01,.021,rng.choices(style,[7,2,2])[0],pitch=-sign*angle)
    beam((x-w/2,y,z+rise+.024),(x+w/2,y,z+rise+.024),.047,.047,style[1])
    for sign in [-1,1]:
        pts=[(x+sign*(w/2-.07),y-d/2,z-.025),(x+sign*(w/2-.07),y+d/2,z-.025),(x+sign*(w/2-.07),y,z+rise-.04)]
        poly(pts,[(0,1,2) if sign>0 else (2,1,0)],'plaster')

def window(x,y,z,w,h,side,shutters=True):
    def layer(offset,width,height,color,shift=0):
        box(x+shift,y+side*offset,z,width,.022,height,color)
    layer(.013,w+.075,h+.075,'cream')
    layer(.028,w,h,'dark')
    layer(.040,w*.82,h*.83,'glass')
    layer(.052,.022,h*.88,'cream')
    box(x,y+side*.055,z+h*.07,w*.91,.014,.021,'cream')
    box(x,y+side*.063,z-h*.53,w+.115,.103,.040,'stone')
    if shutters:
        for edge in [-1,1]:
            shift=edge*(w*.74+.02)
            layer(.031,w*.38,h*1.02,'green',shift)
            for fraction in [-.32,.28]:
                box(x+shift,y+side*.05,z+h*fraction,w*.35,.016,.029,'sage')

def townhouse(kind):
    hall=kind=='Mairie'
    w,d,h=(2.65,1.36,1.54) if hall else (2.4,1.52,1.48)
    base={'Mairie':'chalk','Mercerie':'plaster','Boulangerie':'ochre','Maison':'grey'}[kind]
    box(0,0,h/2,w,d,h,base)
    box(0,0,.09,w+.06,d+.06,.18,'stone')
    box(0,0,h*.53,w+.035,d+.04,.048,'cream')
    for sign in [-1,1]:
        y=sign*d/2
        for col in [-1,0,1]:
            window(col*w*.32,y,h*.76,.265,.36,sign,not hall)
            if col and (kind=='Maison' or hall or sign==1):
                window(col*w*.32,y,.48,.31,.38,sign,not hall)
        box(0,y+sign*.03,.33,.37,.065,.65,'dark')
        box(0,y+sign*.07,.31,.3,.025,.58,'wood')
        for edge in [-1,1]:
            for i in range(6):
                box(edge*(w/2-.062),y,h*(i+.5)/6,.14 if i%2 else .21,.065,h/7,'stone')
    for sign in [-1,1]:
        for y in [-.32,.32]:
            box(sign*(w/2+.012),y,h*.73,.032,.25,.32,'dark')
            box(sign*(w/2+.031),y,h*.73,.020,.018,.32,'cream')
    roof(0,0,h,w+.14,d+.16,.50,kind=='Boulangerie')
    box(w*.28,.18,h+.49,.23,.24,.58,'brick')
    box(w*.28,.18,h+.79,.29,.30,.075,'stone')
    if hall:
        # Open bell chamber, four-sided cap and clock over the entrance.
        box(0,0,2.04,.57,.55,.47,'cream')
        for sx in [-1,1]:
            for sy in [-1,1]:
                box(sx*.21,sy*.20,2.41,.075,.075,.41,'chalk')
        beam((0,0,2.47),(0,0,2.26),.07,.145,'gold')
        poly([(-.38,-.37,2.59),(.38,-.37,2.59),(.38,.37,2.59),(-.38,.37,2.59),(0,0,3.06)],[(0,1,4),(1,2,4),(2,3,4),(3,0,4)],'slate')
        beam((0,-.281,2.075),(0,-.309,2.075),.162,.162,'dark',20)
        beam((0,-.309,2.075),(0,-.319,2.075),.139,.139,'linen',20)
        beam((0,-.327,2.075),(-.068,-.327,2.12),.011,.01,'coal')
        beam((0,-.327,2.075),(.014,-.327,2.18),.011,.009,'coal')
        box(0,-d/2-.08,.74,.59,.22,.07,'cream')
        for i in range(3): box(0,-d/2-.09-.075*i,.044*(3-i),.72+.08*i,.19,.083,'stone')
        for i,c in enumerate(['flagblue','linen','flagred']):
            box(w*.46+i*.105,-.42,1.67,.108,.025,.26,c)
        beam((w*.46-.055,-.42,0),(w*.46-.055,-.42,1.90),.02,.013,'iron')
    elif kind!='Maison':
        for x in [-.75,.75]:
            window(x,-d/2,.46,.64,.4,-1,False)
        color='wine' if kind=='Boulangerie' else 'green'
        box(0,-d/2-.06,.91,w*.88,.09,.15,color)
        for i in range(12):
            x=-w*.44+(i+.5)*w*.88/12
            c=color if i%2 else 'linen'
            box(x,-d/2-.18,.83,w*.88/12,.34,.044,c,pitch=.2)
            box(x,-d/2-.337,.748,w*.88/12,.033,.095,c)
    else:
        for x in [-.62,.62]:
            box(x,-.35,h+.24,.35,.3,.31,'plaster')
            window(x,-.50,h+.24,.18,.17,-1,False)
            roof(x,-.34,h+.41,.46,.43,.14)

def ruins(variant):
    w,d=2.38,1.58
    box(0,0,.06,w,d,.12,'mortar')
    for i in range(10): box(-1+i*.216,0,.13,.202,d*.89,.045,'plank' if i%4 else 'ash')
    # Break each wall at a different height. Exposed masonry and a surviving window pier
    # give a roofless house recognizable openings even from the overhead camera.
    for wall in range(4):
        n=12 if wall<2 else 8
        for j in range(n):
            t=(j+.5)/n
            high=(1.12 if wall in [1,2] else .51)+.35*math.sin(t*5+wall+variant)+rng.uniform(-.15,.15)
            if wall==1 and .37<t<.66:high=.36
            if wall==0 and .34<t<.59:high=.07
            courses=max(1,round(high/.135))
            for k in range(courses):
                x,y=(w*(t-.5),(wall-.5)*d) if wall<2 else ((wall-2.5)*w,d*(t-.5))
                color=rng.choice(['stone','grey','ochre','brick2']) if k>=courses-2 else rng.choice(['plaster','plaster','stone'])
                box(x,y,.12+.135*(k+.5),w/n-.012 if wall<2 else .135,.135 if wall<2 else d/n-.01,.129,color)
    for i in range(48):
        x=rng.uniform(-1.02,1.02);y=rng.uniform(-.67,.67)
        box(x,y,.16+rng.uniform(0,.09),rng.uniform(.075,.22),rng.uniform(.06,.16),rng.uniform(.06,.14),rng.choice(['brick','brick2','stone','ash','chalk']),rng.uniform(-1,1),rng.uniform(-.18,.18))
    for i in range(4):
        beam((-.85+i*.46,.57,.75+rng.random()*.3),(-.52+i*.41,-.49,.19),.047,.036,'coal',4)
    box(-.82,.41,.80,.25,.27,1.37,'brick')
    box(-.82,.41,1.51,.31,.32,.09,'ash')

def crown(center,radii,color,n=7):
    x,y,z=center;w,d,h=radii
    pts=[(x,y,z-h),(x,y,z+h)]
    for ring,elev in enumerate([-.42,.30]):
        for i in range(n):
            a=(i+ring*.41)*math.tau/n
            scale=rng.uniform(.83,1.13)
            pts.append((x+math.cos(a)*w*scale,y+math.sin(a)*d*scale,z+h*(elev+rng.uniform(-.16,.16))))
    loops=[]
    for i in range(n):
        j=(i+1)%n
        loops.extend([(0,2+j,2+i),(2+i,2+j,2+n+i),(2+j,2+n+j,2+n+i),(2+n+i,2+n+j,1)])
    poly(pts,loops,color)

def orchard():
    beam((0,0,-.04),(.05,.02,.99),.105,.058,'bark')
    for i in range(9):
        a=i*2.4
        x,y=.52*math.cos(a),.51*math.sin(a)
        z=1.24+(i%3)*.18
        beam((.04,0,.58+i*.032),(x,y,z),.058,.022,'bark')
        for j in range(6):
            xx=x+rng.uniform(-.27,.27);yy=y+rng.uniform(-.24,.24);zz=z+rng.uniform(-.07,.22)
            crown((xx,yy,zz),(.26,.25,.19),rng.choice(['shade','leaf','tip','foliage']))
            for k in range(3):
                crown((xx+rng.uniform(-.13,.13),yy+rng.uniform(-.13,.13),zz+.12),(.073,.071,.054),rng.choice(['petal','petal','blush']),5)

def stall():
    for x in [-.52,.52]:
        for y in [-.24,.24]:box(x,y,.43,.053,.052,.85,'wood')
    box(0,0,.38,1.19,.60,.085,'plank')
    for i in range(9):
        x=(i-4)*.142
        for side in [-1,1]:
            box(x,side*.179,.93,.142,.39,.042,'sage' if i%2 else 'linen',pitch=-side*.27)
            box(x,side*.363,.844,.142,.03,.068,'sage' if i%2 else 'linen')
    for i in range(4):
        x=(i-1.5)*.265
        box(x,0,.46,.25,.42,.089,'wood')
        for j in range(7):
            crown((x+rng.uniform(-.085,.085),rng.uniform(-.16,.16),.535),(.052,.05,.043),['leaf','yellow','wine','ochre'][i],5)

def lamp():
    beam((0,0,.01),(0,0,.17),.116,.083,'iron')
    beam((0,0,.15),(0,0,1.02),.04,.029,'iron')
    beam((0,0,1.01),(0,0,1.22),.085,.117,'linen',4)
    beam((0,0,1.22),(0,0,1.32),.139,.021,'iron',4)

def flowers():
    for i in range(68):
        x=rng.uniform(-.6,.6);y=rng.uniform(-.36,.36);h=rng.uniform(.05,.17)
        poly([(x-.021,y,0),(x+.021,y+.009,0),(x+.036,y+.02,h)],[(0,1,2)],rng.choice(['tip','leaf','shade']))
        if i%4==0:crown((x+.035,y+.019,h),(.026,.025,.018),rng.choice(['petal','yellow']),5)

def gun_site():
    # Individual setts follow the ring, with small gaps at footpaths and brushes of grass.
    for i in range(56):
        a=i*math.tau/56
        if i%19==0: continue
        r=.96+rng.uniform(-.02,.018)
        box(math.cos(a)*r,math.sin(a)*r,.049,.096,.103,rng.uniform(.047,.071),
            rng.choice(['grey','stone','mortar','stone']),a+rng.uniform(-.06,.06))
    for i in range(92):
        a=rng.uniform(0,math.tau);r=math.sqrt(rng.random())*.90
        box(r*math.cos(a),r*math.sin(a),.076,rng.uniform(.025,.075),rng.uniform(.018,.04),.007,
            rng.choice(['ochre','plank','stone']),a)
    for i in range(4):
        a=i*math.pi/2+.18
        box(math.cos(a)*1.01,math.sin(a)*1.01,.105,.055,.055,.21,'wood')
        box(math.cos(a)*1.01,math.sin(a)*1.01,.204,.056,.056,.03,'cream')

def handcart():
    for side in [-1,1]:
        x=side*.29
        for i in range(14):
            a=i*math.tau/14;b=(i+1)*math.tau/14
            def rim(t):return (x,.25*math.cos(t),.26+.25*math.sin(t))
            beam(rim(a),rim(b),.029,.029,'iron',5)
            if i%2==0:beam((x,0,.26),rim(a),.016,.016,'wood',4)
        beam((x,-.3,.36),(x,-.91,.49),.032,.026,'wood')
    for i in range(5):box((i-2)*.1,0,.36,.094,.70,.065,'plank')
    for side in [-1,1]:
        box(side*.25,0,.48,.036,.74,.19,'wood')
    for i in range(4):box((i%2-.5)*.22,(i//2-.5)*.27,.50,.19,.24,.19,'ochre')

def hip(x,y,z,w,d,h,topw,topd,color):
    pts=[(x+sx*ww/2,y+sy*dd/2,zz) for ww,dd,zz in [(w,d,z),(topw,topd,z+h)]
         for sx,sy in [(-1,-1),(1,-1),(1,1),(-1,1)]]
    poly(pts,[(0,1,5,4),(1,2,6,5),(2,3,7,6),(3,0,4,7),(4,5,6,7)],color)

def arch(x,y,base,r,depth):
    # Individual voussoirs make a genuine opening with a shadowed soffit.
    for i in range(9):
        pts=[]
        for yy in [y-depth/2,y+depth/2]:
            for rr,k in [(r,i),(r+.092,i),(r+.092,i+1),(r,i+1)]:
                t=math.pi*k/9
                pts.append((x+rr*math.cos(t),yy,base+rr*math.sin(t)))
        poly(pts,[(3,2,1,0),(4,5,6,7),(0,1,5,4),(2,3,7,6),(3,0,4,7)],'cream' if i%3 else 'stone')

def urbanhouse(kind):
    w,d=2.4,1.72
    floors=3 if kind in ('Hotel','BrickTenement') else 2
    wall={'Hotel':'chalk','Arcade':'plaster','GabledDraper':'cream','BrickTenement':'brick','RuinFacade':'grey'}[kind]
    h=.84+floors*.49
    ruined=kind=='RuinFacade'
    if ruined:
        # The street elevation survives, exposing the cut floors and fireplaces behind it.
        ruins(3)
        for x in [-.95,-.48,.03,.54,1.01]:
            top=2.10 if x<.3 else 1.48
            box(x,-.77,top/2,.145,.15,top,wall)
            for z in [.73,1.37]:
                box(x+.17,-.77,z,.38,.15,.105,'stone')
        for i in range(7):
            x=-.99+i*.295
            beam((x,-.68,1.33),(x+.06,.26,1.13-i*.035),.035,.035,'coal',4)
        return
    arcade=kind=='Arcade'
    box(0,.10 if arcade else 0,h/2,w,d-(.20 if arcade else 0),h,wall)
    for sign in [-1,1]:
        y=sign*d/2
        if arcade and sign<0:
            for j in range(4):box(-1.18+j*.79,-1.12,.34,.135,.24,.68,'stone')
            for j in range(3):arch(-.785+j*.79,-1.12,.64,.328,.22)
            box(0,-1.02,1.34,w,.41,.60,wall)
            y=-1.225
        for k in range(floors):
            z=.99+k*.49
            for col in [-.80,0,.80]:
                window(col,y,z,.27,.32,sign,kind=='GabledDraper')
        for z in [.77,h-.06]:
            box(0,y+sign*.026,z,w+.055,.09,.065,'cream')
        for x in [-.78,.78]:
            window(x,y,.38,.49,.51,sign,False)
        box(0,y+sign*.04,.31,.34,.09,.62,'wood')
        if kind=='Hotel':
            box(0,y+sign*.148,.91,w*.87,.32,.055,'stone')
            box(0,y+sign*.285,1.05,w*.87,.025,.035,'iron')
            for i in range(17):box((i-8)*.124,y+sign*.285,.982,.014,.018,.14,'iron')
        if kind=='BrickTenement':
            for i in range(3):
                box((i-1)*.78,y+sign*.065,h-.17,.18,.09,.17,'stone')
    for side in [-1,1]:
        for y in [-.45,.19,.59]:
            for story in range(floors):
                z=.93+story*.49
                box(side*(w/2+.015),y,z,.035,.15,.29,'dark')
                box(side*(w/2+.04),y,z-.149,.083,.21,.036,'cream')
    if kind in ('Hotel','Arcade'):
        hip(0,0,h,2.49,2.0,.71,1.97,1.14,'slate3')
        hip(0,0,h+.71,2.01,1.17,.11,1.78,.91,'zinc')
        for sign in [-1,1]:
            for x in [-.72,0,.72]:
                box(x,sign*.80,h+.30,.32,.31,.43,'chalk')
                window(x,sign*.955,h+.32,.19,.24,sign,False)
                roof(x,sign*.81,h+.51,.40,.39,.14)
    elif kind=='GabledDraper':
        start=len(verts)
        roof(0,0,h,1.83,2.46,.73,True)
        for i in range(start,len(verts)):
            x,y,z=verts[i];verts[i]=(-y,x,z)
        for sign in [-1,1]:
            for i in range(5):
                box(0,sign*.87,h+.085+i*.147,2.34-i*.42,.135,.155,wall)
            window(0,sign*.94,h+.28,.22,.28,sign,False)
    else:roof(0,0,h,2.47,1.86,.38)
    for x in [-.89,.84]:
        box(x,.26,h+.51,.205,.28,.62,'brick')
        for yy in [.19,.33]:beam((x,yy,h+.79),(x,yy,h+.96),.058,.051,'tile',8)

def parish():
    # Apse, nave and surviving west tower. The roof break exposes two trusses.
    box(.43,0,.68,2.45,1.28,1.36,'grey')
    roof(.11,0,1.36,1.76,1.43,.78)
    for x in [.95,1.48]:
        for sign in [-1,1]:
            beam((x,sign*.65,1.38),(x,0,2.10),.038,.038,'wood',4)
        beam((x,-.63,1.40),(x,.63,1.40),.039,.038,'coal',4)
    for x in [-.57,-.04,.49,1.02,1.55]:
        for sign in [-1,1]:
            y=sign*.65
            box(x,y+sign*.065,.50,.125,.23,1,'stone')
            box(x+.16,y,.88,.135,.028,.52,'dark')
            arch(x+.16,y+sign*.021,1.13,.068,.04)
    box(-1.02,0,1.08,.83,1.06,2.16,'stone')
    for z in [.78,1.68,2.15]:box(-1.02,0,z,.92,1.14,.065,'chalk')
    for sign in [-1,1]:
        for x in [-1.17,-.87]:
            box(x,sign*.536,1.92,.137,.028,.31,'dark')
            arch(x,sign*.552,2.06,.069,.045)
    hip(-1.02,0,2.20,.96,1.18,.41,.55,.64,'slate3')
    hip(-1.02,0,2.60,.56,.65,.80,.04,.06,'slate')
    beam((-1.02,0,3.38),(-1.02,0,3.59),.018,.012,'iron')
    beam((-1.11,0,3.52),(-.93,0,3.52),.013,.013,'iron')

def fountain():
    for r,z,h,c in [(.71,.05,.10,'grey'),(.61,.13,.12,'cream'),(.51,.185,.025,'glint')]:
        beam((0,0,z-h/2),(0,0,z+h/2),r,r,c,24)
    beam((0,0,.17),(0,0,.62),.16,.095,'stone',12)
    beam((0,0,.52),(0,0,.65),.11,.37,'grey',20)
    beam((0,0,.65),(0,0,.85),.065,.038,'green',10)

def save_model(name, build, index):
    verts.clear();faces.clear();pigments.clear()
    build()
    mesh=bpy.data.meshes.new(name)
    mesh.from_pydata(verts,[],faces)
    mesh.update()
    uv=mesh.uv_layers.new(name='Pigment')
    for face,color in zip(mesh.polygons,pigments):
        k=keys.index(color)
        for idx in face.loop_indices:uv.data[idx].uv=((k%8+.5)/8,(k//8+.5)/8)
    obj=bpy.data.objects.new(name,mesh)
    bpy.context.collection.objects.link(obj)
    obj.data.materials.append(mat)
    bpy.ops.object.select_all(action='DESELECT')
    obj.select_set(True)
    bpy.context.view_layer.objects.active=obj
    bpy.ops.export_scene.fbx(filepath=str(out/(name+'.fbx')),use_selection=True,axis_forward='-Z',axis_up='Y',apply_unit_scale=True,bake_anim=False,path_mode='STRIP')
    obj.location.x=index*3.6

kits=[(k,lambda k=k:townhouse(k)) for k in ['Mairie','Mercerie','Boulangerie','Maison']]
kits += [('BurnedHouse',lambda:ruins(0)),('ShellHouse',lambda:ruins(2)),('AppleTree',orchard),('ProduceStall',stall),('StreetLamp',lamp),('SpringVerge',flowers),('GunSite',gun_site),('Handcart',handcart)]
kits.extend((name,lambda name=name:urbanhouse(name)) for name in ['Hotel','Arcade','GabledDraper','BrickTenement','RuinFacade'])
kits.extend([('Fountain',fountain),('ParishChurch',parish)])
for i,(name,build) in enumerate(kits):save_model(name,build,i)
bpy.ops.wm.save_as_mainfile(filepath=str(repo/'Tools/Blender/saint_aubin.blend'),compress=True)
