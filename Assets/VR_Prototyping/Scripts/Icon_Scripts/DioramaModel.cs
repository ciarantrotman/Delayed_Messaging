using System;
using Sirenix.OdinInspector;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace VR_Prototyping.Scripts.Icon_Scripts
{
    public class DioramaModel : MonoBehaviour
    {
        private Vector3 defaultLocalScale;
        private Vector3 scaleMax;
        private Vector3 scaleMin;

        private Manipulation manipulation;
        private ControllerTransforms controllerTransforms;
        private ObjectSelection objectSelection;

        private bool dualGrab;
        
        [BoxGroup("Diorama Scale Settings")] [Range(.01f, 1f)] public float minScaleFactor;
        [BoxGroup("Diorama Scale Settings")] [Range(1f, 10f)] public float maxScaleFactor;
        private void Awake()
        {
            foreach (GameObject rootGameObject in SceneManager.GetActiveScene().GetRootGameObjects())
            {
                if (rootGameObject.name != "[VR Player]") continue;
                manipulation = rootGameObject.GetComponent<Manipulation>();
                controllerTransforms = rootGameObject.GetComponent<ControllerTransforms>();
                objectSelection = rootGameObject.GetComponent<ObjectSelection>();
            }
            
            SetupScaling();
        }

        private void SetupScaling()
        {
            defaultLocalScale = transform.localScale;
            scaleMin = defaultLocalScale.ScaledScale(minScaleFactor);
            scaleMax = defaultLocalScale.ScaledScale(maxScaleFactor);
        }

        private void Update()
        {
            if (DualGrab() && !dualGrab)
            {
                manipulation.DualGrabStart(transform, false, true, maxScaleFactor, minScaleFactor, scaleMax, scaleMin);
                return;
            }

            if (DualGrab() && dualGrab)
            {
                Rigidbody rb = new Rigidbody();
                manipulation.DualGrabStay(rb, transform, false, true, scaleMin, scaleMax);
                return;
            }

            if (!DualGrab() && dualGrab)
            {
                defaultLocalScale = transform.localScale;
                manipulation.DualGrabEnd();
                return;
            }
        }

        private void LateUpdate()
        {
            dualGrab = DualGrab();
        }

        private bool DualGrab()
        {
            return controllerTransforms.LeftGrab() && controllerTransforms.RightGrab();
        }
    }
}
