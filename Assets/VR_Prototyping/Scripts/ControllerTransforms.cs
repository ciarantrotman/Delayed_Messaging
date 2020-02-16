using System.Collections.Generic;
using Leap;
using Leap.Unity;
using Leap.Unity.Interaction;
using Sirenix.OdinInspector;
using UnityEngine;
using UnityEngine.Events;
using Valve.VR;

namespace VR_Prototyping.Scripts
{
    public class ControllerTransforms : MonoBehaviour
    {
        [BoxGroup("Settings"), SerializeField] public bool steamEnabled;
        [BoxGroup("Settings"), SerializeField] public bool leapMotionEnabled;
        [BoxGroup("Settings"), ShowIf("leapMotionEnabled"), Indent, Range(0, 1), SerializeField] private float leapStabilisation = .3f;
        [BoxGroup("Settings"), ShowIf("leapMotionEnabled"), Indent, Range(.01f, .05f), SerializeField] private float leapGestureThreshold = .05f;
        [BoxGroup("Settings"), ShowIf("leapMotionEnabled"), Indent, Range(.05f, .5f), SerializeField] private float leapControllerThreshold = .3f;
        [BoxGroup("Settings"), ShowIf("leapMotionEnabled"), Indent, SerializeField] private InteractionManager interactionManager;
        [BoxGroup("Settings"), SerializeField, Space(10)] public bool debugActive;
        [BoxGroup("Settings"), SerializeField, Space(10)] public bool directInteraction;
        [BoxGroup("Settings"), ShowIf("directInteraction"), Indent, Range(.01f, .05f), SerializeField] private float directDistance = .025f;
        [BoxGroup("Settings"), ShowIf("directInteraction"), Indent, Range(0, 31)] public int layerIndex = 9;
        
        [FoldoutGroup("Transforms"), SerializeField, Required] private Transform cameraRig;
        [FoldoutGroup("Transforms"), SerializeField, Space(10), Required, ShowIf("steamEnabled")] private Transform leftController;
        [FoldoutGroup("Transforms"), SerializeField, Required, ShowIf("steamEnabled")] private Transform rightController;
        [FoldoutGroup("Transforms"), SerializeField, Space(10), Required, ShowIf("leapMotionEnabled")] private Transform leftHand;
        [FoldoutGroup("Transforms"), SerializeField, Required, ShowIf("leapMotionEnabled")] private Transform rightHand;
        
        [FoldoutGroup("LeapMotion Hands"), SerializeField, Required, ShowIf("leapMotionEnabled")] public Transform rightThumb;
        [FoldoutGroup("LeapMotion Hands"), SerializeField, Required, ShowIf("leapMotionEnabled")] public Transform rightIndex;
        [FoldoutGroup("LeapMotion Hands"), SerializeField, Required, ShowIf("leapMotionEnabled")] public Transform rightMiddle;
        [FoldoutGroup("LeapMotion Hands"), SerializeField, Required, ShowIf("leapMotionEnabled")] public Transform rightRing;
        [FoldoutGroup("LeapMotion Hands"), SerializeField, Required, ShowIf("leapMotionEnabled")] public Transform rightLittle;
        [FoldoutGroup("LeapMotion Hands"), SerializeField, Required, ShowIf("leapMotionEnabled")] public Transform rightPalm;
        [FoldoutGroup("LeapMotion Hands"), SerializeField, Required, ShowIf("leapMotionEnabled")] public Transform rightWrist;
        [FoldoutGroup("LeapMotion Hands"), SerializeField, Required, ShowIf("leapMotionEnabled"), Space(10)] public Transform leftThumb;
        [FoldoutGroup("LeapMotion Hands"), SerializeField, Required, ShowIf("leapMotionEnabled")] public Transform leftIndex;
        [FoldoutGroup("LeapMotion Hands"), SerializeField, Required, ShowIf("leapMotionEnabled")] public Transform leftMiddle;
        [FoldoutGroup("LeapMotion Hands"), SerializeField, Required, ShowIf("leapMotionEnabled")] public Transform leftRing;
        [FoldoutGroup("LeapMotion Hands"), SerializeField, Required, ShowIf("leapMotionEnabled")] public Transform leftLittle;
        [FoldoutGroup("LeapMotion Hands"), SerializeField, Required, ShowIf("leapMotionEnabled")] public Transform leftPalm;
        [FoldoutGroup("LeapMotion Hands"), SerializeField, Required, ShowIf("leapMotionEnabled")] public Transform leftWrist;

