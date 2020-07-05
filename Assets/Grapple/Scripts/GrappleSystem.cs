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
        [SerializeField] private GameObject hook;
        public Material grappleVisualMaterial;

        private ControllerTransforms controller;
        private LaunchAnchor launchAnchor;


        
        [HideInInspector] public BallisticReferenceFrame rightReferenceFrame;
        [HideInInspector] public BallisticReferenceFrame leftReferenceFrame;
        
        [HideInInspector] public GrappleLaunch leftGrapple;
        [HideInInspector] public GrappleLaunch rightGrapple;

        [Serializable] public class BallisticReferenceFrame
        {
            public LineRenderer grappleVisual;
            public GameObject referenceFrame;
            public GameObject start;
            public GameObject middle;
            public GameObject end;
            
            [HideInInspector] public BallisticData ballisticData;
            [HideInInspector] public BallisticVariables ballisticVariables;

            public void SetupReferenceFrame(string side, Material mat)
            {
                referenceFrame = new GameObject($"[Ballistic Reference Frame / {side}]");
                start = new GameObject("[Ballistic Reference Frame / Start]");
                middle = new GameObject("[Ballistic Reference Frame / Middle]");
                end = new GameObject("[Ballistic Reference Frame / End]");
            
                start.transform.SetParent(referenceFrame.transform);
                middle.transform.SetParent(referenceFrame.transform);
                end.transform.SetParent(referenceFrame.transform);
                
                grappleVisual = start.AddComponent<LineRenderer>();
                grappleVisual.SetupLineRender(mat, .005f, true);
            }

            public void SetBallisticData(BallisticData data, BallisticVariables variables, float anchorOffset = 0f)
            {
                referenceFrame.transform.position = new Vector3(variables.anchorPosition.x, 0, variables.anchorPosition.z);
                referenceFrame.transform.forward = data.thetaVector;

                start.transform.localPosition = new Vector3(0, variables.anchorPosition.y, 0);
                middle.transform.localPosition = new Vector3(0, data.height, data.range * .5f);
                end.transform.localPosition = new Vector3(0, 0, data.range);

                data.start = start.transform.position;
                data.middle = middle.transform.position;
                data.end = end.transform.position;
            }

            public void ApplyBallisticsData(Vector3 hand, Vector3 anchor, float speed)
            {
                ballisticVariables.SetBallisticVariables(hand, anchor, speed);
                ballisticData.Calculate(ballisticVariables);
                SetBallisticData(ballisticData, ballisticVariables);
                grappleVisual.BezierLineRenderer(ballisticData.start, ballisticData.middle, ballisticData.end);
            }
        }

        [Serializable] public class GrappleLaunch
        {
            private bool grabCurrent;
            private bool grabPrevious;

            private Rigidbody currentRigidBody;

            public void CheckLaunch(bool grab, GameObject grapplePrefab, BallisticReferenceFrame referenceFrame)
            {
                grabCurrent = grab;
                if (grabCurrent && !grabPrevious) Launch(grapplePrefab, referenceFrame);
                grabPrevious = grabCurrent;
            }
            
            private void Launch(GameObject grapplePrefab, BallisticReferenceFrame referenceFrame)
            {
                GameObject hook = Instantiate(grapplePrefab, null);
                hook.transform.position = referenceFrame.ballisticVariables.anchorPosition;
                
                currentRigidBody = hook.GetComponent<Rigidbody>();
                currentRigidBody.AddForce(referenceFrame.ballisticData.initialVelocity, ForceMode.Impulse);
            }
        }
        
        private void Start()
        {
            controller = GetComponent<ControllerTransforms>();
            launchAnchor = GetComponent<LaunchAnchor>();

            rightReferenceFrame.SetupReferenceFrame("Right", grappleVisualMaterial);
            leftReferenceFrame.SetupReferenceFrame("Left", grappleVisualMaterial);
        }

        private void Update()
        {
            rightReferenceFrame.ApplyBallisticsData(controller.RightPosition(), launchAnchor.RightAnchor(), launchSpeed);
            leftReferenceFrame.ApplyBallisticsData(controller.LeftPosition(), launchAnchor.LeftAnchor(), launchSpeed);
            
            rightGrapple.CheckLaunch(controller.RightGrab(), hook, rightReferenceFrame);
            leftGrapple.CheckLaunch(controller.LeftGrab(), hook, leftReferenceFrame);
        }
    }
}
