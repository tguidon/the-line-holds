using System;
using System.Linq;
using UnityEditor;
using UnityEngine;
public static class CheckFoliage
{
    public static string Assets()
    {
        const string root="Assets/TheLineHolds/";
        foreach(var name in new[]{"MachineGun","AntiTank","Mortar","Sandbags"})
        {
            var prefab=AssetDatabase.LoadAssetAtPath<GameObject>(root+"Prefabs/"+name+".prefab");
            var bags=prefab.GetComponentsInChildren<Transform>().Where(t=>t.name.StartsWith("Sandbag")).Where(t=>t!=prefab.transform).ToArray();
            int expected=name=="AntiTank"?32:28;float radius=name=="AntiTank"?.79f:.68f;
            if(bags.Length!=expected)throw new Exception(name+" must have two complete staggered courses; got "+bags.Length);
            var angles=bags.Select(t=>Mathf.Repeat(Mathf.Atan2(t.position.z,t.position.x),Mathf.PI*2)).OrderBy(a=>a).ToArray();
            float gap=0;for(int i=0;i<angles.Length;i++)gap=Mathf.Max(gap,Mathf.Repeat(angles[(i+1)%angles.Length]-angles[i],Mathf.PI*2));
            if(gap>.25f)throw new Exception(name+" sandbag ring has a gap");
            foreach(var bag in bags)
            {
                var radial=new Vector3(bag.position.x,0,bag.position.z);
                if(Mathf.Abs(radial.magnitude-radius)>.02f || Mathf.Abs(Vector3.Dot(radial.normalized,bag.right))>.1f)throw new Exception(name+" sandbag is not tangent to its circular wall");
            }
        }
        foreach(var name in new[]{"Pine","Bush"})
        {
            var prefab=AssetDatabase.LoadAssetAtPath<GameObject>(root+"Prefabs/"+name+".prefab");
            var leaves=prefab.GetComponentsInChildren<MeshFilter>().Single(m=>m.name=="Broadleaf canopy" || m.name=="Shrub leaves");
            var mesh=leaves.sharedMesh;
            if(mesh.uv.Length!=mesh.vertexCount || mesh.tangents.Length!=mesh.vertexCount)throw new Exception(name+" leaf UVs/tangents missing");
            if(leaves.GetComponent<Renderer>().sharedMaterials.Any(m=>!m.mainTexture || m.mainTexture.name!="Broadleaf veins"))throw new Exception(name+" leaf texture missing");
        }
        string result="PASS complete staggered sandbag rings with tangential bags; tree and shrub leaf geometry, UVs, tangents, and vein texture";
        Debug.Log("[Foliage refinement] "+result);return result;
    }
}
