using System;
using System.Diagnostics;
using System.IO;
using System.Threading.Tasks;
using TheLineHolds;
using UnityEngine;
using Unity.Pipeline.Scripting;

public static class ReviewMiniatures
{
    public static async Task<string> Capture(string pass)
    {
        if (pass != "before" && pass != "after") throw new ArgumentException(nameof(pass));
        var g = UnityEngine.Object.FindAnyObjectByType<BattleController>();
        var directory = Path.GetFullPath("Screenshots");
        void CaptureFrame(string name) => ScreenCapture.CaptureScreenshot(Path.Combine(directory, "miniatures-" + name + ".png"));
        void Build(int pad, int role)
        {
            g.Choose(role);
            g.ClickPad(pad);
            g.Cancel();
            if (g.Match.Towers[pad]?.Definition != g.defenses[role]) throw new Exception("Purchase failed on site " + pad);
        }
        async Task FinishWave()
        {
            await Wait.Until(() => g.Match.Phase == MatchPhase.Preparation || g.Match.Ended, timeoutMs: 120000, because: "wave completion");
            if (g.Match.Ended) throw new Exception("Match ended early");
        }
        g.NewMatch(42);
        Build(3, 0);
        Build(7, 1);
        try
        {
            Time.timeScale = 3;
            g.StartNextWave();
            await FinishWave();
            Build(4, 2);
            Time.timeScale = 1;
            await Task.Delay(1000);
            CaptureFrame(pass);
            await Task.Delay(500);
            g.StartNextWave();
            await Wait.Until(() => g.Match.Enemies.Count >= 4, timeoutMs: 40000, because: "active second wave");
            int startFrame = Time.frameCount;
            var timer = Stopwatch.StartNew();
            await Task.Delay(8000);
            float fps = (Time.frameCount - startFrame) / (float)timer.Elapsed.TotalSeconds;
            Time.timeScale = 3;
            await FinishWave();
            g.ClickPad(3);
            g.UpgradeSelected(0);
            if (g.Match.Towers[3].Definition.tier != 2) throw new Exception("Veteran purchase failed");
            g.Cancel();
            g.StartNextWave();
            if (pass == "after")
            {
                await Wait.Until(() => g.Match.Enemies.Count >= 8 && g.combatEffects.ActiveProjectiles > 0, timeoutMs: 50000, because: "cluster under fire");
                Time.timeScale = 1;
                CaptureFrame("wave");
                await Task.Delay(250);
            }
            Time.timeScale = 3;
            await FinishWave();
            return $"{pass}, {Screen.width}x{Screen.height}, zoom {g.battleCamera.orthographicSize:F3}, wave 2 sample {fps:F1} fps, {g.Match.CompletedWaves} waves, {g.Match.Lives} lives, {g.Match.Killed} kills, {g.Match.Escaped} escapes; MG, AT, mortar placed, veteran MG purchased";
        }
        finally { Time.timeScale = 1; }
    }
}
