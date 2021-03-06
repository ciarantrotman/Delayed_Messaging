﻿using Spaces.Scripts.Player;
using Spaces.Scripts.User_Interface.Interface_Elements;
using Spaces.Scripts.Utilities;
using UnityEngine;
using Button = Spaces.Scripts.User_Interface.Interface_Elements.Button;

namespace Spaces.Scripts.User_Interface.Interface_Interaction
{
    public class DirectInterface : Interaction
    {
        private Button button;
        private bool extantButton;
        private SphereCollider directCollider;

        public void Initialise(float radius, ControllerTransforms.Check checkCache)
        {
            // Create references
            gameObject.tag = BaseInterface.Direct;
            directCollider = gameObject.AddComponent<SphereCollider>();
            orientation = checkCache;
            
            // Configure direct collider
            directCollider.isTrigger = true;
            directCollider.radius = radius;
        }

        protected override void Select()
        {
            if (!extantButton) return;
            button.buttonSelect.Invoke();
        }

        protected override void GrabStart()
        {
            if (!extantButton) return;
            // Set the orientation based on which direct interface triggers the event
            button.SetCheck(orientation);
            // Trigger grab start
            button.grabStart.Invoke();
        }
        
        protected override void GrabStay()
        {
            if (!extantButton) return;
            button.grabStay.Invoke();
        }
        
        protected override void GrabEnd()
        {
            if (!extantButton) return;
            button.grabEnd.Invoke();
        }

        private void OnTriggerEnter(Collider other)
        {
            if (!other.CompareTag(tagComparison)) return;
            
            // Create a new button cache
            Button newButton = other.transform.GetComponent<Button>();
                
            // Logic for when it is a new button
            if (extantButton && button != newButton)
            {
                button.HoverEnd();
            }
                
            // Button is now that new button
            button = newButton;
            extantButton = true;
            button.HoverStart();
        }

        private void OnTriggerExit(Collider other)
        {
            if (other.CompareTag(tagComparison) && other.GetComponent<Button>() == button && extantButton)
            {
                // You are no longer near at a button, set the last button to not being hovered on
                button.HoverEnd();
                button = null;
                extantButton = false;
            }
        }
    }
}
