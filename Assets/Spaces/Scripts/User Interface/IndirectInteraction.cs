using Spaces.Scripts.User_Interface.Interface_Elements;
using Spaces.Scripts.Utilities;
using UnityEngine;

namespace Spaces.Scripts.User_Interface
{
    public class IndirectInteraction: MonoBehaviour
    {
        private GameObject rayParent;
        private LineRenderer ray;
        private Button button;
        private bool extantButton;
        private const float Offset = .5f;
        private static Vector3 OffsetPosition => new Vector3(0,0,Offset);

        public void Initialise(GameObject parent, string rayName, Material material)
        {
            rayParent = Set.Object(parent, rayName);
            ray = rayParent.LineRender(material, .005f, .001f, true, false);
                
            // Set the positions of the raycast
            ray.SetPosition(0, Vector3.zero);
            ray.SetPosition(1, OffsetPosition);
        }

        // Find the current button
        public void Check(Vector3 origin, Vector3 vector, float range, bool rayEnabled)
        {
            float rayDistance = Offset;

            if (Physics.Raycast(origin, vector, out RaycastHit hit, range) && hit.transform.CompareTag(BaseInterface.Button) && rayEnabled)
            {
                // Create a new button cache
                Button newButton = hit.transform.GetComponent<Button>();
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
                
            ConfigureRay(origin, vector, rayDistance, rayEnabled);
        }
        private void ConfigureRay(Vector3 origin, Vector3 vector, float range, bool enableRay)
        {
            ray.enabled = enableRay;
            if (!enableRay) return;
                
            ray.SetPosition(1, Vector3.Lerp(ray.GetPosition(1), new Vector3(0,0,range), .25f));
            rayParent.transform.position = Vector3.Lerp(rayParent.transform.position, origin, .75f);
            rayParent.transform.forward = Vector3.Lerp(rayParent.transform.forward, vector, .5f);
        }
        
        // Wrappers for button events
        public void ButtonSelect()
        {
            if (extantButton && button.interactionType != BaseInterface.InteractionType.DIRECT && button.triggerType != BaseInterface.TriggerType.GRAB)
            {
                button.buttonSelect.Invoke();
            }
        }
        public void ButtonGrab()
        {
            if (!extantButton) return;
        }
    }
}