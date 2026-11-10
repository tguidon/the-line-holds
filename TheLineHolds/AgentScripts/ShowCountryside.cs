using UnityEngine;
using TheLineHolds;
public static class ShowCountryside
{
 public static string Panorama(){var g=Object.FindAnyObjectByType<BattleController>();g.NewMatch(42);g.CameraControls.Zoom(6);g.hud.Refresh();return "Playable overview zoomed out for scenery review";}
 public static string Village(){var g=Object.FindAnyObjectByType<BattleController>();g.CameraControls.enabled=false;var p=BattleRoute.OnGround(15,13);g.battleCamera.transform.position=p-g.battleCamera.transform.forward*40;g.battleCamera.orthographicSize=6;g.hud.GetComponent<Canvas>().enabled=false;return "Temporary village architecture close-up";}
 public static string Farm(){var g=Object.FindAnyObjectByType<BattleController>();var p=BattleRoute.OnGround(-22,-15);g.battleCamera.transform.position=p-g.battleCamera.transform.forward*40;g.battleCamera.orthographicSize=5.5f;return "Temporary dairy/cottage/tool-shed detail";}
 public static string Reset(){var g=Object.FindAnyObjectByType<BattleController>();g.hud.GetComponent<Canvas>().enabled=true;g.CameraControls.enabled=true;g.CameraControls.ResetView();g.MainMenu();return "Review camera and HUD restored";}
}
