using System.Text.Json;
namespace BobbyBoi;
sealed class ModelAsset {
 public Dictionary<string,float[]> Pivots{get;set;}=[];
 public MeshFace[] Faces{get;set;}=[];
 public sealed class MeshFace { public string Part{get;set;}="Body"; public string Name{get;set;}="";public float[][] Points{get;set;}=[];public int[] Color{get;set;}=[]; }
  public void Validate(){
  if(Pivots.Count!=6||!Faces.Any(f=>f.Name=="Mouth")||!Faces.Any(f=>f.Name=="Hair crown"))throw new Exception("Incomplete Blender export");
  foreach(var f in Faces){if(f.Points.Length!=4||f.Color.Length!=3||!Pivots.ContainsKey(f.Part))throw new Exception("Invalid mesh face");var a=f.Points[0];var b=f.Points[1];var c=f.Points[2];var ab=new System.Numerics.Vector3(b[0]-a[0],b[1]-a[1],b[2]-a[2]);var ac=new System.Numerics.Vector3(c[0]-a[0],c[1]-a[1],c[2]-a[2]);if(System.Numerics.Vector3.Cross(ab,ac).LengthSquared()<.0000001f)throw new Exception("Degenerate mesh face");}
 }
 public static ModelAsset Load(){using var stream=typeof(ModelAsset).Assembly.GetManifestResourceStream("BobbyBoi.assets.character.json")??throw new Exception("Bundled Blender mesh missing");return JsonSerializer.Deserialize<ModelAsset>(stream)??throw new Exception("Invalid Blender mesh");}
}
