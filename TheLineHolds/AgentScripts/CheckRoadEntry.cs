// Run in a development desktop player with script eval --file.
using System;
using System.Linq;
using System.Threading.Tasks;
using UnityEngine;
using Unity.Pipeline.Scripting;
using TheLineHolds;

var g=UnityEngine.Object.FindAnyObjectByType<BattleController>();
var initialSize=new Vector2Int(Screen.width,Screen.height);
var report=new System.Text.StringBuilder();
void Check(bool condition,string message){if(!condition)throw new Exception(message);}
async Task Resize(int w,int h)
{
    Screen.SetResolution(w,h,FullScreenMode.Windowed);
    await Wait.Until(()=>Screen.width==w && Screen.height==h,timeoutMs:8000,because:"resize the native game window");
    await Task.Delay(100);
}
async Task<EnemyState> Arrival(int lastId)
{
    await Wait.Until(()=>g.Match.Enemies.Any(e=>e.Id>lastId),timeoutMs:15000,because:"receive a new arrival");
    var unit=g.Match.Enemies.First(e=>e.Id>lastId);
    var planes=GeometryUtility.CalculateFrustumPlanes(g.battleCamera);
    Check(unit.Distance<0,"Spawn inside the gateway");
    Check(!GeometryUtility.TestPlanesAABB(planes,new Bounds(unit.Position+Vector3.up,Vector3.one*2.5f)),"Spawn visible in this camera");
    return unit;
}
try
{
    foreach(var size in new[]{new Vector2Int(1024,768),new Vector2Int(1600,900),new Vector2Int(1920,800)})
    {
        await Resize(size.x,size.y);
        g.NewMatch(42);g.Choose(0);g.ClickPad(0);g.Cancel();
        g.StartNextWave();
        var unit=await Arrival(0);
        float spawn=unit.Distance;
        await Wait.Until(()=>unit.Distance>=0,timeoutMs:60000,because:"march along the approach and through the gate");
        Check(Vector3.Distance(unit.Position,g.Match.Route.Points[0])<1.6f,"Missed entrance landmark");
        report.AppendLine($"{size.x}x{size.y}: offscreen spawn {spawn:F1} m from gateway, crossed on road");
    }
    g.NewMatch(42);g.StartNextWave();await Arrival(0);
    await Resize(1200,900);g.CameraControls.Rotate(115);g.CameraControls.Zoom(5);
    var latest=g.Match.Enemies.Max(e=>e.Id);
    await Arrival(latest);
    report.AppendLine("Spawn stayed offscreen after resizing and orbiting during the wave");
    var route=g.Match.Route;
    for(int site=0;site<g.Match.Pads.Length;site++)
    {
        var pad=g.Match.Pads[site];
        Check(Vector3.Distance(pad,g.padViews[site].position)<.005f,"Build click target does not match its pad");
        float nearest=float.MaxValue;
        for(float d=0;d<=route.Length;d+=.05f){
            var delta=route.Position(d)-pad;
            nearest=Mathf.Min(nearest,new Vector2(delta.x,delta.z).magnitude);
        }
        Check(nearest>=2.47f,"Pad too close to traffic: "+nearest);
        Check(nearest<g.defenses.Min(w=>w.range),"Base weapon cannot reach closest traffic");
    }
    return report.ToString()+"All ten pads have roadside clearance and can engage the road with every base weapon.";
}
finally
{
    Screen.SetResolution(initialSize.x,initialSize.y,FullScreenMode.Windowed);
    g.MainMenu();
}
