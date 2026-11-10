using System;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.Rendering;
namespace TheLineHolds.Editor
{
    // Repeatable source editing. Originals and their licenses are retained under Tools/Audio/Sources.
    public static class CombatPresentationBuilder
    {
        const string Root = "Assets/TheLineHolds";
        const string Audio = Root + "/Audio/Combat";
        const string Art = Root + "/Art/Combat";
        const int Rate = 48000;
        static readonly string Working = Root + "/Audio/CombatSourceWorking";
        [MenuItem("The Line Holds/Visuals/Build Weapon Presentation")]
        public static string Build()
        {
            if (EditorApplication.isPlaying) throw new InvalidOperationException("Stop Play Mode before authoring.");
            Directory.CreateDirectory(Audio); Directory.CreateDirectory(Art); Directory.CreateDirectory(Working); AssetDatabase.Refresh();
            try
            {
                var source = Path.GetFullPath(Path.Combine(Application.dataPath, "../../Tools/Audio/Sources"));
                var rifle = Read(source + "/Gunshots/mosin.wav");
                var shotgun = Read(source + "/Gunshots/shotty.wav");
                var boom = Read(source + "/Explosion/mechanical_explosion.wav");
                var metal = new[] { Read(source + "/Kenney/impactMetal_medium_000.ogg"), Read(source + "/Kenney/impactMetal_medium_001.ogg"), Read(source + "/Kenney/impactPlate_heavy_000.ogg") };
                var body = new[] { Read(source + "/Kenney/impactPunch_medium_000.ogg"), Read(source + "/Kenney/impactPunch_medium_001.ogg"), Read(source + "/Kenney/impactSoft_heavy_000.ogg") };
                var game = UnityEngine.Object.FindAnyObjectByType<BattleController>();
                var audio = game.audioPlayer;
                audio.machineGun = new AudioClip[3]; audio.cannon = new AudioClip[3]; audio.mortarLaunch = new AudioClip[3];
                audio.softHit = new AudioClip[3]; audio.armorHit = new AudioClip[3]; audio.shellImpact = new AudioClip[3]; audio.mortarImpact = new AudioClip[3];
                float[] rifleShots = { .438f, 3.58f, 5.98f };
                for (int i = 0; i < 3; i++)
                {
                    audio.machineGun[i] = Write("MG-" + i, Filter(Cut(rifle, rifleShots[i], .23f, 1.12f), 220, 8200));
                    var cannon = Cut(rifle, rifleShots[i], .68f, .72f);
                    Mix(cannon, Filter(Cut(boom, .03f + .01f * i, .68f, .9f), 65, 1300), .65f);
                    audio.cannon[i] = Write("Cannon-" + i, Filter(cannon, 65, 8500));
                    audio.mortarLaunch[i] = Write("MortarLaunch-" + i, Filter(Cut(shotgun, .105f + .003f * i, .32f, .65f + .03f * i), 100, 1300));
                    audio.softHit[i] = Write("SoftHit-" + i, Filter(Cut(body[i], 0, .24f, 1), 180, 4500));
                    audio.armorHit[i] = Write("ArmorHit-" + i, Filter(Cut(metal[i], 0, .42f, .93f), 200, 10000));
                    var shell = Filter(Cut(boom, 0, .7f, .96f + i * .02f), 70, 3000);
                    Mix(shell, Cut(metal[i], 0, .7f, .82f), .35f);
                    audio.shellImpact[i] = Write("ShellImpact-" + i, shell);
                    var mortar = Filter(Cut(boom, 0, .86f, .72f + .02f * i), 55, 2100);
                    Mix(mortar, Filter(Cut(body[i], 0, .86f, .6f), 75, 1300), .45f);
                    audio.mortarImpact[i] = Write("MortarImpact-" + i, mortar);
                }
                EditorUtility.SetDirty(audio);
                DioramaBuilder.RefreshModels(new[] { "MachineGun", "AntiTank", "Mortar" });
                var bags = AssetDatabase.LoadAssetAtPath<Material>(DioramaSurface.Root + "/BagCloth.mat");
                bags.SetFloat("_Smoothness", .05f); EditorUtility.SetDirty(bags);
                var fx = game.GetComponent<CombatEffects>(); if (!fx) fx = game.gameObject.AddComponent<CombatEffects>();
                fx.flashMaterial = ParticleMaterial("Warm flash", Stamp("Flash", true), new Color(2.3f, 1.4f, .6f), true);
                var puff = Stamp("Puff", false);
                fx.smokeMaterial = ParticleMaterial("Soft smoke", puff, Color.white);
                fx.dustMaterial = ParticleMaterial("Earth dust", puff, Color.white);
                fx.sparkMaterial = ParticleMaterial("Hot spark", Stamp("Spark", true), new Color(2, 1.1f, .4f), true);
                fx.brassMaterial = ParticleMaterial("Brass", Stamp("Brass", true), new Color(.82f, .65f, .33f));
                fx.shellMaterial = DioramaBuilder.Material("Shell casing", "79774D", .25f);
                game.combatEffects = fx; EditorUtility.SetDirty(fx); EditorUtility.SetDirty(game);
                AssetDatabase.DeleteAsset(Root + "/Audio/Firing.wav"); AssetDatabase.DeleteAsset(Root + "/Audio/Impact.wav");
                AssetDatabase.SaveAssets(); EditorSceneManager.MarkSceneDirty(game.gameObject.scene); EditorSceneManager.SaveScene(game.gameObject.scene);
                return "Built 21 edited weapon/hit variations, muzzle/recoil prefabs and pooled effect materials; scene saved";
            }
            finally { AssetDatabase.DeleteAsset(Working); }
        }
        static float[] Read(string path)
        {
            string target = Working + "/" + Path.GetFileName(path); File.Copy(path, target, true); AssetDatabase.ImportAsset(target, ImportAssetOptions.ForceSynchronousImport);
            Configure(target);
            var clip = AssetDatabase.LoadAssetAtPath<AudioClip>(target); clip.LoadAudioData();
            var data = new float[clip.samples * clip.channels]; if (!clip.GetData(data, 0)) throw new Exception("Unable to decode " + path);
            if (clip.channels != 1 || clip.frequency != Rate) throw new Exception("Expected mono 48 kHz: " + path);
            return data;
        }
        static void Configure(string path)
        {
            var importer = (AudioImporter)AssetImporter.GetAtPath(path); importer.forceToMono = true;
            var settings = importer.defaultSampleSettings; settings.loadType = AudioClipLoadType.DecompressOnLoad;
            settings.compressionFormat = AudioCompressionFormat.PCM; settings.sampleRateSetting = AudioSampleRateSetting.OverrideSampleRate; settings.sampleRateOverride = Rate;
            settings.preloadAudioData = true; importer.defaultSampleSettings = settings; importer.SaveAndReimport();
        }
        static float[] Cut(float[] source, float from, float length, float speed)
        {
            var result = new float[Mathf.RoundToInt(length * Rate)];
            for (int i = 0; i < result.Length; i++)
            {
                float index = from * Rate + i * speed; int first = Mathf.FloorToInt(index);
                if (first >= 0 && first + 1 < source.Length) result[i] = Mathf.Lerp(source[first], source[first + 1], index - first);
            }
            return result;
        }
        static float[] Filter(float[] source, float highpass, float lowpass)
        {
            float low = 0, baseline = 0, lp = 1 - Mathf.Exp(-2 * Mathf.PI * lowpass / Rate), hp = 1 - Mathf.Exp(-2 * Mathf.PI * highpass / Rate);
            var result = new float[source.Length];
            for (int i = 0; i < source.Length; i++) { baseline += hp * (source[i] - baseline); low += lp * (source[i] - baseline - low); result[i] = low; }
            return result;
        }
        static void Mix(float[] destination, float[] layer, float gain)
        { for (int i = 0; i < Mathf.Min(destination.Length, layer.Length); i++) destination[i] += layer[i] * gain; }
        static AudioClip Write(string name, float[] data)
        {
            float peak = data.Max(v => Mathf.Abs(v)); if (peak < .001f) throw new Exception("Silent edit " + name);
            string path = Audio + "/" + name + ".wav";
            using (var writer = new BinaryWriter(File.Create(path)))
            {
                writer.Write(System.Text.Encoding.ASCII.GetBytes("RIFF")); writer.Write(36 + data.Length * 2); writer.Write(System.Text.Encoding.ASCII.GetBytes("WAVEfmt "));
                writer.Write(16); writer.Write((short)1); writer.Write((short)1); writer.Write(Rate); writer.Write(Rate * 2); writer.Write((short)2); writer.Write((short)16);
                writer.Write(System.Text.Encoding.ASCII.GetBytes("data")); writer.Write(data.Length * 2);
                for (int i = 0; i < data.Length; i++)
                {
                    float fade = Mathf.Min(1, i / (Rate * .002f)) * Mathf.Min(1, (data.Length - 1 - i) / (Rate * .018f));
                    writer.Write((short)(Mathf.Clamp(data[i] * .78f / peak * fade, -1, 1) * short.MaxValue));
                }
            }
            AssetDatabase.ImportAsset(path, ImportAssetOptions.ForceSynchronousImport); Configure(path); return AssetDatabase.LoadAssetAtPath<AudioClip>(path);
        }
        static Texture2D Stamp(string name, bool bright)
        {
            const int n = 128; var image = new Texture2D(n, n, TextureFormat.RGBA32, false);
            for (int y = 0; y < n; y++) for (int x = 0; x < n; x++)
            {
                float u = (x + .5f) / n * 2 - 1, v = (y + .5f) / n * 2 - 1, angle = Mathf.Atan2(v, u);
                float radius = Mathf.Sqrt(u * u + v * v);
                float edge = bright ? .65f + .25f * Mathf.Pow(Mathf.Abs(Mathf.Cos(angle * 3)), 6) : .86f + .12f * Mathf.PerlinNoise(u * 5 + 14, v * 5 + 6);
                float alpha = Mathf.Pow(Mathf.Clamp01(1 - radius / edge), bright ? 1.1f : 1.6f);
                if (!bright) alpha *= .65f + .35f * Mathf.PerlinNoise(u * 7 + 4, v * 7 + 9);
                image.SetPixel(x, y, new Color(1, 1, 1, alpha));
            }
            image.Apply(); string path = Art + "/" + name + ".png"; File.WriteAllBytes(path, image.EncodeToPNG()); UnityEngine.Object.DestroyImmediate(image);
            AssetDatabase.ImportAsset(path, ImportAssetOptions.ForceSynchronousImport);
            var importer = (TextureImporter)AssetImporter.GetAtPath(path); importer.alphaIsTransparency = true; importer.mipmapEnabled = true; importer.wrapMode = TextureWrapMode.Clamp; importer.SaveAndReimport();
            return AssetDatabase.LoadAssetAtPath<Texture2D>(path);
        }
        static Material ParticleMaterial(string name, Texture2D texture, Color color, bool additive = false)
        {
            string path = Art + "/" + name + ".mat"; var material = AssetDatabase.LoadAssetAtPath<Material>(path);
            if (!material) { material = new Material(Shader.Find("Universal Render Pipeline/Particles/Unlit")); AssetDatabase.CreateAsset(material, path); }
            material.SetTexture("_BaseMap", texture); material.SetColor("_BaseColor", color); material.SetColor("_Color",color);
            material.SetFloat("_Surface", 1); material.SetFloat("_Blend", additive ? 2 : 0); material.SetFloat("_SrcBlend", (float)BlendMode.SrcAlpha); material.SetFloat("_DstBlend", (float)(additive ? BlendMode.One : BlendMode.OneMinusSrcAlpha));
            material.SetFloat("_SrcBlendAlpha",1);material.SetFloat("_DstBlendAlpha",(float)(additive?BlendMode.One:BlendMode.OneMinusSrcAlpha));
            material.SetFloat("_ZWrite", 0); material.EnableKeyword("_SURFACE_TYPE_TRANSPARENT"); material.renderQueue = (int)RenderQueue.Transparent;
            EditorUtility.SetDirty(material); return material;
        }
    }
}
