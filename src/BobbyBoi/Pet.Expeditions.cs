using System.Diagnostics;
using System.Numerics;
namespace BobbyBoi;
sealed partial class Pet {
  readonly bool expeditionTest,expeditionStress;CancellationTokenSource? generationStop;ExpeditionState? tracedState;readonly List<string> expeditionTrace=[];
 void ExpeditionTrace(){
  if(tracedState!=expedition.State){tracedState=expedition.State;expeditionTrace.Add($"{watch.Elapsed.TotalSeconds:0.00}s {expedition.State} x={x:0} y={y:0} remaining={expedition.Remaining:0.00}");}
  if(expedition.Delivered>=(expeditionStress?12:1)&&expedition.State==ExpeditionState.Idle&&expedition.Age>=4){Directory.CreateDirectory("artifacts");File.WriteAllLines(expeditionStress?"artifacts/expedition-stress.txt":"artifacts/expedition-live.txt",expeditionTrace.Append($"Delivered={expedition.Delivered}; card={deliveryCard is {IsDisposed:false}}; files={Directory.GetFiles(expeditionStress?"artifacts/expedition-stress-output":"artifacts/expedition-live-output").Length}; visible={Visible}; workingSetMB={Environment.WorkingSet/1048576}"));frame?.Save("artifacts/expedition-live-pet.png");if(deliveryCard is {IsDisposed:false}){using var picture=new Bitmap(deliveryCard.Width,deliveryCard.Height);deliveryCard.DrawToBitmap(picture,new Rectangle(Point.Empty,picture.Size));picture.Save("artifacts/expedition-card.png");}Close();}
 }
 readonly ExpeditionClock expedition=new(); readonly CancellationTokenSource expeditionStop=new();
 NativeDelivery? nativeDelivery; Task<MemeDelivery>? generation; MemeDelivery? cargo; DeliveryCard? deliveryCard;
 Rectangle expeditionArea; float expeditionOffX,expeditionStopX,expeditionDirection; bool deliveredCard;
 static readonly string[] departures=["brb","important business","the mines are calling","don't follow me","i forgot something","going to ohio"];
 static readonly string[] arrivals=["found this in the mines","don't ask where i got this","stole this from steve","i made this with my feet","took me 7 business days","the villagers are after me","i am officially unemployed","this cost 64 emeralds","my lawyer said not to explain","i committed art","this is worth millions","bro i found evidence","i cooked"];

