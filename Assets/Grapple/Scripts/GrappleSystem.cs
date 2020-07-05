using System;
using Delayed_Messaging.Scripts.Player;
using Delayed_Messaging.Scripts.Utilities;
using UnityEngine;
using BallisticData =  Grapple.Scripts.BallisticTrajectory.BallisticTrajectoryData;
using BallisticVariables =  Grapple.Scripts.BallisticTrajectory.BallisticVariables;

namespace Grapple.Scripts
{
    [RequireComponent(typeof(ControllerTransforms), typeof(LaunchAnchor))]
    public class GrappleSystem : MonoBehaviour
    {
        [Header("Grapple System Configuration")]
        [Range(.1f, 50f)] public float launchSpeed = 1f;
        public Material grappleVisualMaterial;

        private ControllerTransforms controller;
        private LaunchAnchor launchAnchor;
        private LineRenderer grappleVisual;
        
        [HideInInspector] public BallisticData ballisticData;
        [HideInInspector] public BallisticVariables ballisticVariables;
        [HideInInspector] public BallisticReferenceFrame ballisticReferenceFrame;

        [Serializable] public class BallisticReferenceFrame
        {
            public GameObject referenceFrame;
            public GameObject start;
            public GameObject middle;
            public GameObject end;

            public void SetupReferenceFrame()
            {
                referenceFrame = new GameObject("[Ballistic Reference Frame]");
                start = new GameObject("[Ballistic Reference Frame / Start]");
                middle = new GameObject("[Ballistic Reference Frame / Middle]");
                end = new GameObject("[Ballistic Reference Frame / End]");
            
                start.transform.SetParent(referenceFrame.transform);
                middle.transform.SetParent(referenceFrame.transform);
                end.transform.SetParent(referenceFrame.transform);
            }

            public void SetBallisticData(BallisticData data, BallisticVariables variables)
            {
                referenceFrame.transform.position = new Vector3(variables.handPosition.x, 0, variables.handPosition.z);
                referenceFrame.transform.forward = data.thetaVector;
                
                start.transform.localPosition = new Vector3(0, data.initialHeight, 0);
                middle.transform.localPosition = new Vector3(0, data.height, Mathf.Lerp(0, data.range, .5f));
                end.transform.localPosition = new Vector3(0, 0, data.range);

                data.start = start.transform.position;
                data.middle = middle.transform.position;
                data.end = end.transform.position;
            }
        }
        
        private void Start()
        {
            controller = GetComponent<ControllerTransforms>();
            launchAnchor = GetComponent<LaunchAnchor>();

            grappleVisual = gameObject.AddComponent<LineRenderer>();
            grappleVisual.SetupLineRender(grappleVisualMaterial, .01f, true);
            
            ballisticReferenceFrame.SetupReferenceFrame();
        }

        private void Update()
        {
            ballisticVariables.SetBallisticVariables(controller.RightPosition(), launchAnchor.rightAnchor.transform.position, launchSpeed);
            ballisticData.Calculate(ballisticVariables);
            ballisticReferenceFrame.SetBallisticData(ballisticData, ballisticVariables);
            grappleVisual.BezierLineRenderer(ballisticData.start, ballisticData.middle, ballisticData.end);
        }
    }
}
