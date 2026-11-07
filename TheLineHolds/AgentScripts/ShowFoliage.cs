using UnityEngine;
using TheLineHolds;
var game=Object.FindAnyObjectByType<BattleController>();game.NewMatch();
foreach(var name in new[]{"Tower visual review","Unit visual review"}){var old=GameObject.Find(name);if(old)Object.DestroyImmediate(old);}
game.CameraControls.enabled=false;
game.battleCamera.orthographicSize=2.8f;
var focus=BattleRoute.OnGround(2.8f,6.1f)+Vector3.up*1.1f;
game.battleCamera.transform.position=focus-game.battleCamera.transform.forward*40;
await System.Threading.Tasks.Task.Delay(500);
Debug.Log("[Foliage refinement] Leaf/vein detail review at 2.8 orthographic size; gameplay minimum zoom remains unchanged.");return Time.frameCount;
