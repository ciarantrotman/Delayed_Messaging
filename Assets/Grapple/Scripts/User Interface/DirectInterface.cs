using System;
using Delayed_Messaging.Scripts.Player;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Grapple.Scripts.User_Interface
{
    public abstract class DirectInterface : MonoBehaviour
    {
        public enum DetectionMode { GRAB, SELECT }
        [Header("Direct Interface Options")]
        public DetectionMode detectionMode = DetectionMode.GRAB;

        private void Awake()
        {
            Initialise();
        }

        /// <summary>
        /// This is only called once all references have been made
        /// </summary>
        protected abstract void Initialise();
    }
}
