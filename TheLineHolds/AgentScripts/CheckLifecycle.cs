using TheLineHolds;
using UnityEngine;
using Unity.Pipeline.Scripting;
var game=Object.FindAnyObjectByType<BattleController>();
void Check(bool condition,string message) { if(!condition)throw new System.Exception(message); }
if(!game)
{
    Object.FindAnyObjectByType<TitleMenu>().countryside.onClick.Invoke();
    await Wait.Until(()=>Object.FindAnyObjectByType<BattleController>()?.Match!=null,timeoutMs:15000);
    game=Object.FindAnyObjectByType<BattleController>();
}
game.NewMatch();game.Choose(0);game.ClickPad(3);game.StartNextWave();
await Wait.Until(()=>game.Match.Enemies.Count>0,timeoutMs:10000);
game.Choose(1);game.Escape();Check(game.Placement==-1 && !game.Match.Paused,"Escape first cancels placement.");
game.Pause();Check(game.Match.Paused,"Pause state entered.");
float distance=game.Match.Enemies[0].Distance;int remaining=game.Match.Remaining;int shotCount=game.Match.Shots.Count;
await System.Threading.Tasks.Task.Delay(1000);
Check(game.Match.Enemies[0].Distance==distance && game.Match.Remaining==remaining && game.Match.Shots.Count==shotCount,"Pause freezes route, spawns, and shots across real frames.");
game.Pause();await Wait.Until(()=>game.Match.Enemies[0].Distance>distance,timeoutMs:5000);
game.NewMatch();await System.Threading.Tasks.Task.Yield();
Check(game.Match.Funds==300 && game.Match.Lives==20 && game.Match.Wave==0 && game.Match.Killed==0 && game.Match.Escaped==0,"Retry resets all match counters.");
Check(game.Match.Enemies.Count==0 && game.Match.Shots.Count==0 && System.Linq.Enumerable.All(game.Match.Towers,t=>t==null),"Retry resets enemies, shots, and towers.");
game.MainMenu();await System.Threading.Tasks.Task.Delay(100);
Check(!game && Object.FindAnyObjectByType<TitleMenu>(),"Main menu unloads the match and its views.");
Debug.Log("[Lifecycle verification] PASS cancellation priority, pause freeze/resume, restart counters, menu cleanup.");
return "PASS cancellation priority, pause freeze/resume, restart counters, menu cleanup";
