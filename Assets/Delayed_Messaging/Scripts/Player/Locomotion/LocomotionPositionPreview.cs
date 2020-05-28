using System;
using Delayed_Messaging.Scripts.Player;
using Delayed_Messaging.Scripts.Utilities;
//using Sirenix.OdinInspector;
using UnityEngine;

namespace VR_Prototyping.Scripts
{
    public class LocomotionPositionPreview : MonoBehaviour
    {
        [SerializeField] private Transform head;
        [SerializeField] private Transform lHand;
        [SerializeField] private Transform rHand;
        private bool active;
        
        public ControllerTransforms ControllerTransforms { private get; set; }

        private void Update()
        {
            if (!active) return;

            Transform thisTransform = transform;
            
            thisTransform.localPosition = Vector3.zero;
            thisTransform.forward = thisTransform.parent.forward;
            Transform headTransform = head.transform;
            headTransform.LocalTransforms(ControllerTransforms.HmdLocalRelativeTransform());
            Vector3 localRot = headTransform.localEulerAngles;
            headTransform.localEulerAngles = new Vector3(localRot.x, 0, localRot.z);
            lHand.transform.LocalTransforms(ControllerTransforms.LeftLocalRelativeTransform());
            rHand.transform.LocalTransforms(ControllerTransforms.RightLocalRelativeTransform());
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
