using System.Numerics;
namespace BobbyBoi;
static class JailTests {
 public static void Run(){Directory.CreateDirectory("artifacts");using var pet=new Pet(true,true);pet.VerifyJail();}
}
sealed partial class Pet {
 public void VerifyJail(){worldTesting=true;data.World=new(){Automatic=false};data.Expeditions.Enabled=false;data.NaturalMobs=false;data.Volume=0;wolf=null;mobs.Clear();clones.Clear();mischief=90;health=20;action=Act.Idle;throwVx=vy=0;world.Queue(WorldEventKind.Prison);UpdateWorld(.1f);var report=new List<string>();
  foreach(var sample in new[]{(6f,"entry"),(11f,"closed"),(18.5f,"opening"),(20.5f,"exit")}){while(world.Busy&&world.Active!.Age<sample.Item1)UpdateWorld(.05f);if(!world.Busy)throw new Exception("Prison ended before release animation");var area=new Rectangle((int)chaosAnchor.X-180,(int)chaosAnchor.Y-220,440,280);using var image=WorldCanvas.Snapshot(ExpansionVisuals(),area);using(var g=Graphics.FromImage(image)){if(!actorInside)Character.Draw(g,new(x+Width/2-area.X,y+Height-10-area.Y),4*data.Scale,action,worldAge,yaw,0);}image.Save("artifacts/jail-"+sample.Item2+".png");if(sample.Item2=="closed"){if(!actorInside)throw new Exception("Jailed character not composited inside");int green=0;for(int yy=50;yy<220;yy++)for(int xx=130;xx<240;xx++){var c=image.GetPixel(xx,yy);if(c.A>0&&c.G>c.R*1.2&&c.G>c.B*1.2)green++;}if(green<50)throw new Exception("Jail gate hides the whole character");}if(sample.Item2=="exit"&&(actorInside||x+Width/2<chaosAnchor.X+120))throw new Exception("Release left BobbyBoi inside the cell");report.Add("PASS: "+sample.Item2+" stage rendered with visible character and correct door/exit state.");}
  for(int i=0;i<40&&world.Busy;i++)UpdateWorld(.05f);if(world.Busy||actorInside||characterWorldScale!=1)throw new Exception("Jail cleanup failed");report.Add("PASS: release completes and restores the normal desktop character.");File.WriteAllLines("artifacts/jail-tests.txt",report);timer.Stop();tray.Dispose();menu.Dispose();worldCanvas.Dispose();mobs.Dispose();
 }
}
