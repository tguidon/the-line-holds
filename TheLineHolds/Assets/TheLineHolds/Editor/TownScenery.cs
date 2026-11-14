using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

namespace TheLineHolds.Editor
{
    internal sealed class TownScenery
    {
        readonly Transform root;
        readonly Material pigments;
        readonly Dictionary<string,GameObject> models=new();
        readonly System.Random random=new(426);
        internal TownScenery(Transform root)
        {
            this.root=root;
            string path=TownBuilder.Art+"/Town pigments.png";
            var importer=(TextureImporter)AssetImporter.GetAtPath(path);
            importer.mipmapEnabled=false;
            importer.textureCompression=TextureImporterCompression.Uncompressed;
            importer.filterMode=FilterMode.Point;
            importer.SaveAndReimport();
            pigments=TownBuilder.Mat("Town pigments","FFFFFF");
            pigments.mainTexture=AssetDatabase.LoadAssetAtPath<Texture2D>(path);
            pigments.enableInstancing=true;
            EditorUtility.SetDirty(pigments);
        }

        internal GameObject Place(string name,float x,float z,Vector3 scale,float yaw=0)
        {
            if(!models.TryGetValue(name,out var model))
            {
                model=AssetDatabase.LoadAssetAtPath<GameObject>(TownBuilder.Art+"/Models/"+name+".fbx");
                if(!model)model=AssetDatabase.LoadAssetAtPath<GameObject>("Assets/TheLineHolds/Prefabs/"+name+".prefab");
                if(!model)throw new InvalidOperationException("Missing Saint-Aubin scenery: "+name);
                models.Add(name,model);
            }
            // Scale lot width, height and depth in Unity world axes, outside FBX axis conversion.
            var lot=new GameObject(name);
            lot.transform.SetParent(root,false);
            lot.transform.SetLocalPositionAndRotation(new Vector3(x,0,z),Quaternion.Euler(0,yaw,0));
            lot.transform.localScale=scale;
            var go=(GameObject)PrefabUtility.InstantiatePrefab(model,lot.transform);
            if(AssetDatabase.GetAssetPath(model).EndsWith(".fbx",StringComparison.OrdinalIgnoreCase))
            {
                foreach(var renderer in go.GetComponentsInChildren<MeshRenderer>())
                {
                    renderer.sharedMaterial=pigments;
                    PrefabUtility.RecordPrefabInstancePropertyModifications(renderer);
                }
            }
            return lot;
        }
        void Prop(string name,float x,float z,float size,float angle=0)=>Place(name,x,z,Vector3.one*size,angle);

        internal void Dress()
        {
            Prop("Fountain",9.81f,4.44f,.91f);
            Prop("ProduceStall",10.04f,5.18f,.69f,0);
            Prop("Handcart",5.08f,4.88f,.61f,-32);
            foreach(var p in new[]{new Vector2(-8.1f,1.64f),new Vector2(-2.04f,4.05f),
                new Vector2(1.31f,-3.1f),new Vector2(8.68f,-3.27f),new Vector2(8.96f,5.00f)})
                Prop("StreetLamp",p.x,p.y,.79f);
            // Garden courts break up the masonry. Orchard growth is confined to the outer gate.
            foreach(var p in new[]{new Vector2(-11.8f,7.15f),new Vector2(-3.98f,9.97f),
                new Vector2(-1.81f,-7.94f),new Vector2(-6.92f,-5.6f),new Vector2(14.96f,-.04f),
                new Vector2(9.27f,8.78f),new Vector2(1.03f,-8.25f)})
                Prop("AppleTree",p.x,p.y,Between(.64f,.88f),Between(0,360));
            for(int j=-4;j<=4;j++)
                for(int i=0;i<3;i++)
                {
                    float x=-16.2f-i*2.81f+Between(-.42f,.42f),z=j*3.08f+Between(-.48f,.48f);
                    if(Mathf.Abs(Mathf.Abs(z)-2.9f)<2.25f)continue;
                    Prop("AppleTree",x,z,Between(.65f,.93f),Between(0,360));
                    Prop("SpringVerge",x+.48f,z+.61f,1.1f);
                }
        }
        float Between(float min,float max)=>Mathf.Lerp(min,max,(float)random.NextDouble());
    }
}
