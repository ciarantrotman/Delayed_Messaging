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
        [SerializeField, Range(.1f, .5f)] protected float detectionRadius = .1f;
        public DetectionMode detectionMode = DetectionMode.GRAB;
        protected SphereCollider detectionCollider;
        protected ControllerTransforms controller;

        private void Awake()
        {
            foreach (GameObject rootGameObject in SceneManager.GetActiveScene().GetRootGameObjects())
            {
                if (!rootGameObject.CompareTag($"Player")) continue;
                controller = rootGameObject.GetComponent<ControllerTransforms>();
            }
            
            Initialise();
        }

        /// <summary>
        /// This is only called once all references have been made
        /// </summary>
        protected abstract void Initialise();

        private void OnDrawGizmos()
        {
            Popcron.Gizmos.Sphere(transform.position, detectionRadius, Color.white);
        }
    }
}
