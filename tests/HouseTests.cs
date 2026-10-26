using System.Text.Json;
namespace BobbyBoi;
static class HouseTests {
 public static void Run(){Directory.CreateDirectory("artifacts");using var pet=new Pet(true,true);pet.VerifyHouseRemoval();}
}
sealed partial class Pet {
 public void VerifyHouseRemoval(){worldTesting=true;data.World=new(){Automatic=false};data.NaturalMobs=false;data.Expeditions.Enabled=false;data.Volume=0;data.Bubbles=false;mobs.Clear();wolf=null;health=20;energy=90;hunger=80;var report=new List<string>();string inventory=JsonSerializer.Serialize(data.Items);
  void Enter(){CancelWorld();world.Clear();paused=false;data.World.Houses=true;data.World.HasHouse=true;data.World.HouseBlocks=HouseAsset().Cutoffs.Length;data.World.HouseX=x+Width/2;data.World.HouseY=y+Height;onHouseRoof=false;action=Act.Idle;vy=throwVx=0;respawnTime=0;world.Queue(WorldEventKind.HouseVisit);for(int i=0;i<100&&!actorInside;i++)UpdateWorld(.1f);if(!actorInside||!ActorHidden)throw new Exception("House fixture never entered");Render(true);if(BodyPixels()!=0)throw new Exception("House fixture did not hide the desktop character");}
  int BodyPixels(){int count=0;if(frame==null)return 0;for(int yy=100;yy<Height;yy++)for(int xx=0;xx<Width;xx++)if(frame.GetPixel(xx,yy).A>15)count++;return count;}
  void Visible(){if(actorInside||ActorHidden||onHouseRoof||world.Active?.Spec.Kind==WorldEventKind.HouseVisit)throw new Exception("House removal left stale inside/roof state");if(BodyPixels()<100)throw new Exception("House removal did not repaint the visible character");}
  try{
   Enter();paused=true;SetHousesEnabled(false);Visible();if(!paused)throw new Exception("House removal unexpectedly unpaused the app");report.Add("PASS: disabling houses while inside immediately redraws BobbyBoi even when paused.");
   Enter();data.World.HasHouse=false;UpdateWorld(.1f);Render(true);Visible();report.Add("PASS: removing the house cancels an active visit and clears hidden state.");
   Enter();data.World.HouseBlocks=0;UpdateWorld(.1f);Render(true);Visible();report.Add("PASS: an empty house mesh cannot retain a hidden occupant.");
   Enter();InterruptActivity();action=Act.Eat;SetHousesEnabled(false);if(action!=Act.Eat)throw new Exception("House cleanup cancelled the temporary gift interaction");data.World.Houses=true;action=Act.Idle;heldItem=null;ResumeInterrupted(5);Render(true);Visible();if(interruptedWorld!=null)throw new Exception("Removed house visit remained suspended");report.Add("PASS: an interrupted house visit cannot resume and hide him after removal/re-enabling.");
   data.World.HasHouse=true;data.World.Houses=true;onHouseRoof=true;y-=100;Set(Act.Sit,6);SetHousesEnabled(false);Render(true);Visible();if(action!=Act.Fall)throw new Exception("Removed roof did not restore falling physics");report.Add("PASS: removing the supporting roof restores the visible falling character.");
   CancelWorld();world.Clear();data.World.HasHouse=false;data.World.Houses=true;action=Act.Idle;vy=throwVx=0;world.Queue(WorldEventKind.Cave);for(int i=0;i<90&&!actorInside;i++)UpdateWorld(.1f);var cave=world.Active?.Id;SetHousesEnabled(false);Render(true);if(!ActorHidden||world.Active?.Id!=cave)throw new Exception("House cleanup interrupted an unrelated cave visit");report.Add("PASS: intentional hiding in other events is preserved.");
   CancelWorld();world.Clear();data.World.HasHouse=false;characterWorldScale=.7f;actorInside=false;RecoverMissingHouse();if(characterWorldScale!=.7f)throw new Exception("Missing house reset unrelated visible animation scale");characterWorldScale=1;actorInside=true;Render(true);Visible();report.Add("PASS: orphaned inside state recovers without changing other animation scales.");
   if(JsonSerializer.Serialize(data.Items)!=inventory)throw new Exception("House visibility cleanup changed inventory");report.Add("PASS: inventory and gifts are preserved.");File.WriteAllLines("artifacts/house-tests.txt",report);
  }finally{timer.Stop();tray.Dispose();menu.Dispose();mobs.Dispose();worldCanvas.Dispose();frame?.Dispose();frame=null;}
 }
}
