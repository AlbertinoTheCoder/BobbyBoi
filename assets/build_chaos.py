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
cell=[B((0,1,0),(38,2,25),stone),B((0,28,0),(38,2,25),stone),B((0,14,-11),(38,26,2),stone),B((-18,14,0),(2,26,25),stone),B((18,14,0),(2,26,25),stone),B((-8,5,-4),(14,3,10),(112,52,43),'bed')]
for x in range(-15,16,5):cell+=[B((x,14,12),(1,26,1),iron,'bars')]
cell+=[B((0,10,12),(36,1,1),iron,'bars'),B((0,22,12),(36,1,1),iron,'bars')]
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
