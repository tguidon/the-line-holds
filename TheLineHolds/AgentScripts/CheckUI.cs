using System.Linq;
using TheLineHolds;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
var game=Object.FindAnyObjectByType<BattleController>();
void Check(bool condition,string message) { if(!condition)throw new System.Exception(message); }
void Click(string startsWith)
{
    var button=Object.FindObjectsByType<Button>().First(b=>b.gameObject.name.StartsWith(startsWith));
    var rt=(RectTransform)button.transform;
    var pointer=new PointerEventData(EventSystem.current) { position=RectTransformUtility.WorldToScreenPoint(null,rt.TransformPoint(rt.rect.center)),button=PointerEventData.InputButton.Left };
    var hits=new System.Collections.Generic.List<RaycastResult>();EventSystem.current.RaycastAll(pointer,hits);
    Check(hits.Count>0 && hits[0].gameObject.GetComponentInParent<Button>()==button,"UI raycast reaches "+startsWith);
    ExecuteEvents.Execute(button.gameObject,pointer,ExecuteEvents.pointerClickHandler);
}
if(game)game.MainMenu();
await Unity.Pipeline.Scripting.Wait.Until(()=>Object.FindAnyObjectByType<TitleMenu>()!=null,timeoutMs:15000);
Click("Countryside");
await Unity.Pipeline.Scripting.Wait.Until(()=>Object.FindAnyObjectByType<BattleController>()?.Match!=null,timeoutMs:15000);
game=Object.FindAnyObjectByType<BattleController>();
Check(game.Match.Funds==300,"Photograph opens a fresh match");
Click("Shop MG");Check(game.Placement==0,"Shop button selects defense");
game.ClickPad(3);game.Cancel();
Click("Help");await System.Threading.Tasks.Task.Delay(100);
var rotation=game.battleCamera.transform.rotation;
Click("Rotate right");Check(Quaternion.Angle(rotation,game.battleCamera.transform.rotation)>40,"Rotate view button");
Click("Zoom in");Check(game.battleCamera.orthographicSize<game.CameraControls.OverviewSize,"Zoom view button");
Click("Reset view");Check(Quaternion.Angle(rotation,game.battleCamera.transform.rotation)<.01f,"Reset view button");
Click("Help");Click("Send wave");Check(game.Match.Phase==MatchPhase.Combat,"Send wave starts combat");
Click("Pause");Check(game.Match.Paused,"Pause freezes combat");
Click("Pause");Check(!game.Match.Paused,"Pause resumes combat");
game.MainMenu();return "PASS menu, shop, camera controls, wave dispatch, pause and resume raycasts and callbacks";
