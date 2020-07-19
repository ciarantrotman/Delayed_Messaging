using Delayed_Messaging.Scripts.Utilities;
using Spaces.Scripts.User_Interface.Interface_Elements;
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
        private Vector3 OffsetPosition => new Vector3(0,0,Offset);

        public void Initialise(GameObject parent, string rayName, Material material)
        {
            rayParent = Set.Object(parent, rayName);
            ray = rayParent.LineRender(material, .005f, .001f, true, false);
                
            // Set the positions of the raycast
            ray.SetPosition(0, Vector3.zero);
            ray.SetPosition(1, OffsetPosition);
        }

        // Find the current button
        public void Check(Vector3 origin, Vector3 vector, float range, bool enabled)
        {
            float rayDistance;
                
            if (Physics.Raycast(origin, vector, out RaycastHit hit, range) && hit.transform.CompareTag(BaseInterface.Button))
            {
                // Create a new button cache
                Button newButton = hit.transform.GetComponent<Button>();
                rayDistance = hit.distance;

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
            else
            {
                // You are no longer pointing at a button, set the last button to not being hovered on
                if (extantButton)
                {
                    button.HoverEnd();
                    button = null;
                    extantButton = false;
                }
                rayDistance = Offset;
            }
                
            ConfigureRay(origin, vector, rayDistance, enabled);
        }
        private void ConfigureRay(Vector3 origin, Vector3 vector, float range, bool enabled)
        {
            ray.enabled = enabled;
            if (!enabled) return;
                
            ray.SetPosition(1, Vector3.Lerp(ray.GetPosition(1), new Vector3(0,0,range), .25f));
            rayParent.transform.position = Vector3.Lerp(rayParent.transform.position, origin, .75f);
            rayParent.transform.forward = Vector3.Lerp(rayParent.transform.forward, vector, .5f);
        }
        // Wrappers for button events
        public void ButtonSelect()
        {
            Debug.Log($"PRE -> {button == null}, {extantButton}");
            if(button == null) return;
            Debug.Log($"CALLED -> {button.name}, {extantButton}");
            button.buttonSelect.Invoke();
            Debug.Log("YES");
        }
        public void ButtonGrab()
        {
        }
    }
}