using System;
using Delayed_Messaging.Scripts.Player;
using UnityEngine;

namespace Grapple.Scripts
{
    public class BoostSystem : MonoBehaviour
    {
        [Header("Boost System Configuration")]
        [SerializeField, Range(1f, 50f)] private float boostStrength = 10f;
        [SerializeField] private TimeManager.SlowTimeData slowTime;

        private ControllerTransforms controller;
        private Rigidbody playerRigidBody;
        private TimeManager timeManager;

        [HideInInspector] public Boost leftBoost, rightBoost;

        [Serializable] public class Boost
        {
            private Vector3 boostVector;
            private bool currentBoost, previousBoost;
            private TimeManager timeManager;
            private TimeManager.SlowTimeData slowTime;
            private Rigidbody player;

            /// <summary>
            /// Function called once to cache references from the parent class
            /// </summary>
            /// <param name="time"></param>
            /// <param name="rigidBody"></param>
            /// <param name="slowTimeData"></param>
            public void Initialise(TimeManager time, Rigidbody rigidBody, TimeManager.SlowTimeData slowTimeData)
            {
                timeManager = time;
                player = rigidBody;
                slowTime = slowTimeData;
            }
            /// <summary>
            /// Called once per frame to transfer vector data to internal class
            /// </summary>
            /// <param name="direction"></param>
            /// <param name="magnitude"></param>
            public void SetBoostVector(Vector3 direction, float magnitude)
            {
                boostVector = direction.normalized * magnitude;
            }
            /// <summary>
            /// Checks the conditions to boost
            /// </summary>
            /// <param name="boost"></param>
            public void CheckBoost(bool boost)
            {
                currentBoost = boost;
                if (currentBoost && previousBoost) ApplyBoost();
                else if (!currentBoost && previousBoost) Stop();
                previousBoost = currentBoost;
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
                timeManager, 
                playerRigidBody, 
                slowTime);
            rightBoost.Initialise(
                timeManager,
                playerRigidBody,
                slowTime);
        }

        private void Update()
        {
            leftBoost.SetBoostVector(
                controller.LeftForwardVector(),
                boostStrength);
            rightBoost.SetBoostVector(
                controller.RightForwardVector(),
                boostStrength);
            
            leftBoost.CheckBoost(controller.LeftGrab());
            rightBoost.CheckBoost(controller.RightGrab());
        }
    }
}
