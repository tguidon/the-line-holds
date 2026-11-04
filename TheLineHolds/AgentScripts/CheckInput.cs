// Injects Input System device events across real frames, without invoking game or UI callbacks.
// Temporary devices and settings are restored even if an assertion fails.
using System;
using System.Linq;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.LowLevel;
using UnityEngine.UI;
using TheLineHolds;
public static class CheckInput
{
    public static async Task<string> All()
    {
        var game = UnityEngine.Object.FindAnyObjectByType<BattleController>();
        if (!Application.isPlaying || game == null) throw new Exception("Enter prototype Play Mode first.");
        var settings = InputSystem.settings;
        var background = settings.backgroundBehavior;
        var originals = InputSystem.devices.Where(d => d is Mouse || d is Keyboard).Select(d => (device:d, enabled:d.enabled)).ToArray();
        Mouse mouse = null;
        Keyboard keyboard = null;
        Vector2 pointer = Vector2.zero;
        void Check(bool condition, string message) { if (!condition) throw new Exception("FAILED: " + message); }
        Task Frame() => Task.Delay(80);
        void MouseEvent(ushort buttons = 0, float scroll = 0)
        {
            InputSystem.QueueStateEvent(mouse, new MouseState { position = pointer, buttons = buttons, scroll = new Vector2(0, scroll) });
        }
        async Task Click(Vector2 position, ushort button = 1)
        {
            pointer = position; MouseEvent(); await Frame();
            MouseEvent(button); await Frame();
            MouseEvent(); await Frame();
        }
        async Task Key(Key key)
        {
            InputSystem.QueueStateEvent(keyboard, new KeyboardState(key)); await Frame();
            InputSystem.QueueStateEvent(keyboard, new KeyboardState()); await Frame();
        }
        async Task Button(string prefix)
        {
            var button = UnityEngine.Object.FindObjectsByType<Button>().Single(b => b.name.StartsWith(prefix, StringComparison.Ordinal));
            var rect = (RectTransform)button.transform;
            await Click(RectTransformUtility.WorldToScreenPoint(null, rect.TransformPoint(rect.rect.center)));
        }
        Vector2 Pad(int index) => game.battleCamera.WorldToScreenPoint(game.padViews[index].position);
        try
        {
            settings.backgroundBehavior = InputSettings.BackgroundBehavior.IgnoreFocus;
            foreach (var original in originals) if (original.enabled) InputSystem.DisableDevice(original.device);
            mouse = InputSystem.AddDevice<Mouse>("PrototypeInputCheckMouse");
            keyboard = InputSystem.AddDevice<Keyboard>("PrototypeInputCheckKeyboard");
            mouse.MakeCurrent(); keyboard.MakeCurrent();
            game.MainMenu(); await Frame();
            await Button("DEPLOY");
            Check(!game.InMenu && game.Match.Funds == 300, "mouse starts match");
            await Button("Shop Machine"); Check(game.Placement == 0, "mouse selects defense");
            pointer = Pad(3); MouseEvent(1); await Frame(); MouseEvent(); await Frame();
            Check(game.Match.Towers[3] != null && game.Match.Funds == 225, "moving from shop to pad and clicking in one input frame places tower");
            await Click(Pad(3)); Check(game.SelectedPad == 3 && game.Match.Funds == 225, "mouse inspects occupied pad without spending");
            await Button("SELL"); Check(game.Match.Towers[3] == null && game.Match.Funds == 262, "mouse sells for floor half price");
            await Button("Shop Mortar");
            await Click(Pad(6)); Check(game.Match.Towers[6] != null && game.Match.Funds == 112, "mouse places mortar");
            await Click(Pad(1)); Check(game.Match.Towers[1] == null && game.Match.Funds == 112, "unaffordable placement preserves funds");
            await Click(Pad(1), 2); Check(game.Placement == -1, "right-click cancels placement");
            await Button("Shop Anti"); await Key(UnityEngine.InputSystem.Key.Escape);
            Check(game.Placement == -1 && !game.Match.Paused, "Escape cancels placement before pausing");
            float zoom = game.battleCamera.orthographicSize;
            MouseEvent(0, 120); await Frame(); Check(game.battleCamera.orthographicSize < zoom, "wheel zooms");
            MouseEvent(0, 4000); await Frame(); Check(Mathf.Approximately(game.battleCamera.orthographicSize, 12.5f), "zoom minimum clamps");
            MouseEvent(0, -4000); await Frame(); Check(Mathf.Approximately(game.battleCamera.orthographicSize, 15), "zoom maximum clamps");
            await Button("START WAVE"); Check(game.Match.Phase == MatchPhase.Combat, "mouse starts wave");
            await Key(UnityEngine.InputSystem.Key.Escape); Check(game.Match.Paused, "Escape pauses combat");
            await Button("RESUME"); Check(!game.Match.Paused, "mouse resumes combat");
            await Key(UnityEngine.InputSystem.Key.Escape); await Button("RETRY");
            Check(game.Match.Wave == 0 && game.Match.Funds == 300 && game.Match.Lives == 20, "pause Restart resets match");
            await Key(UnityEngine.InputSystem.Key.Escape); await Button("MAIN MENU");
            Check(game.InMenu && game.Match == null, "pause Main Menu clears match");
            const string result = "PASS frame-driven mouse placement/inspection/selling, rejected purchases, right-click, Escape priority, wheel limits, wave start, pause/resume/restart/menu";
            Debug.Log("[Input verification] " + result);
            return result;
        }
        finally
        {
            if (mouse != null) InputSystem.RemoveDevice(mouse);
            if (keyboard != null) InputSystem.RemoveDevice(keyboard);
            foreach (var original in originals) if (original.enabled) InputSystem.EnableDevice(original.device);
            settings.backgroundBehavior = background;
            game.MainMenu();
        }
    }
}
