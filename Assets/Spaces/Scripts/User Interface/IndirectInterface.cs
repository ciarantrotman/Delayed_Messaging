using Spaces.Scripts.Player;
using Spaces.Scripts.User_Interface.Interface_Elements;
using Spaces.Scripts.Utilities;
using UnityEngine;
using UnityEngine.Events;
using Event = Spaces.Scripts.Player.ControllerTransforms.EventTracker.EventType;

namespace Spaces.Scripts.User_Interface
{
    public class IndirectInterface: InteractionController
    {
        private GameObject rayParent;
        private LineRenderer ray;
        private Button button;
        private bool extantButton;
        private const float Offset = .5f;
        private static Vector3 OffsetPosition => new Vector3(0,0,Offset);

        public void Initialise(GameObject parent, string rayName, Material material)
        {
            rayParent = Set.Object(parent, rayName, Vector3.zero);
            ray = rayParent.LineRender(material, .005f, .001f, true, false);
                
            // Set the positions of the raycast
            ray.SetPosition(0, Vector3.zero);
            ray.SetPosition(1, OffsetPosition);
        }

        // Find the current button
        public void Check(Vector3 origin, Vector3 vector, float range, bool rayEnabled)
        {
            float rayDistance = Offset;

            if (Physics.Raycast(origin, vector, out RaycastHit hit, range) && hit.transform.CompareTag(BaseInterface.Interface) && rayEnabled)
            {
                // Create a new button cache
                Button newButton = hit.transform.GetComponent<Button>();
                // In the case that the collider we hit is in the child of the object
                newButton = newButton == null ? hit.transform.GetComponentInParent<Button>() : newButton;
                // Record the distance for the UI ray
                rayDistance = hit.distance;
                
                if (newButton.interactionType != BaseInterface.InteractionType.DIRECT);
                {
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
            }
            else
            {
                // You are no longer pointing at a button, set the last button to not being hovered on
                if (extantButton)
                {
                    button.HoverEnd();
                    button = null;
                    extantButton = false;
                }
            }
            // Always call this at the end to set the state of the UI ray
            ConfigureRay(origin, vector, rayDistance, rayEnabled);
        }
        /// <summary>
        /// Sets the state of the UI ray
        /// </summary>
        /// <param name="origin"></param>
        /// <param name="vector"></param>
        /// <param name="range"></param>
        /// <param name="enableRay"></param>
        private void ConfigureRay(Vector3 origin, Vector3 vector, float range, bool enableRay)
        {
            ray.enabled = enableRay;
            if (!enableRay) return;
                
            ray.SetPosition(1, Vector3.Lerp(ray.GetPosition(1), new Vector3(0,0,range), .25f));
            rayParent.transform.position = Vector3.Lerp(rayParent.transform.position, origin, .75f);
            rayParent.transform.forward = Vector3.Lerp(rayParent.transform.forward, vector, .5f);
        }
        
        // Wrappers for button events
        protected override void Select()
        {
            if (extantButton)
            {
                button.buttonSelect.Invoke();
            }
        }
        protected override void GrabStart()
        {
            if (!extantButton) return;
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
        
    }
}