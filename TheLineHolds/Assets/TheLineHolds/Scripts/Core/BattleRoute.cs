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
        readonly BattleRoute approach;
        public readonly Func<float, float, float> SurfaceHeight;

        // The western farm lane is ordered outwards from the gateway.
        public static readonly Vector2[] WesternLane = {
            new Vector2(-12,-5), new Vector2(-16,-5), new Vector2(-21,-9),
            new Vector2(-31,-15), new Vector2(-55,-17), new Vector2(-90,-18)
        };
        public static BattleRoute Countryside()
        {
            var battlefield=DefaultPoints;
            var lane=new Vector3[WesternLane.Length+1];
            lane[0]=battlefield[0];
            for(int i=0;i<WesternLane.Length;i++)lane[i+1]=OnGround(WesternLane[i].x,WesternLane[i].y);
            return new BattleRoute(battlefield,new BattleRoute(lane));
        }
        public BattleRoute(Vector3[] points, BattleRoute approach=null, Func<float,float,float> surfaceHeight=null)
        {
            if (points == null || points.Length < 2) throw new ArgumentException("Route needs two points.");
            this.approach=approach;
            SurfaceHeight=surfaceHeight ?? Ground;
            Points = (Vector3[])points.Clone(); cumulative = new float[points.Length];
            for (int i = 1; i < points.Length; i++) cumulative[i] = cumulative[i-1] + Vector3.Distance(points[i-1], points[i]);
            Length = cumulative[cumulative.Length-1];
        }
        public Vector3 Position(float distance)
        {
            if(distance<0 && approach!=null)
            {
                float outward=-distance;
                if(outward<=approach.Length)return approach.Position(outward);
                var lane=approach.Points;
                return lane[lane.Length-1]+(lane[lane.Length-1]-lane[lane.Length-2]).normalized*(outward-approach.Length);
            }
            for (int i = 1; i < Points.Length; i++)
                if (distance <= cumulative[i]) return Vector3.Lerp(Points[i-1], Points[i], Mathf.InverseLerp(cumulative[i-1], cumulative[i], distance));
            return Points[Points.Length-1];
        }
        public Vector3 Position(float distance, float lateralOffset)
        {
            var position = Position(distance);
            var direction = Position(Mathf.Min(Length, distance + .1f)) - Position(distance - .1f);
            position += Vector3.Cross(Vector3.up, direction).normalized * lateralOffset;
            position.y = SurfaceHeight(position.x, position.z) + .035f;
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
            OnGround(-11,-5), OnGround(-7,-5), OnGround(-4.9f,-3.6f), OnGround(-6,1.15f),
            OnGround(-8.3f,4.55f), OnGround(-4,5.65f), OnGround(.15f,4.65f), OnGround(1,1),
            OnGround(-1.4f,-2.35f), OnGround(3,-4.5f), OnGround(7.45f,-3.5f), OnGround(8.45f,0), OnGround(6.45f,3.05f), OnGround(8,5), OnGround(11,5)
        });
        internal static Vector3[] Rounded(Vector3[] corners, Func<float,float,Vector3> onGround=null)
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
                    points.Add((onGround ?? OnGround)(p.x,p.z));
                }
            }
            points.Add(corners[corners.Length-1]); return points.ToArray();
        }
        static readonly Vector2[] PadSites = {
            new Vector2(-8,-2.4f),new Vector2(-2.6f,-4.9f),new Vector2(-8.3f,0),new Vector2(-4,2.9f),new Vector2(-1.7f,1.6f),
            new Vector2(3.1f,3),new Vector2(2.6f,-1.2f),new Vector2(5,-1.4f),new Vector2(4.5f,5.15f),new Vector2(10.3f,1.7f)
        };
        public static float Ground(float x,float z)
        {
            float h=NaturalGround(x,z);
            foreach(var pad in PadSites)
            {
                float d=Vector2.Distance(pad,new Vector2(x,z));
                const float flatRadius=.72f, vergeRadius=2.2f;
                if(d>=vergeRadius)continue;
                float blend=1-Mathf.SmoothStep(0,1,Mathf.InverseLerp(flatRadius,vergeRadius,d));
                h=Mathf.Lerp(h,NaturalGround(pad.x,pad.y),blend);
            }
            return h;
        }
        public static Vector3[] DefaultPads => System.Array.ConvertAll(PadSites,p=>OnGround(p.x,p.y));
    }
}