        [FoldoutGroup("Aesthetics"),  SerializeField, Required] public Material lineRenderMat;
        [FoldoutGroup("Aesthetics"),  SerializeField, Required] public Material dottedLineRenderMat;
        [FoldoutGroup("Aesthetics"),  SerializeField, Required] public Material doubleSidedLineRenderMat;
        [FoldoutGroup("Aesthetics"),  SerializeField, Required] public Material voidSkyBox;
        [FoldoutGroup("Aesthetics"),  SerializeField, Required] public Material environmentSkyBox;
        [FoldoutGroup("Aesthetics"), Space(10), ShowIf("leapMotionEnabled"),  SerializeField, Required] public GameObject grabParticleSystem;
        [FoldoutGroup("Aesthetics"), ShowIf("leapMotionEnabled"),  SerializeField, Required] public GameObject selectParticleSystem;
        [FoldoutGroup("Aesthetics"), ShowIf("leapMotionEnabled"),  SerializeField, Required] public GameObject fingerTouchParticleSystem;
        
        [FoldoutGroup("Events"), ShowIf("steamEnabled")] public SteamVR_Action_Boolean grabGrip;
        [FoldoutGroup("Events"), ShowIf("steamEnabled")] public SteamVR_Action_Boolean triggerGrip;
        [FoldoutGroup("Events"), ShowIf("steamEnabled")] public SteamVR_Action_Boolean menu;
        [FoldoutGroup("Events"), ShowIf("steamEnabled")] public SteamVR_Action_Boolean joystickPress;
        [FoldoutGroup("Events"), ShowIf("steamEnabled")] public SteamVR_Action_Vector2 joystickDirection;
        [FoldoutGroup("Events"), ShowIf("steamEnabled")] public SteamVR_Action_Vibration haptic;
        [FoldoutGroup("Events"), Space(10), ShowIf("leapMotionEnabled"), Required] public HandEnableDisable leftHandEnabled;
        [FoldoutGroup("Events"), ShowIf("leapMotionEnabled"), Required] public HandEnableDisable rightHandEnabled;
        [FoldoutGroup("Events"), ShowIf("leapMotionEnabled"), Required] public CapsuleHand leftCapsuleHand;
        [FoldoutGroup("Events"), ShowIf("leapMotionEnabled"), Required] public CapsuleHand rightCapsuleHand;

        [HideInInspector] public UnityEvent lIndexSoloSelect;
        [HideInInspector] public UnityEvent lMiddleSoloSelect;
        [HideInInspector] public UnityEvent lRingSoloSelect;
        [HideInInspector] public UnityEvent lLittleSoloSelect;
        [HideInInspector] public UnityEvent lDualSelect;
        [HideInInspector] public UnityEvent lGrabSelect;
        [HideInInspector] public UnityEvent rIndexSoloSelect;
        [HideInInspector] public UnityEvent rMiddleSoloSelect;
        [HideInInspector] public UnityEvent rRingSoloSelect;
        [HideInInspector] public UnityEvent rLittleSoloSelect;
        [HideInInspector] public UnityEvent rDualSelect;
        [HideInInspector] public UnityEvent rGrabSelect;

