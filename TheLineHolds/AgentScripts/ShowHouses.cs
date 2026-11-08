using UnityEngine;
using UnityEditor;
using TheLineHolds;
var game=Object.FindAnyObjectByType<BattleController>();game.NewMatch();
var previous=GameObject.Find("House visual review");if(previous)Object.DestroyImmediate(previous);
var review=new GameObject("House visual review");
var prefab=AssetDatabase.LoadAssetAtPath<GameObject>("Assets/TheLineHolds/Prefabs/Farmhouse.prefab");
for(int i=0;i<2;i++)
{
    var view=Object.Instantiate(prefab,BattleRoute.OnGround(-1.5f+i*3,0),Quaternion.Euler(0,i*95,0),review.transform);
    view.transform.localScale=Vector3.one*.8f;
}
game.CameraControls.enabled=false;game.battleCamera.orthographicSize=3.1f;
var focus=new Vector3(0,1.4f,0);game.battleCamera.transform.position=focus-game.battleCamera.transform.forward*40;
await System.Threading.Tasks.Task.Delay(500);
Debug.Log("[House review] Two temporary orientations; crisp masonry, closed gables, continuous pitched slate roofs.");return Time.frameCount;
