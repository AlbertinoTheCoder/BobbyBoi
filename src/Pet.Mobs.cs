using System.Numerics;
namespace BobbyBoi;
sealed partial class Pet {
  readonly bool mobTest;readonly List<string> mobTrace=[];int recordedGroups,recordedBlasts;
 void MobTrace(){if(mobs.Groups!=recordedGroups){recordedGroups=mobs.Groups;mobTrace.Add($"{watch.Elapsed.TotalSeconds:0.00}s group {mobs.Groups}, mobs={mobs.Count}");}if(mobs.ExplosionCount!=recordedBlasts){recordedBlasts=mobs.ExplosionCount;mobTrace.Add($"{watch.Elapsed.TotalSeconds:0.00}s explosions={mobs.ExplosionCount}, mobs={mobs.Count}");}
  if(watch.Elapsed.TotalSeconds>82){Directory.CreateDirectory("artifacts");File.WriteAllLines("artifacts/mob-live.txt",mobTrace.Append($"PASS={mobs.Groups>=1&&mobs.ExplosionCount>=1}; visible={Visible}; workingSetMB={Environment.WorkingSet/1048576}"));Close();}}
   public void MenuLiveTest(bool outsideClick=false){
  data.Volume=0;data.NaturalMobs=false;data.Expeditions.Enabled=false;data.World.Automatic=false;using var probe=new System.Windows.Forms.Timer{Interval=outsideClick?45000:4000};bool opened=false;
  Shown+=(_,_)=>{ShowPet();SpawnMobs();menu.Show(new Point(Screen.PrimaryScreen!.WorkingArea.Left+80,Screen.PrimaryScreen.WorkingArea.Top+80));opened=menu.Visible;probe.Start();};
  probe.Tick+=(_,_)=>{probe.Stop();Directory.CreateDirectory("artifacts");File.WriteAllText(outsideClick?"artifacts/menu-outside-test.txt":"artifacts/menu-live-test.txt",$"PASS={opened&&(outsideClick?!menu.Visible:menu.Visible)&&mobs.Count>0}; outsideClickTest={outsideClick}; menuVisible={menu.Visible}; liveMobs={mobs.Count}; {menu.DismissDiagnostics}");Close();};Application.Run(this);
 }
 public void VerifyMobMenu(){
  try{
   data.Volume=0;
   ToolStripItem Get(string name)=>menu.Items.Cast<ToolStripItem>().Single(i=>i.Text==name);
   ((ToolStripMenuItem)Get("Spawn Zombie")).PerformClick();if(mobs.Count!=1)throw new Exception("Zombie menu did not spawn");
   ((ToolStripMenuItem)Get("Spawn Creeper")).PerformClick();if(mobs.Count!=2)throw new Exception("Creeper menu did not spawn");
   var sub=(ToolStripMenuItem)Get("Spawn mobs");sub.DropDownItems.Cast<ToolStripItem>().Single(i=>i.Text=="Spawn zombie + creeper group").PerformClick();if(mobs.Count<5)throw new Exception("Group menu did not spawn");
   sub.DropDownItems.Cast<ToolStripItem>().Single(i=>i.Text=="Clear all mobs and effects").PerformClick();if(mobs.Count!=0)throw new Exception("Clear menu did not work");
      Get("Restart BobbyBoi");((ToolStripMenuItem)Get("Clear Mobs")).PerformClick();
   var inventoryBefore=System.Text.Json.JsonSerializer.Serialize(data.Items);health=20;invulnerable=0;ApplyMobDamage(new Vector2(x+180,y+Height),2,170);if(health!=18||damageTint<=0)throw new Exception("Zombie damage / red tint missing");ApplyMobDamage(new Vector2(x+180,y+Height),2,170);if(health!=18)throw new Exception("Damage cooldown missing");invulnerable=0;ApplyMobDamage(new Vector2(x+180,y+Height),8,500);if(health!=10||damageTint<=0)throw new Exception("Creeper damage missing");
   health=20;respawnTime=0;invulnerable=0;throwVx=vy=0;action=Act.Idle;mobs.Spawn(Screen.AllScreens.Select(s=>s.WorkingArea).ToArray(),new Vector2(x+Width/2,y+Height),"zombie");mobs.Entities.Single().Feet=new(x+Width/2+120,y+Height);UpdateMobReactions(.01f);if(action!=Act.Run||dir!=-1)throw new Exception("Flee behavior missing");if(System.Text.Json.JsonSerializer.Serialize(data.Items)!=inventoryBefore)throw new Exception("Combat changed inventory");
   if(menu.AutoClose||sub.DropDown.AutoClose)throw new Exception("Mob focus changes can auto-dismiss menu");menu.Show(new Point(80,80));menu.DismissIfOutside(new Point(menu.Right+20,menu.Bottom+20));Application.DoEvents();if(menu.Visible)throw new Exception("Outside-click observer did not dismiss menu");
   Directory.CreateDirectory("artifacts");File.WriteAllText("artifacts/menu-test.txt","PASS: actual BobbyBoi menu has direct Spawn Zombie / Spawn Creeper entries; clicking each spawns its mob; group and clear actions work; restart entry exists; mob-safe dismissal mode; fleeing; zombie and creeper damage; brief red tint; attack invulnerability; inventory preserved.\n");
  }finally{timer.Stop();tray.Dispose();menu.Dispose();mobs.Dispose();}
 }
 readonly MobDirector mobs=new();
 void SpawnMobs(string? kind=null){var areas=Screen.AllScreens.Select(s=>s.WorkingArea).ToArray();Vector2 near=new(x+Width/2,y+Height);if(expedition.Busy){var a=Screen.FromPoint(Cursor.Position).WorkingArea;near=new(a.Left+a.Width/2,a.Bottom);}mobs.Spawn(areas,near,kind,data.Scale);Say(kind=="creeper"?"nice knowing you":kind=="zombie"?"bro wants my bread":"uninvited guests");}
 void MobMenu(){menu.Items.Add("Clear Mobs",null,(_,_)=>ClearMobs());menu.Items.Add("Spawn Zombie",null,(_,_)=>SpawnMobs("zombie"));menu.Items.Add("Spawn Creeper",null,(_,_)=>SpawnMobs("creeper"));var spawn=new ToolStripMenuItem("Spawn mobs");void Add(string text,Action a)=>spawn.DropDownItems.Add(text,null,(_,_)=>a());Add("Spawn zombie + creeper group",()=>SpawnMobs());Add("Spawn zombie",()=>SpawnMobs("zombie"));Add("Spawn creeper",()=>SpawnMobs("creeper"));Add("Detonate creepers",mobs.Detonate);spawn.DropDownItems.Add(new ToolStripSeparator());var natural=new ToolStripMenuItem("Natural groups every 60 seconds"){Checked=data.NaturalMobs,CheckOnClick=true};natural.Click+=(_,_)=>{data.NaturalMobs=natural.Checked;data.Save();};spawn.DropDownItems.Add(natural);Add("Clear all mobs and effects",ClearMobs);menu.Items.Add(spawn);menu.Opening+=(_,_)=>natural.Checked=data.NaturalMobs;}
 float damageTint,invulnerable,healthShown,respawnTime,escapeJumpCooldown;int health=20;
 void ClearMobs(){mobs.Clear();if(action==Act.Run)Set(Act.Idle,2);}
 void ApplyMobDamage(Vector2 origin,int amount,float push){
  if(actorInside||dragging||expedition.Busy||invulnerable>0||respawnTime>0)return;
  health=Math.Max(0,health-amount);damageTint=.32f;invulnerable=.65f;healthShown=5;throwVx=(x+Width/2<origin.X?-1:1)*push;vy=-Math.Min(310,push*.7f);Set(Act.Hurt,.65f);Say(quips.Next("hurt"));
  if(health==0){Sfx("death");CancelWorld();CountWorld("deaths");respawnTime=4;throwVx=vy=0;Set(Act.Hurt,5);Say("BobbyBoi experienced a skill issue");}
 }
 bool UpdateMobReactions(float dt){
  damageTint=Math.Max(0,damageTint-dt);invulnerable=Math.Max(0,invulnerable-dt);healthShown=Math.Max(0,healthShown-dt);escapeJumpCooldown=Math.Max(0,escapeJumpCooldown-dt);
  if(respawnTime>0){respawnTime-=dt;if(respawnTime<=0){Sfx("respawn");health=20;ShowPet();Set(Act.Confused,3);Say(quips.Next("respawn"));}Location=new((int)x,(int)y);Render();return true;}
  if(world.Busy||dragging||menu.Visible||expedition.Busy||action==Act.Hurt||Math.Abs(throwVx)>30)return false;
  var threat=mobs.NearestThreat(new Vector2(x+Width/2,y+Height),300);if(!threat.HasValue)return false;
  follow=false;travelTime=-1;dir=x+Width/2<threat.Value.X?-1:1;if(action!=Act.Run)Set(Act.Run,1.5f);
  var a=Screen.FromPoint(new((int)x+Width/2,(int)y+Height-10)).WorkingArea;
  if(escapeJumpCooldown<=0&&vy==0&&(dir<0&&x+Width/2<a.Left+60||dir>0&&x+Width/2>a.Right-60)){dir=-dir;throwVx=dir*220;vy=-290;escapeJumpCooldown=2;Set(Act.Jump,1);}
  return false;
 }
 void UpdateMobs(float dt){var areas=Screen.AllScreens.Select(s=>s.WorkingArea).ToArray();Vector2? target=expedition.Busy||respawnTime>0?null:new Vector2(x+Width/2,y+Height);mobs.Step(dt,areas,target,data.NaturalMobs,data.Scale,data.OnTop,position=>{
  if(data.Volume>0){Audio.PlayExplosion(data.Volume);soundCooldown=.8f;}
  if(Vector2.Distance(new Vector2(x+Width/2,y+Height-30),position)<210)ApplyMobDamage(position,8,500);
 },zombieHit:position=>ApplyMobDamage(position,2,170),renderRate:data.World.PerformanceMode?12:24,sound:cue=>Sfx(cue));}
}
