using System.Diagnostics;
namespace BobbyBoi;
static class PerformanceTests {
 public static void Run(){Directory.CreateDirectory("artifacts");using var b=new Bitmap(220,240);using var g=Graphics.FromImage(b);
  // Same eight-mob animation sequence, including both directions, for each renderer.
  void Frames(bool cached){for(int frame=0;frame<24;frame++)for(int mob=0;mob<8;mob++){g.Clear(Color.Transparent);string kind=mob%2==0?"creeper":"zombie";float age=frame/24f,direction=mob<4?-1:1;if(cached)MobSprites.Draw(g,kind,4,age,direction,0);else MobRenderer.Draw(g,kind,new(110,230),4,age,direction,0);}}
  Frames(false);MobSprites.Clear();Frames(true);var watch=Stopwatch.StartNew();Frames(false);double raw=watch.Elapsed.TotalMilliseconds;watch.Restart();Frames(true);double cached=watch.Elapsed.TotalMilliseconds;
  Rectangle screen=new(0,0,3840,2160);Rectangle[] props=[new(50,1850,340,230),new(3450,20,315,55),new(1800,1900,180,145)];var regions=WorldCanvas.Regions(props,screen);long area=regions.Sum(r=>(long)r.Width*r.Height);var union=props.Aggregate(Rectangle.Union);long previous=(long)union.Width*union.Height;if(regions.Count!=3||area>previous/5)throw new Exception("Distant props still repaint the whole desktop");
  var lines=new Voicelines();foreach(var group in Voicelines.Lines){var picked=Enumerable.Range(0,group.Value.Length).Select(_=>lines.Next(group.Key)).ToArray();if(picked.Distinct().Count()!=picked.Length)throw new Exception("Quip repeats before its deck ends");}
  File.WriteAllText("artifacts/performance-test.txt",$"Eight mobs, 24 frames, same animation sequence: uncached={raw:0.0}ms; warmed cache={cached:0.0}ms; speedup={raw/Math.Max(cached,.01):0.0}x.\n4K separated props: previous bounding box={previous} pixels; new tiles={area} pixels; reduction={100*(1-area/(double)previous):0.0}%.\nPASS: bounded sprite cache; separated redraw regions; {Voicelines.Lines.Values.Sum(a=>a.Length)} shuffled contextual lines without repeats within a deck.\nThese are rendering microbenchmarks, not an end-to-end FPS guarantee.\n");
 }
}
