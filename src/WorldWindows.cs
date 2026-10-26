using System.Runtime.InteropServices;
using System.Text;
namespace BobbyBoi;
record WindowPlatform(nint Handle,Rectangle Bounds);
static class WindowGeometry {
 delegate bool EnumProc(nint h,nint p);
 [StructLayout(LayoutKind.Sequential)]struct R{public int L,T,Right,Bottom;}
 [DllImport("user32.dll")]static extern bool EnumWindows(EnumProc f,nint l);
 [DllImport("user32.dll")]static extern bool IsWindowVisible(nint h);
 [DllImport("user32.dll")]static extern bool IsIconic(nint h);
 [DllImport("user32.dll")]static extern bool GetWindowRect(nint h,out R r);
 [DllImport("user32.dll")]static extern uint GetWindowThreadProcessId(nint h,out uint pid);
 [DllImport("dwmapi.dll")]static extern int DwmGetWindowAttribute(nint h,int attr,out R value,int size);
 [DllImport("dwmapi.dll",EntryPoint="DwmGetWindowAttribute")]static extern int DwmInt(nint h,int attr,out int value,int size);
 [DllImport("user32.dll",CharSet=CharSet.Unicode)]static extern int GetClassName(nint h,StringBuilder s,int n);
 static DateTime updated;static List<WindowPlatform> cached=[];
 public static WindowPlatform? Read(nint h){if(h==0||!IsWindowVisible(h)||IsIconic(h))return null;GetWindowThreadProcessId(h,out var pid);if(pid==Environment.ProcessId)return null;var name=new StringBuilder(256);GetClassName(h,name,256);if(name.ToString() is "Progman" or "WorkerW" or "Shell_TrayWnd" or "tooltips_class32" or "#32768")return null;try{if(DwmInt(h,14,out var cloaked,4)==0&&cloaked!=0)return null;}catch{}R r;try{if(DwmGetWindowAttribute(h,9,out r,Marshal.SizeOf<R>())!=0&&!GetWindowRect(h,out r))return null;}catch{if(!GetWindowRect(h,out r))return null;}var bounds=Rectangle.FromLTRB(r.L,r.T,r.Right,r.Bottom);if(bounds.Width<180||bounds.Height<100)return null;var area=Screen.FromRectangle(bounds).WorkingArea;if(bounds.Top<area.Top+75||bounds.Top>area.Bottom-80)return null;return new(h,bounds);}
 public static IReadOnlyList<WindowPlatform> Platforms(){if((DateTime.UtcNow-updated).TotalSeconds<1)return cached;updated=DateTime.UtcNow;var list=new List<WindowPlatform>();EnumWindows((h,_)=>{var p=Read(h);if(p!=null&&!list.Any(front=>front.Bounds.Contains(p.Bounds.Left+40,p.Bounds.Top)))list.Add(p);return true;},0);cached=list.Take(20).ToList();return cached;}
}
static class CursorPrank {
 [DllImport("user32.dll")]static extern nint OpenInputDesktop(uint flags,bool inherit,uint access);
 [DllImport("user32.dll")]static extern bool CloseDesktop(nint h);
 [DllImport("user32.dll",CharSet=CharSet.Unicode)]static extern bool GetUserObjectInformation(nint h,int index,StringBuilder name,int size,out int needed);
 [DllImport("user32.dll")]static extern bool SetCursorPos(int x,int y);
 [DllImport("user32.dll")]static extern nint GetForegroundWindow();
 [DllImport("user32.dll",CharSet=CharSet.Unicode)]static extern int GetClassName(nint h,StringBuilder s,int n);
 [DllImport("user32.dll")]static extern short GetAsyncKeyState(int key);
 public static bool Escape=>(GetAsyncKeyState(27)&0x8000)!=0;
 public static bool Emergency=>(GetAsyncKeyState(17)&0x8000)!=0&&(GetAsyncKeyState(18)&0x8000)!=0&&(GetAsyncKeyState(8)&0x8000)!=0;
 public static bool SafeDesktop(){if(Diagnostics.Desktop()!="Default")return false;var input=OpenInputDesktop(0,false,1);if(input==0)return false;try{var desktop=new StringBuilder(128);if(!GetUserObjectInformation(input,2,desktop,256,out _)||desktop.ToString()!="Default")return false;}finally{CloseDesktop(input);}var name=new StringBuilder(128);GetClassName(GetForegroundWindow(),name,128);return name.ToString() is "Progman" or "WorkerW";}
 public static bool Move(Point point,bool enabled){if(!enabled||!SafeDesktop()||Escape||Emergency)return false;return SetCursorPos(point.X,point.Y);}
}
