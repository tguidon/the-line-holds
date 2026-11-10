using System.Linq;
using UnityEngine;
namespace TheLineHolds
{
    // Only miniature children move. The gameplay root always stays on the route.
    public sealed class EnemyVisual : MonoBehaviour
    {
        Transform body; Vector3 position,scale; Quaternion rotation;
        Transform[] wheels; Quaternion[] wheelBinds; Renderer[] surfaces; Color[] colors;
        MaterialPropertyBlock block; InfantryVisual infantry;
        float hit,deathAge,smokeClock,tintHit=-1,tintHealth=-1; bool vehicle,dying; float health=1;
        public bool Dying => dying;
        public float DeathDuration => vehicle ? 1.35f : .85f;
        public void Initialize(bool isVehicle)
        {
            vehicle=isVehicle;
            if(!body)
            {
                body=transform.Find("Miniature");if(!body)body=transform.GetChild(0);
                position=body.localPosition;rotation=body.localRotation;scale=body.localScale;
                infantry=GetComponent<InfantryVisual>();
                wheels=GetComponentsInChildren<Transform>().Where(t=>t.name.StartsWith("RoadWheel") || t.name.StartsWith("Rolling wheel")).ToArray();
                wheelBinds=wheels.Select(t=>t.localRotation).ToArray();
                surfaces=body.GetComponentsInChildren<Renderer>();colors=surfaces.Select(r=>r.sharedMaterial.color).ToArray();block=new MaterialPropertyBlock();
            }
            hit=deathAge=smokeClock=0;tintHit=-1;dying=false;health=1;body.localPosition=position;body.localRotation=rotation;body.localScale=scale;Tint();
        }
        public void Hit(float fraction){health=Mathf.Clamp01(fraction);hit=.16f;Tint();}
        public void Die(){dying=true;deathAge=0;hit=.12f;}
        public bool Tick(EnemyState enemy,float dt)
        {
            hit=Mathf.Max(0,hit-dt);
            if(dying)
            {
                deathAge+=dt;float t=Mathf.Clamp01(deathAge/DeathDuration);
                body.localRotation=rotation*Quaternion.Euler(Vector3.Lerp(Vector3.zero,new Vector3(vehicle?5:75,0,vehicle?9:18),Mathf.SmoothStep(0,1,deathAge/.4f)));
                body.localPosition=position+Vector3.down*(vehicle?.04f:.12f);
                float fade=1-Mathf.SmoothStep(0,1,Mathf.InverseLerp(.65f,1,t));body.localScale=scale*fade;
                Tint();return deathAge>=DeathDuration;
            }
            float phase=enemy.Distance*(vehicle?7:10)+enemy.Id*.73f;
            body.localPosition=position+Vector3.up*(vehicle?Mathf.Sin(phase)*.012f:Mathf.Abs(Mathf.Sin(phase))*.025f);
            body.localRotation=rotation*Quaternion.Euler(vehicle?Mathf.Sin(phase*.6f)*1.1f:2,0,Mathf.Sin(phase)*(vehicle?1.1f:2.5f)+hit*22);
            infantry?.Pose(enemy.Distance,enemy.Id);
            for(int i=0;i<wheels.Length;i++)wheels[i].localRotation=wheelBinds[i]*Quaternion.Euler(0,enemy.Distance*450,0);
            Tint();smokeClock-=dt;return false;
        }
        public bool SmokeDue => vehicle && health<.35f && smokeClock<=0;
        public void SmokeEmitted(){smokeClock=.45f;}
        void Tint()
        {
            if(Mathf.Abs(tintHit-hit)<.001f && tintHealth==health)return; tintHit=hit;tintHealth=health;
            for(int i=0;i<surfaces.Length;i++)
            {
                var c=colors[i]*Mathf.Lerp(.75f,1,health);c.a=1;
                block.SetColor("_BaseColor",Color.Lerp(c,new Color(.95f,.83f,.57f),Mathf.Clamp01(hit/.16f)*.48f));surfaces[i].SetPropertyBlock(block);
            }
        }
    }
}
