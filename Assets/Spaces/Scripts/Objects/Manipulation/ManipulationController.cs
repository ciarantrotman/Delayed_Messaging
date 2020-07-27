using Spaces.Scripts.Player;
using Spaces.Scripts.Space;
using Spaces.Scripts.Utilities;
using UnityEngine;

namespace Spaces.Scripts.Objects.Manipulation
{
    public class ManipulationController : MonoBehaviour
    {
        [SerializeField, Range(0,1)] private float directDamping = .75f, indirectDamping = .75f;
        private ObjectInstance focusObject;
        private static ControllerTransforms Controller => Reference.Controller();
        /// <summary>
        /// 
        /// </summary>
        /// <param name="objectInstance"></param>
        /// <param name="check"></param>
        /// <param name="totemisedSpace"></param>
        public void Direct(ObjectInstance objectInstance, ControllerTransforms.Check check, bool totemisedSpace = false)
        {
            // Make sure you can grab the object
            if (!objectInstance.ValidGrab() || objectInstance == focusObject) return;
            focusObject = objectInstance;
            objectInstance.SetGrabState(false);
            // todo tidy this but up, this is not a good way of doing this
            // check if the object you're grabbing has a space instance connected to it, if it does then it's a space totem
            // add a listener to the space totem to check distance from "space button"
            // when that distance goes below a threshold, load that scene
            // could move this into the object instance script?
            SpaceInstance spaceInstance = objectInstance.Space() ? objectInstance.transform.GetComponent<SpaceInstance>() : null;
            if (spaceInstance != null && !totemisedSpace) spaceInstance.SetHoldingState(true);
            // ---------------------------------------------------------------------------------------------------------
            // Stop the object from moving first
            objectInstance.Rigidbody.velocity = Vector3.zero;
            // Create a proxy parent to parent the object to
            GameObject parent = Set.Object(null, $"[Direct Parent] {objectInstance.name}", Vector3.zero);
            // Add the direct parent script to it to tell it to follow the provided transform
            parent.AddComponent<ManipulationParent>().DirectInitialise(targetTransform: Controller.Transform(check), lerp: directDamping);
            // Parent the object to it
            objectInstance.transform.SetParent(parent.transform);
            // Insert logic for when you release that object
            Controller.GrabEvent(check, ControllerTransforms.EventTracker.EventType.END).AddListener(() =>
            {
                // When you throw it, pass through this logic to throw and decouple
                Debug.Log($"<b>{objectInstance.name} Released</b>");
                objectInstance.GrabEnd();
                if (spaceInstance != null && !totemisedSpace) spaceInstance.SetHoldingState(false);
                // But also remove the listener to do that because it has been thrown
                Debug.Log($"<b>Listener Removed from {objectInstance.name}</b>");
                Controller.GrabEvent(check, ControllerTransforms.EventTracker.EventType.END).RemoveListener(objectInstance.GrabEnd);
                // Reset the grab state of the object
                objectInstance.SetGrabState(true);
                focusObject = null;
            });
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="objectInstance"></param>
        /// <param name="check"></param>
        public void Indirect(ObjectInstance objectInstance, ControllerTransforms.Check check)
        {
            // Make sure you can grab the object
            if (!objectInstance.ValidGrab() || objectInstance == focusObject) return;
            focusObject = objectInstance;
            objectInstance.SetGrabState(false);
            // Cache reference to object 
            Transform objectTransform = objectInstance.transform;
            // Create all the objects needed
            GameObject controllerProxy = Set.Object(null, "[Controller Proxy]", Controller.Position(check));
            controllerProxy.transform.rotation = Controller.Transform(check).rotation;
            GameObject parent = Set.Object(controllerProxy, "[Indirect Parent]", Vector3.zero);
            // Add the direct parent script and pass through the relevant information
            parent.AddComponent<ManipulationParent>().IndirectInitialise(controller: controllerProxy, objectPosition: objectTransform.position, check: check, lerp: indirectDamping);
            // Couple the object and the manipulation parent
            objectTransform.SetParent(parent.transform);
            // Insert logic for when you release that object
            Controller.GrabEvent(check, ControllerTransforms.EventTracker.EventType.END).AddListener(() =>
            {
                // When you throw it, pass through this logic to throw and decouple
                Debug.Log($"<b>{objectInstance.name} Released</b>");
                objectInstance.GrabEnd();
                // Destroy the cached objects
                Destroy(controllerProxy);
                // But also remove the listener to do that because it has been thrown
                Debug.Log($"<b>Listener Removed from {objectInstance.name}</b>");
                Controller.GrabEvent(check, ControllerTransforms.EventTracker.EventType.END).RemoveListener(objectInstance.GrabEnd);
                // Reset the grab state of the object
                objectInstance.SetGrabState(true);
                focusObject = null;
            });
        }
    }
}
