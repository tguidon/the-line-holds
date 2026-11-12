using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;
namespace TheLineHolds
{
    // A fixed set of world-space emitters and a warmed projectile pool. No damage logic lives here.
    public sealed class CombatEffects : MonoBehaviour
    {
        public Material fireballMaterial,flashMaterial, smokeMaterial, dustMaterial, sparkMaterial, brassMaterial, shellMaterial;
        ParticleSystem flash, smoke, dust, sparks, brass, fireball, fragments, purchase;
        readonly List<ParticleSystem> emitters = new List<ParticleSystem>();
        readonly Dictionary<int, Flight> flights = new Dictionary<int, Flight>();
        readonly List<int> expired = new List<int>();
        readonly System.Random variation = new System.Random(1944);
        UnityEngine.Pool.ObjectPool<Flight> pool;
        Transform root;
        int created;
        public int ProjectileObjectsCreated => created;
        public int ActiveProjectiles => flights.Count;
        public int ParticleCount { get { int n = 0; foreach (var ps in emitters) n += ps.particleCount; return n; } }
        sealed class Flight
        {
            public GameObject View; public Transform Body; public LineRenderer Trail;
            public Renderer BodyRenderer; public ShotState Shot; public Vector3 Origin, Previous;
            public WeaponStyle Style;
        }
        void Awake()
        {
            root = new GameObject("Pooled combat effects").transform; root.SetParent(transform, false);
            purchase=Emitter("Upgrade celebration",sparkMaterial,128,false);
            flash = Emitter("Muzzle flash", flashMaterial, 128, false);
            smoke = Emitter("Weapon smoke", smokeMaterial, 256, false);
            dust = Emitter("Road dust", dustMaterial, 384, false);
            sparks = Emitter("Armor sparks", sparkMaterial, 192, true);
            brass = Emitter("Ejected cartridges", brassMaterial, 128, true);
            fireball = Emitter("Vehicle fireball", fireballMaterial?fireballMaterial:flashMaterial, 128, false);
            fragments = Emitter("Vehicle fragments", shellMaterial, 128, true);
            var chunk=GameObject.CreatePrimitive(PrimitiveType.Cube);chunk.SetActive(false);var debrisRenderer=fragments.GetComponent<ParticleSystemRenderer>();debrisRenderer.renderMode=ParticleSystemRenderMode.Mesh;debrisRenderer.mesh=chunk.GetComponent<MeshFilter>().sharedMesh;Destroy(chunk);
            pool = new UnityEngine.Pool.ObjectPool<Flight>(CreateFlight, f => f.View.SetActive(true), f => f.View.SetActive(false), f => Destroy(f.View), true, 32, 128);
            var warm = new List<Flight>();
            for (int i = 0; i < 32; i++) warm.Add(pool.Get());
            foreach (var f in warm) pool.Release(f);
        }
        ParticleSystem Emitter(string title, Material material, int capacity, bool debris)
        {
            var go = new GameObject(title); go.transform.SetParent(root, false);
            var ps = go.AddComponent<ParticleSystem>(); ps.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
            var main = ps.main; main.playOnAwake = false; main.loop = false; main.simulationSpace = ParticleSystemSimulationSpace.World;
            main.maxParticles = capacity; main.startSpeed = 0; main.startColor = new Color(.6f, .5f, .3f);
            main.gravityModifier = debris ? .65f : .035f;
            var emission = ps.emission; emission.enabled = false;
            var shape = ps.shape; shape.enabled = false;
            var color = ps.colorOverLifetime; color.enabled = true;
            var fade = new Gradient(); fade.SetKeys(new[] { new GradientColorKey(Color.white, 0), new GradientColorKey(Color.white, 1) },
                new[] { new GradientAlphaKey(1, 0), new GradientAlphaKey(.7f, .3f), new GradientAlphaKey(0, 1) }); color.color = fade;
            var size = ps.sizeOverLifetime; size.enabled = true;
            size.size = new ParticleSystem.MinMaxCurve(1, new AnimationCurve(new Keyframe(0, .6f), new Keyframe(.35f, 1), new Keyframe(1, 0)));
            var renderer = ps.GetComponent<ParticleSystemRenderer>(); renderer.sharedMaterial = material;
            renderer.shadowCastingMode = ShadowCastingMode.Off; renderer.receiveShadows = false;
            if (debris) { renderer.renderMode = ParticleSystemRenderMode.Stretch; renderer.lengthScale = 1.5f; renderer.velocityScale = .025f; }
            emitters.Add(ps); return ps;
        }
        float V(float lo, float hi) => Mathf.Lerp(lo, hi, (float)variation.NextDouble());
        void Puff(ParticleSystem ps, Vector3 p, Vector3 velocity, float size, float life, Color color)
        {
            ps.Emit(new ParticleSystem.EmitParams { position = p, velocity = velocity, startSize = size, startLifetime = life, startColor = color, rotation = V(0, 360) }, 1);
        }
        public void Fire(ShotState shot, TowerVisual tower)
        {
            var style = WeaponStyles.For(shot.Definition);
            var origin = tower ? tower.MuzzlePosition : shot.Origin;
            var forward = tower ? tower.MuzzleForward : (shot.Destination - origin).normalized;
            bool mg = style == WeaponStyle.MachineGun;
            Puff(flash, origin + forward * .04f, forward * .2f, mg ? .36f : style == WeaponStyle.Cannon ? .78f : .42f, mg ? .085f : .12f, new Color(1, .86f, .42f));
            int count = mg ? 1 : style == WeaponStyle.Cannon ? 4 : 3;
            for (int i = 0; i < count; i++)
                Puff(smoke, origin + forward * V(0, .14f), forward * V(.25f, .7f) + Vector3.up * V(.25f, .6f), V(.22f, mg ? .34f : .58f), V(.18f, .40f), new Color(.71f, .73f, .70f, .48f));
            if (mg)
            {
                var side = Vector3.Cross(Vector3.up, forward).normalized;
                Puff(brass, origin - forward * .5f + side * .1f, side * V(.7f, 1.2f) + Vector3.up * .8f, .065f, .45f, new Color(.8f, .63f, .29f));
            }
            if (style == WeaponStyle.Cannon) GroundDust(tower ? tower.transform.position : shot.Origin, 6, .55f);
            var flight = pool.Get(); flight.Shot = shot; flight.Style = style; flight.Origin = flight.Previous = origin;
            flight.View.transform.position = origin;
            flight.Body.localScale = mg ? new Vector3(.045f, .045f, .15f) : new Vector3(.11f, .11f, style == WeaponStyle.Mortar ? .25f : .32f);
            flight.BodyRenderer.sharedMaterial = mg ? sparkMaterial : shellMaterial;
            bool arcing = style == WeaponStyle.Mortar;
            flight.Trail.sharedMaterial = arcing ? smokeMaterial : flashMaterial;
            flight.Trail.startColor = arcing ? new Color(.82f,.81f,.70f,.65f) :
                mg ? new Color(1f,.85f,.44f) : new Color(1f,.66f,.32f);
            var tail = flight.Trail.startColor;
            tail.a = 0;
            flight.Trail.endColor = tail;
            flight.Trail.startWidth = mg ? .048f : arcing ? .095f : .080f;
            flight.Trail.endWidth = arcing ? .12f : .014f;
            flight.Trail.SetPosition(0, origin); flight.Trail.SetPosition(1, origin);
            flights.Add(shot.Id, flight);
        }
        public void Impact(ShotState shot)
        {
            var style = WeaponStyles.For(shot.Definition); bool armor = shot.Target.Definition.spawnSeparation>0;
            var p = shot.Destination;
            // Simulation hits the unit centre. Draw ricochets at the exposed hull edge.
            if (armor && style != WeaponStyle.Mortar)
                p += Vector3.ProjectOnPlane(shot.Origin - p, Vector3.up).normalized * .62f + Vector3.up * .22f;
            if (shot.Definition.splashRadius > 0)
                BlastSkirt(shot.Destination, shot.Definition.splashRadius);
            if (style == WeaponStyle.Mortar)
            {
                p.y = BattleRoute.Ground(p.x, p.z) + .05f;
                Puff(flash, p + Vector3.up * .12f, Vector3.up * .4f, .65f, .12f, new Color(1.3f, .74f, .26f));
                GroundDust(p, 6, .78f);
                for (int i = 0; i < 4; i++) Puff(smoke, p + Vector3.up * V(.2f, .4f), new Vector3(V(-.45f, .45f), V(.6f, 1.3f), V(-.45f, .45f)), V(.38f, .63f), V(.28f, .52f), new Color(.35f, .32f, .25f, .75f));
            }
            else if (armor)
            {
                int count = style == WeaponStyle.Cannon ? 10 : 4;
                for (int i = 0; i < count; i++) Puff(sparks, p, new Vector3(V(-1.5f, 1.5f), V(.5f, 2), V(-1.5f, 1.5f)), V(.055f, .12f), V(.13f, .3f), new Color(1.5f, 1.05f, .4f));
                Puff(flash, p, Vector3.up * .1f, style==WeaponStyle.Cannon?.65f:.18f, .10f, new Color(1.4f, .9f, .36f));
                if(style==WeaponStyle.Cannon){Puff(smoke,p,Vector3.up*.5f,.55f,.45f,new Color(.3f,.29f,.25f,.8f));GroundDust(p,7,.7f);}
            }
            else GroundDust(p, style == WeaponStyle.Cannon ? 9 : 3, style == WeaponStyle.Cannon ? .7f : .27f);
            if (flights.Remove(shot.Id, out var flight)) pool.Release(flight);
        }
        void BlastSkirt(Vector3 centre, float radius)
        {
            // Low, discontinuous dust follows the real blast footprint, leaving the target visible.
            centre.y=BattleRoute.Ground(centre.x,centre.z)+.07f;
            float start=V(0,2*Mathf.PI);
            for(int i=0;i<9;i++)
            {
                float angle=start+i*(2*Mathf.PI/9)+V(-.1f,.1f);
                var outward=new Vector3(Mathf.Cos(angle),0,Mathf.Sin(angle));
                Puff(dust,centre+outward*(radius*.17f),outward*radius*1.9f+Vector3.up*.23f,
                    V(.23f,.39f),V(.27f,.42f),new Color(.68f,.59f,.45f,.70f));
            }
        }
        void GroundDust(Vector3 p, int count, float spread)
        {
            p.y = BattleRoute.Ground(p.x, p.z) + .08f;
            for (int i = 0; i < count; i++)
            {
                float angle = V(0, Mathf.PI * 2), speed = V(.25f, 1) * spread;
                Puff(dust, p, new Vector3(Mathf.Cos(angle) * speed, V(.2f, .7f) * spread, Mathf.Sin(angle) * speed), V(.2f, .42f) * (.6f + spread * .5f), V(.25f, .65f), new Color(.63f, .55f, .39f, .8f));
            }
        }
        public void DamageSmoke(Vector3 p) => Puff(smoke,p,Vector3.up*.5f,.55f,.7f,new Color(.25f,.24f,.2f,.6f));
        public void Destruction(Vector3 p,bool vehicle)
        {
            GroundDust(p,vehicle?12:4,vehicle?1:.4f);
            if(vehicle)
            {
                Puff(flash,p+Vector3.up*.6f,Vector3.up*.2f,2.3f,.16f,new Color(2,1,.3f));
                for(int i=0;i<12;i++)
                {
                    var velocity=new Vector3(V(-1.4f,1.4f),V(.7f,2.3f),V(-1.4f,1.4f));
                    Puff(fireball,p+Vector3.up*.55f,velocity,V(.65f,1.2f),V(.28f,.52f),new Color(1.5f,V(.32f,.65f),.08f));
                    Puff(smoke,p+Vector3.up*V(.5f,.9f),velocity*.6f,V(.8f,1.2f),V(.7f,1.1f),new Color(.2f,.21f,.18f,.85f));
                    Puff(fragments,p+Vector3.up*.6f,velocity*2,V(.035f,.075f),V(.4f,.8f),new Color(.27f,.3f,.2f));
                }
            }
        }
        public void Upgrade(Vector3 p)
        {
            for(int i=0;i<18;i++){float a=i*Mathf.PI*2/18;Puff(purchase,p+new Vector3(Mathf.Cos(a)*.7f,.1f,Mathf.Sin(a)*.7f),new Vector3(Mathf.Cos(a)*.3f,1.1f,Mathf.Sin(a)*.3f),.11f,.65f,new Color(1,.83f,.36f));}
        }
        public void TickPurchases(float dt)=>purchase.Simulate(dt,false,false);
        public void Escape(Vector3 p) => GroundDust(p, 4, .4f);
        public void Tick(float dt, IReadOnlyList<ShotState> shots)
        {
            foreach (var ps in emitters) ps.Simulate(dt, false, false);
            expired.Clear();
            foreach (var pair in flights)
            {
                var f = pair.Value; var shot = f.Shot;
                // The shot can be cleared by wave end or defeat without an impact.
                if (!Contains(shots, shot)) { expired.Add(pair.Key); continue; }
                float t = Mathf.Clamp01(shot.Age / shot.Definition.flightTime);
                var p = Vector3.Lerp(f.Origin, shot.Destination, t);
                if (f.Style == WeaponStyle.Mortar) p += Vector3.up * (4 * t * (1 - t) * 3);
                var d = p - f.Previous; f.View.transform.position = p;
                if (d.sqrMagnitude > .000001f) f.Body.rotation = Quaternion.LookRotation(d);
                f.Trail.SetPosition(0, p); f.Trail.SetPosition(1, p - d.normalized * Mathf.Min((p - f.Origin).magnitude, f.Style == WeaponStyle.MachineGun ? .70f : f.Style == WeaponStyle.Cannon ? .96f : .40f));
                f.Previous = p;
            }
            foreach (int id in expired) { pool.Release(flights[id]); flights.Remove(id); }
        }
        static bool Contains(IReadOnlyList<ShotState> shots, ShotState shot)
        { for (int i = 0; i < shots.Count; i++) if (shots[i] == shot) return true; return false; }
        Flight CreateFlight()
        {
            var go = new GameObject("Pooled projectile"); go.transform.SetParent(root, false);
            var body = GameObject.CreatePrimitive(PrimitiveType.Sphere); Destroy(body.GetComponent<Collider>()); body.transform.SetParent(go.transform, false);
            var renderer = body.GetComponent<Renderer>(); renderer.shadowCastingMode = ShadowCastingMode.Off;
            var trail = go.AddComponent<LineRenderer>(); trail.positionCount = 2; trail.useWorldSpace = true; trail.shadowCastingMode = ShadowCastingMode.Off; trail.receiveShadows = false;
            created++; return new Flight { View = go, Body = body.transform, BodyRenderer = renderer, Trail = trail };
        }
        public void Clear()
        {
            if (pool == null) return;
            foreach (var f in flights.Values) pool.Release(f); flights.Clear();
            foreach (var ps in emitters) ps.Clear();
        }
        void OnDestroy() { pool?.Clear(); if (root) Destroy(root.gameObject); }
    }
}
