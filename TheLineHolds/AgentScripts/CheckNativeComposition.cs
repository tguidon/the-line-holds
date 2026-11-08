using UnityEngine;
using TheLineHolds;
using System.Linq;
var game=Object.FindAnyObjectByType<BattleController>();game.NewMatch();
if(game.padViews.Length!=10 || game.padViews.Any(p=>p.GetComponent<Renderer>()))throw new System.Exception("Site anchors or pedestal removal invalid");
var earth=GameObject.Find("Exposed earth layers").GetComponent<MeshRenderer>();
if(earth.sharedMaterials.Length!=3 || earth.sharedMaterials.Any(m=>!m.mainTexture || m.mainTexture.width<512))throw new System.Exception("Detailed soil horizons missing in player");
if(!earth.GetComponent<MeshFilter>().sharedMesh.HasVertexAttribute(UnityEngine.Rendering.VertexAttribute.Tangent))throw new System.Exception("Native soil tangent basis missing");
var details=GameObject.Find("Authored battlefield details");
if(!details || !GameObject.Find("Enemy entry gateway") || !GameObject.Find("Defended exit checkpoint"))throw new System.Exception("Landmark composition missing");
if(details.GetComponentsInChildren<Renderer>().Count(r=>r.sharedMaterial.name.StartsWith("Road sign "))!=2)throw new System.Exception("Two physical painted signs missing");
await System.Threading.Tasks.Task.Delay(400);
var temp=System.IO.Path.GetTempPath();
ScreenCapture.CaptureScreenshot(System.IO.Path.Combine(temp,"composition-native-overview.png"));await System.Threading.Tasks.Task.Delay(200);
game.CameraControls.Rotate(135);await System.Threading.Tasks.Task.Delay(250);
ScreenCapture.CaptureScreenshot(System.IO.Path.Combine(temp,"composition-native-rotated.png"));await System.Threading.Tasks.Task.Delay(200);
game.CameraControls.ResetView();game.CameraControls.enabled=false;game.battleCamera.orthographicSize=3.2f;
foreach(var shot in new[]{new {name="entry",p=BattleRoute.OnGround(-10.8f,-4.6f)},new{name="exit",p=BattleRoute.OnGround(10.8f,4.7f)},new{name="soil",p=new Vector3(-3.5f,0,-6.4f)}})
{
    game.battleCamera.transform.position=shot.p-game.battleCamera.transform.forward*40;
    await System.Threading.Tasks.Task.Delay(250);
    ScreenCapture.CaptureScreenshot(System.IO.Path.Combine(temp,"composition-native-"+shot.name+".png"));await System.Threading.Tasks.Task.Delay(200);
}
game.CameraControls.enabled=true;game.CameraControls.ResetView();game.MainMenu();
string result="PASS ten ground sites without pedestals; three textured soil horizons with tangents; physical signs and endpoint landmarks; overview, orbit and three close views";
Debug.Log("[Map composition] "+result);return result;
