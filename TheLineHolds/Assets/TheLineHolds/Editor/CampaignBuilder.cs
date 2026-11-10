using System;
using System.Linq;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
namespace TheLineHolds.Editor
{
 public static class CampaignBuilder
 {
  const string Root="Assets/TheLineHolds";
  static T Asset<T>(string path) where T:ScriptableObject
  {string p=Root+"/Data/"+path+".asset";var a=AssetDatabase.LoadAssetAtPath<T>(p);if(!a){a=ScriptableObject.CreateInstance<T>();AssetDatabase.CreateAsset(a,p);}return a;}
  static EnemyDefinition Enemy(string key,string name,string hint,float hp,float speed,int reward,bool armor,float gap=0)
  {var e=Asset<EnemyDefinition>("Enemies/"+key);e.displayName=name;e.description=hint;e.health=hp;e.speed=speed;e.reward=reward;e.armored=armor;e.spawnSeparation=gap;e.prefab=AssetDatabase.LoadAssetAtPath<GameObject>(Root+"/Prefabs/"+key+".prefab");EditorUtility.SetDirty(e);return e;}
  static DefenseDefinition Defense(string key,string name,string hint,DefenseRole role,int tier,int cost,float range,float interval,float damage,float splash,float armor,float slow=1,float duration=0)
  {
   var d=Asset<DefenseDefinition>("Defenses/"+key);d.displayName=name;d.description=hint;d.role=role;d.tier=tier;d.cost=cost;d.range=range;d.interval=interval;d.damage=damage;d.splashRadius=splash;d.flightTime=role==DefenseRole.Mortar?.95f:role==DefenseRole.Cannon?.35f:.12f;d.infantryDamageMultiplier=role==DefenseRole.Cannon && splash==0?.65f:1;d.armorDamageMultiplier=armor;d.priority=role==DefenseRole.Mortar?TargetPriority.Cluster:role==DefenseRole.Cannon?TargetPriority.Armor:TargetPriority.First;d.slowMultiplier=slow;d.slowDuration=duration;d.upgrades=Array.Empty<UpgradeOption>();
   d.prefab=AssetDatabase.LoadAssetAtPath<GameObject>(Root+"/Prefabs/"+key+".prefab");if(!d.prefab)d.prefab=AssetDatabase.LoadAssetAtPath<GameObject>(Root+"/Prefabs/"+(role==DefenseRole.MachineGun?"MachineGun":role==DefenseRole.Cannon?"AntiTank":"Mortar")+".prefab");EditorUtility.SetDirty(d);return d;
  }
  static void Upgrades(DefenseDefinition d,params UpgradeOption[] options){d.upgrades=options;EditorUtility.SetDirty(d);}
  static UpgradeOption To(DefenseDefinition d,int price)=>new UpgradeOption{defense=d,price=price};
  [MenuItem("The Line Holds/Build Ten Wave Campaign")]
  public static string Build()
  {
   if(EditorApplication.isPlaying)throw new InvalidOperationException("Stop Play Mode first.");
   DioramaBuilder.RefreshModels(new[]{"Infantry","LightTank","Scout","AssaultInfantry","Truck","ArmoredCar","HeavyTank"});
   var inf=Enemy("Infantry","Riflemen","Regular infantry. Machine guns are efficient.",68,1.2f,4,false);
   var scout=Enemy("Scout","Runners","Fast, fragile scouts. Cover the rear bends.",38,2.05f,4,false);
   var assault=Enemy("AssaultInfantry","Assault troops","Durable infantry. Mortars punish their clusters.",150,1.05f,7,false);
   var truck=Enemy("Truck","Trucks","Fast unarmored transport. Machine guns can help.",200,1.85f,12,false,4.4f);
   var car=Enemy("ArmoredCar","Armored cars","Quick armor. Anti-tank coverage at several bends.",270,1.9f,15,true,4.4f);
   var tank=Enemy("LightTank","Light tanks","Slow armor. Anti-tank guns are the main counter.",530,1.15f,22,true,4.4f);
   var heavy=Enemy("HeavyTank","Heavy tanks","Very durable armor. Upgrade your anti-tank guns.",1250,.9f,38,true,4.4f);
   var mg=Defense("MachineGun","Machine-gun nest","Single infantry / weak vs armor",DefenseRole.MachineGun,1,75,3.3f,.32f,8,0,.15f);
   var gun=Defense("AntiTank","Anti-tank gun","Armor priority / single target",DefenseRole.Cannon,1,125,5.8f,2,95,0,1);
   var mortar=Defense("Mortar","Mortar","Targets clusters / splash damage",DefenseRole.Mortar,1,150,4.8f,3.8f,42,1.4f,.35f);
   var mg2=Defense("MachineGunII","Veteran nest","Faster fire and better reach",DefenseRole.MachineGun,2,165,3.7f,.26f,12,0,.22f);
   var mgRapid=Defense("MachineGunRapid","Suppressor nest","Rapid fire / briefly slows single targets",DefenseRole.MachineGun,3,345,3.9f,.16f,14,0,.22f,.72f,.6f);
   var mgAP=Defense("MachineGunAP","AP machine gun","Longer reach / strong against light armor",DefenseRole.MachineGun,3,355,4.4f,.25f,21,0,.75f);
   var gun2=Defense("AntiTankII","Veteran anti-tank","Heavier shells and faster reload",DefenseRole.Cannon,2,265,6.2f,1.7f,155,0,1.15f);
   var hunter=Defense("TankHunter","Tank hunter","Long-range heavy armor specialist",DefenseRole.Cannon,3,515,6.6f,1.65f,255,0,1.35f);
   var howitzer=Defense("FieldHowitzer","Field howitzer","Fast explosive shells / small-area damage",DefenseRole.Cannon,3,500,5.8f,1.25f,78,.9f,.7f);
   var mortar2=Defense("MortarII","Veteran mortar","Larger blast and faster loading",DefenseRole.Mortar,2,310,5.3f,3.1f,66,1.7f,.4f);
   var siege=Defense("SiegeMortar","Siege mortar","Heavy shells / stronger armor damage",DefenseRole.Mortar,3,560,5.8f,3.1f,110,1.95f,.6f);
   var barrage=Defense("BarrageMortar","Barrage mortar","Wide blast / slows groups for 1.5 seconds",DefenseRole.Mortar,3,540,5.4f,2.35f,55,2,.4f,.55f,1.5f);
   Upgrades(mg,To(mg2,90));Upgrades(mg2,To(mgRapid,180),To(mgAP,190));Upgrades(gun,To(gun2,140));Upgrades(gun2,To(hunter,250),To(howitzer,235));Upgrades(mortar,To(mortar2,160));Upgrades(mortar2,To(siege,250),To(barrage,230));
   var enemies=new[]{inf,scout,assault,truck,car,tank,heavy};
   int[][] counts={new[]{18,0,0,0,0,0,0},new[]{8,28,0,0,0,0,0},new[]{30,0,10,0,0,0,0},new[]{0,14,0,6,5,0,0},new[]{0,0,0,0,4,8,0},new[]{30,20,10,0,0,3,0},new[]{0,24,18,6,0,4,0},new[]{54,14,18,0,0,0,0},new[]{0,0,0,0,6,8,6},new[]{40,26,16,6,6,10,3}};
   string[] titles={"Probing patrol","The runners","Platoon advance","Motor reconnaissance","Armored spearhead","Crossfire","The feint","Infantry surge","Heavy column","The final push"};
   string[] hints={"Start with overlapping MG coverage. Save for other roles.","Fast runners test gaps. Extend coverage toward the exit.","Durable clusters favor mortars. Prepare anti-tank guns next.","Cars resist machine guns; trucks do not. Cover more than one bend.","Vehicles only. Upgrade anti-tank guns before calling this wave.","Mixed pressure. Choose between another site and a veteran upgrade.","Runners, transport and tough infantry. Keep a rear defense ready.","Dense infantry. Blast radius and suppression earn their cost.","Heavy armor only. Tank hunters and siege mortars are valuable.","Combined assault. Specialize, cover the exit and spend your reserve."};
   var formations=new[]{WaveFormation.Patrol,WaveFormation.Stream,WaveFormation.Clusters,WaveFormation.Mixed,WaveFormation.Convoy,WaveFormation.Mixed,WaveFormation.Stream,WaveFormation.Clusters,WaveFormation.Convoy,WaveFormation.Mixed};
   var waves=new WaveDefinition[10];
   for(int w=0;w<10;w++)
   {var d=Asset<WaveDefinition>("Waves/Wave"+(w+1));d.title=titles[w];d.briefing=hints[w];d.formation=formations[w];d.varied=true;d.healthMultiplier=new[]{1f,1.04f,1.12f,1.2f,1.3f,1.5f,1.8f,2.5f,2.7f,5.8f}[w];d.spawns=counts[w].Select((n,i)=>new SpawnEntry(enemies[i],1,n,.2f)).Where(e=>e.count>0).ToArray();waves[w]=d;EditorUtility.SetDirty(d);}
   var game=UnityEngine.Object.FindAnyObjectByType<BattleController>();game.waves=waves;game.defenses=new[]{mg,gun,mortar};EditorUtility.SetDirty(game);AssetDatabase.SaveAssets();EditorSceneManager.MarkSceneDirty(game.gameObject.scene);EditorSceneManager.SaveScene(game.gameObject.scene);
   return "Saved ten varied waves, seven enemy archetypes and three upgrade trees with six specializations";
  }
 }
}
