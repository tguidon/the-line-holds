using System.Collections.Generic;
using UnityEngine;
namespace TheLineHolds
{
    // Models and their bars are reused across waves; defeated actors retire without affecting the simulation.
    public sealed class EnemyViewPool
    {
        sealed class View { public GameObject Root; public Transform Bar; public EnemyVisual Visual; public EnemyState State; public GameObject Prefab; }
        readonly Dictionary<int,View> active=new Dictionary<int,View>();
        readonly Dictionary<GameObject,Stack<View>> free=new Dictionary<GameObject,Stack<View>>();
        readonly List<View> retiring=new List<View>();readonly List<int> missing=new List<int>();readonly HashSet<int> present=new HashSet<int>();
        readonly Transform parent;readonly Camera camera;readonly Material barMaterial;readonly CombatEffects effects;
        public int Created {get;private set;}
        public int RetiringCount=>retiring.Count;
        public EnemyViewPool(Transform parent,Camera camera,Material barMaterial,CombatEffects effects){this.parent=parent;this.camera=camera;this.barMaterial=barMaterial;this.effects=effects;}
        View Get(EnemyState enemy)
        {
            if(active.TryGetValue(enemy.Id,out var v))return v;
            var prefab=enemy.Definition.prefab;
            if(!free.TryGetValue(prefab,out var pool)){pool=new Stack<View>();free[prefab]=pool;}
            if(pool.Count>0)v=pool.Pop();
            else
            {
                var root=Object.Instantiate(prefab,parent);var visual=root.GetComponent<EnemyVisual>();if(!visual)visual=root.AddComponent<EnemyVisual>();
                var bar=GameObject.CreatePrimitive(PrimitiveType.Cube);Object.Destroy(bar.GetComponent<Collider>());bar.name="Health";bar.transform.SetParent(root.transform,false);bar.transform.localPosition=Vector3.up*1.35f;bar.GetComponent<Renderer>().sharedMaterial=barMaterial;
                v=new View{Root=root,Visual=visual,Bar=bar.transform,Prefab=prefab};Created++;
            }
            v.State=enemy;v.Root.SetActive(true);v.Root.transform.position=enemy.Position;v.Visual.Initialize(enemy.Definition.spawnSeparation>0);v.Bar.gameObject.SetActive(true);active.Add(enemy.Id,v);return v;
        }
        public void Hit(EnemyState enemy){var v=Get(enemy);v.Visual.Hit(enemy.Health/enemy.Definition.health);}
        public void Kill(EnemyState enemy)
        {
            var v=Get(enemy);active.Remove(enemy.Id);v.Bar.gameObject.SetActive(false);v.Visual.Die();retiring.Add(v);
            effects.Destruction(enemy.Position,enemy.Definition.spawnSeparation>0);
            if(retiring.Count>24){Return(retiring[0]);retiring.RemoveAt(0);}
        }
        public void Sync(MatchState match,float dt)
        {
            present.Clear();
            foreach(var enemy in match.Enemies)
            {
                present.Add(enemy.Id);var v=Get(enemy);v.Root.transform.position=enemy.Position;
                var direction=match.Route.Position(enemy.Distance+.1f,enemy.LateralOffset)-enemy.Position;
                if(direction.sqrMagnitude>.00001f)v.Root.transform.rotation=Quaternion.LookRotation(direction);
                v.Visual.Tick(enemy,dt);
                if(dt>0 && v.Visual.SmokeDue){effects.DamageSmoke(enemy.Position+Vector3.up*.7f);v.Visual.SmokeEmitted();}
                v.Bar.rotation=camera.transform.rotation;v.Bar.localScale=new Vector3(.65f*Mathf.Clamp01(enemy.Health/enemy.Definition.health),.055f,.07f);
            }
            missing.Clear();foreach(var pair in active)if(!present.Contains(pair.Key)){Return(pair.Value);missing.Add(pair.Key);}foreach(int id in missing)active.Remove(id);
            for(int i=retiring.Count-1;i>=0;i--)if(retiring[i].Visual.Tick(retiring[i].State,dt)){Return(retiring[i]);retiring.RemoveAt(i);}
        }
        void Return(View v){v.Root.SetActive(false);free[v.Prefab].Push(v);}
        public void Clear(){foreach(var v in active.Values)Return(v);active.Clear();foreach(var v in retiring)Return(v);retiring.Clear();}
    }
}
