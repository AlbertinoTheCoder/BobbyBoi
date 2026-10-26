using System.Runtime.InteropServices;
namespace BobbyBoi;
sealed class PetMenu : ContextMenuStrip {
 public Action? ClickSound;
 readonly System.Windows.Forms.Timer dismissTimer=new(){Interval=15};
 int polls,hookEvents;public string DismissDiagnostics=>$"polls={polls}; hooks={hookEvents}; hook={mouseHook}; foreground={openingForeground}/{GetForegroundWindow()}; armed={armed}; pointer={Cursor.Position}; bounds={Bounds}";bool leftDown,rightDown,middleDown,armed; nint mouseHook,openingForeground; readonly HookProc hookCallback;
 delegate nint HookProc(int code,nint message,nint data);
 [System.Runtime.InteropServices.StructLayout(System.Runtime.InteropServices.LayoutKind.Sequential)]struct HookPoint{public int X,Y;}
 [DllImport("user32.dll")]static extern nint SetWindowsHookEx(int id,HookProc callback,nint module,uint thread);
 [DllImport("user32.dll")]static extern bool UnhookWindowsHookEx(nint hook);
 [DllImport("user32.dll")]static extern nint CallNextHookEx(nint hook,int code,nint message,nint data);
 [DllImport("kernel32.dll",CharSet=CharSet.Unicode)]static extern nint GetModuleHandle(string? name);
 [DllImport("user32.dll")] static extern short GetAsyncKeyState(int key);
 [DllImport("user32.dll")] static extern nint GetForegroundWindow();
 static bool Down(int key)=>(GetAsyncKeyState(key)&0x8000)!=0;
 public PetMenu(){
  hookCallback=ObserveMouse;AutoClose=false;ShowImageMargin=false;ShowCheckMargin=false;
  BackColor=Color.FromArgb(30,35,31);ForeColor=Color.FromArgb(235,235,218);
  Font=new Font("Segoe UI",10);Padding=new Padding(5,5,5,6);Renderer=new PixelMenuRenderer();
  Opened+=(_,_)=>{armed=false;leftDown=Down(1);rightDown=Down(2);middleDown=Down(4);armed=!leftDown&&!rightDown&&!middleDown;openingForeground=GetForegroundWindow();mouseHook=SetWindowsHookEx(14,hookCallback,GetModuleHandle(null),0);dismissTimer.Start();};
  Closed+=(_,_)=>{CloseChildren(this);dismissTimer.Stop();ReleaseHook();};
  dismissTimer.Tick+=(_,_)=>{polls++;
   if(Down(27)){Close();return;}
   short ls=GetAsyncKeyState(1),rs=GetAsyncKeyState(2),ms=GetAsyncKeyState(4);bool l=(ls&0x8000)!=0,r=(rs&0x8000)!=0,m=(ms&0x8000)!=0;
   if(!armed){leftDown=l;rightDown=r;middleDown=m;if(!l&&!r&&!m)armed=true;return;}
   bool click=l&&!leftDown||r&&!rightDown||m&&!middleDown||(ls&1)!=0||(rs&1)!=0||(ms&1)!=0;
   if(click&&!ContainsPointer(this,Cursor.Position))Close();
   leftDown=l;rightDown=r;middleDown=m;
  };
 }
  nint ObserveMouse(int code,nint message,nint data){
  if(code>=0&&Visible){hookEvents++;long msg=message.ToInt64();if(msg is 0x202 or 0x205 or 0x208 or 0x20C)armed=true;
   if(armed&&msg is 0x201 or 0x204 or 0x207 or 0x20B){var p=Marshal.PtrToStructure<HookPoint>(data);DismissIfOutside(new Point(p.X,p.Y));}
  }return CallNextHookEx(mouseHook,code,message,data);
 }
 internal void DismissIfOutside(Point p){if(!ContainsPointer(this,p))try{BeginInvoke(()=>{if(!IsDisposed&&Visible)Close();});}catch(InvalidOperationException){}}
 void ReleaseHook(){if(mouseHook!=0){UnhookWindowsHookEx(mouseHook);mouseHook=0;}}
 static void CloseChildren(ToolStripDropDown parent){foreach(ToolStripItem item in parent.Items)if(item is ToolStripDropDownItem child&&child.HasDropDownItems){CloseChildren(child.DropDown);if(child.DropDown.Visible)child.DropDown.Close();}}
 static bool ContainsPointer(ToolStripDropDown dropdown,Point p){
  if(dropdown.Visible&&dropdown.Bounds.Contains(p))return true;
  foreach(ToolStripItem item in dropdown.Items)if(item is ToolStripDropDownItem child&&child.HasDropDownItems&&child.DropDown.Visible&&ContainsPointer(child.DropDown,p))return true;
  return false;
 }
 public void StyleChildren(){Style(this);ItemClicked+=DismissAction;}
 void DismissAction(object? sender,ToolStripItemClickedEventArgs e){if(!IsDisposed&&e.ClickedItem is ToolStripMenuItem item&&!item.HasDropDownItems){ClickSound?.Invoke();Close();}}
 void Style(ToolStrip strip){
  strip.Renderer=Renderer;strip.BackColor=BackColor;strip.ForeColor=ForeColor;strip.Font=Font;
  foreach(ToolStripItem item in strip.Items){
   if(item is ToolStripMenuItem){item.Padding=new Padding(12,6,20,6);item.Margin=new Padding(1,0,1,0);}
   if(item is ToolStripDropDownItem child&&child.HasDropDownItems){Style(child.DropDown);child.DropDown.AutoClose=false;child.DropDown.ItemClicked+=DismissAction;if(child.DropDown is ToolStripDropDownMenu sub){sub.ShowImageMargin=false;sub.ShowCheckMargin=false;sub.Padding=new Padding(5);}}
  }
 }
 protected override void Dispose(bool disposing){if(disposing){ReleaseHook();dismissTimer.Dispose();}base.Dispose(disposing);}
 sealed class PixelMenuRenderer : ToolStripProfessionalRenderer {
  public PixelMenuRenderer(){RoundedEdges=false;}
  protected override void OnRenderToolStripBackground(ToolStripRenderEventArgs e){e.Graphics.Clear(Color.FromArgb(30,35,31));}
  protected override void OnRenderToolStripBorder(ToolStripRenderEventArgs e){var r=new Rectangle(0,0,e.ToolStrip.Width-1,e.ToolStrip.Height-1);using var pen=new Pen(Color.FromArgb(105,128,85));e.Graphics.DrawRectangle(pen,r);r.Inflate(-2,-2);using var inner=new Pen(Color.FromArgb(49,59,44));e.Graphics.DrawRectangle(inner,r);}
  protected override void OnRenderMenuItemBackground(ToolStripItemRenderEventArgs e){
   if(e.Item.Selected&&e.Item.Enabled){using var fill=new SolidBrush(Color.FromArgb(58,76,49));e.Graphics.FillRectangle(fill,new Rectangle(1,1,e.Item.Width-2,e.Item.Height-2));using var accent=new SolidBrush(Color.FromArgb(229,179,88));e.Graphics.FillRectangle(accent,1,3,3,e.Item.Height-6);}
  }
  protected override void OnRenderItemText(ToolStripItemTextRenderEventArgs e){e.TextColor=e.Item.Enabled?e.Item.ForeColor:Color.FromArgb(152,166,145);base.OnRenderItemText(e);}
  protected override void OnRenderArrow(ToolStripArrowRenderEventArgs e){e.ArrowColor=Color.FromArgb(191,204,172);base.OnRenderArrow(e);}
  protected override void OnRenderSeparator(ToolStripSeparatorRenderEventArgs e){using var pen=new Pen(Color.FromArgb(65,77,58));e.Graphics.DrawLine(pen,12,e.Item.Height/2,e.Item.Width-12,e.Item.Height/2);}
  protected override void OnRenderImageMargin(ToolStripRenderEventArgs e){}
 }
}
