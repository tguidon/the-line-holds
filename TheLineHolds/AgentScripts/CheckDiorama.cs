using System;
using System.Linq;
using UnityEditor;
using UnityEngine;
using TheLineHolds;
public static class CheckDiorama
{
    public static string Towers()
    {
        foreach(string name in new[]{"MachineGun","AntiTank","Mortar"})
        {
            var prefab=AssetDatabase.LoadAssetAtPath<GameObject>("Assets/TheLineHolds/Prefabs/"+name+".prefab");
            var view=UnityEngine.Object.Instantiate(prefab);
            try
            {
                var visual=view.GetComponent<TowerVisual>();
                if(!visual)throw new Exception("Tower visual missing: "+name);
                // Awake runs in Play Mode; validate the actual traverse direction and static sandbags.
                var pivot=view.GetComponentsInChildren<Transform>().First(t=>t.name.StartsWith("Traverse"));
                var barrel=pivot.GetComponentsInChildren<Transform>().First(t=>t.name.StartsWith("Barrel"));
                var bag=view.GetComponentsInChildren<Transform>().First(t=>t.name.StartsWith("Sandbag"));
                var fixedPosition=bag.position;var fixedRotation=bag.rotation;
                foreach(var direction in new[]{Vector3.forward,Vector3.left,Vector3.back})
                {
                    visual.Aim(direction,1f);
                    var forward=Vector3.ProjectOnPlane(barrel.position-pivot.position,Vector3.up);
                    if(Vector3.Angle(forward,direction)>1)throw new Exception("Wrong traverse axis: "+name);
                    if(bag.position!=fixedPosition || bag.rotation!=fixedRotation)throw new Exception("Sandbags rotated: "+name);
                }
            }
            finally{UnityEngine.Object.DestroyImmediate(view);}
        }
        Debug.Log("[Diorama check] PASS three tower traverse axes and stationary sandbags");return "PASS three tower traverse axes and stationary sandbags";
    }
    public static string Units()
    {
        var tank=AssetDatabase.LoadAssetAtPath<GameObject>("Assets/TheLineHolds/Prefabs/LightTank.prefab");
        if(tank.GetComponentsInChildren<Transform>().First(t=>t.name.StartsWith("Muzzle")).position.z<.9f)throw new Exception("Tank faces backward relative to the +Z movement root");
        var prefab=AssetDatabase.LoadAssetAtPath<GameObject>("Assets/TheLineHolds/Prefabs/Infantry.prefab");
        var view=UnityEngine.Object.Instantiate(prefab);
        try
        {
            var visual=view.GetComponent<InfantryVisual>();
            var leg=view.GetComponentsInChildren<Transform>().First(t=>t.name.StartsWith("StrideLeft"));
            visual.Pose(0,0);var first=leg.localRotation;var position=view.transform.position;
            visual.Pose(.15f,0);
            if(Quaternion.Angle(first,leg.localRotation)<10 || view.transform.position!=position)throw new Exception("Stride missing or changed simulation position");
            var frozen=leg.localRotation;visual.Pose(.15f,0);
            if(leg.localRotation!=frozen)throw new Exception("Stride moved at a fixed simulation distance");
        }
        finally{UnityEngine.Object.DestroyImmediate(view);}
        Debug.Log("[Diorama check] PASS infantry stride responds to distance, freezes at fixed distance, and never moves unit root");
        return "PASS infantry stride, fixed-distance freeze, unchanged unit root";
    }
    public static string Map()
    {
        var root=GameObject.Find("Diorama landscape");if(!root)throw new Exception("Diorama missing");
        var mesh=AssetDatabase.LoadAssetAtPath<Mesh>("Assets/TheLineHolds/Art/Diorama/Meadow.asset");
        if(mesh.tangents.Length!=mesh.vertexCount)throw new Exception("Textured terrain tangent basis missing; native Lit shading can corrupt");
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