        private ParticleSystem lIndexParticle;
        private ParticleSystem lMiddleParticle;
        private ParticleSystem lRingParticle;
        private ParticleSystem lLittleParticle;
        private ParticleSystem lSelectParticle;
        private ParticleSystem lGrabParticle;
        private ParticleSystem rIndexParticle;
        private ParticleSystem rMiddleParticle;
        private ParticleSystem rRingParticle;
        private ParticleSystem rLittleParticle;
        private ParticleSystem rSelectParticle;
        private ParticleSystem rGrabParticle;

        private GameObject lHandDirect;
        private GameObject rHandDirect;

        private GameObject localRef;
        private GameObject localHeadset;
        private GameObject localR;
        private GameObject localL;

        private GameObject stableRef;
        private GameObject lHandStable;
        private GameObject lPalmStable;
        private GameObject rHandStable;
        private GameObject rPalmStable;
        
        private GameObject armRef;
        private GameObject lElbow;
        private GameObject rElbow;
        private GameObject lAnchor;
        private GameObject rAnchor;

        private readonly List<Vector3> lPalmDirection = new List<Vector3>();
        private readonly List<Vector3> rPalmDirection = new List<Vector3>();
        
        private Arm lArm;
        private Arm rArm;
        
        private Hand lHand;
        private Hand rHand;

        public const string LTag = "Direct/Left";
        public const string RTag = "Direct/Right";

        private void Start()
        {
            SetupParticleSystem();
            SetupDirect();
            SetupLocal();
            SetupStable();
            leftHandEnabled.handEnabledEvent.AddListener(SetupLeftArm);
            rightHandEnabled.handEnabledEvent.AddListener(SetupRightArm);
        }

        private void SetupDirect()
        {
            lHandDirect = new GameObject(LTag);
            rHandDirect = new GameObject(RTag);
            lHandDirect.transform.SetParent(transform);
            rHandDirect.transform.SetParent(transform);
            lHandDirect.layer = layerIndex;
            rHandDirect.layer = layerIndex;
            SphereCollider ls = lHandDirect.AddComponent<SphereCollider>();
            SphereCollider rs = rHandDirect.AddComponent<SphereCollider>();
            ls.radius = directDistance;
            rs.radius = directDistance;
        }

        private void SetupLocal()
        {
            localRef = Set.NewGameObject(gameObject, "[Local Reference Rig]");
            localHeadset = Set.NewGameObject(localRef, "Local/HMD");
            localR = Set.NewGameObject(localHeadset,"Local/Right");
            localL = Set.NewGameObject(localHeadset,"Local/Left");
        }

        private void SetupStable()
        {
            stableRef = Set.NewGameObject(gameObject,"[Stable Rig]");
            lHandStable = Set.NewGameObject(stableRef,"Left_Hand/Stable");
            rHandStable = Set.NewGameObject(stableRef,"Right_Hand/Stable");
            lPalmStable = Set.NewGameObject(stableRef,"Left_Palm/Stable");
            rPalmStable = Set.NewGameObject(stableRef,"Right_Palm/Stable");
            
            armRef = Set.NewGameObject(gameObject,"[Leap Arm Rig]");
        }

        private void SetupLeftArm()
        {
            lHand = leftCapsuleHand._hand;
            lArm = lHand.Arm;
            
            lElbow = Set.NewGameObject(armRef,"Left_Elbow");
            lAnchor = Set.NewGameObject(armRef,"Left_Anchor");

            leftHandEnabled.handEnabledEvent.RemoveAllListeners();
        }
        
        private void SetupRightArm()
        {
            rHand = rightCapsuleHand._hand;
            rArm = rHand.Arm;
            
            rElbow = Set.NewGameObject(armRef,"Right_Elbow");
            rAnchor = Set.NewGameObject(armRef,"Right_Anchor");

            rightHandEnabled.handEnabledEvent.RemoveAllListeners();
        }

