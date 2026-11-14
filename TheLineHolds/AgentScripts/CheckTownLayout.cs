// Structural checks only: no campaign playthrough or balance tuning.
using System;
using System.Linq;
using TheLineHolds;
using UnityEngine;
using UnityEditor;

public static class CheckTownLayout
{
    public static string All()
    {
        void Check(bool ok,string reason){if(!ok)throw new Exception(reason);}
        var a=TownLayout.Route(false);var b=TownLayout.Route(true);
        float merge=new BattleRoute(TownLayout.Entrance(false)).Length;
        Check(merge/a.Length>=.10f && merge/a.Length<=.15f,"Merge fraction: "+merge/a.Length);
        Check(Mathf.Abs(a.Length-b.Length)<.001f,"Unequal approach lengths");
        for(float d=merge;d<a.Length;d+=.1f)Check((a.Position(d)-b.Position(d)).magnitude<.001f,"Disconnected shared street");
        var sites=TownLayout.Pads;
        var mg=AssetDatabase.LoadAssetAtPath<DefenseDefinition>("Assets/TheLineHolds/Data/Defenses/MachineGun.asset");
        float min=float.MaxValue;
        foreach(var site in sites)
        {
            float nearest=float.MaxValue,coverage=0;
            foreach(var r in new[]{a,b})
            {
                float length=0;
                for(float d=0;d<r.Length;d+=.05f)
                {
                    float gap=Vector3.Distance(site,r.Position(d));
                    nearest=Mathf.Min(gap,nearest);if(gap<=mg.range)length+=.05f;
                }
                coverage=Mathf.Max(coverage,length);
            }
            Check(nearest>=2.47f && coverage>=5f,$"Site {site}: clearance {nearest:F2}, coverage {coverage:F1}");
            min=Mathf.Min(min,nearest);
        }
        var game=UnityEngine.Object.FindAnyObjectByType<BattleController>();
        if(game && game.saintAubin)
        {
            var quarter=GameObject.Find("Saint-Aubin • market quarter");
            Check(quarter!=null,"Missing authored street blocks");
            var streetPoints=new System.Collections.Generic.List<Vector3>();
            foreach(var route in new[]{a,b})
                for(float d=-16;d<=route.Length+.08f;d+=.08f)streetPoints.Add(route.Position(d));
            foreach(Transform lot in quarter.transform)
            {
                if(!lot.name.Contains(" · ") && lot.name!="Mairie" && lot.name!="ParishChurch")continue;
                var envelope=lot.GetComponentInChildren<MeshRenderer>().bounds;
                float Gap(Vector3 p){p.y=envelope.center.y;return Vector3.Distance(envelope.ClosestPoint(p),p);}
                if(lot.name=="Mairie")
                {
                    var end=a.Position(a.Length);
                    Check(Mathf.Abs(end.x-lot.position.x)<.02f && Mathf.Abs(end.z-envelope.min.z)<.06f,
                        "Route misses the central City Hall steps");
                    Check(Vector3.Dot((end-a.Position(a.Length-.5f)).normalized,Vector3.forward)>.99f,
                        "Final approach must face the hall entrance");
                    Check(sites.Min(Gap)>=1.17f,"City Hall intrudes on a build site");
                    foreach(var point in streetPoints)
                        if(point.z<end.z-1.45f || Mathf.Abs(point.x-end.x)>1.45f)
                            Check(Gap(point)>=1.41f,"City Hall obstructs the street before its steps");
                    continue;
                }
                Check(streetPoints.Min(Gap)>=1.41f && sites.Min(Gap)>=1.17f,"Building intrudes on road or site: "+lot.name);
            }
        }
        var wave=AssetDatabase.LoadAssetAtPath<WaveDefinition>("Assets/TheLineHolds/Data/Waves/Wave1.asset");
        for(int pattern=0;pattern<3;pattern++)
        {
            var match=new MatchState(new[]{wave},a,sites,42,b,merge);
            var spawner=new WaveSpawner();spawner.Begin(wave,42,_=>-2,pattern);spawner.Step(match,500);
            Check(match.Enemies.Count==wave.UnitCount && spawner.Complete,"Wave headcount changed");
            var entries=match.Enemies.Select(e=>e.Entrance).Distinct().OrderBy(i=>i).ToArray();
            Check(entries.SequenceEqual(pattern==2?new[]{0,1}:new[]{pattern}),"Incorrect entrance allocation");
            foreach(var e in match.Enemies)
                Check(Vector3.Distance(e.Position,match.RouteFor(e.Entrance).Position(-2,e.LateralOffset))<.001f,"Spawn off its lane");
        }
        return $"Both routes {a.Length:F2} m; junction at {merge:F2} m ({merge/a.Length:P1}); {sites.Length} sites, minimum centerline clearance {min:F2} m. Three entrance patterns preserve the roster.";
    }
}
