using System;
using System.Linq;
using UnityEditor;
using UnityEngine;
using TheLineHolds;
public static class CheckMapComposition
{
    public static string All()
    {
        var game=UnityEngine.Object.FindAnyObjectByType<BattleController>();var pads=BattleRoute.DefaultPads;
        if(game.padViews.Length!=10 || game.padViews.Where((p,i)=>Vector3.Distance(p.position,pads[i])>.001f).Any())throw new Exception("Gameplay anchors moved");
        if(game.padViews.Any(p=>p.GetComponent<Renderer>()))throw new Exception("Pedestal renderer still present");
        if(UnityEngine.Object.FindObjectsByType<TMPro.TextMeshPro>().Any(t=>t.name=="Pad number" || t.text.Contains("ENEMY ENTRY") || t.text.Contains("DEFENDED EXIT")))throw new Exception("Legacy floating labels remain");
        var earth=AssetDatabase.LoadAssetAtPath<Mesh>("Assets/TheLineHolds/Art/Diorama/Earth.asset");
        var texture=AssetDatabase.LoadAssetAtPath<Texture2D>("Assets/TheLineHolds/Art/Diorama/EarthLayers.png");
        if(earth.subMeshCount!=3 || earth.tangents.Length!=earth.vertexCount || texture.width<512)throw new Exception("Detailed stratified cut missing");
        var route=new BattleRoute(BattleRoute.DefaultPoints);
        if(Vector3.Distance(GameObject.Find("Enemy entry gateway").transform.position,route.Points[0])>.001f || Vector3.Distance(GameObject.Find("Defended exit checkpoint").transform.position,route.Points.Last())>.001f)throw new Exception("Landmarks do not match the gameplay endpoints");
        foreach(var name in new[]{"FieldFence","Duckboard","FieldStone"})
        {
            var prefab=AssetDatabase.LoadAssetAtPath<GameObject>("Assets/TheLineHolds/Prefabs/"+name+".prefab");
            if(prefab.transform.localPosition!=Vector3.zero || Quaternion.Angle(prefab.transform.localRotation,Quaternion.identity)>.01f || prefab.transform.localScale!=Vector3.one)throw new Exception(name+" placement root must preserve FBX conversion under a child");
        }
        var details=GameObject.Find("Authored battlefield details");var colliders=new System.Collections.Generic.List<MeshCollider>();
        try
        {
            foreach(var filter in details.GetComponentsInChildren<MeshFilter>())
            {var collider=filter.gameObject.AddComponent<MeshCollider>();collider.sharedMesh=filter.sharedMesh;colliders.Add(collider);}
            Physics.SyncTransforms();
            for(float d=0;d<route.Length;d+=.16f)foreach(float lane in new[]{-.62f,0,.62f})foreach(float height in new[]{.18f,.55f,.95f})
            {
                var p=route.Position(d,lane)+Vector3.up*height;
                if(Physics.OverlapSphere(p,.04f).Any(c=>c is MeshCollider meshCollider && colliders.Contains(meshCollider)))throw new Exception($"Decoration intersects vehicle corridor at distance {d:0.0}, lane {lane}, height {height}");
            }
        }
        finally{foreach(var collider in colliders)UnityEngine.Object.DestroyImmediate(collider);}
        string result="PASS ten unchanged ground-site anchors; no pedestals/floating labels; three sharp soil strata; aligned landmarks; full vehicle corridor clear";
        Debug.Log("[Map composition] "+result);return result;
    }
}
