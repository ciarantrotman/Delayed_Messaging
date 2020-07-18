using System;
using System.Collections;
using Delayed_Messaging.Scripts.Player;
using Delayed_Messaging.Scripts.Player.Locomotion;
using Delayed_Messaging.Scripts.Utilities;
using DG.Tweening;
using Grapple.Scripts.Player;
using Grapple.Scripts.User_Interface;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Grapple.Scripts.Vehicle
{
    public class VehicleController : MonoBehaviour
    {
        #region Variables and References

        // Inspector Variables
        [SerializeField] private Transform interiorPosition, exteriorPosition;
        [SerializeField] private Transform leftDoor, rightDoor, leftWheels, rightWheels, steeringWheelRest;
        [SerializeField] private TextMeshPro readout;
        [SerializeField] private GameObject steeringWheel;
        [SerializeField] private Button driverDoorExternal, driverDoorInternal, powerButton;
        [SerializeField, Range(0, 180f)] private float doorOpenAngle;
        [SerializeField, Range(0, 5)] private float doorOpenSpeed;
        [SerializeField, Range(0, 180f)] private float wheelUpAngle;
        [SerializeField, Range(0, 5)] private float wheelUpSpeed;
        [SerializeField, Range(0, 5)] private float enterSpeed, exitSpeed;
        
        // Private Variables
        private enum VehicleState { ON, OFF }
        private VehicleState state = VehicleState.OFF;
        private enum AnimationState { UP, DOWN }
        private enum Side { LEFT, RIGHT, BOTH }
        private ControllerTransforms controller;
        private PlayerController playerController;
        
        public float height = 10f, thrust = 10f;
        public float hoverForce = 10f;

        #region Adjusted Variables

        private Vector3 DoorOpenRotation => new Vector3(0,0,doorOpenAngle);
        private static Vector3 DoorCloseRotation => Vector3.zero;  
        private float DoorOpenSpeed => doorOpenSpeed;
        private float DoorCloseSpeed => doorOpenSpeed * .75f;
        private Vector3 WheelUpRotation => new Vector3(0,0,wheelUpAngle);
        private static Vector3 WheelDownRotation => Vector3.zero;
        private float WheelUpSpeed => wheelUpSpeed;
        private float WheelDownSpeed => wheelUpSpeed * .75f;

        #endregion

        #endregion

        #region Coroutine Wrappers

        private void EnterVehicle()
        {
            StartCoroutine(Enter());
        }
        private void ExitVehicle()
        {
            StartCoroutine(Exit());
        }

        #endregion
        
        /// <summary>
        /// Coroutine containing the logic to move you into the vehicle
        /// </summary>
        /// <returns></returns>
        private IEnumerator Enter()
        {
            SetDoorState(Side.LEFT, AnimationState.UP);
            playerController.ChangePlayerState(PlayerController.PlayerState.VEHICLE);
            yield return new WaitForSeconds(DoorOpenSpeed);
            playerController.MovePlayer(interiorPosition, Locomotion.Method.DASH, enterSpeed);
            yield return new WaitForSeconds(enterSpeed);
            transform.SetParent(controller.transform);
            SetDoorState(Side.BOTH, AnimationState.DOWN);
        }
        /// <summary>
        /// Coroutine containing the logic to move you out of the vehicle
        /// </summary>
        /// <returns></returns>
        private IEnumerator Exit()
        {
            SetDoorState(Side.LEFT, AnimationState.UP);
            yield return new WaitForSeconds(DoorCloseSpeed);
            transform.SetParent(null);
            playerController.MovePlayer(exteriorPosition, Locomotion.Method.DASH, exitSpeed);
            yield return new WaitForSeconds(exitSpeed);
            playerController.ChangePlayerState(PlayerController.PlayerState.PEDESTRIAN);
            SetDoorState(Side.BOTH, AnimationState.DOWN);
        }
        
        private void Power()
        {
            switch (state)
            {
                // This means the vehicle is on, so you turn it off
                case VehicleState.ON:
                    PowerOff();
                    break;
                // This means the vehicle is off, so you turn it on
                case VehicleState.OFF:
                    PowerOn();
                    break;
                default:
                    return;
            }
        }

        private void PowerOn()
        {
            state = VehicleState.ON;
            
            SetWheelState(AnimationState.UP);
            readout.SetText("ON");
            playerController.playerRigidbody.useGravity = true;
        }
        
        private void PowerOff()
        {
            state = VehicleState.OFF;
            
            SetWheelState(AnimationState.DOWN);
            readout.SetText("");
            playerController.playerRigidbody.useGravity = false;
            playerController.playerRigidbody.drag = 10f;
            playerController.playerRigidbody.angularDrag = 10f;
        }

        private void SetVehicleVector()
        {
            switch (state)
            {
                case VehicleState.ON:
                    SetReadout();
                    steeringWheel.transform.LerpTransform(controller.ControllerCenter(), .75f);
                    break;
                case VehicleState.OFF:
                    steeringWheel.transform.LerpTransform(steeringWheelRest, .75f);
                    break;
                default:
                    return;
            }
        }

        private void Hover()
        {
            if (state == VehicleState.OFF) return;
            
            float proportionalHeight = (height - Height()) / height;
            Vector3 appliedHoverForce = Vector3.up * (proportionalHeight * hoverForce);
            playerController.playerRigidbody.AddForce(appliedHoverForce, ForceMode.Acceleration);
            
            Debug.DrawRay(transform.position, appliedHoverForce, Color.yellow);
        }

        private void Thrust()
        {
            if (state == VehicleState.OFF) return;

            playerController.playerRigidbody.AddRelativeForce(ThrustVector(), ForceMode.Acceleration);
            playerController.playerRigidbody.AddRelativeTorque(ThrustVector(), ForceMode.Acceleration);
            Debug.DrawRay(transform.position, ThrustVector(), Color.red);
        }
        
        private float Height()
        {
            return transform.position.y;
        }

        private Vector3 ThrustVector()
        {
            return (steeringWheelRest.position - steeringWheel.transform.position).normalized * thrust;
        }
        

        /// <summary>
        /// Sets the values of the vehicle based on the rigidbody states
        /// </summary>
        private void SetReadout()
        {
            readout.SetText($"Speed:  <b>{Math.Round(playerController.playerRigidbody.velocity.magnitude, 2)}</b> \n" +
                            $"Height: <b>{Math.Round(Height(), 2)}</b> \n" +
                            $"Fuel:   <b>{0f}</b> \n");
        }
        /// <summary>
        /// Triggers the animation of the doors of the vehicle depending on the input states
        /// </summary>
        /// <param name="side"></param>
        /// <param name="animationState"></param>
        private void SetDoorState(Side side, AnimationState animationState)
        {
            switch (side)
            {
                // Left door
                case Side.LEFT when animationState == AnimationState.UP:
                    AnimationController(leftDoor, -DoorOpenRotation, DoorOpenSpeed);
                    return;
                case Side.LEFT when animationState == AnimationState.DOWN:
                    AnimationController(leftDoor, DoorCloseRotation, DoorCloseSpeed);
                    return;
                // Right door
                case Side.RIGHT when animationState == AnimationState.UP:
                    AnimationController(rightDoor, DoorOpenRotation, DoorOpenSpeed);
                    return;
                case Side.RIGHT when animationState == AnimationState.DOWN:
                    AnimationController(rightDoor, DoorCloseRotation, DoorCloseSpeed);
                    return;
                // Both doors
                case Side.BOTH when animationState == AnimationState.UP:
                    AnimationController(leftDoor, -DoorOpenRotation, DoorOpenSpeed);
                    AnimationController(rightDoor, DoorOpenRotation, DoorOpenSpeed);
                    return;
                case Side.BOTH when animationState == AnimationState.DOWN:
                    AnimationController(leftDoor, DoorCloseRotation, DoorCloseSpeed);
                    AnimationController(rightDoor, DoorCloseRotation, DoorCloseSpeed);
                    return;
                default:
                    return;
            }
        }
        /// <summary>
        /// Triggers the animation of the wheels of the vehicle depending on the input states
        /// </summary>
        /// <param name="animationState"></param>
        private void SetWheelState(AnimationState animationState)
        {
            switch (animationState)
            {
                case AnimationState.UP:
                    AnimationController(leftWheels, WheelUpRotation, WheelUpSpeed);
                    AnimationController(rightWheels, -WheelUpRotation, WheelUpSpeed);
                    return;
                case AnimationState.DOWN:
                    AnimationController(leftWheels, WheelDownRotation, WheelDownSpeed);
                    AnimationController(rightWheels, WheelDownRotation, WheelDownSpeed);
                    return;
                default:
                    return;
            }
        }
        /// <summary>
        /// Used as a boilerplate way of controlling the local rotation of vehicle parts
        /// </summary>
        /// <param name="door"></param>
        /// <param name="rotation"></param>
        /// <param name="speed"></param>
        private static void AnimationController(Transform door, Vector3 rotation, float speed)
        {
            door.DOLocalRotate(rotation, speed);
        }

        private void Awake()
        {
            // Assign references
            foreach (GameObject rootGameObject in SceneManager.GetActiveScene().GetRootGameObjects())
            {
                if (!rootGameObject.CompareTag($"Player")) continue;
                controller = rootGameObject.GetComponent<ControllerTransforms>();
                playerController = rootGameObject.GetComponent<PlayerController>();
            }
            
            // Set up event listeners
            driverDoorExternal.buttonPress.AddListener(EnterVehicle);
            driverDoorInternal.buttonPress.AddListener(ExitVehicle);
            powerButton.buttonPress.AddListener(Power);
        }
        private void Update()
        {
            SetVehicleVector();
            Hover();
            Thrust();
        }
    }
}
