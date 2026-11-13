// Development player check. Compare identical opening encounters at the same window size and pace.
using System;
using System.Linq;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.UI;
using Unity.Pipeline.Scripting;
using TheLineHolds;
BattleController Game()=>UnityEngine.Object.FindAnyObjectByType<BattleController>();
var original=new Vector2Int(Screen.width,Screen.height);
var report=new System.Text.StringBuilder();
async Task Resize(int w,int h)
{
    Screen.SetResolution(w,h,FullScreenMode.Windowed);
    await Wait.Until(()=>Screen.width==w && Screen.height==h,timeoutMs:10000);
}
try
{
    await Resize(1600,900);
    foreach(bool winter in new[]{false,true})
    {
        var game=Game();if(game)game.MainMenu();
        await Wait.Until(()=>UnityEngine.Object.FindAnyObjectByType<TitleMenu>()!=null,timeoutMs:15000);
        string label=winter?"Winter Forest":"Countryside";
        UnityEngine.Object.FindObjectsByType<Button>().Single(b=>b.name==label).onClick.Invoke();
        await Wait.Until(()=>Game() && Game().Match!=null && Game().winterForest==winter,timeoutMs:20000);
        game=Game();game.NewMatch(42);
        game.Choose(0);game.ClickPad(3);game.ClickPad(4);game.Choose(2);game.ClickPad(6);game.Cancel();game.StartNextWave();
        await Wait.Until(()=>game.Match.Enemies.Any(e=>e.Distance>=0),timeoutMs:60000);
        int frames=Time.frameCount;float start=Time.realtimeSinceStartup;
        await Task.Delay(4000);
        float fps=(Time.frameCount-frames)/(Time.realtimeSinceStartup-start);
        report.AppendLine($"{label} at {Screen.width}x{Screen.height}: {fps:F1} FPS, {Time.frameCount-frames} frames");
        if(fps<35)throw new Exception(report.ToString());
    }
    foreach(var size in new[]{new Vector2Int(1024,768),new Vector2Int(1920,800)})
    {
        await Resize(size.x,size.y);
        var game=Game();game.NewMatch(42);game.StartNextWave();
        await Wait.Until(()=>game.Match.Enemies.Count>0,timeoutMs:15000);
        var enemy=game.Match.Enemies.First();
        if(GeometryUtility.TestPlanesAABB(GeometryUtility.CalculateFrustumPlanes(game.battleCamera),new Bounds(enemy.Position+Vector3.up,Vector3.one*2.5f)))
            throw new Exception("Spawn visible after resize");
        float spawn=enemy.Distance;
        await Wait.Until(()=>enemy.Distance>=0,timeoutMs:60000);
        report.AppendLine($"{size.x}x{size.y}: arrived on track from {spawn:F1} m");
    }
    return report.ToString();
}
finally{Screen.SetResolution(original.x,original.y,FullScreenMode.Windowed);if(Game())Game().MainMenu();}
