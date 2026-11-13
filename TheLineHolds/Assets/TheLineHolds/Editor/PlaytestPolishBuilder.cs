using System;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
namespace TheLineHolds.Editor
{
    public static class PlaytestPolishBuilder
    {
        public static readonly string[] Upgrades={"MachineGunII","MachineGunRapid","MachineGunAP","AntiTankII","TankHunter","FieldHowitzer","MortarII","SiegeMortar","BarrageMortar"};
        public static string Build()
        {
            if(EditorApplication.isPlaying)throw new Exception("Stop Play Mode first");
            DioramaBuilder.RefreshModels(new[]{"Barn"});DioramaBuilder.RefreshModels(Upgrades);
            foreach(string key in Upgrades)
            {
                var d=AssetDatabase.LoadAssetAtPath<DefenseDefinition>("Assets/TheLineHolds/Data/Defenses/"+key+".asset");d.prefab=AssetDatabase.LoadAssetAtPath<GameObject>("Assets/TheLineHolds/Prefabs/"+key+".prefab");EditorUtility.SetDirty(d);
            }
            DioramaBuilder.Map();
            var game=UnityEngine.Object.FindAnyObjectByType<BattleController>();var audio=game.audioPlayer;
            string firePath="Assets/TheLineHolds/Art/Combat/Vehicle fire.mat";var fire=AssetDatabase.LoadAssetAtPath<Material>(firePath);if(!fire){fire=new Material(game.combatEffects.flashMaterial);AssetDatabase.CreateAsset(fire,firePath);}fire.SetTexture("_BaseMap",game.combatEffects.smokeMaterial.GetTexture("_BaseMap"));game.combatEffects.fireballMaterial=fire;EditorUtility.SetDirty(fire);EditorUtility.SetDirty(game.combatEffects);
            string path="Assets/TheLineHolds/Audio/Countryside.wav";AssetDatabase.ImportAsset(path);var importer=(AudioImporter)AssetImporter.GetAtPath(path);var settings=importer.defaultSampleSettings;settings.loadType=AudioClipLoadType.CompressedInMemory;settings.compressionFormat=AudioCompressionFormat.Vorbis;settings.quality=.7f;importer.defaultSampleSettings=settings;importer.SaveAndReimport();audio.ambienceClip=AssetDatabase.LoadAssetAtPath<AudioClip>(path);EditorUtility.SetDirty(audio);
            string iconPath="Assets/TheLineHolds/Art/Identity/AppIcon.png";AssetDatabase.ImportAsset(iconPath);var ti=(TextureImporter)AssetImporter.GetAtPath(iconPath);ti.textureCompression=TextureImporterCompression.Uncompressed;ti.maxTextureSize=1024;ti.mipmapEnabled=false;ti.SaveAndReimport();
            DesktopIdentity.Configure();
            AssetDatabase.SaveAssets();EditorSceneManager.MarkSceneDirty(game.gameObject.scene);EditorSceneManager.SaveScene(game.gameObject.scene);return "Saved continuous countryside, wider road, nine authored upgrade models, original ambience and application icon";
        }
    }
}
