using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;
namespace TheLineHolds
{
    // A fixed set of world-space emitters and a warmed projectile pool. No damage logic lives here.
    public sealed class CombatEffects : MonoBehaviour
    {
        public Material flashMaterial, smokeMaterial, dustMaterial, sparkMaterial, brassMaterial, shellMaterial;
        ParticleSystem flash, smoke, dust, sparks, brass;
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
            flash = Emitter("Muzzle flash", flashMaterial, 128, false);
            smoke = Emitter("Weapon smoke", smokeMaterial, 256, false);
            dust = Emitter("Road dust", dustMaterial, 384, false);
            sparks = Emitter("Armor sparks", sparkMaterial, 192, true);
            brass = Emitter("Ejected cartridges", brassMaterial, 128, true);
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
            int count = mg ? 2 : style == WeaponStyle.Cannon ? 7 : 5;
            for (int i = 0; i < count; i++)
                Puff(smoke, origin + forward * V(0, .14f), forward * V(.25f, .7f) + Vector3.up * V(.25f, .6f), V(.22f, mg ? .34f : .58f), V(.25f, .65f), new Color(.68f, .66f, .55f, .6f));
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
            flight.Trail.sharedMaterial = flashMaterial; flight.Trail.widthMultiplier = mg ? .035f : .045f;
            flight.Trail.enabled = style != WeaponStyle.Mortar;
            flight.Trail.SetPosition(0, origin); flight.Trail.SetPosition(1, origin);
            flights.Add(shot.Id, flight);
        }
        public void Impact(ShotState shot)
        {
            var style = WeaponStyles.For(shot.Definition); bool armor = !shot.Target.Resolved && shot.Target.Definition.armored;
            var p = shot.Destination;
            // Simulation hits the unit centre. Draw ricochets at the exposed hull edge.
            if (armor && style != WeaponStyle.Mortar)
                p += Vector3.ProjectOnPlane(shot.Origin - p, Vector3.up).normalized * .75f + Vector3.up * .25f;
            if (style == WeaponStyle.Mortar)
            {
                p.y = BattleRoute.Ground(p.x, p.z) + .05f;
                Puff(flash, p + Vector3.up * .12f, Vector3.up * .4f, .65f, .12f, new Color(1.3f, .74f, .26f));
                GroundDust(p, 18, 1.25f);
                for (int i = 0; i < 7; i++) Puff(smoke, p + Vector3.up * V(.2f, .4f), new Vector3(V(-.45f, .45f), V(.6f, 1.3f), V(-.45f, .45f)), V(.5f, .8f), V(.45f, .8f), new Color(.35f, .32f, .25f, .75f));
            }
            else if (armor)
            {
                int count = style == WeaponStyle.Cannon ? 10 : 4;
                for (int i = 0; i < count; i++) Puff(sparks, p, new Vector3(V(-1.5f, 1.5f), V(.5f, 2), V(-1.5f, 1.5f)), V(.055f, .12f), V(.13f, .3f), new Color(1.5f, 1.05f, .4f));
                if (style == WeaponStyle.Cannon) { Puff(flash, p, Vector3.up * .1f, .42f, .08f, new Color(1.4f, .9f, .36f)); GroundDust(p, 7, .7f); }
            }
            else GroundDust(p, style == WeaponStyle.Cannon ? 9 : 3, style == WeaponStyle.Cannon ? .7f : .27f);
            if (flights.Remove(shot.Id, out var flight)) pool.Release(flight);
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
            if(vehicle){Puff(flash,p+Vector3.up*.5f,Vector3.up*.2f,.9f,.14f,new Color(1,.7f,.3f));for(int i=0;i<10;i++)DamageSmoke(p+Vector3.up*V(.3f,.8f));}
        }
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
                f.Trail.SetPosition(0, p); f.Trail.SetPosition(1, p - d.normalized * Mathf.Min((p - f.Origin).magnitude, f.Style == WeaponStyle.MachineGun ? .5f : .7f));
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
