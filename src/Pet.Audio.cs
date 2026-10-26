using System.Numerics;
namespace BobbyBoi;
sealed partial class Pet {
 void Sfx(string cue){if(data.Volume<=0||paused||userHidden||hiddenFullscreen||expedition.Busy&&!data.Expeditions.Sounds)return;WorldAudio.Play(cue,data.Volume);}
 void UpdatePetSfx(float dt){float interval=action switch{Act.Eat=>.45f,Act.Sleep=>5,Act.Mine=>.7f,Act.Dance=>1.8f,Act.Climb=>.45f,_=>0};if(interval>0&&(int)(age/interval)!=(int)(Math.Max(0,age-dt)/interval))Tone(action);if(world.Active?.Spec.Kind==WorldEventKind.Dragon&&(int)(world.Active.Age/5)!=(int)(Math.Max(0,world.Active.Age-dt)/5))Sfx("dragon");}
 static readonly Dictionary<WorldEventKind,(float At,string Cue)[]> soundEvents=new(){
 [WorldEventKind.Prison]=[(0,"golem"),(3,"chains"),(5,"golem"),(8,"jail"),(17.8f,"unlock")],
 [WorldEventKind.Casino]=[(0,"slot"),(2,"slot"),(3,"slot"),(4,"slot")],
 [WorldEventKind.Portal]=[(0,"block"),(1,"block"),(2,"block"),(3,"block"),(4,"ignite"),(6,"portal"),(10,"portal")],
 [WorldEventKind.Herobrine]=[(0,"mystery"),(4,"vanish")],
 [WorldEventKind.Earthquake]=[(0,"rumble"),(1,"debris"),(2,"rumble"),(3,"debris"),(4,"land")],
 [WorldEventKind.FakeUpdate]=[(0,"update"),(4,"error"),(8,"error"),(11,"error")],
 [WorldEventKind.AdoptCreeper]=[(0,"hiss"),(2,"tame")],
 [WorldEventKind.YouTube]=[(0,"camera"),(2,"record"),(8,"subscribe")],
 [WorldEventKind.Ufo]=[(0,"ufo"),(3,"beam"),(7,"vanish"),(11,"beam"),(14,"land")],
 [WorldEventKind.Cave]=[(0,"block"),(3,"block"),(4,"block"),(5,"block"),(6,"block"),(7,"block"),(8,"pickup")]
 };
 void StepExpansionSfx(WorldActivity a,float dt){if(soundEvents.TryGetValue(a.Spec.Kind,out var cues))foreach(var cue in cues)if(a.Age>=cue.At&&a.Age-dt<cue.At)Sfx(cue.Cue);if(a.Spec.Kind==WorldEventKind.Casino&&a.Age>=5&&a.Age-dt<5)Sfx(chaosResult==0?"lose":"win");}
}