        private void SetupParticleSystem()
        {
            return;
            GameObject particle = Instantiate(fingerTouchParticleSystem, leftIndex);
            particle.transform.ResetLocalTransform();
            lIndexParticle = particle.GetComponent<ParticleSystem>();
            particle = Instantiate(fingerTouchParticleSystem, leftMiddle);
            particle.transform.ResetLocalTransform();
            lMiddleParticle = particle.transform.GetComponent<ParticleSystem>();
            particle = Instantiate(fingerTouchParticleSystem, leftRing);
            particle.transform.ResetLocalTransform();
            lRingParticle = particle.transform.GetComponent<ParticleSystem>();
            particle = Instantiate(fingerTouchParticleSystem, leftLittle);
            particle.transform.ResetLocalTransform();
            lLittleParticle = particle.transform.GetComponent<ParticleSystem>();

            particle = Instantiate(fingerTouchParticleSystem, rightIndex);
            particle.transform.ResetLocalTransform();
            rIndexParticle = particle.transform.GetComponent<ParticleSystem>();
            particle = Instantiate(fingerTouchParticleSystem, rightMiddle);
            particle.transform.ResetLocalTransform();
            rMiddleParticle = particle.transform.GetComponent<ParticleSystem>();
            particle = Instantiate(fingerTouchParticleSystem, rightRing);
            particle.transform.ResetLocalTransform();
            rRingParticle = particle.transform.GetComponent<ParticleSystem>();
            particle = Instantiate(fingerTouchParticleSystem, rightLittle);
            particle.transform.ResetLocalTransform();
            rLittleParticle = particle.transform.GetComponent<ParticleSystem>();
        }

        private void FixedUpdate()
        {
            lHandDirect.transform.Transforms(LeftTransform());
            rHandDirect.transform.Transforms(RightTransform());
            localRef.transform.SplitPositionVector(0, CameraTransform());
            localHeadset.transform.Transforms(CameraTransform());
            localR.transform.Transforms(RightTransform());
            localL.transform.Transforms(LeftTransform());

            lPalmStable.transform.StableTransforms(leftPalm, leapStabilisation);
            rPalmStable.transform.StableTransforms(rightPalm, leapStabilisation);

            Set.HandPosition(lElbow == null, lElbow, lArm, lHandStable, Set.FivePointMidpointPosition(leftIndex, leftMiddle, leftRing, leftLittle, leftWrist), lAnchor, leftThumb);  
            Set.HandPosition(rElbow == null || rightCapsuleHand._hand.IsLeft, rElbow, rArm, rHandStable, Set.FivePointMidpointPosition(rightIndex, rightMiddle, rightRing, rightLittle, rightWrist), rAnchor, rightThumb);
        }

        public GameObject Player()
        {
            return gameObject;
        }
        
        public Transform LeftTransform()
        {
            return LeftHandEnabled() && !LeftHandController() ? AdjustedLeftHandTransform() : leftController;
        }

        private Transform AdjustedLeftHandTransform()
        {
            return lHandStable.transform;
        }

        public Transform RightTransform()
        {
            return RightHandEnabled() && !RightHandController() ? AdjustedRightHandTransform() : rightController;
        }
        
        private Transform AdjustedRightHandTransform()
        {
            return rHandStable.transform;
        }
        
        public Vector3 LeftPosition()
        {
            return LeftTransform().position;
        }
    
        public Vector3 RightPosition()
        {
            return RightTransform().position;
        }
        
        public Vector3 LeftLocalPosition()
        {
            return LeftTransform().localPosition;
        }
        
        public Vector3 RightLocalPosition()
        {
            return RightTransform().localPosition;
        }
        
        public Transform HmdLocalRelativeTransform()
        {
            return localHeadset.transform;
        }
        public Transform LeftLocalRelativeTransform()
        {
            return localL.transform;
        }
        
        public Transform RightLocalRelativeTransform()
        {
            return localR.transform;
        }

        public float ControllerDistance()
        {
            return Vector3.Distance(RightPosition(), LeftPosition());
        }

        public Transform CameraTransform()
        {
            return cameraRig;
        }

        public Vector3 CameraPosition()
        {
            return cameraRig.position;
        }
        
        public Vector3 CameraLocalPosition()
        {
            return cameraRig.localPosition;
        }

