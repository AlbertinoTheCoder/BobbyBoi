using System.Diagnostics;
namespace BobbyBoi;
static partial class MemeFactory {
 public static void Test(){
  ExpeditionClock.Test();string folder=Path.GetFullPath("artifacts/expedition-tests");Directory.CreateDirectory(folder);
  var samples=new List<string>();var names=new HashSet<string>();
  for(int category=0;category<3;category++){
   int count=category==0?Texts.Length:category==1?Memes.Length:Paints.Length;
   for(int i=0;i<count;i++){
    var d=Generate(category,i,1234+i,folder,CancellationToken.None);
    var open=DeliveryCard.OpenInfo(d.Path);if(open.FileName!=d.Path||!open.UseShellExecute)throw new Exception("Delivery click targets wrong file");
    if(!names.Add(d.Path)||!File.Exists(d.Path))throw new Exception("Missing or overwritten delivery");
    if(category==0){if(File.ReadAllText(d.Path).Length<70)throw new Exception("Empty document");}
    else{using var png=Image.FromFile(d.Path);if(png.Width!=900||png.Height!=640)throw new Exception("Invalid image");if(i<3)samples.Add(d.Path);}
   }
  }
  // Repeating seeds are reproducible, while saving always creates a different file.
  var first=Generate(2,1,99,folder,CancellationToken.None);var second=Generate(2,1,99,folder,CancellationToken.None);
  if(first.Path==second.Path||!File.ReadAllBytes(first.Path).SequenceEqual(File.ReadAllBytes(second.Path)))throw new Exception("Seeded image or unique filenames failed");
  using(var canceled=new CancellationTokenSource()){canceled.Cancel();int before=Directory.GetFiles(folder).Length;try{Generate(0,0,1,folder,canceled.Token);throw new Exception("Cancellation ignored");}catch(OperationCanceledException){}if(Directory.GetFiles(folder).Length!=before)throw new Exception("Canceled generation wrote a file");}
  // Warm GDI, then check repeated actual PNG production does not retain bitmaps/handles.
  Generate(1,2,3,folder,CancellationToken.None);int handles=Process.GetCurrentProcess().HandleCount;
  for(int i=0;i<12;i++)Create(CancellationToken.None,folder,i%3);
  GC.Collect();GC.WaitForPendingFinalizers();int after=Process.GetCurrentProcess().HandleCount;
  if(after-handles>30)throw new Exception("Repeated generation retained excessive handles");
  using(var sheet=new Bitmap(900,640)){using var g=Graphics.FromImage(sheet);for(int i=0;i<samples.Count;i++){using var img=Image.FromFile(samples[i]);g.DrawImage(img,new Rectangle(i%3*300,i/3*320,300,213));}sheet.Save("artifacts/expedition-contact-sheet.png");}
  File.WriteAllText("artifacts/expedition-tests.txt",$"PASS: {Texts.Length} TXT concepts, {Memes.Length} illustrated meme concepts, {Paints.Length} Paint concepts; validated every TXT/PNG.\nPASS: first departure at 20s, preparation, pause, offscreen delay, manual overlap guard, 12 randomized 120-180s cooldowns, disabled scheduler.\nPASS: unique files, correct click launch targets, reproducible Paint drawings, cancellation, 12 consecutive generators; handle delta {after-handles}.\nAll test files: {folder}\n");
 }
}
