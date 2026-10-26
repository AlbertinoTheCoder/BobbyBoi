using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Text;
namespace BobbyBoi;
// Controls only a newly-created Paint/Notepad window opened for this exact generated file.
sealed class NativeDelivery:IDisposable {
 delegate bool EnumProc(nint h,nint p);
 [StructLayout(LayoutKind.Sequential)]struct R{public int L,T,Right,Bottom;}
 [DllImport("user32.dll")]static extern bool EnumWindows(EnumProc fn,nint p);
 [DllImport("user32.dll")]static extern uint GetWindowThreadProcessId(nint h,out uint pid);
 [DllImport("user32.dll",CharSet=CharSet.Unicode)]static extern int GetWindowText(nint h,StringBuilder s,int n);
 [DllImport("user32.dll")]static extern bool GetWindowRect(nint h,out R r);
 [DllImport("user32.dll")]static extern bool IsWindow(nint h);
 [DllImport("user32.dll")]static extern bool SetWindowPos(nint h,nint after,int x,int y,int w,int height,uint flags);
 [DllImport("user32.dll")]static extern bool ShowWindow(nint h,int cmd);
 readonly HashSet<nint> previous=[];readonly string stem,appName;Process? process;readonly Stopwatch launchAge=Stopwatch.StartNew();nint window;bool released,hidden,sized;Point lastHand;float lastDirection;Rectangle area;
 public Rectangle WindowBounds{get{if(!Ready||!GetWindowRect(window,out var r))return Rectangle.Empty;return Rectangle.FromLTRB(r.L,r.T,r.Right,r.Bottom);}}
 public bool Ready=>window!=0&&IsWindow(window);public bool TimedOut=>launchAge.Elapsed.TotalSeconds>10;
 public bool IsCarried=>!released;public string Status=>Ready?"new application window attached":TimedOut?"application reused an existing window; left untouched":"waiting for application";
 public NativeDelivery(MemeDelivery delivery,Rectangle area){this.area=area;stem=Path.GetFileNameWithoutExtension(delivery.Path);bool png=delivery.Path.EndsWith(".png",StringComparison.OrdinalIgnoreCase);appName=png?"mspaint":"notepad";
  EnumWindows((h,_)=>{previous.Add(h);return true;},0);
  string exe=Path.Combine(Environment.SystemDirectory,appName+".exe");if(!File.Exists(exe))exe=Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),"Microsoft","WindowsApps",appName+".exe");var start=new ProcessStartInfo(exe){UseShellExecute=true};start.ArgumentList.Add(delivery.Path);process=Process.Start(start);
 }
 void FindWindow(){if(window!=0||released)return;EnumWindows((h,_)=>{
   if(previous.Contains(h))return true;var title=new StringBuilder(2048);GetWindowText(h,title,title.Capacity);if(!title.ToString().Contains(stem,StringComparison.OrdinalIgnoreCase))return true;
   GetWindowThreadProcessId(h,out var pid);try{using var p=Process.GetProcessById((int)pid);if(!p.ProcessName.Equals(appName,StringComparison.OrdinalIgnoreCase))return true;}catch{return true;}
   window=h;return false;
  },0);
 }
 public void Carry(Point hand,float movementDirection){lastHand=hand;lastDirection=movementDirection;FindWindow();if(released||!Ready||hidden)return;
  if(!sized){SetWindowPos(window,0,hand.X+22,hand.Y-436,620,460,0x0010|0x0040);sized=true;}var bounds=WindowBounds;int w=bounds.Width,h=bounds.Height;
  int xx=movementDirection>0?hand.X-w-22:hand.X+22;SetWindowPos(window,0,xx,hand.Y-h+24,0,0,0x0001|0x0010|0x0040);ShowWindow(window,4);
 }
 public void Drop(Rectangle workingArea){if(released)return;area=Screen.AllScreens.Any(s=>s.WorkingArea==workingArea)?workingArea:Screen.PrimaryScreen!.WorkingArea;FindWindow();if(Ready){int w=Math.Min(900,area.Width-30),h=Math.Min(690,area.Height-45);int x=Math.Clamp(lastHand.X-w/2,area.Left+12,Math.Max(area.Left+12,area.Right-w-12));int y=area.Bottom-h-18;ShowWindow(window,9);SetWindowPos(window,0,x,y,w,h,0x0010|0x0040);}released=true;}
 public void Suspend(){if(released||!Ready||hidden)return;hidden=true;ShowWindow(window,0);}
 public void Resume(){if(released||!hidden)return;hidden=false;Carry(lastHand,lastDirection);}
 public void Dispose(){if(!released)Drop(area);process?.Dispose();}
}
