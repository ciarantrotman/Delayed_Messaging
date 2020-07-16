using System;
using System.Collections.Generic;
using Delayed_Messaging.Scripts.Player;
using Delayed_Messaging.Scripts.Utilities;
using UnityEngine;
using BallisticData =  Grapple.Scripts.BallisticTrajectory.BallisticTrajectoryData;

namespace Grapple.Scripts
{
    [RequireComponent(typeof(ControllerTransforms), typeof(LaunchAnchor))]
    public class GrappleSystem : MonoBehaviour
    {
        [Range(0, 100f)] public float launchSpeed = 1f, grappleRange = 50f, indirectAngle = 25f;
        [SerializeField] private TimeManager.SlowTimeData slowTime;
        [SerializeField] private GameObject hook, visual, oneMeterMarker, tenMeterMarker;
        public Material grappleVisualMaterial, ropeMaterial;
        public LayerMask grappleLayer;

        private ControllerTransforms controller;
        private LaunchAnchor launchAnchor;
        private Rigidbody playerRigidBody;
        private TimeManager timeManager;
        private SphereCollider headCollider;

        [HideInInspector] public Location leftLocation, rightLocation;
        [HideInInspector] public Grapple leftGrapple, rightGrapple;
        [HideInInspector] public Distance leftDistance, rightDistance;

