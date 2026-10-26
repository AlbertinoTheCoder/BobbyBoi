using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.Text;
namespace BobbyBoi;
record MemeDelivery(string Path,string Title,string Kind,string? Reaction=null);
static partial class MemeFactory {
 static readonly object gate=new();static readonly Random random=new();static readonly Dictionary<string,Queue<int>> decks=[];
 public static string OutputFolder=>Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.DesktopDirectory),"BobbyBoi Memes");
 static int Next(string category,int count){lock(gate){if(!decks.TryGetValue(category,out var deck)||deck.Count==0){var choices=Enumerable.Range(0,count).OrderBy(_=>random.Next()).ToArray();decks[category]=deck=new Queue<int>(choices);}return deck.Dequeue();}}
 public static MemeDelivery Create(CancellationToken token,string? folder=null,int? forcedCategory=null){int category;int seed;lock(gate){category=forcedCategory??random.Next(3);seed=random.Next();}return Generate(category,Next(category.ToString(),category==0?Texts.Length:category==1?Memes.Length:Paints.Length),seed,folder??OutputFolder,token);}
 public static MemeDelivery Generate(int category,int index,int seed,string folder,CancellationToken token){
  token.ThrowIfCancellationRequested();string title,slug,ext,kind;string? reaction=null;byte[] bytes;
  if(category==0){var t=Texts[index%Texts.Length];title=t.Title;slug=t.Slug;ext=".txt";kind="TXT";string text=t.Text.Replace("{seed}",(seed%90000+10000).ToString());bytes=new UTF8Encoding(false).GetBytes(title.ToUpperInvariant()+"\r\n"+new string('=',title.Length)+"\r\n\r\n"+text.Replace("\n","\r\n")+"\r\n\r\nRecovered by BobbyBoi. No villagers were paid for this document.\r\n");if(slug.Contains("bank"))reaction="we are 64 emeralds in debt";else if(slug.Contains("classified"))reaction="they're following me";}
  else{var concept=(category==1?Memes:Paints)[index%(category==1?Memes.Length:Paints.Length)];title=concept.Title;slug=concept.Slug;ext=".png";kind=category==1?"MEME":"PAINT";using var image=category==1?DrawMeme(index,seed):DrawPaint(index,seed);using var stream=new MemoryStream();image.Save(stream,ImageFormat.Png);bytes=stream.ToArray();if(category==2)reaction="i made this with my feet";}
  token.ThrowIfCancellationRequested();Directory.CreateDirectory(folder);string path=Path.Combine(folder,$"{slug}_{DateTime.Now:yyyyMMdd_HHmmss}_{Guid.NewGuid():N}"+ext);
  // CreateNew guarantees no replacement. Bytes are generated entirely in memory first.
  using(var file=new FileStream(path,FileMode.CreateNew,FileAccess.Write,FileShare.None,4096,FileOptions.WriteThrough))file.Write(bytes);
  return new(path,title,kind,reaction);
 }
 record TextConcept(string Slug,string Title,string Text);
 static readonly TextConcept[] Texts=[
 new("steve_search_history","Steve's search history","how to craft a girlfriend\nhow to uncraft a girlfriend\ncan villagers sue\nwhy is the golem speedwalking toward me\nclear search history using redstone"),
 new("minecraft_bank_statement","Emerald bank statement","ACCOUNT: BOBBYBOI\nBALANCE: -64 EMERALDS\nCREDIT SCORE: DIRT\nLAST PURCHASE: one stick, 32 emeralds\nThe bank has repossessed your left shoe."),
 new("herobrine_apology","Herobrine's apology","dear steve,\nyour house was a social experiment.\nthe lava was the control group.\nsorry about the dog. he works for us now.\nherobrine"),
 new("job_application","BobbyBoi applies for employment","EXPERIENCE: 14 years punching trees\nEDUCATION: crafting table\nSKILL: falling into lava\nEXPECTED SALARY: 64 diamonds\nWHY DID YOU LEAVE? creeper\nREFERENCE: a chicken that refuses to answer the phone"),
 new("villager_court","Villager court transcript","CASE {seed}\nCRIME: transporting seventeen villagers in one boat\nDEFENSE: they enjoyed the cruise\nJUDGE: hrrm\nSENTENCE: 400 years in a dirt house with no door"),
 new("classified_chicken","Classified chicken report","CLASSIFIED // EYES ONLY\nThe chickens are not laying eggs. They are deploying sensors.\nAgent Cluck has infiltrated the wheat supply.\nIf this document makes a noise, feed it seeds.\nBurn after reading. Preferably in a furnace."),
 new("creeper_insurance","Home insurance denial","CLAIM DENIED.\nOur policy covers acts of nature.\nA green man whispering 'nice house' is an act of poor judgment."),
 new("enderman_rent","Enderman rental application","Bedrooms: 1\nCeiling height: insufficient\nPets: one stolen grass block\nSpecial requirements: please remove all rain\nDeposit: paid in eye contact"),
 new("dirt_house_listing","Luxury dirt residence","ASKING PRICE: 4,500,000 emeralds\nOpen-plan living. Extremely open.\nWaterfront whenever it rains.\nNatural ventilation through creeper damage.\nOne previous owner, currently missing."),
 new("golem_parking_ticket","Iron golem parking appeal","I was not parked.\nI was standing dramatically.\nThe poppy is my permit."),
 new("crafting_tax_form","Crafting tax return","Declare all sticks earned this year: 4\nDeductible expenses: 86 lost pickaxes\nDo you own offshore assets? one boat\nSignature: accidentally placed a torch here"),
 new("zombie_meal_plan","Zombie meal preparation","Monday: brains\nTuesday: brains\nWednesday: vegetarian brains\nThursday: stare at a door for six hours\nFriday: cheat day (door)"),
 new("skeleton_band","Skeleton band contract","Genre: bone metal\nPayment: exposure to moonlight\nDrummer brings his own ribs.\nOutdoor daytime gigs are strictly prohibited."),
 new("slime_diary","Slime's diary","dear diary\ntoday i became four smaller problems"),
 new("witch_refund","Witch potion refund","Customer reports invisibility potion did not work.\nWe cannot locate the customer to process this claim.\nCase closed successfully."),
 new("pig_flight","Pig aviation licence","PILOT: Sir Oinks\nAIRCRAFT: saddle\nRUNWAY: cliff\nTEST RESULT: brief but convincing\nRenewal requires a carrot and a better plan."),
 new("nether_travel","Nether travel review","ONE STAR.\nNo water. No beds. Hotel exploded when I tried to sleep.\nStaff kept screaming.\nWould return for the quartz."),
 new("villager_union","Villager union demands","1. Actual doors.\n2. Boats with cup holders.\n3. No more 'temporary' underground offices.\n4. Stop calling kidnapping a trading hall.\n5. Dental coverage for the nose."),
 new("diamond_resume","Diamond's resignation","I resign effective immediately.\nYou spent three hours finding me and dropped me in lava.\nPlease forward my final paycheck to bedrock."),
 new("chest_instructions","Chest assembly manual","Step 1: put wood in a square.\nStep 2: discover it is a crafting table.\nStep 3: quietly buy a chest from someone competent."),
 new("bed_recall","Emergency bed recall","These beds may explode when used in dimensions that dislike naps.\nAffected models: all of them.\nSuggested alternative: remain awake forever."),
 new("redstone_support","Redstone technical support","Have you tried turning the lever off and on?\nHave you tried admitting you copied the tutorial wrong?\nYour ticket has been escalated to a chicken."),
 new("furnace_restaurant","Furnace restaurant menu","Today's special: suspiciously warm potato\nChef's tasting menu: 64 identical potatoes\nService charge: one bucket of lava\nPlease stop eating the furniture."),
 new("warden_noise","Warden noise complaint","To the upstairs tenant:\nI can hear you eating carrots through seventeen layers of stone.\nConsider soup.\nRegards, your downstairs problem."),
 new("bee_hr","Bee human resources","Your performance has been outstanding.\nUnfortunately you stung the manager and immediately died.\nWe have marked this as voluntary resignation."),
 new("axolotl_award","Axolotl employee award","EMPLOYEE OF THE MONTH\nFor lying down during every emergency.\nManagement calls this strategic horizontal thinking."),
 new("boat_speeding","Boat speeding citation","LOCATION: ice highway\nSPEED: emotionally unreasonable\nCARGO: two villagers pretending not to know you\nFINE: surrender the ice"),
 new("obsidian_receipt","Obsidian purchase receipt","ITEM: one purple rock\nDELIVERY ESTIMATE: ten seconds with diamond pickaxe\nCUSTOMER USED: wooden shovel\nDELIVERY ESTIMATE UPDATED: next Thursday"),
 new("snow_golem_letter","Snow golem summer holiday","Dear family,\nFlorida was a mistake.\nPlease send a freezer.\nThis letter is getting wet."),
 new("creeper_cover_letter","Creeper cover letter","Dear demolition company,\nI have a passion for open-plan architecture.\nMy previous project received a very loud response.\nI am available for exactly one assignment."),
 new("spider_web_design","Spider web design invoice","Responsive web design: 64 emeralds\nBug removal: included\nMobile support: eight legs\nYour website has caught three actual flies."),
 new("mushroom_club","Mushroom club minutes","Meeting called to order at sunset.\nMotion to become taller: carried.\nMotion to explain suspicious stew: denied.\nMinutes taken by a cow. Nobody knows why."),
 new("bobby_shopping","BobbyBoi's shopping list","bread\napple\nemotional support cobblestone\na smaller mortgage\nbread again in case the first bread leaves"),
 new("lava_safety","Lava safety certificate","This certifies that BobbyBoi looked directly at lava and said 'probably fine'.\nCertificate validity: already expired."),
 new("portal_customer_service","Portal support transcript","USER: the portal made a screaming noise\nSUPPORT: that is the customer satisfaction survey\nUSER: i spawned inside a wall\nSUPPORT: premium customers get windows"),
 new("sheep_fashion","Sheep fashion forecast","This season: wool.\nNext season: also wool.\nBold prediction: wool, but purple.\nThe naked look remains controversial."),
 new("enchanted_legal","Enchanted sword legal notice","SHARPNESS V is not a personality.\nUNBREAKING III does not apply to relationships.\nCURSE OF BINDING is not a marriage licence.\nPlease consult an actual villager."),
 new("map_review","Treasure map review","The X was a spider.\nThe treasure was more sand.\nThe ocean is in the wrong place.\nTwo stars because the paper was edible."),
 new("bobby_calendar","BobbyBoi's busy calendar","09:00 panic\n09:05 stare at mouse\n10:00 unpaid mining\n12:00 lunch (imaginary)\n13:00 meeting with dirt\n17:00 leave mysteriously\n17:05 return with evidence"),
 new("final_boss_invoice","Final boss consulting invoice","CONSULTING: 1 dramatic monologue\nEXPENSES: 400 broken blocks\nTRAVEL: one dragon\nTOTAL: your entire inventory\nTerms: payment due before respawn")
 ];
 record ImageConcept(string Slug,string Title,string Caption);
 static readonly ImageConcept[] Memes=[
 new("creeper_insurance","Creeper insurance","WE NEED TO DISCUSS YOUR HOME INSURANCE"),new("villager_markup","Villager economics","ONE DIRT BLOCK. 64 EMERALDS. FINAL OFFER."),new("bed_theft","Steve's arrest","THEY SAID THE BEDS WERE FREE"),new("enderman_court","Enderman in court","YOUR HONOR, THE BLOCK FOLLOWED ME HOME"),new("golem_ticket","Golem parking ticket","I WAS STANDING HERE FOR YOUR PROTECTION"),new("negative_balance","BobbyBoi checks the bank","MY SAVINGS HAVE ENTERED SPECTATOR MODE"),new("creeper_job","Creeper job interview","MY STRENGTH? OPEN-PLAN ARCHITECTURE"),new("chicken_drone","Chicken surveillance","GOVERNMENT SURVEILLANCE DRONE"),new("crafting_taxes","Crafting table taxes","ME TRYING TO CRAFT A TAX RETURN"),new("luxury_dirt","Housing market","4,500,000 EMERALDS. NO LOWBALLS."),
 new("zombie_door","Zombie engineering","SIX HOURS LATER: STILL A DOOR"),new("skeleton_band","Skeleton world tour","WE ONLY PLAY BONE METAL"),new("pig_pilot","Pig flight school","THE INSTRUCTOR SAID BELIEVE IN YOURSELF"),new("witch_support","Witch customer support","THE REFUND POTION COSTS EXTRA"),new("warden_library","Warden librarian","YOUR BREATHING IS OVERDUE"),new("slime_meeting","Slime management","WE SPLIT THE TEAM TO IMPROVE EFFICIENCY"),new("bee_office","Bee corporate life","ANOTHER MEETING THAT COULD HAVE BEEN POLLEN"),new("axolotl_fire","Axolotl emergency response","PLAYING DEAD IS MY FIVE-YEAR PLAN"),new("snow_vacation","Snow golem vacation","SUMMER HOLIDAY: CANCELLED"),new("spider_website","Spider freelancer","I BUILT YOUR WEBSITE. IT HAS BUGS."),
 new("lava_pool","Luxury swimming pool","THE WATER IS SPICY TODAY"),new("diamond_potato","Inventory priorities","SOLD MY DIAMONDS FOR ONE POTATO"),new("enderman_umbrella","Enderman rain problem","I HAVE MADE A TACTICAL MISTAKE"),new("golem_gym","Iron golem gym","LEG DAY WAS A SOFTWARE UPDATE"),new("villager_auction","Villager auction house","RARE AIR BLOCK. STARTING BID: 128 EMERALDS"),new("steve_portal","Portal tech support","HAVE YOU TRIED WALKING BACK THROUGH IT?"),new("chicken_boss","Chicken final boss","YOU LAUGHED AT THE CHICKEN. NOW RUN."),new("creeper_wedding","Creeper wedding photographer","EVERYBODY SAY SSSSSSS"),new("cow_milk","Cow subscription service","MILK IS NOW A MONTHLY SUBSCRIPTION"),new("bobby_evidence","BobbyBoi brings evidence","MY LAWYER SAID NOT TO EXPLAIN THIS")
 ];
 static readonly ImageConcept[] Paints=[new("seventeen_arms","Steve with 17 arms","i made this"),new("long_creeper","Creeper with human legs","took 7 hours"),new("enderman_spaghetti","Enderman eating spaghetti","pls rate 5 stars"),new("cow_supercar","Cow driving a supercar","mom said im talented"),new("self_portrait","BobbyBoi self portrait","this is art"),new("blue_potato","A very expensive blue potato","definitely a diamond"),new("herobrine_cart","Herobrine shopping cart","i made this"),new("golem_skateboard","Iron golem skateboard","took 7 hours"),new("giant_chicken","Steve fights a giant chicken","pls rate 5 stars"),new("treasure_map","BobbyBoi treasure map","X is probably treasure"),new("pig_gym","Pig at the gym","mom said im talented"),new("ghast_balloon","Ghast birthday balloon","this is art")];
}
