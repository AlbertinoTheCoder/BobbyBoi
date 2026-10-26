import bpy, json, random, os, math
from mathutils import Vector
out=os.path.dirname(os.path.abspath(__file__))
scene=bpy.data.scenes.new("BobbyBoi Mob Workshop");bpy.context.window.scene=scene
collection=bpy.data.collections.new("Blender Designed Mobs");scene.collection.children.link(collection)
palette={}
def cv(p):return (p[0],-p[2],p[1])
def material(rgb):
    if rgb not in palette:
        m=bpy.data.materials.new("Mob Pixel "+str(rgb));m.diffuse_color=tuple(c/255 for c in rgb)+(1,);m.use_nodes=True
        s=next(n for n in m.node_tree.nodes if n.type=="BSDF_PRINCIPLED");s.inputs["Base Color"].default_value=tuple(c/12.92 if c<=.04045 else ((c+.055)/1.055)**2.4 for c in m.diffuse_color[:3])+(1,);s.inputs["Roughness"].default_value=1
        palette[rgb]=m
    return palette[rgb]
def make_mob(kind,offset):
    pivots={"Body":(0,12,0),"Head":(0,24,0),"LeftArm":(-6,24,0),"RightArm":(6,24,0),"LeftLeg":(-2,12,0),"RightLeg":(2,12,0)} if kind=="zombie" else {"Body":(0,6,0),"Head":(0,18,0),"FrontLeft":(-2,6,2.5),"FrontRight":(2,6,2.5),"BackLeft":(-2,6,-2.5),"BackRight":(2,6,-2.5)}
    rig={};faces_out=[]
    for name,p in pivots.items():
        ob=bpy.data.objects.new(kind+" "+name+" pivot",None);collection.objects.link(ob);ob.location=cv((p[0]+offset,p[1],p[2]));ob.empty_display_size=.5;rig[name]=ob
    def box(name,c,size,color,part="Body",pixels=True):
        vs=[];qs=[];cols=[];r=random.Random(kind+name)
        cx,cy,cz=c;hx,hy,hz=[s/2 for s in size]
        for side in range(6):
            uw=size[2] if side<2 else size[0];vh=size[1] if side<4 else size[2];nu=max(1,round(uw)) if pixels else 1;nv=max(1,round(vh)) if pixels else 1
            def point(a,b):return [(cx-hx,cy-hy+b,cz-hz+a),(cx+hx,cy-hy+b,cz+hz-a),(cx-hx+a,cy-hy+b,cz+hz),(cx+hx-a,cy-hy+b,cz-hz),(cx-hx+a,cy+hy,cz+hz-b),(cx-hx+a,cy-hy,cz-hz+b)][side]
            for u in range(nu):
                for v in range(nv):
                    a=u*uw/nu;b=v*vh/nv;du=uw/nu;dv=vh/nv
                    q=[point(a,b),point(a+du,b),point(a+du,b+dv),point(a,b+dv)];k=len(vs);vs.extend(q);qs.append(tuple(range(k,k+4)))
                    n=r.choice([-16,-8,-3,0,0,6,12]) if pixels else 0;cols.append(tuple(max(0,min(255,x+n)) for x in color))
        p=pivots[part];mesh=bpy.data.meshes.new(kind+" "+name);mesh.from_pydata([cv(tuple(v[i]-p[i] for i in range(3))) for v in vs],[],qs);mesh.update()
        ob=bpy.data.objects.new(kind+" "+name,mesh);collection.objects.link(ob);ob.parent=rig[part];used={}
        for poly,c in zip(mesh.polygons,cols):
            if c not in used:used[c]=len(used);mesh.materials.append(material(c))
            poly.material_index=used[c]
        for q,c in zip(qs,cols):faces_out.append({"Part":part,"Name":name,"Points":[list(vs[i]) for i in q],"Color":list(c)})
    if kind=="zombie":
        green=(79,125,62);shirt=(49,135,141)
        box("ragged teal shirt",(0,18,0),(8,12,4),shirt)
        box("exposed neck",(0,23,2.06),(2.6,1.8,.15),green,pixels=False)
        for x,y in [(-3,12.8),(1,13.2),(3,14)]:box("torn hem "+str(x),(x,y,2.08),(1.4,2.1,.18),(51,75,49),pixels=False)
        box("head",(0,28,0),(8,8,8),green,"Head")
        for sign,part in [(-1,"LeftArm"),(1,"RightArm")]:
            box("arm "+part,(sign*6,18,0),(4,12,4),green,part)
            box("sleeve "+part,(sign*6,22,0),(4.1,4.1,4.1),shirt,part)
        for sign,part in [(-1,"LeftLeg"),(1,"RightLeg")]:
            box("trousers "+part,(sign*2,7,0),(4,10,4),(65,58,104),part)
            box("boot "+part,(sign*2,1,0),(4.1,2,4.2),(44,46,45),part)
        for x in [-2,2]:
            box("dark eye "+str(x),(x,28.4,4.08),(2,1.15,.18),(25,43,21),"Head",False)
            box("brow "+str(x),(x,29.4,4.08),(2.2,.55,.16),(55,84,37),"Head",False)
        box("mouth",(0,25.8,4.09),(3,.8,.2),(33,51,28),"Head",False)
        box("tooth",(.7,26,4.21),(.6,.45,.1),(161,170,111),"Head",False)
        for part in ["LeftArm","RightArm"]:rig[part].rotation_euler.x=-1.35
    else:
        green=(91,159,58)
        box("mottled body",(0,12,0),(6,12,4),green)
        box("square head",(0,22,0),(8,8,8),green,"Head")
        for name,p in pivots.items():
            if name.startswith("Front") or name.startswith("Back"):box("foot "+name,(p[0],3,p[2]),(4,6,4),(67,119,43),name)
        for x in [-2,2]:box("black eye "+str(x),(x,23,4.09),(2,2,.18),(17,31,15),"Head",False)
        box("nose",(0,21.3,4.1),(2,2,.2),(17,31,15),"Head",False)
        box("mouth",(0,19.8,4.1),(4,1.5,.2),(17,31,15),"Head",False)
        for x in [-1.4,1.4]:box("mouth lower "+str(x),(x,18.8,4.1),(1.2,1.5,.2),(17,31,15),"Head",False)
    for name,ob in rig.items():
        base=ob.rotation_euler.copy()
        for f,value in [(1,0),(9,.65),(17,0),(25,-.65),(33,0)]:
            ob.rotation_euler=base.copy()
            if "Leg" in name or name.startswith("Front") or name.startswith("Back"):ob.rotation_euler.x=value*(-1 if "Right" in name else 1)
            ob.keyframe_insert("rotation_euler",frame=f)
    with open(os.path.join(out,kind+".json"),"w") as file:json.dump({"Pivots":{k:list(v) for k,v in pivots.items()},"Faces":faces_out},file,separators=(",",":"))
    return len(faces_out)