        public bool LeftGrab()
        {
            return LeftHandEnabled() && !LeftHandController() 
                ? LeftHandGrab() 
                : grabGrip.GetState(SteamVR_Input_Sources.LeftHand);
        }

        private bool LeftHandGrab()
        {
            //return LeftIndexSoloSelect();
            //return leftThumb.DualSelect(leftIndex, leftMiddle, leapGestureThreshold);
            return leftPalm.Grab(leftIndex, leftMiddle, leftRing, leftLittle, leapGestureThreshold);
        }
    
        public bool RightGrab()
        {
            return RightHandEnabled() && !RightHandController() 
                ? RightHandGrab() 
                : grabGrip.GetState(SteamVR_Input_Sources.RightHand);
        }
        
        private bool RightHandGrab()
        {
            //return RightIndexSoloSelect();
            //return rightThumb.DualSelect(rightIndex, rightMiddle, leapGestureThreshold);
            return rightHand.Grab(rightIndex, rightMiddle, leftRing, leftLittle, leapGestureThreshold);
        }
        
        public bool LeftMenu()
        {
            return LeftHandEnabled() && !LeftHandController()
                ? LeftMiddleSoloSelect()
                : menu.GetState(SteamVR_Input_Sources.LeftHand);
        }
    
        public bool RightMenu()
        {
            return RightHandEnabled() && !RightHandController()
                ? RightMiddleSoloSelect()
                : menu.GetState(SteamVR_Input_Sources.RightHand);
        }

        public bool LeftSelect()
        {
            return LeftHandEnabled() && !LeftHandController()
                ? LeftIndexSoloSelect()
                : triggerGrip.GetState(SteamVR_Input_Sources.LeftHand);
        }
        
        public bool RightSelect()
        {
            return RightHandEnabled() && !RightHandController()
                ? RightIndexSoloSelect()
                : triggerGrip.GetState(SteamVR_Input_Sources.RightHand);
        }

        public bool LeftIndexSelect()
        {
            return leftThumb.Select(leftIndex, leapGestureThreshold);
        }

        public bool LeftIndexSoloSelect()
        {
            bool state = LeftIndexSelect() && !(LeftMiddleSelect() || LeftRingSelect() || LeftLittleSelect());
            //if (state) lIndexParticle.Play();// lIndexSoloSelect.Invoke();
            return state;
        }
        
        public bool LeftMiddleSelect()
        {
            return leftThumb.Select(leftMiddle, leapGestureThreshold);
        }
        
        public bool LeftMiddleSoloSelect()
        {
            bool state = LeftMiddleSelect() && !(LeftIndexSelect() || LeftRingSelect() || LeftLittleSelect());
            //if (state) lMiddleParticle.Play();//lMiddleSoloSelect.Invoke();
            return state;
        }
        
        public bool LeftRingSelect()
        {
            return leftThumb.Select(leftRing, leapGestureThreshold);
        }
        
        public bool LeftRingSoloSelect()
        {
            bool state = LeftRingSelect() && !(LeftIndexSelect() || LeftMiddleSelect() || LeftLittleSelect());
            //if (state) lRingParticle.Play();//lRingSoloSelect.Invoke();
            return state;
        }
        
        public bool LeftLittleSelect()
        {
            return leftThumb.Select(leftLittle, leapGestureThreshold);
        }
        
        public bool LeftLittleSoloSelect()
        {
            bool state = LeftLittleSelect() && !(LeftIndexSelect() || LeftMiddleSelect() || LeftRingSelect());
            //if (state) lLittleParticle.Play();//lLittleSoloSelect.Invoke();
            return state;
        }
        
        public bool RightIndexSelect()
        {
            return rightThumb.Select(rightIndex, leapGestureThreshold);
        }

        public bool RightIndexSoloSelect()
        {
            bool state = RightIndexSelect() && !(RightMiddleSelect() || RightRingSelect() || RightLittleSelect());
            //if (state) rIndexParticle.Play();//rIndexSoloSelect.Invoke();
            return state;
        }
        
