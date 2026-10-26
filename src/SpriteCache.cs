namespace BobbyBoi;
// Bounded by bytes, not frame count: larger display scales cannot grow memory indefinitely.
sealed class SpriteCache(long budget):IDisposable {
 readonly Dictionary<string,LinkedListNode<(string Key,Bitmap Image)>> entries=[];
 readonly LinkedList<(string Key,Bitmap Image)> recent=[];
 long bytes;
 public Bitmap? Get(string key){if(!entries.TryGetValue(key,out var n))return null;recent.Remove(n);recent.AddFirst(n);return n.Value.Image;}
 public void Add(string key,Bitmap image){long size=(long)image.Width*image.Height*4;if(entries.ContainsKey(key)){image.Dispose();return;}while(bytes+size>budget&&recent.Last is {} old){recent.RemoveLast();entries.Remove(old.Value.Key);bytes-=(long)old.Value.Image.Width*old.Value.Image.Height*4;old.Value.Image.Dispose();}var node=recent.AddFirst((key,image));entries[key]=node;bytes+=size;}
 public void Dispose(){foreach(var v in recent)v.Image.Dispose();recent.Clear();entries.Clear();bytes=0;}
}
static class MobSprites {
 static readonly SpriteCache frames=new(16*1024*1024);
 public static void Draw(Graphics g,string kind,float scale,float age,float direction,float fuse){
  int leg=(int)MathF.Round(MathF.Sin(age*7)*8),arm=(int)MathF.Round(MathF.Sin(age*2)*2);
  int swell=fuse>0?(int)MathF.Round(MathF.Sin(fuse*24)*2):0;
  bool flash=fuse>0&&MathF.Sin(fuse*(12+fuse*9))>.25f;
  string key=$"{kind}/{scale}/{direction}/{leg}/{arm}/{swell}/{flash}";
  var image=frames.Get(key);if(image==null){image=new Bitmap(220,240);using(var canvas=Graphics.FromImage(image))MobRenderer.Draw(canvas,kind,new(110,230),scale,age,direction,fuse);frames.Add(key,image);}g.DrawImageUnscaled(image,0,0);
 }
 public static void Clear()=>frames.Dispose();
}
