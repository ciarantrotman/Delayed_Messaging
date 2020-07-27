using Spaces.Scripts.Player;
using Spaces.Scripts.User_Interface.Interface_Elements;
using Spaces.Scripts.Utilities;
using UnityEngine;

namespace Spaces.Scripts.Objects.Object_Interaction
{
    public class DirectObject : Interaction
    {
        private ObjectInstance objectInstance;
        private bool extantObject;
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
            if (!extantObject) return;
            objectInstance.Select();
        }

        protected override void GrabStart()
        {
            if (!extantObject) return;
            // Call the grab start function of the object
            objectInstance.GrabStart(orientation, Mode.DIRECT);
            // Remove the reference to this object
            RemoveReference();
        }
        
        protected override void GrabStay()
        {
        }
        
        protected override void GrabEnd()
        {
        }

        private void OnTriggerEnter(Collider other)
        {
            // Make sure it's an object first
            if (!other.CompareTag(tagComparison)) return;
            
            // Create a new object cache, note the object parent is going to be in the parent
            ObjectInstance newObject = other.GetComponentInParent<ObjectInstance>();
                
            // Logic for when it is a new object
            if (extantObject && objectInstance != newObject)
            {
                objectInstance.HoverEnd();
            }
                
            // Object is now that new object
            objectInstance = newObject;
            extantObject = true;
            objectInstance.HoverStart();
        }

        private void OnTriggerExit(Collider other)
        {
            if (other.CompareTag(tagComparison) && other.GetComponentInParent<ObjectInstance>() == objectInstance && extantObject)
            {
                RemoveReference();
            }
        }
        /// <summary>
        /// Remove any references you have to that object, freeing this up to do other things
        /// </summary>
        private void RemoveReference()
        {
            objectInstance.HoverEnd();
            objectInstance = null;
            extantObject = false;
        }
    }
}
