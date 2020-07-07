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
        [SerializeField] private TimeManager.SlowTimeData slowTime;
        [Header("Grapple System References")]
        [Space(10), SerializeField] private GameObject hook;
        public Material grappleVisualMaterial, ropeMaterial;
        public LayerMask grappleLayer;

        private ControllerTransforms controller;
        private LaunchAnchor launchAnchor;
        private Rigidbody playerRigidBody;
        private TimeManager timeManager;

        [HideInInspector] public BallisticReferenceFrame rightReferenceFrame, leftReferenceFrame;
        [HideInInspector] public Grapple leftGrapple, rightGrapple;

        [Serializable] public class BallisticReferenceFrame
        {
            public LineRenderer grappleVisual;
            public GameObject referenceFrame, start, middle, end;

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
                middle.transform.localPosition = new Vector3(0, data.height > 0 ? data.height : 0, data.range > 0 ? data.range * .5f : 0f);
                end.transform.localPosition = new Vector3(0, 0, data.range > 0 ? data.range : 0);

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
            private bool launchCurrent, launchPrevious;
            private GameObject hookPrefab, anchor;

            private Rigidbody player;
            private bool reel;
            private const float ReelForce = 2f;
            private Vector3 reelLocation;
                
            public GrappleHook grappleHook;
            private LayerMask grappleMask;
            public SpringJoint grappleJoint;
            
            public LineRenderer rope;
            private const float RopeWidth = .015f;
            private GameObject ropeCenter;
            
            private TimeManager.SlowTimeData slowTime;
            private TimeManager timeManager;

            /// <summary>
            /// Configures the setup and caches external variables so they don't need to be passed more than once
            /// </summary>
            /// <param name="ropeMaterial"></param>
            /// <param name="ropeParent"></param>
            /// <param name="self"></param>
            /// <param name="grapplePrefab"></param>
            /// <param name="anchorReference"></param>
            /// <param name="layerMask"></param>
            /// <param name="rb"></param>
            /// <param name="slowTimeData"></param>
            public void ConfigureGrapple(Material ropeMaterial, Transform ropeParent, Transform self, GameObject grapplePrefab, GameObject anchorReference, LayerMask layerMask, Rigidbody rb, TimeManager.SlowTimeData slowTimeData, TimeManager manager)
            {
                // Setup Rope
                rope = ropeParent.gameObject.AddComponent<LineRenderer>();
                rope.SetupLineRender(ropeMaterial, RopeWidth, true);
                ropeCenter = new GameObject("[Rope Center]");
                ropeCenter.transform.SetParent(ropeParent);
                
                // Setup Joints
                //grappleJoint = self.gameObject.AddComponent<SpringJoint>();
                //grappleJoint.SetSpringJointValues();
                
                // Setup References
                slowTime = slowTimeData;
                timeManager = manager;
                player = rb;
                hookPrefab = grapplePrefab;
                anchor = anchorReference;
                grappleMask = layerMask;
                CreateHook(grappleMask);
            }
            /// <summary>
            /// Creates a frozen hook at the anchor location
            /// </summary>
            private void CreateHook(LayerMask layerMask)
            {
                // Create Hook
                GameObject hook = Instantiate(hookPrefab, anchor.transform);
                grappleHook = hook.GetComponent<GrappleHook>();
                grappleHook.ConfigureGrappleHook(layerMask);
                grappleHook.SpawnHook();
            }
            /// <summary>
            /// Checks the conditions to launch a grapple
            /// </summary>
            /// <param name="launch"></param>
            /// <param name="referenceFrame"></param>
            public void CheckLaunch(bool launch, BallisticReferenceFrame referenceFrame)
            {
                launchCurrent = launch;
                if (launchCurrent && !launchPrevious) Launch(referenceFrame);
                else if (launchCurrent && launchPrevious) ReelIn();
                launchPrevious = launchCurrent;
            }
            /// <summary>
            /// Contains the logic which launches a grapple
            /// </summary>
            /// <param name="referenceFrame"></param>
            private void Launch(BallisticReferenceFrame referenceFrame)
            {
                CreateHook(grappleMask);
                grappleHook.LaunchHook(referenceFrame.ballisticData.initialVelocity);
                grappleHook.collide.AddListener(GrappleAttach);
                reel = false;
            }
            /// <summary>
            /// Update analogue called when holding a grapple which has attached to something successfully
            /// </summary>
            private void ReelIn()
            {
                if (!reel) return;
                player.AddForce((reelLocation - anchor.transform.position) * ReelForce, ForceMode.Acceleration);
            }
            /// <summary>
            /// Called once when a grapple is released
            /// </summary>
            private void Decouple()
            {
                //grappleJoint.spring = 0;
                timeManager.SlowTime(slowTime);
            }
            /// <summary>
            /// This is called when the collide event is triggered in the active GrappleHook
            /// </summary>
            private void GrappleAttach()
            {
                reel = true;
                reelLocation = grappleHook.grapplePoint;
                //grappleJoint.SetSpringJointAnchor(grappleHook.grapplePoint);
                //grappleJoint.SetSpringJointValues();
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
            playerRigidBody = GetComponent<Rigidbody>();
            launchAnchor = GetComponent<LaunchAnchor>();
            timeManager = GetComponent<TimeManager>();
            
            launchAnchor.ConfigureAnchors(controller);

            rightReferenceFrame.SetupReferenceFrame("Right", grappleVisualMaterial);
            leftReferenceFrame.SetupReferenceFrame("Left", grappleVisualMaterial);
            
            leftGrapple.ConfigureGrapple(
                ropeMaterial, 
                launchAnchor.leftAnchor.transform, 
                transform, 
                hook,
                launchAnchor.leftAnchor, 
                grappleLayer, 
                playerRigidBody, 
                slowTime, 
                timeManager);
            rightGrapple.ConfigureGrapple(
                ropeMaterial, 
                launchAnchor.rightAnchor.transform, 
                transform, 
                hook,
                launchAnchor.rightAnchor, 
                grappleLayer, 
                playerRigidBody, 
                slowTime, 
                timeManager);
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
