using System;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using TheLineHolds;
public static class CheckCampaignUI
{
 static void Check(bool b,string m){if(!b)throw new Exception(m);}
 static Rect Bounds(RectTransform r){var c=new Vector3[4];r.GetWorldCorners(c);return Rect.MinMaxRect(c[0].x,c[0].y,c[2].x,c[2].y);}
 public static string All()
 {
  var g=UnityEngine.Object.FindAnyObjectByType<BattleController>();g.NewMatch();g.Choose(0);g.ClickPad(0);g.Cancel();g.ClickPad(0);g.hud.Refresh();Canvas.ForceUpdateCanvases();
  var button=g.hud.GetComponentsInChildren<Button>().Single(b=>b.GetComponentInChildren<TextMeshProUGUI>().text.StartsWith("VETERAN NEST"));
  Check(button.interactable,"Affordable upgrade disabled");button.onClick.Invoke();g.hud.Refresh();Canvas.ForceUpdateCanvases();
  Check(g.Match.Funds==135 && g.Match.Towers[0].Definition.tier==2,"Upgrade button not connected");
  var choices=g.hud.GetComponentsInChildren<Button>().Where(b=>b.name=="UPGRADE").ToArray();Check(choices.Length==2 && choices.All(b=>!b.interactable),"Branch choice/affordability wrong");
  Check(!Bounds((RectTransform)choices[0].transform).Overlaps(Bounds((RectTransform)choices[1].transform)),"Upgrade choices overlap");
  var labels=g.hud.GetComponentsInChildren<TextMeshProUGUI>();var status=labels.Single(t=>t.text.StartsWith("LIVES"));
  Check(status.text.Contains("/ 10"),"Wave header still five");
  var speed=g.hud.GetComponentsInChildren<Button>().Single(b=>b.GetComponentInChildren<TextMeshProUGUI>().text=="1x");speed.onClick.Invoke();Check(g.GameSpeed==2,"Pace toggle not connected");
  Check(!Bounds(status.rectTransform).Overlaps(Bounds((RectTransform)speed.transform)),"Speed/header overlap");
  Check(labels.Any(t=>t.text.StartsWith("NEXT 1:") && t.text.Contains("Riflemen")),"Announced next-wave composition missing");
  g.Pause();var funds=g.Match.Funds;g.UpgradeSelected(0);Check(g.Match.Funds==funds,"Paused upgrade spent funds");g.MainMenu();
  string result="PASS connected veteran/branch controls, prices and affordability, ten-wave header/intel, speed toggle, pause and non-overlapping inspection";Debug.Log("[Campaign UI] "+result);return result;
 }
}
