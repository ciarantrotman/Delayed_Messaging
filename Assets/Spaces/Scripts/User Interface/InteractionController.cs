using System;
using Delayed_Messaging.Scripts.Player;
using Delayed_Messaging.Scripts.Utilities;
using Spaces.Scripts.User_Interface.Interface_Elements;
using UnityEngine;

namespace Spaces.Scripts.User_Interface
{
    [RequireComponent(typeof(ControllerTransforms))]
    public class InteractionController : MonoBehaviour
    {
        [Header("Interaction Settings")]
        [SerializeField, Range(1, 50)] private float range;
        [SerializeField] private Material material;
        
        private ControllerTransforms Controller => GetComponent<ControllerTransforms>();
        private IndirectInteraction dominant = new IndirectInteraction(), nonDominant = new IndirectInteraction();
        
        [Serializable] public struct IndirectInteraction
        {
            private GameObject rayParent;
            private LineRenderer ray;
            private Button button;
            private const float Offset = 1f;
            private Vector3 OffsetPosition => new Vector3(0,0,Offset);

            public void Initialise(GameObject parent, string name, Material material)
            {
                rayParent = Set.Object(parent, name);
                ray = rayParent.LineRender(material, .01f, .001f, true, false);
                
                // Set the positions of the raycast
                ray.SetPosition(0, Vector3.zero);
                ray.SetPosition(1, OffsetPosition);
            }

            // Find the current button
            public void Check(Vector3 origin, Vector3 vector, float range, bool enabled)
            {
                float rayDistance;
                
                if (Physics.Raycast(origin, vector, out RaycastHit hit, range))
                {
                    button = hit.transform.CompareTag(BaseInterface.Button) ? hit.transform.GetComponent<Button>() : null;
                    rayDistance = hit.distance;
                }
                else
                {
                    button = null;
                    rayDistance = Offset;
                }
                
                ConfigureRay(origin, vector, rayDistance, enabled);
            }
            private void ConfigureRay(Vector3 origin, Vector3 vector, float range, bool enabled)
            {
                ray.enabled = enabled;
                if (!enabled) return;
                
                ray.SetPosition(1, Vector3.Lerp(ray.GetPosition(1), new Vector3(0,0,range), .75f));
                rayParent.transform.position = Vector3.Lerp(rayParent.transform.position, origin, .75f);
                rayParent.transform.forward = Vector3.Lerp(rayParent.transform.forward, vector, .75f);
            }
            // Wrappers for button events
            public void ButtonSelect()
            {
                if (button == null || button.triggerType == BaseInterface.TriggerType.GRAB) return;
                button.select.Invoke();
            }
            public void ButtonGrab()
            {
                if (button == null || button.triggerType == BaseInterface.TriggerType.SELECT) return;
                button.grabStart.Invoke();
            }
        }

        private void Awake()
        {
            // Initialise
            nonDominant.Initialise(
                gameObject, 
                "[Non-Dominant]", 
                material);
            dominant.Initialise(
                gameObject, 
                "[Dominant]", 
                material);
            
            // Add event listeners for both controllers
            Controller.SelectEvent(
                ControllerTransforms.Check.LEFT, 
                ControllerTransforms.EventTracker.EventType.END).AddListener(nonDominant.ButtonSelect);
            Controller.SelectEvent(
                ControllerTransforms.Check.RIGHT, 
                ControllerTransforms.EventTracker.EventType.END).AddListener(dominant.ButtonSelect);
        }

        private void Update()
        {
            dominant.Check(
                Controller.Position(ControllerTransforms.Check.RIGHT), 
                Controller.ForwardVector(ControllerTransforms.Check.RIGHT), 
                range,
                true);
            nonDominant.Check(
                Controller.Position(ControllerTransforms.Check.LEFT),
                Controller.ForwardVector(ControllerTransforms.Check.LEFT), 
                range,
                true);
        }
    }
}
