using System.Numerics;
namespace BobbyBoi;
sealed class VoxelMesh {
 public readonly List<(Vector3[] Points,Color Color,string Part)> Faces=[];
 public void Box(Vector3 center,Vector3 size,Color color,string part="body",bool pixels=false){var h=size/2;int counter=0;
  for(int side=0;side<6;side++){float uw=side<2?size.Z:size.X,vh=side<4?size.Y:size.Z;int nu=pixels?Math.Max(1,(int)Math.Ceiling(uw/2)):1,nv=pixels?Math.Max(1,(int)Math.Ceiling(vh/2)):1;
   Vector3 P(float a,float b)=>side switch{0=>new(center.X-h.X,center.Y-h.Y+b,center.Z-h.Z+a),1=>new(center.X+h.X,center.Y-h.Y+b,center.Z+h.Z-a),2=>new(center.X-h.X+a,center.Y-h.Y+b,center.Z+h.Z),3=>new(center.X+h.X-a,center.Y-h.Y+b,center.Z-h.Z),4=>new(center.X-h.X+a,center.Y+h.Y,center.Z+h.Z-b),_=>new(center.X-h.X+a,center.Y-h.Y,center.Z-h.Z+b)};
   for(int u=0;u<nu;u++)for(int v=0;v<nv;v++){float a=u*uw/nu,b=v*vh/nv,du=uw/nu,dv=vh/nv;int noise=pixels?((counter++*17+u*13+v*7)%19)-9:0;var c=Color.FromArgb(Math.Clamp(color.R+noise,0,255),Math.Clamp(color.G+noise,0,255),Math.Clamp(color.B+noise,0,255));Faces.Add(([P(a,b),P(a+du,b),P(a+du,b+dv),P(a,b+dv)],c,part));}
  }
 }
 public void Draw(Graphics g,Vector2 origin,float scale,float yaw,Func<Vector3,string,Vector3>? deform=null,int limit=int.MaxValue){var faces=new List<(Vector3[] P,Color C)>();float cs=MathF.Cos(yaw),sn=MathF.Sin(yaw);
  foreach(var face in Faces.Take(limit)){var q=face.Points.Select(p=>{if(deform!=null)p=deform(p,face.Part);return new Vector3(p.X*cs+p.Z*sn,p.Y,-p.X*sn+p.Z*cs);}).ToArray();var n=Vector3.Cross(q[1]-q[0],q[2]-q[0]);if(n.LengthSquared()<.00001f)continue;n=Vector3.Normalize(n);if(Vector3.Dot(n,new(0,.34f,1))<=0)continue;float light=Math.Clamp(.75f+n.Y*.23f+n.Z*.15f,.52f,1.05f);var color=Color.FromArgb((int)Math.Clamp(face.Color.R*light,0,255),(int)Math.Clamp(face.Color.G*light,0,255),(int)Math.Clamp(face.Color.B*light,0,255));faces.Add((q.Select(p=>new Vector3(origin.X+p.X*scale,origin.Y-p.Y*scale+p.Z*scale*.34f,p.Z+p.Y*.34f)).ToArray(),color));}
  if(faces.Count==0)return;float left=MathF.Floor(faces.Min(f=>f.P.Min(p=>p.X)))-1,top=MathF.Floor(faces.Min(f=>f.P.Min(p=>p.Y)))-1;int w=(int)MathF.Ceiling(faces.Max(f=>f.P.Max(p=>p.X))-left)+2,h=(int)MathF.Ceiling(faces.Max(f=>f.P.Max(p=>p.Y))-top)+2;if(w<=0||h<=0||w>1600||h>1600)return;var raster=new PixelDepthBuffer(w,h);
  foreach(var face in faces){var p=face.P.Select(v=>v-new Vector3(left,top,0)).ToArray();raster.Triangle(p[0],p[1],p[2],face.C);raster.Triangle(p[0],p[2],p[3],face.C);}using var image=raster.Image();g.DrawImageUnscaled(image,(int)left,(int)top);
 } public ModelAsset.MeshFace[] Held(){return Faces.Select(f=>new ModelAsset.MeshFace{Part="RightArm",Name="Held world prop",Color=[f.Color.R,f.Color.G,f.Color.B],Points=f.Points.Select(p=>new[]{p.X+5.5f,p.Y+11,p.Z+3}).ToArray()}).ToArray();}
}
static class WorldAssets {
 static readonly Dictionary<string,VoxelMesh> cache=[];
 public static VoxelMesh Item(string name){if(cache.TryGetValue(name,out var mesh))return mesh;mesh=new();var wood=Color.FromArgb(121,77,38);var gold=Color.Goldenrod;
  if(name is "Diamond" or "Emerald"){mesh.Box(new(0,0,0),new(2.5f,3.5f,1),name=="Diamond"?Color.Turquoise:Color.LimeGreen);mesh.Box(new(0,0,.6f),new(.65f,2,.2f),Color.PaleTurquoise);}
  else if(name.Contains("Rod")){mesh.Box(new(0,3,0),new(.5f,9,.5f),wood);mesh.Box(new(.7f,-.5f,.3f),new(1.6f,1.3f,1),Color.SlateGray);}
  else if(name.Contains("Cod")||name.Contains("Salmon")||name.Contains("Fish")){var c=name.Contains("Salmon")?Color.Salmon:Color.Silver;mesh.Box(new(0,0,0),new(5,1.8f,1.2f),c);mesh.Box(new(-2.8f,0,0),new(1.5f,3,.7f),Color.Teal);mesh.Box(new(1.8f,.4f,.66f),new(.4f,.4f,.1f),Color.Black);}
  else if(name.Contains("Boot")){mesh.Box(new(0,.7f,0),new(1.7f,3,2),wood);mesh.Box(new(1,-.6f,0),new(3,1.2f,2.2f),Color.SaddleBrown);}
  else if(name.Contains("Bone")){mesh.Box(new(0,0,0),new(.8f,4,.8f),Color.Beige);mesh.Box(new(0,2,0),new(2,1,1),Color.Beige);mesh.Box(new(0,-2,0),new(2,1,1),Color.Beige);}
  else if(name.Contains("Stick")||name=="String"){mesh.Box(new(0,0,0),new(.6f,5,.6f),name=="String"?Color.Beige:wood);}
  else if(name.Contains("Chest")){mesh.Box(new(0,0,0),new(4,3,3),wood);mesh.Box(new(0,.2f,1.6f),new(.7f,1,.3f),gold);}
  else if(name.Contains("Book")||name.Contains("Tag")||name.Contains("Bill")||name.Contains("Paper")||name.Contains("issue")||name.Contains("nothing")||name.Contains("Error")){mesh.Box(new(0,.4f,0),new(3,4,.5f),name.Contains("Book")?Color.Purple:Color.Beige);mesh.Box(new(0,1,.3f),new(2,.3f,.1f),Color.IndianRed);}
  else if(name.Contains("Apple")){mesh.Box(new(0,0,0),new(3,2.5f,2),gold);mesh.Box(new(0,1.7f,0),new(.4f,1,.4f),wood);}
  else if(name.Contains("Trophy")){mesh.Box(new(0,1,0),new(3,3,2),gold);mesh.Box(new(0,-1,0),new(1,2,1),gold);mesh.Box(new(0,-2,0),new(3,.6f,2),wood);}
  else if(name.Contains("Table")){mesh.Box(new(0,1,0),new(4,1,3),wood);for(int x=-1;x<=1;x+=2)for(int z=-1;z<=1;z+=2)mesh.Box(new(x,-.5f,z),new(.7f,3,.7f),wood);}
  else if(name.Contains("Bowl")){mesh.Box(new(0,0,0),new(3,1,2),wood);mesh.Box(new(0,.7f,0),new(2,.6f,1),Color.Black);}
  else if(name.Contains("Steve")||name.Contains("Mini")){mesh.Box(new(0,1.7f,0),new(2,2,2),Color.BurlyWood);mesh.Box(new(0,-.2f,0),new(1.8f,2,1),Color.Teal);mesh.Box(new(0,-1.7f,0),new(1.7f,1,1),Color.MidnightBlue);}
  else{mesh.Box(new(0,0,0),new(3,2.5f,2.5f),name.Contains("Flesh")?Color.OliveDrab:Color.Peru,true.ToString(),true);}
  cache[name]=mesh;return mesh;
 }
 public static VoxelMesh Chest{get;}=MakeChest();
 static VoxelMesh MakeChest(){var m=new VoxelMesh();var c=Color.FromArgb(145,88,35);m.Box(new(0,5,0),new(16,10,12),c,"base",true);m.Box(new(0,11,0),new(16.5f,3,12.5f),Color.Peru,"lid",true);m.Box(new(0,9,6.4f),new(2,3,.7f),Color.Goldenrod);for(int x=-1;x<=1;x+=2)m.Box(new(x*6,5,6.2f),new(.8f,9,.4f),Color.FromArgb(70,43,25));return m;}
 public static VoxelMesh Wolf{get;}=MakeWolf();
 static VoxelMesh MakeWolf(){var m=new VoxelMesh();var c=Color.FromArgb(174,177,171);m.Box(new(0,10,0),new(7,7,13),c,"body",true);m.Box(new(0,15,7),new(8,7,7),Color.LightGray,"head",true);m.Box(new(0,13,11.8f),new(5,3,4),Color.Gainsboro,"head");m.Box(new(0,13,14),new(2,1.5f,.6f),Color.FromArgb(35,34,34),"head");for(int s=-1;s<=1;s+=2){m.Box(new(s*2.5f,19.5f,7),new(2,4,2),c,"head");m.Box(new(s*2.3f,16,10.7f),new(1,1,.4f),Color.Black,"head");for(int z=-1;z<=1;z+=2)m.Box(new(s*2.4f,4.5f,z*4.3f),new(2,9,2),c,s==z?"legA":"legB",true);}m.Box(new(0,12,4.5f),new(7.3f,2,2),Color.IndianRed);m.Box(new(0,13,-8),new(2,2,7),c,"tail",true);return m;}
 public static VoxelMesh Dragon{get;}=MakeDragon();
 static VoxelMesh MakeDragon(){var m=new VoxelMesh();var c=Color.FromArgb(36,25,44);m.Box(new(0,12,0),new(10,9,19),c,"body",true);m.Box(new(0,15,15),new(9,8,11),c,"head",true);m.Box(new(0,13,22),new(8,4,7),c,"head");for(int s=-1;s<=1;s+=2){m.Box(new(s*4.6f,17,17),new(.4f,1.3f,4),Color.MediumPurple,"head");m.Box(new(s*3,20,13),new(1.3f,5,1.3f),Color.Gray,"head");m.Box(new(s*14,13,-1),new(24,.8f,15),Color.FromArgb(66,38,75),s<0?"wingL":"wingR");m.Box(new(s*14,13.5f,6),new(25,1,1.5f),c,s<0?"wingL":"wingR");for(int z=-1;z<=1;z+=2)m.Box(new(s*4,6,z*6),new(2,8,2),c,"legs");}for(int i=0;i<5;i++)m.Box(new(0,12-i*.9f,-13-i*5),new(5-i*.65f,5-i*.65f,6),c,"tail"+i,true);return m;}
 public static (VoxelMesh Mesh,int[] Cutoffs) House(int variant){var m=new VoxelMesh();var cuts=new List<int>();void Block(int x,int y,int z,bool roof=false){m.Box(new(x*8,y*8+4,z*8),new(8),roof?Color.FromArgb(92,111,47):Color.FromArgb(128,85,43),"block",true);cuts.Add(m.Faces.Count);}
  for(int x=-2;x<=2;x++)for(int z=-1;z<=1;z++)Block(x,0,z);
  if(variant!=3)for(int y=1;y<=3;y++){for(int x=-2;x<=2;x++)Block(x,y,-1);for(int z=0;z<=1;z++){Block(-2,y,z);Block(2,y,z);}for(int x=-1;x<=1;x++)if((x!=0||y==3)&&(x!= -1||y!=2))Block(x,y,1);}
  int roof=variant==2?4:3;for(int x=-roof;x<=roof;x++)for(int z=-2;z<=2;z++)Block(x,4,z,true);
  m.Box(new(0,12,12.1f),new(7,16,.8f),Color.FromArgb(129,88,42),"door",true);m.Box(new(2,12,12.6f),new(.7f,.7f,.4f),Color.Goldenrod,"door");m.Box(new(-8,20,12.1f),new(6,6,.4f),Color.LightSkyBlue);m.Box(new(-9,10,0),new(10,3,13),Color.IndianRed);m.Box(new(-9,11, -4),new(10,1,4),Color.Beige);m.Box(new(11,13,12.4f),new(1.1f,6,1.1f),Color.SaddleBrown);m.Box(new(11,17,12.4f),new(2,2,2),Color.Orange);cuts.Add(m.Faces.Count);return(m,cuts.ToArray());
 }
}
