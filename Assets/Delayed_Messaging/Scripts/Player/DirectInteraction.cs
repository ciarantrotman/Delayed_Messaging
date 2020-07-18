using System;
using Delayed_Messaging.Scripts.Utilities;
using Grapple.Scripts.User_Interface;
using UnityEngine;

namespace Delayed_Messaging.Scripts.Player
{
    public class DirectInteraction : MonoBehaviour
    {
        private ControllerTransforms controller;
        private SphereCollider directCollider;
        private ControllerTransforms.Check check;
        private Button button;

        public void Initialise(ControllerTransforms controllerTransforms, ControllerTransforms.Check configureCheck, float radius)
        {
            // Create references
            directCollider = gameObject.AddComponent<SphereCollider>();
            directCollider.isTrigger = true;
            
            // Cache variables
            controller = controllerTransforms;
            directCollider.radius = radius;
            check = configureCheck;

            // Setup events
            controller.GrabEvent(check, ControllerTransforms.EventTracker.EventType.START).AddListener(ButtonGrab);
            controller.SelectEvent(check, ControllerTransforms.EventTracker.EventType.START).AddListener(ButtonSelect);
        }

        private void ButtonGrab()
        {
            if (button == null || button.detectionMode != DirectInterface.DetectionMode.GRAB) return;
            button.buttonPress.Invoke();
        }
        
        private void ButtonSelect()
        {
            if (button == null || button.detectionMode != DirectInterface.DetectionMode.SELECT) return;
            button.buttonPress.Invoke();
        }

        private void OnTriggerEnter(Collider other)
        {
            if (other.CompareTag($"Button"))
            {
                button = other.transform.GetComponent<Button>();
            }
        }

        private void OnTriggerExit(Collider other)
        {
            if (other.CompareTag($"Button"))
            {
                button = null;
            }
        }
    }
}
