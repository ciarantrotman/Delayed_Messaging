using System;
using UnityEngine;
using UnityEngine.Events;

namespace Spaces.Scripts.Objects
{
    [RequireComponent(typeof(ObjectTotem))]
    public class BaseObject : MonoBehaviour 
    {
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
        public TotemState totemState;
        public RelativeTransform relativeTransform;
        private ObjectTotem ObjectTotem => GetComponent<ObjectTotem>();
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
        /// Only called once at the start, lambda expression is giving me issues
        /// </summary>
        private void AddListeners()
        {
            totemise.AddListener(() => ObjectTotem.SetState(TotemState.TOTEM));
            objectise.AddListener(() => ObjectTotem.SetState(TotemState.OBJECT));
        }
        /// <summary>
        /// Called whenever an object is created, either for the first time, or when transitioning between spaces
        /// </summary>
        public void CreateObject(ObjectClass objectClass = null)
        {
            switch (extant)
            {
                // When the object is being created the first time and a class is fed through
                case false when objectClass != null:
                    // Feed through the object class data
                    ObjectTotem.InstantiateObjectTotem(transform, objectClass);
                    // Reconfigure the script to read as spawned
                    ExtantState();
                    // Add listeners
                    AddListeners();
                    // Set the state of the new object
                    SetTotemState(objectClass.spawnState);
                    return;
                // When the object is being removed from an inventory, space totem, or a space is being loaded 
                case true:
                    return;
                // Only reachable when no object class is provided when it's needed
                default:
                    Debug.LogError("Object Class Missing");
                    return;
            }
        }
    }
}
