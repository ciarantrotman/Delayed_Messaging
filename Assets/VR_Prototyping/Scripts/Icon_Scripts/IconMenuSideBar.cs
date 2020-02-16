using Sirenix.OdinInspector;
using UnityEngine;

namespace VR_Prototyping.Scripts.Icon_Scripts
{
    public class IconMenuSideBar : SelectableObject
    {
        private float blendShapeWeight;
        private const float DirectDistance = .05f;

        [BoxGroup("Side Menu Settings")] [SerializeField] private SkinnedMeshRenderer grabTarget;
        [BoxGroup("Side Menu Settings")] [SerializeField] [Range(.05f, .25f)] private float triggerDistance;

        protected override void ObjectUpdate()
        {
            SetBlendShape();
            
            transform.LookAtVertical(controllerTransforms.CameraTransform());
        }
        private void SetBlendShape()
        {
            grabTarget.SetBlendShapeWeight(0, BlendShapeWeight());
        }
        
        private float BlendShapeWeight()
        {
            Vector3 position = transform.position;
            
            float rDistance = Vector3.Distance(position, controllerTransforms.RightPosition());
            float lDistance = Vector3.Distance(position, controllerTransforms.LeftPosition());

            if (rDistance < lDistance)
            {
                return rDistance < DirectDistance ? 0f : (Mathf.InverseLerp(DirectDistance, triggerDistance, rDistance)) * 100f;
            }
            return lDistance < DirectDistance ? 0f : (Mathf.InverseLerp(DirectDistance, triggerDistance, lDistance)) * 100f;
        }
    }
}
