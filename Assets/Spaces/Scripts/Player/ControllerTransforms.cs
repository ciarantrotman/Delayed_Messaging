using System;
using System.Collections.Generic;
using Delayed_Messaging.Scripts.Utilities;
using Spaces.Scripts.Utilities;
using UnityEngine;
using UnityEngine.Events;
using Valve.VR;

namespace Delayed_Messaging.Scripts.Player
{
    public class ControllerTransforms : MonoBehaviour
    {
        [SerializeField, Space(10)] public bool debugActive;
        public enum DebugType { NEVER, SELECTED_ONLY, ALWAYS }

        [SerializeField, Space(10)] private Transform cameraRig;
        [SerializeField, Space(10)] private Transform leftController;
        [SerializeField] private Transform rightController;

        [Header("SteamVR Actions")]
        public SteamVR_Action_Vector2 joystickDirection;
        public SteamVR_Action_Boolean grab, trigger, menu, joystick;

        [HideInInspector] public EventTracker leftGrab, rightGrab, leftSelect, rightSelect, leftMenu, rightMenu, leftJoystick, rightJoystick; 

        private GameObject lHandDirect, rHandDirect, localRef, localHeadset, localR, localL, controllerMidpoint;
        public const float MaxAngle = 110f, MinAngle = 60f, Trigger = .7f, Sensitivity = 10f, Tolerance = .1f;
        public readonly List<Vector2> rJoystickValues = new List<Vector2>(), lJoystickValues = new List<Vector2>();

        [Serializable] public struct EventTracker
        {
            public enum EventType { START, STAY, END }
            
            private bool current;
            private bool previous;

            public UnityEvent onStart;
            public UnityEvent onStay;
            public UnityEvent onEnd;

            public void CheckState(bool currentState)
            {
                current = currentState;

                if (current && !previous)
                {
                    onStart.Invoke();
                }
                if (current && previous)
                {
                    onStay.Invoke();
                }
                if (!current && previous)
                {
                    onEnd.Invoke();
                }

                previous = current;
            }

            public UnityEvent Event(EventType eventType)
            {
                switch (eventType)
                {
                    case EventType.START:
                        return onStart;
                    case EventType.STAY:
                        return onStay;
                    case EventType.END:
                        return onEnd;
                    default:
                        return null;
                }
            }
        }
        
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
            
            controllerMidpoint = Set.Object(gameObject, "[Controller Midpoint]");
        }

        private void SetupDirect()
        {
            /*
            DirectInteraction direct;
            // Left
            direct = Transform(Check.LEFT).gameObject.AddComponent<DirectInteraction>();
            direct.Initialise(this, Check.LEFT, directDistance);
            
            // Right
            direct = Transform(Check.RIGHT).gameObject.AddComponent<DirectInteraction>();
            direct.Initialise(this, Check.RIGHT, directDistance);
            */
        }

        private void SetupLocal()
        {
            localRef = Set.Object(gameObject, "[Local Reference Rig]");
            localHeadset = Set.Object(localRef, "Local/HMD");
            localR = Set.Object(localHeadset,"Local/Right");
            localL = Set.Object(localHeadset,"Local/Left");
        }

        private void Update()
        {
            rJoystickValues.Vector2ListCull(JoyStick(Check.RIGHT), Sensitivity);
            lJoystickValues.Vector2ListCull(JoyStick(Check.LEFT), Sensitivity);
            
            leftGrab.CheckState(Grab(Check.LEFT));
            rightGrab.CheckState(Grab(Check.RIGHT));
            
            leftSelect.CheckState(Select(Check.LEFT));
            rightSelect.CheckState(Select(Check.RIGHT));
            
            leftMenu.CheckState(Menu(Check.LEFT));
            rightMenu.CheckState(Menu(Check.RIGHT));
            
            leftJoystick.CheckState(Joystick(Check.LEFT));
            rightJoystick.CheckState(Joystick(Check.RIGHT));
        }
        
