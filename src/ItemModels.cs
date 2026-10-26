using System.Numerics;
namespace BobbyBoi;
static class ItemModels {
 static readonly Dictionary<string,ModelAsset.MeshFace[]> cache=[];
 public static ModelAsset.MeshFace[] Get(string? name){if(string.IsNullOrEmpty(name))return [];if(cache.TryGetValue(name,out var found))return found;var faces=new List<ModelAsset.MeshFace>();
 void Cube(Vector3 center,Vector3 size,int[] color){Vector3 h=size/2;Vector3[] v=[center+new Vector3(-h.X,-h.Y,-h.Z),center+new Vector3(h.X,-h.Y,-h.Z),center+new Vector3(h.X,h.Y,-h.Z),center+new Vector3(-h.X,h.Y,-h.Z),center+new Vector3(-h.X,-h.Y,h.Z),center+new Vector3(h.X,-h.Y,h.Z),center+new Vector3(h.X,h.Y,h.Z),center+new Vector3(-h.X,h.Y,h.Z)];int[][] quads=[[0,3,2,1],[4,5,6,7],[0,4,7,3],[1,2,6,5],[3,7,6,2],[0,1,5,4]];foreach(var q in quads)faces.Add(new(){Part="RightArm",Name="Held "+name,Points=q.Select(i=>new[]{v[i].X,v[i].Y,v[i].Z}).ToArray(),Color=color});}
 if(name is "Dirt" or "Cobblestone"){for(int x=0;x<4;x++)for(int y=0;y<4;y++)for(int z=0;z<4;z++){int n=(x*17+y*7+z*13)%24;int[] c=name=="Dirt"?(y==3?[83+n,125+n,48+n]:[111+n,69+n,36+n]):[99+n,103+n,100+n];Cube(new(5.5f+(x-1.5f)*.85f,11+(y-1.5f)*.85f,3+(z-1.5f)*.85f),new(.85f),c);}}
 else {
 string[] map=name switch{
 "Paper"=>[".aaaaaa.",".abbbba.",".acccba.",".abbbba.",".acccba.",".abbbba.",".aaaaaa."],
 "Picture"=>["aaaaaaaa","abbbcbba","abccb bba".Replace(" ",""),"abccccba","abcccbba","aaaaaaaa"],
 "Diamond"=>["..aaaa..",".abbbba.","abcccbba",".abbbba.","..abba..","...aa..."],
 "Emerald"=>["..aaaa..",".abccba.",".abccba.",".abccba.",".abbbba.","..aaaa.."],
 "Apple"=>["....de..","...de...",".aabbba.","abbbcbba","abbbc bba".Replace(" ",""),"abbbbbba",".abbbba.","..a.aa.."],
 "Bread"=>["..aaaa..",".abbbba.","abcbc bba".Replace(" ",""),"abcbc bba".Replace(" ",""),"abbbbbba",".aaaaaa."],
 "Wooden Pickaxe"=>["..aaaaaaa.",".abbbbbba.",".aaad.aa..","...dd.....","..dd......",".dd.......","dd........"],
 "Diamond Sword"=>[".......aa.","......aba.",".....abba.","....abba..",".d.abba...","..ddaa....","..ded.....",".dd.dd....","dd........"],_=>[]};
 int[][] palette=name switch{
   "Paper"=>[[159,128,72],[243,235,199],[109,119,106]],
 "Picture"=>[[135,88,41],[110,171,197],[100,153,74]],
 "Diamond"=>[[23,107,117],[42,210,211],[184,255,246]],
 "Emerald"=>[[21,91,52],[41,176,82],[130,241,139]],
 "Apple"=>[[132,30,32],[220,51,41],[255,153,98],[97,61,29],[75,143,46]],
 "Bread"=>[[142,83,27],[223,162,66],[255,210,118]],
 "Wooden Pickaxe"=>[[83,48,22],[165,111,48],[223,170,83],[124,77,32]],
 _=>[[20,100,111],[43,210,211],[164,255,237],[88,57,29],[156,111,54]]};
 int width=map.Length==0?0:map.Max(r=>r.Length);float step=name.Contains("Sword")||name.Contains("Pickaxe")?.65f:.6f;
 for(int y=0;y<map.Length;y++)for(int x=0;x<map[y].Length;x++){int index=map[y][x]-'a';if(index<0||index>=palette.Length)continue;Cube(new(5.5f+(x-(width-1)/2f)*step,11+((map.Length-1)/2f-y)*step,3),new(step,step,.42f),palette[index]);}
 }
 if(faces.Count==0)return cache[name]=WorldAssets.Item(name).Held();return cache[name]=faces.ToArray();
 }
  // Model coordinates identify the actual handle / lower paper edge, not the sprite centre.
 public static Vector3 Grip(string name)=>name switch {
  "Wooden Pickaxe"=>new(3.225f,9.7f,3),
  "Diamond Sword"=>new(3.225f,9.05f,3),
  "Paper"=>new(5.5f,9.2f,3),
  "Picture"=>new(5.5f,9.5f,3),
  _=>new(5.5f,10.7f,3)
 };
 public static Vector3 InHand(string name,Vector3 point,float armAngle){
  Vector3 local=point-Grip(name);
  bool tool=name.Contains("Sword")||name.Contains("Pickaxe");
  // Straighten the diagonal pixel-art handle and pitch it slightly out of the forearm.
  if(tool)local=Vector3.Transform(local,Matrix4x4.CreateRotationZ(MathF.PI/4-.16f));
  // An articulated wrist keeps broad objects upright rather than edge-on during arm swings.
  float wrist=-armAngle*(tool?.55f:1f);
  local=Vector3.Transform(local,Matrix4x4.CreateRotationX(wrist));
  return local+new Vector3(5.5f,12.5f,2.35f);
 }
  public static void Preview(){using var sheet=new Bitmap(1040,540);using var graphics=Graphics.FromImage(sheet);int index=0;foreach(float yaw in new[]{-.95f,.95f})foreach(string name in new[]{"Paper","Picture","Wooden Pickaxe","Diamond Sword"}){using var tile=new Bitmap(260,270);using var g=Graphics.FromImage(tile);for(int n=0;n<24;n++){g.Clear(Color.Transparent);Character.Draw(g,new(130,240),4.5f,Act.Walk,1,yaw,0,0,name);}graphics.DrawImageUnscaled(tile,index%4*260,index/4*270);using var font=new Font("Consolas",10);graphics.DrawString(name,font,Brushes.White,index%4*260+35,index/4*270+250);index++;}sheet.Save("artifacts/grips.png");}
 public static void Validate(){foreach(var name in SaveData.Names.Concat(new[]{"Paper","Picture"})){var anchor=InHand(name,Grip(name),-1.2f);if(Vector3.Distance(anchor,new(5.5f,12.5f,2.35f))>.001f)throw new Exception("Detached item grip: "+name);var mesh=Get(name);if(mesh.Length<6||mesh.Any(f=>f.Points.Length!=4))throw new Exception("Missing gift model: "+name);}}
}
