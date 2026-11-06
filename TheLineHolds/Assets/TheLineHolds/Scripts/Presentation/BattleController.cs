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
        public string Notice { get; private set; } = "Choose a defense, then click a numbered emplacement.";
        public bool InMenu { get; private set; } = true;
        readonly Dictionary<int, GameObject> enemyViews = new Dictionary<int, GameObject>();
        readonly Dictionary<int, Transform> healthViews = new Dictionary<int, Transform>();
        readonly Dictionary<int, GameObject> shotViews = new Dictionary<int, GameObject>();
        readonly Dictionary<int, GameObject> towerViews = new Dictionary<int, GameObject>();
        readonly List<Effect> effects = new List<Effect>();
        sealed class Effect { public GameObject View; public float Life; public float Total; public bool Smoke; }
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
            ClearViews(); audioPlayer.ResetAudio(); InMenu=false; Placement=SelectedPad=-1;
            Match=new MatchState(waves,new BattleRoute(BattleRoute.DefaultPoints),BattleRoute.DefaultPads);
            Match.Cue += OnCue;
            CameraControls.ResetView(); Notice="Deploy your defenses. Wave 1 waits for your order.";
        }
        public void MainMenu()
        {
            if(Match!=null) { Match.Cue-=OnCue; }
            Match=null; InMenu=true; Placement=SelectedPad=-1; ClearViews(); audioPlayer.ResetAudio();
        }
        void ClearViews()
        {
            foreach(var v in enemyViews.Values) Destroy(v); enemyViews.Clear(); healthViews.Clear();
            foreach(var v in towerViews.Values) Destroy(v); towerViews.Clear();
            foreach(var v in shotViews.Values) Destroy(v); shotViews.Clear();
            foreach(var e in effects) Destroy(e.View); effects.Clear();
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
                    TickEffects(Time.deltaTime);
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
            if(Match.Towers[pad]!=null) { SelectedPad=pad; Placement=-1; Notice="Emplacement "+(pad+1)+" selected."; return; }
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
                if(!towerViews.TryGetValue(i,out var view)) { view=Instantiate(t.Definition.prefab,Match.Pads[i]+Vector3.up*.12f,Quaternion.identity,dynamicRoot); towerViews[i]=view; }
                var target=Match.Target(t); if(target!=null) { var d=target.Position-view.transform.position; d.y=0; view.transform.rotation=Quaternion.LookRotation(d); }
            }
            active.Clear();
            foreach(var s in Match.Shots)
            {
                active.Add(s.Id);
                if(!shotViews.TryGetValue(s.Id,out var v))
                {
                    v=GameObject.CreatePrimitive(PrimitiveType.Sphere); Destroy(v.GetComponent<Collider>()); v.name="Tracer"; v.transform.SetParent(dynamicRoot);
                    v.GetComponent<Renderer>().sharedMaterial=tracerMaterial; v.transform.localScale=Vector3.one*(s.Definition.splashRadius>0?.16f:.07f); shotViews[s.Id]=v;
                }
                float t=Mathf.Clamp01(s.Age/s.Definition.flightTime);
                v.transform.position=Vector3.Lerp(s.Origin,s.Destination,t)+Vector3.up*(s.Definition.splashRadius>0?4*t*(1-t)*3:0);
            }
            RemoveMissing(shotViews,active,null);
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
                for(int j=0;j<97;j++) { float a=j/96f*Mathf.PI*2; var p=Match.Pads[pad]+new Vector3(Mathf.Cos(a)*definition.range,0,Mathf.Sin(a)*definition.range); p.y=BattleRoute.Ground(p.x)+.18f; ring.SetPosition(j,p); }
            }
            bool show=allowed && Placement>=0 && hover>=0 && Match.Towers[hover]==null;
            if(show && previewType!=Placement)
            {
                if(preview) Destroy(preview); preview=Instantiate(defenses[Placement].prefab,dynamicRoot); previewType=Placement;
                foreach(var renderer in preview.GetComponentsInChildren<Renderer>()) renderer.sharedMaterial=rangeMaterial;
            }
            if(preview) { preview.SetActive(show); if(show) preview.transform.position=Match.Pads[hover]+Vector3.up*.16f; }
        }
        void OnCue(string cue,Vector3 position)
        {
            audioPlayer.Play(cue);
            if(cue!="fire" && cue!="impact" && cue!="escape") return;
            var v=GameObject.CreatePrimitive(PrimitiveType.Sphere); Destroy(v.GetComponent<Collider>()); v.transform.SetParent(dynamicRoot); v.transform.position=position;
            v.GetComponent<Renderer>().sharedMaterial=cue=="escape"?damageMaterial:tracerMaterial;
            float life=cue=="fire"?.08f:.28f; v.transform.localScale=Vector3.one*(cue=="fire"?.2f:.35f);
            effects.Add(new Effect{View=v,Life=life,Total=life});
            if(cue=="impact")
            {
                var smoke=GameObject.CreatePrimitive(PrimitiveType.Sphere); Destroy(smoke.GetComponent<Collider>()); smoke.name="Impact smoke"; smoke.transform.SetParent(dynamicRoot); smoke.transform.position=position; smoke.transform.localScale=Vector3.one*.16f; smoke.GetComponent<Renderer>().sharedMaterial=smokeMaterial;
                effects.Add(new Effect{View=smoke,Life=.65f,Total=.65f,Smoke=true});
            }
        }
        void TickEffects(float dt)
        {
            for(int i=effects.Count-1;i>=0;i--) { var e=effects[i]; e.Life-=dt; if(e.Life<=0) { Destroy(e.View); effects.RemoveAt(i); } else { e.View.transform.localScale*=1+dt*3; if(e.Smoke)e.View.transform.position+=Vector3.up*dt*.6f; } }
        }
        void OnDestroy() { if(Match!=null) { Match.Cue-=OnCue; } }
    }
}
