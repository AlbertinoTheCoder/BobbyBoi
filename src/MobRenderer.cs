using System.Numerics;
using System.Text.Json;
namespace BobbyBoi;
static class MobRenderer {
 static readonly Dictionary<string,ModelAsset> models=new();
 public static ModelAsset Model(string kind){if(models.TryGetValue(kind,out var found))return found;using var stream=typeof(MobRenderer).Assembly.GetManifestResourceStream("BobbyBoi.assets."+kind+".json")??throw new Exception("Missing Blender mob mesh: "+kind);return models[kind]=JsonSerializer.Deserialize<ModelAsset>(stream)??throw new Exception("Invalid mob mesh");}
 public static void Draw(Graphics g,string kind,Vector2 origin,float scale,float age,float direction,float fuse){var model=Model(kind);float yaw=direction*.95f,swing=MathF.Sin(age*7)*.6f;float inflate=fuse>0?1+MathF.Sin(fuse*24)*.035f:1;bool flash=fuse>0&&MathF.Sin(fuse*(12+fuse*9))>.25f;var raster=new PixelDepthBuffer(Math.Max(1,(int)g.VisibleClipBounds.Width),Math.Max(1,(int)g.VisibleClipBounds.Height));
  foreach(var face in model.Faces){var pp=model.Pivots[face.Part];var pivot=new Vector3(pp[0],pp[1],pp[2]);float angle=0;if(face.Part.Contains("Leg")||face.Part.StartsWith("Front")||face.Part.StartsWith("Back"))angle=swing*(face.Part.Contains("Right")?-1:1)*(face.Part.StartsWith("Back")?-1:1);if(face.Part.Contains("Arm"))angle=-1.35f+MathF.Sin(age*2)*.06f;
   var q=face.Points.Select(a=>{var v=new Vector3(a[0],a[1],a[2])-pivot;v=new(v.X,v.Y*MathF.Cos(angle)-v.Z*MathF.Sin(angle),v.Y*MathF.Sin(angle)+v.Z*MathF.Cos(angle));v=(v+pivot)*inflate;return new Vector3(v.X*MathF.Cos(yaw)+v.Z*MathF.Sin(yaw),v.Y,-v.X*MathF.Sin(yaw)+v.Z*MathF.Cos(yaw));}).ToArray();var n=Vector3.Normalize(Vector3.Cross(q[1]-q[0],q[2]-q[0]));if(Vector3.Dot(n,new(0,.34f,1))<=0)continue;float shade=Math.Clamp(.76f+n.Y*.23f+n.Z*.15f,.54f,1.06f);bool facial=face.Name.Contains("eye")||face.Name.Contains("mouth")||face.Name=="nose";int R(int i)=>flash&&!facial?Math.Clamp((int)(face.Color[i]*.25f+190),0,255):Math.Clamp((int)(face.Color[i]*shade),0,255);var c=Color.FromArgb(R(0),R(1),R(2));var points=q.Select(v=>new Vector3(origin.X+v.X*scale,origin.Y-v.Y*scale+v.Z*scale*.34f,v.Z+v.Y*.34f)).ToArray();raster.Triangle(points[0],points[1],points[2],c);raster.Triangle(points[0],points[2],points[3],c);
  }
  using var image=raster.Image();g.DrawImageUnscaled(image,0,0);
 }
}
sealed class EffectOverlay:Form {
 Bitmap? frame;
 public EffectOverlay(Size size){FormBorderStyle=FormBorderStyle.None;ShowInTaskbar=false;StartPosition=FormStartPosition.Manual;Size=size;Text="BobbyBoi mob effect";}
 protected override bool ShowWithoutActivation=>true;
 protected override CreateParams CreateParams {get{var p=base.CreateParams;p.ExStyle|=0x80000|0x80|0x20|0x08000000;return p;}}
 protected override void WndProc(ref Message m){if(m.Msg==0x84){m.Result=-1;return;}base.WndProc(ref m);}
 public void Present(Point position,Bitmap bitmap,bool topmost){if(IsDisposed){bitmap.Dispose();return;}if(TopMost!=topmost)TopMost=topmost;Location=position;if(!Visible)Show();Native.Present(this,bitmap);frame?.Dispose();frame=bitmap;}
 protected override void Dispose(bool disposing){if(disposing)frame?.Dispose();base.Dispose(disposing);}
}
sealed class ExplosionVisual:IDisposable {
 public readonly Vector2 Position;readonly EffectOverlay window=new(new Size(520,420));readonly List<(Vector2 velocity,float size,Color color)> particles=[];public float Age{get;private set;}
 public bool Finished=>Age>1.35f;
 public ExplosionVisual(Vector2 position,int seed){Position=position;var rng=new Random(seed);for(int i=0;i<48;i++){float a=(float)rng.NextDouble()*MathF.Tau;float speed=rng.Next(65,300);particles.Add((new Vector2(MathF.Cos(a)*speed,MathF.Sin(a)*speed-85),rng.Next(4,14),i%3==0?Color.FromArgb(100,105,91):i%3==1?Color.Orange:Color.LightGoldenrodYellow));}}
 public void Step(float dt){Age+=dt;}
 public void Draw(Graphics g){float t=Age;Vector2 center=new(260,230);if(t<.65f){float radius=15+t*210;using var pen=new Pen(Color.FromArgb((int)(220*(1-t/.65f)),Color.LightGoldenrodYellow),Math.Max(2,18*(1-t/.65f)));g.DrawEllipse(pen,center.X-radius,center.Y-radius,radius*2,radius*2);}
  if(t<.16f){using var flash=new SolidBrush(Color.FromArgb((int)(180*(1-t/.16f)),Color.White));g.FillEllipse(flash,center.X-65,center.Y-65,130,130);}
  foreach(var p in particles){Vector2 point=center+p.velocity*t+new Vector2(0,180*t*t);float size=p.size*(1-t/1.5f);if(size<=0)continue;using var b=new SolidBrush(Color.FromArgb(Math.Clamp((int)(255*(1-t/1.4f)),0,255),p.color));g.FillRectangle(b,point.X-size/2,point.Y-size/2,size,size);}
 }
 public void Render(bool onTop){var bitmap=new Bitmap(520,420,System.Drawing.Imaging.PixelFormat.Format32bppArgb);using(var g=Graphics.FromImage(bitmap)){g.Clear(Color.Transparent);Draw(g);}window.Present(new Point((int)Position.X-260,(int)Position.Y-230),bitmap,onTop);}
 public void Hide()=>window.Hide();public void Dispose()=>window.Dispose();
}
