namespace BobbyBoi;
public enum ExpeditionState { Idle, Preparing, Exiting, Offscreen, Returning, Delivering, Cooldown }
sealed class ExpeditionOptions {
 public bool ExternalWindows{get;set;}=true; public bool Enabled{get;set;}=true;
 public bool Sounds{get;set;}=true;
 public bool AutoPaint{get;set;}
 public bool Previews{get;set;}=true;
 public int MinSeconds{get;set;}=120;
 public int MaxSeconds{get;set;}=180;
 public int SessionLimit{get;set;}
 public void Normalize(){MinSeconds=Math.Clamp(MinSeconds,30,3600);MaxSeconds=Math.Clamp(MaxSeconds,MinSeconds,3600);SessionLimit=Math.Clamp(SessionLimit,0,999);}
}
sealed class ExpeditionClock {
 readonly Random random; public ExpeditionState State{get;private set;}=ExpeditionState.Idle;
 public double Remaining{get;private set;}=20; public double Age{get;private set;}
 public int Delivered{get;private set;} public bool Requested{get;private set;}
 public ExpeditionClock(Random? random=null){this.random=random??new Random();}
 public bool Busy=>State is not (ExpeditionState.Idle or ExpeditionState.Cooldown);
 public void Request(){if(!Busy)Requested=true;}
 public bool Tick(double dt,bool safe,ExpeditionOptions options){
  if(!options.Enabled)return false;
  if(!safe&&State!=ExpeditionState.Idle)return false;
  Age+=dt;
  if(State==ExpeditionState.Cooldown){State=ExpeditionState.Idle;Age=0;}
  if(State==ExpeditionState.Idle){if(options.SessionLimit>0&&Delivered>=options.SessionLimit){Requested=false;return false;}Remaining=Math.Max(0,Remaining-dt);if(safe&&(Remaining<=0||Requested)){Requested=false;Go(ExpeditionState.Preparing);return true;}}
  else if(State==ExpeditionState.Preparing&&Age>=1.8)Go(ExpeditionState.Exiting);
  return false;
 }
 public void Go(ExpeditionState state){State=state;Age=0;}
 public void Finish(ExpeditionOptions options,bool delivered=true){if(delivered)Delivered++;Remaining=random.Next(options.MinSeconds,options.MaxSeconds+1);Go(ExpeditionState.Cooldown);}
 public void Cancel(){Requested=false;Remaining=20;Go(ExpeditionState.Idle);}
 public static void Test(){var o=new ExpeditionOptions();var c=new ExpeditionClock(new Random(7));for(int i=0;i<199;i++)c.Tick(.1,true,o);if(c.Busy)throw new Exception("First expedition early");c.Tick(.11,true,o);if(c.State!=ExpeditionState.Preparing)throw new Exception("First expedition timing");c.Tick(10,false,o);if(c.Age!=0)throw new Exception("Pause advanced expedition");c.Tick(1.81,true,o);if(c.State!=ExpeditionState.Exiting)throw new Exception("Preparation transition");c.Go(ExpeditionState.Offscreen);c.Tick(4.99,true,o);if(c.Age>=5)throw new Exception("Offscreen delay");c.Request();if(c.Requested)throw new Exception("Overlapping expedition");var intervals=new HashSet<double>();for(int i=0;i<12;i++){c.Finish(o);intervals.Add(c.Remaining);if(c.Remaining<120||c.Remaining>180)throw new Exception("Cooldown bounds");}if(intervals.Count<3)throw new Exception("Cooldown lacks randomness");c.Cancel();o.Enabled=false;c.Request();c.Tick(100,true,o);if(c.Busy)throw new Exception("Disabled scheduler started");}
}
