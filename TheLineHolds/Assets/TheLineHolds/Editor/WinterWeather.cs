using UnityEditor;
using UnityEngine;
using UnityEngine.Rendering;

namespace TheLineHolds.Editor
{
    internal static class WinterWeather
    {
        internal static void Build(Transform root,Camera camera)
        {
            var sun=Object.FindAnyObjectByType<Light>();
            sun.color=DioramaSurface.Hex("FFEEE0");sun.intensity=1.85f;
            sun.transform.rotation=Quaternion.Euler(32,-42,0);
            sun.shadowStrength=.74f;
            RenderSettings.ambientSkyColor=DioramaSurface.Hex("ADC4E0");
            RenderSettings.ambientEquatorColor=DioramaSurface.Hex("879EB5");
            RenderSettings.ambientGroundColor=DioramaSurface.Hex("697A8B");
            camera.backgroundColor=DioramaSurface.Hex("B8CBDA");

            var snow=Emitter(root,"Light falling snow",new Vector3(-4,9,0),new Vector3(43,1,36),38,
                new Color(.98f,1,1,.84f),new Vector2(.044f,.106f),new Vector2(5,8),new Vector3(.74f,-1.26f,.18f));
            var wind=snow.noise;wind.enabled=true;wind.strength=.28f;wind.frequency=.33f;wind.scrollSpeed=.24f;
            var drift=Emitter(root,"Spindrift through clearing",new Vector3(-8,1.1f,.7f),new Vector3(26,.36f,16),5,
                new Color(.91f,.97f,1,.21f),new Vector2(.11f,.23f),new Vector2(1.4f,2.5f),new Vector3(2.5f,.025f,.48f));
            var streak=drift.GetComponent<ParticleSystemRenderer>();streak.renderMode=ParticleSystemRenderMode.Stretch;
            streak.lengthScale=2.2f;streak.velocityScale=.18f;
        }
        static ParticleSystem Emitter(Transform root,string name,Vector3 position,Vector3 extent,float rate,
            Color color,Vector2 size,Vector2 life,Vector3 velocity)
        {
            var go=new GameObject(name);go.transform.SetParent(root,false);go.transform.position=position;
            var ps=go.AddComponent<ParticleSystem>();
            ps.Stop(true,ParticleSystemStopBehavior.StopEmittingAndClear);
            var main=ps.main;main.startSpeed=0;main.startColor=color;main.prewarm=true;
            main.startSize=new ParticleSystem.MinMaxCurve(size.x,size.y);
            main.startLifetime=new ParticleSystem.MinMaxCurve(life.x,life.y);
            main.maxParticles=Mathf.CeilToInt(rate*life.y)+8;main.simulationSpace=ParticleSystemSimulationSpace.World;
            var shape=ps.shape;shape.shapeType=ParticleSystemShapeType.Box;shape.scale=extent;
            var motion=ps.velocityOverLifetime;motion.enabled=true;motion.space=ParticleSystemSimulationSpace.World;
            motion.x=velocity.x;motion.y=velocity.y;motion.z=velocity.z;
            var emission=ps.emission;emission.rateOverTime=rate;
            var fade=new Gradient();fade.SetKeys(new[]{new GradientColorKey(Color.white,0),new GradientColorKey(Color.white,1)},
                new[]{new GradientAlphaKey(0,0),new GradientAlphaKey(1,.14f),new GradientAlphaKey(1,.64f),new GradientAlphaKey(0,1)});
            var opacity=ps.colorOverLifetime;opacity.enabled=true;opacity.color=fade;
            var renderer=ps.GetComponent<ParticleSystemRenderer>();
            renderer.sharedMaterial=AssetDatabase.LoadAssetAtPath<Material>("Assets/TheLineHolds/Art/Combat/Soft smoke.mat");
            renderer.shadowCastingMode=ShadowCastingMode.Off;renderer.receiveShadows=false;
            return ps;
        }
    }
}
