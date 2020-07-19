using Spaces.Scripts.User_Interface.Interface_Elements;
using UnityEngine;
using Button = Spaces.Scripts.User_Interface.Interface_Elements.Button;

namespace Spaces.Scripts.User_Interface
{
    public class DirectInteraction : MonoBehaviour
    {
        private Button button;
        private bool extantButton;
        private SphereCollider directCollider;

        public void Initialise(float radius)
        {
            // Create references
            gameObject.tag = BaseInterface.Direct;
            directCollider = gameObject.AddComponent<SphereCollider>();

            // Configure direct collider
            directCollider.isTrigger = true;
            directCollider.radius = radius;
        }

        public void ButtonGrabStart()
        {
            if (!extantButton) return;
            button.grabStart.Invoke();
        }
        
        public void ButtonGrabStay()
        {
            if (!extantButton) return;
            button.grabStay.Invoke();
        }
        
        public void ButtonGrabEnd()
        {
            if (!extantButton) return;
            button.grabEnd.Invoke();
        }

        private void OnTriggerEnter(Collider other)
        {
            if (!other.CompareTag(BaseInterface.Button)) return;
            
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
            if (other.CompareTag(BaseInterface.Button) && other.GetComponent<Button>() == button && extantButton)
            {
                // You are no longer near at a button, set the last button to not being hovered on
                button.HoverEnd();
                button = null;
                extantButton = false;
            }
        }
    }
}
