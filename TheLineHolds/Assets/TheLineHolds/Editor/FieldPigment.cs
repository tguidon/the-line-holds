using UnityEngine;

namespace TheLineHolds.Editor
{
    // Colours are baked in world metres, so the lower-resolution outlying pasture meets the road.
    static class FieldPigment
    {
        static readonly Color Hollow = DioramaSurface.Hex("526849"), Meadow = DioramaSurface.Hex("839461"),
            Hay = DioramaSurface.Hex("AFAB73"), Clay = DioramaSurface.Hex("957958"),
            Chalk = DioramaSurface.Hex("C2B093"), Track = DioramaSurface.Hex("695942"),
            Grit = DioramaSurface.Hex("CEBEA1"), Loam = DioramaSurface.Hex("806E50"),
            Boards = DioramaSurface.Hex("988468"), Joint = DioramaSurface.Hex("625848");

        public static float Ramp(float low, float high, float value) =>
            Mathf.SmoothStep(0, 1, Mathf.InverseLerp(low, high, value));

        public static Color Grass(float x, float z)
        {
            float growth = Mathf.PerlinNoise(x*.31f+43, z*.31f+17);
            float rough = Mathf.PerlinNoise(x*1.8f+23, z*1.8f+8);
            var baseColor = Color.Lerp(Hollow, Meadow, Ramp(.18f,.80f,growth*.78f+rough*.22f));
            float dry = Mathf.PerlinNoise(x*.74f+15, z*.61f+32);
            return Color.Lerp(baseColor, Hay, .55f*Ramp(.50f,.86f,dry));
        }

        public static Color Road(float x, float z, float distance, float along)
        {
            float patches = Mathf.PerlinNoise(x*1.3f+37,z*1.3f+23);
            float fine = Mathf.PerlinNoise(x*13+19,z*13+13);
            float verge = Ramp(.59f,1.05f,distance);
            float wheel = distance - .42f - .085f*(patches-.5f);
            float rut = Mathf.Exp(-wheel*wheel/.007f);
            float lip = Mathf.Exp(-Mathf.Pow((wheel-.108f)/.042f,2));
            var road = Color.Lerp(Clay, Chalk, .41f+patches*.53f-verge*.37f);
            float tread = Ramp(.68f,.97f,Mathf.Sin(along*44+wheel*16));
            road = Color.Lerp(road,Track,rut*(.36f+patches*.30f+tread*.18f));
            road = Color.Lerp(road,Chalk,lip*.35f);
            float gravel = Ramp(.64f,.84f,fine)*(.23f+verge*.74f);
            return Color.Lerp(road,Grit,gravel)*(.96f+.08f*fine);
        }

        public static Color GunSite(float x, float z, Vector2 offset, float grain)
        {
            // Two interrupted runs of timber edging and a tamped, drained firing floor.
            float radius = offset.magnitude;
            var soil = Color.Lerp(Loam, Chalk, .22f+grain*.29f);
            float theta = Mathf.Atan2(offset.y,offset.x);
            float arc = Mathf.Abs(Mathf.Sin(theta*2));
            float curb = (1-Ramp(.018f,.048f,Mathf.Abs(radius-.79f)))*Ramp(.16f,.32f,arc);
            float plank = Mathf.PerlinNoise(x*9+4,z*3+17);
            return Color.Lerp(soil, Color.Lerp(Joint,Boards,plank+.08f),curb);
        }

        public static Color GateSetts(float x, float z, Color earth)
        {
            float gate = 1-Ramp(.34f,.76f,Mathf.Abs(x+11.13f));
            if (gate <= 0) return earth;
            float row = z/.19f;
            float column = x/.34f + (Mathf.FloorToInt(row)&1)*.5f;
            float seam = Mathf.Min(Mathf.Abs(Mathf.Repeat(row,1)-.5f)*.19f,
                                  Mathf.Abs(Mathf.Repeat(column,1)-.5f)*.34f);
            float stone = Mathf.PerlinNoise(Mathf.Floor(column+.5f)*.73f+44, Mathf.Floor(row+.5f)*.73f+57);
            var paving = Color.Lerp(Joint,Color.Lerp(Boards,Chalk,stone),Ramp(.003f,.026f,seam));
            return Color.Lerp(earth,paving,gate*(.67f+stone*.24f));
        }
    }
}
