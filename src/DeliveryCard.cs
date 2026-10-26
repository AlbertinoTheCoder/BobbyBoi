using System.Diagnostics;
using System.Drawing.Drawing2D;
namespace BobbyBoi;
// A real content window, pulled in by the pet. The saved file is an archive, not an opener icon.
sealed class DeliveryCard:Form {
 readonly MemeDelivery delivery;readonly Bitmap? picture;readonly Control content;
 readonly System.Windows.Forms.Timer animation=new(){Interval=16};readonly System.Diagnostics.Stopwatch clock=new();
 Rectangle startBounds,targetBounds;double settleAge;bool settling,carried=true,suspended;readonly Label footer;
 public DeliveryCard(MemeDelivery delivery,bool showWhileCarrying){
  this.delivery=delivery;Text=delivery.Title+" - BobbyBoi";FormBorderStyle=FormBorderStyle.SizableToolWindow;ShowInTaskbar=false;TopMost=true;StartPosition=FormStartPosition.Manual;Size=new(320,250);MinimumSize=new(240,180);BackColor=Color.FromArgb(30,35,31);ForeColor=Color.Beige;DoubleBuffered=true;
  var footerPanel=new Panel{Dock=DockStyle.Bottom,Height=34,BackColor=Color.FromArgb(38,47,35)};
  footer=new Label{Text="BobbyBoi is bringing this in...",Dock=DockStyle.Fill,TextAlign=ContentAlignment.MiddleLeft,Padding=new Padding(8,0,0,0),Font=new Font("Segoe UI",8)};
  var open=new Button{Text="Open saved copy",Dock=DockStyle.Right,Width=120,FlatStyle=FlatStyle.Flat,Font=new Font("Segoe UI",8)};open.Click+=(_,_)=>{try{Process.Start(OpenInfo(delivery.Path));}catch{footer.Text="Could not open saved copy";}};
  footerPanel.Controls.Add(footer);footerPanel.Controls.Add(open);Controls.Add(footerPanel);
  if(delivery.Path.EndsWith(".png",StringComparison.OrdinalIgnoreCase)){
   try{using var source=Image.FromFile(delivery.Path);picture=new Bitmap(source);}catch{}
   content=new PictureBox{Dock=DockStyle.Fill,Image=picture,SizeMode=PictureBoxSizeMode.Zoom,BackColor=Color.FromArgb(21,25,22)};
  }else{
   string text;try{text=File.ReadAllText(delivery.Path);}catch{text="The document could not be read. Your saved copy remains in BobbyBoi Memes.";}
   content=new RichTextBox{Dock=DockStyle.Fill,ReadOnly=true,Text=text,Font=new Font("Consolas",11),BackColor=Color.FromArgb(247,243,225),ForeColor=Color.FromArgb(37,43,32),BorderStyle=BorderStyle.None,WordWrap=true,DetectUrls=false};
  }
  Controls.Add(content);content.BringToFront();content.Visible=showWhileCarrying;
  animation.Tick+=(_,_)=>{
   double dt=clock.Elapsed.TotalSeconds;clock.Restart();if(!settling)return;settleAge+=dt;float t=Math.Clamp((float)(settleAge/.45),0,1);float smooth=t*t*(3-2*t);
   Bounds=new Rectangle((int)(startBounds.X+(targetBounds.X-startBounds.X)*smooth),(int)(startBounds.Y+(targetBounds.Y-startBounds.Y)*smooth-Math.Sin(t*Math.PI)*12),(int)(startBounds.Width+(targetBounds.Width-startBounds.Width)*smooth),(int)(startBounds.Height+(targetBounds.Height-startBounds.Height)*smooth));
   if(t>=1){settling=false;animation.Stop();Bounds=targetBounds;}
  };
 }
 protected override bool ShowWithoutActivation=>true;
 protected override void Dispose(bool disposing){if(disposing){animation.Dispose();picture?.Dispose();}base.Dispose(disposing);}
 internal static ProcessStartInfo OpenInfo(string path){if(!File.Exists(path)||Path.GetExtension(path).ToLowerInvariant() is not (".png" or ".txt"))throw new IOException("Invalid delivery file");return new ProcessStartInfo(path){UseShellExecute=true};}
 public void Carry(Point hand,float movementDirection){
  if(IsDisposed||!carried)return;int xx=movementDirection>0?hand.X-Width-22:hand.X+22;
  Location=new Point(xx,hand.Y-Height+24);if(!Visible&&!suspended)Show();
 }
 public void Place(Point feet,Rectangle area){
  if(IsDisposed)return;carried=false;content.Visible=true;footer.Text="Delivered by BobbyBoi";startBounds=Bounds;
  int w=Math.Min(620,Math.Max(240,area.Width-40)),h=Math.Min(480,Math.Max(180,area.Height-60));
  int xx=Math.Clamp(Left,area.Left+12,Math.Max(area.Left+12,area.Right-w-12));int yy=Math.Clamp(area.Bottom-h-18,area.Top+12,Math.Max(area.Top+12,area.Bottom-h));
  targetBounds=new Rectangle(xx,yy,w,h);settleAge=0;settling=true;clock.Restart();animation.Start();if(!Visible)Show();
 }
 public void Suspend(){if(IsDisposed||suspended)return;suspended=true;animation.Stop();clock.Stop();Hide();}
 public void Resume(){if(IsDisposed||!suspended)return;suspended=false;Show();clock.Restart();if(settling)animation.Start();}
}
