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
            foreach(var building in CountrysideLayout.Buildings.Concat(BattlefieldFarm.Buildings))
                Model(building.Key,building.Position.x,building.Position.y,building.Scale,building.Yaw);
            // Farm boundaries have deliberate gaps at access lanes; hedges follow the same clearances.
            void Hedge(float x0,float z0,float x1,float z1)
            {
                var a=new Vector2(x0,z0);var b=new Vector2(x1,z1);int count=Mathf.CeilToInt(Vector2.Distance(a,b)/.68f);
                for(int i=0;i<=count;i++){var p=Vector2.Lerp(a,b,i/(float)count);if(CountrysideLayout.RoadDistance(p)<1.8f)continue;Model("Bush",p.x,p.y,.7f+(float)rng.NextDouble()*.2f,i*53);}
            }
            Hedge(-31,14,-22,14);Hedge(-30,1,-22,1);Hedge(22,4,28,4);Hedge(28,4,28,12);Hedge(-22,-23,-13,-23);Hedge(12,-27,23,-27);
            void Plot(float cx,float cz,int nx,int nz)
            {
                for(int z=0;z<nz;z++)for(int x=0;x<nx;x++)
                {var p=new Vector2(cx+(x-(nx-1)*.5f)*.55f,cz+(z-(nz-1)*.5f)*.7f);if(CountrysideLayout.Reserved(p))continue;Model("Bush",p.x,p.y,.19f+(float)rng.NextDouble()*.06f,x*71+z*29);}
                for(int i=0;i<4;i++){var p=new Vector2(cx-1.95f+i*1.3f,cz-nz*.4f);if(CountrysideLayout.RoadDistance(p)>1.6f)Model("FieldFence",p.x,p.y,1,0);}
            }
            Plot(-27,3,8,5);Plot(24,8,8,6);Plot(-20,-21,9,4);
            // Small planted orchard, distinct from the looser tree belts and the kitchen gardens.
            for(int row=0;row<2;row++)for(int i=0;i<5;i++)
            {float x=10+i*2.7f+(float)rng.NextDouble()*.35f,z=-23-row*2.8f;var p=new Vector2(x,z);if(!CountrysideLayout.Reserved(p))Model("Pine",x,z,.62f+(float)rng.NextDouble()*.15f,i*37);}
            for(int i=0;i<145;i++)
            {
                float x=(float)(rng.NextDouble()-.5)*88,z=(float)(rng.NextDouble()-.5)*70;var p=new Vector2(x,z);
                if(CountrysideLayout.InCore(p,2) || CountrysideLayout.Reserved(p) || CountrysideLayout.RoadDistance(p)<2.2f)continue;
                Model("Pine",x,z,.7f+(float)rng.NextDouble()*.6f,(float)rng.NextDouble()*360);
                var shrub=p+Vector2.right*1.2f;if(i%3==0 && !CountrysideLayout.Reserved(shrub))Model("Bush",shrub.x,shrub.y,.9f,(float)rng.NextDouble()*360);
            }
            foreach(var building in CountrysideLayout.Buildings.Where(b=>b.Key=="ToolShed" || b.Key=="Barn"))
            {
                var p=building.Position+new Vector2(building.Radius+.7f,.5f);if(CountrysideLayout.RoadDistance(p)>1.3f)Model("Crate",p.x,p.y,.65f,17);
            }
            foreach(Transform prop in props.Cast<Transform>().ToArray())
            {
                if(prop.name!="Pine" && prop.name!="Bush")continue;
                var p=prop.position;
                if(BattlefieldFarm.Occupied(new Vector2(p.x,p.z),prop.name=="Pine"?1.2f:.45f))
                    UnityEngine.Object.DestroyImmediate(prop.gameObject);
            }
            BattlefieldFarm.Plant(root);
            PastureDetail.Build(root);
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
            var used=new HashSet<string>();
            foreach(var pair in groups)
            {
                used.Add(pair.Key);var mesh=new Mesh{name=pair.Key,indexFormat=IndexFormat.UInt32};mesh.CombineMeshes(pair.Value.parts.ToArray());var go=new GameObject(pair.Key,typeof(MeshFilter),typeof(MeshRenderer));go.transform.SetParent(root,false);go.GetComponent<MeshFilter>().sharedMesh=DioramaBuilder.SaveMesh(mesh,pair.Key);go.GetComponent<MeshRenderer>().sharedMaterial=pair.Value.material;
            }
            UnityEngine.Object.DestroyImmediate(props.gameObject);
            foreach(var guid in AssetDatabase.FindAssets("t:Mesh",new[]{DioramaSurface.Root}))
            {var path=AssetDatabase.GUIDToAssetPath(guid);string name=Path.GetFileNameWithoutExtension(path);if(name.StartsWith("Scenery ") && !used.Contains(name))AssetDatabase.DeleteAsset(path);}
        }
        internal static Texture2D Paint()
        {
            const int n=2048;var pixels=new Color32[n*n];Color straw=DioramaSurface.Hex("ADAB75"), yard=DioramaSurface.Hex("A29370");
            var fields=new[]{new Vector2(-27,3),new Vector2(-20,-21),new Vector2(24,8)};
            var roadDistance=CountrysideLayout.RasterRoads(n,Extent);
            for(int z=0;z<n;z++)for(int x=0;x<n;x++)
            {
                float px=x*180f/(n-1)-90,pz=z*180f/(n-1)-90;float fine=Mathf.PerlinNoise(px*8+17,pz*8+9);
                var c=FieldPigment.Grass(px,pz);
                float distance=roadDistance[z*n+x];float edge=.9f+(Mathf.PerlinNoise(px*3+80,pz*3+80)-.5f)*.12f;
                float lane=1-Mathf.SmoothStep(0,1,Mathf.InverseLerp(edge-.08f,edge+.18f,distance));
                if(lane>0)c=Color.Lerp(c,FieldPigment.Road(px,pz,distance,px),lane);
                if(Mathf.Abs(px)<35 && Mathf.Abs(pz)<30)c=Color.Lerp(c,yard,CountrysideLayout.YardWeight(new Vector2(px,pz))*.72f);
                float field=0;foreach(var center in fields){float dx=Mathf.Abs(px-center.x),fieldZ=Mathf.Abs(pz-center.y);field=Mathf.Max(field,(1-Mathf.SmoothStep(0,1,Mathf.InverseLerp(4,5,dx)))*(1-Mathf.SmoothStep(0,1,Mathf.InverseLerp(2.5f,3.5f,fieldZ))));}c=Color.Lerp(c,straw,field*.18f);c=BattlefieldFarm.Paint(px,pz,c);c*=.965f+fine*.07f;pixels[z*n+x]=c;
            }
            var texture=new Texture2D(n,n,TextureFormat.RGB24,true);texture.SetPixels32(pixels);texture.Apply();string path=DioramaSurface.Root+"/SurroundingPasture.png";File.WriteAllBytes(path,texture.EncodeToPNG());UnityEngine.Object.DestroyImmediate(texture);AssetDatabase.ImportAsset(path);
            var importer=(TextureImporter)AssetImporter.GetAtPath(path);importer.maxTextureSize=2048;importer.anisoLevel=8;importer.textureCompression=TextureImporterCompression.CompressedHQ;importer.SaveAndReimport();return AssetDatabase.LoadAssetAtPath<Texture2D>(path);
        }
    }
}
