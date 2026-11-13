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

        internal void Place(string name,float x,float z,Vector3 scale,float yaw=0)
        {
            if(!models.TryGetValue(name,out var model))
            {
                model=AssetDatabase.LoadAssetAtPath<GameObject>(TownBuilder.Art+"/Models/"+name+".fbx");
                if(!model)model=AssetDatabase.LoadAssetAtPath<GameObject>("Assets/TheLineHolds/Prefabs/"+name+".prefab");
                if(!model)throw new InvalidOperationException("Missing Saint-Aubin scenery: "+name);
                models.Add(name,model);
            }
            var go=(GameObject)PrefabUtility.InstantiatePrefab(model,root);
            go.transform.localPosition=new Vector3(x,0,z);
            // Preserve the model import transform, including its FBX axis conversion.
            go.transform.localRotation=Quaternion.Euler(0,yaw,0)*go.transform.localRotation;
            go.transform.localScale=Vector3.Scale(go.transform.localScale,scale);
            if(AssetDatabase.GetAssetPath(model).EndsWith(".fbx",StringComparison.OrdinalIgnoreCase))
                foreach(var renderer in go.GetComponentsInChildren<MeshRenderer>())
                    renderer.sharedMaterial=pigments;
        }
        void Prop(string name,float x,float z,float size,float angle=0)=>Place(name,x,z,Vector3.one*size,angle);

        internal void Dress(Func<float,float,float,bool> clear)
        {
            // The market occupies the two quiet sides of the hall rather than the turning apron.
            Prop("ProduceStall",4.85f,3.42f,.90f,-7);
            Prop("ProduceStall",6.55f,3.36f,.77f,4);
            Prop("Handcart",6.97f,.01f,.72f,73);
            Prop("ProduceStall",-5.85f,9.45f,.92f,6);
            Prop("Handcart",-4.69f,9.52f,.78f,99);
            Prop("Crate",6.98f,2.68f,.48f,12);
            Prop("StreetLamp",7.27f,.64f,1);
            Prop("StreetLamp",4.41f,.68f,.95f);
            Prop("StreetLamp",2.02f,7.65f,.95f);
            Prop("StreetLamp",11,-3.8f,.98f);
            // Orchard blocks flank the two farm lanes. Keep trees back from traffic and gun sites.
            for(int row=-5;row<=5;row++)
                for(int col=0;col<7;col++)
                {
                    float x=-10.1f-col*2.56f+Between(-.42f,.42f);
                    float z=row*2.85f+Between(-.42f,.42f);
                    if(random.NextDouble()<.28 || !clear(x,z,.75f))continue;
                    Prop("AppleTree",x,z,Between(.74f,1.18f),Between(0,360));
                }
            // Low garden detail fills the gaps between plots. Keep projected crowns away from sites.
            for(int i=0;i<250;i++)
            {
                float x=Between(-9,22), z=Between(-14,16);
                if(!clear(x,z,.36f))continue;
                if(i%9==0 && clear(x,z,.86f))Prop("AppleTree",x,z,Between(.62f,.81f),Between(0,360));
                else if(i%3==0)Prop("Bush",x,z,Between(.36f,.65f),Between(0,360));
                else Prop("SpringVerge",x,z,Between(.65f,1.18f),Between(0,360));
            }
            for(int i=0;i<145;i++)
            {
                float x=Between(-26,-8.5f),z=Between(-12,14);
                if(clear(x,z,.09f))Prop("SpringVerge",x,z,Between(.8f,1.7f),Between(0,360));
            }
            for(int i=0;i<6;i++)
                foreach(float z in new[]{-8.48f,8.48f})
                {
                    float x=-11.8f-i*2.17f;
                    if(clear(x,z,.15f))Prop("FieldFence",x,z,.78f);
                }
        }
        float Between(float min,float max)=>Mathf.Lerp(min,max,(float)random.NextDouble());
    }
}
