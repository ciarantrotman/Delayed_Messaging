using System;
using UnityEngine;
using UnityEngine.Events;

namespace Spaces.Scripts.Objects
{
    public class BaseObject : MonoBehaviour 
    {
        /// <summary>
        /// A reference to the prefabs for the object and totem
        /// </summary>
        [Serializable] public class ObjectTotem
        {
            private bool extant;
            public GameObject objectObject, totemObject;

            /// <summary>
            /// Called only once, when the object is first created
            /// </summary>
            /// <param name="parent"></param>
            /// <param name="objectClass"></param>
            public void InstantiateObjectTotem(Transform parent, ObjectClass objectClass)
            {
                if (extant) return;
                
                objectObject = Instantiate(objectClass.objectObject, parent);
                totemObject = Instantiate(objectClass.totemObject, parent);
                
                extant = true;
            }
            // Handles the logic for transitioning between totem and object
            public UnityAction SetState(TotemState state)
            {
                switch (state)
                {
                    case TotemState.TOTEM:
                        objectObject.SetActive(false);
                        totemObject.SetActive(true);
                        return null;
                    case TotemState.OBJECT:
                        objectObject.SetActive(true);
                        totemObject.SetActive(false);
                        return null;
                    default:
                        return null;
                }
            }
        }
        /// <summary>
        /// This is the relative transform of the object - relative to the origin of the scene that it is in
        /// </summary>
        [Serializable] public class RelativeTransform
        {
            public Vector3 position;
            public Vector3 rotation;
            /// <summary>
            /// Placeholder method to call when you finish moving the object
            /// </summary>
            /// <param name="pos"></param>
            /// <param name="rot"></param>
            public void SetRelativeTransform(Vector3 pos, Vector3 rot)
            {
                position = pos;
                rotation = rot;
            }
        }
        /// <summary>
        /// This is used to reference what state any given object is in
        /// </summary>
        public enum TotemState { TOTEM, OBJECT }
        public UnityEvent totemise, objectise;

        // Core object information
        public ObjectTotem objectTotem;
        public TotemState totemState;
        public RelativeTransform relativeTransform;
        private bool extant;
        
        /// <summary>
        /// Defines the object state of the object
        /// </summary>
        /// <param name="state"></param>
        private void SetTotemState(TotemState state)
        {
            switch (state)
            {
                case TotemState.TOTEM:
                    totemState = TotemState.TOTEM;
                    totemise.Invoke();
                    break;
                case TotemState.OBJECT:
                    totemState = TotemState.OBJECT;
                    objectise.Invoke();
                    break;
                default:
                    return;
            }
        }
        /// <summary>
        /// Swaps what state the object is in
        /// </summary>
        public void ToggleTotemState()
        {
            switch (totemState)
            {
                case TotemState.TOTEM:
                    SetTotemState(TotemState.OBJECT);
                    break;
                case TotemState.OBJECT:
                    SetTotemState(TotemState.TOTEM);
                    break;
                default:
                    return;
            }
        }
        /// <summary>
        /// Only ever called once the first time the object is spawned
        /// </summary>
        private void ExtantState()
        {
            extant = true;
        }
        /// <summary>
        /// Called whenever an object is created, either for the first time, or when transitioning between spaces
        /// </summary>
        public void CreateObject(ObjectClass objectClass = null)
        {
            switch (extant)
            {
                // When the object is being created the first time
                case false:
                    objectTotem.InstantiateObjectTotem(transform, objectClass);
                    ExtantState();
                    SetTotemState(TotemState.OBJECT);
                    return;
                // When the object is being removed from an inventory, space totem, or a space is being loaded 
                case true:
                    return;
            }
        }

        private void Awake()
        {
            totemise.AddListener(objectTotem.SetState(TotemState.TOTEM));
            objectise.AddListener(objectTotem.SetState(TotemState.OBJECT));
        }
    }
}
