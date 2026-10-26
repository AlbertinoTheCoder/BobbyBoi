using System.Numerics;
using System.Runtime.InteropServices;
namespace BobbyBoi;
sealed class PixelDepthBuffer {
 readonly int width,height;readonly int[] colors;readonly float[] depths;
 public PixelDepthBuffer(int width,int height){this.width=width;this.height=height;colors=new int[width*height];depths=new float[width*height];Array.Fill(depths,float.NegativeInfinity);}
 public void Triangle(Vector3 a,Vector3 b,Vector3 c,Color color){float d=(b.Y-c.Y)*(a.X-c.X)+(c.X-b.X)*(a.Y-c.Y);if(Math.Abs(d)<.00001f)return;int x0=Math.Clamp((int)MathF.Floor(Math.Min(a.X,Math.Min(b.X,c.X))),0,width-1),x1=Math.Clamp((int)MathF.Ceiling(Math.Max(a.X,Math.Max(b.X,c.X))),0,width-1);int y0=Math.Clamp((int)MathF.Floor(Math.Min(a.Y,Math.Min(b.Y,c.Y))),0,height-1),y1=Math.Clamp((int)MathF.Ceiling(Math.Max(a.Y,Math.Max(b.Y,c.Y))),0,height-1);
  for(int y=y0;y<=y1;y++)for(int x=x0;x<=x1;x++){float w0=((b.Y-c.Y)*(x+.5f-c.X)+(c.X-b.X)*(y+.5f-c.Y))/d,w1=((c.Y-a.Y)*(x+.5f-c.X)+(a.X-c.X)*(y+.5f-c.Y))/d,w2=1-w0-w1;if(w0<-.00001f||w1<-.00001f||w2<-.00001f)continue;float z=w0*a.Z+w1*b.Z+w2*c.Z;int index=y*width+x;if(z>=depths[index]){depths[index]=z;colors[index]=color.ToArgb();}}
 }
 public Bitmap Image(){var b=new Bitmap(width,height,System.Drawing.Imaging.PixelFormat.Format32bppArgb);var data=b.LockBits(new Rectangle(0,0,width,height),System.Drawing.Imaging.ImageLockMode.WriteOnly,b.PixelFormat);try{Marshal.Copy(colors,0,data.Scan0,colors.Length);}finally{b.UnlockBits(data);}return b;}
}
