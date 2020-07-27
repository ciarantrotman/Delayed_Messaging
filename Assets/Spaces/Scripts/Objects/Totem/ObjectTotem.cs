using System;
using Spaces.Scripts.Objects.Object_Classes;
using Spaces.Scripts.Utilities;
using UnityEngine;
using VR_Prototyping.Plugins.QuickOutline.Scripts;

namespace Spaces.Scripts.Objects.Totem
{
    public class ObjectTotem : MonoBehaviour
    {
        private bool extant, outline;
        public GameObject objectObject, totemObject;
        public Outline objectOutline, totemOutline;
        private SpaceTotem spaceTotem;
        private ObjectClass.ObjectPhysics objectPhysics = new ObjectClass.ObjectPhysics();
        
        // ------------------------------------------------------------------------------------------------------------

        /// <summary>
        /// Called only once, when the object is first created
        /// </summary>
        /// <param name="parent"></param>
        /// <param name="objectClass"></param>
        /// <param name="instance"></param>
        public void InstantiateObjectTotem(Transform parent, ObjectClass objectClass, ObjectInstance instance = null)
        {
            if (extant) return;
            extant = true;
            // Create the relevant objects
            objectObject = Create(objectClass.objectObject, parent);
            totemObject = Create(objectClass.totemObject, parent);
            // Add outlines for the objects
            objectOutline = objectObject.Outline(objectClass.objectOutline);
            totemOutline = totemObject.Outline(objectClass.totemOutline);
            // Create space totem references
            spaceTotem = totemObject.AddComponent<SpaceTotem>();
            // Create the rigidbodies for the children
            objectPhysics.SetObjectPhysics(useGravity: false, isKinematic: true);
            objectObject.AddComponent<Rigidbody>().Rigidbody(objectPhysics);
            totemObject.AddComponent<Rigidbody>().Rigidbody(objectPhysics);
        }
        /// <summary>
        /// Creates a new object and sets it to inactive in the same frame
        /// </summary>
        /// <param name="prefab"></param>
        /// <param name="parent"></param>
        /// <returns></returns>
        private static GameObject Create(GameObject prefab, Transform parent)
        {
            GameObject placeholder = Instantiate(prefab, parent);
            placeholder.tag = ObjectInstance.Object;
            placeholder.SetActive(false);
            placeholder.transform.localPosition = Vector3.zero;
            return placeholder;
        }
        /// <summary>
        /// Public wrapper to set the state of the object to a totem
        /// </summary>
        public void Totemise()
        {
            SetState(ObjectInstance.TotemState.TOTEM);
        }
        /// <summary>
        /// Public wrapper to set the state of the object to an object
        /// </summary>
        public void Objectise()
        {
            SetState(ObjectInstance.TotemState.OBJECT);
        }
        // Handles the logic for transitioning between totem and object
        private void SetState(ObjectInstance.TotemState state)
        {
            // Only allow this to be called once the objects have been spawned
            if (!extant) return;
                
            switch (state)
            {
                case ObjectInstance.TotemState.TOTEM:
                    // Set the outline for the active object to false, then disable the object
                    if (outline)
                    {
                        Outline(ObjectInstance.TotemState.OBJECT, false);
                    }
                    objectObject.SetActive(false);
                    // Set the totem active, then enable the outline
                    totemObject.SetActive(true);
                    if (outline)
                    {
                        Outline(ObjectInstance.TotemState.TOTEM, true);
                    }
                    break;
                case ObjectInstance.TotemState.OBJECT:
                    // Disable the outline on the totem, then disable the totem
                    if (outline)
                    {
                        Outline(ObjectInstance.TotemState.TOTEM, false);
                    }
                    totemObject.SetActive(false);
                    // Then enable the object and enable its outline
                    objectObject.SetActive(true);
                    if (outline)
                    {
                        Outline(ObjectInstance.TotemState.OBJECT, true);
                    }
                    break;
                default:
                    break;
            }
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="totemState"></param>
        /// <param name="state"></param>
        public void Outline(ObjectInstance.TotemState totemState, bool state)
        {
            outline = state;
            switch (totemState)
            {
                case ObjectInstance.TotemState.TOTEM:
                    totemOutline.enabled = state;
                    break;
                case ObjectInstance.TotemState.OBJECT:
                    objectOutline.enabled = state;
                    break;
                default:
                    return;
            }
        }
        /// <summary>
        /// Used currently for setting the colour of the space totem, but can be expanded in future
        /// </summary>
        /// <param name="color"></param>
        public void SetTotemColour(Color color)
        {
            // Set the outline colour of the totem
            totemOutline.OutlineColor = color;
            spaceTotem.SetTotemColour(color);
        }
        /// <summary>
        /// 
        /// </summary>
        public void RecenterObjectTotem()
        {
            objectObject.transform.localPosition = Vector3.zero;
            totemObject.transform.localPosition = Vector3.zero;
        }
    }
}
