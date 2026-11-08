using System;
using System.Linq;
using UnityEditor;
using UnityEngine;
public static class CheckHouses
{
    public static string All()
    {
        var prefab=AssetDatabase.LoadAssetAtPath<GameObject>("Assets/TheLineHolds/Prefabs/Farmhouse.prefab");
        var house=UnityEngine.Object.Instantiate(prefab,new Vector3(30,10,30),Quaternion.identity);
        try
        {
            var renderers=house.GetComponentsInChildren<MeshRenderer>();
            if(renderers.Length>12 || renderers.Any(r=>r.sharedMaterials.Any(m=>!m || !m.shader.isSupported)))throw new Exception("House materials or batching invalid");
            foreach(var filter in house.GetComponentsInChildren<MeshFilter>())filter.gameObject.AddComponent<MeshCollider>().sharedMesh=filter.sharedMesh;
            Physics.SyncTransforms();
            float Height(float x)
            {
                if(!Physics.Raycast(new Vector3(30+x,15,30.55f),Vector3.down,out var hit,8))throw new Exception("Roof has a hole at "+x);
                return hit.point.y-10;
            }
            float ridge=Height(0),left=Height(-.4f),right=Height(.4f),leftEave=Height(-1.05f),rightEave=Height(1.05f);
            if(!(ridge>left+.15f && ridge>right+.15f && left>leftEave+.3f && right>rightEave+.3f))throw new Exception("Roof does not descend from a shared ridge");
            if(Mathf.Abs(left-right)>.025f || Mathf.Abs(leftEave-rightEave)>.025f)throw new Exception("Roof slopes are asymmetric");
            string result=$"PASS closed pitched roof: ridge {ridge:0.00}m, mid slopes {left:0.00}/{right:0.00}m, eaves {leftEave:0.00}/{rightEave:0.00}m; {renderers.Length} material-batched renderers";
            Debug.Log("[House review] "+result);return result;
        }
        finally{UnityEngine.Object.DestroyImmediate(house);}
    }
}
