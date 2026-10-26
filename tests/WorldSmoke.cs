using System.Diagnostics;
namespace BobbyBoi;
sealed partial class Pet {
 public void WorldSmoke(bool chaos=false){string reportPath=chaos?"artifacts/chaos-live.txt":"artifacts/world-live.txt";string imagePath=chaos?"artifacts/chaos-live.png":"artifacts/world-live.png";Directory.CreateDirectory("artifacts");File.WriteAllText(reportPath,"Starting world test\n");data.World=new(){Automatic=false};data.NaturalMobs=false;data.Expeditions.Enabled=false;data.Volume=0;data.Fullscreen=false;wolf=null;clones.Clear();mobs.Clear();worldTesting=false;
  WorldEventKind[] kinds=[WorldEventKind.Fishing,WorldEventKind.BuildHouse,WorldEventKind.Treasure,WorldEventKind.AdoptWolf,WorldEventKind.Clone,WorldEventKind.Dragon];if(chaos)kinds=Enum.GetValues<WorldEventKind>().Where(k=>k>=WorldEventKind.Prison).ToArray();int next=0;bool waiting=false,running=false;float tail=0;using var testClock=new System.Windows.Forms.Timer{Interval=50};var elapsed=Stopwatch.StartNew();var log=new List<string>();worldSpeed=chaos?4:3;
  Shown+=(_,_)=>{ShowPet();testClock.Start();};testClock.Tick+=(_,_)=>{
   if(waiting&&world.Busy)running=true;if(waiting&&running&&!world.Busy){log.Add($"{elapsed.Elapsed.TotalSeconds:0.00}s completed {kinds[next-1]}");waiting=false;File.WriteAllLines(reportPath,log);}
   if(!waiting&&next<kinds.Length&&respawnTime<=0){if(kinds[next]==WorldEventKind.AdoptCreeper)data.World.PetCreeper=false;world.Queue(kinds[next]);log.Add($"{elapsed.Elapsed.TotalSeconds:0.00}s queued {kinds[next++]}");waiting=true;running=false;File.WriteAllLines(reportPath,log);}
   if(next==kinds.Length&&!waiting){tail+=.05f;if(tail>2){testClock.Stop();log.Add($"PASS=True; activities={kinds.Length}; house={data.World.HasHouse}; wolf={data.World.WolfAdopted}; clones={clones.Count}; inventory={data.Items.Values.Sum()}; workingSetMB={Environment.WorkingSet/1048576}");Directory.CreateDirectory("artifacts");File.WriteAllLines(reportPath,log);using var image=WorldCanvas.Snapshot(WorldVisuals(),new Rectangle(Screen.PrimaryScreen!.WorkingArea.Left,Screen.PrimaryScreen.WorkingArea.Bottom-500,Screen.PrimaryScreen.WorkingArea.Width,500));image.Save(imagePath);Close();}}
   if(elapsed.Elapsed.TotalSeconds>100){testClock.Stop();File.WriteAllLines(reportPath,log.Append("FAIL: timeout; active="+world.Active?.Spec.Kind));Close();}
  };Application.Run(this);
 }
 public void ClimbSmoke(){Directory.CreateDirectory("artifacts");File.WriteAllText("artifacts/climb-live.txt","Starting climb test\n");data.World=new(){Automatic=false};data.NaturalMobs=false;data.Expeditions.Enabled=false;data.Fullscreen=false;data.Volume=0;mobs.Clear();worldTesting=false;
  var start=new ProcessStartInfo(Environment.ProcessPath!){UseShellExecute=false,WorkingDirectory=Environment.CurrentDirectory};start.ArgumentList.Add("--window-fixture");using var fixture=Process.Start(start)!;
  using var probe=new System.Windows.Forms.Timer{Interval=100};var elapsed=Stopwatch.StartNew();bool queued=false,reached=false,moved=false,fell=false;int lastTop=0;
  Shown+=(_,_)=>{ShowPet();probe.Start();};probe.Tick+=(_,_)=>{if(!queued&&elapsed.Elapsed.TotalSeconds>2){fixture.Refresh();var platform=WindowGeometry.Read(fixture.MainWindowHandle);if(platform!=null){worldClimbFixture=platform.Handle;x=platform.Bounds.Left-Width/2-8;y=Screen.FromRectangle(platform.Bounds).WorkingArea.Bottom-Height;vy=throwVx=0;action=Act.Idle;world.Queue(WorldEventKind.Climb);queued=true;}}if(standingPlatform!=null){reached=true;if(lastTop!=0&&lastTop!=standingPlatform.Bounds.Top)moved=true;lastTop=standingPlatform.Bounds.Top;}else if(reached&&elapsed.Elapsed.TotalSeconds>21)fell=true;
   if(elapsed.Elapsed.TotalSeconds>29){probe.Stop();Directory.CreateDirectory("artifacts");File.WriteAllText("artifacts/climb-live.txt",$"PASS={queued&&reached&&moved&&fell}; queued={queued}; reached={reached}; followedMovedPlatform={moved}; fellAfterMinimize={fell}; fixtureOnly=true");Close();}
  };Application.Run(this);
 }
}
static class WorldFixture {
 public static void Run(){using var f=new Form{Text="BobbyBoi platform test fixture",Size=new Size(640,340),StartPosition=FormStartPosition.Manual,Location=new Point(350,260)};using var clock=new System.Windows.Forms.Timer{Interval=100};var age=Stopwatch.StartNew();clock.Tick+=(_,_)=>{if(age.Elapsed.TotalSeconds>24){f.Close();return;}if(age.Elapsed.TotalSeconds>20)f.WindowState=FormWindowState.Minimized;else if(age.Elapsed.TotalSeconds>12)f.Top=180;};f.Shown+=(_,_)=>clock.Start();Application.Run(f);}
}
