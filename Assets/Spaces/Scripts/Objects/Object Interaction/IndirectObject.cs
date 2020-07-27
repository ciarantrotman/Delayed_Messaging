using Spaces.Scripts.Player;
using Spaces.Scripts.Utilities;
using UnityEngine;

namespace Spaces.Scripts.Objects.Object_Interaction
{
    public class IndirectObject : Interaction
    {
        private GameObject rayOrigin, rayCenter, rayEnd;
        private LineRenderer ray;
        private ObjectInstance objectInstance;
        private bool extant;
        private float range;
        private const float Offset = .1f;
        private static Vector3 OffsetPosition => new Vector3(0,0,Offset);
        private static ObjectInteractionController InteractionController => Reference.Player().GetComponent<ObjectInteractionController>();
        public void Initialise(GameObject parent, string rayName, Material material, float detectionRange, ControllerTransforms.Check check)
        {
            // Cache values
            range = detectionRange;
            orientation = check;
            
            // Create objects for linerenderer
            rayOrigin = Set.Object(parent, rayName, Vector3.zero);
            rayCenter = Set.Object(rayOrigin, $"{rayName} / Center", Vector3.zero);
            rayEnd = Set.Object(rayOrigin, $"{rayName} / End", OffsetPosition);
            
            // Create and configure linerenderer
            ray = rayOrigin.LineRender(material, .005f, .001f, true, true);
            ray.Bezier(rayOrigin.transform.position, Center(), rayEnd.transform.position);
        }
        
        // Find the current object
        public void Check(Vector3 origin, Vector3 vector, float radius, float castRange, bool rayEnabled)
        {
            range = castRange;
            if (Physics.SphereCast(new Ray {origin = origin, direction = vector}, radius, out RaycastHit hit, range) && hit.transform.CompareTag(tagComparison) && rayEnabled)
            {
                // Create a new button cache
                ObjectInstance newObject = hit.transform.GetComponent<ObjectInstance>();
                // In the case that the collider we hit is in the child of the object
                newObject = newObject == null ? hit.transform.GetComponentInParent<ObjectInstance>() : newObject;

                // Logic for when it is a new object
                if (objectInstance != newObject)
                {
                    // Stop hovering on the old object if there is one
                    if (extant)
                    {
                        objectInstance.HoverEnd();
                    }
                    // start hovering on the new object
                    newObject.HoverStart();
                }
                    
                // object instance is now that new button
                rayEnd.transform.position = hit.point;
                objectInstance = newObject;
                extant = true;
            }
            else
            {
                // You are no longer pointing at a button, set the last button to not being hovered on
                if (extant)
                {
                    objectInstance.HoverEnd();
                    objectInstance = null;
                    extant = false;
                }
                // Realign the end point of the ray
                Vector3 end = rayEnd.transform.localPosition;
                rayEnd.transform.localPosition = new Vector3(
                    Mathf.Lerp(end.x, 0, .5f),
                    Mathf.Lerp(end.y, 0, .5f), 
                    Mathf.Lerp(end.z, Offset, .25f));
            }
            // Always call this at the end to set the state of the UI ray
            ConfigureRay(origin, vector, rayEnabled);
        }
        /// <summary>
        /// Sets the state of the UI ray
        /// </summary>
        /// <param name="origin"></param>
        /// <param name="vector"></param>
        /// <param name="enableRay"></param>
        private void ConfigureRay(Vector3 origin, Vector3 vector, bool enableRay)
        {
            ray.enabled = enableRay;
            if (!enableRay) return;
            
            rayOrigin.transform.position = Vector3.Lerp(rayOrigin.transform.position, origin, .75f);
            rayOrigin.transform.forward = Vector3.Lerp(rayOrigin.transform.forward, vector, .5f);
            
            ray.Bezier(rayOrigin.transform.position, Center(), rayEnd.transform.position);
        }
        /// <summary>
        /// Laggy dynamic center point for linerenderer
        /// </summary>
        /// <returns></returns>
        private Vector3 Center()
        {
            float z = rayCenter.transform.localPosition.z;
            rayCenter.transform.localPosition = new Vector3(0,0, Mathf.Lerp(z, Vector3.Distance(rayOrigin.transform.position, rayEnd.transform.position), .5f));
            return rayCenter.transform.position;
        }
        
        protected override void Select()
        {
            if (!extant) return;
            objectInstance.Select();
            InteractionController.SetFocusObject(objectInstance, orientation);
        }

        protected override void GrabStart()
        {
            if (!extant) return;
            objectInstance.GrabStart(orientation, Mode.INDIRECT);
        }

        protected override void GrabStay()
        {
            if (!extant) return;
            //objectInstance.GrabStay();
        }

        protected override void GrabEnd()
        {
            if (!extant) return;
            // Currently this doesn't need to be called because other listeners are added elsewhere
            // It also may not be the current object when this is triggered
            //objectInstance.GrabEnd();
        }
    }
}
