using System;
using System.Linq;
using System.Text;
using UnityEditor;
using UnityEngine;
using TheLineHolds;
public static class CheckFeedback
{
    public static string Geometry()
    {
        var mesh=AssetDatabase.LoadAssetAtPath<Mesh>("Assets/TheLineHolds/Art/Materials/RoadMesh.asset");
        var vertices=mesh.vertices;var triangles=mesh.triangles;float clearance=float.MaxValue;Vector3 worst=default;
        for(int i=0;i<triangles.Length;i+=3)
            for(int a=0;a<=10;a++)for(int b=0;b<=10-a;b++)
            {
                var p=(vertices[triangles[i]]*a+vertices[triangles[i+1]]*b+vertices[triangles[i+2]]*(10-a-b))/10;
                float gap=p.y-BattleRoute.Ground(p.x);if(gap<clearance){clearance=gap;worst=p;}
            }
        string result=$"Road minimum clearance={clearance:0.0000}m at {worst}";Debug.Log("[Road regression] "+result);
        if(clearance<.015f)throw new Exception("Road intersects terrain: "+result);
        return result;
    }
    public static string Hud()
    {
        var game=UnityEngine.Object.FindAnyObjectByType<BattleController>();
        if(!Application.isPlaying || !game)throw new Exception("Enter Play Mode first");
        var scaler=game.hud.GetComponent<UnityEngine.UI.CanvasScaler>();var canvas=game.hud.GetComponent<Canvas>();
        bool enabled=scaler.enabled;float scale=canvas.scaleFactor;var report=new StringBuilder();
        Rect Bounds(RectTransform t){var p=new Vector3[4];t.GetWorldCorners(p);return Rect.MinMaxRect(p[0].x,p[0].y,p[2].x,p[2].y);}
        try
        {
            game.NewMatch();scaler.enabled=false;
            foreach(int width in new[]{1100,1200,1386,1600,2000})
            {
                canvas.scaleFactor=Screen.width/(float)width;Canvas.ForceUpdateCanvases();game.hud.Refresh();Canvas.ForceUpdateCanvases();
                var labels=game.hud.GetComponentsInChildren<TMPro.TextMeshProUGUI>();
                var status=labels.Single(t=>t.text.StartsWith("LIVES"));var briefing=labels.Single(t=>t.text.StartsWith("ORDERS REQUIRED"));
                var notice=labels.Single(t=>t.text.StartsWith("Deploy your defenses"));var hint=labels.Single(t=>t.text.StartsWith("Rotate:"));
                var buttons=game.hud.GetComponentsInChildren<UnityEngine.UI.Button>();
                var pause=buttons.Single(b=>b.name.StartsWith("II"));var start=buttons.Single(b=>b.name.StartsWith("START WAVE"));
                bool overlap=Bounds(status.rectTransform).Overlaps(Bounds((RectTransform)pause.transform)) ||
                    Bounds(briefing.rectTransform).Overlaps(Bounds((RectTransform)start.transform)) ||
                    Bounds(briefing.rectTransform).Overlaps(Bounds(notice.rectTransform));
                var row=buttons.Where(b=>b.name.StartsWith("Shop") || b==start).ToArray();
                for(int a=0;a<row.Length;a++)for(int b=a+1;b<row.Length;b++)
                    overlap|=Bounds((RectTransform)row[a].transform).Overlaps(Bounds((RectTransform)row[b].transform));
                foreach(var b in buttons.Where(b=>b.transform.parent.name=="Map controls"))overlap|=Bounds(hint.rectTransform).Overlaps(Bounds((RectTransform)b.transform));
                status.ForceMeshUpdate();briefing.ForceMeshUpdate();
                if(overlap || status.isTextTruncated || briefing.isTextTruncated)throw new Exception("HUD overlap/truncation at logical width="+width);
                report.AppendLine($"PASS HUD logical width {width}: reserved regions, readable counters/briefing, separate controls");
            }
            Debug.Log("[HUD regression]\n"+report);return report.ToString();
        }
        finally {canvas.scaleFactor=scale;scaler.enabled=enabled;game.MainMenu();Canvas.ForceUpdateCanvases();game.hud.Refresh();}
    }
}
