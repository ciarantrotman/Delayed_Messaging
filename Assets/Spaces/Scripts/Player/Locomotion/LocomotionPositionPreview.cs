using Spaces.Scripts.Utilities;
using UnityEngine;

namespace Spaces.Scripts.Player.Locomotion
{
    public class LocomotionPositionPreview : MonoBehaviour
    {
        [SerializeField] private Transform head;
        [SerializeField] private Transform lHand;
        [SerializeField] private Transform rHand;
        private bool active;

        private static ControllerTransforms Controller => Reference.Controller();

        private void Update()
        {
            if (!active) return;

            Transform thisTransform = transform;
            
            thisTransform.localPosition = Vector3.zero;
            thisTransform.forward = thisTransform.parent.forward;
            Transform headTransform = head.transform;
            headTransform.LocalTransforms(Controller.RelativeTransform(ControllerTransforms.Check.HEAD));
            Vector3 localRot = headTransform.localEulerAngles;
            headTransform.localEulerAngles = new Vector3(localRot.x, 0, localRot.z);
            lHand.transform.LocalTransforms(Controller.RelativeTransform(ControllerTransforms.Check.LEFT));
            rHand.transform.LocalTransforms(Controller.RelativeTransform(ControllerTransforms.Check.RIGHT));
        }

        public void GhostToggle(Transform parent, bool state)
        {
            Transform self = transform;
            self.SetParent(parent);
            self.localPosition = Vector3.zero;
            head.gameObject.SetActive(state);
            lHand.gameObject.SetActive(state);
            rHand.gameObject.SetActive(state);
            active = state;
        }
    }
}
