using System.Linq;
using UnityEngine;
namespace TheLineHolds.Editor
{
    // Decorative routes form a village network. Only the original battle road is navigable.
    public static class CountrysideLayout
    {
        public readonly struct Building
        {
            public readonly string Key;public readonly Vector2 Position;public readonly float Scale,Yaw,Radius;
            public Building(string key,float x,float z,float scale,float yaw,float radius){Key=key;Position=new Vector2(x,z);Scale=scale;Yaw=yaw;Radius=radius;}
        }
        public static readonly Building[] Buildings={
            new Building("Barn",-25,11,1,12,2.2f),new Building("StoneCottage",-18,12,.82f,-15,1.55f),new Building("ToolShed",-28,7,.8f,85,1.3f),new Building("VillageWell",-22,6,.8f,0,.9f),
            new Building("VillageInn",13,12,1,5,2.1f),new Building("StoneCottage",20,18,.9f,-35,1.7f),new Building("StoneCottage",9,19,.76f,20,1.5f),new Building("ToolShed",24,14,.85f,-70,1.4f),
            new Building("Dairy",-17,-14,1,-20,2.3f),new Building("StoneCottage",-26,-18,.88f,20,1.7f),new Building("ToolShed",-12,-17,.9f,30,1.45f),
            new Building("StoneCottage",17,-20,1,10,1.8f),new Building("ToolShed",25,-13,.95f,100,1.5f),new Building("VillageWell",21,-16,.9f,0,1),
            new Building("StoneCottage",-6,24,.8f,-40,1.6f),new Building("ToolShed",28,-23,1,-15,1.5f)
        };
        public static readonly Vector2[][] Roads={
            Path(-12,-5,-16,-5,-21,-9,-31,-15,-55,-17,-90,-18),
            Path(-16,-5,-18,3,-20,10,-16,15,-3,16,10,16,18,12,17,6,12,5),
            Path(12,5,16,5,21,1,23,-6,19,-14,5,-17,-10,-18,-21,-9),
            Path(-3,16,-4,30,3,60,10,90),Path(23,-6,40,-4,60,0,90,0),Path(5,-17,8,-32,4,-65,0,-90),
            Path(-20,10,-25,9),Path(-18,3,-22,6),Path(-16,15,-18,12),Path(-25,9,-28,7),
            Path(18,12,13,12),Path(18,12,20,18),Path(10,16,9,19),Path(18,12,24,14),
            Path(-21,-9,-17,-14),Path(-31,-15,-26,-18),Path(-10,-18,-12,-17),
            Path(19,-14,17,-20),Path(23,-6,25,-13),Path(19,-14,21,-16),Path(-4,30,-6,24),Path(17,-20,28,-23)
        };
        static Vector2[] Path(params float[] points)=>Enumerable.Range(0,points.Length/2).Select(i=>new Vector2(points[i*2],points[i*2+1])).ToArray();
        public static float RoadDistance(Vector2 p)
        {
            float best=float.MaxValue;
            foreach(var road in Roads)for(int i=1;i<road.Length;i++)
            {var a=road[i-1];var delta=road[i]-a;best=Mathf.Min(best,(p-a-delta*Mathf.Clamp01(Vector2.Dot(p-a,delta)/delta.sqrMagnitude)).magnitude);}
            return best;
        }
        public static bool InCore(Vector2 p,float margin=0)=>Mathf.Abs(p.x)<12+margin && Mathf.Abs(p.y)<7+margin;
        public static bool Reserved(Vector2 p)=>InCore(p,.05f) || RoadDistance(p)<1.05f || Buildings.Any(b=>Vector2.Distance(p,b.Position)<b.Radius+.35f);
        public static float YardWeight(Vector2 p)
        {
            float value=0;foreach(var b in Buildings){float d=Vector2.Distance(p,b.Position);value=Mathf.Max(value,1-Mathf.SmoothStep(0,1,Mathf.InverseLerp(b.Radius,b.Radius+1.3f,d)));}return value;
        }
        public static float[] RasterRoads(int width,float extent)
        {
            var distances=new float[width*width];System.Array.Fill(distances,100f);
            foreach(var road in Roads)for(int i=1;i<road.Length;i++)
            {
                var a=road[i-1];var b=road[i];var d=b-a;float span=extent*2;
                int x0=Mathf.Clamp(Mathf.FloorToInt((Mathf.Min(a.x,b.x)-1.5f+extent)/span*(width-1)),0,width-1),x1=Mathf.Clamp(Mathf.CeilToInt((Mathf.Max(a.x,b.x)+1.5f+extent)/span*(width-1)),0,width-1);
                int z0=Mathf.Clamp(Mathf.FloorToInt((Mathf.Min(a.y,b.y)-1.5f+extent)/span*(width-1)),0,width-1),z1=Mathf.Clamp(Mathf.CeilToInt((Mathf.Max(a.y,b.y)+1.5f+extent)/span*(width-1)),0,width-1);
                for(int z=z0;z<=z1;z++)for(int x=x0;x<=x1;x++)
                {var p=new Vector2(x*span/(width-1)-extent,z*span/(width-1)-extent);int pixel=z*width+x;distances[pixel]=Mathf.Min(distances[pixel],(p-a-d*Mathf.Clamp01(Vector2.Dot(p-a,d)/d.sqrMagnitude)).magnitude);}
            }
            return distances;
        }
    }
}
