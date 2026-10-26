using System.Numerics;
using System.Runtime.InteropServices;
namespace BobbyBoi;
sealed class CharacterPose {public float[]? Previous;}
 static class Character {
 static readonly SpriteCache sprites=new(24*1024*1024);static readonly ModelAsset model=ModelAsset.Load(); record Face(PointF[] Points,Color Color,float Depth); static readonly CharacterPose originalPose=new();
  public static Vector2 Hand(Vector2 origin,float scale,float yaw){var pose=originalPose.Previous;float ra=pose?[1]??0,tilt=pose?[4]??0,bob=pose?[5]??0;float tiltC=MathF.Cos(tilt),tiltS=MathF.Sin(tilt);var v=new Vector3(0,-11.5f,2.35f);v=new(5.5f,v.Y*MathF.Cos(ra)-v.Z*MathF.Sin(ra)+24,v.Y*MathF.Sin(ra)+v.Z*MathF.Cos(ra));v.Y-=16;v=new(v.X*tiltC-v.Y*tiltS,v.X*tiltS+v.Y*tiltC+16+bob,v.Z);v=new(v.X*MathF.Cos(yaw)+v.Z*MathF.Sin(yaw),v.Y,-v.X*MathF.Sin(yaw)+v.Z*MathF.Cos(yaw));return new(origin.X+v.X*scale,origin.Y-v.Y*scale+v.Z*scale*.34f);}
 public static void Draw(Graphics g,Vector2 origin,float scale,Act act,float time,float yaw,float cursor,float fallSpeed=0,string? heldItem=null,bool carryingWindow=false,float hitTint=0,CharacterPose? animationPose=null){var faces=new List<Face>();float swing=MathF.Sin(time*(act==Act.Run?15:8))*(act is Act.Walk or Act.Run or Act.CursorGrab?.85f:.025f);float armSpread=0,legSpread=0;float la=swing,ra=-swing,ll=-swing,rl=swing,tilt=0,bob=0,head=Math.Clamp(cursor/500,-.35f,.35f);
 if(act==Act.Fall){float fear=Math.Clamp(fallSpeed/900,0,1);armSpread=2.5f;legSpread=.22f+fear*.22f;ra=la=-.3f;ll=.2f;rl=-.2f;tilt=MathF.Sin(time*(18+fear*32))*fear*.09f;head+=MathF.Sin(time*35)*fear*.18f;}
 if((heldItem!=null||carryingWindow)&&act is Act.Idle or Act.Walk or Act.Run){ra=-.65f+MathF.Sin(time*8)*.08f;}
 if(act is Act.FishingCast or Act.FishingWait or Act.FishingReel){ll=rl=-1.25f;bob=-6;ra=act==Act.FishingCast?-1.8f:act==Act.FishingWait?-.8f:-1.2f+MathF.Sin(time*13)*.45f;la=-.5f;}
 if(act==Act.CursorGrab){la=ra=-1.4f;ll=swing;rl=-swing;}
 if(act==Act.Climb){ra=-2.5f+MathF.Sin(time*9)*.25f;la=-2.5f-MathF.Sin(time*9)*.25f;ll=MathF.Sin(time*9)*.5f;rl=-ll;}
 if(act==Act.Wave)ra=-2.4f+MathF.Sin(time*10)*.3f;
 if(act==Act.Mine)ra=-1.2f+MathF.Sin(time*13)*.8f;
 if(act==Act.Eat)ra=-1.4f+MathF.Sin(time*6)*.13f;
 if(act==Act.Celebrate){ra=-2.8f;la=2.8f;bob=Math.Abs(MathF.Sin(time*8))*2;}
 if(act==Act.Dance){ra=MathF.Sin(time*8)*1.7f;la=-ra;ll=ra*.4f;rl=-ll;tilt=MathF.Sin(time*5)*.2f;yaw+=time*1.5f;}
 if(act==Act.Dragged){ll=MathF.Sin(time*10)*.3f;rl=-ll;ra=-.4f;la=.4f;}
 if(act==Act.Sit){ll=-1.4f;rl=-1.4f;bob=-7;}
 if(act==Act.Sleep){tilt=1.57f;bob=-8;}
 if(act==Act.Hurt){tilt=MathF.Sin(Math.Min(time,1)*1.57f)*1.4f;bob=-7;}
 if(act==Act.Confused)head=MathF.Sin(time*2)*.5f;
 if(act==Act.Jump){ra=-.8f;la=.8f;ll=.4f;rl=-.4f;}if(act==Act.Land)bob=-MathF.Sin(time/.35f*MathF.PI)*2;
 var poseState=animationPose??originalPose;var previous=poseState.Previous;var pose=new[]{la,ra,ll,rl,tilt,bob,head};if(previous!=null)for(int i=0;i<pose.Length;i++)pose[i]=previous[i]+(pose[i]-previous[i])*.24f;poseState.Previous=pose;la=pose[0];ra=pose[1];ll=pose[2];rl=pose[3];tilt=pose[4];bob=pose[5];head=pose[6];
 float mouth=act switch{Act.Eat=>.55f+Math.Abs(MathF.Sin(time*9))*1.4f,Act.Fall=>.6f+Math.Clamp(fallSpeed/600,0,1)*1.9f,Act.Celebrate or Act.Dance=>1.2f,Act.Confused=>1.7f,Act.Sleep=>.45f,_=>1};
 bool blink=act==Act.Sleep||time%5.2f>5.05f;bool smile=act is Act.Celebrate or Act.Dance;
 string cacheKey=$"{scale}/{heldItem}/{string.Join(',',pose.Select(p=>(int)MathF.Round(p*16)))}/{(int)MathF.Round(yaw*24)}/{armSpread}/{legSpread}/{(int)MathF.Round(mouth*8)}/{blink}/{smile}/{(int)MathF.Round(hitTint*12)}";
 var destination=g;var destinationOrigin=origin;var cached=sprites.Get(cacheKey);if(cached!=null){destination.DrawImageUnscaled(cached,(int)(origin.X-cached.Width/2),(int)(origin.Y-cached.Height/2));return;}
 int radius=(int)MathF.Ceiling(scale*38+12);var sprite=new Bitmap(radius*2,radius*2);using var canvas=Graphics.FromImage(sprite);g=canvas;origin=new(radius,radius);
 float tiltC=MathF.Cos(tilt),tiltS=MathF.Sin(tilt),yawC=MathF.Cos(yaw),yawS=MathF.Sin(yaw),headC=MathF.Cos(head),headS=MathF.Sin(head);
 Vector3 Global(Vector3 v){v.Y-=16;v=new(v.X*tiltC-v.Y*tiltS,v.X*tiltS+v.Y*tiltC+16+bob,v.Z);return new(v.X*yawC+v.Z*yawS,v.Y,-v.X*yawS+v.Z*yawC);}
 PointF Project(Vector3 v)=>new(origin.X+v.X*scale,origin.Y-v.Y*scale+v.Z*scale*.34f);
 foreach(var face in model.Faces.Concat(ItemModels.Get(heldItem))){
  var pv=model.Pivots[face.Part];var pivot=new Vector3(pv[0],pv[1],pv[2]);
  float angle=face.Part switch{"LeftArm"=>la,"RightArm"=>ra,"LeftLeg"=>ll,"RightLeg"=>rl,_=>0};
  float roll=face.Part switch{"LeftArm"=>-armSpread,"RightArm"=>armSpread,"LeftLeg"=>-legSpread,"RightLeg"=>legSpread,_=>0};
  float rollC=MathF.Cos(roll),rollS=MathF.Sin(roll),angleC=MathF.Cos(angle),angleS=MathF.Sin(angle);
  Vector3 Transform(float[] point){var p=new Vector3(point[0],point[1],point[2]);
   if(face.Name.StartsWith("Held ")&&heldItem!=null)p=ItemModels.InHand(heldItem,p,angle);
   if(face.Name=="Mouth"){
    p.Y=25.6f+(p.Y-25.6f)*mouth;p.X*=smile?1.3f:1;
   }
   if((face.Name.StartsWith("Eye")||face.Name.StartsWith("Iris"))&&blink)p.Y=28.15f+(p.Y-28.15f)*.12f;
   p-=pivot;p=new(p.X*rollC-p.Y*rollS,p.X*rollS+p.Y*rollC,p.Z);
   p=new(p.X,p.Y*angleC-p.Z*angleS,p.Y*angleS+p.Z*angleC);
   if(face.Part=="Head")p=new(p.X*headC+p.Z*headS,p.Y,-p.X*headS+p.Z*headC);
   return Global(p+pivot);
  }
  var q=face.Points.Select(Transform).ToArray();var normal=Vector3.Normalize(Vector3.Cross(q[1]-q[0],q[2]-q[0]));
  if(Vector3.Dot(normal,new Vector3(0,.34f,1))<=0)continue;
  float shade=Math.Clamp(.76f+normal.Y*.23f+normal.Z*.15f,.54f,1.06f);var c=Color.FromArgb(Math.Clamp((int)(face.Color[0]*shade),0,255),Math.Clamp((int)(face.Color[1]*shade),0,255),Math.Clamp((int)(face.Color[2]*shade),0,255));
  if(hitTint>0&&!face.Name.StartsWith("Held ")){float tint=Math.Clamp(hitTint*2,0,1)*.65f;c=Color.FromArgb((int)(c.R+(255-c.R)*tint),(int)(c.G*(1-tint)),(int)(c.B*(1-tint)));}
  faces.Add(new(q.Select(Project).ToArray(),c,q.Average(p=>p.Z+p.Y*.34f)));
 } var brushes=new Dictionary<int,SolidBrush>();try{foreach(var face in faces.OrderBy(f=>f.Depth)){int color=face.Color.ToArgb();if(!brushes.TryGetValue(color,out var brush))brushes[color]=brush=new SolidBrush(face.Color);g.FillPolygon(brush,face.Points);}}finally{foreach(var brush in brushes.Values)brush.Dispose();}
 sprites.Add(cacheKey,sprite);destination.DrawImageUnscaled(sprite,(int)(destinationOrigin.X-radius),(int)(destinationOrigin.Y-radius));

 }

}
static class Native {
 [StructLayout(LayoutKind.Sequential)] struct Pt{public int X,Y;public Pt(int x,int y){X=x;Y=y;}}
 [StructLayout(LayoutKind.Sequential)] struct Sz{public int X,Y;public Sz(int x,int y){X=x;Y=y;}}
 [StructLayout(LayoutKind.Sequential,Pack=1)] struct Blend{public byte Op,Flags,Alpha,Format;}
 [StructLayout(LayoutKind.Sequential)] struct Rect{public int Left,Top,Right,Bottom;}
 [DllImport("user32.dll")]static extern nint GetDC(nint h);[DllImport("user32.dll")]static extern int ReleaseDC(nint h,nint dc);[DllImport("gdi32.dll")]static extern nint CreateCompatibleDC(nint dc);[DllImport("gdi32.dll")]static extern nint SelectObject(nint dc,nint obj);[DllImport("gdi32.dll")]static extern bool DeleteObject(nint o);[DllImport("gdi32.dll")]static extern bool DeleteDC(nint dc);
 [DllImport("user32.dll",SetLastError=true)]static extern bool UpdateLayeredWindow(nint h,nint dst,ref Pt pos,ref Sz size,nint src,ref Pt source,int key,ref Blend blend,int flags);
 [DllImport("user32.dll")]static extern bool IsZoomed(nint h);[DllImport("user32.dll")]static extern nint GetForegroundWindow();[DllImport("user32.dll")]static extern bool GetWindowRect(nint h,out Rect r);[DllImport("user32.dll",CharSet=CharSet.Unicode)]static extern int GetClassName(nint h,System.Text.StringBuilder s,int count);
 public static bool Fullscreen(nint self){var h=GetForegroundWindow();if(h==0||h==self||IsZoomed(h))return false;var name=new System.Text.StringBuilder(256);GetClassName(h,name,256);if(name.ToString() is "Progman" or "WorkerW" or "Shell_TrayWnd")return false;if(!GetWindowRect(h,out var r))return false;var b=Screen.FromHandle(h).Bounds;return r.Left<=b.Left&&r.Top<=b.Top&&r.Right>=b.Right&&r.Bottom>=b.Bottom;}
 public static void Present(Form form,Bitmap bmp){nint dc=GetDC(0),mem=CreateCompatibleDC(dc),hb=bmp.GetHbitmap(Color.FromArgb(0)),old=SelectObject(mem,hb);try{var p=new Pt(form.Left,form.Top);var s=new Sz(bmp.Width,bmp.Height);var zero=new Pt();var blend=new Blend{Alpha=255,Format=1};if(!UpdateLayeredWindow(form.Handle,dc,ref p,ref s,mem,ref zero,0,ref blend,2))throw new System.ComponentModel.Win32Exception();}finally{SelectObject(mem,old);DeleteObject(hb);DeleteDC(mem);ReleaseDC(0,dc);}}
}
static class Audio {
 public static void PlayExplosion(int volume)=>WorldAudio.Play("explosion",volume);
 public static void Play(Act act,int volume)=>WorldAudio.Play(act switch{Act.Walk or Act.Run=>"step",Act.Eat=>"eat",Act.Jump=>"jump",Act.Land=>"land",Act.Hurt=>"hurt",Act.Sleep=>"sleep",Act.Wake=>"wake",Act.Dance=>"dance",Act.Mine=>"block",Act.Dragged=>"grab",Act.Celebrate=>"pickup",_=>"chat"},volume);
}
static class SelfTest {
 public static void Run(){ItemModels.Validate();ThrowPhysics.Test();DesktopWorld.Test();ModelAsset.Load().Validate();foreach(Act a in Enum.GetValues<Act>()){using var b=new Bitmap(260,270);using var g=Graphics.FromImage(b);Character.Draw(g,new(130,235),4,a,1,.4f,0);int count=0;for(int x=0;x<b.Width;x++)for(int y=0;y<b.Height;y++)if(b.GetPixel(x,y).A>0)count++;if(count<100)throw new Exception("Empty animation: "+a);}var data=new SaveData();if(data.Items.Count!=8)throw new Exception("Inventory");Directory.CreateDirectory("artifacts");using var image=new Bitmap(260,270);using(var g=Graphics.FromImage(image)){Character.Draw(g,new(130,235),4,Act.Wave,1,.4f,0);}image.Save("artifacts/character.png");using(var sheet=new Bitmap(780,540)){using var graphics=Graphics.FromImage(sheet);int i=0;foreach(var state in new[]{Act.Idle,Act.Wave,Act.Fall,Act.Eat,Act.Celebrate,Act.Sleep}){using var tile=new Bitmap(260,270);using var tg=Graphics.FromImage(tile);for(int n=0;n<24;n++){tg.Clear(Color.Transparent);Character.Draw(tg,new(130,245),5,state,1,.4f,0,state==Act.Fall?950:0);}graphics.DrawImageUnscaled(tile,(i%3)*260,(i/3)*270);i++;}sheet.Save("artifacts/poses.png");}using(var gifts=new Bitmap(1040,540)){using var gg=Graphics.FromImage(gifts);int i=0;foreach(var name in SaveData.Names){using var tile=new Bitmap(260,270);using var tg=Graphics.FromImage(tile);var state=name is "Apple" or "Bread"?Act.Eat:name.Contains("Pickaxe")?Act.Mine:Act.Celebrate;for(int n=0;n<24;n++){tg.Clear(Color.Transparent);Character.Draw(tg,new(130,245),4.5f,state,1,.4f,0,0,name);}gg.DrawImageUnscaled(tile,i%4*260,i/4*270);using var font=new Font("Consolas",10);gg.DrawString(name,font,Brushes.White,i%4*260+50,i/4*270+250);i++;}gifts.Save("artifacts/gifts.png");}ItemModels.Preview();File.WriteAllText("artifacts/self-test.txt","PASS: all "+Enum.GetValues<Act>().Length+" animation states render; inventory has 8 items; throw momentum, stationary release, velocity limits, and frame-independent friction; offset and stacked monitor layout checks; all 8 gift meshes.\n");}
}
