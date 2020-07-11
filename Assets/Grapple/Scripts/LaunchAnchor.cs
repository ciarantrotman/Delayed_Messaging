using System;
using Delayed_Messaging.Scripts.Player;
using UnityEngine;

namespace Grapple.Scripts
{
    [RequireComponent(typeof(ControllerTransforms))]
    public class LaunchAnchor : MonoBehaviour
    {
        [HideInInspector] public GameObject waist, leftAnchor, rightAnchor;

        [SerializeField] private GameObject configurationTarget;
        
        private Vector3 headPosition, headForward;
        private ControllerTransforms controller;
        private bool previousLeft, previousRight;
        
        [SerializeField] private float height = .75f, offset = .2f, back = .1f, grabRadius = .2f;

        public enum Configuration { CENTER, RIGHT, LEFT }

        internal Configuration configuration = Configuration.CENTER;
        
        /// <summary>
        /// Called once to create cached game objects
        /// </summary>
        /// <param name="controllerTransforms"></param>
        public void ConfigureAnchors(ControllerTransforms controllerTransforms)
        {
            controller = controllerTransforms;
            
            waist = new GameObject("[Launch Anchors / Waist]");
            leftAnchor = new GameObject("[Launch Anchor / Left]");
            rightAnchor = new GameObject("[Launch Anchor / Right]");
            
            configurationTarget = Instantiate(configurationTarget, waist.transform);
            configurationTarget.transform.localPosition = Vector3.zero;
            configurationTarget.transform.rotation = Quaternion.identity;
            
            leftAnchor.transform.SetParent(waist.transform);
            rightAnchor.transform.SetParent(waist.transform);
            
            SetAnchorOffset(Configuration.CENTER);
        }
        /// <summary>
        /// Creates the waist parent object used to calculate anchor positions
        /// </summary>
        /// <param name="position"></param>
        /// <param name="forward"></param>
        private void CalculateWaistPosition(Vector3 position, Vector3 forward)
        {
            waist.transform.position = new Vector3(position.x, position.y - height, position.z);
            waist.transform.forward = forward; // todo: swap between reference frames when grappled
        }
        /// <summary>
        /// Returns the relevant anchor's position
        /// </summary>
        /// <returns></returns>
        public Vector3 Anchor(Configuration check)
        {
            switch (check)
            {
                case Configuration.LEFT:
                    return leftAnchor.transform.position;
                case Configuration.RIGHT:
                    return rightAnchor.transform.position;
                case Configuration.CENTER:
                    return Vector3.zero;
                default:
                    return Vector3.zero;
            }
        }
        /// <summary>
        /// Returns the relevant vector based on configuration
        /// </summary>
        /// <returns></returns>
        public Vector3 Direction(Configuration check)
        {
            switch (configuration)
            {
                // When grappling left handed
                case Configuration.LEFT:
                    return  controller.Position(ControllerTransforms.Check.LEFT) - Anchor(Configuration.LEFT);
                // When grappling right handed
                case Configuration.RIGHT:
                    return controller.Position(ControllerTransforms.Check.RIGHT) - Anchor(Configuration.RIGHT);
                // When grappling with both hands
                case Configuration.CENTER when check == Configuration.LEFT:
                    return  controller.Position(ControllerTransforms.Check.LEFT) - Anchor(Configuration.LEFT);
                case Configuration.CENTER when check == Configuration.RIGHT:
                    return controller.Position(ControllerTransforms.Check.RIGHT) - Anchor(Configuration.RIGHT);
                default:
                    return Vector3.zero;
            }
        }
        /// <summary>
        /// Changes the local position of the two anchors based on the configuration
        /// </summary>
        /// <param name="config"></param>
        private void SetAnchorOffset(Configuration config)
        {
            leftAnchor.transform.localPosition = new Vector3(- offset, 0, - back);
            rightAnchor.transform.localPosition = new Vector3(offset, 0, - back);
            
            return;
            switch (config)
            {
                case Configuration.CENTER:
                    leftAnchor.transform.localPosition = new Vector3(- offset, 0, - back);
                    rightAnchor.transform.localPosition = new Vector3(offset, 0, - back);
                    break;
                case Configuration.RIGHT:
                    leftAnchor.transform.localPosition = new Vector3(offset, 0, - back);
                    rightAnchor.transform.localPosition = new Vector3(offset + (offset * .5f), 0, - back);
                    break;
                case Configuration.LEFT:
                    leftAnchor.transform.localPosition = new Vector3(- offset - (offset * .5f), 0, - back);
                    rightAnchor.transform.localPosition = new Vector3(- offset, 0, - back);
                    break;
                default:
                    break;
            }
        }
        /// <summary>
        /// Changes the configuration and resets the position of the anchors
        /// </summary>
        /// <param name="config"></param>
        private void SetConfiguration(Configuration config)
        {
            configuration = config;
            SetAnchorOffset(configuration);
        }
        /// <summary>
        /// Swaps the configuration of the anchors based on what hand you grab your waist with
        /// </summary>
        /// <param name="check"></param>
        /// <param name="hand"></param>
        /// <param name="grab"></param>
        /// <param name="previous"></param>
        private void CheckConfiguration(Configuration check, Vector3 hand, bool grab, bool previous)
        {
            // Only run through this if you grab that frame and are within range
            if (!(grab && !previous) || (Vector3.Distance(waist.transform.position, hand) > grabRadius)) return;

            switch (check)
            {
                // When you grab the right anchor
                case Configuration.RIGHT when configuration == Configuration.CENTER:
                    SetConfiguration(Configuration.RIGHT);
                    break;
                case Configuration.RIGHT when configuration == Configuration.LEFT:
                    SetConfiguration(Configuration.CENTER);
                    break;
                case Configuration.RIGHT when configuration == Configuration.RIGHT:
                    SetConfiguration(Configuration.CENTER);
                    break;
                // When you grab the left anchor
                case Configuration.LEFT when configuration == Configuration.CENTER:
                    SetConfiguration(Configuration.LEFT);
                    break;
                case Configuration.LEFT when configuration == Configuration.LEFT:
                    SetConfiguration(Configuration.CENTER);
                    break;
                case Configuration.LEFT when configuration == Configuration.RIGHT:
                    SetConfiguration(Configuration.CENTER);
                    break;
                default:
                    return;
            }
        }
        private void Update()
        {
            CalculateWaistPosition(controller.Position(ControllerTransforms.Check.HEAD), controller.ForwardVector(ControllerTransforms.Check.HEAD));
            
            CheckConfiguration(
                Configuration.LEFT, 
                controller.Position(ControllerTransforms.Check.LEFT), 
                controller.Grab(ControllerTransforms.Check.LEFT),
                previousLeft);
            CheckConfiguration(
                Configuration.RIGHT, 
                controller.Position(ControllerTransforms.Check.RIGHT), 
                controller.Grab(ControllerTransforms.Check.RIGHT),
                previousRight);

            previousLeft = controller.Grab(ControllerTransforms.Check.LEFT);
            previousRight = controller.Grab(ControllerTransforms.Check.RIGHT);
        }
    }
}
