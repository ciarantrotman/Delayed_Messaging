using System;
using System.Collections.Generic;
using UnityEngine;

namespace Spaces.Scripts.Objects
{
    [RequireComponent(typeof(ObjectTotem))]
    public class ObjectInstance : MonoBehaviour 
    {
        /// <summary>
        /// This is the relative transform of the object - relative to the origin of the scene that it is in
        /// </summary>
        [Serializable] public class RelativeTransform
        {
            /// <summary>
            /// A struct which represents a location in 3D space
            /// </summary>
            [Serializable] public class Location
            {
                public Vector3 position;
                public Vector3 rotation;
                /// <summary>
                /// Defines the values for this location
                /// </summary>
                /// <param name="pos"></param>
                /// <param name="rot"></param>
                public void DefineLocation(Vector3 pos, Vector3 rot)
                {
                    position = pos;
                    rotation = rot;
                }
            }
            public List<Location> locations = new List<Location>();

            /// <summary>
            /// Add the input values into a location struct, then store that in a list of locations
            /// This can be used to "undo" moves
            /// </summary>
            /// <param name="location"></param>
            public void SetRelativeTransform(Location location)
            {
                locations.Add(location);
            }
        }
        /// <summary>
        /// This is used to reference what state any given object is in
        /// </summary>
        public enum TotemState { TOTEM, OBJECT }
        //public UnityEvent totemise, objectise;

        // Core object information
        private ObjectTotem ObjectTotem => GetComponent<ObjectTotem>();
        public RelativeTransform relativeTransform = new RelativeTransform();
        public TotemState totemState;
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
                    //totemise.Invoke();
                    ObjectTotem.Totemise();
                    break;
                case TotemState.OBJECT:
                    totemState = TotemState.OBJECT;
                    //objectise.Invoke();
                    ObjectTotem.Objectise();
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
        /// Creates a new location to add to the locations list
        /// </summary>
        /// <returns></returns>
        private RelativeTransform.Location CurrentLocation()
        {
            RelativeTransform.Location location = new RelativeTransform.Location();
            Transform objectTransform = transform;
            location.DefineLocation(objectTransform.position, objectTransform.eulerAngles);
            return location;
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
                    CreateNewObject(objectClass);
                    return;
                // When the object is being removed from an inventory, space totem, or a space is being loaded 
                case true:
                    CreateExtantObject();
                    return;
                // Only reachable when no object class is provided when it's needed
                default:
                    Debug.LogError("Object Class Missing");
                    return;
            }
        }
        /// <summary>
        /// Logic for the first time the object is created
        /// </summary>
        /// <param name="objectClass"></param>
        private void CreateNewObject(ObjectClass objectClass)
        {
            // Feed through the object class data
            ObjectTotem.InstantiateObjectTotem(transform, objectClass, this);
            // Reconfigure the script to read as spawned
            ExtantState();
            // Set the state of the new object
            SetTotemState(objectClass.spawnState);
            // Set the relative transform for the object
            relativeTransform.SetRelativeTransform(CurrentLocation());
        }
        /// <summary>
        /// This is the same as "recreating" an extant object
        /// </summary>
        private void CreateExtantObject()
        {
            Debug.LogWarning("You haven't gotten to this yet!");
        }
    }
}
