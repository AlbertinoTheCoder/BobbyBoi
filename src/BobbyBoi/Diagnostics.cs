using System.Runtime.InteropServices;
using System.Text;
namespace BobbyBoi;
static class Diagnostics {
 delegate bool EnumProc(nint h,nint p);
 [DllImport("user32.dll")]static extern bool EnumWindows(EnumProc p,nint l);
 [DllImport("user32.dll")]static extern uint GetWindowThreadProcessId(nint h,out uint pid);
 [DllImport("user32.dll")]static extern bool IsWindowVisible(nint h);
 [DllImport("user32.dll")]static extern bool GetWindowRect(nint h,out R r);
 [DllImport("user32.dll",CharSet=CharSet.Unicode)]static extern int GetWindowText(nint h,StringBuilder s,int n);
 [DllImport("user32.dll")]static extern nint GetForegroundWindow();
 [DllImport("user32.dll")]static extern nint GetThreadDesktop(uint t);
 [DllImport("kernel32.dll")]static extern uint GetCurrentThreadId();
 [DllImport("user32.dll",CharSet=CharSet.Unicode)]static extern bool GetUserObjectInformation(nint h,int index,StringBuilder s,int size,out int needed);
 [StructLayout(LayoutKind.Sequential)]struct R{public int L,T,Rt,B;public override string ToString()=>$"{L},{T}..{Rt},{B}";}
 public static string Desktop(){var s=new StringBuilder(256);GetUserObjectInformation(GetThreadDesktop(GetCurrentThreadId()),2,s,512,out _);return s.ToString();}
 public static void Run(){var log=new StringBuilder("Desktop: "+Desktop()+"\n");var fg=GetForegroundWindow();EnumWindows((h,l)=>{GetWindowThreadProcessId(h,out var pid);string name;try{name=System.Diagnostics.Process.GetProcessById((int)pid).ProcessName;}catch{return true;}if(name=="BobbyBoi"||h==fg){var s=new StringBuilder(256);GetWindowText(h,s,256);GetWindowRect(h,out var r);log.AppendLine($"{name} pid={pid} hwnd={h} visible={IsWindowVisible(h)} rect={r} foreground={h==fg} title={s}");}return true;},0);log.AppendLine("Fullscreen: "+Native.Fullscreen(0));foreach(var s in Screen.AllScreens)log.AppendLine($"Screen {s.DeviceName} bounds={s.Bounds} work={s.WorkingArea}");Directory.CreateDirectory("artifacts");File.WriteAllText("artifacts/diagnostics.txt",log.ToString());}
}
