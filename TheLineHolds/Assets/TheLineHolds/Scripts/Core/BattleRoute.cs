using System;
using UnityEngine;
namespace TheLineHolds
{
    // Distance along the authored route is also the targeting priority. Height has no combat bonus.
    public sealed class BattleRoute
    {
        public readonly Vector3[] Points;
        public readonly float Length;
        readonly float[] cumulative;
        public BattleRoute(Vector3[] points)
        {
            if (points == null || points.Length < 2) throw new ArgumentException("Route needs two points.");
            Points = (Vector3[])points.Clone(); cumulative = new float[points.Length];
            for (int i = 1; i < points.Length; i++) cumulative[i] = cumulative[i-1] + Vector3.Distance(points[i-1], points[i]);
            Length = cumulative[cumulative.Length-1];
        }
        public Vector3 Position(float distance)
        {
            for (int i = 1; i < Points.Length; i++)
                if (distance <= cumulative[i]) return Vector3.Lerp(Points[i-1], Points[i], Mathf.InverseLerp(cumulative[i-1], cumulative[i], distance));
            return Points[Points.Length-1];
        }
        public Vector3 Position(float distance, float lateralOffset)
        {
            var position = Position(distance);
            var direction = Position(Mathf.Min(Length, distance + .1f)) - Position(Mathf.Max(0, distance - .1f));
            position += Vector3.Cross(Vector3.up, direction).normalized * lateralOffset;
            position.y = Ground(position.x, position.z) + .035f;
            return position;
        }
        public static float Ground(float x) => Ground(x, 0);
        // Continuous derivatives remove the old diagonal terrace seams. Shared by art and movement.
        static float NaturalGround(float x, float z) => .25f
            + .7f * Mathf.SmoothStep(0, 1, Mathf.InverseLerp(-5, -1, x))
            + .7f * Mathf.SmoothStep(0, 1, Mathf.InverseLerp(3, 7, x))
            + .09f * Mathf.Sin(x * .47f + z * .38f) * Mathf.Sin(z * .55f);
        public static Vector3 OnGround(float x, float z) => new Vector3(x, Ground(x,z) + .035f, z);
        public static Vector3[] DefaultPoints => Rounded(new[] {
            OnGround(-11,-5), OnGround(-7,-5), OnGround(-5,-3), OnGround(-6,1),
            OnGround(-8,4), OnGround(-4,5), OnGround(0,4), OnGround(1,1),
            OnGround(-1,-2), OnGround(3,-4), OnGround(7,-3), OnGround(8,0), OnGround(6,3), OnGround(8,5), OnGround(11,5)
        });
        static Vector3[] Rounded(Vector3[] corners)
        {
            var points = new System.Collections.Generic.List<Vector3> { corners[0] };
            for (int i=1; i<corners.Length-1; i++)
            {
                var a=corners[i]+(corners[i-1]-corners[i]).normalized*.7f;
                var b=corners[i]+(corners[i+1]-corners[i]).normalized*.7f;
                for(int j=0;j<=8;j++)
                {
                    float t=j/8f;
                    var p=(1-t)*(1-t)*a+2*(1-t)*t*corners[i]+t*t*b;
                    points.Add(OnGround(p.x,p.z));
                }
            }
            points.Add(corners[corners.Length-1]); return points.ToArray();
        }
        static readonly Vector2[] PadSites = {
            new Vector2(-8,-3),new Vector2(-3,-4),new Vector2(-8,1),new Vector2(-5,3),new Vector2(-2,2),
            new Vector2(2,3),new Vector2(2,-1),new Vector2(5,-1.5f),new Vector2(5,4.5f),new Vector2(10,2.5f)
        };
        public static float Ground(float x,float z)
        {
            float h=NaturalGround(x,z);
            foreach(var pad in PadSites)
            {
                float d=Vector2.Distance(pad,new Vector2(x,z));
                if(d>=1.65f)continue;
                float blend=1-Mathf.SmoothStep(0,1,Mathf.InverseLerp(.76f,1.65f,d));
                h=Mathf.Lerp(h,NaturalGround(pad.x,pad.y),blend);
            }
            return h;
        }
        public static Vector3[] DefaultPads => System.Array.ConvertAll(PadSites,p=>OnGround(p.x,p.y));
    }
}
