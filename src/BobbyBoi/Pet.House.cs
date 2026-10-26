namespace BobbyBoi;
sealed partial class Pet {
 bool HouseAvailable=>data.World.HasHouse&&data.World.Houses&&(data.World.HouseBlocks>0||world.Active?.Spec.Kind==WorldEventKind.BuildHouse);
 bool ActorHidden=>actorInside&&(world.Active?.Spec.Kind switch {
  WorldEventKind.HouseVisit=>HouseAvailable,
  WorldEventKind.Cave or WorldEventKind.Portal or WorldEventKind.Ufo or WorldEventKind.Prison=>true,
  _=>false
 });
 void ClearHouseCache(){houseSprite?.Dispose();houseSprite=null;houseSpriteKey=int.MinValue;houseMesh=null;houseMeshVariant=-1;}
 void SetHousesEnabled(bool enabled){data.World.Houses=enabled;if(!enabled)RecoverMissingHouse(true);}
 void RecoverMissingHouse(bool refresh=false){
  if(HouseAvailable)return;
  world.DiscardPending(WorldEventKind.HouseVisit);
  if(!data.World.Houses)world.DiscardPending(WorldEventKind.BuildHouse);
  bool activeHouse=world.Active?.Spec.Kind==WorldEventKind.HouseVisit||!data.World.Houses&&world.Active?.Spec.Kind==WorldEventKind.BuildHouse;
  bool suspendedHouse=interruptedWorld?.Spec.Kind==WorldEventKind.HouseVisit||!data.World.Houses&&interruptedWorld?.Spec.Kind==WorldEventKind.BuildHouse;
  bool roof=onHouseRoof;
  if(activeHouse)CancelWorld();
  else if(suspendedHouse){interruptedWorld=null;interruptedItem=null;}
  onHouseRoof=false;
  // Other events intentionally hide him too: do not interrupt a cave, portal, UFO or jail.
  if(activeHouse||suspendedHouse||roof||actorInside&&!ActorHidden){actorInside=false;characterWorldScale=1;if(activeHouse||suspendedHouse){houseInside=0;if(!dragging&&respawnTime<=0&&(activeHouse||action==Act.Sleep))Set(Act.Idle,3);}if(roof&&!dragging){vy=Math.Max(0,vy);Set(Act.Fall,5);}}
  ClearHouseCache();
  if(refresh){worldCanvas.Hide();if(frame!=null&&!userHidden)Render(true);}
 }
}
