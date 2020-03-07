using System;
using System.Collections.Generic;
using Delayed_Messaging.Scripts.Utilities;
using UnityEngine;
using UnityEngine.Events;
using Valve.VR;
using VR_Prototyping.Scripts.Utilities;

namespace VR_Prototyping.Scripts
{
    public class ControllerTransforms : MonoBehaviour
    {
        [SerializeField] public bool steamEnabled;
        [SerializeField, Space(10)] public bool debugActive;
        public enum DebugType { NEVER, SELECTED_ONLY, ALWAYS }
        [SerializeField, Space(10)] public bool directInteraction;
        [Range(.01f, .05f), SerializeField] private float directDistance = .025f;
        [Range(0, 31)] public int layerIndex = 9;
        
        [SerializeField, Space(10)] private Transform cameraRig;
        [SerializeField, Space(10)] private Transform leftController;
        [SerializeField] private Transform rightController;

        [SerializeField] public Material lineRenderMaterial;

        [Header("SteamVR Actions")]
        public SteamVR_Action_Boolean grab;
        public SteamVR_Action_Boolean trigger;
        public SteamVR_Action_Boolean menu;
        public SteamVR_Action_Boolean joystick;
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
        
        public const float MaxAngle = 110f;
        public const float MinAngle = 60f;
        
        public const float Trigger = .7f;
        public const float Sensitivity = 10f;
        public const float Tolerance = .1f;

        public readonly List<Vector2> rJoystickValues = new List<Vector2>();
        public readonly List<Vector2> lJoystickValues = new List<Vector2>();
        
        public struct CastGameObjects
        {
            public GameObject parent;
            public GameObject cN;
            
            public GameObject rCf; // follow
            public GameObject rCp; // proxy
            public GameObject rCn; // normalised
            public GameObject rMp; // midpoint
            public GameObject rTs; // target
            public GameObject rHp; // hit
            public GameObject rVisual; // visual
            
            public GameObject rRt; // rotation
            public GameObject lCf; // follow
            public GameObject lCp; // proxy
            public GameObject lCn; // normalised
            public GameObject lMp; // midpoint
            public GameObject lTs; // target
            public GameObject lHp; // hit
            public GameObject lVisual; // visual
            public GameObject lRt; // rotation
            
            public Vector3 rLastValidPosition;
            public Vector3 lLastValidPosition;
        }
        
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

        private void Update()
        {
            rJoystickValues.Vector2ListCull(RightJoystick(), Sensitivity);
            lJoystickValues.Vector2ListCull(LeftJoystick(), Sensitivity);
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
            return grab.GetState(SteamVR_Input_Sources.LeftHand);
        }

        public bool RightGrab()
        {
            return grab.GetState(SteamVR_Input_Sources.RightHand);
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
            return trigger.GetState(SteamVR_Input_Sources.LeftHand);
        }
        
        public bool RightSelect()
        {
            return trigger.GetState(SteamVR_Input_Sources.RightHand);
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
            return joystick.GetState(SteamVR_Input_Sources.LeftHand);
        }
        
        private bool RightJoystickPress()
        {
            return joystick.GetState(SteamVR_Input_Sources.RightHand);
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
        
        public void SetupCastObjects(CastGameObjects castGameObjects, GameObject targetVisual, string instanceName, bool startActive = false)
        {
            castGameObjects.parent = new GameObject("[" + instanceName + "/Calculations]");
            Transform parentTransform = castGameObjects.parent.transform;
            parentTransform.SetParent(transform);

            castGameObjects.rCf = new GameObject("[" + instanceName + "/Follow/Right]");
            castGameObjects.rCp = new GameObject("[" + instanceName + "/Proxy/Right]");
            castGameObjects.rCn = new GameObject("[" + instanceName + "/Normalised/Right]");
            castGameObjects.rMp = new GameObject("[" + instanceName + "/MidPoint/Right]");
            castGameObjects.rTs = new GameObject("[" + instanceName + "/Target/Right]");
            castGameObjects.rHp = new GameObject("[" + instanceName + "/HitPoint/Right]");
            castGameObjects.rRt = new GameObject("[" + instanceName + "/Rotation/Right]");
            
            castGameObjects.lCf = new GameObject("[" + instanceName + "/Follow/Left]");
            castGameObjects.lCp = new GameObject("[" + instanceName + "/Proxy/Left]");
            castGameObjects.lCn = new GameObject("[" + instanceName + "/Normalised/Left]");
            castGameObjects.lMp = new GameObject("[" + instanceName + "/MidPoint/Left]");
            castGameObjects.lTs = new GameObject("[" + instanceName + "/Target/Left]");
            castGameObjects.lHp = new GameObject("[" + instanceName + "/HitPoint/Left]");
            castGameObjects.lRt = new GameObject("[" + instanceName + "/Rotation/Left]");
            
            castGameObjects.rVisual = Instantiate(targetVisual, castGameObjects.rHp.transform);
            castGameObjects.rVisual.name = "[" + instanceName + "/Visual/Right]";
            castGameObjects.rVisual.SetActive(startActive);
            
            castGameObjects.lVisual = Instantiate(targetVisual, castGameObjects.lHp.transform);
            castGameObjects.lVisual.name = "[" + instanceName + "/Visual/Left]";
            castGameObjects.lVisual.SetActive(startActive);

            castGameObjects.rCf.transform.SetParent(parentTransform);
            castGameObjects.rCp.transform.SetParent(castGameObjects.rCf.transform);
            castGameObjects.rCn.transform.SetParent(castGameObjects.rCf.transform);
            castGameObjects.rMp.transform.SetParent(castGameObjects.rCp.transform);
            castGameObjects.rTs.transform.SetParent(castGameObjects.rCn.transform);
            castGameObjects.rHp.transform.SetParent(transform);
            castGameObjects.rRt.transform.SetParent(castGameObjects.rHp.transform);
            
            castGameObjects.lCf.transform.SetParent(parentTransform);
            castGameObjects.lCp.transform.SetParent(castGameObjects.lCf.transform);
            castGameObjects.lCn.transform.SetParent(castGameObjects.lCf.transform);
            castGameObjects.lMp.transform.SetParent(castGameObjects.lCp.transform);
            castGameObjects.lTs.transform.SetParent(castGameObjects.lCn.transform);
            castGameObjects.lHp.transform.SetParent(transform);
            castGameObjects.lRt.transform.SetParent(castGameObjects.lHp.transform);
        }
    }
}