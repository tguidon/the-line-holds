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
        public static float Ground(float x) => .25f + .7f * Mathf.InverseLerp(-4, -2, x) + .7f * Mathf.InverseLerp(4, 6, x);
        public static Vector3 OnGround(float x, float z) => new Vector3(x, Ground(x) + .035f, z);
        public static Vector3[] DefaultPoints => new[] {
            OnGround(-11,-5), OnGround(-7,-5), OnGround(-5,-3), OnGround(-6,1),
            OnGround(-8,4), OnGround(-4,5), OnGround(0,4), OnGround(1,1),
            OnGround(-1,-2), OnGround(3,-4), OnGround(7,-3), OnGround(8,0), OnGround(6,3), OnGround(8,5), OnGround(11,5)
        };
        public static Vector3[] DefaultPads => new[] {
            OnGround(-8,-3), OnGround(-3,-4), OnGround(-8,1), OnGround(-5,3), OnGround(-2,2),
            OnGround(2,3), OnGround(2,-1), OnGround(5,-1.5f), OnGround(5,4.5f), OnGround(10,2.5f)
        };
    }
}
