using System.Numerics;
namespace BobbyBoi;
static class ThrowPhysics {
 public static float Facing(float horizontalVelocity,float current)=>Math.Abs(horizontalVelocity)>.01f?MathF.CopySign(.95f,horizontalVelocity):current;
 public static Vector2 Release(Vector2 velocity,float sinceMove)=>sinceMove>.12f?Vector2.Zero:Vector2.Clamp(velocity,new(-1400,-1100),new(1400,1100));
 public static float Damp(float velocity,float dt,bool grounded)=>velocity*MathF.Exp(-(grounded?5f:.25f)*dt);
 public static bool SettleAtFloor(ref float y,ref float velocity,float floor){if(y<floor||velocity<0)return false;y=floor;velocity=0;return true;}
 public static void Test(){
  float floorY=500,fallSpeed=300;if(!SettleAtFloor(ref floorY,ref fallSpeed,500)||fallSpeed!=0)throw new Exception("Floor release did not stop falling");floorY=520;fallSpeed=-200;if(SettleAtFloor(ref floorY,ref fallSpeed,500))throw new Exception("Upward throw incorrectly landed");
  foreach(float velocity in new[]{-240f,-130f,-1f,1f,130f,240f})if(Math.Sign(Facing(velocity,-Math.Sign(velocity)*.95f))!=Math.Sign(velocity))throw new Exception("Walking / expedition facing regression");
  if(Facing(0,.95f)!=.95f)throw new Exception("Idle facing changed");
  if(Release(new(800,-600),.02f)!=new Vector2(800,-600))throw new Exception("Throw lost release momentum");
  if(Release(new(800,-600),.2f)!=Vector2.Zero)throw new Exception("Stationary drop retains stale momentum");
  if(Release(new(9000,-9000),0)!=new Vector2(1400,-1100))throw new Exception("Throw exceeds velocity limits");
  float one=Damp(800,1,true),steps=800;for(int i=0;i<100;i++)steps=Damp(steps,.01f,true);
  if(Math.Abs(one-steps)>.01f||Damp(800,1,false)<=one)throw new Exception("Throw friction depends on frame rate");
 }
}
