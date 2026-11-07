using TheLineHolds;
using UnityEngine;
var game=Object.FindAnyObjectByType<BattleController>();game.NewMatch();
var showcase=new GameObject("Tower visual review");
for(int i=0;i<3;i++)
{
    var p=new Vector3(-3.8f+i*2.4f,0,1.5f);p.y=BattleRoute.Ground(p.x,p.z)+.12f;
    var view=Object.Instantiate(game.defenses[i].prefab,p,Quaternion.identity,showcase.transform);
    view.GetComponent<TowerVisual>().Aim(new Vector3(-.3f,0,-1),0);
}
game.CameraControls.Zoom(-8);
var focus=new Vector3(-1.4f,1,1.5f);game.battleCamera.transform.position=focus-game.battleCamera.transform.forward*40;
await System.Threading.Tasks.Task.Delay(500);
Debug.Log("[Visual review] Three tower roles, fixed sandbags, independent traverse assemblies.");
return Time.frameCount;
