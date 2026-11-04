// Run via `script eval --file AgentScripts/Playthrough.cs` in Editor or a development Player.
// Uses real frame-driven combat, actual purchases, and controller callbacks. No funds, health, or wave cheats.
using TheLineHolds;
using UnityEngine;
using Unity.Pipeline.Scripting;
var game=Object.FindAnyObjectByType<BattleController>();
void Check(bool condition,string message) { if(!condition)throw new System.Exception(message); }
game.NewMatch();
game.Choose(0);game.ClickPad(3);game.ClickPad(4);game.Choose(2);game.ClickPad(6);game.Cancel();
Check(game.Match.Funds==0,"Starting purchases must spend exactly 300.");
int matchFrames=Time.frameCount;float started=Time.realtimeSinceStartup;
while(!game.Match.Ended)
{
    if(game.Match.Phase==MatchPhase.Preparation)
    {
        int[] pads={3,4,6,7,5,1,8,2,9,0}, types={0,0,2,1,2,0,1,2,0,0};
        for(int i=0;i<pads.Length;i++)
        {
            int pad=pads[i],type=types[i];
            if(game.Match.Towers[pad]==null && game.Match.Funds<game.defenses[type].cost)break;
            if(game.Match.Towers[pad]==null && game.Match.Funds>=game.defenses[type].cost) { game.Choose(type);game.ClickPad(pad); }
        }
        game.Cancel();game.StartNextWave();
    }
    await Wait.Until(()=>game.Match.Ended || game.Match.Phase==MatchPhase.Preparation,timeoutMs:180000,because:"wave should resolve");
}
Check(game.Match.Phase==MatchPhase.Victory,"The match must finish in victory.");
Check(game.Match.CompletedWaves==5 && game.Match.Killed+game.Match.Escaped==171,"All waves and units resolve once.");
Debug.Log($"[Standalone verification] Victory: {game.Match.Stars} stars, {game.Match.Lives} lives, {game.Match.Killed} kills, {game.Match.Escaped} escaped, {Time.realtimeSinceStartup-started:0}s real time, {Time.frameCount-matchFrames} rendered frames.");
return $"Victory; stars={game.Match.Stars}; lives={game.Match.Lives}; killed={game.Match.Killed}; escaped={game.Match.Escaped}; seconds={Time.realtimeSinceStartup-started:0}; frames={Time.frameCount-matchFrames}";
