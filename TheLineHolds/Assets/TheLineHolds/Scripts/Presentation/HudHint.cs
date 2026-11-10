using UnityEngine;
using UnityEngine.EventSystems;
namespace TheLineHolds
{
    public sealed class HudHint : MonoBehaviour,IPointerEnterHandler,IPointerExitHandler,ISelectHandler,IDeselectHandler
    {
        public GameHud owner;public string hint;
        public void OnPointerEnter(PointerEventData e)=>owner.ShowHint(hint);
        public void OnPointerExit(PointerEventData e)=>owner.ShowHint(null);
        public void OnSelect(BaseEventData e)=>owner.ShowHint(hint);
        public void OnDeselect(BaseEventData e)=>owner.ShowHint(null);
        void OnDisable(){if(owner)owner.ShowHint(null);}
    }
}
