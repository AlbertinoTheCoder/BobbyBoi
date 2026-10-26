using System.Runtime.InteropServices;
namespace BobbyBoi;
static class IconRefresh {
 [DllImport("shell32.dll",CharSet=CharSet.Unicode)]static extern void SHChangeNotify(uint change,uint flags,string? item,nint other);
 public static void Notify(string path){SHChangeNotify(0x2000,0x1005,path,0);SHChangeNotify(0x08000000,0x1000,null,0);}
}
