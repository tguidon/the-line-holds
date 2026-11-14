using UnityEditor;
using UnityEngine;
using UnityEngine.Rendering;

namespace TheLineHolds.Editor
{
    internal static class TownEmbers
    {
        internal static void Build(Transform parent,Vector3 position,bool burning)
        {
            var smoke=Emitter("Smoke from damaged building",parent,position,3.4f,.84f,1.05f,3.0f,
                new Color(.25f,.24f,.23f,.67f),"Soft smoke");
            var expansion=smoke.sizeOverLifetime;
            expansion.enabled=true;
            expansion.size=new ParticleSystem.MinMaxCurve(1,AnimationCurve.EaseInOut(0,.46f,1,1.75f));
            var drift=smoke.velocityOverLifetime;
            drift.enabled=true;
            drift.space=ParticleSystemSimulationSpace.World;
            drift.x=.17f;
            drift.z=.09f;
            if(burning)
                Emitter("Flame in exposed floor",parent,position,13f,.74f,.80f,.85f,
                    new Color(1f,.42f,.12f,.87f),"Vehicle fire");
        }

        static ParticleSystem Emitter(string name,Transform parent,Vector3 position,float rate,float size,
            float rise,float lifetime,Color tint,string material)
        {
            var go=new GameObject(name);
            go.transform.SetParent(parent,false);
            go.transform.SetPositionAndRotation(position,Quaternion.Euler(-90,0,0));
            var ps=go.AddComponent<ParticleSystem>();
            ps.Stop(true,ParticleSystemStopBehavior.StopEmittingAndClear);
            var main=ps.main;
            main.prewarm=true;
            main.startLifetime=new ParticleSystem.MinMaxCurve(lifetime*.72f,lifetime);
            main.startSpeed=new ParticleSystem.MinMaxCurve(rise*.73f,rise);
            main.startSize=new ParticleSystem.MinMaxCurve(size*.67f,size);
            main.startColor=tint;
            main.maxParticles=Mathf.CeilToInt(lifetime*rate)+2;
            var shape=ps.shape;
            shape.shapeType=ParticleSystemShapeType.Cone;
            shape.angle=7;
            shape.radius=.22f;
            var emission=ps.emission;
            emission.rateOverTime=rate;
            var colors=new Gradient();
            colors.SetKeys(new[]{new GradientColorKey(Color.white,0),new GradientColorKey(Color.white,1)},
                new[]{new GradientAlphaKey(0,0),new GradientAlphaKey(1,.14f),new GradientAlphaKey(.84f,.54f),new GradientAlphaKey(0,1)});
            var fade=ps.colorOverLifetime;
            fade.enabled=true;
            fade.color=colors;
            var renderer=ps.GetComponent<ParticleSystemRenderer>();
            renderer.sharedMaterial=AssetDatabase.LoadAssetAtPath<Material>("Assets/TheLineHolds/Art/Combat/"+material+".mat");
            renderer.shadowCastingMode=ShadowCastingMode.Off;
            renderer.receiveShadows=false;
            return ps;
        }
    }
}
