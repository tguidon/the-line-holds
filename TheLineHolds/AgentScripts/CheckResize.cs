// Standalone-only resize/zoom regression, run through Pipeline script eval.
using TheLineHolds;
using UnityEngine;
using UnityEngine.UI;
using System.Linq;
using System.Threading.Tasks;
var game=Object.FindAnyObjectByType<BattleController>();
void Check(bool condition,string message){if(!condition)throw new System.Exception(message);}
Rect Bounds(RectTransform t){var p=new Vector3[4];t.GetWorldCorners(p);return Rect.MinMaxRect(p[0].x,p[0].y,p[2].x,p[2].y);}
var report=new System.Text.StringBuilder();
foreach(var size in new[]{new Vector2Int(1600,900),new Vector2Int(1200,1000),new Vector2Int(1024,768),new Vector2Int(1920,1080)})
{
    Screen.SetResolution(size.x,size.y,FullScreenMode.Windowed);await Task.Delay(500);
    game.NewMatch();game.hud.Refresh();Canvas.ForceUpdateCanvases();await Task.Delay(100);
    var labels=game.hud.GetComponentsInChildren<Component>().Where(c=>c.GetType().Name=="TextMeshProUGUI").ToArray();
    string Text(Component c)=>(string)c.GetType().GetProperty("text").GetValue(c);var buttons=game.hud.GetComponentsInChildren<Button>();
    var status=labels.Single(t=>Text(t).StartsWith("LIVES"));var briefing=labels.Single(t=>Text(t).StartsWith("ORDERS REQUIRED"));
    var start=buttons.Single(b=>b.name.StartsWith("START WAVE"));var pause=buttons.Single(b=>b.name.StartsWith("II"));
    Check(!Bounds((RectTransform)status.transform).Overlaps(Bounds((RectTransform)pause.transform)),"status overlaps Pause");
    Check(!Bounds((RectTransform)briefing.transform).Overlaps(Bounds((RectTransform)start.transform)),"briefing overlaps Start");
    bool Truncated(Component c)=>(bool)c.GetType().GetProperty("isTextTruncated").GetValue(c);
    Check(!Truncated(status) && !Truncated(briefing),"counters/briefing truncated");
    string root=System.IO.Path.GetTempPath();
    ScreenCapture.CaptureScreenshot(System.IO.Path.Combine(root,$"the-line-holds-hud-{size.x}x{size.y}.png"));await Task.Delay(150);
    game.CameraControls.Zoom(MapCameraControls.MinimumZoom-game.battleCamera.orthographicSize);game.CameraControls.Rotate(135);
    await Task.Delay(150);
    ScreenCapture.CaptureScreenshot(System.IO.Path.Combine(root,$"the-line-holds-road-close-{size.x}x{size.y}.png"));await Task.Delay(150);
    report.AppendLine($"PASS native resize {Screen.width}x{Screen.height}, separate UI regions, close zoom {game.battleCamera.orthographicSize}, orbit 135 degrees");
}
Screen.SetResolution(1600,900,FullScreenMode.Windowed);await Task.Delay(400);game.MainMenu();
Debug.Log("[Resize verification]\n"+report);return report.ToString();