        public bool RightMiddleSelect()
        {
            return rightThumb.Select(rightMiddle, leapGestureThreshold);
        }
        
        public bool RightMiddleSoloSelect()
        {
            bool state = RightMiddleSelect() && !(RightIndexSelect() || RightRingSelect() || RightLittleSelect());
            //if (state) rMiddleParticle.Play();//rMiddleSoloSelect.Invoke();
            return state;
        }
        
        public bool RightRingSelect()
        {
            return rightThumb.Select(rightRing, leapGestureThreshold);
        }
        
        public bool RightRingSoloSelect()
        {
            bool state = RightRingSelect() && !(RightIndexSelect() || RightMiddleSelect() || RightLittleSelect());
            //if (state) rRingParticle.Play();//rRingSoloSelect.Invoke();
            return state;
        }
        
        public bool RightLittleSelect()
        {
            return rightThumb.Select(rightLittle, leapGestureThreshold);
        }
        
        public bool RightLittleSoloSelect()
        {
            bool state = RightLittleSelect() && !(RightIndexSelect() || RightMiddleSelect() || RightRingSelect());
            //if (state) rLittleParticle.Play();//rLittleSoloSelect.Invoke();
            return state;
        }

        public bool LeftLocomotion()
        {
            return LeftHandEnabled() && !LeftHandController() ? leftPalm.PalmDown(lPalmDirection, 5f, 30) : LeftJoystickPress();
        }

        public bool RightLocomotion()
        {
            return RightHandEnabled() && !RightHandController() ? rightPalm.PalmDown(rPalmDirection, 5f, 30) : RightJoystickPress();
        }

        private bool LeftJoystickPress()
        {
            return joystickPress.GetState(SteamVR_Input_Sources.LeftHand);
        }
        
        private bool RightJoystickPress()
        {
            return joystickPress.GetState(SteamVR_Input_Sources.RightHand);
        }

        public Vector2 LeftJoystick()
        {
            return LeftHandEnabled() && !LeftHandController() ? new Vector2(1, 0) : joystickDirection.GetAxis(SteamVR_Input_Sources.LeftHand);
        }

        public Vector2 RightJoystick()
        {
            return RightHandEnabled() && !RightHandController() ? new Vector2(1, 0) : joystickDirection.GetAxis(SteamVR_Input_Sources.RightHand);
        }
        
        public Vector3 LeftForwardVector()
        {
            return leftHandEnabled && !LeftHandController() ? AdjustedForward(CameraTransform(), RightTransform()) : LeftTransform().TransformVector(Vector3.forward);
        }
    
        public Vector3 RightForwardVector()
        {
            return RightHandEnabled() && !RightHandController() ? AdjustedForward(CameraTransform(), RightTransform()) : RightTransform().TransformVector(Vector3.forward);
        }

        private static Vector3 AdjustedForward(Transform head, Transform hand)
        {
            return hand.forward;
        }

        public Vector3 CameraForwardVector()
        {
            return cameraRig.forward;
        }

        public static SteamVR_Input_Sources LeftSource()
        {
            return SteamVR_Input_Sources.LeftHand;
        }
        
        public static SteamVR_Input_Sources RightSource()
        {
            return SteamVR_Input_Sources.RightHand;
        }
        
        public bool LeftHandEnabled()
        {
            return leftHandEnabled.handEnabled && leapMotionEnabled && lHand.IsLeft;
        }

        private bool LeftHandController()
        {
            return leftHand.TransformDistanceCheck(leftController, leapControllerThreshold);
        }
        
        public bool RightHandEnabled()
        {
            return rightHandEnabled.handEnabled && leapMotionEnabled && rHand.IsRight;
        }
        
        private bool RightHandController()
        {
            return rightHand.TransformDistanceCheck(rightController, leapControllerThreshold);
        }

        public InteractionManager InteractionManager()
        {
            return interactionManager;
        }
    }
}

