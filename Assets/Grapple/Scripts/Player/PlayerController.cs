using System;
using Delayed_Messaging.Scripts.Player.Locomotion;
using Grapple.Scripts.Player.Grapple_System;
using UnityEngine;

namespace Grapple.Scripts.Player
{
    public class PlayerController : MonoBehaviour
    {
        public enum PlayerState { VEHICLE, PEDESTRIAN}
        [HideInInspector] public PlayerState playerState;
        [HideInInspector] public Rigidbody playerRigidbody;
        
        private GrappleSystem grappleSystem;
        private LaunchAnchor launchAnchor;
        private BoostSystem boostSystem;
        private Locomotion locomotion;

        private void Awake()
        {
            grappleSystem = gameObject.GetComponent<GrappleSystem>();
            boostSystem = gameObject.GetComponent<BoostSystem>();
            locomotion = gameObject.GetComponent<Locomotion>();
            playerRigidbody = GetComponent<Rigidbody>();
            launchAnchor = GetComponent<LaunchAnchor>();
        }

        public void ChangePlayerState(PlayerState state)
        {
            playerState = state;

            switch (playerState)
            {
                case PlayerState.VEHICLE:
                    EnterVehicle();
                    return;
                case PlayerState.PEDESTRIAN:
                    ExitVehicle();
                    break;
                default:
                    return;
            }
        }

        public void MovePlayer(Transform target, Locomotion.Method method, float time)
        {
            locomotion.CustomLocomotion(target.position, target.eulerAngles, method, time);
        }

        private void EnterVehicle()
        {
            grappleSystem.ToggleGrapple(false);
            boostSystem.enabled = false;
            launchAnchor.waist.SetActive(false);
        }

        private void ExitVehicle()
        {
            grappleSystem.ToggleGrapple(true);
            boostSystem.enabled = true;
            launchAnchor.waist.SetActive(true);
        }
    }
}
