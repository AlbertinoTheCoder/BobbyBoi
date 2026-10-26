import bpy, math, random, json, os
from mathutils import Vector
out=os.path.dirname(os.path.abspath(__file__))
os.makedirs(out,exist_ok=True)
scene=bpy.data.scenes.new("BobbyBoi Design")
bpy.context.window.scene=scene
collection=bpy.data.collections.new("BobbyBoi Character")
scene.collection.children.link(collection)
def cv(v): return (v[0],-v[2],v[1])
pivots={"Body":(0,12,0),"Head":(0,24,0),"LeftArm":(-5.5,24,0),"RightArm":(5.5,24,0),"LeftLeg":(-2,12,0),"RightLeg":(2,12,0)}
rig={}
for part,p in pivots.items():
    ob=bpy.data.objects.new(part+" Pivot",None);collection.objects.link(ob);ob.location=cv(p);ob.empty_display_size=.7;rig[part]=ob
palette={}
def mat(rgb):
    key=tuple(rgb)
    if key not in palette:
        m=bpy.data.materials.new("Pixel %02x%02x%02x"%key);m.diffuse_color=tuple(c/255 for c in rgb)+(1,);m.use_nodes=True
        shader=next(n for n in m.node_tree.nodes if n.type=="BSDF_PRINCIPLED")
        shader.inputs["Base Color"].default_value=tuple(c/12.92 if c<=.04045 else ((c+.055)/1.055)**2.4 for c in m.diffuse_color[:3])+(1,);shader.inputs["Roughness"].default_value=.95
        palette[key]=m
    return palette[key]
skin=(197,169,137);shirt=(95,143,86);hair=(201,117,42);pants=(100,66,44);boots=(64,65,62)
export=[]
def box(name,center,size,color,part="Body",pixel=True):
    pivot=pivots[part];vertices=[];faces=[];colors=[];r=random.Random(name)
    cx,cy,cz=center;hx,hy,hz=[s/2 for s in size]
    for side in range(6):
        uw=size[2] if side<2 else size[0];vh=size[1] if side<4 else size[2]
        nu=max(1,round(uw)) if pixel else 1;nv=max(1,round(vh)) if pixel else 1
        def point(a,b):
            return [(cx-hx,cy-hy+b,cz-hz+a),(cx+hx,cy-hy+b,cz+hz-a),(cx-hx+a,cy-hy+b,cz+hz),(cx+hx-a,cy-hy+b,cz-hz),(cx-hx+a,cy+hy,cz+hz-b),(cx-hx+a,cy-hy,cz-hz+b)][side]
        for u in range(nu):
            for v in range(nv):
                a=u*uw/nu;b=v*vh/nv;du=uw/nu;dv=vh/nv
                quad=[point(a,b),point(a+du,b),point(a+du,b+dv),point(a,b+dv)]
                start=len(vertices);vertices.extend(quad);faces.append(tuple(range(start,start+4)))
                delta=r.choice([-9,-4,0,0,4,8]) if pixel else 0
                colors.append(tuple(max(0,min(255,c+delta)) for c in color))
    mesh=bpy.data.meshes.new(name);mesh.from_pydata([cv(tuple(v[i]-pivot[i] for i in range(3))) for v in vertices],[],faces);mesh.update()
    ob=bpy.data.objects.new(name,mesh);collection.objects.link(ob);ob.parent=rig[part]
    used={}
    for poly,c in zip(mesh.polygons,colors):
        if c not in used:used[c]=len(used);mesh.materials.append(mat(c))
        poly.material_index=used[c]
    ob["part"]=part
    for face,c in zip(faces,colors):export.append({"Part":part,"Name":name,"Points":[list(vertices[i]) for i in face],"Color":list(c)})
    return ob
