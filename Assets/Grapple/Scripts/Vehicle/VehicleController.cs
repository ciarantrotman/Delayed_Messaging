using System;
using DG.Tweening;
using TMPro;
using UnityEngine;

namespace Grapple.Scripts.Vehicle
{
    public class VehicleController : MonoBehaviour
    {
        #region Variables

        [Header("Vehicle References")]
        [SerializeField] private Transform playerPosition;
        [SerializeField] private Transform leftDoor, rightDoor, leftWheels, rightWheels;
        [SerializeField] private TextMeshPro readout;
        [SerializeField] private Rigidbody vehicleRigidbody;
        [Header("Animation Variables")]
        [SerializeField, Range(0, 180f)] private float doorOpenAngle;
        [SerializeField, Range(0, 5)] private float doorOpenSpeed;
        [SerializeField, Range(0, 180f)] private float wheelUpAngle;
        [SerializeField, Range(0, 5)] private float wheelUpSpeed;
        private Vector3 DoorOpenRotation => new Vector3(0,0,doorOpenAngle);
        private static Vector3 DoorCloseRotation => Vector3.zero;
        private float DoorOpenSpeed => doorOpenSpeed;
        private float DoorCloseSpeed => doorOpenSpeed * .5f;
        private Vector3 WheelUpRotation => new Vector3(0,0,wheelUpAngle);
        private static Vector3 WheelDownRotation => Vector3.zero;
        private float WheelUpSpeed => wheelUpSpeed;
        private float WheelDownSpeed => wheelUpSpeed * .5f;
        public enum State { UP, DOWN }
        public enum Side { LEFT, RIGHT, BOTH }

        #endregion
        
        /// <summary>
        /// Sets the values of the vehicle based on the rigidbody states
        /// </summary>
        public void SetReadout()
        {
            readout.SetText($"Speed:  <b>{vehicleRigidbody.velocity.magnitude}</b>" +
                            $"Height: <b>{vehicleRigidbody.transform.position.y}</b>" +
                            $"Fuel:   <b>{0f}</b>");
        }
        /// <summary>
        /// Triggers the animation of the doors of the vehicle depending on the input states
        /// </summary>
        /// <param name="side"></param>
        /// <param name="state"></param>
        public void SetDoorState(Side side, State state)
        {
            switch (side)
            {
                // Left door
                case Side.LEFT when state == State.UP:
                    AnimationController(leftDoor, DoorOpenRotation, DoorOpenSpeed);
                    return;
                case Side.LEFT when state == State.DOWN:
                    AnimationController(leftDoor, DoorCloseRotation, DoorCloseSpeed);
                    return;
                // Right door
                case Side.RIGHT when state == State.UP:
                    AnimationController(rightDoor, DoorOpenRotation, DoorOpenSpeed);
                    return;
                case Side.RIGHT when state == State.DOWN:
                    AnimationController(rightDoor, DoorCloseRotation, DoorCloseSpeed);
                    return;
                // Both doors
                case Side.BOTH when state == State.UP:
                    AnimationController(leftDoor, DoorOpenRotation, DoorOpenSpeed);
                    AnimationController(rightDoor, DoorOpenRotation, DoorOpenSpeed);
                    return;
                case Side.BOTH when state == State.DOWN:
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
        /// <param name="state"></param>
        public void SetWheelState(State state)
        {
            switch (state)
            {
                case State.UP:
                    AnimationController(leftWheels, WheelUpRotation, WheelUpSpeed);
                    AnimationController(rightWheels, WheelUpRotation, WheelUpSpeed);
                    return;
                case State.DOWN:
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
    }
}
