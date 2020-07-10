using System;
using Delayed_Messaging.Scripts.Player;
using Delayed_Messaging.Scripts.Utilities;
using UnityEngine;
using BallisticData =  Grapple.Scripts.BallisticTrajectory.BallisticTrajectoryData;

namespace Grapple.Scripts
{
    [RequireComponent(typeof(ControllerTransforms), typeof(LaunchAnchor))]
    public class GrappleSystem : MonoBehaviour
    {
        [Header("Grapple System Configuration")]
        [Range(0, 100f)] public float launchSpeed = 1f, grappleRange = 50f, indirectAngle = 25f;
        [SerializeField] private TimeManager.SlowTimeData slowTime;
        [Header("Grapple System References")]
        [Space(10), SerializeField] private GameObject hook, visual;
        public Material grappleVisualMaterial, ropeMaterial;
        public LayerMask grappleLayer;

        private ControllerTransforms controller;
        private LaunchAnchor launchAnchor;
        private Rigidbody playerRigidBody;
        private TimeManager timeManager;
        private SphereCollider headCollider;

        [HideInInspector] public Location leftLocation, rightLocation;
        [HideInInspector] public Grapple leftGrapple, rightGrapple;

        [Serializable] public class Location
        {
            private Vector3 grappleDirection, anchor;
            private float indirectAngle, grappleDistance;
            private LayerMask grappleLayer;
            public enum GrappleType { DIRECT, INDIRECT, INVALID }
            [Serializable] public struct GrappleLocationData
            {
                public Vector3 direction;
                public RaycastHit grappleLocation, indirectLocation;
                public GrappleType grappleType;
            }
            public GrappleLocationData data;

            /// <summary>
            /// Cache the values set in the inspector, only called once on start
            /// </summary>
            /// <param name="distance"></param>
            /// <param name="angle"></param>
            /// <param name="layerMask"></param>
            public void CacheValues(float distance, float angle, LayerMask layerMask)
            {
                indirectAngle = angle;
                grappleDistance = distance;
                grappleLayer = layerMask;
            }
            /// <summary>
            /// Takes in the anchor and hand location, outputs grapple location and type
            /// </summary>
            /// <param name="anchorPosition"></param>
            /// <param name="hand"></param>
            /// <returns></returns>
            public void GrappleLocation(Vector3 anchorPosition, Vector3 hand)
            {
                anchor = anchorPosition;
                grappleDirection = (hand - anchor).normalized;
                data.direction = grappleDirection;

                switch (Physics.Raycast(anchor, grappleDirection, out RaycastHit hit, grappleDistance, grappleLayer))
                {
                    case true:
                        data.indirectLocation = hit;
                        data.grappleLocation = hit;
                        data.grappleType = GrappleType.DIRECT;
                        return;
                    case false when ValidFallback():
                        data.grappleLocation = data.indirectLocation;
                        data.grappleType = GrappleType.INDIRECT;
                        return;
                    case false:
                        data.grappleType = GrappleType.INVALID;
                        return;
                }
            }
            /// <summary>
            /// Checks if the last grapple location is valid or not
            /// </summary>
            /// <returns></returns>
            private bool ValidFallback()
            {
                return Vector3.Angle(grappleDirection, data.indirectLocation.point - anchor) <= indirectAngle;
            }
        }
        
