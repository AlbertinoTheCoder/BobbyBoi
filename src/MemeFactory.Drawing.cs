using System.Drawing.Drawing2D;
namespace BobbyBoi;
static partial class MemeFactory {
 sealed class Painter:IDisposable {
  public readonly Graphics G;public readonly Random R;readonly Font caption=new("Impact",34),label=new("Consolas",16,FontStyle.Bold),hand=new("Comic Sans MS",22);
  public Painter(Bitmap b,int seed){G=Graphics.FromImage(b);R=new Random(seed);G.SmoothingMode=SmoothingMode.None;G.TextRenderingHint=System.Drawing.Text.TextRenderingHint.AntiAliasGridFit;}
  public void Rect(Color c,float x,float y,float w,float h){using var b=new SolidBrush(c);G.FillRectangle(b,x,y,w,h);}
  public void Line(Color c,float x,float y,float xx,float yy,float width=5){using var p=new Pen(c,width);G.DrawLine(p,x,y,xx,yy);}
  public void Ellipse(Color c,float x,float y,float w,float h){using var b=new SolidBrush(c);G.FillEllipse(b,x,y,w,h);}
  public void Text(string s,float x,float y,float w=500,bool handwriting=false){using var b=new SolidBrush(Color.FromArgb(30,32,29));G.DrawString(s,handwriting?hand:label,b,new RectangleF(x,y,w,85));}
  public void Cap(string s,float y,float height){using var f=new StringFormat{Alignment=StringAlignment.Center,LineAlignment=StringAlignment.Center};G.DrawString(s,caption,Brushes.White,new RectangleF(24,y,852,height),f);}
  public void Block(float x,float y,Color? c=null,float size=90){var color=c??Color.SaddleBrown;Rect(color,x,y,size,size);Rect(Color.FromArgb(86,132,53),x,y,size,16);for(int i=0;i<10;i++)Rect(Color.FromArgb(118+i%3*12,79,40),x+R.Next((int)Math.Max(1,size-10)),y+18+R.Next((int)Math.Max(1,size-28)),8,8);}
  public void Gem(float x,float y,float size=30){using var b=new SolidBrush(Color.Turquoise);G.FillPolygon(b,new PointF[]{new(x,y-size),new(x+size,y),new(x,y+size),new(x-size,y)});Line(Color.PaleTurquoise,x,y-size,x+size,y,3);}
  public void Bed(float x,float y){Rect(Color.SaddleBrown,x,y+30,140,35);Rect(Color.IndianRed,x,y,140,35);Rect(Color.Beige,x,y,32,30);Rect(Color.SaddleBrown,x,y+60,12,20);Rect(Color.SaddleBrown,x+126,y+60,12,20);}
  public void Table(float x,float y){Rect(Color.Peru,x,y,170,38);Rect(Color.SaddleBrown,x+8,y+38,16,90);Rect(Color.SaddleBrown,x+148,y+38,16,90);for(int i=0;i<3;i++)for(int j=0;j<3;j++)Rect(Color.FromArgb(95,58,28),x+47+i*24,y+5+j*9,18,5);}
  public void Paper(float x,float y,string text="BILL",float w=105){Rect(Color.FromArgb(45,49,44),x-4,y-4,w+8,110);Rect(Color.Beige,x,y,w,102);Text(text,x+8,y+10,w-10);for(int i=0;i<3;i++)Line(Color.Gray,x+10,y+55+i*13,x+w-10,y+55+i*13,2);}
  public void House(float x,float y){Block(x,y,null,175);Block(x+175,y,null,175);Rect(Color.SaddleBrown,x+20,y-20,310,25);Rect(Color.FromArgb(60,44,27),x+145,y+70,65,105);Rect(Color.LightSkyBlue,x+38,y+48,58,58);Rect(Color.LightSkyBlue,x+245,y+48,58,58);}
  public void Actor(string type,float x,float y,float scale=1,bool suit=false){var state=G.Save();G.TranslateTransform(x,y);G.ScaleTransform(scale,scale);Color skin=Color.FromArgb(191,157,120);bool creeper=type=="creeper",ender=type=="enderman",golem=type=="golem",villager=type=="villager",zombie=type=="zombie",skeleton=type=="skeleton";
   if(type=="chicken"||type=="bee"||type=="slime"||type=="axolotl"||type=="pig"||type=="cow"||type=="spider"){
    Color body=type switch{"bee"=>Color.Goldenrod,"slime"=>Color.YellowGreen,"axolotl"=>Color.Pink,"pig"=>Color.LightPink,"cow"=>Color.White,"spider"=>Color.FromArgb(52,38,37),_=>Color.White};
    Rect(body,-55,-90,110,70);Rect(body,-48,-135,62,65);Rect(Color.Black,-38,-118,12,12);Rect(Color.Black,-8,-118,12,12);
    if(type=="chicken"){Rect(Color.Goldenrod,-18,-98,50,16);Rect(Color.Red,-8,-82,14,24);Line(Color.Orange,-25,-20,-28,0);Line(Color.Orange,28,-20,30,0);}
    if(type=="cow"){Rect(Color.Black,-30,-70,25,30);Rect(Color.Black,25,-85,25,25);Rect(Color.Pink,-46,-93,58,18);Rect(Color.Silver,-45,-147,12,15);Rect(Color.Silver,-5,-147,12,15);}
    if(type=="bee"){for(int i=0;i<3;i++)Rect(Color.FromArgb(60,45,25),-30+i*28,-87,12,64);Ellipse(Color.LightCyan,-30,-125,65,30);}
    if(type=="spider")for(int i=0;i<4;i++){Line(Color.Black,-42,-70+i*12,-95,-90+i*30,7);Line(Color.Black,42,-70+i*12,95,-90+i*30,7);}
    if(type=="pig"){Rect(Color.HotPink,-38,-98,40,20);Rect(Color.Black,-31,-94,6,6);Rect(Color.Black,-15,-94,6,6);}
    if(type=="axolotl")for(int i=-1;i<=1;i++){Rect(Color.HotPink,-65,-124+i*17,18,8);Rect(Color.HotPink,10,-124+i*17,18,8);}
    if(type=="slime")Rect(Color.DarkGreen,-28,-65,45,12);
    if(type is "cow" or "pig")for(int i=0;i<4;i++)Rect(body,-47+i*28,-22,14,25);
   }else{
    Color body=creeper?Color.FromArgb(83,163,62):ender?Color.FromArgb(31,22,37):golem?Color.FromArgb(191,194,173):villager?Color.FromArgb(125,78,48):zombie?Color.SeaGreen:skeleton?Color.LightGray:Color.FromArgb(79,141,118);
    if(creeper||zombie)skin=Color.FromArgb(93,172,73);if(ender)skin=body;if(golem||skeleton)skin=body;
    Rect(body,-36,-140,72,83);Rect(skin,-41,-208,82,72);
    Color trousers=suit?Color.FromArgb(35,38,43):creeper?body:Color.FromArgb(73,64,66);
    Rect(trousers,-34,-58,27,58);Rect(trousers,8,-58,27,58);
    if(!creeper){Rect(body,-58,-138,23,85);Rect(body,36,-138,23,85);}
    if(suit){Rect(Color.FromArgb(40,43,48),-36,-140,72,84);Rect(Color.White,-12,-140,24,58);Rect(Color.IndianRed,-5,-136,10,48);}
    if(creeper){Rect(Color.Black,-27,-185,18,18);Rect(Color.Black,9,-185,18,18);Rect(Color.Black,-9,-169,18,17);Rect(Color.Black,-23,-155,46,13);Rect(Color.Black,-23,-142,13,8);Rect(Color.Black,10,-142,13,8);}
    else{Rect(ender?Color.MediumPurple:Color.White,-29,-183,22,10);Rect(ender?Color.MediumPurple:Color.White,7,-183,22,10);if(!ender){Rect(Color.FromArgb(39,77,49),-20,-183,7,10);Rect(Color.FromArgb(39,77,49),12,-183,7,10);Rect(Color.FromArgb(113,66,54),-12,-153,24,6);}}
    if(villager){Rect(Color.FromArgb(159,115,80),-9,-178,18,38);Rect(Color.FromArgb(75,48,28),-33,-193,65,7);}
    if(type=="steve"||type=="bobby"){Rect(type=="bobby"?Color.FromArgb(197,115,39):Color.FromArgb(70,42,27),-41,-208,82,19);if(type=="bobby"){Rect(Color.Peru,26,-190,15,72);Rect(Color.Peru,32,-120,15,52);}}
    if(type=="witch"){Rect(Color.Purple,-60,-213,120,14);Rect(Color.FromArgb(48,29,59),-30,-260,60,48);Rect(Color.Olive,-12,-172,24,30);}
   }G.Restore(state);
  }
  public void Dispose(){caption.Dispose();label.Dispose();hand.Dispose();G.Dispose();}
 }
 static Bitmap DrawMeme(int index,int seed){var b=new Bitmap(900,640);using var p=new Painter(b,seed);p.G.Clear(Color.FromArgb(24,28,24));p.Rect(Color.FromArgb(154,194,211),0,110,900,430);p.Rect(Color.FromArgb(101,143,66),0,465,900,95);for(int i=0;i<35;i++)p.Rect(Color.FromArgb(91,130,55),p.R.Next(900),p.R.Next(472,550),18,6);
  var c=Memes[index%Memes.Length];p.Cap(c.Caption,8,94);p.Cap("BOBBYBOI EXPEDITION ARCHIVE",570,55);
  switch(index%30){
   case 0:p.Actor("creeper",240,475,1.2f,true);p.House(490,280);p.Paper(330,310,"POLICY");break;
   case 1:p.Actor("villager",245,475,1.3f);p.Table(460,365);p.Block(500,287,null,75);p.Text("64 EMERALDS",470,420);p.Gem(720,355);break;
   case 2:p.Actor("steve",270,475,1.2f);for(int i=0;i<3;i++)p.Bed(520,260+i*65);for(int i=0;i<5;i++)p.Line(Color.Gray,160+i*70,175,160+i*70,495,13);break;
   case 3:p.Actor("enderman",260,475,1.3f);p.Block(305,335);p.Table(535,360);p.Actor("villager",630,355,.75f);p.Text("COURT",575,165);break;
   case 4:p.Actor("golem",300,475,1.4f);p.Paper(475,285,"PARKING");p.Rect(Color.Gray,640,180,12,300);p.Text("NO\nPARKING",650,180,180);break;
   case 5:p.Actor("bobby",255,475,1.2f);p.Rect(Color.DarkSlateGray,430,210,330,220);p.Rect(Color.PaleGreen,448,228,292,175);p.Text("BALANCE\n-64\nEMERALDS",478,245,245);break;
   case 6:p.Actor("creeper",260,475,1.25f,true);p.Table(515,350);p.Paper(490,235,"CV");p.Text("DEMOLITION\nINTERVIEWS",610,175,250);break;
   case 7:p.Actor("chicken",420,455,2);p.Rect(Color.Black,410,267,45,24);p.Ellipse(Color.Red,420,270,16,16);p.Line(Color.Black,426,268,475,205,5);p.Text("REC",488,200,100);break;
   case 8:p.Actor("steve",245,475,1.3f);p.Table(470,355);p.Paper(640,260,"TAXES");p.Text("???",400,190);break;
   case 9:p.House(150,260);p.Rect(Color.Beige,560,235,270,130);p.Text("FOR SALE\n4,500,000",582,255,240);p.Rect(Color.SaddleBrown,670,365,14,130);break;
   case 10:p.Actor("zombie",270,475,1.3f);p.Rect(Color.SaddleBrown,560,210,110,265);for(int y=0;y<3;y++)p.Rect(Color.Peru,575,230+y*70,80,45);p.Text("PUSH? PULL?",430,150);break;
   case 11:for(int i=0;i<3;i++){p.Actor("skeleton",220+i*240,465,1.05f);p.Line(Color.SaddleBrown,185+i*240,365,270+i*240,265,18);}p.Ellipse(Color.DarkRed,335,390,140,90);break;
   case 12:p.Actor("pig",370,330,1.4f);p.Line(Color.Brown,315,260,250,390);p.Line(Color.Brown,430,260,490,390);p.Rect(Color.Gray,90,480,720,20);p.Text("FLIGHT SCHOOL",240,160);break;
   case 13:p.Actor("witch",270,490,1.15f);p.Table(500,350);for(int i=0;i<3;i++){p.Ellipse(Color.FromArgb(150+i*35,60,180),530+i*60,300,42,50);p.Rect(Color.SaddleBrown,544+i*60,285,12,20);}break;
   case 14:p.Actor("golem",330,480,1.5f);p.Rect(Color.DarkCyan,250,180,160,28);for(int i=0;i<8;i++)p.Rect(Color.SaddleBrown,530+i*30,210,20,235);p.Text("SILENCE",510,150);break;
   case 15:p.Actor("slime",260,460,2);p.Actor("slime",550,455,.8f);p.Actor("slime",710,455,.8f);p.Line(Color.White,400,350,500,350,9);break;
   case 16:p.Actor("bee",260,400,1.7f);p.Table(500,355);p.Paper(520,248,"MEETING");p.Text("Q4 POLLEN",540,175);break;
   case 17:p.Actor("axolotl",350,470,1.8f);for(int i=0;i<5;i++){p.Ellipse(Color.OrangeRed,560+i*30,285+i%2*45,85,155);p.Ellipse(Color.Gold,575+i*30,330+i%2*45,45,100);}break;
   case 18:p.Ellipse(Color.Gold,625,145,105,105);p.Actor("golem",300,475,1.2f);p.Rect(Color.Orange,240,220,120,20);p.Ellipse(Color.LightBlue,200,465,220,35);p.Text("32 C",610,270);break;
   case 19:p.Actor("spider",300,450,1.8f);p.Rect(Color.SlateGray,565,225,230,180);p.Rect(Color.WhiteSmoke,580,240,200,145);p.Actor("bee",670,365,.65f);break;
   case 20:p.House(100,270);p.Rect(Color.Gray,535,380,290,90);p.Rect(Color.OrangeRed,550,390,260,60);p.Text("POOL",620,290);p.Actor("steve",470,470,.85f);break;
   case 21:p.Actor("bobby",265,475,1.3f);p.Gem(530,320,50);p.Ellipse(Color.Peru,640,340,115,70);p.Text("1 POTATO",595,430);p.Line(Color.Black,570,335,620,365,8);break;
   case 22:p.Actor("enderman",380,475,1.3f);p.Ellipse(Color.IndianRed,300,200,190,45);p.Line(Color.Brown,400,210,400,350);for(int i=0;i<40;i++){int xx=p.R.Next(900),yy=p.R.Next(130,430);p.Line(Color.SteelBlue,xx,yy,xx-10,yy+28,3);}break;
   case 23:p.Actor("golem",400,475,1.4f);p.Line(Color.Black,200,370,650,370,14);p.Rect(Color.DarkSlateGray,170,320,60,100);p.Rect(Color.DarkSlateGray,625,320,60,100);break;
   case 24:p.Actor("villager",240,475,1.3f);p.Table(480,370);using(var pen=new Pen(Color.White,5))p.G.DrawRectangle(pen,530,270,80,80);p.Text("RARE AIR\n128 EMERALDS",620,280,240);break;
   case 25:p.Actor("steve",220,475,1.2f);p.Rect(Color.FromArgb(37,23,45),520,175,180,310);p.Rect(Color.MediumPurple,545,205,130,280);p.Text("ERROR 404\nHOME NOT FOUND",460,495,350);break;
   case 26:p.Actor("chicken",610,465,2.7f);p.Actor("steve",195,480,.7f);p.Line(Color.Turquoise,220,410,280,350,12);p.Rect(Color.Red,300,150,450,22);p.Text("CHICKEN PRIME",350,180);break;
   case 27:p.Actor("creeper",260,475,1.3f,true);p.Actor("bobby",575,475,1.3f);p.Rect(Color.Black,660,300,100,70);p.Ellipse(Color.Gray,675,310,48,48);p.Line(Color.Black,710,370,670,480);p.Line(Color.Black,710,370,760,480);break;
   case 28:p.Actor("cow",280,455,1.9f);p.Table(525,375);p.Rect(Color.White,560,285,65,90);p.Text("9.99 / MONTH",500,195,330);break;
   default:p.Actor("bobby",280,475,1.3f);p.Paper(480,275,"EVIDENCE",180);p.Actor("chicken",730,470,.8f);p.Text("DO NOT OPEN",455,190);break;
  }return b;
 }
 static Bitmap DrawPaint(int index,int seed){var b=new Bitmap(900,640);using var p=new Painter(b,seed);p.G.Clear(Color.FromArgb(255,255,239));p.G.SmoothingMode=SmoothingMode.AntiAlias;var concept=Paints[index%Paints.Length];p.Text(concept.Title,30,15,840,true);p.Text(concept.Caption,45,560,800,true);
  void Crooked(string type,float x,float y,float scale=1){var state=p.G.Save();p.G.TranslateTransform(x,y);p.G.RotateTransform(p.R.Next(-9,10));p.Actor(type,0,0,scale);p.G.Restore(state);}
  switch(index%12){
   case 0:Crooked("steve",420,495,1.5f);for(int i=0;i<17;i++){double a=i*Math.PI*2/17;p.Line(Color.FromArgb(p.R.Next(80,255),p.R.Next(80,200),p.R.Next(100)),420,330,420+(float)Math.Cos(a)*240,330+(float)Math.Sin(a)*150,13);}break;
   case 1:Crooked("creeper",430,330,1.1f);p.Line(Color.Salmon,400,275,280,540,25);p.Line(Color.Salmon,450,275,590,540,25);p.Ellipse(Color.Brown,210,525,100,28);p.Ellipse(Color.Brown,560,525,110,28);break;
   case 2:Crooked("enderman",300,530,1.7f);p.Ellipse(Color.Tomato,495,420,250,70);for(int i=0;i<22;i++)p.Line(Color.Goldenrod,530+p.R.Next(180),450,300+p.R.Next(100),230+p.R.Next(100),4);break;
   case 3:p.Rect(Color.Lime,170,370,580,140);p.Ellipse(Color.Black,230,465,100,80);p.Ellipse(Color.Black,620,465,100,80);Crooked("cow",430,390,1.7f);p.Text("LAMBO",520,420,200,true);break;
   case 4:Crooked("bobby",430,530,2);p.Ellipse(Color.CornflowerBlue,350,220,32,65);p.Ellipse(Color.CornflowerBlue,450,230,20,45);p.Text("me (handsome)",560,370,260,true);break;
   case 5:p.Ellipse(Color.DodgerBlue,220,190,480,300);for(int i=0;i<20;i++)p.Ellipse(Color.RoyalBlue,p.R.Next(250,640),p.R.Next(220,450),18,12);p.Text("100% DIAMOND",255,490,500,true);break;
   case 6:Crooked("steve",405,410,1.3f);for(int i=0;i<5;i++)p.Line(Color.Gray,200,350+i*25,650,370+i*25,4);p.Line(Color.Gray,220,350,260,500,7);p.Line(Color.Gray,650,370,610,500,7);p.Ellipse(Color.Black,250,500,50,50);p.Ellipse(Color.Black,570,500,50,50);p.Rect(Color.White,370,176,20,12);p.Rect(Color.White,417,176,20,12);break;
   case 7:Crooked("golem",430,470,1.4f);p.Line(Color.Magenta,250,500,640,520,24);p.Ellipse(Color.Black,290,520,45,45);p.Ellipse(Color.Black,560,520,45,45);break;
   case 8:Crooked("chicken",620,480,2.6f);Crooked("steve",190,510,1.1f);p.Line(Color.Blue,230,400,360,260,12);p.Text("oh no",65,200,200,true);break;
   case 9:for(int i=0;i<18;i++)p.Line(Color.SaddleBrown,p.R.Next(60,800),p.R.Next(100,530),p.R.Next(60,800),p.R.Next(100,530),3);p.Line(Color.Red,560,300,660,400,12);p.Line(Color.Red,660,300,560,400,12);p.Text("u are lost",100,440,400,true);break;
   case 10:Crooked("pig",450,470,2);p.Line(Color.Gray,170,350,720,350,12);p.Rect(Color.Purple,120,300,70,110);p.Rect(Color.Purple,700,300,70,110);break;
   default:p.Rect(Color.WhiteSmoke,350,150,230,220);for(int i=0;i<7;i++)p.Line(Color.Gray,370+i*28,340,350+i*35,470+p.R.Next(60),12);p.Line(Color.Red,470,360,440,550,3);p.Text("happy birth day",35,480,350,true);break;
  }
  // Deliberately clumsy signature and stray pencil marks; seeded for reproducibility.
  for(int i=0;i<12;i++)p.Line(Color.FromArgb(80,p.R.Next(255),p.R.Next(255),p.R.Next(255)),p.R.Next(900),p.R.Next(90,540),p.R.Next(900),p.R.Next(90,540),1);
  p.Text("- bobbyboi",640,575,250,true);return b;
 }
}
