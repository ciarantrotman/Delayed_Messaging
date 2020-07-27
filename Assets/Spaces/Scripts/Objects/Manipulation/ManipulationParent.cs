using System;
using Spaces.Scripts.Player;
using Spaces.Scripts.Utilities;
using UnityEngine;

namespace Spaces.Scripts.Objects.Manipulation
{
    public class ManipulationParent : MonoBehaviour
    {
        private Vector3 velocity, lastPosition, angular, lastRotation;
        private float damping, initialControllerDepth, initialObjectDepth;
        private Transform target;
        private bool extant;
        private GameObject controllerProxy;
        private Interaction.Mode mode;
        private static ControllerTransforms Controller => Reference.Controller();
        private ControllerTransforms.Check orientation;

        // ------------------------------------------------------------------------------------------------------------
        
        /// <summary>
        /// 
        /// </summary>
        /// <param name="targetTransform"></param>
        /// <param name="lerp"></param>
        public void DirectInitialise(Transform targetTransform, float lerp)
        {
            mode = Interaction.Mode.DIRECT;
            // Set it to the transform supplied
            transform.Transforms(targetTransform);
            // Cache references
            damping = lerp;
            target = targetTransform;
            extant = true;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="objectPosition"></param>
        /// <param name="check"></param>
        /// <param name="lerp"></param>
        /// <param name="controller"></param>
        public void IndirectInitialise(GameObject controller, Vector3 objectPosition, ControllerTransforms.Check check, float lerp)
        {
            mode = Interaction.Mode.INDIRECT;
            // Cache references
            orientation = check;
            target = Controller.Transform(orientation);
            damping = lerp;
            extant = true;
            controllerProxy = controller;
            // Set reference values
            initialControllerDepth = Vector3.Distance(Controller.Position(orientation), Controller.ChestPosition(orientation));
            initialObjectDepth = Vector3.Distance(objectPosition, Controller.Position(orientation));
            // Set the initial position of the object
            Transform objectTransform = transform;
            // Apply the required transformations
            objectTransform.localPosition = new Vector3(0,0, initialObjectDepth);
            objectTransform.rotation = Quaternion.identity;
        }
        /// <summary>
        /// Returns the inverse velocity of this object for some reason?
        /// </summary>
        /// <returns></returns>
        public Vector3 Velocity()
        {
            return - velocity;
        }
        /// <summary>
        /// Returns the inverse angular velocity of this object for some reason?
        /// </summary>
        /// <returns></returns>
        public Vector3 AngularVelocity()
        {
            return angular;
        }
        /// <summary>
        /// Tracks the velocity of this object
        /// </summary>
        private void SetVelocity()
        {
            // Create cached reference
            Transform tracking = transform;
            // Calculate velocity
            Vector3 currentPosition = tracking.position;
            velocity = (lastPosition - currentPosition) / Time.deltaTime;
            lastPosition = currentPosition;
            // Calculate angular velocity
            Vector3 currentRotation = tracking.eulerAngles;
            angular = (lastRotation - currentRotation);
            lastRotation = currentRotation;
        }

        // ------------------------------------------------------------------------------------------------------------
        
        private void FixedUpdate()
        {
            // Track the velocity of this object
            SetVelocity();
            
            // If this has been initialised then follow the supplied transform
            if (!extant) return;
            switch (mode)
            {
                case Interaction.Mode.DIRECT:
                    transform.LerpTransform(target, damping);
                    break;
                case Interaction.Mode.INDIRECT:
                    // Make the controller proxy follow the relevant controller
                    controllerProxy.transform.LerpTransform(target, damping);
                    // Cache references
                    Transform objectTransform = transform;
                    float currentDepth = objectTransform.localPosition.z;
                    float targetDepth = (Vector3.Distance(
                                             Controller.Position(orientation), 
                                             Controller.ChestPosition(orientation)) 
                                         / initialControllerDepth) * 
                                        initialObjectDepth;
                    // Apply the required transformations
                    objectTransform.localPosition = new Vector3(0,0, 
                        Mathf.Lerp(
                            currentDepth, 
                            targetDepth, 
                            damping));
                    objectTransform.rotation = Quaternion.identity;
                    break;
                default:
                    return;
            }
        }
    }
}
