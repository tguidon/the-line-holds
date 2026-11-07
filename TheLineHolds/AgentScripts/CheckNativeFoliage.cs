using UnityEngine;
using TheLineHolds;
using System.Linq;
using Unity.Pipeline.Scripting;
Debug.developerConsoleVisible=false;
var game=Object.FindAnyObjectByType<BattleController>();game.NewMatch();
foreach(var definition in game.defenses)
{
    var count=definition.prefab.GetComponentsInChildren<Transform>().Count(t=>t.name.StartsWith("Sandbag"));
    if(count!=28 && count!=32)throw new System.Exception("Incomplete native sandbag wall: "+definition.name);
}
var leaves=Object.FindObjectsByType<MeshFilter>().Where(m=>m.name=="Broadleaf canopy" || m.name.StartsWith("Leafy shrubs Leaf")).ToArray();
if(leaves.Length<12)throw new System.Exception("Leaf-based map foliage missing");
foreach(var filter in leaves)
{
    var mesh=filter.sharedMesh;
    if(!mesh.HasVertexAttribute(UnityEngine.Rendering.VertexAttribute.TexCoord0) || !mesh.HasVertexAttribute(UnityEngine.Rendering.VertexAttribute.Tangent))throw new System.Exception("Missing native leaf surface data");
    if(filter.GetComponent<Renderer>().sharedMaterials.Any(m=>!m.mainTexture || m.mainTexture.name!="Broadleaf veins"))throw new System.Exception("Native leaf veins missing");
}
game.Choose(0);game.ClickPad(3);game.ClickPad(4);game.Choose(2);game.ClickPad(6);game.Cancel();game.StartNextWave();
await Wait.Until(()=>game.Match.Enemies.Count>=8,timeoutMs:15000);
float started=Time.realtimeSinceStartup;int frame=Time.frameCount;
await System.Threading.Tasks.Task.Delay(3000);
float fps=(Time.frameCount-frame)/(Time.realtimeSinceStartup-started);
ScreenCapture.CaptureScreenshot(System.IO.Path.Combine(System.IO.Path.GetTempPath(),"foliage-native-overview.png"));await System.Threading.Tasks.Task.Delay(250);
game.CameraControls.Zoom(-8);var focus=BattleRoute.OnGround(-5,1);game.battleCamera.transform.position=focus-game.battleCamera.transform.forward*40;
await System.Threading.Tasks.Task.Delay(250);
ScreenCapture.CaptureScreenshot(System.IO.Path.Combine(System.IO.Path.GetTempPath(),"foliage-native-close.png"));await System.Threading.Tasks.Task.Delay(250);
string result=$"PASS native circular walls and leaf textures/UVs/tangents; {leaves.Length} leaf renderers; {fps:0.0} fps during combat; frames advanced {Time.frameCount-frame}";
Debug.Log("[Foliage refinement] "+result);game.MainMenu();return result;
