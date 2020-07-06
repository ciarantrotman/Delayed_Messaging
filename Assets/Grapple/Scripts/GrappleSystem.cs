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
        public Material ropeMaterial;

        private ControllerTransforms controller;
        private LaunchAnchor launchAnchor;

        [HideInInspector] public BallisticReferenceFrame rightReferenceFrame;
        [HideInInspector] public BallisticReferenceFrame leftReferenceFrame;
        
        [HideInInspector] public Grapple leftGrapple;
        [HideInInspector] public Grapple rightGrapple;

        [Serializable] public class BallisticReferenceFrame
        {
            public LineRenderer grappleVisual;
            public GameObject referenceFrame;
            public GameObject start;
            public GameObject middle;
            public GameObject end;
            
            [HideInInspector] public BallisticData ballisticData;
            [HideInInspector] public BallisticVariables ballisticVariables;
            
            /// <summary>
            /// Creates a local 2D reference frame to do ballistic calculations in
            /// </summary>
            /// <param name="side"></param>
            /// <param name="mat"></param>
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
            /// <summary>
            /// Sets the positions of objects within the reference frame based on calculated ballistic trajectories
            /// </summary>
            /// <param name="data"></param>
            /// <param name="variables"></param>
            /// <param name="anchorOffset"></param>
            private void SetBallisticData(BallisticData data, BallisticVariables variables, float anchorOffset = 0f)
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
            /// <summary>
            /// Calls the ballistic trajectory calculations and sets ballistic data, includes visuals
            /// </summary>
            /// <param name="hand"></param>
            /// <param name="anchor"></param>
            /// <param name="speed"></param>
            public void ApplyBallisticsData(Vector3 hand, Vector3 anchor, float speed)
            {
                ballisticVariables.SetBallisticVariables(hand, anchor, speed);
                ballisticData.Calculate(ballisticVariables);
                SetBallisticData(ballisticData, ballisticVariables);
                //grappleVisual.BezierLineRenderer(ballisticData.start, ballisticData.middle, ballisticData.end);
                grappleVisual.BallisticTrajectory(ballisticData);
            }
        }
        [Serializable] public class Grapple
        {
            private bool launchCurrent;
            private bool launchPrevious;
            private GameObject hookPrefab;
            private GameObject anchor;

            public GrappleHook grappleHook;
            public SpringJoint grappleJoint;
            
            public LineRenderer rope;
            private const float RopeWidth = .05f;
            private GameObject ropeCenter;
            /// <summary>
            /// Configures the setup and caches external variables so they don't need to be passed more than once
            /// </summary>
            /// <param name="ropeMaterial"></param>
            /// <param name="ropeParent"></param>
            /// <param name="self"></param>
            /// <param name="grapplePrefab"></param>
            /// <param name="anchorReference"></param>
            public void ConfigureGrapple(Material ropeMaterial, Transform ropeParent, Transform self, GameObject grapplePrefab, GameObject anchorReference)
            {
                // Setup Rope
                rope = ropeParent.gameObject.AddComponent<LineRenderer>();
                rope.SetupLineRender(ropeMaterial, RopeWidth, true);
                ropeCenter = new GameObject("[Rope Center]");
                ropeCenter.transform.SetParent(ropeParent);
                
                // Setup Joints
                grappleJoint = self.gameObject.AddComponent<SpringJoint>();
                grappleJoint.SetSpringJointValues();
                
                // Setup References
                hookPrefab = grapplePrefab;
                anchor = anchorReference;
                CreateHook();
            }
            /// <summary>
            /// Creates a frozen hook at the anchor location
            /// </summary>
            private void CreateHook()
            {
                // Create Hook
                GameObject hook = Instantiate(hookPrefab, anchor.transform);
                grappleHook.SpawnHook();
                grappleHook = hook.GetComponent<GrappleHook>();
            }
            
            /// <summary>
            /// Checks the conditions to launch a grapple
            /// </summary>
            /// <param name="launch"></param>
            /// <param name="referenceFrame"></param>
            public void CheckLaunch(bool launch, BallisticReferenceFrame referenceFrame)
            {
                launchCurrent = launch;
                if (launchCurrent && !launchPrevious)
                {
                    Launch(referenceFrame);
                }
                launchPrevious = launchCurrent;
            }
            /// <summary>
            /// Contains the logic which launches a grapple
            /// </summary>
            /// <param name="referenceFrame"></param>
            private void Launch(BallisticReferenceFrame referenceFrame)
            {
                CreateHook();
                grappleHook.LaunchHook(referenceFrame.ballisticData.initialVelocity);
                grappleHook.collide.AddListener(GrappleAttach);

                Debug.Log($"" +
                          $"Initial Velocity: {referenceFrame.ballisticData.initialVelocity.magnitude}, " +
                          $"Theta: {referenceFrame.ballisticData.theta}, " +
                          $"Initial Height: {referenceFrame.ballisticData.initialHeight}, " +
                          $"Height: {referenceFrame.ballisticData.height}, " +
                          $"Range: {referenceFrame.ballisticData.range}");
            }
            /// <summary>
            /// This is called when the collide event is triggered in the active GrappleHook
            /// </summary>
            private void GrappleAttach()
            {
                grappleJoint.SetSpringJointAnchor(grappleHook.grapplePoint);
            }
            /// <summary>
            /// Draws the rope between the anchor and the grapple hook
            /// </summary>
            public void DrawRope()
            {
                rope.DrawStraightLineRender(anchor.transform, grappleHook.transform);
            }
        }

        private void Start()
        {
            controller = GetComponent<ControllerTransforms>();
            launchAnchor = GetComponent<LaunchAnchor>();

            rightReferenceFrame.SetupReferenceFrame("Right", grappleVisualMaterial);
            leftReferenceFrame.SetupReferenceFrame("Left", grappleVisualMaterial);
            
            leftGrapple.ConfigureGrapple(ropeMaterial, launchAnchor.leftAnchor.transform, transform, hook, launchAnchor.leftAnchor);
            rightGrapple.ConfigureGrapple(ropeMaterial, launchAnchor.rightAnchor.transform, transform, hook, launchAnchor.rightAnchor);
        }

        private void Update()
        {
            rightReferenceFrame.ApplyBallisticsData(controller.RightPosition(), launchAnchor.RightAnchor(), launchSpeed);
            leftReferenceFrame.ApplyBallisticsData(controller.LeftPosition(), launchAnchor.LeftAnchor(), launchSpeed);
            
            rightGrapple.CheckLaunch(controller.RightSelect(), rightReferenceFrame);
            leftGrapple.CheckLaunch(controller.LeftSelect(), leftReferenceFrame);
            
            rightGrapple.DrawRope();
            leftGrapple.DrawRope();
        }
    }
}
