using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEngine;
using UnityEngine.Rendering;
namespace TheLineHolds.Editor
{
    // The battlefield remains unchanged. Four seamless aprons fill every allowed camera view.
    public static class CountrysideSurroundings
    {
        const float Extent=90;
        public static void Build(Transform parent)
        {
            var root=new GameObject("Surrounding countryside").transform;root.SetParent(parent,false);
            var grass=DioramaBuilder.Material("Surrounding pasture","FFFFFF",.05f);grass.mainTexture=Paint();EditorUtility.SetDirty(grass);
            void Patch(string name,float x0,float x1,float z0,float z1)
            {
                int nx=Mathf.CeilToInt((x1-x0)*2),nz=Mathf.CeilToInt((z1-z0)*2);var v=new Vector3[(nx+1)*(nz+1)];var uv=new Vector2[v.Length];var t=new List<int>();
                for(int z=0;z<=nz;z++)for(int x=0;x<=nx;x++)
                {int i=z*(nx+1)+x;float px=Mathf.Lerp(x0,x1,x/(float)nx),pz=Mathf.Lerp(z0,z1,z/(float)nz);v[i]=BattleRoute.OnGround(px,pz)-Vector3.up*.035f;uv[i]=new Vector2((px+Extent)/(Extent*2),(pz+Extent)/(Extent*2));if(x<nx && z<nz)t.AddRange(new[]{i,i+nx+1,i+1,i+1,i+nx+1,i+nx+2});}
                var mesh=new Mesh{name=name,indexFormat=IndexFormat.UInt32};mesh.vertices=v;mesh.uv=uv;mesh.SetTriangles(t,0);mesh.RecalculateNormals();mesh.RecalculateTangents();mesh.RecalculateBounds();
                var go=new GameObject(name,typeof(MeshFilter),typeof(MeshRenderer));go.transform.SetParent(root,false);go.GetComponent<MeshFilter>().sharedMesh=DioramaBuilder.SaveMesh(mesh,name);go.GetComponent<MeshRenderer>().sharedMaterial=grass;
            }
            Patch("North pasture",-Extent,Extent,7,Extent);Patch("South pasture",-Extent,Extent,-Extent,-7);Patch("West pasture",-Extent,-12,-7,7);Patch("East pasture",12,Extent,-7,7);
            var props=new GameObject("Farm scenery").transform;props.SetParent(root,false);var rng=new System.Random(7091944);
            void Model(string key,float x,float z,float size,float yaw)
            {
                var prefab=AssetDatabase.LoadAssetAtPath<GameObject>("Assets/TheLineHolds/Prefabs/"+key+".prefab");if(!prefab)throw new Exception("Missing "+key);
                var go=(GameObject)PrefabUtility.InstantiatePrefab(prefab);go.transform.SetParent(props);go.transform.position=BattleRoute.OnGround(x,z);go.transform.rotation=Quaternion.Euler(0,yaw,0);go.transform.localScale=Vector3.one*size;
            }
            // Small hamlets and farmyards frame the road without covering its ten sites.
            foreach(var c in new[]{new Vector2(-17,11),new Vector2(17,12),new Vector2(-18,-13),new Vector2(20,-13)})
            {
                Model("Barn",c.x,c.y,1,15);Model("Farmhouse",c.x+4,c.y+1,.9f,-20);Model("Farmhouse",c.x+1,c.y+5,.75f,30);
                for(int i=0;i<5;i++)Model("FieldFence",c.x-3+i*1.3f,c.y-3,1,0);
                Model("Crate",c.x+2,c.y-1,1,30);Model("Crate",c.x+2.5f,c.y-1.5f,.8f,0);
            }
            for(int i=0;i<110;i++)
            {
                float x=(float)(rng.NextDouble()-.5)*85,z=(float)(rng.NextDouble()-.5)*65;
                if(Mathf.Abs(x)<13 && Mathf.Abs(z)<9)continue;
                if(Mathf.Abs(x)<4 || Mathf.Abs(z)<1.5f)continue; // Loose farm lanes, rather than a tree wall.
                Model("Pine",x,z,.7f+(float)rng.NextDouble()*.6f,(float)rng.NextDouble()*360);
                if(i%3==0)Model("Bush",x+1,z,.9f,(float)rng.NextDouble()*360);
            }
            // Batch by material and spatial sector; distant leaves retain useful culling.
            var groups=new Dictionary<string,(Material material,List<CombineInstance> parts)>();
            foreach(var f in props.GetComponentsInChildren<MeshFilter>())
            {
                var r=f.GetComponent<MeshRenderer>();if(!r)continue;
                for(int sub=0;sub<f.sharedMesh.subMeshCount;sub++)
                {
                    var mat=r.sharedMaterials[sub];string key=$"Scenery {Mathf.FloorToInt(f.transform.position.x/24)} {Mathf.FloorToInt(f.transform.position.z/24)} {mat.name}";
                    if(!groups.TryGetValue(key,out var group))group=(mat,new List<CombineInstance>());
                    group.parts.Add(new CombineInstance{mesh=f.sharedMesh,subMeshIndex=sub,transform=root.worldToLocalMatrix*f.transform.localToWorldMatrix});groups[key]=group;
                }
            }
            foreach(var pair in groups)
            {
                var mesh=new Mesh{name=pair.Key,indexFormat=IndexFormat.UInt32};mesh.CombineMeshes(pair.Value.parts.ToArray());var go=new GameObject(pair.Key,typeof(MeshFilter),typeof(MeshRenderer));go.transform.SetParent(root,false);go.GetComponent<MeshFilter>().sharedMesh=DioramaBuilder.SaveMesh(mesh,pair.Key);go.GetComponent<MeshRenderer>().sharedMaterial=pair.Value.material;
            }
            UnityEngine.Object.DestroyImmediate(props.gameObject);
        }
        static Texture2D Paint()
        {
            const int n=2048;var pixels=new Color32[n*n];Color moss=DioramaSurface.Hex("607044"),grass=DioramaSurface.Hex("85905B"),straw=DioramaSurface.Hex("A4A273"),road=DioramaSurface.Hex("AF9D76");
            var fields=new[]{new Vector2(-20,16),new Vector2(20,-20),new Vector2(24,20)};
            for(int z=0;z<n;z++)for(int x=0;x<n;x++)
            {
                float px=x*180f/(n-1)-90,pz=z*180f/(n-1)-90;float noise=Mathf.PerlinNoise(px*.28f+43,pz*.28f+17),fine=Mathf.PerlinNoise(px*8+17,pz*8+9);
                var c=Color.Lerp(moss,grass,Mathf.SmoothStep(.1f,.9f,noise));c=Color.Lerp(c,straw,Mathf.SmoothStep(.54f,.83f,Mathf.PerlinNoise(px*.8f+5,pz*.8f+32))*.35f);
                // Continue entry/exit into quiet farm lanes. No new navigable route is introduced.
                float dz=Mathf.Min(Mathf.Abs(pz+5),Mathf.Abs(pz-5));float lane=(Mathf.Abs(px)>11.8f?1:0)*(1-Mathf.SmoothStep(.6f,1.1f,dz));c=Color.Lerp(c,road,lane);
                float field=0;foreach(var center in fields){float dx=Mathf.Abs(px-center.x),fieldZ=Mathf.Abs(pz-center.y);field=Mathf.Max(field,(1-Mathf.SmoothStep(4,5,dx))*(1-Mathf.SmoothStep(2.5f,3.5f,fieldZ)));}c=Color.Lerp(c,straw,field*.18f);c*=.965f+fine*.07f;pixels[z*n+x]=c;
            }
            var texture=new Texture2D(n,n,TextureFormat.RGB24,true);texture.SetPixels32(pixels);texture.Apply();string path=DioramaSurface.Root+"/SurroundingPasture.png";File.WriteAllBytes(path,texture.EncodeToPNG());UnityEngine.Object.DestroyImmediate(texture);AssetDatabase.ImportAsset(path);
            var importer=(TextureImporter)AssetImporter.GetAtPath(path);importer.maxTextureSize=2048;importer.anisoLevel=8;importer.textureCompression=TextureImporterCompression.CompressedHQ;importer.SaveAndReimport();return AssetDatabase.LoadAssetAtPath<Texture2D>(path);
        }
    }
}
