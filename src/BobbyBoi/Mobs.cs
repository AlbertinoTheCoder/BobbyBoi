using System.Numerics;
namespace BobbyBoi;
sealed class MobEntity:IDisposable {
 public readonly string Kind;public Vector2 Feet;public int Health=10;public float AttackCooldown;public float Age,Fuse,Direction=1,Vx,Vy;public bool Dead,Exploded;public readonly float AutoFuseAt;public readonly EffectOverlay Window=new(new Size(220,240));
 public MobEntity(string kind,Vector2 feet,float fuseAt){Kind=kind;Feet=feet;AutoFuseAt=fuseAt;}
 public void Step(float dt,Rectangle[] screens,Vector2? target){
  if(Dead)return;Age+=dt;AttackCooldown=Math.Max(0,AttackCooldown-dt);if(Age>55){Dead=true;return;}
  float distance=target.HasValue?Vector2.Distance(Feet,target.Value):float.MaxValue;
  if(target.HasValue&&distance<650&&Math.Abs(target.Value.X-Feet.X)>25)Direction=MathF.Sign(target.Value.X-Feet.X);
  if(Kind=="creeper"&&(Fuse>0||distance<115||Age>=AutoFuseAt)){Fuse+=dt;if(Fuse>=2){Dead=true;Exploded=true;return;}}
  Feet.X+=((Fuse>0?0:Direction*(Kind=="zombie"?25:34))+Vx)*dt;Vx*=MathF.Exp(-3*dt);
  float floor=DesktopWorld.Floor(screens,Feet.X,Feet.Y,Math.Abs(Vx)<20);
  if(Feet.Y<floor||Vy<0){Vy+=650*dt;Feet.Y+=Vy*dt;if(Feet.Y>=floor){Feet.Y=floor;Vy=0;}}else Feet.Y=floor;
  float left=screens.Min(r=>r.Left)+35,right=screens.Max(r=>r.Right)-35;
  if(Feet.X<left){Feet.X=left;Direction=1;Vx=Math.Abs(Vx)*.4f;}if(Feet.X>right){Feet.X=right;Direction=-1;Vx=-Math.Abs(Vx)*.4f;}
 }
 public void Render(float scale,bool onTop){if(Dead)return;var bitmap=new Bitmap(220,240,System.Drawing.Imaging.PixelFormat.Format32bppArgb);using(var g=Graphics.FromImage(bitmap)){g.Clear(Color.Transparent);MobSprites.Draw(g,Kind,4*scale,Age,Direction,Fuse);}Window.Present(new Point((int)Feet.X-110,(int)Feet.Y-240),bitmap,onTop);}
 public void Dispose()=>Window.Dispose();
}
sealed class MobSpawnClock {
 public float Remaining{get;private set;}=60;
 public bool Step(float dt){Remaining-=dt;if(Remaining>0)return false;Remaining=60;return true;}
 public void Reset()=>Remaining=60;
}
sealed class MobDirector:IDisposable {
 readonly List<MobEntity> mobs=[];readonly List<ExplosionVisual> explosions=[];readonly Random random;readonly MobSpawnClock clock=new();float renderAge;bool suspended;
 internal IEnumerable<MobEntity> Entities=>mobs;public int Count=>mobs.Count;public int ExplosionCount{get;private set;}public int Groups{get;private set;}
 public const int MaximumMobs=8;
 public MobDirector(int? seed=null){random=seed.HasValue?new Random(seed.Value):new Random();}
 public void Spawn(Rectangle[] areas,Vector2 near,string? kind=null,float scale=1){
  int count=kind==null?random.Next(3,6):1;var area=DesktopWorld.Select(areas,near.X,near.Y);
  for(int i=0;i<count&&mobs.Count<MaximumMobs;i++){
   string type=kind??(i==0?"creeper":i==1?"zombie":random.Next(2)==0?"zombie":"creeper");
   float spacing=72*scale;float? chosen=null;
   for(int attempt=0;attempt<50;attempt++){float candidate=Math.Clamp(near.X+(i%2==0?-1:1)*random.Next(170,520),area.Left+40,Math.Max(area.Left+40,area.Right-40));if(mobs.All(m=>Math.Abs(m.Feet.Y-area.Bottom)>60*scale||Math.Abs(m.Feet.X-candidate)>=spacing)){chosen=candidate;break;}}
   if(chosen==null)for(float candidate=area.Left+40;candidate<=area.Right-40;candidate+=spacing){if(mobs.All(m=>Math.Abs(m.Feet.Y-area.Bottom)>60*scale||Math.Abs(m.Feet.X-candidate)>=spacing)){chosen=candidate;break;}}
   if(chosen==null)break;float xx=chosen.Value;
   mobs.Add(new MobEntity(type,new(xx,area.Bottom),random.Next(10,19)){Direction=xx<near.X?1:-1});
  }if(kind==null)Groups++;
 }
 public void Step(float dt,Rectangle[] areas,Vector2? target,bool natural,float scale,bool onTop,Action<Vector2> blast,bool render=true,Action<Vector2>? zombieHit=null,float renderRate=24,Action<string>? sound=null){
  suspended=false;if(natural&&clock.Step(dt))Spawn(areas,target??new Vector2(areas[0].Left+areas[0].Width/2,areas[0].Bottom),scale:scale);
  foreach(var mob in mobs){float fuse=mob.Fuse;mob.Step(dt,areas,target);if(fuse==0&&mob.Fuse>0)sound?.Invoke("fuse");}Separate(areas,72*scale);
  if(target.HasValue)foreach(var zombie in mobs.Where(m=>!m.Dead&&m.Kind=="zombie"&&m.AttackCooldown<=0&&Vector2.Distance(m.Feet,target.Value)<76)){zombie.AttackCooldown=1.25f;sound?.Invoke("zombie");zombieHit?.Invoke(zombie.Feet);}
  foreach(var mob in mobs.Where(m=>m.Exploded).ToArray()){
   mob.Exploded=false;ExplosionCount++;Vector2 center=mob.Feet-new Vector2(0,48*scale);explosions.Add(new ExplosionVisual(center,random.Next()));blast(center);
   foreach(var other in mobs.Where(m=>!m.Dead&&Vector2.Distance(m.Feet,center)<190)){
    float direction=other.Feet.X<center.X?-1:1;other.Vx=direction*340;other.Vy=-220;
    if(other.Kind=="creeper")other.Fuse=Math.Max(other.Fuse,1.4f);else if(Vector2.Distance(other.Feet,center)<85)other.Dead=true;
   }
  }
  foreach(var dead in mobs.Where(m=>m.Dead).ToArray()){dead.Dispose();mobs.Remove(dead);}
  foreach(var effect in explosions.ToArray()){effect.Step(dt);if(effect.Finished){effect.Dispose();explosions.Remove(effect);}}
  renderAge+=dt;if(!render||renderAge<1f/renderRate)return;renderAge=0;
  foreach(var mob in mobs)mob.Render(scale,onTop);foreach(var effect in explosions)effect.Render(onTop);
 }
  void Separate(Rectangle[] areas,float spacing){
  // Position projection is deterministic, so chasing mobs cannot collapse into a single sprite.
  float left=areas.Min(r=>r.Left)+35,right=areas.Max(r=>r.Right)-35;
  var sorted=mobs.Where(m=>!m.Dead).OrderBy(m=>m.Feet.X).ToArray();
  for(int pass=0;pass<12;pass++){
   for(int i=1;i<sorted.Length;i++)for(int j=0;j<i;j++)if(Math.Abs(sorted[i].Feet.Y-sorted[j].Feet.Y)<spacing*.85f&&sorted[i].Feet.X-sorted[j].Feet.X<spacing)sorted[i].Feet.X=sorted[j].Feet.X+spacing;
   for(int i=sorted.Length-1;i>=0;i--){sorted[i].Feet.X=Math.Min(right,sorted[i].Feet.X);for(int j=i+1;j<sorted.Length;j++)if(Math.Abs(sorted[i].Feet.Y-sorted[j].Feet.Y)<spacing*.85f&&sorted[j].Feet.X-sorted[i].Feet.X<spacing)sorted[i].Feet.X=sorted[j].Feet.X-spacing;sorted[i].Feet.X=Math.Max(left,sorted[i].Feet.X);}
  }
 }
 public void Suspend(){if(suspended)return;suspended=true;foreach(var m in mobs)m.Window.Hide();foreach(var e in explosions)e.Hide();}
 public Vector2? NearestThreat(Vector2 position,float range)=>mobs.Where(m=>!m.Dead&&Vector2.Distance(m.Feet,position)<range).OrderBy(m=>Vector2.DistanceSquared(m.Feet,position)).Select(m=>(Vector2?)m.Feet).FirstOrDefault();
 public void Strike(Vector2 position,int damage){var m=mobs.Where(m=>!m.Dead&&Vector2.Distance(m.Feet,position)<110).OrderBy(m=>Vector2.DistanceSquared(m.Feet,position)).FirstOrDefault();if(m==null)return;m.Health-=damage;m.Vx=(m.Feet.X<position.X?-1:1)*80;if(m.Health<=0)m.Dead=true;}
 public void Detonate(){foreach(var m in mobs.Where(m=>m.Kind=="creeper"&&!m.Dead))m.Fuse=Math.Max(1.8f,m.Fuse);}
 public void Clear(){foreach(var m in mobs)m.Dispose();mobs.Clear();foreach(var e in explosions)e.Dispose();explosions.Clear();clock.Reset();}
 public void Dispose()=>Clear();
}
