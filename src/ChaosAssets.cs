using System.Numerics;
using System.Text.Json;
namespace BobbyBoi;
static class ChaosAssets {
 sealed class Face {public float[][] Points{get;set;}=[];public int[] Color{get;set;}=[];public string Part{get;set;}="body";}
 static readonly Dictionary<string,VoxelMesh> meshes=Load();
 static Dictionary<string,VoxelMesh> Load(){using var s=typeof(ChaosAssets).Assembly.GetManifestResourceStream("BobbyBoi.assets.chaos.json")??throw new Exception("Expansion Blender assets missing");var data=JsonSerializer.Deserialize<Dictionary<string,Face[]>>(s)!;var result=new Dictionary<string,VoxelMesh>();foreach(var pair in data){var mesh=new VoxelMesh();foreach(var f in pair.Value){if(f.Points.Length!=4||f.Color.Length!=3)throw new Exception("Invalid expansion mesh");mesh.Faces.Add((f.Points.Select(p=>new Vector3(p[0],p[1],p[2])).ToArray(),Color.FromArgb(f.Color[0],f.Color[1],f.Color[2]),f.Part));}result[pair.Key]=mesh;}return result;}
 public static IEnumerable<string> Names=>meshes.Keys;
 static readonly SpriteCache sprites=new(12*1024*1024);
 public static void Draw(Graphics g,string name,Vector2 feet,float scale,float yaw,float time=0,bool animated=false,int buildStage=100){yaw=MathF.Round(yaw*24)/24;int pose=animated?(int)MathF.Round(MathF.Sin(time*5)*4):0;string key=$"{name}/{scale}/{yaw}/{pose}/{buildStage}";var image=sprites.Get(key);if(image==null){image=new Bitmap(320,320);using(var cg=Graphics.FromImage(image))meshes[name].Draw(cg,new(160,250),scale,yaw,(v,part)=>{if(name=="golem"&&(part.StartsWith("leg")||part.StartsWith("arm"))){bool leg=part.StartsWith("leg"),left=part.EndsWith("L");var pivot=new Vector3((left?-1:1)*(leg?3.5f:10),leg?12:30,0);float angle=pose*(leg?.16f:-.10f)*(left?1:-1);v-=pivot;v=new(v.X,v.Y*MathF.Cos(angle)-v.Z*MathF.Sin(angle),v.Y*MathF.Sin(angle)+v.Z*MathF.Cos(angle));v+=pivot;}else if(part is "leg" or "arm")v.Z+=pose*.25f;if(part=="lever")v.Y+=pose*.3f;return v;},(meshes[name].Faces.Count*Math.Clamp(buildStage,0,100)/100/6)*6);sprites.Add(key,image);}g.DrawImageUnscaled(image,(int)feet.X-160,(int)feet.Y-250);}
}