        private void FixedUpdate()
        {
            localRef.transform.SplitPositionVector(0, CameraTransform());
            localHeadset.transform.Transforms(CameraTransform());
            localR.transform.Transforms(RightTransform());
            localL.transform.Transforms(LeftTransform());
            
            // Set the controller midpoint
            controllerMidpoint.transform.position = Vector3.Lerp(Position(Check.LEFT), Position(Check.RIGHT), .5f);
            controllerMidpoint.transform.LookAt(Position(Check.RIGHT));
            //controllerMidpoint.transform.rotation = Quaternion.Lerp(Transform(Check.LEFT).rotation, Transform(Check.RIGHT).rotation, .5f);
        }

        #region Private Accessors

        private Transform LeftTransform()
        {
            return leftController;
        }

        private Transform RightTransform()
        {
            return rightController;
        }

        private Vector3 LeftPosition()
        {
            return LeftTransform().position;
        }
    
        private Vector3 RightPosition()
        {
            return RightTransform().position;
        }
        
        private Vector3 LeftLocalPosition()
        {
            return LeftTransform().localPosition;
        }
        
        private Vector3 RightLocalPosition()
        {
            return RightTransform().localPosition;
        }
        
        private Transform HmdLocalRelativeTransform()
        {
            return localHeadset.transform;
        }
        private Transform LeftLocalRelativeTransform()
        {
            return localL.transform;
        }
        
        private Transform RightLocalRelativeTransform()
        {
            return localR.transform;
        }

        public float ControllerDistance()
        {
            return Vector3.Distance(RightPosition(), LeftPosition());
        }

        private Transform CameraTransform()
        {
            return cameraRig;
        }

        private Vector3 CameraPosition()
        {
            return cameraRig.position;
        }

        private Vector3 CameraLocalPosition()
        {
            return cameraRig.localPosition;
        }

        private bool LeftGrab()
        {
            return grab.GetState(SteamVR_Input_Sources.LeftHand);
        }

        private bool RightGrab()
        {
            return grab.GetState(SteamVR_Input_Sources.RightHand);
        }

        private bool LeftMenu()
        {
            return menu.GetState(SteamVR_Input_Sources.LeftHand);
        }
    
        private bool RightMenu()
        {
            return menu.GetState(SteamVR_Input_Sources.RightHand);
        }

        private bool LeftSelect()
        {
            return trigger.GetState(SteamVR_Input_Sources.LeftHand);
        }
        
        private bool RightSelect()
        {
            return trigger.GetState(SteamVR_Input_Sources.RightHand);
        }

        private bool LeftLocomotion()
        {
            return LeftJoystickPress();
        }

        private bool RightLocomotion()
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

        private Vector2 LeftJoystick()
        {
            return joystickDirection.GetAxis(SteamVR_Input_Sources.LeftHand);
        }

        private Vector2 RightJoystick()
        {
            return joystickDirection.GetAxis(SteamVR_Input_Sources.RightHand);
        }
        
        private Vector3 LeftForwardVector()
        {
            return LeftTransform().TransformVector(Vector3.forward);
        }
    
        private Vector3 RightForwardVector()
        {
            return RightTransform().TransformVector(Vector3.forward);
        }

        private static Vector3 AdjustedForward(Transform head, Transform hand)
        {
            return hand.forward;
        }

