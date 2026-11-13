using System.Linq;
using UnityEngine;
namespace TheLineHolds
{
    // Only miniature children move. The gameplay root always stays on the route.
    public sealed class EnemyVisual : MonoBehaviour
    {
        Transform body; Vector3 position,scale; Quaternion rotation;
        Transform[] wheels; Quaternion[] wheelBinds; Vector3[] wheelAxes; Renderer[] surfaces; Color[] colors;
        MaterialPropertyBlock block; InfantryVisual infantry;
        float hit,deathAge,smokeClock,tintHit=-1,tintHealth=-1; bool vehicle,dying,striking; float health=1;
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
                wheelAxes=wheels.Select(t=>{var size=t.GetComponent<MeshFilter>().sharedMesh.bounds.size;return size.x<size.y && size.x<size.z?Vector3.right:size.y<size.z?Vector3.up:Vector3.forward;}).ToArray();
                surfaces=body.GetComponentsInChildren<Renderer>();colors=surfaces.Select(r=>r.sharedMaterial.color).ToArray();block=new MaterialPropertyBlock();
            }
            hit=deathAge=smokeClock=0;tintHit=-1;dying=striking=false;health=1;body.localPosition=position;body.localRotation=rotation;body.localScale=scale;Tint();
        }
        public void Hit(float fraction){health=Mathf.Clamp01(fraction);hit=.16f;Tint();}
        public void StrikeBase(){striking=true;deathAge=0;}
        public void Die(){dying=true;deathAge=0;hit=.12f;}
        public bool Tick(EnemyState enemy,float dt)
        {
            hit=Mathf.Max(0,hit-dt);
            if(striking)
            {
                deathAge+=dt;
                float duration=vehicle?.65f:.42f;
                float t=Mathf.Clamp01(deathAge/duration);
                // The front of the miniature meets the objective, recoils, then clears the lane.
                float kick=Mathf.Sin(Mathf.PI*Mathf.Min(1,t/.55f));
                body.localPosition=position+Vector3.back*kick*(vehicle?.30f:.15f);
                body.localRotation=Quaternion.Euler(vehicle?-6*kick:14*kick,0,0)*rotation;
                body.localScale=scale*(1-Mathf.SmoothStep(0,1,Mathf.InverseLerp(.55f,1,t)));
                return t>=1;
            }
            if(dying)
            {
                deathAge+=dt;float t=Mathf.Clamp01(deathAge/DeathDuration);
                body.localRotation=rotation*Quaternion.Euler(Vector3.Lerp(Vector3.zero,new Vector3(vehicle?5:75,0,vehicle?9:18),Mathf.SmoothStep(0,1,deathAge/.4f)));
                body.localPosition=position+Vector3.down*(vehicle?.04f:.12f);
                float fade=1-Mathf.SmoothStep(0,1,Mathf.InverseLerp(.65f,1,t));body.localScale=scale*fade;
                Tint();return deathAge>=DeathDuration;
            }
            if(vehicle)
            {
                // The route supplies hull attitude. Only roll the wheels during travel.
                for(int i=0;i<wheels.Length;i++)
                    wheels[i].localRotation=wheelBinds[i]*Quaternion.AngleAxis(enemy.Distance*450,wheelAxes[i]);
            }
            else
            {
                float phase=enemy.Distance*10+enemy.Id*.73f;
                body.localPosition=position+Vector3.up*(Mathf.Abs(Mathf.Sin(phase))*.025f);
                body.localRotation=rotation*Quaternion.Euler(2,0,Mathf.Sin(phase)*2.5f+hit*22);
                infantry?.Pose(enemy.Distance,enemy.Id);
            }
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
                block.SetColor("_BaseColor",Color.Lerp(c,new Color(.95f,.83f,.57f),vehicle?0:Mathf.Clamp01(hit/.16f)*.32f));surfaces[i].SetPropertyBlock(block);
            }
        }
    }
}
