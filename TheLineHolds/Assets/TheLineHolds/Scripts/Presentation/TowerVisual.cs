using System.Linq;
using UnityEngine;
namespace TheLineHolds
{
    // Presentation only. The simulation never waits for traverse or recoil.
    public sealed class TowerVisual : MonoBehaviour
    {
        Transform traverse, slide, muzzle, barrel;
        Vector3 rootScale;
        public DefenseDefinition Definition=>configured;
        DefenseDefinition configured; GameObject markings;
        public void Configure(DefenseDefinition definition,Material metal)
        {
            if(configured==definition)return;configured=definition;Style=WeaponStyles.For(definition);
            if(markings)Destroy(markings);if(definition.tier<=1)return;
            markings=new GameObject("Veteran markings");markings.transform.SetParent(transform,false);
            for(int i=0;i<definition.tier;i++){var pip=GameObject.CreatePrimitive(PrimitiveType.Cube);Destroy(pip.GetComponent<Collider>());pip.transform.SetParent(markings.transform,false);pip.transform.localPosition=new Vector3(-.16f+i*.16f,.36f,-.62f);pip.transform.localScale=new Vector3(.1f,.12f,.035f);pip.GetComponent<Renderer>().sharedMaterial=metal;var tint=new MaterialPropertyBlock();tint.SetColor("_BaseColor",new Color(.78f,.67f,.36f));pip.GetComponent<Renderer>().SetPropertyBlock(tint);}
        }
        Quaternion bindRotation;
        Vector3 bindPosition, bindForward, aimForward;
        float recoilAge = 10, celebration=10;
        public void Celebrate(){celebration=0;}
        public WeaponStyle Style { get; private set; }
        public Vector3 MuzzlePosition => muzzle ? muzzle.position : transform.position + Vector3.up * .65f;
        public Vector3 MuzzleForward => muzzle && barrel ? (muzzle.position - barrel.position).normalized : aimForward;
        public float RecoilDistance { get; private set; }
        public float RecoveryDuration => Style == WeaponStyle.Cannon ? .48f : Style == WeaponStyle.Mortar ? .26f : .12f;
        void Awake()
        {
            rootScale=transform.localScale;
            Style = name.StartsWith("AntiTank") ? WeaponStyle.Cannon : name.StartsWith("Mortar") ? WeaponStyle.Mortar : WeaponStyle.MachineGun;
            var parts = GetComponentsInChildren<Transform>();
            traverse = parts.FirstOrDefault(t => t.name.StartsWith("Traverse"));
            slide = parts.FirstOrDefault(t => t.name.StartsWith("Recoil"));
            muzzle = parts.FirstOrDefault(t => t.name.StartsWith("Muzzle socket"));
            barrel = parts.FirstOrDefault(t => t.name.StartsWith("Barrel"));
            if (!traverse || !barrel) return;
            bindRotation = traverse.rotation;
            bindForward = Vector3.ProjectOnPlane(barrel.position - traverse.position, Vector3.up).normalized;
            aimForward = bindForward;
            if (slide) bindPosition = slide.localPosition;
        }
        public void Fire() { recoilAge = 0; ApplyRecoil(); }
        public void FireAt(Vector3 destination)
        {
            Turn(destination - transform.position, 0, true);
            Fire();
        }
        public void Aim(Vector3 direction, float dt)
        {
            Turn(direction, dt, false);
            celebration+=dt;
            transform.localScale=rootScale*(1+Mathf.Sin(Mathf.Clamp01(celebration/.42f)*Mathf.PI)*.13f);
            recoilAge += dt;
            ApplyRecoil();
        }
        void Turn(Vector3 direction, float dt, bool snap)
        {
            if (!traverse) return;
            direction.y = 0;
            if (direction.sqrMagnitude < .0001f) return;
            aimForward = direction.normalized;
            var rotation = Quaternion.AngleAxis(Vector3.SignedAngle(bindForward, aimForward, Vector3.up), Vector3.up) * bindRotation;
            traverse.rotation = snap ? rotation : Quaternion.RotateTowards(traverse.rotation, rotation, dt * 540);
        }
        void ApplyRecoil()
        {
            if (!slide) return;
            float travel = Style == WeaponStyle.Cannon ? .17f : Style == WeaponStyle.Mortar ? .065f : .028f;
            float remaining = 1 - Mathf.Clamp01(recoilAge / RecoveryDuration);
            RecoilDistance = travel * remaining * remaining;
            // InverseTransformVector includes the imported FBX scale/axis conversion.
            slide.localPosition = bindPosition + slide.parent.InverseTransformVector(-MuzzleForward * RecoilDistance);
        }
    }
}
