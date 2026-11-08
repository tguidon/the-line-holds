using UnityEngine;
using TheLineHolds;
var game=Object.FindAnyObjectByType<BattleController>();game.NewMatch();
game.CameraControls.enabled=false;game.battleCamera.orthographicSize=3.2f;
var focus=new Vector3(-3.5f,0,-6.4f);game.battleCamera.transform.position=focus-game.battleCamera.transform.forward*40;
await System.Threading.Tasks.Task.Delay(500);Debug.Log("[Map composition] Close review of cut soil, roots, stone, field position and ground site.");return Time.frameCount;
