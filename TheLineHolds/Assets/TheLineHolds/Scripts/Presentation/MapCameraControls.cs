using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
namespace TheLineHolds
{
    // Camera movement never changes the route, world geometry, or simulation positions.
    [DefaultExecutionOrder(-50)]
    public sealed class MapCameraControls : MonoBehaviour
    {
        public const float MinimumZoom = 5.5f, MaximumZoom = 18, OverviewZoom = 13.5f;
        Camera view;
        BattleController game;
        Vector3 homeFocus, focus, homeOffset;
        Quaternion homeRotation;
        TMPro.TextMeshPro[] labels;
        Quaternion[] labelRotations;
        float yaw;
        bool dragging, modifierDrag;
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
            homeFocus=-transform.up*.9f;
            homeOffset=transform.position-homeFocus;
            ResetView();
        }
        public void ResetView()
        {
            focus=homeFocus; yaw=0; dragging=false;
            view.orthographicSize=OverviewZoom; Apply();
        }
        public void Rotate(float degrees) { yaw=Mathf.Repeat(yaw+degrees,360); Apply(); }
        public void Zoom(float amount)
        {
            view.orthographicSize=Mathf.Clamp(view.orthographicSize+amount,MinimumZoom,MaximumZoom);
        }
        void Pan(Vector2 movement)
        {
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
            if(game==null || game.InMenu || game.Match==null || game.Match.Paused || game.Match.Ended)
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
