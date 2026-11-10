using System;
using System.Linq;
using System.Threading.Tasks;
using UnityEngine;
using TheLineHolds;
var g=UnityEngine.Object.FindAnyObjectByType<BattleController>();var originalSize=new Vector2Int(Screen.width,Screen.height);
void CheckFit()
{
 var points=BattleRoute.DefaultPoints.Concat(new[]{BattleRoute.OnGround(-12.25f,-6.6f),BattleRoute.OnGround(-12.25f,-6.6f)+Vector3.up*1.6f,BattleRoute.OnGround(12.25f,5.8f)+Vector3.up*1.6f});
 foreach(var p in points){var v=g.battleCamera.WorldToViewportPoint(p);if(v.z<=0 || v.x<.04f || v.x>.96f || v.y<.13f || v.y>.89f)throw new Exception($"Clipped playable area at {Screen.width}x{Screen.height}: {v}");}
}
var report=new System.Text.StringBuilder();
try
{
 g.NewMatch(42);
 foreach(var size in new[]{new Vector2Int(1600,900),new Vector2Int(1516,1324),new Vector2Int(1024,768),new Vector2Int(1024,1280),new Vector2Int(1920,1080)})
 {
  Screen.SetResolution(size.x,size.y,FullScreenMode.Windowed);await Task.Delay(500);CheckFit(); // Resize the untouched overview without asking for Reset.
  g.NewMatch(42);await Task.Delay(100);CheckFit();g.hud.Refresh();Canvas.ForceUpdateCanvases();
  ScreenCapture.CaptureScreenshot(System.IO.Path.Combine(System.IO.Path.GetTempPath(),$"overview-native-{size.x}x{size.y}.png"));await Task.Delay(100);
  report.AppendLine($"PASS {Screen.width}x{Screen.height}; overview={g.CameraControls.OverviewSize:0.00}; complete route and landmarks");
 }
 g.CameraControls.Zoom(-2);g.CameraControls.Rotate(25);float zoom=g.battleCamera.orthographicSize;var rotation=g.battleCamera.transform.rotation;
 Screen.SetResolution(1516,1324,FullScreenMode.Windowed);await Task.Delay(500);
 if(Mathf.Abs(g.battleCamera.orthographicSize-zoom)>.001f || Quaternion.Angle(rotation,g.battleCamera.transform.rotation)>.01f)throw new Exception("Resize unexpectedly resets a manually adjusted view");
 g.CameraControls.ResetView();CheckFit();return report+"PASS manual camera preserved on resize; Reset fits again";
}
finally{Screen.SetResolution(originalSize.x,originalSize.y,FullScreenMode.Windowed);await Task.Delay(400);g.CameraControls.ResetView();g.MainMenu();}
