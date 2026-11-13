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
        var editorRouting = settings.editorInputBehaviorInPlayMode;
        var originals = InputSystem.devices.Where(d => d is Mouse || d is Keyboard).Select(d => (device:d, enabled:d.enabled)).ToArray();
        Mouse mouse = null;
        Keyboard keyboard = null;
        Vector2 pointer = Vector2.zero;
        void Check(bool condition, string message) { if (!condition) throw new Exception("FAILED: " + message); }
        Task Frame() => Task.Delay(80);
        void MouseEvent(ushort buttons = 0, float scroll = 0, Vector2? delta = null)
        {
            InputSystem.QueueStateEvent(mouse, new MouseState { position = pointer, buttons = buttons, delta = delta ?? Vector2.zero, scroll = new Vector2(0, scroll) });
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
            prefix=prefix switch {"DEPLOY"=>"Play","Shop Machine"=>"Shop MG","SELL"=>"Sell","RESET VIEW"=>"Reset view",">"=>"Rotate right","+"=>"Zoom in","-"=>"Zoom out","START WAVE"=>"Send wave","RESUME"=>"Pause","RETRY"=>"Restart","MAIN MENU"=>"Main menu",_=>prefix};
            bool camera=prefix=="Reset view" || prefix=="Rotate right" || prefix=="Zoom in" || prefix=="Zoom out";
            var all=UnityEngine.Object.FindObjectsByType<Button>(FindObjectsInactive.Include);
            if((prefix=="Restart" || prefix=="Main menu") && !all.Any(b=>b.gameObject.activeInHierarchy && b.name.StartsWith(prefix,StringComparison.Ordinal)))await Button("Options");
            if(camera && !all.Single(b=>b.name==prefix).gameObject.activeInHierarchy)await Button("Help");
            if((prefix.StartsWith("Shop ") || prefix=="Send wave") && all.Single(b=>b.name=="Reset view").gameObject.activeInHierarchy)await Button("Help");
            var button = UnityEngine.Object.FindObjectsByType<Button>().Single(b => b.name.StartsWith(prefix, StringComparison.Ordinal));
            var rect = (RectTransform)button.transform;
            await Click(RectTransformUtility.WorldToScreenPoint(null, rect.TransformPoint(rect.rect.center)));
        }
        Vector2 Pad(int index) => game.battleCamera.WorldToScreenPoint(game.padViews[index].position);
        try
        {
            settings.backgroundBehavior = InputSettings.BackgroundBehavior.IgnoreFocus;
            settings.editorInputBehaviorInPlayMode = InputSettings.EditorInputBehaviorInPlayMode.AllDeviceInputAlwaysGoesToGameView;
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
            pointer = new Vector2(Screen.width*.5f,Screen.height*.5f); MouseEvent(); await Frame();
            var homePosition=game.battleCamera.transform.position;var homeRotation=game.battleCamera.transform.rotation;
            var landmark=GameObject.Find("Enemy entry gateway").transform;var landmarkRotation=landmark.rotation;
            float zoom = game.battleCamera.orthographicSize;
            MouseEvent(0, 120); await Frame(); Check(game.battleCamera.orthographicSize < zoom, "wheel zooms");
            MouseEvent(0, 4000); await Frame(); Check(Mathf.Approximately(game.battleCamera.orthographicSize, MapCameraControls.MinimumZoom), "close zoom minimum clamps");
            MouseEvent(0, -4000); await Frame(); Check(Mathf.Approximately(game.battleCamera.orthographicSize, game.CameraControls.OverviewSize), "zoom maximum clamps");
            await Key(UnityEngine.InputSystem.Key.R);
            Check(Vector3.Distance(homePosition,game.battleCamera.transform.position)<.001f && Mathf.Approximately(game.battleCamera.orthographicSize,game.CameraControls.OverviewSize), "R resets overview");
            await Key(UnityEngine.InputSystem.Key.E); Check(Quaternion.Angle(homeRotation,game.battleCamera.transform.rotation)>1,"E rotates map");
            await Key(UnityEngine.InputSystem.Key.Q);
            var rotation=game.battleCamera.transform.rotation;
            MouseEvent(4); await Frame(); MouseEvent(4,delta:new Vector2(120,0)); await Frame(); MouseEvent(); await Frame();
            Check(Quaternion.Angle(rotation,game.battleCamera.transform.rotation)>20,"middle drag rotates map");
            Check(Quaternion.Angle(landmarkRotation,landmark.rotation)<.01f,"physical landmarks remain fixed during camera orbit");
            await Button("RESET VIEW");await Button("Shop Machine");
            pointer=Pad(4);MouseEvent();await Frame();
            int fundsBeforeDrag=game.Match.Funds;
            var modifier=Application.platform==RuntimePlatform.OSXEditor?UnityEngine.InputSystem.Key.LeftMeta:UnityEngine.InputSystem.Key.LeftCtrl;
            InputSystem.QueueStateEvent(keyboard,new KeyboardState(modifier));await Frame();
            rotation=game.battleCamera.transform.rotation;
            MouseEvent(1);await Frame();MouseEvent(1,delta:new Vector2(120,0));await Frame();MouseEvent();
            InputSystem.QueueStateEvent(keyboard,new KeyboardState());await Frame();
            Check(Quaternion.Angle(rotation,game.battleCamera.transform.rotation)>20,"platform modifier plus left-drag rotates");
            Check(game.Match.Funds==fundsBeforeDrag && game.Match.Towers[4]==null,"modifier drag over an empty pad never places a tower");
            Check(game.Placement==0,"modifier drag preserves build selection");
            await Click(new Vector2(Screen.width*.5f,Screen.height*.5f),2);
            var panPosition=game.battleCamera.transform.position;rotation=game.battleCamera.transform.rotation;
            InputSystem.QueueStateEvent(keyboard,new KeyboardState(UnityEngine.InputSystem.Key.LeftShift));
            MouseEvent(4); await Frame(); MouseEvent(4,delta:new Vector2(80,50)); await Frame();MouseEvent();
            InputSystem.QueueStateEvent(keyboard,new KeyboardState());await Frame();
            Check(Vector3.Distance(panPosition,game.battleCamera.transform.position)>.3f && Quaternion.Angle(rotation,game.battleCamera.transform.rotation)<.01f,"Shift-middle drag pans without rotating");
            panPosition=game.battleCamera.transform.position;await Key(UnityEngine.InputSystem.Key.W);
            Check(Vector3.Distance(panPosition,game.battleCamera.transform.position)>.1f,"WASD pans");
            await Button("RESET VIEW");Check(Vector3.Distance(homePosition,game.battleCamera.transform.position)<.001f && Quaternion.Angle(homeRotation,game.battleCamera.transform.rotation)<.01f,"reset button restores view");
            await Button(">");Check(Quaternion.Angle(homeRotation,game.battleCamera.transform.rotation)>40,"rotate button works");
            await Button("+");Check(game.battleCamera.orthographicSize<game.CameraControls.OverviewSize,"zoom button works");
            await Button("Shop Machine");await Click(Pad(4));
            Check(game.Match.Towers[4]!=null && game.Match.Funds==37,"world placement works after rotation and zoom");
            await Button("-");zoom=game.battleCamera.orthographicSize;MouseEvent(0,120);await Frame();
            Check(Mathf.Approximately(zoom,game.battleCamera.orthographicSize),"scroll over UI does not zoom map");
            await Button("RESET VIEW");
            Check(game.Match.Wave==0 && game.Match.Killed==0 && game.Match.Enemies.Count==0,"camera controls do not advance combat or alter the match");
            await Click(new Vector2(Screen.width*.5f,Screen.height*.5f),2);
            await Button("START WAVE"); Check(game.Match.Phase == MatchPhase.Combat, "mouse starts wave");
            await Task.Delay(1500);await Key(UnityEngine.InputSystem.Key.Escape); Check(game.Match.Paused, "Escape pauses combat");
            var frozen=game.Match.Enemies.Select(e=>(e.Id,e.Distance,e.Health)).ToArray();var frozenShots=game.Match.Shots.Select(s=>(s.Id,s.Age)).ToArray();
            await Click(Pad(6));await Button("SELL");Check(game.Match.Towers[6]==null && game.Match.Funds==112,"Paused pointer sale");
            await Button("Shop Machine");await Click(Pad(6));Check(game.Match.Towers[6]!=null && game.Match.Funds==37,"Paused pointer build");
            await Click(Pad(6));await Button("Upgrade 0");Check(game.Match.Funds==37 && game.Match.Towers[6].Definition.tier==1,"Disabled upgrade consumed funds");
            rotation=game.battleCamera.transform.rotation;await Key(UnityEngine.InputSystem.Key.E);Check(Quaternion.Angle(rotation,game.battleCamera.transform.rotation)>1,"Camera blocked during pause");
            Check(frozen.SequenceEqual(game.Match.Enemies.Select(e=>(e.Id,e.Distance,e.Health))) && frozenShots.SequenceEqual(game.Match.Shots.Select(s=>(s.Id,s.Age))),"Combat advanced during paused pointer management");
            await Click(new Vector2(Screen.width*.5f,Screen.height*.5f),2);
            await Button("RESUME"); Check(!game.Match.Paused, "mouse resumes combat");
            await Key(UnityEngine.InputSystem.Key.Escape); await Button("RETRY");
            Check(game.Match.Wave == 0 && game.Match.Funds == 300 && game.Match.Lives == 20, "pause Restart resets match");
            await Key(UnityEngine.InputSystem.Key.Escape); await Button("MAIN MENU");
            Check(game.InMenu && game.Match == null, "pause Main Menu clears match");
            const string result = "PASS frame-driven mouse placement/inspection/selling, rejected purchases, right-click, Escape priority, close zoom limits, orbit/modifier-drag/pan/reset buttons and keys, rotated placement, UI scroll isolation, wave start, pause/resume/restart/menu";
            Debug.Log("[Input verification] " + result);
            return result;
        }
        finally
        {
            if (mouse != null) InputSystem.RemoveDevice(mouse);
            if (keyboard != null) InputSystem.RemoveDevice(keyboard);
            foreach (var original in originals) if (original.enabled) InputSystem.EnableDevice(original.device);
            settings.backgroundBehavior = background;
            settings.editorInputBehaviorInPlayMode = editorRouting;
            game.MainMenu();
        }
    }
}
