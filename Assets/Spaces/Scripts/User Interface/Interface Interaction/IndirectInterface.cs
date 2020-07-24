using Spaces.Scripts.Player;
using Spaces.Scripts.User_Interface.Interface_Elements;
using Spaces.Scripts.Utilities;
using UnityEngine;

namespace Spaces.Scripts.User_Interface.Interface_Interaction
{
    public class IndirectInterface: Interaction
    {
        private GameObject rayParent;
        private LineRenderer ray;
        private Button button;
        private bool extant;
        private const float Offset = .5f;
        private static Vector3 OffsetPosition => new Vector3(0,0,Offset);

        public void Initialise(GameObject parent, string rayName, Material material, ControllerTransforms.Check check)
        {
            // Cache values
            orientation = check;
            
            // Create ray objects
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
                    if (button != newButton)
                    {
                        // Stop hovering on the old object if there is one
                        if (extant)
                        {
                            button.HoverEnd();
                        }
                        // start hovering on the new object
                        newButton.HoverStart();
                    }
                    
                    // Button is now that new button
                    button = newButton;
                    extant = true;
                }
            }
            else
            {
                // You are no longer pointing at a button, set the last button to not being hovered on
                if (extant)
                {
                    button.HoverEnd();
                    button = null;
                    extant = false;
                }
            }
            // Always call this at the end to set the state of the UI ray
            ConfigureRay(origin, vector, rayDistance, extant);
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
            if (!extant || !button.CheckInvocation(BaseInterface.TriggerType.SELECT, BaseInterface.InteractionType.INDIRECT)) return;
            button.buttonSelect.Invoke();
        }
        protected override void GrabStart()
        {
            if (!extant || !button.CheckInvocation(BaseInterface.TriggerType.GRAB, BaseInterface.InteractionType.INDIRECT)) return;
            button.grabStart.Invoke();
        }
        
        protected override void GrabStay()
        {
            if (!extant || !button.CheckInvocation(BaseInterface.TriggerType.GRAB, BaseInterface.InteractionType.INDIRECT)) return;
            button.grabStay.Invoke();
        }
        
        protected override void GrabEnd()
        {
            if (!extant || !button.CheckInvocation(BaseInterface.TriggerType.GRAB, BaseInterface.InteractionType.INDIRECT)) return;
            button.grabEnd.Invoke();
        }
        
    }
}