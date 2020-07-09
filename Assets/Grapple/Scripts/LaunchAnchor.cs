using System;
using Delayed_Messaging.Scripts.Player;
using UnityEngine;

namespace Grapple.Scripts
{
    [RequireComponent(typeof(ControllerTransforms))]
    public class LaunchAnchor : MonoBehaviour
    {
        [HideInInspector] public GameObject waist, leftAnchor, rightAnchor;
        
        private Vector3 headPosition, headForward;
        public ControllerTransforms controller;
        
        [Header("Anchor Configuration")]
        [SerializeField] private float height = .75f;
        [SerializeField] private float displacement;
        [SerializeField] private float offset = .2f;
        
        public enum Configuration { CENTER, RIGHT, LEFT }
        private Configuration configuration;
        
        /// <summary>
        /// 
        /// </summary>
        /// <param name="controllerTransforms"></param>
        public void ConfigureAnchors(ControllerTransforms controllerTransforms)
        {
            controller = controllerTransforms;
            
            waist = new GameObject("[Launch Anchors / Waist]");
            leftAnchor = new GameObject("[Launch Anchor / Left]");
            rightAnchor = new GameObject("[Launch Anchor / Right]");
            
            leftAnchor.transform.SetParent(waist.transform);
            rightAnchor.transform.SetParent(waist.transform);
            
            SetAnchorOffset(Configuration.CENTER);
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="position"></param>
        /// <param name="forward"></param>
        private void CalculateWaistPosition(Vector3 position, Vector3 forward)
        {
            waist.transform.position = new Vector3(position.x, position.y - height, position.z);
            waist.transform.forward = forward; // todo: swap between reference frames when grappled
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="config"></param>
        public void SetConfiguration(Configuration config)
        {
            configuration = config;
            SetAnchorOffset(configuration);
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
        /// 
        /// </summary>
        /// <param name="config"></param>
        private void SetAnchorOffset(Configuration config)
        {
            switch (config)
            {
                case Configuration.CENTER:
                    leftAnchor.transform.localPosition = new Vector3(- offset, 0, 0);
                    rightAnchor.transform.localPosition = new Vector3(offset, 0, 0);
                    break;
                case Configuration.RIGHT:
                    leftAnchor.transform.localPosition = new Vector3(offset, 0, 0);
                    rightAnchor.transform.localPosition = new Vector3(offset + (offset * .5f), 0, 0);
                    break;
                case Configuration.LEFT:
                    leftAnchor.transform.localPosition = new Vector3(- offset - (offset * .5f), 0, 0);
                    rightAnchor.transform.localPosition = new Vector3(- offset, 0, 0);
                    break;
                default:
                    break;
            }
        }
        private void Update()
        {
            CalculateWaistPosition(controller.Position(ControllerTransforms.Check.HEAD), controller.ForwardVector(ControllerTransforms.Check.HEAD));
        }
    }
}
