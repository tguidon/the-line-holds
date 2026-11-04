using UnityEngine;
namespace TheLineHolds
{
    // A font-independent rating icon; Liberation Sans does not contain star glyphs.
    [RequireComponent(typeof(CanvasRenderer))]
    public sealed class StarGraphic : UnityEngine.UI.MaskableGraphic
    {
        protected override void OnPopulateMesh(UnityEngine.UI.VertexHelper mesh)
        {
            mesh.Clear(); var rect=rectTransform.rect; var center=rect.center;
            float radius=Mathf.Min(rect.width,rect.height)*.48f;
            mesh.AddVert(center,color,Vector2.zero);
            for(int i=0;i<10;i++)
            {
                float angle=(90+i*36)*Mathf.Deg2Rad;
                float length=radius*(i%2==0?1:.43f);
                mesh.AddVert(center+new Vector2(Mathf.Cos(angle),Mathf.Sin(angle))*length,color,Vector2.zero);
            }
            for(int i=0;i<10;i++)mesh.AddTriangle(0,i+1,(i+1)%10+1);
        }
    }
}
