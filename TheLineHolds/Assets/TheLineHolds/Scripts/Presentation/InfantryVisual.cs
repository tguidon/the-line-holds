using System.Linq;
using UnityEngine;
namespace TheLineHolds
{
    // Drive a stride from simulation distance, so it freezes on pause and cannot change movement.
    public sealed class InfantryVisual : MonoBehaviour
    {
        Transform left,right;
        Quaternion leftBind,rightBind;
        void Awake()
        {
            var parts=GetComponentsInChildren<Transform>();
            left=parts.FirstOrDefault(t=>t.name.StartsWith("StrideLeft"));right=parts.FirstOrDefault(t=>t.name.StartsWith("StrideRight"));
            if(left)leftBind=left.localRotation;if(right)rightBind=right.localRotation;
        }
        public void Pose(float distance,int id)
        {
            float stride=Mathf.Sin(distance*10+id*.73f)*22;
            if(left)left.localRotation=leftBind*Quaternion.Euler(stride,0,0);
            if(right)right.localRotation=rightBind*Quaternion.Euler(-stride,0,0);
        }
    }
}
