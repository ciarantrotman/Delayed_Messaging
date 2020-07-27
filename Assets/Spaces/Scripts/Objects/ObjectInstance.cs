using System;
using System.Collections.Generic;
using DG.Tweening;
using Spaces.Scripts.Objects.Manipulation;
using Spaces.Scripts.Objects.Object_Classes;
using Spaces.Scripts.Objects.Totem;
using Spaces.Scripts.Player;
using Spaces.Scripts.Space;
using Spaces.Scripts.Utilities;
using UnityEngine;

namespace Spaces.Scripts.Objects
{
    [RequireComponent(typeof(ObjectTotem))]
    public class ObjectInstance : MonoBehaviour, IInteractive<ControllerTransforms.Check, Interaction.Mode, Boolean>
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
            public Location CurrentLocation => locations[locations.Count - 1];
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
        
        // ------------------------------------------------------------------------------------------------------------
        
        /// <summary>
        /// This is used to reference what state any given object is in
        /// </summary>
        public enum TotemState { TOTEM, OBJECT }
        public const string Object = "Object";

        // Core object information
        private static ManipulationController Manipulation => Reference.Manipulation();
        private static SpaceManager SpaceManager => Reference.SpaceManager();
        internal ObjectTotem ObjectTotem => GetComponent<ObjectTotem>();
        internal Rigidbody Rigidbody => GetComponent<Rigidbody>();
        public RelativeTransform relativeTransform = new RelativeTransform();
        private Transform parentCache;
        private SpaceInstance registeredSpace;
        public TotemState totemState;
        public ObjectClass objectClass;
        private bool extant, space;
        
        // ------------------------------------------------------------------------------------------------------------

        /// <summary>
        /// Defines the object state of the object
        /// </summary>
        /// <param name="state"></param>
        public void SetTotemState(TotemState state)
        {
            // This is how the object remembers its state when being totemised by a scene

            switch (state)
            {
                case TotemState.TOTEM:
                    totemState = TotemState.TOTEM;
                    ObjectTotem.Totemise();
                    break;
                case TotemState.OBJECT:
                    totemState = TotemState.OBJECT;
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
        /// 
        /// </summary>
        public void SetRegisteredSpace(SpaceInstance space)
        {
            registeredSpace = space;
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
        /// Logic for the first time the object is created
        /// </summary>
        /// <param name="supplyObjectClass"></param>
        /// <param name="register"></param>
        /// <param name="isSpace"></param>
        public void CreateNewObject(ObjectClass supplyObjectClass, bool register = true, bool isSpace = false)
        {
            // Cache local variables
            objectClass = supplyObjectClass;
            space = isSpace;
            SetParentCache();
            // Feed through the object class data
            ObjectTotem.InstantiateObjectTotem(transform, supplyObjectClass, this);
            // Reconfigure the script to read as spawned
            ExtantState();
            // Set the state of the new object
            SetTotemState(supplyObjectClass.spawnState);
            // Set the relative transform for the object
            relativeTransform.SetRelativeTransform(CurrentLocation());
            // Register the object with the space manager
            if (!register) return;
            SpaceManager.ObjectRegistration(this);
        }
        /// <summary>
        /// This is the same as "recreating" an extant object
        /// </summary>
        public void CreateExtantObject(SpaceInstance.SpaceData data, Vector3 origin, bool load = false)
        {
            // Set its cached totem state
            SetTotemState(data.objectState);
            // Don't respawn objects if the scene is being loaded
            if (load) return;
            // Cache reference
            Transform objectTransform = transform;
            // Snap to space totem
            objectTransform.position = origin;
            objectTransform.localScale = Vector3.zero;
            // Calculate how long it should take to reload
            float duration = Vector3.Distance(objectTransform.position, data.objectLocation.position);
            // Tween to supplied location
            objectTransform.DOLocalMove(data.objectLocation.position, duration);
            objectTransform.DOLocalRotate(data.objectLocation.rotation, duration);
            objectTransform.DOScale(Vector3.one, duration);
        }
        /// <summary>
        /// Check if this object is associated with a space totem
        /// </summary>
        /// <returns></returns>
        public bool Space()
        {
            return space;
        }
        /// <summary>
        /// 
        /// </summary>
        private void SetParentCache()
        {
            parentCache = transform.parent;
            string parent = parentCache == null ? "[Root]" : parentCache.name;
            Debug.Log($"<b>{name}</b> Object Parenting: <b>{name}</b> parent set to <b>{parent}</b>");
        }
        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        private Transform ParentCache()
        {
            return parentCache == null ? null : parentCache;
        }
        
        // ------------------------------------------------------------------------------------------------------------
        
        /// <summary>
        /// 
        /// </summary>
        public void HoverStart()
        {
            ObjectTotem.Outline(totemState, true);
        }
        /// <summary>
        /// 
        /// </summary>
        public void HoverEnd()
        {
            ObjectTotem.Outline(totemState, false);
        }
        /// <summary>
        /// 
        /// </summary>
        public void Select()
        {

        }
        /// <summary>
        /// 
        /// </summary>
        public void GrabStart(ControllerTransforms.Check check, Interaction.Mode mode, bool totemisedSpace = false)
        {
            // Cache a reference to its parent when you first grab it
            if (!totemisedSpace)
            {
                SetParentCache();
            }
            // Debug information
            Debug.Log($"Grab: <b>{name}</b> was grabbed by the {check} hand <b>{mode}LY</b>");
            
            // todo remove any listeners that were added to the object from another grab to prevent mix ups
            // todo actually better would be figuring out two handed manipulation instead
            switch (mode)
            {
                case Interaction.Mode.DIRECT:
                    Manipulation.Direct(objectInstance: this, check, totemisedSpace);
                    break;
                case Interaction.Mode.INDIRECT:
                    Manipulation.Indirect(objectInstance: this, check);
                    break;
                default:
                    return;
            }
        }
        /// <summary>
        /// 
        /// </summary>
        public void GrabStay()
        {
            Debug.Log($"{name}: Grab Stay");
        }
        /// <summary>
        /// 
        /// </summary>
        public void GrabEnd()
        {
            // Check to see if there is indeed a parent, and make sure its a manipulation parent
            if (transform.parent == null || transform.parent.GetComponent<ManipulationParent>() == null) return;
            
            // Cache references to the objects parent object and cache velocity
            GameObject parent = transform.parent.gameObject;
            Vector3 velocity = parent.GetComponent<ManipulationParent>().Velocity();
            Vector3 angular = parent.GetComponent<ManipulationParent>().AngularVelocity();
            
            // Decouple the object and destroy its parent
            transform.SetParent(ParentCache());
            Destroy(parent);
            
            // Apply grab end effects
            //Debug.Log($"<b>Manipulation</b>: {name} was thrown with a velocity of {velocity} and an angular velocity of {angular}");
            Rigidbody.velocity = velocity;
            Rigidbody.angularVelocity = angular;
            
            // todo, make this be when the object stops moving
            relativeTransform.SetRelativeTransform(CurrentLocation());
        }
    }
}
