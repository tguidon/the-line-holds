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
game.MainMenu();await System.Threading.Tasks.Task.Delay(100);Click("DEPLOY");
Check(!game.InMenu && game.Match.Funds==300,"Play button creates match");
await System.Threading.Tasks.Task.Delay(100);Click("Shop Machine");Check(game.Placement==0,"Shop button selects defense");
game.ClickPad(3);game.Cancel();Click("START WAVE");Check(game.Match.Phase==MatchPhase.Combat,"Start Wave button starts combat");
Click("II");Check(game.Match.Paused,"Pause button pauses");await System.Threading.Tasks.Task.Delay(100);Click("RESUME");Check(!game.Match.Paused,"Resume button resumes");
game.MainMenu();return "PASS menu, shop, Start Wave, Pause, and Resume button raycasts and callbacks";
