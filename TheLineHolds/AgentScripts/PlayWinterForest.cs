// Unity Pipeline script eval --file in Play Mode. Exercise actual menu buttons and earned orders.
using System;
using System.Linq;
using System.Threading.Tasks;
using UnityEngine;
using TheLineHolds;
using UnityEngine.UI;
using Unity.Pipeline.Scripting;
BattleController Game()=>UnityEngine.Object.FindAnyObjectByType<BattleController>();
void Press(string label)=>UnityEngine.Object.FindObjectsByType<Button>().Single(b=>b.name==label).onClick.Invoke();
void Check(bool value,string error){if(!value)throw new Exception(error);}
var game=Game();
if(game)game.MainMenu();
await Wait.Until(()=>UnityEngine.Object.FindAnyObjectByType<TitleMenu>()!=null,timeoutMs:15000);
Press("Countryside");
await Wait.Until(()=>Game() && !Game().InMenu && !Game().winterForest,timeoutMs:15000);
game=Game();if(game)game.MainMenu();
await Wait.Until(()=>UnityEngine.Object.FindAnyObjectByType<TitleMenu>()!=null,timeoutMs:15000);
Press("Winter Forest");
await Wait.Until(()=>Game() && Game().winterForest && Game().Match!=null,timeoutMs:15000);
game=Game();game.NewMatch(42);
var m=game.Match;
int[] order={3,4,6,7,5,1,8,2,9,0}, roles={0,0,2,1,2,0,1,2,1,0};
int upgrades=0, tick=0, rearShots=0;
float depth=0;
m.Cue+=(cue,p)=>{if(m.Wave==10 && cue=="fire" && p.x>4)rearShots++;};
var folder=System.IO.Path.Combine(Application.dataPath,"../Screenshots");
bool combatCaptured=false,overviewCaptured=false;
while(!m.Ended && tick<140000)
{
    if(m.Phase==MatchPhase.Preparation)game.StartNextWave();
    for(int i=0;i<order.Length;i++)
    {
        int pad=order[i];Check(Vector3.Distance(game.padViews[pad].position,m.Pads[pad])<.01f,"Pad mismatch");
        if(m.Towers[pad]!=null)continue;
        if(m.Funds<game.defenses[roles[i]].cost)break;
        game.Choose(roles[i]);game.ClickPad(pad);game.Cancel();
    }
    foreach(int pad in order)
    {
        var t=m.Towers[pad];if(t==null)continue;
        int opt=t.Definition.role==DefenseRole.Mortar && t.Definition.upgrades.Length==2?1:0;
        if(opt>=t.Definition.upgrades.Length || m.Funds<t.Definition.upgrades[opt].price)continue;
        game.Cancel();game.ClickPad(pad);game.UpgradeSelected(opt);game.Cancel();upgrades++;
    }
    for(int j=0;j<25 && m.Phase==MatchPhase.Combat;j++,tick++)
    {
        m.Step(1f/60);
        if(m.Wave==10)foreach(var e in m.Enemies)depth=Mathf.Max(depth,e.Distance/m.Route.Length);
    }
    await Task.Yield();
    if(!overviewCaptured && m.Wave==1 && m.Enemies.Any(e=>e.Distance>2))
    {
        ScreenCapture.CaptureScreenshot(System.IO.Path.Combine(folder,"winter-forest-patrol.png"));
        overviewCaptured=true;
    }
    if(!combatCaptured && m.Wave==9 && m.Enemies.Any(e=>e.Definition.name=="HeavyTank" && e.Distance>.39f*m.Route.Length))
    {
        ScreenCapture.CaptureScreenshot(System.IO.Path.Combine(folder,"winter-forest-armor.png"));
        combatCaptured=true;
    }
}
Check(m.Phase==MatchPhase.Victory && m.CompletedWaves==10,"Full campaign: "+m.Phase+" wave "+m.Wave);
Check(m.Killed+m.Escaped==game.waves.Sum(w=>w.UnitCount),"Enemy accounting");
Check(combatCaptured && overviewCaptured && upgrades>=15,"Capture or upgrade missing");
game.NewMatch(42);Check(game.winterForest && game.Match.Funds==300,"Winter restart");
return $"Ten waves: {m.Lives} lives, {m.Killed} kills, {m.Escaped} escaped, {upgrades} upgrades, final penetration {depth:P0}, {rearShots} rear shots";
