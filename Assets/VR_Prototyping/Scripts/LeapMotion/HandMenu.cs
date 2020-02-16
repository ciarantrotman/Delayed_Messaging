using System;
using Sirenix.OdinInspector;
using UnityEngine;

namespace VR_Prototyping.Scripts.LeapMotion
{
    [RequireComponent(typeof(ControllerTransforms))]
    public class HandMenu : MonoBehaviour
    {
        private ControllerTransforms controllerTransforms;
        private LineRenderer proximityLine;

        private bool fingerTrigger;

        private Transform target;
        
        [BoxGroup("Menu Settings"), Range(.01f, .1f)] private float selectionTriggerDistance;
        
        private void Awake()
        {
            SetupReferences();
            SetupLineRenderer();
        }

        private void SetupReferences()
        {
            controllerTransforms = transform.GetComponent<ControllerTransforms>();
        }
        private void SetupLineRenderer()
        {
            target = controllerTransforms.leftThumb;
            proximityLine = controllerTransforms.leftThumb.AddOrGetLineRenderer();
            proximityLine.SetupLineRender(controllerTransforms.dottedLineRenderMat, .001f, true);
            proximityLine.positionCount = 2;
        }

        private void Update()
        {
            if (controllerTransforms.LeftIndexSelect())
            {
                target = controllerTransforms.leftIndex;
                return;
            }
            if (controllerTransforms.LeftMiddleSelect())
            {
                target = controllerTransforms.leftMiddle;
                return;
            }
            if (controllerTransforms.LeftRingSelect())
            {
                target = controllerTransforms.leftRing;
                return;
            }
            if (controllerTransforms.LeftLittleSelect())
            {
                target = controllerTransforms.leftLittle;
                return;
            }
        }

        void LateUpdate()
        {
            proximityLine.StraightLineRender(controllerTransforms.leftThumb, target);
        }
    }
}