counts={kind:make_mob(kind,offset) for kind,offset in [("zombie",-11),("creeper",11)]}
scene.frame_end=33;scene.frame_set(1)
camdata=bpy.data.cameras.new("Mob Portrait");cam=bpy.data.objects.new("Mob Portrait",camdata);scene.collection.objects.link(cam);cam.location=(43,-87,43);target=Vector((0,0,15));cam.rotation_euler=(target-cam.location).to_track_quat('-Z','Y').to_euler();camdata.type='ORTHO';camdata.ortho_scale=55;scene.camera=cam
scene.render.resolution_x=960;scene.render.resolution_y=760;scene.render.resolution_percentage=100;scene.render.film_transparent=True
for name,loc in [("Mob key",(15,-30,45)),("Mob fill",(-35,-15,25))]:
    d=bpy.data.lights.new(name,'AREA');d.energy=15000;d.size=25;o=bpy.data.objects.new(name,d);scene.collection.objects.link(o);o.location=loc;o.rotation_euler=(target-o.location).to_track_quat('-Z','Y').to_euler()
scene.world=bpy.data.worlds.new("Mob world");scene.world.color=(.3,.3,.3)
bpy.ops.wm.save_as_mainfile(filepath=os.path.join(out,"BobbyBoiMobs.blend"))
print("Blender-designed mobs exported:",counts)
