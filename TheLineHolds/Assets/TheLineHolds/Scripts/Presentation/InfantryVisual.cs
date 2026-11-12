using System.Linq;
using UnityEngine;
namespace TheLineHolds
{
    // Drive a stride from simulation distance, so it freezes on pause and cannot change movement.
    public sealed class InfantryVisual : MonoBehaviour
    {
        [SerializeField] float swingDegrees=22;
        [SerializeField] float strideFrequency=10;
        public void SetGait(float swing,float frequency){swingDegrees=swing;strideFrequency=frequency;}
        Transform left,right,carry;
        Quaternion leftBind,rightBind,carryBind;
        void Awake()
        {
            var parts=GetComponentsInChildren<Transform>();
            left=parts.FirstOrDefault(t=>t.name.StartsWith("StrideLeft"));right=parts.FirstOrDefault(t=>t.name.StartsWith("StrideRight"));
            carry=parts.FirstOrDefault(t=>t.name.StartsWith("Carry"));if(carry)carryBind=carry.localRotation;
            if(left)leftBind=left.localRotation;if(right)rightBind=right.localRotation;
        }
        public void Pose(float distance,int id)
        {
            float stride=Mathf.Sin(distance*strideFrequency+id*.73f)*swingDegrees;
            if(carry)carry.localRotation=carryBind*Quaternion.Euler(stride*.14f,0,-stride*.07f);
            if(left)left.localRotation=leftBind*Quaternion.Euler(stride,0,0);
            if(right)right.localRotation=rightBind*Quaternion.Euler(-stride,0,0);
        }
    }
}
