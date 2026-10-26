using System.Numerics;
namespace BobbyBoi;
sealed partial class Pet {
 void UpdateCompanions(float dt){
  if(wolf!=null){wolf.Age+=dt;var target=new Vector2(x+Width/2-75,y+Height);if(actorInside||wolf.Mode=="Sleep"&&data.World.HasHouse)target=new(data.World.HouseX,data.World.HouseY);float dx=target.X-wolf.Feet.X;wolf.Direction=dx>=0?1:-1;
   if(respawnTime>0){wolf.Action=Act.Sit;}else if(wolf.Mode=="Sleep"&&data.World.HasHouse&&Math.Abs(dx)>20){wolf.Action=Act.Walk;wolf.Feet.X+=Math.Clamp(dx,-dt*155,dt*155);}else if(wolf.Mode=="Sleep"){wolf.Action=Act.Sleep;}else if(wolf.Mode=="Sit"){wolf.Action=Act.Sit;}else if(Math.Abs(dx)>60){wolf.Action=Act.Walk;wolf.Feet.X+=Math.Clamp(dx,-dt*(data.World.WolfAdopted?155:250),dt*(data.World.WolfAdopted?155:250));}else{wolf.Action=action==Act.Sleep?Act.Sleep:Act.Sit;}
   var areas=Screen.AllScreens.Select(s=>s.WorkingArea).ToArray();wolf.Feet.Y=DesktopWorld.Floor(areas,wolf.Feet.X,wolf.Feet.Y,true);if(data.World.WolfAdopted&&mobs.NearestThreat(wolf.Feet,100)!=null&&wolf.Idle<=0){mobs.Strike(wolf.Feet,3);WorldAudio.Play("bark",data.Volume);wolf.Idle=1;worldParticles.Burst(wolf.Feet-new Vector2(0,45),Color.Goldenrod,4);}wolf.Idle-=dt;
  }
  foreach(var clone in clones.ToArray()){
   clone.Age+=dt;clone.Idle-=dt;if(clone.Age>clone.Life){worldParticles.Burst(clone.Feet-new Vector2(0,50),Color.Silver,10);clones.Remove(clone);continue;}
   if(clone.Idle<=0){clone.Idle=rng.Next(2,6);clone.Action=new[]{Act.Idle,Act.Walk,Act.Run,Act.Dance,Act.Confused,Act.Wave,Act.Hurt}[rng.Next(7)];clone.Direction=rng.Next(2)==0?-1:1;}
   if(clone.Action is Act.Walk or Act.Run)clone.Feet.X+=clone.Direction*(clone.Action==Act.Run?95:36)*dt;var a=Screen.FromPoint(new((int)clone.Feet.X,(int)clone.Feet.Y-5)).WorkingArea;clone.Feet.Y=DesktopWorld.Floor(Screen.AllScreens.Select(s=>s.WorkingArea).ToArray(),clone.Feet.X,clone.Feet.Y,true);if(clone.Feet.X<a.Left+40){clone.Feet.X=a.Left+40;clone.Direction=1;}if(clone.Feet.X>a.Right-40){clone.Feet.X=a.Right-40;clone.Direction=-1;}
  }
 }
 void StepClimb(WorldActivity a,float dt){if(climbingTarget==null){CompleteWorld();return;}var platform=WindowGeometry.Read(climbingTarget.Handle);if(platform==null){standingPlatform=null;CompleteWorld();Set(Act.Fall,10);return;}climbingTarget=platform;
  if(a.Phase==0){if(WalkWorld(a.Target,dt,130)){a.Phase=1;a.PhaseAge=0;a.Start=new(x,y);}}else if(a.Phase==1){FaceMotion(1);Pose(Act.Climb);float t=Math.Clamp(a.PhaseAge/3,0,1);x=platform.Bounds.Left-Width/2-8;y=a.Start.Y+(platform.Bounds.Top+10-Height-a.Start.Y)*t;if(t>=1){a.Phase=2;a.PhaseAge=0;standingPlatform=platform;Say("mount everest");}}else{Pose(Act.Walk);x+=dt*55;FaceMotion(55);y=platform.Bounds.Top+10-Height;if(a.PhaseAge>2){standingPlatform=platform;CountWorld("climbs");CompleteWorld();Pose(Act.Sit);duration=6;}}
 }
 float WorldFloor(Rectangle[] areas,float feetX,float feetY,bool walking){if(onHouseRoof){if(!data.World.HasHouse||Math.Abs(feetX-data.World.HouseX)>80)onHouseRoof=false;else return data.World.HouseY-(data.World.HouseVariant==1?78:115)+10;}if(standingPlatform!=null){var p=WindowGeometry.Read(standingPlatform.Handle);if(p==null||feetX<p.Bounds.Left-10||feetX>p.Bounds.Right+10){standingPlatform=null;Say("why is it moving");if(vy>=0)Pose(Act.Fall);}else{float dx=p.Bounds.X-standingPlatform.Bounds.X,dy=p.Bounds.Y-standingPlatform.Bounds.Y;x+=dx;y+=dy;standingPlatform=p;return p.Bounds.Top+10;}}return DesktopWorld.Floor(areas,feetX,feetY,walking);}
 void StepCursor(WorldActivity a,float dt){
  if(!data.World.AllowCursor||!CursorPrank.SafeDesktop()||CursorPrank.Escape||CursorPrank.Emergency){CancelWorld();return;}
  if(!cursorGrabbed){var p=Cursor.Position;a.Target=new(p.X-Width/2,y);WalkWorld(a.Target,dt,120);if(Math.Abs(p.X-(x+Width/2))<35&&Math.Abs(p.Y-(y+Height-65))<35){cursorGrabbed=true;cursorGrabClock.Restart();a.PhaseAge=0;lastCursor=p;cursorCooldown=0;Say("yoink");}if(a.Age>4&&!cursorGrabbed)CompleteWorld();return;}
  if(cursorGrabClock.Elapsed.TotalSeconds>=1.2||a.PhaseAge>1.2f||Vector2.Distance(new(Cursor.Position.X,Cursor.Position.Y),new(lastCursor.X,lastCursor.Y))>14){if(a.PhaseAge>.9f)Award("mouse","Mouse Criminal");CompleteWorld();Say("worth it");return;}
  float direction=dir==0?1:dir;var area=Screen.FromPoint(lastCursor).WorkingArea;x=Math.Clamp(x+direction*65*dt,area.Left-30,area.Right-Width+30);Pose(Act.CursorGrab);var point=new Point(Math.Clamp(lastCursor.X+(int)(direction*65*dt),area.Left+3,area.Right-3),lastCursor.Y);if(!CursorPrank.Move(point,data.World.AllowCursor)){CancelWorld();return;}lastCursor=point;
 }
 void StepDragon(WorldActivity a,float dt){
  var area=Screen.FromPoint(new((int)x+Width/2,(int)y+Height-5)).WorkingArea;
  if(a.Phase==2){Pose(Act.Celebrate);if(a.PhaseAge>2)CompleteWorld();return;}
  float orbit=area.Left+area.Width*.5f+MathF.Sin(a.Age*1.6f)*Math.Min(250,area.Width*.25f);float altitude=area.Bottom-180-Math.Abs(MathF.Sin(a.Age*.8f))*140;
  if(a.Age<2)dragonPosition=Vector2.Lerp(new(area.Right+220,area.Bottom-200),new(orbit,altitude),a.Age/2);else{float dive=(a.Age%5)>3?1-Math.Abs((a.Age%5-4)):0;dragonPosition=Vector2.Lerp(new(orbit,altitude),new(x+Width/2,area.Bottom-65),Math.Clamp(dive,0,1));}
  heldItem="Diamond Sword";heldUntil=watch.Elapsed.TotalSeconds+1;FaceMotion(dragonPosition.X-x-Width/2);Pose(Act.Mine);float dx=dragonPosition.X-x-Width/2;if(Math.Abs(dx)>110)x+=Math.Sign(dx)*dt*70;
  attackAge+=dt;bossHitAge+=dt;float distance=Vector2.Distance(dragonPosition,new(x+Width/2,y+Height-60));
  if(distance<185&&attackAge>.7f){attackAge=0;dragonHealth-=6+(wolf!=null?2:0)+clones.Count;worldParticles.Burst(dragonPosition,Color.MediumPurple,6);Sfx("sword");}
  if(distance<85&&bossHitAge>2.2f){bossHitAge=0;ApplyMobDamage(dragonPosition,4,160);worldParticles.Burst(new(x+Width/2,y+Height-65),Color.IndianRed,5);if(respawnTime>0){CancelWorld();Say("this boss is pay to win");return;}}
  if(dragonHealth<=0){a.Phase=2;a.PhaseAge=0;worldParticles.Burst(dragonPosition,Color.YellowGreen,28);Award("dragon","FREE THE END - somehow survived");WorldAudio.Play("xp",data.Volume);AddWorldLoot(rng.Next(5)==0?"Dirt":"Dragon Trophy");Say("i have a wooden sword");}
  else if(a.Age>34){CancelWorld();FakeDeath("BobbyBoi lost to downloadable content");}
 }
 void WorldPassive(float dt){worldAge+=dt;toastTime=Math.Max(0,toastTime-dt);worldParticles.Step(dt);UpdateCompanions(dt);UpdateExpansionCompanion(dt);RenderWorld(dt);}
 void RenderWorld(float dt){if(worldTesting)return;var visuals=WorldVisuals().ToList();var shake=WorldShake;if(shake!=Vector2.Zero)visuals=visuals.Select(v=>new SceneVisual(Rectangle.Inflate(v.Bounds,6,6),(g,o)=>{var saved=g.Save();g.TranslateTransform(shake.X,shake.Y);v.Draw(g,o);g.Restore(saved);},v.Click)).ToList();worldCanvas.Render(dt,visuals,data.OnTop,data.World.PerformanceMode);}
 IEnumerable<SceneVisual> WorldVisuals(){
  foreach(var expansion in ExpansionVisuals())yield return expansion;
  if(HouseAvailable){var p=new Vector2(data.World.HouseX,data.World.HouseY);var asset=HouseAsset();int blocks=Math.Clamp(data.World.HouseBlocks,0,asset.Cutoffs.Length);int limit=blocks==0?0:asset.Cutoffs[blocks-1];float scale=data.World.HouseVariant==1?2:3;
   bool doorOpen=actorInside&&(world.Active?.PhaseAge??0)<.8f;int key=blocks+data.World.HouseVariant*1000+(doorOpen?10000:0);
   if(houseSprite==null||key!=houseSpriteKey){houseSprite?.Dispose();houseSprite=new Bitmap(340,230);houseSpriteKey=key;using var hg=Graphics.FromImage(houseSprite);asset.Mesh.Draw(hg,new(170,180),scale,.38f,(v,part)=>{if(part=="door"&&doorOpen){var pivot=new Vector3(-3.5f,12,12);v-=pivot;v=new(v.X*.2f+v.Z*.98f,v.Y,-v.X*.98f+v.Z*.2f);v+=pivot;}return v;},limit);}
   var sprite=houseSprite;yield return new(new((int)p.X-170,(int)p.Y-180,340,230),(g,o)=>g.DrawImageUnscaled(sprite,(int)(p.X+o.X-170),(int)(p.Y+o.Y-180)),button=>HousePopup());   if(actorInside&&world.Active?.Spec.Kind==WorldEventKind.HouseVisit){yield return new(new((int)p.X-45,(int)p.Y-95,90,50),(g,o)=>{using var f=new Font("Consolas",10,FontStyle.Bold);g.DrawString("zzzz",f,Brushes.Beige,p.X+o.X-18,p.Y+o.Y-85+(float)Math.Sin(worldAge*2)*3);});}
  }
  if(wolf!=null&&data.World.Companions&&!(HouseAvailable&&wolf.Mode=="Sleep"&&Math.Abs(wolf.Feet.X-data.World.HouseX)<25)){var w=wolf;var p=w.Feet;yield return new(new((int)(p.X-90*data.Scale),(int)(p.Y-125*data.Scale),(int)(180*data.Scale),(int)(145*data.Scale)),(g,o)=>WorldAssets.Wolf.Draw(g,p+o,3*data.Scale,w.Direction*.75f,(v,part)=>{
    if(part=="legA"||part=="legB"){float angle=w.Action==Act.Walk?MathF.Sin(w.Age*10)*(part=="legA"?1:-1)*.5f:0;v.Y-=8;v=new(v.X,v.Y*MathF.Cos(angle)-v.Z*MathF.Sin(angle),v.Y*MathF.Sin(angle)+v.Z*MathF.Cos(angle));v.Y+=8;}if(part=="tail"){float a=MathF.Sin(w.Age*7)*.35f;v.X+=MathF.Sin(a)*Math.Abs(v.Z)*.4f;}if(part=="head")v.X+=MathF.Sin(w.Age*1.2f)*.35f;if(w.Action==Act.Sit)v.Y*=.75f;if(w.Action==Act.Sleep){v.Y*=.4f;v.X+=MathF.Sin(w.Age)*.1f;}return v;
   }),button=>WolfPopup(button));}
  foreach(var c in clones){var clone=c;yield return new(new((int)(c.Feet.X-115*data.Scale),(int)(c.Feet.Y-260*data.Scale),(int)(230*data.Scale),(int)(280*data.Scale)),(g,o)=>Character.Draw(g,clone.Feet+o-new Vector2(0,10),4*data.Scale,clone.Action,clone.Age,clone.Direction*.95f,Cursor.Position.X-clone.Feet.X,animationPose:clone.Pose));}
  if(treasure!=null){var chest=treasure;var p=chest.Feet;float size=chest.Nested&&!chest.Claimed?3-Math.Min(1.6f,(world.Active?.PhaseAge??0)*.3f):3;
   yield return new(new((int)p.X-50,(int)p.Y-75,100,90),(g,o)=>WorldAssets.Chest.Draw(g,p+o,size,.4f,(v,part)=>{if(part=="lid"){v-=new Vector3(0,10,-6);float a=-chest.Opening*1.25f;v=new(v.X,v.Y*MathF.Cos(a)-v.Z*MathF.Sin(a),v.Y*MathF.Sin(a)+v.Z*MathF.Cos(a));v+=new Vector3(0,10,-6);}return v;}),button=>ChestPopup());
  }
  var active=world.Active;
  if(active?.Spec.Kind==WorldEventKind.Fishing&&active.Phase>0){var a=active;var hand=Character.Hand(new(x+Width/2,y+Height-10),4*data.Scale,yaw);var tip=hand+new Vector2(dir*15,-36);var end=fishingBobber+new Vector2(0,active.Phase==2?(float)Math.Sin(worldAge*8)*2:0);var bounds=Rectangle.FromLTRB((int)Math.Min(hand.X,end.X)-35,(int)Math.Min(tip.Y,end.Y)-50,(int)Math.Max(hand.X,end.X)+40,(int)Math.Max(hand.Y,end.Y)+30);
   yield return new(bounds,(g,o)=>{WorldAssets.Item("Fishing Rod").Draw(g,hand+o,5,dir*.8f,(v,part)=>{float angle=-dir*.6f;return new Vector3(v.X*MathF.Cos(angle)-v.Y*MathF.Sin(angle),v.X*MathF.Sin(angle)+v.Y*MathF.Cos(angle),v.Z);});using var line=new Pen(Color.LightGray,1);g.DrawBezier(line,new PointF(tip.X+o.X,tip.Y+o.Y),new PointF(tip.X+dir*35+o.X,tip.Y+35+o.Y),new PointF(end.X-dir*30+o.X,end.Y-25+o.Y),new PointF(end.X+o.X,end.Y+o.Y));g.FillRectangle(Brushes.Red,end.X+o.X-3,end.Y+o.Y-3,6,6);g.DrawEllipse(Pens.LightBlue,end.X+o.X-12,end.Y+o.Y-2,24,5);});
   if(a.Phase>=3){var p=a.Phase==3?caughtPosition:new Vector2(x+Width/2+25,y+Height-90);yield return new(new((int)p.X-40,(int)p.Y-35,80,75),(g,o)=>WorldAssets.Item(a.Loot).Draw(g,p+o,a.Heavy?9:5,.3f));}
  }
  if(active?.Spec.Kind==WorldEventKind.Dragon){var p=dragonPosition;float fade=active.Phase==2?Math.Max(0,1-active.PhaseAge/2):1;
   if(fade>0)yield return new(new((int)p.X-165,(int)p.Y-115,330,220),(g,o)=>WorldAssets.Dragon.Draw(g,p+o,3*fade,MathF.Sin(worldAge)*.4f,(v,part)=>{if(part is "wingL" or "wingR"){float a=MathF.Sin(worldAge*9)*.5f*(part=="wingL"?1:-1);v=new(v.X*MathF.Cos(a)-v.Y*MathF.Sin(a),v.X*MathF.Sin(a)+v.Y*MathF.Cos(a),v.Z);}if(part.StartsWith("tail"))v.X+=MathF.Sin(worldAge*4+v.Z*.1f)*2;return v;}));
   var area=Screen.FromPoint(new((int)x+Width/2,(int)y+Height-5)).WorkingArea;yield return new(new(area.Right-330,area.Bottom-365,300,45),(g,o)=>{float xx=area.Right-325+o.X,yy=area.Bottom-355+o.Y;g.FillRectangle(Brushes.Black,xx-3,yy+18,286,12);g.FillRectangle(Brushes.MediumPurple,xx,yy+20,Math.Max(0,280*dragonHealth/40),7);using var f=new Font("Consolas",10,FontStyle.Bold);g.DrawString("ENDER DRAGON",f,Brushes.Beige,xx,yy);});
  }
  if(respawnTime>0){for(int i=0;i<6;i++){int k=i;var p=new Vector2(x+Width/2-65+i*25,y+Height-5-Math.Abs(MathF.Sin(worldAge*3+i))*9);yield return new(new((int)p.X-15,(int)p.Y-25,30,35),(g,o)=>WorldAssets.Item(k%2==0?"Diamond":"Stick").Draw(g,p+o,3,k));}}
  foreach(var particle in worldParticles.Visuals())yield return particle;
  if(toastTime>0){var a=Screen.FromPoint(new((int)x+Width/2,(int)y+Height-5)).WorkingArea;yield return new(new(a.Right-340,a.Top+25,315,55),(g,o)=>{g.FillRectangle(Brushes.DarkOliveGreen,a.Right-340+o.X,a.Top+25+o.Y,315,50);g.DrawRectangle(Pens.Goldenrod,a.Right-338+o.X,a.Top+27+o.Y,311,46);using var f=new Font("Consolas",10,FontStyle.Bold);g.DrawString(toast,f,Brushes.Beige,new RectangleF(a.Right-330+o.X,a.Top+34+o.Y,290,36));});}
 }
}