        private Vector3 CameraForwardVector()
        {
            return cameraRig.forward;
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

        #endregion

        public enum Check { LEFT, RIGHT, HEAD }

        public Transform RelativeTransform(Check check)
        {
            switch (check)
            {
                case Check.LEFT:
                    return LeftLocalRelativeTransform();
                case Check.RIGHT:
                    return RightLocalRelativeTransform();
                case Check.HEAD:
                    return HmdLocalRelativeTransform();
                default:
                    return HmdLocalRelativeTransform();
            }
        }
        public Transform Transform(Check check)
        {
            switch (check)
            {
                case Check.LEFT:
                    return LeftTransform();
                case Check.RIGHT:
                    return RightTransform();
                case Check.HEAD:
                    return CameraTransform();
                default:
                    return CameraTransform();
            }
        }
        public Transform ControllerCenter()
        {
            return controllerMidpoint.transform;
        }
        public Vector2 JoyStick(Check check)
        {
            switch (check)
            {
                case Check.LEFT:
                    return LeftJoystick();
                case Check.RIGHT:
                    return RightJoystick();
                case Check.HEAD:
                    return Vector2.zero;
                default:
                    return Vector2.zero;
            }
        }
        public bool Select(Check check)
        {
            switch (check)
            {
                case Check.LEFT:
                    return LeftSelect();
                case Check.RIGHT:
                    return RightSelect();
                case Check.HEAD:
                    return false;
                default:
                    return false;
            }
        }
        public UnityEvent SelectEvent(Check check, EventTracker.EventType eventType)
        {
            switch (check)
            {
                case Check.LEFT:
                    return leftSelect.Event(eventType);
                case Check.RIGHT:
                    return rightSelect.Event(eventType);
                case Check.HEAD:
                    return null;
                default:
                    return null;
            }
        }
        public bool Grab(Check check)
        {
            switch (check)
            {
                case Check.LEFT:
                    return LeftGrab();
                case Check.RIGHT:
                    return RightGrab();
                case Check.HEAD:
                    return false;
                default:
                    return false;
            }
        }
        public UnityEvent GrabEvent(Check check, EventTracker.EventType eventType)
        {
            switch (check)
            {
                case Check.LEFT:
                    return leftGrab.Event(eventType);
                case Check.RIGHT:
                    return rightGrab.Event(eventType);
                case Check.HEAD:
                    return null;
                default:
                    return null;
            }
        }
        public bool Menu(Check check)
        {
            switch (check)
            {
                case Check.LEFT:
                    return LeftMenu();
                case Check.RIGHT:
                    return RightMenu();
                case Check.HEAD:
                    return false;
                default:
                    return false;
            }
        }
        public UnityEvent MenuEvent(Check check, EventTracker.EventType eventType)
        {
            switch (check)
            {
                case Check.LEFT:
                    return leftMenu.Event(eventType);
                case Check.RIGHT:
                    return rightMenu.Event(eventType);
                case Check.HEAD:
                    return null;
                default:
                    return null;
            }
        }
        public Vector3 ForwardVector(Check check)
        {
            switch (check)
            {
                case Check.LEFT:
                    return LeftForwardVector();
                case Check.RIGHT:
                    return RightForwardVector();
                case Check.HEAD:
                    return CameraForwardVector();
                default:
                    return Vector3.zero;
            }
        }
        public Vector3 Position(Check check)
        {
            switch (check)
            {
                case Check.LEFT:
                    return LeftPosition();
                case Check.RIGHT:
                    return RightPosition();
                case Check.HEAD:
                    return CameraPosition();
                default:
                    return Vector3.zero;
            }
        }
        public bool Joystick(Check check)
        {
            switch (check)
            {
                case Check.LEFT:
                    return LeftJoystickPress();
                case Check.RIGHT:
                    return RightJoystickPress();
                case Check.HEAD:
                    return false;
                default:
                    return false;
            }
        }
        public UnityEvent JoystickEvent(Check check, EventTracker.EventType eventType)
        {
            switch (check)
            {
                case Check.LEFT:
                    return leftJoystick.Event(eventType);
                case Check.RIGHT:
                    return rightJoystick.Event(eventType);
                case Check.HEAD:
                    return null;
                default:
                    return null;
            }
        }
        public Vector3 LocalPosition(Check check)
        {
            switch (check)
            {
                case Check.LEFT:
                    return LeftLocalPosition();
                case Check.RIGHT:
                    return RightLocalPosition();
                case Check.HEAD:
                    return CameraLocalPosition();
                default:
                    return Vector3.zero;
            }
        }
    }
}