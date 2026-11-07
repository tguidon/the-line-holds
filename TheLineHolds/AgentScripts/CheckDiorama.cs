using System;
using System.Linq;
using UnityEditor;
using UnityEngine;
using TheLineHolds;
public static class CheckDiorama
{
    public static string Map()
    {
        var root=GameObject.Find("Diorama landscape");if(!root)throw new Exception("Diorama missing");
        var mesh=AssetDatabase.LoadAssetAtPath<Mesh>("Assets/TheLineHolds/Art/Diorama/Meadow.asset");
        if(mesh.vertexCount<10000 || mesh.uv.Length!=mesh.vertexCount)throw new Exception("Continuous textured terrain missing");
        if(mesh.normals.Any(n=>n.y<.8f))throw new Exception("Terrain has a cliff or inverted face");
        foreach(var p in BattleRoute.DefaultPads)if(Mathf.Abs(p.y-BattleRoute.Ground(p.x,p.z)-.035f)>.001f)throw new Exception("Pad detached from terrain");
        if(root.GetComponentsInChildren<Renderer>().Any(r=>r.sharedMaterials.Any(m=>!m || !m.shader.isSupported)))throw new Exception("Missing/unsupported material");
        if(PlayerSettings.defaultScreenWidth!=1920 || PlayerSettings.defaultScreenHeight!=1080)throw new Exception("Default window too small");
        var route=new BattleRoute(BattleRoute.DefaultPoints);float maxTurn=0;
        for(int i=1;i<route.Points.Length-1;i++)maxTurn=Mathf.Max(maxTurn,Vector3.Angle(route.Points[i]-route.Points[i-1],route.Points[i+1]-route.Points[i]));
        if(maxTurn>25)throw new Exception("Abrupt road corner: "+maxTurn);
        string result=$"PASS continuous meadow {mesh.vertexCount} vertices; ten grounded pads; supported materials; max route turn {maxTurn:0.0}°; 1920x1080 default";Debug.Log("[Diorama check] "+result);return result;
    }
}