        /// <summary>
        /// Used to visualise raycast distances
        /// </summary>
        [Serializable] public class Distance
        {
            private GameObject parent;
            private LineRenderer visual;
            /// <summary>
            /// 
            /// </summary>
            /// <param name="target"></param>
            /// <param name="range"></param>
            /// <param name="one"></param>
            /// <param name="ten"></param>
            /// <param name="material"></param>
            public void ConfigureDistance(Transform target, float range, GameObject one, GameObject ten, Material material)
            {
                // Create parent objects
                parent = new GameObject("[Distance Parent]");
                parent.transform.SetParent(target);
                parent.transform.localPosition = Vector3.zero;

                // Setup line renderer
                visual = parent.AddComponent<LineRenderer>();
                visual.SetupLineRender(material, .001f, true);
                visual.useWorldSpace = false;
                visual.positionCount = 2;
                visual.SetPosition(0, Vector3.zero);
                visual.SetPosition(1, new Vector3(0,0, range));
                
                // Create markers
                for (int i = 1; i < range; i++)
                {
                    GameObject marker = Instantiate(i % 10 == 0 ? ten : one, parent.transform);
                    marker.transform.localPosition = new Vector3(0, 0, i);
                }
            }
            /// <summary>
            /// Aligns the parent object with the input vector
            /// </summary>
            /// <param name="vector"></param>
            public void AlignDistance(Vector3 vector)
            {
                parent.transform.forward = Vector3.Lerp(parent.transform.forward, vector, .75f);
            }
        }
        /// <summary>
        /// Used to calculate where the grapple location is
        /// </summary>
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
                public bool connected;
                ///
                /// This returns the valid location for the initial grapple location and should only be used for visualisation
                /// 
                public RaycastHit GrappleLocation()
                {
                    switch (grappleType)
                    {
                        case GrappleType.DIRECT:
                            return grappleLocation;
                        case GrappleType.INDIRECT:
                            return indirectLocation;
                        case GrappleType.INVALID:
                            return grappleLocation;
                        default:
                            return grappleLocation;
                    }
                }
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
            /// <param name="direction"></param>
            /// <returns></returns>
            public void GrappleLocation(Vector3 anchorPosition, Vector3 direction)
            {
                anchor = anchorPosition;
                grappleDirection = direction.normalized;
                data.direction = grappleDirection;

                switch (Physics.Raycast(anchor, grappleDirection, out RaycastHit hit, grappleDistance, grappleLayer))
                {
                    case true:
                        data.indirectLocation = hit;
                        data.grappleLocation = hit;
                        data.grappleType = GrappleType.DIRECT;
                        break;
                    case false when ValidFallback():
                        data.grappleLocation = data.indirectLocation;
                        data.grappleType = GrappleType.INDIRECT;
                        break;
                    case false:
                        data.grappleType = GrappleType.INVALID;
                        break;
                }
            }
            /// <summary>
            /// Checks if the last grapple location is valid or not - it checks deviation and distance
            /// </summary>
            /// <returns></returns>
            private bool ValidFallback()
            {
                // todo: rework for moving objects
                return Vector3.Angle(grappleDirection, data.indirectLocation.point - anchor) <= indirectAngle && Vector3.Distance(data.indirectLocation.point, anchor) <= grappleDistance;
            }
        }
        /// <summary>
        /// Used to calculate grappling states and apply relevant logic
        /// </summary>
        [Serializable] public class Grapple
        {
            private bool launchCurrent, launchPrevious, grappleConnected, hanging;
            private GameObject hookPrefab, anchor, detectionParent, invalid, indirect, grappleVisual;

            private float angle; 
            private const float ReelForce = 15f, RopeWidth = .01f, RopeSpring = 35f, Damper = 10f, MinimumDistance = .1f, Slack = .5f, DetectionWidth = .0075f;
            private Vector3 lookDirection, ropeCenter, detectionMiddle, detectionEnd;
            private Vector2 joystick;

            public GrappleRope grappleRope;
            public GrappleHook grappleHook;
            public SpringJoint grappleJoint;
            public LineRenderer /*rope,*/ detection;
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
            /// <param name="grapplePrefab"></param>
            /// <param name="anchorReference"></param>
            /// <param name="visual"></param>
            /// <param name="rigidBody"></param>
            /// <param name="slowTimeData"></param>
            /// <param name="manager"></param>
            /// <param name="mask"></param>
            public void ConfigureGrapple(Material ropeMaterial, Material visualMaterial, GameObject grapplePrefab, GameObject anchorReference, GameObject visual, Rigidbody rigidBody, TimeManager.SlowTimeData slowTimeData, TimeManager manager, LayerMask mask, float grappleAngle)
            {
                // Setup References
                slowTime = slowTimeData;
                timeManager = manager;
                player = rigidBody;
                hookPrefab = grapplePrefab;
                anchor = anchorReference;
                angle = grappleAngle;
                
                // Setup Rope
                //rope = anchor.AddComponent<LineRenderer>();
                //rope.SetupLineRender(ropeMaterial, RopeWidth, true);
                grappleRope = anchor.AddComponent<GrappleRope>();
                grappleRope.ConfigureRope(anchor.transform, ropeMaterial, mask);
                
                // Setup Detection Visual
                detectionParent = new GameObject("[Detection Parent]");
                invalid = new GameObject("[Invalid]");
                indirect = new GameObject("[Indirect]");
                CreateOffset(invalid, detectionParent, 10f);
                CreateOffset(indirect, detectionParent, 0f);

                detectionParent.transform.SetParent(anchor.transform);
                detection = detectionParent.gameObject.AddComponent<LineRenderer>();
                detection.SetupLineRender(visualMaterial, DetectionWidth, true);
                grappleVisual = Instantiate(visual, anchor.transform);
                
                // Create first hook
                CreateHook();
            }
            /// <summary>
            /// Creates the two offset components
            /// </summary>
            /// <param name="target"></param>
            /// <param name="parent"></param>
            /// <param name="offset"></param>
            private void CreateOffset(GameObject target, GameObject parent, float offset)
            {
                target.transform.SetParent(parent.transform);
                target.transform.localPosition = new Vector3(0,0,offset);
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
            public void GrappleUpdate(bool launch, bool jettison, Vector2 gestureVector, Vector3 look, Location locationData)
            {
                // Cache values
                launchCurrent = launch;
                joystick = gestureVector;
                lookDirection = look.normalized;
                location = locationData;
                location.data.connected = grappleConnected;

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
                
                // Reset Logic
                ResetGrapple();
                grappleHook.LaunchHook(grappleLocation);
                grappleHook.collide.AddListener(GrappleAttach);
                grappleConnected = false;
                grappleRope.LaunchRope(grappleHook.transform, grappleLocation.grappleLocation);
            }
            /// <summary>
            /// This is called when the collide event is triggered in the active GrappleHook
            /// </summary>
            private void GrappleAttach()
            {
                grappleConnected = true;
                grappleRope.AttachRope(grappleLocation.grappleLocation);
                //grappleRope.wrap.AddListener(ReconfigureHang);
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
                        AddForce(grappleRope.RopeVector(), 1.5f);
                        return;
                    case GrappleState.REEL_OUT:
                        AddForce(-grappleRope.RopeVector(), .5f);
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
            /// <param name="modifier"></param>
            private void AddForce(Vector3 vector, float modifier)
            {
                player.AddForce(vector.normalized * (ReelForce * modifier), ForceMode.Acceleration);
                player.AddForce(lookDirection * (ReelForce * .5f), ForceMode.Acceleration);
            }
            /// <summary>
            /// Called once to create a spring joint with the current grapple state
            /// </summary>
            private void Hang()
            {
                if (!hanging)
                {
                    grappleJoint = player.gameObject.AddComponent<SpringJoint>();
                    grappleJoint.ConfigureSpringJoint(
                        grappleRope.GrappleLocation(), 
                        false, RopeSpring, Damper, MinimumDistance, 
                        grappleRope.RopeLength() + Slack);
                    hanging = true;
                }
                // Reconfigure it every update in the case of being grappled to a moving target
                grappleJoint.SetSpringJointAnchor(grappleRope.GrappleLocation());
            }
            /// <summary>
            /// Used to reconfigure the hanging location for the spring joint
            /// </summary>
            private void ReconfigureHang()
            {
                StopHanging();
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
                // Reset all states
                ResetGrapple();
                
                // Only implement the following if you are connected
                if (!grappleConnected) return;
                grappleConnected = false;
                
                // Jettison effects
                timeManager.SlowTime(slowTime);
                player.AddForce(lookDirection * (ReelForce * .25f), ForceMode.VelocityChange);
            }
            /// <summary>
            /// Resets the state of the rope
            /// </summary>
            private void ResetGrapple()
            {
                grappleRope.DisconnectRope();
                StopHanging();
                CreateHook();
            }
            /// <summary>
            /// Draws the rope between the anchor and the grapple hook
            /// </summary>
            public void DrawRope()
            {
                /*
                Vector3 anchorPosition = anchor.transform.position;
                Vector3 hookPosition = grappleHook.transform.position;
                ropeCenter = Vector3.Lerp(ropeCenter, Vector3.Lerp(anchorPosition, hookPosition, .5f), .1f);
                rope.BezierLineRenderer(anchorPosition, ropeCenter, hookPosition);
                */
            }
            /// <summary>
            /// Draws the visual to show where 
            /// </summary>
            private void DrawVisualisation()
            {
                // Cache Values
                Vector3 start = anchor.transform.position, middle, end;
                float speed;

                // Determine where the end of the detection curve should be
                switch (location.data.grappleType)
                {
                    case Location.GrappleType.DIRECT:
                        SetVisual(true, location.data.GrappleLocation());
                        end =  location.data.GrappleLocation().point;
                        middle = detectionMiddle;
                        speed = .75f;
                        break;
                    case Location.GrappleType.INDIRECT:
                        SetVisual(true, location.data.GrappleLocation());
                        end =  location.data.GrappleLocation().point;
                        middle = indirect.transform.position;
                        speed = .35f;
                        break;
                    case Location.GrappleType.INVALID:
                        SetVisual(false, location.data.GrappleLocation());
                        end = invalid.transform.position;
                        middle = detectionMiddle;
                        speed = .9f;
                        break;
                    default:
                        return;
                }
                
                // Align those values
                detectionParent.transform.forward = location.data.direction;
                detectionEnd = Vector3.Lerp(detectionEnd, end, speed);
                detectionMiddle = Vector3.Lerp(detectionMiddle, Vector3.Lerp(start, detectionEnd, .5f), speed);
                indirect.transform.localPosition = new Vector3(0,0,(Vector3.Distance(start, end) * .25f));
                detection.BezierLineRenderer(start, middle, detectionEnd);
                
                // Visual feedback for indirect grapple locations
                float inverseLerp = Mathf.InverseLerp(0f, angle, Vector3.Angle(location.data.direction, end - start));
                float lerp = Mathf.Lerp(DetectionWidth, 0, inverseLerp);
                float scale = Mathf.Lerp(1, 0, inverseLerp);
                
                grappleVisual.transform.localScale = Vector3.one * scale;
                detection.startWidth = lerp; detection.endWidth = lerp;
            }
            /// <summary>
            /// Determines the state of the visual for the grapple point
            /// </summary>
            /// <param name="enabled"></param>
            /// <param name="grapple"></param>
            private void SetVisual(bool enabled, RaycastHit grapple)
            {
                detection.enabled = enabled;
                grappleVisual.SetActive(enabled);
                grappleVisual.transform.position = Vector3.Lerp(grappleVisual.transform.position, grapple.point, .75f);
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
            headCollider.radius = .2f;
            
            leftGrapple.ConfigureGrapple(
                ropeMaterial, grappleVisualMaterial,
                hook, launchAnchor.leftAnchor, visual,
                playerRigidBody, 
                slowTime, timeManager, grappleLayer,
                indirectAngle);
            rightGrapple.ConfigureGrapple(
                ropeMaterial, grappleVisualMaterial,
                hook, launchAnchor.rightAnchor, visual,
                playerRigidBody, 
                slowTime, timeManager, grappleLayer,
                indirectAngle);
            
            leftDistance.ConfigureDistance(
                launchAnchor.leftAnchor.transform, 
                grappleRange, 
                oneMeterMarker, tenMeterMarker,
                grappleVisualMaterial);
            rightDistance.ConfigureDistance(
                launchAnchor.rightAnchor.transform, 
                grappleRange, 
                oneMeterMarker, tenMeterMarker,
                grappleVisualMaterial);
        }
        #region Conditional Values
                /// <summary>
        /// Compares the anchor configuration and returns the handed select value
        /// </summary>
        /// <param name="check"></param>
        /// <returns></returns>
        private bool Launch(ControllerTransforms.Check check)
        {
            switch (launchAnchor.configuration)
            {
                case LaunchAnchor.Configuration.CENTER:
                    return controller.Select(check);
                case LaunchAnchor.Configuration.RIGHT:
                    return controller.Select(ControllerTransforms.Check.RIGHT);
                case LaunchAnchor.Configuration.LEFT:
                    return controller.Select(ControllerTransforms.Check.LEFT);
                default:
                    return false;
            }
        }
        /// <summary>
        /// Compares the anchor configuration and returns the handed select value
        /// </summary>
        /// <param name="check"></param>
        /// <returns></returns>
        private Vector2 GrappleState(ControllerTransforms.Check check)
        {
            switch (launchAnchor.configuration)
            {
                case LaunchAnchor.Configuration.CENTER:
                    return controller.JoyStick(check);
                case LaunchAnchor.Configuration.RIGHT:
                    return controller.JoyStick(ControllerTransforms.Check.RIGHT);
                case LaunchAnchor.Configuration.LEFT:
                    return controller.JoyStick(ControllerTransforms.Check.LEFT);
                default:
                    return Vector2.zero;
            }
        }
        /// <summary>
        /// Compares the anchor configuration and returns the handed select value
        /// </summary>
        /// <param name="check"></param>
        /// <returns></returns>
        private bool Jettison(ControllerTransforms.Check check)
        {
            switch (launchAnchor.configuration)
            {
                case LaunchAnchor.Configuration.CENTER:
                    return controller.Joystick(check);
                case LaunchAnchor.Configuration.RIGHT:
                    return controller.Joystick(ControllerTransforms.Check.RIGHT);
                case LaunchAnchor.Configuration.LEFT:
                    return controller.Joystick(ControllerTransforms.Check.LEFT);
                default:
                    return false;
            }
        }
        #endregion
        private void Update()
        {
            // Align Distance Visualisation
            leftDistance.AlignDistance(launchAnchor.Direction(LaunchAnchor.Configuration.LEFT));
            rightDistance.AlignDistance(launchAnchor.Direction(LaunchAnchor.Configuration.RIGHT));
            
            // Calculate Grapple Location
            leftLocation.GrappleLocation(
                launchAnchor.Anchor(LaunchAnchor.Configuration.LEFT),
                launchAnchor.Direction(LaunchAnchor.Configuration.LEFT));
            rightLocation.GrappleLocation(
                launchAnchor.Anchor(LaunchAnchor.Configuration.RIGHT),
                launchAnchor.Direction(LaunchAnchor.Configuration.RIGHT));
            
            // Check for User Input
            rightGrapple.GrappleUpdate(
                Launch(ControllerTransforms.Check.RIGHT), 
                Jettison(ControllerTransforms.Check.RIGHT),
                GrappleState(ControllerTransforms.Check.RIGHT),
                controller.ForwardVector(ControllerTransforms.Check.HEAD),
                rightLocation);
            leftGrapple.GrappleUpdate(
                Launch(ControllerTransforms.Check.LEFT), 
                Jettison(ControllerTransforms.Check.LEFT),
                GrappleState(ControllerTransforms.Check.LEFT),
                controller.ForwardVector(ControllerTransforms.Check.HEAD),
                leftLocation);
        }
        private void FixedUpdate()
        {
            headCollider.center = controller.LocalPosition(ControllerTransforms.Check.HEAD);

            // Stop misalignment from collisions
            Transform self = transform;
            Vector3 rotation = self.eulerAngles;
            self.eulerAngles = new Vector3(0, rotation.y, 0);
        }
    }
}