        [Serializable] public class Grapple
        {
            private bool launchCurrent, launchPrevious, grappleConnected, hanging;
            private GameObject hookPrefab, anchor, detectionVisual, grappleVisual;

            private const float ReelForce = 20f, RopeWidth = .01f, RopeSpring = 20f, Damper = 50f, MinimumDistance = .1f, Slack = 1f;
            private Vector3 lookDirection, ropeCenter, detectionMiddle, detectionEnd;
            private Vector2 joystick;
            
            public GrappleHook grappleHook;
            public SpringJoint grappleJoint;
            public LineRenderer rope, detection;
            private Rigidbody player;
            private TimeManager timeManager;
            private TimeManager.SlowTimeData slowTime;
            private Location location;
            private Location.GrappleLocationData grappleLocation;
            public enum GrappleState { REEL_IN, REEL_OUT, HANG }
            private GrappleState grappleState;

            /// <summary>
            /// Configures the setup and caches external variables so they don't need to be passed more than once
            /// </summary>
            /// <param name="ropeMaterial"></param>
            /// <param name="visualMaterial"></param>
            /// <param name="ropeParent"></param>
            /// <param name="grapplePrefab"></param>
            /// <param name="anchorReference"></param>
            /// <param name="visual"></param>
            /// <param name="rigidBody"></param>
            /// <param name="slowTimeData"></param>
            /// <param name="manager"></param>
            public void ConfigureGrapple(Material ropeMaterial, Material visualMaterial, Transform ropeParent, GameObject grapplePrefab, GameObject anchorReference, GameObject visual, Rigidbody rigidBody, TimeManager.SlowTimeData slowTimeData, TimeManager manager)
            {
                // Setup Rope
                rope = ropeParent.gameObject.AddComponent<LineRenderer>();
                rope.SetupLineRender(ropeMaterial, RopeWidth, true);

                // Setup References
                slowTime = slowTimeData;
                timeManager = manager;
                player = rigidBody;
                hookPrefab = grapplePrefab;
                anchor = anchorReference;
                
                // Setup Detection Visual
                detectionVisual = new GameObject("[Detection Visual]");
                detectionVisual.transform.SetParent(anchor.transform);
                detection = detectionVisual.gameObject.AddComponent<LineRenderer>();
                detection.SetupLineRender(visualMaterial, .003f, true);
                grappleVisual = Instantiate(visual, anchor.transform);
                
                // Create first hook
                CreateHook();
            }
            /// <summary>
            /// Creates a frozen hook at the anchor location
            /// </summary>
            private void CreateHook()
            {
                GameObject hook = Instantiate(hookPrefab, anchor.transform);
                grappleHook = hook.GetComponent<GrappleHook>();
                grappleHook.SpawnHook();
            }

            /// <summary>
            /// Checks the conditions to launch a grapple
            /// </summary>
            /// <param name="launch"></param>
            /// <param name="jettison"></param>
            /// <param name="gestureVector"></param>
            /// <param name="look"></param>
            /// <param name="locationData"></param>
            public void CheckLaunch(bool launch, bool jettison, Vector2 gestureVector, Vector3 look, Location locationData)
            {
                // Cache values
                launchCurrent = launch;
                joystick = gestureVector;
                lookDirection = look.normalized;
                location = locationData;

                // Check States
                if (launchCurrent && !launchPrevious) Launch();
                if (grappleConnected) CheckGrappleState();
                if (!launchCurrent && launchPrevious) timeManager.SlowTime(slowTime);
                if (jettison) Jettison();
                
                // Reset Values
                launchPrevious = launchCurrent;
                
                // Draw Calls
                DrawRope();
                DrawVisualisation();
            }

            /// <summary>
            /// Contains the logic which launches a grapple, grappleLocation is always valid
            /// </summary>
            private void Launch()
            {
                // Checks if the grapple location is valid and caches values if it is
                if (location.data.grappleType == Location.GrappleType.INVALID) return;
                grappleLocation = location.data;
                
                // Method Calls
                StopHanging();
                CreateHook();
                
                // Reset Logic
                grappleHook.LaunchHook(grappleLocation);
                grappleHook.collide.AddListener(GrappleAttach);
                grappleConnected = false;
            }
            /// <summary>
            /// Feeds in the cached value for the joystick and outputs a grapple state
            /// </summary>
            private void CheckGrappleState()
            {
                switch (Gesture.CheckGrappleState(joystick))
                {
                    case GrappleState.REEL_IN:
                        Reel(GrappleState.REEL_IN);
                        return;
                    case GrappleState.REEL_OUT:
                        Reel(GrappleState.REEL_OUT);
                        return;
                    case GrappleState.HANG:
                        Hang();
                        return;
                    default:
                        return;
                }
            }
            /// <summary>
            /// Update analogue called when holding a grapple which has attached to something successfully, direction determines the direction
            /// </summary>
            /// <param name="direction"></param>
            private void Reel(GrappleState direction)
            {
                StopHanging();
                switch (direction)
                {
                    case GrappleState.REEL_IN:
                        AddForce(grappleLocation.grappleLocation.point - anchor.transform.position);
                        return;
                    case GrappleState.REEL_OUT:
                        AddForce(anchor.transform.position - grappleLocation.grappleLocation.point);
                        return;
                    case GrappleState.HANG:
                        return;
                    default:
                        return;
                }
            }
            /// <summary>
            /// Adds force to the player rigid body in the supplied vector 
            /// </summary>
            /// <param name="vector"></param>
            private void AddForce(Vector3 vector)
            {
                player.AddForce(vector.normalized * ReelForce, ForceMode.Acceleration);
                player.AddForce(lookDirection * Mathf.Lerp(0, ReelForce, .5f));
            }
            /// <summary>
            /// Called once to create a spring joint with the current grapple state
            /// </summary>
            private void Hang()
            {
                if (hanging) return;
                
                grappleJoint = player.gameObject.AddComponent<SpringJoint>();
                grappleJoint.ConfigureSpringJoint(
                    grappleHook.hookRigidBody, 
                    false, RopeSpring, Damper, MinimumDistance, 
                    Vector3.Distance(player.transform.position, grappleLocation.grappleLocation.point) + Slack);
                
                // Ensure this only gets called once
                hanging = true;
            }
            /// <summary>
            /// Destroys the spring joint used to hang
            /// </summary>
            private void StopHanging()
            {
                if (!hanging) return;
                hanging = false;
                Destroy(grappleJoint);
            }
            /// <summary>
            /// Called when ropes are disconnected
            /// </summary>
            private void Jettison()
            {
                StopHanging();
                CreateHook();
                grappleConnected = false;
                timeManager.SlowTime(slowTime);
            }
            /// <summary>
            /// This is called when the collide event is triggered in the active GrappleHook
            /// </summary>
            private void GrappleAttach()
            {
                grappleConnected = true;
            }
            /// <summary>
            /// Draws the rope between the anchor and the grapple hook
            /// </summary>
            public void DrawRope()
            {
                Vector3 anchorPosition = anchor.transform.position;
                Vector3 hookPosition = grappleHook.transform.position;
                ropeCenter = Vector3.Lerp(ropeCenter, Vector3.Lerp(anchorPosition, hookPosition, .5f), .1f);
                rope.BezierLineRenderer(anchorPosition, ropeCenter, hookPosition);
            }
            /// <summary>
            /// Draws the visual to show where 
            /// </summary>
            private void DrawVisualisation()
            {
                // Cache Values
                Vector3 start = anchor.transform.position;
                Vector3 end;
                
                // Determine where the end of the detection curve should be
                switch (location.data.grappleType)
                {
                    case Location.GrappleType.DIRECT:
                        SetVisual(true, location.data.grappleLocation);
                        end =  location.data.grappleLocation.point;
                        detectionMiddle = Vector3.Lerp(detectionMiddle, Vector3.Lerp(start, detectionEnd, .5f), .1f);
                        break;
                    case Location.GrappleType.INDIRECT:
                        SetVisual(true, location.data.indirectLocation);
                        end =  location.data.indirectLocation.point;
                        detectionMiddle = Vector3.Lerp(detectionMiddle, (start + location.data.direction) * (Vector3.Distance(start, end) * 25f), .1f);
                        break;
                    case Location.GrappleType.INVALID:
                        SetVisual(false, location.data.indirectLocation);
                        end =  start + location.data.direction;
                        detectionMiddle = Vector3.Lerp(detectionMiddle, Vector3.Lerp(start, detectionEnd, .5f), .5f);
                        break;
                    default:
                        return;
                }
                
                // Align those values
                detectionEnd = Vector3.Lerp(detectionEnd, end, .5f);
                detection.BezierLineRenderer(start, detectionMiddle, detectionEnd);
            }
            private void SetVisual(bool enabled, RaycastHit grapple)
            {
                grappleVisual.SetActive(enabled);
                grappleVisual.transform.position = grapple.point;
                grappleVisual.transform.forward = grapple.normal;
            }
        }
        /// <summary>
        /// Initialise and Cache Values
        /// </summary>
        private void Start()
        {
            controller = GetComponent<ControllerTransforms>();
            playerRigidBody = GetComponent<Rigidbody>();
            launchAnchor = GetComponent<LaunchAnchor>();
            timeManager = GetComponent<TimeManager>();
            headCollider = gameObject.AddComponent<SphereCollider>();

            leftLocation.CacheValues(
                grappleRange,
                indirectAngle,
                grappleLayer);
            rightLocation.CacheValues(
                grappleRange,
                indirectAngle,
                grappleLayer);
            
            launchAnchor.ConfigureAnchors(controller);
            headCollider.radius = .35f;
            
            leftGrapple.ConfigureGrapple(
                ropeMaterial, grappleVisualMaterial,
                launchAnchor.leftAnchor.transform,
                hook, launchAnchor.leftAnchor, visual,
                playerRigidBody, 
                slowTime, timeManager);
            rightGrapple.ConfigureGrapple(
                ropeMaterial, grappleVisualMaterial,
                launchAnchor.rightAnchor.transform,
                hook, launchAnchor.rightAnchor, visual,
                playerRigidBody, 
                slowTime, timeManager);
        }
        private void Update()
        {
            // Calculate Grapple Location
            leftLocation.GrappleLocation(
                launchAnchor.Anchor(LaunchAnchor.Configuration.LEFT),
                controller.Position(ControllerTransforms.Check.LEFT));
            rightLocation.GrappleLocation(
                launchAnchor.Anchor(LaunchAnchor.Configuration.RIGHT),
                controller.Position(ControllerTransforms.Check.RIGHT));
            
            // Check for User Input
            rightGrapple.CheckLaunch(
                controller.Select(ControllerTransforms.Check.RIGHT),
                controller.Joystick(ControllerTransforms.Check.RIGHT),
                controller.JoyStick(ControllerTransforms.Check.RIGHT),
                controller.ForwardVector(ControllerTransforms.Check.HEAD),
                rightLocation);
            leftGrapple.CheckLaunch(
                controller.Select(ControllerTransforms.Check.LEFT), 
                controller.Joystick(ControllerTransforms.Check.LEFT),
                controller.JoyStick(ControllerTransforms.Check.LEFT),
                controller.ForwardVector(ControllerTransforms.Check.HEAD),
                leftLocation);
        }
        private void FixedUpdate()
        {
            headCollider.center = controller.LocalPosition(ControllerTransforms.Check.HEAD);
        }
    }
}
