using System;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.Rendering;
using Object = UnityEngine.Object;

namespace TheLineHolds.Editor
{
    public static class WinterForestBuilder
    {
        internal const string Art = "Assets/TheLineHolds/Art/WinterForest";
        [MenuItem("The Line Holds/Visuals/Build Winter Forest")]
        public static void Build()
        {
            if (EditorApplication.isPlaying) throw new InvalidOperationException("Stop Play Mode to bake the landscape.");
            var current = EditorSceneManager.GetActiveScene();
            if (current.isDirty) throw new InvalidOperationException("Save the open scene before building Winter Forest.");
            // Copy the current campaign wiring, then replace both countryside scenery roots before saving.
            var scene = EditorSceneManager.OpenScene(PrototypeBuilder.MapScenes[0]);
            var game = Object.FindAnyObjectByType<BattleController>();
            Object.DestroyImmediate(GameObject.Find("Layered countryside diorama"));
            Object.DestroyImmediate(GameObject.Find("Diorama landscape"));
            Directory.CreateDirectory(Art);
            AssetDatabase.Refresh();
            var root = new GameObject("Forest clearing").transform;
            game.winterForest = true;
            game.audioPlayer.ambienceClip = AssetDatabase.LoadAssetAtPath<AudioClip>("Assets/TheLineHolds/Audio/WinterWind.wav");
            var pads = WinterLayout.Pads;
            game.padViews = new Transform[pads.Length];
            for (int i=0; i<pads.Length; i++)
            {
                var site = new GameObject("Emplacement "+(i+1)).transform;
                site.SetParent(root,false);
                site.position = pads[i];
                game.padViews[i] = site;
            }
            WinterSurface.Build(root);
            new WinterScenery(root).Build();
            WinterWeather.Build(root,game.battleCamera);
            EditorSceneManager.SaveScene(scene,PrototypeBuilder.MapScenes[1]);
            EditorBuildSettings.scenes = PrototypeBuilder.MapScenes.Select(p=>new EditorBuildSettingsScene(p,true)).ToArray();
            AssetDatabase.SaveAssets();
        }

        internal static Material Material(string name,Color color)
        {
            var path = Art+"/"+name+".mat";
            var material = AssetDatabase.LoadAssetAtPath<Material>(path);
            if (!material)
            {
                material = new Material(Shader.Find("Universal Render Pipeline/Lit"));
                AssetDatabase.CreateAsset(material,path);
            }
            material.color = color;
            material.SetFloat("_Smoothness",.12f);
            EditorUtility.SetDirty(material);
            return material;
        }
    }
}
