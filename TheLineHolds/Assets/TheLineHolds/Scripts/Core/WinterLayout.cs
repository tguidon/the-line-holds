using System;
using System.Linq;
using UnityEngine;

namespace TheLineHolds
{
    public static class WinterLayout
    {
        static readonly Vector2[] corners = {
            new(-12,4), new(-8,5.9f), new(-2,6.4f), new(2,6), new(5.6f,4.2f),
            new(6,1.8f), new(3.7f,.1f), new(0,0), new(-4,.65f), new(-7,-.7f),
            new(-7.4f,-3.3f), new(-5,-5.6f), new(.8f,-5.8f), new(4,-5.1f),
            new(6.8f,-3.1f), new(11,-3.4f)
        };
        static readonly Vector2[] sites = {
            new(-10,2), new(-6.5f,8.5f), new(-9.9f,-4.4f), new(-4,3.4f),
            new(.5f,3), new(8.8f,3), new(-3.8f,-2.4f), new(.3f,-3),
            new(3,-8), new(8.8f,-.55f)
        };

        // Long outward approach continues behind the treeline for rotated and wide cameras.
        public static readonly Vector2[] Approach = { corners[0], new(-18,2), new(-27,3), new(-42,9), new(-90,16) };
        public static Vector3[] Pads => Array.ConvertAll(sites,p=>OnGround(p.x,p.y));
        public static float Ground(float x,float z) => .48f + .32f*Mathf.Sin(x*.18f+z*.11f)
            + .19f*Mathf.Cos(z*.31f) + .07f*Mathf.Sin(z*.65f+x*.42f);
        public static Vector3 OnGround(float x,float z) => new(x,Ground(x,z)+.035f,z);
        public static BattleRoute Route()
        {
            var path = BattleRoute.Rounded(corners.Select(p=>OnGround(p.x,p.y)).ToArray(),OnGround);
            var lane = new BattleRoute(Approach.Select(p=>OnGround(p.x,p.y)).ToArray(),surfaceHeight:Ground);
            return new BattleRoute(path,lane,Ground);
        }
    }
}
