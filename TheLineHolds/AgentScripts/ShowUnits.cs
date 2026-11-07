using TheLineHolds;
using UnityEditor;
using UnityEngine;
var game=Object.FindAnyObjectByType<BattleController>();game.NewMatch();
foreach(var name in new[]{"Tower visual review","Unit visual review"}){var old=GameObject.Find(name);if(old)Object.DestroyImmediate(old);}
var review=new GameObject("Unit visual review");
var infantry=AssetDatabase.LoadAssetAtPath<EnemyDefinition>("Assets/TheLineHolds/Data/Enemies/Infantry.asset");
var tank=AssetDatabase.LoadAssetAtPath<EnemyDefinition>("Assets/TheLineHolds/Data/Enemies/LightTank.asset");
for(int i=0;i<6;i++)
{
    var p=BattleRoute.OnGround(-6.6f+(i%2)*.38f,-4.9f+(i/2)*.45f);
    var soldier=Object.Instantiate(infantry.prefab,p,Quaternion.Euler(0,210,0),review.transform);
    soldier.GetComponent<InfantryVisual>().Pose(i*.2f,i);
}
Object.Instantiate(tank.prefab,BattleRoute.OnGround(-3.8f,-4.5f),Quaternion.Euler(0,230,0),review.transform);
game.CameraControls.Zoom(-8);
var focus=new Vector3(-5.4f,.4f,-4.7f);game.battleCamera.transform.position=focus-game.battleCamera.transform.forward*40;
await System.Threading.Tasks.Task.Delay(500);
Debug.Log("[Visual review] Six independently posed infantry and the refined light tank.");return Time.frameCount;
