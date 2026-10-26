using System.Numerics;
namespace BobbyBoi;
sealed partial class Pet {
 SceneVisual ChaosProp(string name,Vector2 feet,float scale=3,float yaw=.25f,bool animated=false)=>new(new((int)feet.X-160,(int)feet.Y-250,320,320),(g,o)=>ChaosAssets.Draw(g,name,feet+o,scale,yaw,worldAge,animated));
 static Vector2 PrisonPoint(Vector2 origin,Vector3 v){float yaw=.12f;var p=new Vector3(v.X*MathF.Cos(yaw)+v.Z*MathF.Sin(yaw),v.Y,-v.X*MathF.Sin(yaw)+v.Z*MathF.Cos(yaw));return origin+new Vector2(p.X*3.5f,-p.Y*3.5f+p.Z*3.5f*.34f);}
 IEnumerable<SceneVisual> ExpansionVisuals(){
  if(data.World.PetCreeper){var p=creeperFeet+new Vector2(0,-Math.Abs(MathF.Sin(creeperAge*4))*3);yield return ChaosProp("petcreeper",p,2,creeperFeet.X<x+Width/2?.65f:-.65f,!data.World.CreeperSitting);}
  var a=world.Active;if(a==null||a.Spec.Kind<WorldEventKind.Prison)yield break;float t=a.Age;var anchor=chaosAnchor;
  switch(a.Spec.Kind){
   case WorldEventKind.Prison:
    if(t<8){var golem=new Vector2(x+Width/2+Math.Max(45,220-t*35),y+Height);float facing=t<5?-1:Math.Sign(chaosAnchor.X-(x+Width/2));yield return ChaosProp("golem",golem,3,facing*.85f,true);}
    if(t>=8&&t<20)yield return ChaosProp("golem",anchor+new Vector2(120+Math.Max(0,t-18)*70,0),3,t<18?-.85f:.85f,t>=18);
    if(t>=5&&t<21)yield return new(new((int)anchor.X-165,(int)anchor.Y-235,370,285),(g,o)=>{
     var cell=anchor+o-new Vector2(0,20);float opening=t<7.4f?1:t<8?1-(t-7.4f)/.6f:t<17.8f?0:Math.Clamp((t-17.8f)/.6f,0,1);int gate=(int)MathF.Round(opening*12);
     ChaosAssets.Draw(g,"prison",cell,3.5f,.12f,layer:"back");
     if(actorInside){float exit=Math.Clamp((t-18)/2,0,1);var feet=cell+Vector2.Lerp(new(0,-7),new(130,28),exit);Character.Draw(g,feet,3.1f,action,worldAge,exit>0?.85f:.12f,0,animationPose:jailPose);}
     ChaosAssets.Draw(g,"prison",cell,3.5f,.12f,layer:"front",gateStep:gate);
     var sign=PrisonPoint(cell,new(0,39,20.5f));PixelText.Draw(g,"BOBBY JAIL",(int)sign.X-29,(int)sign.Y-3,Color.FromArgb(244,216,143),1);
    });break;
   case WorldEventKind.Casino:
    yield return ChaosProp("casino",anchor,3.4f,.1f,t<5);yield return new(new((int)anchor.X-60,(int)anchor.Y-100,130,50),(g,o)=>{PixelText.Draw(g,"CASINO",(int)(anchor.X+o.X-35),(int)(anchor.Y+o.Y-100),Color.Gold,2);for(int i=0;i<3;i++)PixelText.Draw(g,t<5?((int)(t*12+i)%9).ToString():(chaosResult==0?"0":"1"),(int)(anchor.X+o.X-22+i*17),(int)(anchor.Y+o.Y-65),Color.LimeGreen,2);});break;
   case WorldEventKind.Portal:
    if(t>3){yield return new(new((int)anchor.X-40,(int)anchor.Y-110,80,115),(g,o)=>{using var purple=new SolidBrush(Color.FromArgb(140,111+(int)(Math.Sin(worldAge*4)*20),28,190));g.FillRectangle(purple,anchor.X+o.X-30,anchor.Y+o.Y-103,60,100);for(int k=0;k<8;k++){int yy=(int)((worldAge*24+k*13)%95);g.FillRectangle(Brushes.MediumPurple,anchor.X+o.X-25+k%3*10,anchor.Y+o.Y-100+yy,15,3);}});}yield return new(new((int)anchor.X-160,(int)anchor.Y-250,320,320),(g,o)=>ChaosAssets.Draw(g,"portal",anchor+o,3,.06f,buildStage:Math.Clamp((int)(t/4*100),5,100)));break;
   case WorldEventKind.Herobrine:if(t<4.2f)yield return ChaosProp("herobrine",anchor,3,.6f);break;
   case WorldEventKind.Earthquake:yield return new(new((int)x,(int)y+Height-75,260,90),(g,o)=>{using var crack=new Pen(Color.FromArgb(170,91,61),3);float xx=x+Width/2+o.X,yy=y+Height+o.Y-6;g.DrawLines(crack,new PointF[]{new(xx-65,yy),new(xx-25,yy-9),new(xx,yy),new(xx+21,yy-13),new(xx+55,yy)});});break;
   case WorldEventKind.FakeUpdate:
    var panel=new Rectangle((int)anchor.X-135,(int)anchor.Y-200,255,130);yield return new(panel,(g,o)=>{var r=panel;r.Offset((int)o.X,(int)o.Y);PixelText.Panel(g,r,Color.CornflowerBlue);PixelText.Draw(g,"BOBBYBOI FAKE UPDATE",r.X+12,r.Y+14,Color.LightBlue,1);PixelText.Draw(g,t<4?"CONFIGURING DIRT":"DO NOT TURN OFF BREAD",r.X+12,r.Y+37,Color.White,1);int percent=t<4?(int)(t/4*99):t<11?99:100;g.FillRectangle(Brushes.Black,r.X+12,r.Y+62,228,13);g.FillRectangle(Brushes.CornflowerBlue,r.X+14,r.Y+64,(int)(224*percent/100f),9);PixelText.Draw(g,percent+"%",r.X+12,r.Y+89,Color.White,2);});break;
   case WorldEventKind.AdoptCreeper:if(!data.World.PetCreeper)yield return ChaosProp("petcreeper",anchor,2.2f,.5f,true);break;
   case WorldEventKind.YouTube:
    yield return ChaosProp("camera",anchor,3,-.5f);var board=new Rectangle((int)anchor.X-235,(int)anchor.Y-245,232,105);yield return new(board,(g,o)=>{var r=board;r.Offset((int)o.X,(int)o.Y);PixelText.Panel(g,r,Color.IndianRed);PixelText.Draw(g,"BOBBYTUBE - OFFLINE",r.X+10,r.Y+10,Color.Salmon,1);var title=PixelText.Wrap(data.World.LastStory,32);for(int i=0;i<title.Count&&i<2;i++)PixelText.Draw(g,title[i],r.X+10,r.Y+30+i*12,Color.White,1);PixelText.Draw(g,"SUBS: "+data.World.Subscribers,r.X+10,r.Y+75,Color.Gold,1);if(data.World.PetCreeper)ChaosAssets.Draw(g,"petcreeper",new(r.Right-26,r.Bottom-12),.8f,.3f);});break;
   case WorldEventKind.Ufo:
    var ship=new Vector2(a.Start.X+Width/2,a.Start.Y+Height-170);if(t<3)ship.Y-=120*(1-t/3);if(t>7&&t<11)ship.Y-=MathF.Sin((t-7)/4*MathF.PI)*200;if(t>14)ship.Y-=(t-14)*90;if(t>2&&t<14){var beam=Rectangle.FromLTRB((int)ship.X-45,(int)ship.Y+5,(int)ship.X+45,(int)(a.Start.Y+Height));yield return new(beam,(g,o)=>{using var fill=new SolidBrush(Color.FromArgb(55,150,255,195));g.FillPolygon(fill,new PointF[]{new(ship.X+o.X-16,ship.Y+o.Y),new(ship.X+o.X+16,ship.Y+o.Y),new(ship.X+o.X+45,a.Start.Y+Height+o.Y),new(ship.X+o.X-45,a.Start.Y+Height+o.Y)});});}yield return ChaosProp("ufo",ship,3.3f,MathF.Sin(worldAge)*.12f);break;
   case WorldEventKind.Cave:yield return ChaosProp("cave",anchor,3,.05f);break;
  }
 }
}
