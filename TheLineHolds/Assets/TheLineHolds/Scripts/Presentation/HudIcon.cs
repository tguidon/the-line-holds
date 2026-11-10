using UnityEngine;
namespace TheLineHolds
{
    public enum HudSymbol { Heart, Supply, Wave, Infantry, Vehicle, Gun, Cannon, Mortar, Play, Pause, Close, Help, Sound, Reset, Left, Right, Plus, Minus, Upgrade, Menu, Lock }
    // Small, consistent line icons; no font glyph dependencies or raster atlases.
    [RequireComponent(typeof(CanvasRenderer))]
    public sealed class HudIcon : UnityEngine.UI.MaskableGraphic
    {
        public HudSymbol symbol;
        protected override void OnPopulateMesh(UnityEngine.UI.VertexHelper mesh)
        {
            mesh.Clear();var r=GetPixelAdjustedRect();float scale=Mathf.Min(r.width,r.height);
            void Line(Vector2 a,Vector2 b)
            {
                a=r.center+(a-Vector2.one*.5f)*scale;b=r.center+(b-Vector2.one*.5f)*scale;
                var n=Vector2.Perpendicular(b-a).normalized*scale*.043f;int i=mesh.currentVertCount;
                mesh.AddVert(a-n,color,Vector2.zero);mesh.AddVert(a+n,color,Vector2.zero);mesh.AddVert(b+n,color,Vector2.zero);mesh.AddVert(b-n,color,Vector2.zero);mesh.AddTriangle(i,i+1,i+2);mesh.AddTriangle(i,i+2,i+3);
            }
            void Path(params float[] xy){for(int i=2;i<xy.Length;i+=2)Line(new Vector2(xy[i-2],xy[i-1]),new Vector2(xy[i],xy[i+1]));}
            void Box(float x,float y,float w,float h)=>Path(x,y,x+w,y,x+w,y+h,x,y+h,x,y);
            switch(symbol)
            {
                case HudSymbol.Menu:Path(.2f,.25f,.8f,.25f);Path(.2f,.5f,.8f,.5f);Path(.2f,.75f,.8f,.75f);break;
                case HudSymbol.Lock:Box(.2f,.15f,.6f,.45f);Path(.32f,.6f,.32f,.8f,.68f,.8f,.68f,.6f);break;
                case HudSymbol.Heart:Path(.5f,.15f,.13f,.53f,.13f,.73f,.28f,.84f,.5f,.67f,.72f,.84f,.87f,.73f,.87f,.53f,.5f,.15f);break;
                case HudSymbol.Supply:Box(.18f,.2f,.64f,.55f);Path(.18f,.58f,.82f,.58f);Path(.38f,.76f,.38f,.88f,.62f,.88f,.62f,.76f);break;
                case HudSymbol.Wave:Path(.28f,.12f,.28f,.87f,.83f,.78f,.66f,.59f,.28f,.62f);break;
                case HudSymbol.Infantry:Box(.35f,.63f,.3f,.22f);Path(.2f,.18f,.3f,.5f,.7f,.5f,.8f,.18f);break;
                case HudSymbol.Vehicle:Box(.12f,.27f,.76f,.35f);Box(.28f,.62f,.34f,.2f);Path(.6f,.73f,.94f,.73f);Path(.22f,.18f,.22f,.28f);Path(.74f,.18f,.74f,.28f);break;
                case HudSymbol.Gun:Path(.15f,.6f,.8f,.6f,.9f,.67f);Box(.2f,.5f,.25f,.2f);Path(.4f,.48f,.25f,.15f);Path(.4f,.48f,.65f,.15f);break;
                case HudSymbol.Cannon:Path(.24f,.47f,.85f,.78f);Box(.3f,.27f,.1f,.4f);Box(.17f,.16f,.18f,.22f);Box(.58f,.16f,.18f,.22f);break;
                case HudSymbol.Mortar:Path(.33f,.32f,.65f,.88f,.8f,.8f,.49f,.25f);Path(.2f,.15f,.77f,.15f);Path(.48f,.49f,.7f,.16f);break;
                case HudSymbol.Play:Path(.3f,.15f,.82f,.5f,.3f,.85f,.3f,.15f);break;
                case HudSymbol.Pause:Path(.34f,.18f,.34f,.82f);Path(.66f,.18f,.66f,.82f);break;
                case HudSymbol.Close:Path(.25f,.25f,.75f,.75f);Path(.25f,.75f,.75f,.25f);break;
                case HudSymbol.Help:Path(.28f,.75f,.4f,.87f,.67f,.87f,.77f,.68f,.52f,.46f,.52f,.35f);Path(.52f,.19f,.52f,.13f);break;
                case HudSymbol.Sound:Path(.15f,.38f,.32f,.38f,.56f,.18f,.56f,.82f,.32f,.62f,.15f,.62f,.15f,.38f);Path(.71f,.3f,.83f,.5f,.71f,.7f);break;
                case HudSymbol.Reset:Path(.24f,.65f,.38f,.83f,.72f,.8f,.86f,.52f,.74f,.22f,.42f,.16f,.2f,.32f);Path(.24f,.88f,.24f,.65f,.47f,.65f);break;
                case HudSymbol.Left:Path(.68f,.2f,.32f,.5f,.68f,.8f);break;
                case HudSymbol.Right:Path(.32f,.2f,.68f,.5f,.32f,.8f);break;
                case HudSymbol.Plus:Path(.2f,.5f,.8f,.5f);Path(.5f,.2f,.5f,.8f);break;
                case HudSymbol.Minus:Path(.2f,.5f,.8f,.5f);break;
                case HudSymbol.Upgrade:Path(.2f,.45f,.5f,.75f,.8f,.45f);Path(.2f,.22f,.5f,.52f,.8f,.22f);break;
            }
        }
    }
}
