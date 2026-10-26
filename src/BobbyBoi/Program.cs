using System.Drawing.Drawing2D;
using System.Numerics;
using System.Runtime.InteropServices;
using System.Text.Json;
using Microsoft.Win32;

namespace BobbyBoi;
static class Program {
 [STAThread] static void Main(string[] args) {
  ApplicationConfiguration.Initialize();
  if(args.Contains("--jail-test")){try{JailTests.Run();}catch(Exception e){Directory.CreateDirectory("artifacts");File.WriteAllText("artifacts/jail-tests.txt",e.ToString());Environment.ExitCode=1;}return;}
  if(args.Contains("--interruption-test")){using var p=new Pet(true,true);try{p.VerifyInterruptions();}catch(Exception e){Directory.CreateDirectory("artifacts");File.WriteAllText("artifacts/interruption-tests.txt",e.ToString());Environment.ExitCode=1;}return;}
  if(args.Contains("--build-icon")){IconBuilder.Run();return;}
  if(args.Contains("--sfx-test")){SfxTests.Run();return;}
  if(args.Contains("--audio-playback-test")){SfxTests.Playback();return;}
  if(args.Contains("--refresh-icon")){IconRefresh.Notify(Environment.ProcessPath!);return;}
  if(args.Contains("--chaos-test")){try{ChaosTests.Run();}catch{Environment.ExitCode=1;}return;}
  if(args.Contains("--performance-test")){PerformanceTests.Run();return;}
  Application.ThreadException+=(_,e)=>{try{Directory.CreateDirectory(SaveData.Folder);File.WriteAllText(Path.Combine(SaveData.Folder,"runtime-error.txt"),e.Exception.ToString());}catch{}Application.Exit();};
  if(args.Contains("--window-fixture")){WorldFixture.Run();return;}
  if(args.Contains("--chaos-smoke-test")){using var p=new Pet(true,true);p.WorldSmoke(true);return;}
  if(args.Contains("--world-smoke-test")){using var p=new Pet(true,true);p.WorldSmoke();return;}
  if(args.Contains("--climb-smoke-test")){using var p=new Pet(true,true);p.ClimbSmoke();return;}
  if(args.Contains("--world-test")){try{WorldTests.Run();}catch(Exception e){Directory.CreateDirectory("artifacts");File.WriteAllText("artifacts/world-tests.txt",e.ToString());Environment.ExitCode=1;}return;}
  if(args.Length==4&&args[0]=="--restart-helper"){RestartManager.Helper(args);return;}
  if(args.Contains("--restart-test")){RestartManager.Test();return;}
  if(args.Contains("--native-delivery-test")){NativeDeliveryTests.Run();return;}
  if(args.Contains("--menu-outside-test")){using var petTest=new Pet(true,true);petTest.MenuLiveTest(true);return;}
  if(args.Contains("--menu-live-test")){using var petTest=new Pet(true,true);petTest.MenuLiveTest();return;}
  if(args.Contains("--menu-test")){using var testPet=new Pet(true);testPet.VerifyMobMenu();return;}
  if(args.Contains("--mob-test")){MobTests.Run();return;}
  if(args.Contains("--expedition-test")){MemeFactory.Test();return;}
  if(args.Contains("--diagnose")){Diagnostics.Run();return;}
  if(args.Contains("--self-test")){SelfTest.Run();return;}
  string instance="BobbyBoi.DesktopPet."+Diagnostics.Desktop();
  if(args.Contains("--restart-running")){if(EventWaitHandle.TryOpenExisting(instance+".Restart",out var running)){using(running)running.Set();}return;}
  using var restartSignal=new EventWaitHandle(false,EventResetMode.AutoReset,instance+".Restart");
  using var mutex=new Mutex(true,instance,out bool first);
  using var show=new EventWaitHandle(false,EventResetMode.AutoReset,instance+".Show");
  if(!first){show.Set();return;}
  using var pet=new Pet(args.Contains("--smoke-test"),args.Contains("--expedition-smoke-test")||args.Contains("--expedition-stress-test"),args.Contains("--expedition-stress-test"),args.Contains("--mob-smoke-test"));
  pet.Shown+=(_,_)=>pet.ShowPet();
  var registration=ThreadPool.RegisterWaitForSingleObject(show,(_,_)=>{
   try{if(pet.IsHandleCreated&&!pet.IsDisposed)pet.BeginInvoke(pet.ShowPet);}catch(InvalidOperationException){}
  },null,Timeout.Infinite,false);
  var restartRegistration=ThreadPool.RegisterWaitForSingleObject(restartSignal,(_,_)=>{try{if(pet.IsHandleCreated&&!pet.IsDisposed)pet.BeginInvoke(pet.RestartBobbyBoi);}catch(InvalidOperationException){}},null,Timeout.Infinite,false);
  try{Application.Run(pet);}finally{registration.Unregister(null);restartRegistration.Unregister(null);}
 }
}
sealed class SaveData {
 public bool VisibilityMigrated{get;set;} public WorldState World{get;set;}=new();public bool NaturalMobs{get;set;}=true; public float Scale{get;set;}=1; public float Speed{get;set;}=1; public int Volume{get;set;}=12;
 public ExpeditionOptions Expeditions{get;set;}=new(); public bool Chaos{get;set;} public bool Bubbles{get;set;}=true; public bool OnTop{get;set;}=true; public bool Fullscreen{get;set;}=false;
 public Dictionary<string,int> Items{get;set;}=Names.ToDictionary(x=>x,x=>x=="Bread"?3:0);
 public static readonly string[] Names=["Diamond","Emerald","Dirt","Cobblestone","Wooden Pickaxe","Diamond Sword","Bread","Apple"];
 public static string Folder=>Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),"BobbyBoi");
 public static SaveData Load(){try{var d=JsonSerializer.Deserialize<SaveData>(File.ReadAllText(Path.Combine(Folder,"settings.json")))??new(); d.Scale=Math.Clamp(d.Scale,.6f,2.5f);d.Speed=Math.Clamp(d.Speed,.3f,3);foreach(var n in Names)d.Items.TryAdd(n,0);d.Expeditions??=new();d.Expeditions.Normalize();d.World??=new();d.World.Normalize();if(!d.VisibilityMigrated){d.Fullscreen=false;d.OnTop=true;d.VisibilityMigrated=true;}return d;}catch{return new();}}
 public void Save(){try{Directory.CreateDirectory(Folder); var p=Path.Combine(Folder,"settings.json");File.WriteAllText(p+".tmp",JsonSerializer.Serialize(this,new JsonSerializerOptions{WriteIndented=true}));File.Move(p+".tmp",p,true);}catch(Exception e) when(e is IOException or UnauthorizedAccessException){System.Diagnostics.Trace.WriteLine("Could not save BobbyBoi settings: "+e.Message);}}
}
enum Act { Idle,Walk,Run,Jump,Fall,Land,Sit,Sleep,Wake,Wave,Mine,Eat,Dance,Hurt,Celebrate,Dragged,Confused,FishingCast,FishingWait,FishingReel,Climb,CursorGrab }
sealed partial class Pet:Form {
 readonly bool smoke; readonly SaveData data=SaveData.Load(); readonly Random rng=new(); readonly System.Windows.Forms.Timer timer=new(){Interval=16}; readonly System.Diagnostics.Stopwatch watch=System.Diagnostics.Stopwatch.StartNew();
 readonly Voicelines quips=new();readonly MinecraftBubble chatBubble=new();double lastRender=-1;readonly NotifyIcon tray; readonly PetMenu menu=new(); Bitmap? frame; Act action=Act.Idle; float age, duration=4, x,y,vy,dir=1,yaw=.4f; double last; float hunger=75,energy=90,happy=80,curiosity=70,mischief=40,bubbleTime,speechCooldown=15,soundCooldown; string bubble="hello, world"; double revealUntil; bool userHidden; bool dragging,paused,follow,hiddenFullscreen; Point grab,mouseDown; bool pressed; string? heldItem; double heldUntil; float exploreClock=18,travelTime=-1; Vector2 travelFrom,travelTo; float throwVx; Vector2 dragVelocity; Point previousDragPoint; double previousDragTime; float moveDistance; int ticks,renderedFrames; readonly List<(float x,float y,float life)> blocks=[];
 public Pet(bool smoke=false,bool expeditionTest=false,bool expeditionStress=false,bool mobTest=false){this.mobTest=mobTest;if(mobTest){data.NaturalMobs=true;data.Expeditions.Enabled=false;data.Fullscreen=false;data.Volume=0;}this.smoke=smoke;this.expeditionTest=expeditionTest;this.expeditionStress=expeditionStress;if(expeditionTest){data.Expeditions=new();data.Fullscreen=false;}Text="BobbyBoi";FormBorderStyle=FormBorderStyle.None;ShowInTaskbar=smoke||expeditionTest||mobTest;TopMost=data.OnTop;StartPosition=FormStartPosition.Manual;Width=260;Height=270;
 var area=Screen.PrimaryScreen!.WorkingArea;x=area.Right-260;y=area.Bottom-Height;Location=new((int)x,(int)y);BuildMenu();tray=new NotifyIcon{Text="BobbyBoi - right-click for snacks",Icon=Icon.ExtractAssociatedIcon(Environment.ProcessPath!) ?? SystemIcons.Application,Visible=true,ContextMenuStrip=menu};tray.DoubleClick+=(_,_)=>ShowPet();timer.Tick+=Tick;timer.Start();FormClosed+=(_,_)=>{timer.Stop();tray.Dispose();menu.Dispose();frame?.Dispose();chatBubble.Dispose();if(!expeditionTest&&!mobTest)data.Save();expeditionStop.Cancel();generationStop?.Cancel();deliveryCard?.Dispose();nativeDelivery?.Dispose();mobs.Dispose();worldCanvas.Dispose();WorldAudio.Stop();houseSprite?.Dispose();objectMenu?.Dispose();}; MouseDown+=Down;MouseMove+=MovePet;MouseUp+=Up;MouseDoubleClick+=(_,e)=>{if(e.Button==MouseButtons.Left)Jump();};InitWorld(); }
 protected override CreateParams CreateParams {get{var p=base.CreateParams;p.ExStyle|=0x80000;if(!smoke)p.ExStyle|=0x80|0x08000000;return p;}}
 protected override bool ShowWithoutActivation=>true;
  public void ShowPet(){
  if(expedition.Busy){nativeDelivery?.Dispose();nativeDelivery=null;generationStop?.Cancel();expedition.Cancel();cargo=null;generation=null;}paused=false;hiddenFullscreen=false;userHidden=false;revealUntil=watch.Elapsed.TotalSeconds+10;
  var area=Screen.FromPoint(Cursor.Position).WorkingArea;
  x=area.Left+(area.Width-Width)/2;y=area.Bottom-Height;vy=0;
  Show();Render();Say(quips.Next("greet"));Set(Act.Wave,3);
 }
 void Set(Act a,float seconds=4){if(expedition.Busy&&a is Act.Sleep or Act.Dance)InterruptExpedition();bool changed=action!=a;action=a;age=0;duration=seconds;if(a==Act.Sleep)Say(quips.Next("sleep"));if(a==Act.Mine)Say(quips.Next("mine"));if(changed&&a is Act.Eat or Act.Celebrate or Act.Jump or Act.Hurt or Act.Mine or Act.Sleep or Act.Dance or Act.Wake or Act.Dragged)Tone(a);}
 void Say(string s){if(data.Bubbles){bubble=s;bubbleTime=3.5f;Sfx("chat");}}
 void Tone(Act a){if(data.Volume==0||soundCooldown>0||expedition.Busy&&!data.Expeditions.Sounds)return;soundCooldown=.25f;Audio.Play(a,data.Volume);}
 void FaceMotion(float horizontalVelocity){yaw=ThrowPhysics.Facing(horizontalVelocity,yaw);}
 void Jump(){vy=-250;Set(Act.Jump,1);}
 void Down(object? s,MouseEventArgs e){if(e.Button==MouseButtons.Right){menu.Show(Cursor.Position);return;}if(e.Button!=MouseButtons.Left)return;InterruptActivity();onHouseRoof=false;standingPlatform=null;pressed=true;mouseDown=Cursor.Position;grab=e.Location;previousDragPoint=mouseDown;previousDragTime=watch.Elapsed.TotalSeconds;dragVelocity=Vector2.Zero;throwVx=0;travelTime=-1;Capture=true;Say(quips.Next("grab"));Set(Act.Wave,1.8f);}
 void MovePet(object? s,MouseEventArgs e){if(!pressed)return;var p=Cursor.Position;if(!dragging&&Math.Abs(p.X-mouseDown.X)+Math.Abs(p.Y-mouseDown.Y)>5){dragging=true;Set(Act.Dragged,999);}if(dragging){double now=watch.Elapsed.TotalSeconds;float elapsed=(float)(now-previousDragTime);if(elapsed>.001f){var sample=new Vector2(p.X-previousDragPoint.X,p.Y-previousDragPoint.Y)/elapsed;dragVelocity=Vector2.Lerp(dragVelocity,Vector2.Clamp(sample,new(-1400,-1100),new(1400,1100)),.6f);previousDragTime=now;previousDragPoint=p;}x=p.X-grab.X;y=p.Y-grab.Y;vy=0;}}
 void Up(object? s,MouseEventArgs e){if(e.Button!=MouseButtons.Left)return;pressed=false;Capture=false;if(dragging){dragging=false;var release=ThrowPhysics.Release(dragVelocity,(float)(watch.Elapsed.TotalSeconds-previousDragTime));throwVx=release.X;vy=release.Y;FaceMotion(throwVx);Set(Act.Fall,10);Sfx("throw");}}
 void BuildMenu(){menu.ClickSound=()=>Sfx("menu");menu.Items.Add(new ToolStripLabel("BOBBYBOI"){Font=new Font("Segoe UI",11,FontStyle.Bold),ForeColor=Color.FromArgb(229,179,88),Padding=new Padding(12,5,12,0)});menu.Items.Add(new ToolStripLabel("Your desktop companion"){Font=new Font("Segoe UI",8.5f),ForeColor=Color.FromArgb(154,172,143),Padding=new Padding(12,0,12,5)});menu.Items.Add(new ToolStripSeparator());void Add(string n,Action a){menu.Items.Add(n,null,(_,_)=>a());}
 Add("Feed BobbyBoi",()=>Use("Bread"));Add("Give Diamond",()=>Use("Diamond"));Add("Make Him Dance",()=>{InterruptActivity();Set(Act.Dance,9);});Add("Sleep / Wake",()=>{bool wake=action==Act.Sleep;if(!wake)InterruptActivity();Set(wake?Act.Wake:Act.Sleep,wake?2:35);});menu.Items.Add(new ToolStripSeparator());Add("Follow Cursor",()=>{follow=!follow;Say(follow?"lead the way":"bruh");});Add("Explore",()=>{follow=false;exploreClock=0;Set(Act.Walk,12);});Add("Toggle Chaos Mode",()=>{data.Chaos=!data.Chaos;Say(data.Chaos?"chaos time":"behaving. probably");data.Save();});
 menu.Items.Add(new ToolStripSeparator());var inv=new ToolStripMenuItem("Inventory");foreach(var n in SaveData.Names){var item=new ToolStripMenuItem(n);item.Click+=(_,_)=>Use(n);inv.DropDownItems.Add(item);}menu.Items.Add(inv);menu.Opening+=(_,_)=>{for(int i=0;i<SaveData.Names.Length;i++) inv.DropDownItems[i].Text=$"{SaveData.Names[i]}  x{data.Items[SaveData.Names[i]]}";};WorldMenu();MobMenu();Add("Send on Meme Expedition",()=>{data.Expeditions.Enabled=true;expedition.Request();data.Save();});Add("Open BobbyBoi Memes Folder",OpenMemes);Add("Settings",Settings);Add("Pause / Resume",()=>paused=!paused);Add("Hide / Show",()=>{userHidden=!userHidden;if(userHidden)Hide();else ShowPet();});menu.Items.Add(new ToolStripSeparator());Add("Restart BobbyBoi",RestartBobbyBoi);Add("Exit BobbyBoi",Close);menu.StyleChildren();}
 void Use(string item){InterruptActivity();heldItem=item;heldUntil=watch.Elapsed.TotalSeconds+8;data.Items[item]++;if(item is "Bread" or "Apple"){hunger=Math.Min(100,hunger+28);Set(Act.Eat,4);Say(quips.Next("food"));}else if(item is "Diamond" or "Emerald"){happy=Math.Min(100,happy+20);Set(Act.Celebrate,5);Say(quips.Next("gem"));}else if(item.Contains("Sword")){Set(Act.Dance,5);Say("zombie where");}else {Set(Act.Mine,6);Say(item=="Dirt"?"my dream house":"where's my pickaxe");}data.Save();}
 void Tick(object? s,EventArgs e){if(IsDisposed)return;double now=watch.Elapsed.TotalSeconds;float elapsedDt=(float)Math.Max(0,now-last);float dt=Math.Min(.05f,elapsedDt);last=now;if(++ticks==1||ticks==90){try{Directory.CreateDirectory(SaveData.Folder);File.WriteAllText(Path.Combine(SaveData.Folder,"launch.log"),$"{DateTimeOffset.Now:O}\nDesktop: {Diagnostics.Desktop()}\nVisible: {Visible}\nLocation: {Location}\nSize: {Size}\nFrames: {ticks}\nFullscreen suspension: {hiddenFullscreen}\n");}catch(IOException){}catch(UnauthorizedAccessException){}}if(ticks%60==0){bool full=now>revealUntil&&!smoke&&data.Fullscreen&&Native.Fullscreen(Handle);if(full&&!hiddenFullscreen){hiddenFullscreen=true;Hide();}else if(!full&&hiddenFullscreen){hiddenFullscreen=false;if(!userHidden)Show();}}if(paused||hiddenFullscreen||userHidden||!Visible){SuspendWorld();mobs.Suspend();nativeDelivery?.Suspend();if(deliveryCard is {IsDisposed:false})deliveryCard.Suspend();return;}if(!interruptedExpedition)nativeDelivery?.Resume();if(!interruptedExpedition&&deliveryCard is {IsDisposed:false})deliveryCard.Resume();
 if(menu.Visible||objectMenu is {Visible:true}){UpdateMobs(Math.Min(.5f,elapsedDt));Render();return;}
 if(expeditionTest){ExpeditionTrace();if(IsDisposed)return;}
 age+=dt;bubbleTime-=dt;soundCooldown-=dt;speechCooldown-=dt;UpdatePetSfx(dt);hunger=Math.Max(0,hunger-dt*.12f);energy=Math.Clamp(energy+dt*(action==Act.Sleep?2:-.16f),0,100);happy=Math.Max(0,happy-dt*.04f);curiosity=Math.Clamp(curiosity+dt*.05f,0,100);mischief=Math.Clamp(mischief+dt*.03f,0,100);
 UpdateMobs(Math.Min(.5f,elapsedDt));if(UpdateMobReactions(Math.Min(.5f,elapsedDt))){WorldPassive(Math.Min(.5f,elapsedDt));return;}if(mobTest){MobTrace();if(IsDisposed)return;}
 if(UpdateWorld(dt*worldSpeed))return;
 if(UpdateExpedition(dt*(expeditionStress?60:1)))return;
 var areas=Screen.AllScreens.Select(s=>s.WorkingArea).ToArray();var area=DesktopWorld.Select(areas,x+Width/2,y+Height);bool walking=action is Act.Walk or Act.Run || follow;float floor=WorldFloor(areas,x+Width/2,y+Height,walking&&Math.Abs(throwVx)<20)-Height;
 if(!dragging){
 exploreClock-=dt;
 if(travelTime>=0){travelTime+=dt;float t=Math.Clamp(travelTime/2.4f,0,1);float ease=t*t*(3-2*t);FaceMotion(travelTo.X-travelFrom.X);var p=Vector2.Lerp(travelFrom,travelTo,ease);x=p.X;y=p.Y-MathF.Sin(t*MathF.PI)*100;vy=0;throwVx=0;Set(Act.Jump,3);if(t>=1){travelTime=-1;Set(Act.Land,.4f);}Location=new((int)x,(int)y);Render();return;}
 if(exploreClock<0&&!follow&&action!=Act.Sleep&&areas.Length>1&&Math.Abs(throwVx)<20){exploreClock=rng.Next(25,45);var destinations=areas.Where(r=>r!=area).ToArray();var target=destinations[rng.Next(destinations.Length)];float targetX=target.Left+target.Width*.5f-Width/2;bool sideBySide=(target.Right<=area.Left+2||target.Left>=area.Right-2)&&target.Bottom>area.Top&&area.Bottom>target.Top;if(sideBySide){dir=targetX>x?1:-1;Set(Act.Walk,Math.Abs(targetX-x)/(35*data.Speed)+2);}else{travelFrom=new(x,y);travelTo=new(targetX,target.Bottom-Height);travelTime=0;Say("new biome");}}
 float walkSpeed=0;x+=throwVx*dt;throwVx=ThrowPhysics.Damp(throwVx,dt,y>=floor&&vy>=0);if(action is Act.Walk or Act.Run || follow&&action!=Act.Sleep){float speed=(action==Act.Run?95:35)*data.Speed;if(follow){dir=Cursor.Position.X>x+Width/2?1:-1;if(Math.Abs(Cursor.Position.X-x-Width/2)<25)speed=0;}walkSpeed=speed;x+=dir*speed*dt;moveDistance+=speed*dt;if(moveDistance>30){moveDistance=0;Tone(Act.Walk);}}
 floor=WorldFloor(areas,x+Width/2,y+Height,walking&&Math.Abs(throwVx)<20)-Height;if(walking&&Math.Abs(throwVx)<20&&vy==0)y=floor;
 if(y>floor)y=floor;
 if(y<floor||vy<0){vy+=650*dt;y+=vy*dt;if(vy>60&&action!=Act.Fall)Set(Act.Fall,20);if(ThrowPhysics.SettleAtFloor(ref y,ref vy,floor)){Set(Act.Land,.35f);Tone(Act.Land);}}else{bool wasFalling=vy!=0||action is Act.Fall or Act.Jump;ThrowPhysics.SettleAtFloor(ref y,ref vy,floor);if(wasFalling)Set(Act.Land,.35f);}
 float left=areas.Min(r=>r.Left)-65,right=areas.Max(r=>r.Right)-Width+65;if(x<left){x=left;dir=1;throwVx=Math.Abs(throwVx)*.5f;}if(x>right){x=right;dir=-1;throwVx=-Math.Abs(throwVx)*.5f;}FaceMotion(throwVx+dir*walkSpeed);
 if(age>duration){if(energy<20)Set(Act.Sleep,25);else if(hunger<20){Set(Act.Confused,4);Say("bread pls");}else{var choices=new[]{Act.Idle,Act.Idle,Act.Walk,Act.Walk,Act.Sit,Act.Wave,Act.Confused,Act.Mine,Act.Dance,Act.Run};var a=choices[rng.Next(choices.Length)];if(data.Chaos&&rng.Next(3)==0){a=Act.Hurt;blocks.Add((130,222,7));Say("crafting failed");}Set(a,rng.Next(3,10));if(a==Act.Walk||a==Act.Run)dir=rng.Next(2)==0?-1:1;if(rng.Next(12)==0)Jump();}}
 }
 if(speechCooldown<0){speechCooldown=rng.Next(20,45);Say(quips.Next("idle"));}
 for(int i=blocks.Count-1;i>=0;i--){var b=blocks[i];b.life-=dt;if(b.life<=0)blocks.RemoveAt(i);else blocks[i]=b;}
 Location=new((int)x,(int)y);Render();if(smoke&&!expeditionTest&&!mobTest&&now>30){Directory.CreateDirectory("artifacts");File.WriteAllText("artifacts/smoke-test.txt",$"PASS: visible={Visible}; layered overlay rendered {renderedFrames} frames in {now:0.0}s; responsive timer, tray initialized, alpha hit testing active. Working set: {Environment.WorkingSet/1048576} MB.");frame?.Save("artifacts/live-frame.png");Close();}}
 void Render(){double renderNow=watch.Elapsed.TotalSeconds;if(!dragging&&renderNow-lastRender<1d/(data.World.PerformanceMode?20:30))return;lastRender=renderNow;var b=frame??=new Bitmap(Width,Height,System.Drawing.Imaging.PixelFormat.Format32bppArgb);using(var g=Graphics.FromImage(b)){g.Clear(Color.Transparent);g.SmoothingMode=SmoothingMode.None;foreach(var block in blocks){g.FillRectangle(Brushes.SaddleBrown,block.x,block.y,20,20);g.FillRectangle(Brushes.OliveDrab,block.x,block.y,20,5);}if(!actorInside)Character.Draw(g,new Vector2(Width/2,Height-10)+WorldShake,4*data.Scale*characterWorldScale,action,age,yaw,Cursor.Position.X-(x+Width/2),Math.Max(0,vy),watch.Elapsed.TotalSeconds<heldUntil?heldItem:null,expedition.State==ExpeditionState.Returning||expedition.State==ExpeditionState.Delivering&&!deliveredCard,damageTint/.32f);if(healthShown>0||respawnTime>0){for(int i=0;i<10;i++)g.FillRectangle(health>i*2?Brushes.IndianRed:Brushes.DimGray,Width/2-50+i*10,105,8,5);}if(bubbleTime>0&&data.Bubbles)chatBubble.Draw(g,bubble,3.5f-bubbleTime,Width,SpeechAccent());}
 Native.Present(this,b);renderedFrames++;}
 protected override void WndProc(ref Message m){if(m.Msg==0x84){var p=PointToClient(new Point((short)(m.LParam.ToInt64()&65535),(short)(m.LParam.ToInt64()>>16)));m.Result=frame!=null&&p.X>=0&&p.Y>=0&&p.X<Width&&p.Y<Height&&frame.GetPixel(p.X,p.Y).A>15?(nint)1:(nint)(-1);return;}base.WndProc(ref m);}
 void Settings(){var f=new Form{Text="BobbyBoi - Settings",Size=new Size(390,620),StartPosition=FormStartPosition.CenterScreen,BackColor=Color.FromArgb(34,43,36),ForeColor=Color.Beige,FormBorderStyle=FormBorderStyle.FixedDialog,MaximizeBox=false};var panel=new FlowLayoutPanel{Dock=DockStyle.Fill,FlowDirection=FlowDirection.TopDown,WrapContents=false,Padding=new Padding(18),AutoScroll=true};f.Controls.Add(panel);
 void Slider(string title,int min,int max,int value,Action<int> apply){var label=new Label{Text=title,Width=330};panel.Controls.Add(label);var t=new TrackBar{Minimum=min,Maximum=max,Value=Math.Clamp(value,min,max),Width=320,TickStyle=TickStyle.None};t.ValueChanged+=(_,_)=>apply(t.Value);panel.Controls.Add(t);}
 void Check(string text,bool value,Action<bool> apply){var c=new CheckBox{Text=text,Checked=value,Width=330,Height=27};c.CheckedChanged+=(_,_)=>apply(c.Checked);panel.Controls.Add(c);}
 Slider("Character scale",60,180,(int)(data.Scale*100),v=>data.Scale=v/100f);Slider("Movement speed",30,250,(int)(data.Speed*100),v=>data.Speed=v/100f);Slider("Sound volume (0 = mute)",0,100,data.Volume,v=>{data.Volume=v;if(v==0)WorldAudio.Silence();});Check("Natural zombie / creeper groups (60s)",data.NaturalMobs,v=>{data.NaturalMobs=v;data.Save();});Check("Chaos mode",data.Chaos,v=>data.Chaos=v);Check("Speech bubbles",data.Bubbles,v=>data.Bubbles=v);Check("Always on top",data.OnTop,v=>{data.OnTop=v;TopMost=v;});Check("Pause during fullscreen apps",data.Fullscreen,v=>data.Fullscreen=v);
 using(var key=Registry.CurrentUser.OpenSubKey(@"Software\Microsoft\Windows\CurrentVersion\Run")){Check("Start with Windows",key?.GetValue("BobbyBoi")!=null,v=>{using var k=Registry.CurrentUser.CreateSubKey(@"Software\Microsoft\Windows\CurrentVersion\Run");if(v)k.SetValue("BobbyBoi","\""+Environment.ProcessPath+"\"");else k.DeleteValue("BobbyBoi",false);});}
 void Button(string text,Action a){var btn=new Button{Text=text,Width=320,Height=30,FlatStyle=FlatStyle.Flat};btn.Click+=(_,_)=>a();panel.Controls.Add(btn);}Button("Reset position",()=>{var r=Screen.PrimaryScreen!.WorkingArea;x=r.Right-260;y=r.Bottom-Height;vy=0;});Button("Reset inventory",()=>{foreach(var n in SaveData.Names)data.Items[n]=0;});panel.Controls.Add(new Label{Text=$"Hunger {hunger:0}   Energy {energy:0}   Joy {happy:0}\nCuriosity {curiosity:0}   Mischief {mischief:0}",Width=330,Height=42});ExpeditionSettings(panel);WorldSettings(panel);f.FormClosed+=(_,_)=>data.Save();f.Show();}
}
