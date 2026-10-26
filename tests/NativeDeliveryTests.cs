namespace BobbyBoi;
static class NativeDeliveryTests {
 public static void Run(){
  var area=Screen.PrimaryScreen!.WorkingArea;var files=new[]{MemeFactory.Create(CancellationToken.None,Path.GetFullPath("artifacts/native-deliveries"),0),MemeFactory.Create(CancellationToken.None,Path.GetFullPath("artifacts/native-deliveries"),1)};
  using var host=new Form{FormBorderStyle=FormBorderStyle.None,ShowInTaskbar=false,StartPosition=FormStartPosition.Manual,Location=new Point(area.Right+500,area.Bottom+500),Size=new Size(1,1)};
  using var timer=new System.Windows.Forms.Timer{Interval=30};NativeDelivery? cargo=null;int index=0;var watch=System.Diagnostics.Stopwatch.StartNew();double attachedAt=-1;var report=new List<string>();
  timer.Tick+=(_,_)=>{
   try{
    if(cargo==null){cargo=new NativeDelivery(files[index],area);watch.Restart();attachedAt=-1;}
    if(cargo.Ready&&attachedAt<0)attachedAt=watch.Elapsed.TotalSeconds;
    double t=attachedAt<0?0:Math.Clamp((watch.Elapsed.TotalSeconds-attachedAt)/2,0,1);
    int handX=(int)(area.Right+650+(area.Right-720-(area.Right+650))*t);cargo.Carry(new Point(handX,area.Bottom-70),-1);
    if(t>=1||cargo.TimedOut){report.Add($"{files[index].Kind}: {cargo.Status}; movedBounds={cargo.WindowBounds}; file={files[index].Path}");cargo.Drop(area);cargo.Dispose();cargo=null;index++;if(index==files.Length){File.WriteAllLines("artifacts/native-delivery-test.txt",report);timer.Stop();host.Close();}}
   }catch(Exception e){File.WriteAllLines("artifacts/native-delivery-test.txt",report.Append(e.ToString()));cargo?.Dispose();timer.Stop();host.Close();}
  };
  host.Shown+=(_,_)=>timer.Start();Application.Run(host);cargo?.Dispose();
 }
}
