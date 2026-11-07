using System.Linq;
using UnityEngine;
namespace TheLineHolds
{
    // Presentation only: aiming never changes targeting, cooldown, projectile origin, or damage.
    public sealed class TowerVisual : MonoBehaviour
    {
        Transform traverse;
        Quaternion bindRotation;
        Vector3 bindPosition,bindForward,aimForward;
        float recoil;
        void Awake()
        {
            traverse=GetComponentsInChildren<Transform>().FirstOrDefault(t=>t.name.StartsWith("Traverse"));
            if(traverse)
            {
                bindRotation=traverse.rotation;bindPosition=traverse.localPosition;
                var barrel=traverse.GetComponentsInChildren<Transform>().First(t=>t.name.StartsWith("Barrel"));
                bindForward=Vector3.ProjectOnPlane(barrel.position-traverse.position,Vector3.up).normalized;
                aimForward=bindForward;
            }
        }
        public void Fire(){recoil=.045f;}
        public void Aim(Vector3 direction,float dt)
        {
            if(!traverse)return;
            direction.y=0;
            if(direction.sqrMagnitude>.0001f)
            {
                aimForward=direction.normalized;
                traverse.rotation=Quaternion.AngleAxis(Vector3.SignedAngle(bindForward,aimForward,Vector3.up),Vector3.up)*bindRotation;
            }
            recoil=Mathf.MoveTowards(recoil,0,dt*.35f);
            traverse.localPosition=bindPosition+transform.InverseTransformDirection(-aimForward)*recoil;
        }
    }
}
