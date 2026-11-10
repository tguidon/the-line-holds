using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
namespace TheLineHolds
{
    // Camera movement never changes the route, world geometry, or simulation positions.
    [DefaultExecutionOrder(-50)]
    public sealed class MapCameraControls : MonoBehaviour
    {
        public const float MinimumZoom = 5.5f, MaximumZoom = 18;
        public float OverviewSize { get; private set; }
        Camera view;
        BattleController game;
        Vector3 homeFocus, focus, homeOffset;
        Quaternion homeRotation;
        TMPro.TextMeshPro[] labels;
        Quaternion[] labelRotations;
        float yaw;
        bool dragging, modifierDrag, overview;
        int fittedWidth,fittedHeight;float fittedAspect;
        public bool IsRotatingPointer => dragging;
        public static bool RotationModifier(Keyboard keyboard) => keyboard!=null &&
            (Application.platform==RuntimePlatform.OSXPlayer || Application.platform==RuntimePlatform.OSXEditor
                ? keyboard.leftMetaKey.isPressed || keyboard.rightMetaKey.isPressed
                : keyboard.leftCtrlKey.isPressed || keyboard.rightCtrlKey.isPressed);
        public void Initialize(Camera camera, BattleController controller)
        {
            view=camera; game=controller;
            labels=Object.FindObjectsByType<TMPro.TextMeshPro>();
            labelRotations=new Quaternion[labels.Length];
            for(int i=0;i<labels.Length;i++)labelRotations[i]=labels[i].transform.rotation;
            homeRotation=transform.rotation;
            homeOffset=transform.forward*Vector3.Dot(transform.position,transform.forward);
            ResetView();
        }
        public void ResetView()
        {
            // Project playable ground and landmark headroom into the original camera axes.
            // Fit both dimensions inside the map's usable region, leaving room for the HUD.
            var inverse=Quaternion.Inverse(homeRotation);
            var min=new Vector2(float.MaxValue,float.MaxValue);var max=new Vector2(float.MinValue,float.MinValue);
            void Include(Vector3 point)
            {
                for(int x=-1;x<=1;x+=2)for(int z=-1;z<=1;z+=2)for(int y=0;y<=1;y++)
                {
                    var q=inverse*(point+new Vector3(x*1.4f,y*1.9f,z*1.4f));
                    min=Vector2.Min(min,new Vector2(q.x,q.y));max=Vector2.Max(max,new Vector2(q.x,q.y));
                }
            }
            foreach(var p in BattleRoute.DefaultPoints)Include(p);
            foreach(var p in BattleRoute.DefaultPads)Include(p);
            float scale=Mathf.Sqrt(Screen.width/1600f*Screen.height/900f);
            float bottom=Mathf.Clamp(120*scale/Mathf.Max(1,Screen.height)+.03f,.13f,.25f);
            float top=Mathf.Clamp(86*scale/Mathf.Max(1,Screen.height)+.03f,.11f,.22f);
            float height=1-bottom-top;
            OverviewSize=Mathf.Max(MinimumZoom,Mathf.Max((max.x-min.x)/(2*view.aspect*.88f),(max.y-min.y)/(2*height)));
            var center=(min+max)*.5f;
            homeFocus=homeRotation*new Vector3(center.x,center.y-(bottom-top)*OverviewSize,0);
            focus=homeFocus;yaw=0;dragging=false;overview=true;
            fittedWidth=Screen.width;fittedHeight=Screen.height;fittedAspect=view.aspect;
            view.orthographicSize=OverviewSize;Apply();
        }
        public void Rotate(float degrees) { overview=false; yaw=Mathf.Repeat(yaw+degrees,360); Apply(); }
        public void Zoom(float amount)
        {
            if(Mathf.Approximately(amount,0))return;
            overview=false;view.orthographicSize=Mathf.Clamp(view.orthographicSize+amount,MinimumZoom,Mathf.Max(MaximumZoom,OverviewSize));
        }
        void Pan(Vector2 movement)
        {
            overview=false;
            var right=Vector3.ProjectOnPlane(transform.right,Vector3.up).normalized;
            var forward=Vector3.ProjectOnPlane(transform.up,Vector3.up).normalized;
            focus+=right*movement.x+forward*movement.y;
            focus.x=Mathf.Clamp(focus.x,-11,11); focus.z=Mathf.Clamp(focus.z,-6,6);
            Apply();
        }
        void Apply()
        {
            var rotation=Quaternion.Euler(0,yaw,0);
            transform.SetPositionAndRotation(focus+rotation*homeOffset,rotation*homeRotation);
            for(int i=0;i<labels.Length;i++)if(labels[i])labels[i].transform.rotation=rotation*labelRotations[i];
        }
        void Update()
        {
            if(overview && (Screen.width!=fittedWidth || Screen.height!=fittedHeight || !Mathf.Approximately(view.aspect,fittedAspect)))ResetView();
            if(game==null || game.InMenu || game.Match==null || game.Match.Ended)
            { dragging=false; return; }
            var keyboard=Keyboard.current;
            if(keyboard!=null)
            {
                if(keyboard.rKey.wasPressedThisFrame) ResetView();
                float turn=(keyboard.eKey.isPressed?1:0)-(keyboard.qKey.isPressed?1:0);
                if(turn!=0) Rotate(turn*75*Time.unscaledDeltaTime);
                var movement=new Vector2((keyboard.dKey.isPressed?1:0)-(keyboard.aKey.isPressed?1:0),
                    (keyboard.wKey.isPressed?1:0)-(keyboard.sKey.isPressed?1:0));
                if(movement!=Vector2.zero) Pan(movement.normalized*view.orthographicSize*Time.unscaledDeltaTime);
            }
            var mouse=Mouse.current;
            if(mouse==null)return;
            bool overUI=EventSystem.current && EventSystem.current.IsPointerOverGameObject();
            if(!overUI) Zoom(-mouse.scroll.ReadValue().y*.008f);
            if(mouse.middleButton.wasPressedThisFrame) { dragging=!overUI; modifierDrag=false; }
            if(mouse.leftButton.wasPressedThisFrame && RotationModifier(keyboard)) { dragging=!overUI; modifierDrag=true; }
            if(dragging && (modifierDrag ? !mouse.leftButton.isPressed || !RotationModifier(keyboard) : !mouse.middleButton.isPressed)) dragging=false;
            if(dragging)
            {
                var delta=mouse.delta.ReadValue();
                bool pan=!modifierDrag && keyboard!=null && (keyboard.leftShiftKey.isPressed || keyboard.rightShiftKey.isPressed);
                if(pan) Pan(-delta*(2*view.orthographicSize/Screen.height));
                else Rotate(delta.x*.25f);
            }
        }
    }
}