box("Shirt", (0,18,0),(8,12,4),shirt)
box("Belt",(0,13.8,.02),(8.08,1.2,4.08),(43,66,35))
box("Collar",(0,23.05,2.06),(2,1.65,.15),skin,pixel=False)
box("Shirt seam",(0,18,2.08),(.65,7,.12),(80,126,71),pixel=False)
for sign,part in [(-1,"LeftLeg"),(1,"RightLeg")]:
    box(part+" trousers",(sign*2,8,0),(4,8,4),pants,part)
    box(part+" boot",(sign*2,2,0),(4.08,4,4.12),boots,part)
    box(part+" sole",(sign*2,.45,.02),(4.12,.9,4.18),(47,48,45),part)
for sign,part in [(-1,"LeftArm"),(1,"RightArm")]:
    box(part+" skin",(sign*5.5,18,0),(3,12,4),skin,part)
    box(part+" sleeve",(sign*5.5,22,0),(3.15,4.15,4.15),shirt,part)
    box(part+" cuff",(sign*5.5,20.2,0),(3.22,.8,4.22),(75,102,58),part)
box("Face",(0,28,0),(8,8,8),skin,"Head")
# Slim underlayers, covered by separate irregular chunky locks.
box("Hair crown",(0,31.4,-.1),(8.15,1.5,8.1),(176,94,29),"Head")
box("Hair back underlayer",(0,28,-4.05),(8.15,7.6,.3),(160,83,27),"Head")
for side in [-1,1]:
    box("Side underlayer "+str(side),(side*4.03,28,-.5),(.22,7.6,7.1),(176,94,29),"Head")
# A crown assembled from offset clumps rather than one smooth helmet.
for ix in range(5):
    for iz in range(4):
        top=32.0+(.35 if (ix+iz)%3==0 else .08 if (ix+iz)%3==1 else .55)
        box("Crown lock %s %s"%(ix,iz),(-3.35+ix*1.68,top,-3.2+iz*2.1),(1.75,1.15,2.2),(202+(ix%2)*9,116+(iz%2)*8,38),"Head")
# Back locks overlap, but each has a distinct stepped, tapered end.
for i in range(7):
    bottom=[22.8,22.0,21.3,22.2,21.6,22.7,23.6][i]
    xx=-3.65+i*1.22
    box("Back lock "+str(i),(xx,(31.5+bottom)/2,-4.3),(1.35,31.5-bottom,.72),(177+i%3*9,94+i%3*6,29),"Head")
    box("Back tapered tip "+str(i),(xx,bottom-.55,-4.32),(.8,1.2,.65),(181+i%3*7,98+i%3*5,30),"Head")
for side in [-1,1]:
    for i in range(5):
        zz=-3.3+i*1.48
        bottom=[22.6,23.1,22.1,23.5,24.1][i]+(.35 if side==-1 else 0)
        xx=side*(4.22+(.15 if i%2 else 0))
        box("Side flowing lock %s %s"%(side,i),(xx,(31.5+bottom)/2,zz),(.75,31.5-bottom,1.58),(197+i%3*7,108+i%3*6,33),"Head")
        box("Side pointed tip %s %s"%(side,i),(xx,bottom-.55,zz),(.58,1.3,.95),(201+i%2*6,113+i%2*7,36),"Head")
        box("Side highlight %s %s"%(side,i),(xx+side*.39,28.1,zz-.2),(.08,3.5,.28),(230,145,55),"Head",False)
# Swept fringe: uneven, layered strands with short tips clear of the eyes.
for i,(xx,bottom) in enumerate([(-3.4,27.0),(-2.35,29.55),(-1.2,29.85),(0,30.15),(1.2,30.5),(2.35,29.4),(3.35,26.6)]):
    box("Fringe lock "+str(i),(xx,(32+bottom)/2,4.22),(1.2,32-bottom,.62),(213+i%3*6,125+i%3*6,41),"Head")
    box("Fringe tapered tip "+str(i),(xx,bottom-.3,4.24),(.7,.7,.5),(219+i%2*6,132+i%2*7,45),"Head")
