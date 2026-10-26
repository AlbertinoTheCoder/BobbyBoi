import bpy,json,os,random
from mathutils import Vector
out=os.path.dirname(os.path.abspath(__file__))
bpy.ops.object.select_all(action='SELECT');bpy.ops.object.delete(use_global=False)
models={};mats={}
def build(name,boxes,index):
    faces=[];offset=Vector(((index%4)*65,(index//4)*70,0))
    col=bpy.data.collections.new(name);bpy.context.scene.collection.children.link(col)
    for i,(c,s,rgb,part) in enumerate(boxes):
        x,y,z=c;hx,hy,hz=[v/2 for v in s]
        vs=[(x-hx,y-hy,z-hz),(x+hx,y-hy,z-hz),(x+hx,y+hy,z-hz),(x-hx,y+hy,z-hz),(x-hx,y-hy,z+hz),(x+hx,y-hy,z+hz),(x+hx,y+hy,z+hz),(x-hx,y+hy,z+hz)]
        qs=[(0,3,2,1),(4,5,6,7),(0,4,7,3),(1,2,6,5),(3,7,6,2),(0,1,5,4)]
        if name=='herobrine' and part not in ('eye','mouth','brow','nose','beard','neck'):
            tiled=[];newqs=[]
            for q in qs:
                a,b,c,d=[Vector(vs[k]) for k in q]
                nu=max(1,round((b-a).length/2));nv=max(1,round((d-a).length/2))
                for u in range(nu):
                    for v in range(nv):
                        base=len(tiled)
                        for du,dv in [(0,0),(1,0),(1,1),(0,1)]:tiled.append(tuple(a+(b-a)*((u+du)/nu)+(d-a)*((v+dv)/nv)))
                        newqs.append(tuple(range(base,base+4)))
            vs,qs=tiled,newqs
        mesh=bpy.data.meshes.new(name+' '+part);mesh.from_pydata([(a,-c,b) for a,b,c in vs],[],qs);mesh.update()
        ob=bpy.data.objects.new(name+' '+part+' '+str(i),mesh);col.objects.link(ob);ob.location=offset
        if rgb not in mats:
            mat=bpy.data.materials.new('Voxel '+str(rgb));mat.diffuse_color=tuple(a/255 for a in rgb)+(1,);mat.use_nodes=True
            node=next(n for n in mat.node_tree.nodes if n.type=='BSDF_PRINCIPLED');node.inputs['Base Color'].default_value=mat.diffuse_color;node.inputs['Roughness'].default_value=.9;mats[rgb]=mat
        mesh.materials.append(mats[rgb])
        rng=random.Random(name+str(i))
        for poly,q in zip(mesh.polygons,qs):
            shade=rng.choice([-12,-5,0,0,6,12]) if name=='herobrine' and part not in ('eye','mouth','brow','nose','beard','neck') else 0
            color=tuple(max(0,min(255,v+shade)) for v in rgb)
            if color not in mats:
                mat=bpy.data.materials.new('Pixel '+str(color));mat.diffuse_color=tuple(v/255 for v in color)+(1,);mat.use_nodes=True
                next(n for n in mat.node_tree.nodes if n.type=='BSDF_PRINCIPLED').inputs['Base Color'].default_value=mat.diffuse_color;mats[color]=mat
            if mats[color].name not in mesh.materials:mesh.materials.append(mats[color])
            poly.material_index=list(mesh.materials).index(mats[color])
            faces.append({'Points':[list(vs[k]) for k in q],'Color':list(color),'Part':part})
    models[name]=faces
D=(65,54,72);stone=(93,98,106);iron=(192,195,183);wood=(113,69,36);green=(95,161,55);purple=(155,63,224)
def B(c,s,rgb,part='body'):return(c,s,rgb,part)
human=[B((0,18,0),(8,12,4),(33,155,166)),B((0,28,0),(8,8,8),(176,127,88),'head'),B((0,31,0),(8.2,2,8.2),(55,36,26),'head')]
for side in [-1,1]:
 human += [B((side*6,18,0),(4,12,4),(176,127,88),'arm'),B((side*2,6,0),(4,12,4),(57,56,116),'leg'),B((side*2,28.5,4.1),(2,1.3,.25),(255,255,255),'eye')]
# Steve-like skin, short shirt sleeves, beard and unmistakable white eyes.
human += [B((0,25.4,4.18),(6,2.5,.3),(76,47,32),'beard'),B((0,27,4.25),(2,2,.5),(153,101,68),'nose'),B((0,25.8,4.4),(2,.6,.2),(37,25,23),'mouth'),B((0,23.7,2.15),(2.5,1.5,.3),(176,127,88),'neck')]
for side in [-1,1]:
 human += [B((side*6,22,0),(4.1,4,4.1),(32,148,157),'sleeve'),B((side*2,1,0),(4.1,2.2,4.1),(53,54,61),'boot'),B((side*4.1,29.5,0),(0.3,4,8),(55,36,26),'hair'),B((side*2,29.8,4.15),(2.6,.65,.3),(79,49,32),'brow')]
build('herobrine',human,0)
golem=[B((0,21,0),(13,16,7),iron),B((0,33,0),(8,9,7),iron,'head'),B((0,31,4.5),(2,4,3),(143,145,129),'head')]
for side in [-1,1]:golem += [B((side*10,19,0),(5,22,5),iron,'armL' if side<0 else 'armR'),B((side*3.5,6,0),(5,12,5),iron,'legL' if side<0 else 'legR'),B((side*2,34,3.6),(1.6,.7,.3),(160,42,38),'eye')]
for y in [18,23,27]:golem+=[B((-4,y,3.6),(2,3,.3),(75,111,49),'vine')]
build('golem',golem,1)
creep=[B((0,12,0),(6,12,4),green),B((0,22,0),(8,8,8),green,'head'),B((0,17.5,0),(6.3,1.5,4.3),(62,183,204),'collar'),B((0,16.5,2.3),(1.8,2,.3),(247,204,73),'tag')]
for x in [-2,2]:
 creep+=[B((x,23,4.1),(2,2,.2),(17,31,15),'eye')]
 for z in [-2.5,2.5]:creep+=[B((x,3,z),(4,6,4),green,'leg')]
creep += [B((0,21,4.1),(2,2,.2),(17,31,15),'mouth'),B((0,19.5,4.1),(4,1,.2),(17,31,15),'mouth')]
build('petcreeper',creep,2)
# A small stone-brick guardhouse, with a separate hinged foreground gate.
cell=[]
brick_rng=random.Random(74)
for row in range(5):
    y=5+row*6
    for col in range(7):
        x=-21+col*7
        # Staggered brick courses and an inset barred back window.
        if row in (2,3) and col in (3,4):continue
        tone=brick_rng.choice([(91,98,103),(104,109,110),(112,117,115),(95,106,99)])
        cell.append(B((x,y,-13),(6.7,5.7,3),tone,'shell'))
    for side in [-1,1]:
        for z in [-8,-1,6,13]:
            tone=brick_rng.choice([(103,109,112),(118,120,115),(91,103,95)])
            cell.append(B((side*23,y,z),(4,5.7,6.7),tone,'shell'))
for x in range(-20,22,7):
    for z in [-9,-2,5,12]:cell.append(B((x,1,z),(6.8,2,6.8),brick_rng.choice([(83,88,88),(103,106,99),(93,99,96)]),'shell'))
# Masonry uprights, a shallow lintel and crenellated roof trim.
for side in [-1,1]:
    cell += [B((side*23,18,16),(5,34,5),(127,132,128),'front'),B((side*23,36,4),(6,3,30),(153,155,145),'shell')]
cell += [B((0,35,16),(50,4,6),(133,139,133),'front'),B((0,2,17),(44,2,7),(126,130,120),'front'),B((0,38,-12),(50,3,5),(125,132,127),'shell')]
for x in [-22,-11,0,11,22]:cell.append(B((x,40,-12),(5,4,5),(145,148,137),'shell'))
for side in [-1,1]:
    for z in [-7,5,16]:cell.append(B((side*23,40,z),(5,5,5),(142,146,136),'shell'))
# A narrow back window, straw mattress, pillow, and a bucket.
for x in [0,3.5,7]:cell.append(B((x,23,-11.2),(.7,11,.8),(133,150,151),'shell'))
cell += [B((3.5,17,-11),(12,1,1),(137,149,145),'shell'),B((3.5,29,-11),(12,1,1),(137,149,145),'shell'),B((-12,4,-3),(12,3,17),(116,77,39),'shell'),B((-12,6,-3),(11,2,16),(163,139,77),'shell'),B((-12,7,-8),(10,1.5,5),(219,211,175),'shell'),B((15,4,-7),(5,5,5),(91,105,107),'shell')]
# Warm lanterns in the stone pillars, with iron brackets.
for side in [-1,1]:
    cell += [B((side*23,27,20),(1.4,6,1.4),(71,76,76),'front'),B((side*23,24,20),(4.4,6,3.6),(70,76,77),'front'),B((side*23,24,22),(3.2,3.7,.35),(255,195,77),'lamp'),B((side*23,24,22.3),(1.2,2.4,.2),(255,229,146),'lamp')]
# An actual sign board rather than floating yellow text.
cell += [B((0,39,19),(31,7,1.6),(69,46,28),'front'),B((0,39,20),(28.5,4.8,.6),(113,78,40),'front')]
# One gate: thin dark iron rods, two rails, hinges and a small latch.
for x in [-18,-11,-5,5,11,18]:cell.append(B((x,18,17),(0.85,29,1.1),(133,151,151),'gate'))
for y in [4,13,32]:cell.append(B((0,y,17),(37,.9,1.2),(118,137,138),'gate'))
for y in [8,27]:cell.append(B((-19,y,17),(2,2.8,2),(79,89,89),'gate'))
cell += [B((15,18,18),(3.6,4,.8),(76,85,82),'gate'),B((15,18,18.5),(1.8,2,.3),(196,157,68),'gate')]
build('prison',cell,3)
portal=[]
for x in [-12,12]:
 for y in range(3,34,6):portal+=[B((x,y,0),(6,6,7),D,'frame')]
for y in [0,36]:
 for x in range(-12,13,6):portal+=[B((x,y,0),(6,6,7),D,'frame')]
build('portal',portal,4)
build('ufo',[B((0,0,0),(32,3,20),(111,123,135),'hull'),B((0,3,0),(22,3,15),iron,'hull'),B((0,7,0),(12,6,10),(99,191,194),'dome')]+[B((x,-2,9),(3,1,2),(117,255,133),'light') for x in [-12,-6,0,6,12]],5)
cave=[B((-15,14,0),(8,28,14),stone),B((15,14,0),(8,28,14),stone),B((0,28,0),(38,8,14),stone),B((0,13,-6),(24,25,2),(23,24,29)),B((0,27,8),(31,3,3),wood),B((-12,13,8),(3,27,3),wood),B((12,13,8),(3,27,3),wood),B((-11,17,10),(1,6,1),wood),B((-11,21,10),(2,2,2),(255,179,51),'flame')]
for x in [-4,4]:cave+=[B((x,1,14),(1,1,25),iron,'rail')]
build('cave',cave,6)
build('casino',[B((0,12,0),(18,24,12),(87,39,57)),B((0,28,0),(24,7,13),(158,46,54),'sign'),B((0,18,6.2),(15,8,.5),(29,38,31),'screen'),B((0,7,6.2),(12,2,1),wood),B((12,16,0),(2,15,2),iron,'lever'),B((12,24,0),(4,4,4),(218,56,56),'lever')]+[B((x,28,7),(2,2,1),(255,203,77),'bulb') for x in [-9,-3,3,9]],7)
build('camera',[B((0,20,0),(12,7,7),(45,47,50)),B((0,20,5),(5,5,4),stone,'lens'),B((0,9,0),(1.5,17,1.5),iron,'tripod'),B((-4,1,0),(9,1,7),iron,'tripod'),B((5,23,4),(1,1,1),(233,45,54),'light')],8)
with open(os.path.join(out,'chaos.json'),'w') as f:json.dump(models,f,separators=(',',':'))
bpy.ops.wm.save_as_mainfile(filepath=os.path.join(out,'BobbyBoiChaos.blend'))
print('EXPORTED', {k:len(v) for k,v in models.items()})
