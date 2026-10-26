using System.Numerics;
namespace BobbyBoi;
sealed class CompanionActor {
 public Vector2 Feet;public float Age,Direction=1,Idle,Life=35;public string Mode="Follow";public Act Action=Act.Idle;public readonly CharacterPose Pose=new();public readonly int Id;
 public CompanionActor(Vector2 feet,int id=0){Feet=feet;Id=id;}
}
sealed class TreasureProp {
 public Vector2 Feet;public float Opening,Age;public string Kind="Common chest";public Dictionary<string,int> Items=[];public bool Claimed,Nested;
}
sealed partial class Pet {
 readonly WorldEventDirector world=new();readonly WorldCanvas worldCanvas=new();readonly WorldParticles worldParticles=new();readonly List<CompanionActor> clones=[];readonly Queue<string> worldLog=[];
 CompanionActor? wolf;TreasureProp? treasure;PetMenu? objectMenu;float worldAge,toastTime,houseInside,dragonHealth=40,attackAge,bossHitAge;string toast="",lastStage="";bool onHouseRoof,actorInside,worldTesting;float worldSpeed=1;float characterWorldScale=1;Vector2 fishingBobber,caughtPosition,dragonPosition;nint worldClimbFixture;WindowPlatform? climbingTarget,standingPlatform;Point lastCursor;float cursorCooldown=300;bool cursorGrabbed;readonly System.Diagnostics.Stopwatch cursorGrabClock=new(); Bitmap? houseSprite;int houseSpriteKey=int.MinValue;(VoxelMesh Mesh,int[] Cutoffs)? houseMesh;int houseMeshVariant=-1;
 void InitWorld(){InitExpansion();data.World??=new();data.World.Normalize();if(data.World.WolfAdopted)wolf=new(new(x+Width/2-95,y+Height)){Mode=data.World.WolfMode};if(data.World.HasHouse){var area=Screen.FromPoint(new((int)data.World.HouseX,(int)data.World.HouseY-10)).WorkingArea;data.World.HouseX=Math.Clamp(data.World.HouseX,area.Left+100,area.Right-100);data.World.HouseY=area.Bottom;}}
 void SaveWorld(){if(!worldTesting&&!expeditionTest&&!mobTest)data.Save();}
 void WorldTrace(string text){if(!data.World.Debug)return;worldLog.Enqueue($"{DateTimeOffset.Now:O} {text}");while(worldLog.Count>160)worldLog.Dequeue();try{Directory.CreateDirectory(SaveData.Folder);File.WriteAllLines(Path.Combine(SaveData.Folder,"world.log"),worldLog);}catch{}}
 void Award(string id,string title){if(data.World.Achievements.Add(id)){Sfx("achievement");toast=title;toastTime=5;worldParticles.Burst(new(x+Width/2,y+Height-120),Color.Gold,12);SaveWorld();}}
 void CountWorld(string key,int count=1){data.World.Counters.TryGetValue(key,out int old);data.World.Counters[key]=old+count;if(key=="fish"&&old+count>=20)Award("fisher","Master Fisherman");if(key=="chest"&&old+count>=20)Award("loot","Professional Loot Goblin");if(key=="deaths"&&old+count>=10)Award("skill","Skill Issue");if(key=="climbs"&&old+count>=5)Award("parkour","Parkour Legend");SaveWorld();}
 void AddWorldLoot(string name,int amount=1){Sfx("pickup");data.Items.TryGetValue(name,out int old);data.Items[name]=Math.Min(999999,old+amount);SaveWorld();}
 bool WorldEligible(WorldEventKind kind)=>kind switch{WorldEventKind.AdoptCreeper=>!data.World.PetCreeper&&(wolf==null?0:1)<data.World.CompanionLimit,WorldEventKind.Prison=>mischief>15,WorldEventKind.Casino=>curiosity>20,WorldEventKind.AdoptWolf=>!data.World.WolfAdopted&&(data.World.PetCreeper?1:0)<data.World.CompanionLimit,WorldEventKind.HouseVisit=>data.World.HasHouse&&data.World.HouseBlocks>20,WorldEventKind.Clone=>clones.Count<2,WorldEventKind.Climb=>WindowGeometry.Platforms().Count>0,WorldEventKind.CursorPrank=>data.World.AllowCursor&&cursorCooldown>=300&&CursorPrank.SafeDesktop(),WorldEventKind.BuildHouse=>energy>25,WorldEventKind.Fishing=>energy>20&&hunger>10,WorldEventKind.Treasure=>curiosity>20,_=>true};
 void RequestWorld(WorldEventKind kind){if(!data.World.Enabled(kind)){Say("enable this in World settings");return;}if(!WorldEligible(kind)){Say(kind==WorldEventKind.HouseVisit?"build a house first":kind==WorldEventKind.Climb?"no reachable window":kind==WorldEventKind.CursorPrank?"desktop only, after cooldown":"not available right now");return;}world.Queue(kind);Say("on my way");}
 void StartWorld(WorldActivity a){ResetExpansionScene();a.Start=new(x,y);a.Target=a.Start;vy=throwVx=0;travelTime=-1;follow=false;standingPlatform=null;onHouseRoof=false;heldItem=null;lastStage="";WorldTrace($"start {a.Id} {a.Spec.Kind}");
  var area=Screen.FromPoint(new((int)x+Width/2,(int)y+Height-10)).WorkingArea;
  if(a.Spec.Kind>=WorldEventKind.Prison){StartExpansion(a,area);return;}
  switch(a.Spec.Kind){
   case WorldEventKind.Fishing:a.Wait=rng.Next(4,9);var caught=WorldLoot.Catch(rng);a.Loot=caught.Name;a.Heavy=a.Loot=="Suspiciously Large Fish"||rng.Next(50)==0;dir=x+Width/2<area.Left+area.Width/2?-1:1;a.Target=new(Math.Clamp(x+dir*70,area.Left-40,area.Right-Width+40),area.Bottom-Height);Say(quips.Next("fishing"));break;
   case WorldEventKind.BuildHouse:
    data.World.HasHouse=true;data.World.HouseVariant=rng.Next(4);data.World.HouseBlocks=0;data.World.HouseX=Math.Clamp(x+Width/2+dir*140,area.Left+110,area.Right-110);data.World.HouseY=area.Bottom;houseMesh=null;a.Target=new(Math.Clamp(data.World.HouseX-Width/2-dir*105,area.Left-50,area.Right-Width+50),area.Bottom-Height);Say(quips.Next("house"));break;
   case WorldEventKind.Treasure:
    var reward=WorldLoot.Chest(rng);treasure=new(){Feet=new(Math.Clamp(x+Width/2+110,area.Left+55,area.Right-55),area.Bottom),Kind=reward.Kind,Items=reward.Items,Nested=reward.Nested};a.Target=new(treasure.Feet.X-Width/2-45,area.Bottom-Height);Say(quips.Next("treasure"));break;
   case WorldEventKind.AdoptWolf:wolf=new(new(x+Width/2<area.Left+area.Width/2?area.Left-100:area.Right+100,area.Bottom));Say("i found a dog");break;
   case WorldEventKind.Clone:WorldAudio.Play("magic",data.Volume);for(int i=0;i<2&&clones.Count<2;i++)clones.Add(new(new(Math.Clamp(x+Width/2+(i==0?-105:105),area.Left+55,area.Right-55),area.Bottom),rng.Next()){Life=rng.Next(25,46),Direction=i==0?-1:1});worldParticles.Burst(new(x+Width/2,y+Height-65),Color.MediumPurple,24);Say("what does this button do");if(clones.Count==2)Award("identity","Identity Crisis");break;
   case WorldEventKind.Climb:climbingTarget=worldClimbFixture!=0?WindowGeometry.Read(worldClimbFixture):WindowGeometry.Platforms().OrderBy(p=>Math.Abs(p.Bounds.Left-(x+Width/2))+Math.Abs(p.Bounds.Top-(y+Height))).FirstOrDefault();if(climbingTarget==null){CancelWorld();return;}a.Target=new(climbingTarget.Bounds.Left-Width/2-8,area.Bottom-Height);Say("parkour");break;
   case WorldEventKind.CursorPrank:cursorGrabbed=false;lastCursor=Cursor.Position;a.Target=new(lastCursor.X-Width/2,y);Say("give me that");break;
   case WorldEventKind.HouseVisit:a.Target=new(data.World.HouseX-Width/2,data.World.HouseY-Height);Say("luxury apartment");break;
   case WorldEventKind.Dragon:dragonHealth=40;attackAge=bossHitAge=0;dragonPosition=new(area.Right+220,area.Bottom-200);Say(quips.Next("dragon"));break;
  }
 }
 void Pose(Act state){if(action!=state)Set(state,999);}
 bool WalkWorld(Vector2 target,float dt,float speed=100){float dx=target.X-x;FaceMotion(dx);Pose(Act.Walk);moveDistance+=Math.Min(Math.Abs(dx),speed*dt);if(moveDistance>25){moveDistance=0;Tone(Act.Walk);}x+=Math.Clamp(dx,-speed*dt,speed*dt);y+=Math.Clamp(target.Y-y,-350*dt,350*dt);return Math.Abs(dx)<4&&Math.Abs(target.Y-y)<4;}
 void CompleteWorld(){EndExpansion();var a=world.Active;if(a!=null)WorldTrace($"complete {a.Id} {a.Spec.Kind}");actorInside=false;characterWorldScale=1;cursorGrabbed=false;world.Finish();heldItem=null;Set(Act.Idle,3);SaveWorld();}
 void CancelWorld(){ClearInterrupted();EndExpansion();var a=world.Active;if(a!=null)WorldTrace($"cancel {a.Id} {a.Spec.Kind}");actorInside=false;characterWorldScale=1;cursorGrabbed=false;world.Cancel();heldItem=null;if(!dragging)Set(Act.Idle,2);}
 void SuspendWorld(){WorldAudio.Silence();if(world.Active?.Spec.Kind==WorldEventKind.CursorPrank)CancelWorld();worldCanvas.Hide();}
 bool UpdateWorld(float dt){
  RecoverMissingHouse();
  if(!worldTesting&&Native.Fullscreen(Handle)){if(world.Active?.Spec.Kind==WorldEventKind.CursorPrank)CancelWorld();worldCanvas.Hide();return world.Busy;}
  ResumeInterrupted(dt);worldAge+=dt;toastTime=Math.Max(0,toastTime-dt);cursorCooldown+=dt;worldParticles.Step(dt);if(data.World.AllowCursor&&CursorPrank.Emergency){data.World.AllowCursor=false;CancelWorld();SaveWorld();Say("cursor prank disabled");}
  UpdateCompanions(dt);UpdateExpansionCompanion(dt);
  if(treasure!=null){treasure.Age+=dt;if(treasure.Age>150)treasure=null;}
  bool threat=mobs.NearestThreat(new(x+Width/2,y+Height),270)!=null;
  if(world.Busy&&(dragging||respawnTime>0||threat&&world.Active!.Spec.Kind is not(WorldEventKind.Dragon or WorldEventKind.HouseVisit))){CancelWorld();}
  bool safe=interruptedWorld==null&&!interruptedExpedition&&!dragging&&!pressed&&!menu.Visible&&!expedition.Busy&&respawnTime<=0&&Math.Abs(throwVx)<20&&vy==0&&!threat&&action is not(Act.Sleep or Act.Eat or Act.Celebrate or Act.Dance or Act.Hurt);
  if(menu.Visible&&world.Busy){if(world.Active!.Spec.Kind==WorldEventKind.CursorPrank)CancelWorld();RenderWorld(dt);return world.Busy;}
  var started=world.Tick(dt,data.World,safe,expedition.Remaining<25,WorldEligible);if(started!=null)StartWorld(started);
  var activity=world.Active;if(activity!=null){
   if(menu.Visible)return true;
   try{StepWorldActivity(activity,dt);}catch(Exception e){WorldTrace("event error "+e.Message);CancelWorld();Say("that was lag");}
  }
  RenderWorld(dt);
  if(world.Busy){expedition.Tick(dt,false,data.Expeditions);if(!worldTesting){Location=new((int)x,(int)y);Render();}return true;}return false;
 }
 void StepWorldActivity(WorldActivity a,float dt){
  if(a.Spec.Kind>=WorldEventKind.Prison){StepExpansion(a,dt);return;}
  if(lastStage!=a.Stage){lastStage=a.Stage;WorldTrace($"phase {a.Spec.Kind} {a.Stage}");}
  switch(a.Spec.Kind){
   case WorldEventKind.Fishing:StepFishing(a,dt);break;
   case WorldEventKind.BuildHouse:StepHouse(a,dt);break;
   case WorldEventKind.Treasure:StepTreasure(a,dt);break;
   case WorldEventKind.AdoptWolf:Pose(Act.Wave);if(wolf!=null&&Vector2.Distance(wolf.Feet,new(x+Width/2,y+Height))<125||a.Age>7){Sfx("tame");data.World.WolfAdopted=true;if(wolf!=null)wolf.Mode="Follow";worldParticles.Burst(new(x+Width/2-70,y+Height-55),Color.Pink,10,"?");Award("wolf","Who's a Good Boy?");CompleteWorld();}break;
   case WorldEventKind.Clone:Pose(Act.Confused);if(a.Age>3.5f)CompleteWorld();break;
   case WorldEventKind.Climb:StepClimb(a,dt);break;
   case WorldEventKind.CursorPrank:StepCursor(a,dt);break;
   case WorldEventKind.HouseVisit:
    if(a.Phase==0){if(WalkWorld(a.Target,dt)){a.Phase=1;a.PhaseAge=0;}}else{actorInside=true;houseInside+=dt;Pose(Act.Sleep);if(a.PhaseAge>5){actorInside=false;x=a.Target.X+100;energy=Math.Min(100,energy+25);Say(wolf?.Mode=="Sleep"?"the dog stole my bed":"five stars");CompleteWorld();}}break;
   case WorldEventKind.Dragon:StepDragon(a,dt);break;
  }
  if(a.Age>a.Spec.EstimatedSeconds+15&&world.Active==a){Say("i regret this");CompleteWorld();}
 }
 void StepFishing(WorldActivity a,float dt){heldItem=null;var fishingScreen=Screen.FromPoint(new((int)x+Width/2,(int)y+Height-10));fishingBobber=new(Math.Clamp(x+Width/2+dir*125,fishingScreen.Bounds.Left+15,fishingScreen.Bounds.Right-15),Math.Min(fishingScreen.Bounds.Bottom-4,y+Height+8));a.Stage=a.Phase switch{0=>"walking to water",1=>"casting",2=>"waiting",3=>"reeling",_=>"reacting"};
  if(a.Phase==0){if(WalkWorld(a.Target,dt)){a.Phase=1;a.PhaseAge=0;Pose(Act.FishingCast);WorldAudio.Play("cast",data.Volume);}}else if(a.Phase==1){Pose(Act.FishingCast);if(a.PhaseAge>1.2f){a.Phase=2;a.PhaseAge=0;worldParticles.Burst(fishingBobber,Color.LightSkyBlue,6);WorldAudio.Play("splash",data.Volume);}}else if(a.Phase==2){Pose(Act.FishingWait);if(a.PhaseAge>a.Wait){a.Phase=3;a.PhaseAge=0;Sfx("bite");Sfx("reel");}}else if(a.Phase==3){Pose(Act.FishingReel);float t=Math.Clamp(a.PhaseAge/1.1f,0,1);caughtPosition=Vector2.Lerp(fishingBobber,new(x+Width/2+20,y+Height-100),t)-new Vector2(0,MathF.Sin(t*MathF.PI)*100);if(a.Heavy)x+=dir*dt*30;if(t>=1){a.Phase=4;a.PhaseAge=0;AddWorldLoot(a.Loot);CountWorld("fish");WorldAudio.Play("pickup",data.Volume);Say(WorldLoot.Fishing.Single(c=>c.Name==a.Loot).Quip);if(a.Heavy){Pose(Act.Hurt);Say("the fish caught me");}else Pose(Act.Celebrate);if(wolf!=null&&rng.Next(3)==0){wolf.Feet=new(x+Width/2+50,y+Height);Say("the dog stole my fish");}}}else if(a.PhaseAge>2.5f){bool fatal=a.Heavy&&rng.Next(3)==0;CompleteWorld();if(fatal)FakeDeath("BobbyBoi lost a fight against a fish");}}
 void StepHouse(WorldActivity a,float dt){a.Stage="placing blocks";heldItem="Dirt";heldUntil=watch.Elapsed.TotalSeconds+1;if(a.Phase==0){if(WalkWorld(a.Target,dt)){a.Phase=1;a.PhaseAge=0;}}else{Pose(Act.Mine);var house=HouseAsset();int count=Math.Min(house.Cutoffs.Length,(int)(a.PhaseAge/.18f));if(count>data.World.HouseBlocks){data.World.HouseBlocks=count;if(count%5==0)WorldAudio.Play("block",data.Volume);worldParticles.Burst(new(data.World.HouseX,data.World.HouseY-35),Color.SaddleBrown,2);}if(count>=house.Cutoffs.Length){Award("architect","Certified Architect");Say(new[]{"architecture","500k emeralds","luxury apartment","no building permit"}[rng.Next(4)]);CompleteWorld();}}}
 void StepTreasure(WorldActivity a,float dt){if(treasure==null){CompleteWorld();return;}a.Stage="opening treasure";if(a.Phase==0){if(WalkWorld(a.Target,dt)){a.Phase=1;a.PhaseAge=0;WorldAudio.Play("chest",data.Volume);}}else{Pose(Act.Confused);treasure.Opening=Math.Clamp(a.PhaseAge/1.2f,0,1);if(treasure.Nested&&a.PhaseAge<4){treasure.Opening=(a.PhaseAge%1.3f)/1.3f;Pose(Act.Mine);}if(a.PhaseAge>(treasure.Nested?4.5f:2)&&!treasure.Claimed){foreach(var item in treasure.Items)AddWorldLoot(item.Key,item.Value);treasure.Claimed=true;CountWorld("chest");worldParticles.Burst(treasure.Feet-new Vector2(0,45),Color.Gold,14);Say(treasure.Nested?"all that for ONE dirt":treasure.Kind=="Cursed chest"?"the real treasure was unemployment":"i am financially unstable");Pose(treasure.Nested?Act.Hurt:Act.Celebrate);}if(a.PhaseAge>(treasure.Nested?7:4.5f))CompleteWorld();}}
 (VoxelMesh Mesh,int[] Cutoffs) HouseAsset(){if(houseMesh==null||houseMeshVariant!=data.World.HouseVariant){houseMesh=WorldAssets.House(data.World.HouseVariant);houseMeshVariant=data.World.HouseVariant;}return houseMesh.Value;}
 void FakeDeath(string reason){Sfx("death");CancelWorld();CancelExpedition();CountWorld("deaths");health=0;respawnTime=4;damageTint=.32f;throwVx=vy=0;Set(Act.Hurt,5);Say(reason);worldParticles.Burst(new(x+Width/2,y+Height-55),Color.Silver,18);}
}