for i in range(5):
    box("Braid segment "+str(i),(3.1+(.15 if i%2 else -.1),24.5-i*1.6,2.5),(1.95,1.95,1.5),(192-i*5,109-i*3,35),"Body")
box("Braid tie",(3.15,17.7,2.5),(1.4,.5,1.35),(67,95,48),pixel=False)
for sign in [-1,1]:
    box("Eye white "+str(sign),(sign*2,28.15,4.065),(2,1,.15),(231,232,211),"Head",False)
    box("Iris "+str(sign),(sign*1.7,28.15,4.17),(.75,1,.12),(51,104,49),"Head",False)
    box("Eye glint "+str(sign),(sign*1.7-.13,28.4,4.25),(.25,.25,.06),(165,194,126),"Head",False)
    box("Eyebrow "+str(sign),(sign*2,29.1,4.1),(1.8,.35,.14),(155,91,36),"Head",False)
box("Nose",(0,27.05,4.1),(.85,.8,.24),(184,149,117),"Head",False)
mouth=box("Mouth",(0,25.6,4.15),(1.65,.45,.2),(137,78,64),"Head",False)
mouth.shape_key_add(name="Basis");opened=mouth.shape_key_add(name="Open")
for v in opened.data:v.co.z=(v.co.z-(25.6-24))*3+(25.6-24)
for f,val in [(1,0),(25,0),(35,1),(45,0),(55,1),(65,0),(110,1),(150,0)]:
    opened.value=val;opened.keyframe_insert("value",frame=f)
with open(os.path.join(out,"character.json"),"w") as f:json.dump({"Pivots":{k:list(v) for k,v in pivots.items()},"Faces":export},f,separators=(",",":"))
# Editable pivot animation demo: neutral, wave, fall, recover.
for part,ob in rig.items():
    ob.rotation_mode="XYZ"
    for frame in [1,30,75,100,140,180]:
        ob.rotation_euler=(0,0,0)
        if part=="RightArm" and frame in [30,75]:ob.rotation_euler.x=-2.4
        if frame in [100,140]:
            if part=="LeftArm":ob.rotation_euler.y=-2.5
            if part=="RightArm":ob.rotation_euler.y=2.5
            if part=="LeftLeg":ob.rotation_euler.y=-.3
            if part=="RightLeg":ob.rotation_euler.y=.3
        ob.keyframe_insert("rotation_euler",frame=frame)
scene.frame_end=180;scene.frame_set(1)
camdata=bpy.data.cameras.new("Portrait");cam=bpy.data.objects.new("Portrait",camdata);scene.collection.objects.link(cam)
cam.location=(47,-78,49);target=Vector((0,0,16));cam.rotation_euler=(target-cam.location).to_track_quat('-Z','Y').to_euler();camdata.type='ORTHO';camdata.ortho_scale=43;scene.camera=cam
for name,loc,power,size in [("Soft key",(15,-25,45),18000,25),("Fill",(-25,-5,25),10000,25)]:
    d=bpy.data.lights.new(name,'AREA');d.energy=power;d.shape='DISK';d.size=size;o=bpy.data.objects.new(name,d);scene.collection.objects.link(o);o.location=loc;o.rotation_euler=(target-o.location).to_track_quat('-Z','Y').to_euler()
scene.world=bpy.data.worlds.new("BobbyBoi World");scene.world.color=(.25,.25,.25)
scene.render.resolution_x=600;scene.render.resolution_y=760;scene.render.resolution_percentage=100;scene.render.film_transparent=True
bpy.context.view_layer.objects.active=rig["Head"];rig["Head"].select_set(True)
for area in bpy.context.screen.areas:
    if area.type=='VIEW_3D':
        area.spaces.active.region_3d.view_distance=55;area.spaces.active.region_3d.view_location=(0,0,16)
        area.spaces.active.shading.type='MATERIAL'
bpy.ops.wm.save_as_mainfile(filepath=os.path.join(out,"BobbyBoi.blend"))
print("Saved editable Blender character and runtime mesh:",len(export),"pixel faces",len(palette),"materials")
