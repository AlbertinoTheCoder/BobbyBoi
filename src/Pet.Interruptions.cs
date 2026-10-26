using System.Numerics;
namespace BobbyBoi;
sealed partial class Pet {
 WorldActivity? interruptedWorld;Vector2 interruptedAt;Act interruptedPose;string? interruptedItem;double interruptedItemTime;float interruptedPoseAge,interruptGrace;bool interruptedExpedition;Act? interruptedOrdinary;float ordinaryAge,ordinaryDuration;
 void InterruptActivity(){interruptGrace=1.2f;if(world.Busy){interruptedWorld=world.Pause();interruptedAt=new(x,y);interruptedPose=action;interruptedPoseAge=age;interruptedItem=heldItem;interruptedItemTime=Math.Max(0,heldUntil-watch.Elapsed.TotalSeconds);actorInside=false;characterWorldScale=1;heldItem=null;}else if(!expedition.Busy&&interruptedWorld==null&&interruptedOrdinary==null){interruptedOrdinary=action;ordinaryAge=age;ordinaryDuration=duration;}InterruptExpedition();}
 void InterruptExpedition(){if(!expedition.Busy)return;interruptedExpedition=true;interruptGrace=1.2f;nativeDelivery?.Suspend();deliveryCard?.Suspend();}
 void ResumeInterrupted(float dt){interruptGrace=Math.Max(0,interruptGrace-dt);bool gift=heldItem!=null&&watch.Elapsed.TotalSeconds<heldUntil;bool safe=interruptGrace<=0&&!dragging&&!pressed&&!menu.Visible&&!paused&&!userHidden&&respawnTime<=0&&Math.Abs(throwVx)<20&&vy==0&&!gift&&action is not(Act.Sleep or Act.Eat or Act.Celebrate or Act.Dance or Act.Hurt)&&mobs.NearestThreat(new(x+Width/2,y+Height),270)==null;if(!safe)return;
  if(interruptedWorld!=null){var a=interruptedWorld;interruptedWorld=null;if(!data.World.Enabled(a.Spec.Kind)){world.Clear();return;}var delta=new Vector2(x,y)-interruptedAt;a.Start+=delta;a.Target+=delta;chaosAnchor+=delta;if(a.Spec.Kind==WorldEventKind.Ufo)a.Start=new(x,Screen.FromPoint(new((int)x,(int)y)).WorkingArea.Bottom-Height);if(a.Spec.Kind==WorldEventKind.Treasure&&treasure!=null)treasure.Feet+=delta;world.Resume(a);heldItem=interruptedItem;heldUntil=watch.Elapsed.TotalSeconds+interruptedItemTime;action=interruptedPose;age=interruptedPoseAge;duration=999;return;}
  if(interruptedExpedition){interruptedExpedition=false;nativeDelivery?.Resume();deliveryCard?.Resume();action=Act.Walk;age=0;duration=999;return;}
  if(interruptedOrdinary.HasValue){action=interruptedOrdinary.Value;age=ordinaryAge;duration=ordinaryDuration;interruptedOrdinary=null;}
 }
 void ClearInterrupted(){interruptedWorld=null;interruptedOrdinary=null;interruptGrace=0;}
}
