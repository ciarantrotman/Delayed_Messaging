using System;
using Delayed_Messaging.Scripts.Player;
using UnityEngine;

namespace Grapple.Scripts
{
    public class BoostSystem : MonoBehaviour
    {
        //[Header("Boost System Configuration")]
        [SerializeField, Range(1f, 50f)] private float boostStrength = 10f;
        [SerializeField] private TimeManager.SlowTimeData slowTime;

        private ControllerTransforms controller;
        private Rigidbody playerRigidBody;
        private TimeManager timeManager;

        [HideInInspector] public Boost leftBoost, rightBoost;

        [Serializable] public class Boost
        {
            private Vector3 boostVector;
            private TimeManager timeManager;
            private TimeManager.SlowTimeData slowTime;
            private Rigidbody player;

            /// <summary>
            /// Function called once to cache references from the parent class
            /// </summary>
            /// <param name="controller"></param>
            /// <param name="time"></param>
            /// <param name="rigidBody"></param>
            /// <param name="slowTimeData"></param>
            /// <param name="check"></param>
            public void Initialise(ControllerTransforms controller, TimeManager time, Rigidbody rigidBody, TimeManager.SlowTimeData slowTimeData, ControllerTransforms.Check check)
            {
                timeManager = time;
                player = rigidBody;
                slowTime = slowTimeData;
                
                controller.MenuEvent(check, ControllerTransforms.EventTracker.EventType.STAY).AddListener(ApplyBoost);
                controller.MenuEvent(check, ControllerTransforms.EventTracker.EventType.END).AddListener(Stop);
            }

            public void SetBoostVector(Vector3 direction, float magnitude)
            {
                boostVector = direction.normalized * magnitude;
            }
            /// <summary>
            /// Update analogue called when holding to boost
            /// </summary>
            public void ApplyBoost()
            {
                player.AddForce(boostVector, ForceMode.Acceleration);
            }
            /// <summary>
            /// Called once when boost is released
            /// </summary>
            public void Stop()
            {
                timeManager.SlowTime(slowTime);
            }
        }
        private void Start()
        {
            controller = GetComponent<ControllerTransforms>();
            playerRigidBody = GetComponent<Rigidbody>();
            timeManager = GetComponent<TimeManager>();
            
            leftBoost.Initialise(
                controller,
                timeManager, 
                playerRigidBody, 
                slowTime,
                ControllerTransforms.Check.LEFT);
            rightBoost.Initialise(
                controller,
                timeManager,
                playerRigidBody,
                slowTime,
                ControllerTransforms.Check.RIGHT);
        }
        private void Update()
        {
            leftBoost.SetBoostVector(
                controller.ForwardVector(ControllerTransforms.Check.LEFT),
                boostStrength);
            rightBoost.SetBoostVector(
                controller.ForwardVector(ControllerTransforms.Check.RIGHT),
                boostStrength);
        }
    }
}
