using System.Numerics;
namespace BobbyBoi;
record SceneVisual(Rectangle Bounds,Action<Graphics,Vector2> Draw,Action<MouseButtons>? Click=null);
sealed class WorldSurface:Form {
 Bitmap? frame;public readonly List<(Rectangle Bounds,Action<MouseButtons> Action)> Hits=[];
 public WorldSurface(){FormBorderStyle=FormBorderStyle.None;ShowInTaskbar=false;StartPosition=FormStartPosition.Manual;Text="BobbyBoi world";MouseDown+=(_,e)=>{var p=PointToScreen(e.Location);var hit=Hits.LastOrDefault(h=>h.Bounds.Contains(p));hit.Action?.Invoke(e.Button);};}
 protected override bool ShowWithoutActivation=>true;
 protected override CreateParams CreateParams{get{var p=base.CreateParams;p.ExStyle|=0x80000|0x80|0x08000000;return p;}}
 protected override void WndProc(ref Message m){if(m.Msg==0x84){var screen=new Point((short)(m.LParam.ToInt64()&65535),(short)(m.LParam.ToInt64()>>16));var p=PointToClient(screen);m.Result=frame!=null&&Hits.Any(h=>h.Bounds.Contains(screen))&&p.X>=0&&p.Y>=0&&p.X<frame.Width&&p.Y<frame.Height&&frame.GetPixel(p.X,p.Y).A>15?(nint)1:(nint)(-1);return;}base.WndProc(ref m);}
 public void Present(Rectangle bounds,Bitmap image,bool onTop){if(TopMost!=onTop)TopMost=onTop;Location=bounds.Location;if(!Visible)Show();Native.Present(this,image);frame?.Dispose();frame=image;}
 protected override void Dispose(bool disposing){if(disposing)frame?.Dispose();base.Dispose(disposing);}
}
sealed class WorldCanvas:IDisposable {
 readonly Dictionary<string,WorldSurface> surfaces=[];float sinceRender;
 // A toast at the top and a house at the bottom must not allocate a fullscreen bitmap.
 internal static List<Rectangle> Regions(IEnumerable<Rectangle> source,Rectangle screen){var result=new List<Rectangle>();foreach(var original in source){var r=Rectangle.Intersect(original,screen);if(r.IsEmpty||r.Width<1||r.Height<1)continue;r.Inflate(6,6);r=Rectangle.Intersect(r,screen);for(int i=0;i<result.Count;){var nearby=result[i];nearby.Inflate(16,16);if(nearby.IntersectsWith(r)){r=Rectangle.Union(r,result[i]);result.RemoveAt(i);i=0;}else i++;}result.Add(r);}while(result.Count>12){int a=0,b=1;long cost=long.MaxValue;for(int i=0;i<result.Count;i++)for(int j=i+1;j<result.Count;j++){var union=Rectangle.Union(result[i],result[j]);long n=(long)union.Width*union.Height-(long)result[i].Width*result[i].Height-(long)result[j].Width*result[j].Height;if(n<cost){a=i;b=j;cost=n;}}result[a]=Rectangle.Union(result[a],result[b]);result.RemoveAt(b);}return result.OrderBy(r=>r.Y).ThenBy(r=>r.X).ToList();}
 public void Render(float dt,IReadOnlyList<SceneVisual> visuals,bool topmost,bool performance=false){sinceRender+=dt;if(sinceRender<1f/(performance?10:20))return;sinceRender=0;var active=new HashSet<string>();foreach(var screen in Screen.AllScreens){int index=0;foreach(var bounds in Regions(visuals.Select(v=>v.Bounds),screen.Bounds)){var objects=visuals.Where(v=>v.Bounds.IntersectsWith(bounds)).ToArray();string key=screen.DeviceName+"/"+index++;active.Add(key);if(!surfaces.TryGetValue(key,out var surface)){surfaces[key]=surface=new();}surface.Hits.Clear();var image=new Bitmap(bounds.Width,bounds.Height,System.Drawing.Imaging.PixelFormat.Format32bppArgb);using(var g=Graphics.FromImage(image)){g.Clear(Color.Transparent);g.SmoothingMode=System.Drawing.Drawing2D.SmoothingMode.None;foreach(var obj in objects){obj.Draw(g,new Vector2(-bounds.X,-bounds.Y));if(obj.Click!=null)surface.Hits.Add((obj.Bounds,obj.Click));}}surface.Present(bounds,image,topmost);}}foreach(var s in surfaces)if(!active.Contains(s.Key))s.Value.Hide();}
 public void Hide(){foreach(var s in surfaces.Values)s.Hide();}
 public void Dispose(){foreach(var s in surfaces.Values)s.Dispose();surfaces.Clear();}
 public static Bitmap Snapshot(IEnumerable<SceneVisual> visuals,Rectangle bounds){var b=new Bitmap(bounds.Width,bounds.Height);using(var g=Graphics.FromImage(b)){g.Clear(Color.Transparent);foreach(var v in visuals)v.Draw(g,new(-bounds.X,-bounds.Y));}return b;}
}
record WorldParticle(Vector2 Start,Vector2 Velocity,Color Color,float Life,string? Text=null){public float Age;}
sealed class WorldParticles {
 readonly List<WorldParticle> particles=[];readonly Random rng=new(37);public int Count=>particles.Count;
 public void Burst(Vector2 p,Color color,int count=12,string? text=null){for(int i=0;i<count&&particles.Count<96;i++)particles.Add(new(p,new(rng.Next(-75,76),rng.Next(-120,-25)),color,1.5f,text));}
 public void Step(float dt){foreach(var p in particles)p.Age+=dt;particles.RemoveAll(p=>p.Age>=p.Life);}
 public IEnumerable<SceneVisual> Visuals(){foreach(var p in particles){var point=p.Start+p.Velocity*p.Age+new Vector2(0,50*p.Age*p.Age);var color=Color.FromArgb((int)(255*(1-p.Age/p.Life)),p.Color);yield return new(new((int)point.X-12,(int)point.Y-12,50,32),(g,o)=>{using var b=new SolidBrush(color);if(p.Text==null)g.FillRectangle(b,point.X+o.X,point.Y+o.Y,5,5);else{using var f=new Font("Consolas",9,FontStyle.Bold);g.DrawString(p.Text,f,b,point.X+o.X,point.Y+o.Y);}});}}
 public void Clear()=>particles.Clear();
}
