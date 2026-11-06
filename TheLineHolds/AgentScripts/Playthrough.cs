// Run via `script eval --file AgentScripts/Playthrough.cs` in Editor or a development Player.
// Uses real frame-driven combat, actual purchases, and controller callbacks. No funds, health, or wave cheats.
using TheLineHolds;
using UnityEngine;
using Unity.Pipeline.Scripting;
using System.Linq;
var game=Object.FindAnyObjectByType<BattleController>();
void Check(bool condition,string message) { if(!condition)throw new System.Exception(message); }
game.NewMatch();
game.Choose(0);game.ClickPad(3);game.ClickPad(4);game.Choose(2);game.ClickPad(6);game.Cancel();
Check(game.Match.Funds==0,"Starting purchases must spend exactly 300.");
game.Match.Journal += message => Debug.Log("[Playthrough] "+message);
bool combatCaptured=false;float finalDepth=0;int rearShots=0;
game.Match.Cue+=(cue,p)=> { if(cue=="fire" && game.Match.Wave==5 && (Vector3.Distance(p,game.Match.Pads[8]+Vector3.up*.65f)<.01f || Vector3.Distance(p,game.Match.Pads[9]+Vector3.up*.65f)<.01f))rearShots++; };
int matchFrames=Time.frameCount;float started=Time.realtimeSinceStartup;
while(!game.Match.Ended)
{
    if(game.Match.Phase==MatchPhase.Preparation || game.Match.Phase==MatchPhase.Combat)
    {
        int[] pads={3,4,6,7,5,1,8,2,9,0}, types={0,0,2,1,2,0,1,2,0,0};
        for(int i=0;i<pads.Length;i++)
        {
            int pad=pads[i],type=types[i];
            if(game.Match.Towers[pad]==null && game.Match.Funds<game.defenses[type].cost)break;
            if(game.Match.Towers[pad]==null && game.Match.Funds>=game.defenses[type].cost) { game.Choose(type);game.ClickPad(pad); }
        }
        game.Cancel();if(game.Match.Phase==MatchPhase.Preparation)game.StartNextWave();
    }
    await Wait.Until(()=>
    {
        if(game.Match.Wave==5)
            foreach(var enemy in game.Match.Enemies)finalDepth=Mathf.Max(finalDepth,enemy.Distance/game.Match.Route.Length);
        if(!combatCaptured && game.Match.Wave==5 && finalDepth>=.75f)
        {
            ScreenCapture.CaptureScreenshot(System.IO.Path.Combine(System.IO.Path.GetTempPath(),"the-line-holds-combat.png"));
            combatCaptured=true;
        }
        int[] planPads={3,4,6,7,5,1,8,2,9,0},planTypes={0,0,2,1,2,0,1,2,0,0};
        int next=System.Array.FindIndex(planPads,p=>game.Match.Towers[p]==null);
        bool canReinforce=next>=0 && game.Match.Funds>=game.defenses[planTypes[next]].cost;
        return game.Match.Ended || game.Match.Phase==MatchPhase.Preparation || canReinforce;
    },timeoutMs:180000,because:"wave should resolve");
}
Check(game.Match.Phase==MatchPhase.Victory,"The match must finish in victory.");
Check(finalDepth>=.75f && rearShots>=5,"Final assault must engage rear defenses.");
Check(game.Match.CompletedWaves==5 && game.Match.Killed+game.Match.Escaped==216,"All waves and units resolve once.");
await System.Threading.Tasks.Task.Delay(200);
ScreenCapture.CaptureScreenshot(System.IO.Path.Combine(System.IO.Path.GetTempPath(),"the-line-holds-victory.png"));
await System.Threading.Tasks.Task.Delay(150);
Debug.Log($"[Standalone verification] Victory: {game.Match.Stars} stars, {game.Match.Lives} lives, {game.Match.Killed} kills, {game.Match.Escaped} escaped, {Time.realtimeSinceStartup-started:0}s real time, {Time.frameCount-matchFrames} rendered frames; final depth={finalDepth:P0}; rear shots={rearShots}.");
return $"Victory; stars={game.Match.Stars}; lives={game.Match.Lives}; killed={game.Match.Killed}; escaped={game.Match.Escaped}; seconds={Time.realtimeSinceStartup-started:0}; frames={Time.frameCount-matchFrames}; finalDepth={finalDepth:0.000}; rearShots={rearShots}";
