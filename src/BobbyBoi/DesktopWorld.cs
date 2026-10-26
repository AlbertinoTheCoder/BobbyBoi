namespace BobbyBoi;
static class DesktopWorld {
 public static Rectangle Select(Rectangle[] areas,float feetX,float feetY)=>areas.OrderBy(r=>Distance(r,feetX,feetY)).First();
 static float Distance(Rectangle r,float x,float y){float dx=Math.Max(r.Left-x,Math.Max(0,x-(r.Right-1)));float dy=Math.Max(r.Top-y,Math.Max(0,y-(r.Bottom-1)));return dx*dx+dy*dy;}
 public static float Floor(Rectangle[] areas,float feetX,float feetY,bool walking){var current=Select(areas,feetX,feetY);if(!walking)return current.Bottom;foreach(var a in areas)foreach(var b in areas){if(a==b||Math.Abs(a.Right-b.Left)>2)continue;float seam=(a.Right+b.Left)/2f;if(feetX>=seam-65&&feetX<=seam+65){float t=Math.Clamp((feetX-seam+65)/130,0,1);return a.Bottom+(b.Bottom-a.Bottom)*(t*t*(3-2*t));}}return current.Bottom;}
 public static void Test(){Rectangle[] layout=[new(-1920,398,1920,1032),new(0,0,1920,1032)];if(Select(layout,-200,1200)!=layout[0])throw new Exception("Left monitor selection");if(Floor(layout,-100,1000,false)!=1430||Floor(layout,100,1000,false)!=1032)throw new Exception("Per-monitor floors");float before=Floor(layout,-.1f,1032,true),after=Floor(layout,.1f,1032,true);if(Math.Abs(before-after)>2)throw new Exception("Discontinuous walking seam");Rectangle[] vertical=[new(0,0,1920,1080),new(0,-1080,1920,1080)];if(Select(vertical,900,-500)!=vertical[1])throw new Exception("Stacked monitor selection");}
}
