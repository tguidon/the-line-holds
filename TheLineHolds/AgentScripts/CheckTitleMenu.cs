// Development player. Start on the title screen and verify real UI raycasts at three window sizes.
using System;
using System.Linq;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;
using Unity.Pipeline.Scripting;
using TheLineHolds;
TitleMenu Menu()=>UnityEngine.Object.FindAnyObjectByType<TitleMenu>();
BattleController Battle()=>UnityEngine.Object.FindAnyObjectByType<BattleController>();
void Check(bool condition,string reason){if(!condition)throw new Exception(reason);}
void Click(Button button)
{
    Canvas.ForceUpdateCanvases();
    var rect=(RectTransform)button.transform;
    var pointer=new PointerEventData(EventSystem.current){position=RectTransformUtility.WorldToScreenPoint(null,rect.TransformPoint(rect.rect.center)),button=PointerEventData.InputButton.Left};
    var hits=new List<RaycastResult>();EventSystem.current.RaycastAll(pointer,hits);
    Check(hits.Count>0 && hits[0].gameObject.GetComponentInParent<Button>()==button,"Blocked "+button.name+" by "+string.Join(", ",hits.Select(h=>h.gameObject.name)));
    ExecuteEvents.Execute(button.gameObject,pointer,ExecuteEvents.pointerClickHandler);
}
var original=new Vector2Int(Screen.width,Screen.height);
try
{
    Check(Menu() && !Battle() && SceneManager.GetActiveScene().buildIndex==0,"Initial scene must be the dedicated menu");
    foreach(var size in new[]{new Vector2Int(1600,900),new Vector2Int(1024,768),new Vector2Int(1920,800)})
    {
        Screen.SetResolution(size.x,size.y,FullScreenMode.Windowed);
        await Wait.Until(()=>Screen.width==size.x && Screen.height==size.y,timeoutMs:10000);
        await Task.Delay(100);
        foreach(var b in Menu().GetComponentsInChildren<Button>())
        {
            var corners=new Vector3[4];((RectTransform)b.transform).GetWorldCorners(corners);
            Check(corners.All(p=>p.x>=0 && p.x<=Screen.width && p.y>=0 && p.y<=Screen.height),"Clipped "+b.name);
        }
        var previews=Menu().GetComponentsInChildren<RawImage>().Where(i=>i.name.EndsWith(" preview")).ToArray();
        Check(previews.Length==2 && previews.All(i=>i.texture) && previews[0].texture!=previews[1].texture,"Missing distinct photographs");
    }
    foreach(bool winter in new[]{false,true})
    {
        Click(winter?Menu().winterForest:Menu().countryside);
        await Wait.Until(()=>Battle() && Battle().Match!=null,timeoutMs:20000);
        await Task.Delay(150);
        var g=Battle();
        Check(!Menu() && g.winterForest==winter && g.Match.Funds==300 && g.Match.Lives==20 && g.Match.Wave==0,"Wrong deployment");
        var options=g.hud.GetComponentsInChildren<Button>().Single(b=>b.name=="Options");
        Click(options);await Task.Delay(120);
        Click(g.hud.GetComponentsInChildren<Button>().Single(b=>b.name=="Main menu"));
        await Wait.Until(()=>Menu() && !Battle() && EventSystem.current && EventSystem.current.currentSelectedGameObject==Menu().countryside.gameObject,timeoutMs:10000);
        await Task.Delay(150);
    }
    return "Passed standalone title at 1600x900, 1024x768 and 1920x800. Both photograph buttons deployed fresh matches and both Options menus unloaded them.";
}
finally{Screen.SetResolution(original.x,original.y,FullScreenMode.Windowed);}
