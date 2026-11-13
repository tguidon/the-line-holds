using System;
using System.Linq;
using UnityEngine;

namespace TheLineHolds
{
    // Mirrored, equal-length approaches keep distance-to-base and targeting priority identical
    // once either column reaches the shared street. Geometry is also used to bake the paving.
    public static class TownLayout
    {
        static readonly Vector2[] north = { new(-15,6), new(-10,6), new(-6,1.6f), new(-3.4f,0), new(-2.5f,0) };
        static readonly Vector2[] streets = { new(-2.5f,0), new(.1f,0), new(.1f,5.8f), new(9,5.8f),
            new(9,-5.5f), new(3.2f,-5.5f), new(3.2f,-1.1f), new(5.9f,-1.1f) };
        static readonly Vector2[] sites = { new(-12,3.3f), new(-11,-3.3f), new(-7.1f,6.9f),
            new(-4.2f,-3.65f), new(-2.5f,3), new(3,3.15f), new(6.35f,8.45f),
            new(11.7f,1.4f), new(-.65f,-2.8f), new(.6f,-5.2f) };
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
