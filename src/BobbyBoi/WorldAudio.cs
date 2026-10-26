using System.Diagnostics;
using System.Runtime.InteropServices;
namespace BobbyBoi;
static class WorldAudio {
 internal const int Rate=22050;
 internal static readonly string[] Cues=["step","jump","land","bounce","throw","grab","eat","sleep","wake","dance","chat","menu","hurt","death","respawn","cast","splash","bite","reel","pickup","chest","block","sword","bark","tame","fuse","explosion","confetti","zombie","spawn","magic","xp","achievement","golem","chains","jail","unlock","slot","win","lose","ignite","portal","mystery","vanish","rumble","debris","update","error","camera","record","subscribe","ufo","beam","hiss","dragon"];
 static readonly Dictionary<string,float[]> samples=[];static readonly Dictionary<string,double> last=[];static readonly List<Voice> voices=[];static readonly object gate=new();static readonly Stopwatch clock=Stopwatch.StartNew();static Thread? worker;static bool stopping;internal static bool DeviceOpened{get;private set;}internal static string? DeviceError{get;private set;}
 sealed class Voice(float[] data,float gain,int priority){public readonly float[] Data=data;public readonly float Gain=gain;public readonly int Priority=priority;public int Position;}
 public static void Play(string cue,int volume){volume=Math.Clamp(volume,0,100);if(volume==0)return;try{lock(gate){double now=clock.Elapsed.TotalSeconds;double gap=cue is "step" or "block"?.14:cue is "chat"?.16:.08;if(last.TryGetValue(cue,out double previous)&&now-previous<gap)return;last[cue]=now;int priority=cue is "death" or "explosion"?8:cue is "hurt" or "respawn" or "achievement"?6:cue is "step" or "chat" or "menu"?1:3;if(voices.Count>=8){var quiet=voices.MinBy(v=>v.Priority)!;if(quiet.Priority>priority)return;voices.Remove(quiet);}voices.Add(new(GetSamples(cue),volume/100f*(priority==1?.25f:.68f),priority));if(worker==null){stopping=false;worker=new Thread(Run){IsBackground=true,Name="BobbyBoi SFX mixer"};worker.Start();}}}catch(Exception e){DeviceError=e.Message;}}
 internal static float[] GetSamples(string cue){if(samples.TryGetValue(cue,out var cached))return cached;float seconds=cue switch{"explosion"=>.7f,"death"=>.85f,"respawn" or "achievement" or "win"=>.65f,"portal" or "rumble" or "ufo" or "beam"=>.8f,"slot" or "dance" or "reel"=>.45f,"mystery"=>.55f,"sleep"=>.55f,"chat" or "menu" or "step"=>.075f,_=>.22f};var data=new float[(int)(Rate*seconds)];var rng=new Random(cue.Aggregate(19,(n,c)=>unchecked(n*31+c)));
  for(int i=0;i<data.Length;i++){double t=i/(double)Rate,p=i/(double)data.Length,envelope=Math.Min(1,t/.006)*Math.Pow(1-p,1.7),noise=rng.NextDouble()*2-1;double S(double f)=>Math.Sin(t*f*2*Math.PI);double chirp(double start,double end)=>Math.Sin(2*Math.PI*(start*t+(end-start)*t*t/(2*seconds)));double square(double f)=>Math.Tanh(S(f)*3)*.65;double note(int[] notes,double speed=8)=>S(notes[Math.Min(notes.Length-1,(int)(t*speed))]);double v=cue switch{
   "explosion"=>noise*.65*Math.Exp(-p*4)+chirp(95,28)*.6*Math.Exp(-p*6),
   "fuse" or "hiss"=>noise*.45*(.3+p)*(.7+.3*S(90)),"confetti"=>noise*.25*Math.Exp(-p*9)+note([700,950,1400],16)*.4,
   "death"=>square(330*(1-p)*(.8+.2*Math.Sin(t*45)))*.6+noise*.09,"hurt"=>chirp(260,95)*.5+noise*.25,
   "respawn"=>note([262,330,392,523,659],8)*.6,"achievement" or "xp" or "subscribe"=>note([523,659,784,1047],9)*.55,
   "win"=>note([392,494,587,784],8)*.65,"lose" or "error"=>note([233,196,147],6)*.55,
   "step"=>noise*.27*Math.Exp(-p*9)+S(105)*.2*Math.Exp(-p*7),"block" or "debris"=>noise*.4*Math.Exp(-p*6)+S(130)*.3,
   "land" or "bounce"=>chirp(145,55)*.45+noise*.12,"jump" or "throw" or "cast"=>chirp(cue=="jump"?210:650,cue=="jump"?650:130)*.35+noise*.12,
   "splash"=>noise*.4*(.5+.5*S(120))+chirp(650,220)*.2,"bite"=>note([900,1250],14)*.4+noise*.2*Math.Exp(-p*8),"reel"=>noise*.18+square(260)*(Math.Sin(t*85)>0?.5:.06),
   "eat"=>noise*.4*(Math.Sin(t*85)>0?1:.1),"sleep"=>noise*.13*Math.Sin(p*Math.PI)+S(90+Math.Sin(t*8)*25)*.3,
   "bark"=>square(160)*(Math.Sin(t*50)>0?.6:.08)+noise*.3,"tame"=>note([520,660,880],16)*.5,
   "dragon"=>S(55+Math.Sin(t*21)*17)*.5+noise*.35,
   "zombie"=>S(70+Math.Sin(t*24)*13)*.55+noise*.22,"golem"=>S(72)*.4+S(175)*.25+noise*.2,
   "chains" or "jail" or "unlock"=>S(540)*.3+S(827)*.25+noise*.2*Math.Exp(-p*4),
   "slot"=>note([523,659,784,659,880],12)*.5+noise*.08,"ignite"=>noise*.45*Math.Exp(-p*3)+chirp(140,750)*.25,
   "portal" or "magic"=>S(180+Math.Sin(t*12)*50)*.3+S(365)*.18+noise*.12,
   "mystery"=>S(170)*.23+S(177)*.23,"vanish"=>chirp(800,85)*.4,
   "rumble"=>noise*.45*Math.Exp(-p*1.5)+S(44)*.5,"update" or "record"=>note([440,554,660],14)*.4,
   "camera"=>noise*.5*Math.Exp(-p*15)+square(170)*.2,"ufo"=>S(170+Math.Sin(t*15)*45)*.4+S(340+Math.Sin(t*19)*90)*.2,
   "beam"=>chirp(200,1150)*.3+S(120+Math.Sin(t*26)*35)*.3,
   "dance"=>note([330,392,440,392,262],10)*.35+noise*.13*(Math.Sin(t*36)>0?1:0),
   "chat"=>square(520)*.25,"menu"=>S(740)*.25,"grab" or "wake"=>chirp(180,510)*.35,
   "pickup" or "chest"=>note(cue=="chest"?[170,260,390]:[660,990],14)*.5,_=>S(440)*.3};data[i]=(float)Math.Clamp(v*envelope,-1,1);}
  return samples[cue]=data;
 }
 internal static void Mix(short[] target){lock(gate){for(int i=0;i<target.Length;i++){float sum=0;foreach(var v in voices)if(v.Position<v.Data.Length)sum+=v.Data[v.Position++]*v.Gain;target[i]=(short)(Math.Clamp(sum,-.95f,.95f)*short.MaxValue);}voices.RemoveAll(v=>v.Position>=v.Data.Length);}}
 internal static void VerifyMixer(){lock(gate){voices.Clear();voices.Add(new Voice(Enumerable.Repeat(.8f,16).ToArray(),1,8));voices.Add(new Voice(Enumerable.Repeat(.8f,16).ToArray(),1,8));}var mixed=new short[32];Mix(mixed);if(mixed[0]<30000||mixed[0]>=32767||mixed[20]!=0)throw new Exception("Audio overlap/limiter/tail regression");lock(gate){if(voices.Count!=0)throw new Exception("Completed audio voices leaked");}}
 public static void Silence(){lock(gate){voices.Clear();last.Clear();}}
 public static void Stop(){Thread? old;lock(gate){stopping=true;voices.Clear();old=worker;}if(old!=Thread.CurrentThread)old?.Join(750);}
 [StructLayout(LayoutKind.Sequential,Pack=2)]struct Format{public ushort Tag,Channels;public uint Samples,Bytes;public ushort Align,Bits,Extra;}
 [StructLayout(LayoutKind.Sequential)]struct Header{public nint Data;public uint Length,Recorded;public nuint User;public uint Flags,Loops;public nint Next;public nuint Reserved;}
 [DllImport("winmm.dll")]static extern uint waveOutOpen(out nint device,uint id,ref Format format,nint callback,nint instance,uint flags);
 [DllImport("winmm.dll")]static extern uint waveOutPrepareHeader(nint device,nint header,uint size);
 [DllImport("winmm.dll")]static extern uint waveOutWrite(nint device,nint header,uint size);
 [DllImport("winmm.dll")]static extern uint waveOutReset(nint device);
 [DllImport("winmm.dll")]static extern uint waveOutUnprepareHeader(nint device,nint header,uint size);
 [DllImport("winmm.dll")]static extern uint waveOutClose(nint device);
 static void Run(){nint device=0;var blocks=new List<(nint Data,nint Header)>();uint size=(uint)Marshal.SizeOf<Header>();try{var format=new Format{Tag=1,Channels=1,Samples=Rate,Bytes=Rate*2,Align=2,Bits=16};uint result=waveOutOpen(out device,uint.MaxValue,ref format,0,0,0);if(result!=0)throw new IOException("Audio device error "+result);DeviceOpened=true;var buffer=new short[512];for(int i=0;i<4;i++){nint data=Marshal.AllocHGlobal(buffer.Length*2),header=Marshal.AllocHGlobal((int)size);blocks.Add((data,header));Marshal.StructureToPtr(new Header{Data=data,Length=(uint)buffer.Length*2},header,false);if(waveOutPrepareHeader(device,header,size)!=0)throw new IOException("Audio buffer preparation failed");Mix(buffer);Marshal.Copy(buffer,0,data,buffer.Length);if(waveOutWrite(device,header,size)!=0)throw new IOException("Audio buffer submission failed");}
   while(true){lock(gate){if(stopping)break;}foreach(var b in blocks){var h=Marshal.PtrToStructure<Header>(b.Header);if((h.Flags&1)==0)continue;Mix(buffer);Marshal.Copy(buffer,0,b.Data,buffer.Length);if(waveOutWrite(device,b.Header,size)!=0)throw new IOException("Audio device disconnected");}Thread.Sleep(5);}
  }catch(Exception e){DeviceError=e.Message;}finally{if(device!=0)waveOutReset(device);foreach(var b in blocks){if(device!=0)waveOutUnprepareHeader(device,b.Header,size);Marshal.FreeHGlobal(b.Header);Marshal.FreeHGlobal(b.Data);}if(device!=0)waveOutClose(device);lock(gate){worker=null;}}}
 internal static byte[] Wave(string cue){var pcm=GetSamples(cue);using var m=new MemoryStream();using var w=new BinaryWriter(m);w.Write("RIFF"u8);w.Write(36+pcm.Length*2);w.Write("WAVEfmt "u8);w.Write(16);w.Write((short)1);w.Write((short)1);w.Write(Rate);w.Write(Rate*2);w.Write((short)2);w.Write((short)16);w.Write("data"u8);w.Write(pcm.Length*2);foreach(float p in pcm)w.Write((short)(p*.65f*32767));return m.ToArray();}
}
