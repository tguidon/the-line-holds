using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.EventSystems;
namespace TheLineHolds
{
    public sealed class BattleController : MonoBehaviour
    {
        public WaveDefinition[] waves;
        public DefenseDefinition[] defenses;
        public Transform dynamicRoot;
        public Transform[] padViews;
        public Camera battleCamera;
        public Material tracerMaterial, rangeMaterial, healthMaterial, damageMaterial, smokeMaterial;
        public GameHud hud;
        public BattleAudio audioPlayer;
        public MatchState Match { get; private set; }
        public int Placement { get; private set; } = -1;
        public int SelectedPad { get; private set; } = -1;
        public string Notice { get; private set; } = "Choose a defense, then click a cleared gun site.";
        public bool InMenu { get; private set; } = true;
        readonly Dictionary<int, GameObject> enemyViews = new Dictionary<int, GameObject>();
        readonly Dictionary<int, Transform> healthViews = new Dictionary<int, Transform>();
        readonly Dictionary<int, GameObject> towerViews = new Dictionary<int, GameObject>();
        public CombatEffects combatEffects;
        GameObject preview;
        LineRenderer ring;
        int hover = -1, previewType = -1;
        public MapCameraControls CameraControls { get; private set; }
        void Start()
        {
            Application.targetFrameRate=60;
            ring = new GameObject("Range preview").AddComponent<LineRenderer>(); ring.transform.SetParent(dynamicRoot);
            ring.sharedMaterial=rangeMaterial; ring.positionCount=97; ring.loop=false; ring.widthMultiplier=.055f; ring.useWorldSpace=true;
            CameraControls=battleCamera.gameObject.AddComponent<MapCameraControls>();
            CameraControls.Initialize(battleCamera,this);
            hud.Initialize(this); MainMenu();
        }
        public void NewMatch()
        {
            Unsubscribe(); ClearViews(); audioPlayer.ResetAudio(); InMenu=false; Placement=SelectedPad=-1;
            Match=new MatchState(waves,new BattleRoute(BattleRoute.DefaultPoints),BattleRoute.DefaultPads);
            Match.Cue += OnCue; Match.ShotFired += OnShotFired; Match.ShotImpacted += OnShotImpacted;
            CameraControls.ResetView(); Notice="Deploy your defenses. Wave 1 waits for your order.";
        }
        public void MainMenu()
        {
            Unsubscribe();
            Match=null; InMenu=true; Placement=SelectedPad=-1; ClearViews(); audioPlayer.ResetAudio();
        }
        void ClearViews()
        {
            foreach(var v in enemyViews.Values) Destroy(v); enemyViews.Clear(); healthViews.Clear();
            foreach(var v in towerViews.Values) Destroy(v); towerViews.Clear();
            if(combatEffects) combatEffects.Clear();
            if(preview) Destroy(preview); previewType=-1; hover=-1;
            if(ring) ring.enabled=false;
            if(audioPlayer) audioPlayer.SetPaused(false);
        }
        public void Choose(int type)
        {
            if(Match==null || Match.Ended || Match.Paused) return;
            Placement=type; SelectedPad=-1; Notice=defenses[type].description;
        }
        public void Cancel() { Placement=SelectedPad=-1; Notice="Choose a defense, then click an emplacement."; }
        public void Pause()
        {
            if(Match==null || Match.Ended) return;
            if(Placement>=0) { Cancel(); return; }
            Match.Paused=!Match.Paused; audioPlayer.SetPaused(Match.Paused);
        }
        public void StartNextWave()
        {
            if(Match!=null && Match.StartWave()) { Cancel(); Notice=waves[Match.Wave-1].briefing; }
        }
        public void SellSelected()
        {
            if(Match!=null && Match.Sell(SelectedPad)) { Notice="Defense sold. Half its purchase cost returned."; SelectedPad=-1; }
        }
        void Update()
        {
            if(Keyboard.current!=null && Keyboard.current.escapeKey.wasPressedThisFrame) Pause();
            if(Mouse.current!=null)
            {
                if(Mouse.current.rightButton.wasPressedThisFrame) Cancel();
                if(!InMenu && Match!=null && !Match.Paused && !Match.Ended)
                {
                    var mouse=Mouse.current.position.ReadValue(); hover=FindPad(mouse);
                    if(Mouse.current.leftButton.wasPressedThisFrame && !CameraControls.IsRotatingPointer && !MapCameraControls.RotationModifier(Keyboard.current) && !(EventSystem.current && EventSystem.current.IsPointerOverGameObject())) ClickPad(hover);
                }
            }
            if(Match!=null)
            {
                // Bound individual simulation steps for low frame rates; simulation and effects share the pause gate.
                if(!Match.Paused && !Match.Ended)
                {
                    float remaining=Mathf.Min(Time.deltaTime,.25f);
                    while(remaining>0) { float dt=Mathf.Min(remaining,1f/60); Match.Step(dt); remaining-=dt; }
                    combatEffects.Tick(Time.deltaTime,Match.Shots);
                }
                SyncViews();
            }
            UpdatePreview(); hud.Refresh();
        }
        int FindPad(Vector2 mouse)
        {
            int best=-1; float distance=42*Screen.height/900f;
            for(int i=0;i<padViews.Length;i++)
            {
                var p=battleCamera.WorldToScreenPoint(padViews[i].position);
                float d=Vector2.Distance(mouse,new Vector2(p.x,p.y)); if(p.z>0 && d<distance) { distance=d; best=i; }
            }
            return best;
        }
        public void ClickPad(int pad)
        {
            if(Match==null || Match.Paused || Match.Ended || pad<0 || pad>=Match.Pads.Length) return;
            if(Match.Towers[pad]!=null) { SelectedPad=pad; Placement=-1; Notice="Emplacement selected."; return; }
            if(Placement<0) { SelectedPad=-1; Notice="Select a defense from the supply bar first."; return; }
            if(!Match.Build(pad,defenses[Placement])) { Notice="Insufficient funds for this defense."; return; }
            Notice=defenses[Placement].displayName+" deployed. Right-click to finish building.";
        }
        void SyncViews()
        {
            var active=new HashSet<int>();
            foreach(var e in Match.Enemies)
            {
                active.Add(e.Id);
                if(!enemyViews.TryGetValue(e.Id,out var view))
                {
                    view=Instantiate(e.Definition.prefab,dynamicRoot); enemyViews.Add(e.Id,view);
                    var bar=GameObject.CreatePrimitive(PrimitiveType.Cube); Destroy(bar.GetComponent<Collider>()); bar.name="Health";
                    bar.transform.SetParent(view.transform,false); bar.transform.localPosition=Vector3.up*1.35f; bar.GetComponent<Renderer>().sharedMaterial=healthMaterial;
                    healthViews[e.Id]=bar.transform;
                }
                view.transform.position=e.Position;
                view.GetComponent<InfantryVisual>()?.Pose(e.Distance,e.Id);
                var direction=Match.Route.Position(e.Distance+.1f,e.LateralOffset)-e.Position;
                if(direction.sqrMagnitude>.00001f) view.transform.rotation=Quaternion.LookRotation(direction);
                healthViews[e.Id].rotation=battleCamera.transform.rotation;
                healthViews[e.Id].localScale=new Vector3(.65f*Mathf.Clamp01(e.Health/e.Definition.health),.055f,.07f);
            }
            RemoveMissing(enemyViews,active,healthViews);
            for(int i=0;i<Match.Towers.Length;i++)
            {
                var t=Match.Towers[i];
                if(t==null) { if(towerViews.TryGetValue(i,out var old)) { Destroy(old); towerViews.Remove(i); } continue; }
                var view=EnsureTower(i);
                var target=Match.Target(t);var visual=view.GetComponent<TowerVisual>();
                if(visual)visual.Aim(target!=null?target.Position-view.transform.position:Vector3.zero,Match.Paused || Match.Ended?0:Time.deltaTime);
                else if(target!=null) { var d=target.Position-view.transform.position; d.y=0; view.transform.rotation=Quaternion.LookRotation(d); }
            }
        }
        void RemoveMissing(Dictionary<int,GameObject> views,HashSet<int> active,Dictionary<int,Transform> extra)
        {
            var remove=new List<int>(); foreach(var pair in views) if(!active.Contains(pair.Key)) { Destroy(pair.Value); remove.Add(pair.Key); }
            foreach(int id in remove) { views.Remove(id); extra?.Remove(id); }
        }
        void UpdatePreview()
        {
            bool allowed=Match!=null && !Match.Paused && !Match.Ended && !InMenu;
            int pad=Placement>=0?hover:SelectedPad;
            var definition=allowed && pad>=0 ? (Placement>=0?defenses[Placement]:Match.Towers[pad]?.Definition):null;
            ring.enabled=definition!=null;
            if(definition!=null)
            {
                bool valid=Placement<0 || (Match.Towers[pad]==null && Match.Funds>=definition.cost);
                ring.startColor=ring.endColor=valid?new Color(.86f,.76f,.39f):new Color(.95f,.28f,.2f);
                for(int j=0;j<97;j++) { float a=j/96f*Mathf.PI*2; var p=Match.Pads[pad]+new Vector3(Mathf.Cos(a)*definition.range,0,Mathf.Sin(a)*definition.range); p.y=BattleRoute.Ground(p.x,p.z)+.18f; ring.SetPosition(j,p); }
            }
            bool show=allowed && Placement>=0 && hover>=0 && Match.Towers[hover]==null;
            if(show && previewType!=Placement)
            {
                if(preview) Destroy(preview); preview=Instantiate(defenses[Placement].prefab,dynamicRoot); previewType=Placement;
                foreach(var renderer in preview.GetComponentsInChildren<Renderer>()) renderer.sharedMaterial=rangeMaterial;
            }
            if(preview) { preview.SetActive(show); if(show) preview.transform.position=Match.Pads[hover]+Vector3.up*.025f; }
        }
        GameObject EnsureTower(int pad)
        {
            if(!towerViews.TryGetValue(pad,out var view))
            {
                view=Instantiate(Match.Towers[pad].Definition.prefab,Match.Pads[pad]-Vector3.up*.015f,Quaternion.identity,dynamicRoot);
                towerViews[pad]=view;
            }
            return view;
        }
        void OnShotFired(ShotState shot)
        {
            var visual=EnsureTower(shot.TowerPad).GetComponent<TowerVisual>();
            visual.FireAt(shot.Destination);
            combatEffects.Fire(shot,visual); audioPlayer.Fire(shot.Definition,visual.MuzzlePosition);
        }
        void OnShotImpacted(ShotState shot)
        {
            combatEffects.Impact(shot); audioPlayer.Impact(shot);
        }
        void OnCue(string cue,Vector3 position)
        {
            // Legacy journal cues remain available to core checks; weapon events carry exact shot context.
            if(cue=="fire" || cue=="impact")return;
            audioPlayer.Play(cue);
            if(cue=="escape")combatEffects.Escape(position);
        }
        void Unsubscribe()
        {
            if(Match==null)return;
            Match.Cue-=OnCue; Match.ShotFired-=OnShotFired; Match.ShotImpacted-=OnShotImpacted;
        }
        void OnDestroy() { Unsubscribe(); }
    }
}
