using System.Numerics;
namespace BobbyBoi;
static class MobTests {
 public static void Run(){
  Directory.CreateDirectory("artifacts");
  foreach(string kind in new[]{"zombie","creeper"}){var m=MobRenderer.Model(kind);if(m.Faces.Length<500||m.Pivots.Count!=6)throw new Exception("Incomplete Blender mob: "+kind);foreach(var f in m.Faces)if(f.Points.Length!=4||!m.Pivots.ContainsKey(f.Part))throw new Exception("Invalid mob mesh part");}
    var depth=new PixelDepthBuffer(12,12);depth.Triangle(new(1,1,2),new(11,1,2),new(1,11,2),Color.Black);depth.Triangle(new(1,1,1),new(11,1,1),new(1,11,1),Color.White);using(var check=depth.Image())if(check.GetPixel(3,3).R!=0)throw new Exception("Depth ordering covers facial details");
  using(var face=new Bitmap(220,240)){using(var g=Graphics.FromImage(face))MobRenderer.Draw(g,"creeper",new(110,230),5,0,0,0);int dark=0;for(int yy=118;yy<127;yy++)for(int xx=95;xx<105;xx++){var c=face.GetPixel(xx,yy);if(c.A>0&&c.R<40&&c.G<50)dark++;}if(dark<75)throw new Exception("Creeper eye pixels missing: "+dark);face.Save("artifacts/creeper-face-check.png");}
  var clock=new MobSpawnClock();if(clock.Step(59.99f)||!clock.Step(.02f))throw new Exception("Natural spawn not at 60s");if(clock.Step(59)||!clock.Step(1))throw new Exception("Repeated spawn timer drift");
  Rectangle[] areas=[new(-1920,398,1920,1032),new(0,0,1920,1032)];using var director=new MobDirector(42);int blasts=0,hits=0;
  for(int i=0;i<1199;i++)director.Step(.05f,areas,null,true,1,true,_=>blasts++,false);
  if(director.Count!=0)throw new Exception("Early group spawn");director.Step(.1f,areas,null,true,1,true,_=>blasts++,false);
  if(director.Count<3||director.Count>5||director.Groups!=1)throw new Exception("Natural group missing");
  for(int i=0;i<450;i++)director.Step(.05f,areas,null,false,1,true,_=>blasts++,false);
  if(blasts<1||director.ExplosionCount!=blasts)throw new Exception("Creeper did not explode exactly once");
  for(int i=0;i<750;i++)director.Step(.05f,areas,null,false,1,true,_=>blasts++,false);
  if(director.Count!=0)throw new Exception("Mob lifetime cleanup failed");
  for(int i=0;i<10;i++)director.Spawn(areas,new(-500,1430));if(director.Count>MobDirector.MaximumMobs)throw new Exception("Manual spawn cap failed");foreach(var m in director.Entities)m.Feet=new(500,1032);director.Step(.01f,areas,null,false,1,true,_=>{},false);var positions=director.Entities.OrderBy(m=>m.Feet.X).ToArray();for(int i=1;i<positions.Length;i++)if(positions[i].Feet.X-positions[i-1].Feet.X<71.9f)throw new Exception("Mob bodies overlap");
  director.Clear();if(director.Count!=0)throw new Exception("Clear mobs failed");
  using(var creep=new MobEntity("creeper",new(100,1032),100)){creep.Step(1,areas,new(130,1032));if(creep.Fuse<=0)throw new Exception("Proximity fuse failed");creep.Step(1.1f,areas,new(130,1032));if(!creep.Dead||!creep.Exploded)throw new Exception("Fuse completion failed");}
  director.Spawn(areas,new(500,1032),"zombie");var contact=director.Entities.Single();contact.Feet=new(500,1032);director.Step(.05f,areas,new(510,1032),false,1,true,_=>{},false,_=>hits++);director.Step(.05f,areas,new(510,1032),false,1,true,_=>{},false,_=>hits++);if(hits!=1)throw new Exception("Zombie attack / cooldown missing");director.Clear();
  using(var zombie=new MobEntity("zombie",new(600,1032),0)){zombie.Step(.1f,areas,new(400,1032));if(zombie.Direction!=-1||zombie.Feet.X>=600)throw new Exception("Zombie chase failed");zombie.Vx=300;zombie.Vy=-220;zombie.Step(.1f,areas,null);if(zombie.Feet.Y>=1032)throw new Exception("Blast knockback failed");}
  using(var sheet=new Bitmap(920,330)){using var g=Graphics.FromImage(sheet);g.Clear(Color.Transparent);MobRenderer.Draw(g,"zombie",new(120,270),5,1,1,0);MobRenderer.Draw(g,"creeper",new(335,270),5,1,-1,0);MobRenderer.Draw(g,"creeper",new(545,270),5,1,1,1.7f);using var explosion=new ExplosionVisual(new(0,0),42);explosion.Step(.25f);var s=g.Save();g.TranslateTransform(420,-40);explosion.Draw(g);g.Restore(s);sheet.Save("artifacts/mobs-preview.png");}
  File.WriteAllText("artifacts/mob-tests.txt","PASS: both embedded Blender meshes; 60-second first and subsequent group timers; 3-5 mobs per group; creeper proximity and timed explosions; zombie pursuit; knockback physics; lifetime cleanup; manual spawn cap; zombie attack cooldown; clear; depth-buffer order and complete creeper eyes; minimum mob separation after identical starting positions.\n");
 }
}
