using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using Valve.VR;

namespace VR_Prototyping.Scripts
{
    public class ControllerTransforms : MonoBehaviour
    {
        [SerializeField] public bool steamEnabled;
        [SerializeField, Space(10)] public bool debugActive;
        [SerializeField, Space(10)] public bool directInteraction;
        [Range(.01f, .05f), SerializeField] private float directDistance = .025f;
        [Range(0, 31)] public int layerIndex = 9;
        
        [SerializeField, Space(10)] private Transform cameraRig;
        [SerializeField, Space(10)] private Transform leftController;
        [SerializeField] private Transform rightController;

        [SerializeField] public Material lineRenderMaterial;

        public SteamVR_Action_Boolean grabGrip;
        public SteamVR_Action_Boolean triggerGrip;
        public SteamVR_Action_Boolean menu;
        public SteamVR_Action_Boolean joystickPress;
        public SteamVR_Action_Vector2 joystickDirection;
        public SteamVR_Action_Vibration haptic;

        private GameObject lHandDirect;
        private GameObject rHandDirect;

        private GameObject localRef;
        private GameObject localHeadset;
        private GameObject localR;
        private GameObject localL;

        public const string LTag = "Direct/Left";
        public const string RTag = "Direct/Right";

        private void Start()
        {
            SetupDirect();
            SetupLocal();
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

        private void FixedUpdate()
        {
            localRef.transform.SplitPositionVector(0, CameraTransform());
            localHeadset.transform.Transforms(CameraTransform());
            localR.transform.Transforms(RightTransform());
            localL.transform.Transforms(LeftTransform());
        }

        public GameObject Player()
        {
            return gameObject;
        }
        
        public Transform LeftTransform()
        {
            return leftController;
        }

        public Transform RightTransform()
        {
            return rightController;
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
            return grabGrip.GetState(SteamVR_Input_Sources.LeftHand);
        }

        public bool RightGrab()
        {
            return grabGrip.GetState(SteamVR_Input_Sources.RightHand);
        }

        public bool LeftMenu()
        {
            return menu.GetState(SteamVR_Input_Sources.LeftHand);
        }
    
        public bool RightMenu()
        {
            return menu.GetState(SteamVR_Input_Sources.RightHand);
        }

        public bool LeftSelect()
        {
            return triggerGrip.GetState(SteamVR_Input_Sources.LeftHand);
        }
        
        public bool RightSelect()
        {
            return triggerGrip.GetState(SteamVR_Input_Sources.RightHand);
        }

        public bool LeftLocomotion()
        {
            return LeftJoystickPress();
        }

        public bool RightLocomotion()
        {
            return RightJoystickPress();
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
            return joystickDirection.GetAxis(SteamVR_Input_Sources.LeftHand);
        }

        public Vector2 RightJoystick()
        {
            return joystickDirection.GetAxis(SteamVR_Input_Sources.RightHand);
        }
        
        public Vector3 LeftForwardVector()
        {
            return LeftTransform().TransformVector(Vector3.forward);
        }
    
        public Vector3 RightForwardVector()
        {
            return RightTransform().TransformVector(Vector3.forward);
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
    }
}