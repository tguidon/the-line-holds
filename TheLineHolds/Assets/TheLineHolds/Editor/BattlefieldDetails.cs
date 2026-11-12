using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEngine;
using UnityEngine.Rendering;

namespace TheLineHolds.Editor
{
    // Authored scenic areas stay outside the route and the original ten gameplay anchors.
    public static class BattlefieldDetails
    {
        const string Root="Assets/TheLineHolds";
        static Transform parent;
        static readonly List<GameObject> pieces=new List<GameObject>();
        public static float GardenWeight(float x,float z)
        {
            float Box(float cx,float cz,float hx,float hz)=>1-Mathf.SmoothStep(0,1,Mathf.InverseLerp(.8f,1.05f,Mathf.Max(Mathf.Abs(x-cx)/hx,Mathf.Abs(z-cz)/hz)));
            return Mathf.Max(Box(-10.4f,1.8f,.75f,.55f),Box(10.3f,-1.8f,.75f,.5f));
        }
        public static float ScrapeWeight(float x,float z)
        {
            float along=1-Mathf.SmoothStep(0,1,Mathf.InverseLerp(2.0f,2.35f,Mathf.Abs(x+.4f)));
            float across=1-Mathf.SmoothStep(0,1,Mathf.InverseLerp(.20f,.36f,Mathf.Abs(z+6.08f)));
            return along*across;
        }
        public static Vector2 NearestRoad(Vector2 p)
        {
            var points=BattleRoute.DefaultPoints;float best=float.MaxValue;Vector2 nearest=p;
            for(int i=1;i<points.Length;i++)
            {
                var a=new Vector2(points[i-1].x,points[i-1].z);var b=new Vector2(points[i].x,points[i].z);var ab=b-a;
                var q=a+ab*Mathf.Clamp01(Vector2.Dot(p-a,ab)/ab.sqrMagnitude);
                if((q-p).sqrMagnitude<best){best=(q-p).sqrMagnitude;nearest=q;}
            }
            return nearest;
        }
        public static bool Reserved(Vector3 p)=>GardenWeight(p.x,p.z)>.05f || (Mathf.Abs(p.x+.4f)<2.9f && Mathf.Abs(p.z+6.08f)<.9f)
            || (p.x<-10.6f && p.z<-3.7f) || (p.x>10.2f && p.z>3.8f);
        static GameObject Model(string name,Vector3 position,Vector3 scale,float yaw=0)
        {
            var prefab=AssetDatabase.LoadAssetAtPath<GameObject>(Root+"/Prefabs/"+name+".prefab");
            if(!prefab)throw new Exception("Missing field kit prefab "+name);
            var go=(GameObject)PrefabUtility.InstantiatePrefab(prefab);go.transform.SetParent(parent);go.transform.position=position;go.transform.localScale=scale;go.transform.rotation=Quaternion.Euler(0,yaw,0);pieces.Add(go);return go;
        }
        static GameObject OnGround(string name,float x,float z,float scale=1,float yaw=0)=>Model(name,BattleRoute.OnGround(x,z),Vector3.one*scale,yaw);
        static void Landmark(string name,Vector3 p){var go=new GameObject(name);go.transform.SetParent(parent);go.transform.position=p;}
        public static void Build(Transform environment)
        {
            parent=new GameObject("Authored battlefield details").transform;parent.SetParent(environment,false);pieces.Clear();
            var sites=new GameObject("Cleared gun sites").transform;sites.SetParent(parent,false);
            for(int i=0;i<BattleRoute.DefaultPads.Length;i++)
            {
                var p=BattleRoute.DefaultPads[i];var anchor=new GameObject("Gun site "+(i+1)).transform;anchor.SetParent(sites);anchor.position=p;
                for(int a=0;a<4;a++)
                {
                    float x=p.x+(a%2==0?-.73f:.73f),z=p.z+(a/2==0?-.73f:.73f);
                    if(Vector2.Distance(new Vector2(x,z),NearestRoad(new Vector2(x,z)))>.86f)OnGround("BuildStake",x,z,1,a*31);
                }
            }
            var route=new BattleRoute(BattleRoute.DefaultPoints);
            Landmark("Enemy entry gateway",route.Points[0]);Landmark("Defended exit checkpoint",route.Points[route.Points.Length-1]);
            OnGround("FieldGate",-11.15f,-5,1,-90);
            OnGround("Checkpoint",11.1f,5,1,-90);
            Sign(-11.89f,-6.48f,"ENEMY","A96544");Sign(11.89f,3.42f,"EXIT","7D936D");
            // Gate approaches and supplies tell apart the exposed enemy lane and the defended rear.
            OnGround("Crate",10.0f,6.15f,.65f,8);OnGround("Crate",10.65f,6.25f,.52f,-12);
            OnGround("FieldFence",-11.35f,-3.15f,.8f,90);
            OnGround("FieldFence",11.35f,2.85f,.8f,90);
            // Small cultivated plots and boundaries give the two farms a place in the landscape.
            foreach(var centre in new[]{new Vector2(-10.4f,1.8f),new Vector2(10.3f,-1.8f)})
            {
                OnGround("FieldFence",centre.x,centre.y+(centre.y>0?.75f:-.75f),1);
                OnGround("FieldFence",centre.x-.92f,centre.y,.85f,90);
                for(int row=0;row<3;row++)for(int plant=0;plant<5;plant++)
                    OnGround("Bush",centre.x-.55f+plant*.27f,centre.y-.35f+row*.34f,.16f,plant*73+row*31);
            }
            OnGround("Crate",-9.7f,-.9f,.5f,15);
            // A shallow timber-lined communication scrape, kept out of every combat corridor.
            for(int section=0;section<3;section++)
            {
                float x=-1.95f+section*1.55f;
                OnGround("Revetment",x,-6.37f,.95f);OnGround("Revetment",x,-5.79f,.95f,180);
                Model("Duckboard",BattleRoute.OnGround(x,-6.08f)+Vector3.up*.035f,Vector3.one);
            }
            OnGround("Crate",2.35f,-6.05f,.65f,30);
            BoundaryWall(-11.48f,-2.75f,1.65f);
            BoundaryWall(10.96f,-.98f,2.00f);
            // Boots and tyres have swept the heavy grit off the crown of the road.
            var gravel=new System.Random(173);
            for(float d=.30f;d<route.Length;d+=.19f+(float)gravel.NextDouble()*1.24f)
            {
                float verge=gravel.Next(2)==0?-.95f:.95f;
                var p=route.Position(d,verge);
                if(BattleRoute.DefaultPads.Any(site=>Vector3.Distance(site,p)<1.05f))continue;
                for(int i=0;i<4;i++)
                {
                    float x=p.x+(float)gravel.NextDouble()*.13f,z=p.z+(float)gravel.NextDouble()*.10f;
                    float size=.029f+(float)gravel.NextDouble()*.040f;
                    var stone=BattleRoute.OnGround(x,z);stone.y-=.023f;
                    Model("FieldStone",stone,new Vector3(size*1.25f,size*.43f,size),gravel.Next(360));
                }
            }
            EarthDetail();
            Combine();
            VergeFlowers.Build(parent);
            Debug.Log("[Map composition] Ten cleared gun sites; entry gate, exit checkpoint, farm plots, boundaries, timber field position, and detailed earth cut.");
        }
        static void BoundaryWall(float x,float z,float length)
        {
            var random=new System.Random(87);
            for(int course=0;course<3;course++)
            {
                int count=Mathf.CeilToInt(length/.32f)-course%2;
                for(int i=0;i<count;i++)
                {
                    float at=z+(i+course%2*.5f)*.32f;
                    var p=BattleRoute.OnGround(x,at)+Vector3.up*(.05f+course*.128f);
                    Model("FieldStone",p,new Vector3(.15f,.105f,.24f),random.Next(-16,17));
                }
            }
        }
        static void EarthDetail()
        {
            var rng=new System.Random(451);
            for(int edge=0;edge<4;edge++)for(int i=0;i<38;i++)
            {
                float t=(i+(float)rng.NextDouble())/38;
                var p=edge==0?new Vector3(Mathf.Lerp(-11.9f,11.9f,t),0,-7.006f):edge==1?new Vector3(12.006f,0,Mathf.Lerp(-6.9f,6.9f,t)):edge==2?new Vector3(Mathf.Lerp(11.9f,-11.9f,t),0,7.006f):new Vector3(-12.006f,0,Mathf.Lerp(6.9f,-6.9f,t));
                float ground=BattleRoute.Ground(p.x,p.z);p.y=Mathf.Lerp(-1.15f,ground-.1f,(float)rng.NextDouble());
                float size=.025f+(float)rng.NextDouble()*.055f;
                Model("FieldStone",p,new Vector3(size*1.5f,size*.65f,size),i*47);
            }
        }
        static readonly Dictionary<char,string> Glyph=new Dictionary<char,string>{
            {'E',"11111/10000/10000/11110/10000/10000/11111"},{'N',"10001/11001/11001/10101/10011/10011/10001"},
            {'M',"10001/11011/10101/10101/10001/10001/10001"},{'Y',"10001/10001/01010/00100/00100/00100/00100"},
            {'X',"10001/01010/01010/00100/01010/01010/10001"},{'I',"11111/00100/00100/00100/00100/00100/11111"},
            {'T',"11111/00100/00100/00100/00100/00100/00100"}};
        static Material SignMaterial(string word,string stripe)
        {
            const int width=512,height=128;var colors=new Color[width*height];var wood=DioramaSurface.Hex("54462E");var accent=DioramaSurface.Hex(stripe);
            for(int y=0;y<height;y++)for(int x=0;x<width;x++)
            {
                float grain=.94f+.035f*Mathf.Sin(y*.31f+Mathf.Sin(x*.06f));
                colors[y*width+x]=(y<10 || y>117?accent:wood)*grain;
            }
            int cell=12,start=(width-(word.Length*6-1)*cell)/2;
            for(int letter=0;letter<word.Length;letter++)
            {
                var rows=Glyph[word[letter]].Split('/');
                for(int row=0;row<7;row++)for(int col=0;col<5;col++)if(rows[row][col]=='1')
                    for(int yy=0;yy<cell-1;yy++)for(int xx=0;xx<cell-1;xx++)
                        colors[(height-24-row*cell-yy)*width+start+(letter*6+col)*cell+xx]=DioramaSurface.Hex("DDD4AE");
            }
            var texture=new Texture2D(width,height,TextureFormat.RGB24,true);texture.SetPixels(colors);texture.Apply();
            string path=DioramaSurface.Root+"/Sign-"+word+".png";File.WriteAllBytes(path,texture.EncodeToPNG());UnityEngine.Object.DestroyImmediate(texture);AssetDatabase.ImportAsset(path);
            var importer=(TextureImporter)AssetImporter.GetAtPath(path);importer.maxTextureSize=512;importer.anisoLevel=4;importer.mipmapEnabled=true;importer.SaveAndReimport();
            var m=DioramaBuilder.Material("Road sign "+word,"FFFFFF");m.mainTexture=AssetDatabase.LoadAssetAtPath<Texture2D>(path);m.SetFloat("_Cull",0);EditorUtility.SetDirty(m);return m;
        }
        static void Sign(float x,float z,string word,string stripe)
        {
            var go=OnGround("RoadSign",x,z,.9f,-22);
            foreach(var r in go.GetComponentsInChildren<Renderer>())
            {
                var materials=r.sharedMaterials;
                for(int i=0;i<materials.Length;i++)if(materials[i].name=="SignPaint")materials[i]=SignMaterial(word,stripe);
                r.sharedMaterials=materials;
            }
        }
        static void Combine()
        {
            var groups=new Dictionary<Material,List<CombineInstance>>();
            foreach(var go in pieces)foreach(var filter in go.GetComponentsInChildren<MeshFilter>())
            {
                var materials=filter.GetComponent<MeshRenderer>().sharedMaterials;
                for(int s=0;s<filter.sharedMesh.subMeshCount;s++)
                {
                    var material=materials[s];if(!groups.TryGetValue(material,out var list)){list=new List<CombineInstance>();groups[material]=list;}
                    list.Add(new CombineInstance{mesh=filter.sharedMesh,subMeshIndex=s,transform=filter.transform.localToWorldMatrix});
                }
            }
            foreach(var group in groups)
            {
                var mesh=new Mesh{name="Field details "+group.Key.name,indexFormat=IndexFormat.UInt32};mesh.CombineMeshes(group.Value.ToArray());
                var go=new GameObject(mesh.name,typeof(MeshFilter),typeof(MeshRenderer));go.transform.SetParent(parent,false);
                go.GetComponent<MeshFilter>().sharedMesh=DioramaBuilder.SaveMesh(mesh,mesh.name);go.GetComponent<MeshRenderer>().sharedMaterial=group.Key;
            }
            foreach(var go in pieces)UnityEngine.Object.DestroyImmediate(go);pieces.Clear();
        }
    }
}