 void BeginExpedition(){nativeDelivery?.Dispose();nativeDelivery=null;generationStop?.Dispose();generationStop=CancellationTokenSource.CreateLinkedTokenSource(expeditionStop.Token);
  var screens=Screen.AllScreens;float left=screens.Min(s=>s.Bounds.Left),right=screens.Max(s=>s.Bounds.Right);
  expeditionDirection=x+Width/2-left<right-x-Width/2?-1:1;
  var candidates=screens.Where(s=>expeditionDirection<0?s.Bounds.Left==left:s.Bounds.Right==right).ToArray();
  expeditionArea=candidates.OrderBy(s=>Math.Abs(s.WorkingArea.Bottom-(y+Height))).First().WorkingArea;
  expeditionOffX=expeditionDirection<0?left-Width-16:right+16;
  float stopMargin=Math.Min(data.Expeditions.ExternalWindows?720:360,Math.Max(35,(expeditionArea.Width-Width)/2));expeditionStopX=expeditionDirection<0?expeditionArea.Left+stopMargin:expeditionArea.Right-Width-stopMargin;
  throwVx=vy=0;travelTime=-1;follow=false;cargo=null;generation=null;deliveredCard=false;heldItem=null;
  Say(departures[rng.Next(departures.Length)]);Set(Act.Confused,2);
 }
 void CancelExpedition(){interruptedExpedition=false;if(expedition.Busy){if(!deliveredCard){deliveryCard?.Dispose();deliveryCard=null;nativeDelivery?.Dispose();nativeDelivery=null;}generationStop?.Cancel();expedition.Cancel();cargo=null;generation=null;ShowPet();}}
 bool UpdateExpedition(float dt){
  data.Expeditions.Normalize();
  if(interruptedExpedition){if(!data.Expeditions.Enabled)CancelExpedition();return false;}
  if(!data.Expeditions.Enabled){CancelExpedition();return false;}
  bool critical=!expeditionTest&&(world.Busy||dragging||pressed||menu.Visible||action is Act.Sleep or Act.Dragged or Act.Eat or Act.Celebrate or Act.Dance||travelTime>=0||Math.Abs(throwVx)>20||vy!=0||mobs.NearestThreat(new Vector2(x+Width/2,y+Height),300)!=null);

  if(expedition.Tick(dt,!critical||expedition.Busy&&!dragging&&!pressed&&!menu.Visible,data.Expeditions))BeginExpedition();
  if(!expedition.Busy)return false;
  if(dragging||pressed||menu.Visible)return true;
  // If a monitor was unplugged, return safely instead of remaining outside the desktop.
  if(!Screen.AllScreens.Any(s=>s.WorkingArea==expeditionArea)){CancelExpedition();return false;}
  switch(expedition.State){
   case ExpeditionState.Preparing: yaw=expedition.Age<.9?-.8f:.8f;break;
   case ExpeditionState.Exiting:
    var gait=expedition.Age<.5?Act.Walk:Act.Run;if(action!=gait)Set(gait,999);
    FaceMotion(expeditionDirection);x+=expeditionDirection*(100+Math.Min(1,(float)expedition.Age/.6f)*140)*dt;
    y=DesktopWorld.Floor(Screen.AllScreens.Select(s=>s.WorkingArea).ToArray(),x+Width/2,y+Height,true)-Height;
    if(expeditionDirection<0?x<=expeditionOffX:x>=expeditionOffX){x=expeditionOffX;y=expeditionArea.Bottom-Height;expedition.Go(ExpeditionState.Offscreen);var token=generationStop!.Token;generation=Task.Run(()=>MemeFactory.Create(token,expeditionTest?Path.GetFullPath(expeditionStress?"artifacts/expedition-stress-output":"artifacts/expedition-live-output"):null),token);}break;
   case ExpeditionState.Offscreen:
    if(generation?.IsCompleted==true){
     try{cargo=generation.GetAwaiter().GetResult();}catch(Exception){Say("the printer exploded");cargo=null;}
     generation=null;
     if(cargo!=null&&data.Expeditions.ExternalWindows&&!expeditionTest){try{nativeDelivery?.Dispose();nativeDelivery=new NativeDelivery(cargo,expeditionArea);}catch{Say("could not open the app");}}
     else if(cargo!=null){deliveryCard?.Dispose();deliveryCard=new DeliveryCard(cargo,data.Expeditions.Previews){TopMost=data.OnTop};}
    }
    nativeDelivery?.Carry(new Point((int)x+Width/2,(int)y+Height-65),-expeditionDirection);
    if(expedition.Age>=5&&generation==null&&(nativeDelivery==null||nativeDelivery.Ready||nativeDelivery.TimedOut)){
     expedition.Go(ExpeditionState.Returning);Set(Act.Walk,999);
    }break;   case ExpeditionState.Returning:
    FaceMotion(-expeditionDirection);x-=expeditionDirection*130*dt;y=expeditionArea.Bottom-Height;
    deliveryCard?.Carry(new Point((int)x+Width/2,(int)y+Height-65),-expeditionDirection);nativeDelivery?.Carry(new Point((int)x+Width/2,(int)y+Height-65),-expeditionDirection);
    // Brief hesitation under the weight of the masterpiece.
    if(expedition.Age>.9&&expedition.Age<1.1)yaw+=MathF.Sin((float)expedition.Age*35)*.18f;
    if(expeditionDirection<0?x>=expeditionStopX:x<=expeditionStopX){x=expeditionStopX;expedition.Go(ExpeditionState.Delivering);Set(Act.Celebrate,1.5f);}break;
   case ExpeditionState.Delivering:
    if(!deliveredCard)nativeDelivery?.Carry(new Point((int)x+Width/2,(int)y+Height-65),-expeditionDirection);if(!deliveredCard)deliveryCard?.Carry(new Point((int)x+Width/2,(int)y+Height-65-(int)(Math.Sin(Math.Min(1,expedition.Age/1.4)*Math.PI)*28)),-expeditionDirection);
    if(expedition.Age>1.4&&action!=Act.Wave)Set(Act.Wave,2);
    if(expedition.Age>=1.8&&!deliveredCard){deliveredCard=true;if(cargo!=null){nativeDelivery?.Drop(expeditionArea);deliveryCard?.Place(new Point((int)x+Width/2,(int)y+Height),expeditionArea);Say(cargo.Reaction??arrivals[rng.Next(arrivals.Length)]);if(nativeDelivery==null&&data.Expeditions.AutoPaint&&cargo.Path.EndsWith(".png",StringComparison.OrdinalIgnoreCase))OpenPaint(cargo.Path);}else Say("i forgot why i left");}
    if(expedition.Age>=3.8){if(cargo!=null){CountWorld("expeditions");if(data.World.Counters["expeditions"]>=10)Award("expeditions","Unemployed and Dangerous");if(cargo.Title.Contains("bank",StringComparison.OrdinalIgnoreCase))Award("financial","Financial Disaster");}expedition.Finish(data.Expeditions,cargo!=null);cargo=null;Set(Act.Idle,4);exploreClock=20;}break;
  }
  Location=new((int)x,(int)y);Render();return true;
 }
 void OpenMemes(){try{Directory.CreateDirectory(MemeFactory.OutputFolder);Process.Start(new ProcessStartInfo(MemeFactory.OutputFolder){UseShellExecute=true});}catch(Exception e){Say("folder unavailable");tray.ShowBalloonTip(3000,"BobbyBoi",e.Message,ToolTipIcon.Warning);}}
 static void OpenPaint(string path){try{var p=new ProcessStartInfo("mspaint.exe"){UseShellExecute=true};p.ArgumentList.Add(path);Process.Start(p);}catch{}}
 void ExpeditionSettings(FlowLayoutPanel panel){
  panel.Controls.Add(new Label{Text="MEME EXPEDITIONS",ForeColor=Color.Goldenrod,Font=new Font("Segoe UI",10,FontStyle.Bold),Width=325,Height=32});
  void Check(string title,bool v,Action<bool> set){var c=new CheckBox{Text=title,Checked=v,Width=325,Height=28};c.CheckedChanged+=(_,_)=>{set(c.Checked);data.Save();};panel.Controls.Add(c);}
  Check("Enable Meme Expeditions",data.Expeditions.Enabled,v=>{data.Expeditions.Enabled=v;if(!v)CancelExpedition();});
  Check("Use real Paint / Notepad windows",data.Expeditions.ExternalWindows,v=>data.Expeditions.ExternalWindows=v);
  Check("Expedition sounds",data.Expeditions.Sounds,v=>data.Expeditions.Sounds=v);
  Check("Open PNGs in Paint with built-in deliveries",data.Expeditions.AutoPaint,v=>data.Expeditions.AutoPaint=v);
  Check("Show contents while carrying",data.Expeditions.Previews,v=>data.Expeditions.Previews=v);
  void Number(string title,int value,int min,int max,Action<int> set){panel.Controls.Add(new Label{Text=title,Width=325});var n=new NumericUpDown{Value=Math.Clamp(value,min,max),Minimum=min,Maximum=max,Width=310};n.ValueChanged+=(_,_)=>{set((int)n.Value);data.Expeditions.Normalize();data.Save();};panel.Controls.Add(n);}
  Number("Minimum interval after delivery (seconds)",data.Expeditions.MinSeconds,30,3600,v=>data.Expeditions.MinSeconds=v);
  Number("Maximum interval after delivery (seconds)",data.Expeditions.MaxSeconds,30,3600,v=>data.Expeditions.MaxSeconds=v);
  Number("Maximum deliveries this session (0 = unlimited)",data.Expeditions.SessionLimit,0,999,v=>data.Expeditions.SessionLimit=v);
  void Button(string title,Action a){var b=new Button{Text=title,Width=320,Height=32,FlatStyle=FlatStyle.Flat};b.Click+=(_,_)=>a();panel.Controls.Add(b);}
  Button("Send BobbyBoi on Expedition",()=>{data.Expeditions.Enabled=true;data.Save();expedition.Request();});Button("Open BobbyBoi Memes Folder",OpenMemes);
 }
}
