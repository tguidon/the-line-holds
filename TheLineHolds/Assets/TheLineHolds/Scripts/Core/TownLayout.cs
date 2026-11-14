using System;
using System.Linq;
using UnityEngine;

namespace TheLineHolds
{
    // Mirrored, equal-length approaches keep distance-to-base and targeting priority identical
    // once either column reaches the shared street. Geometry is also used to bake the paving.
    public static class TownLayout
    {
        static readonly Vector2[] north = { new(-14,2.9f), new(-12.2f,2.9f), new(-8.6f,.18f), new(-7.7f,0) };
        // The south-facing staircase projects 1.60 m from the hall's lot center.
        public static readonly Vector2 CityHall = new(6.66f,6.39f);
        public static readonly Vector2 CityHallSteps = CityHall + Vector2.down*1.60f;
        static readonly Vector2[] streets = { new(-7.7f,0), new(-3.8f,0), new(-3.8f,5.5f),
            new(3,5.5f), new(3,-4.9f), new(10.3f,-4.9f), new(10.3f,1.95f),
            new(CityHall.x,3.70f), CityHallSteps };
        static readonly Vector2[] sites = { new(-13,0), new(-5.30f,-2.55f), new(-7.8f,-2.85f),
            new(-6.45f,3.25f), new(-1.15f,.35f), new(.35f,-3.65f), new(1.0f,8.15f),
            new(12.95f,-1.4f), new(5.75f,-1.9f), new(8.35f,-7.45f) };
        public static float Ground(float x,float z) => 0;
        static Vector3 Point(Vector2 p) => new(p.x,.035f,p.y);
        static Vector3[] Round(Vector2[] p) => BattleRoute.Rounded(p.Select(Point).ToArray(),(x,z)=>new Vector3(x,.035f,z));
        public static Vector3[] Pads => Array.ConvertAll(sites,Point);
        public static Vector3[] SharedStreet => Round(streets);
        public static Vector3[] Entrance(bool south) => Round(north.Select(p=>new Vector2(p.x,south?-p.y:p.y)).ToArray());
        public static BattleRoute Route(bool south)
        {
            var entry=Entrance(south);
            var approach=new BattleRoute(new[]{entry[0],entry[0]+Vector3.left*90},surfaceHeight:Ground);
            return new BattleRoute(entry.Concat(SharedStreet.Skip(1)).ToArray(),approach,Ground);
        }
    }
}
