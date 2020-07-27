using System;
using System.Collections.Generic;
using System.Linq;
using DG.Tweening;
using Spaces.Scripts.Objects;
using Spaces.Scripts.Objects.Object_Classes;
using Spaces.Scripts.Player;
using Spaces.Scripts.Space.Space_Classes;
using Spaces.Scripts.Utilities;
using UnityEngine;

namespace Spaces.Scripts.Space
{
    public class SpaceInstance : MonoBehaviour
    {
        private int index;
        private bool holding;
        private ObjectInstance spaceObject;
        private SpaceInstance parentSpace;
        private SpaceClass spaceClass;
        private Color spaceColour;
        public List<SpaceData> objectInstances = new List<SpaceData>();
        private static SpaceManager SpaceManager => Reference.SpaceManager();
        private enum SpaceStates { LOADED, UNLOADED, TOTEMISED }
        private SpaceStates spaceState;

        // ------------------------------------------------------------------------------------------------------------

        /// <summary>
        /// Struct used to store data about the space when it has been totemised
        /// </summary>
        [Serializable] public class SpaceData
        {
            public ObjectInstance objectInstance;
            public ObjectClass objectClass;
            public ObjectInstance.RelativeTransform.Location objectLocation;
            public ObjectInstance.TotemState objectState;
            /// <summary>
            /// Input object instance, output space data instance
            /// </summary>
            /// <param name="objectInstanceReference"></param>
            /// <returns></returns>
            public static SpaceData SetData(ObjectInstance objectInstanceReference)
            {
                SpaceData spaceData = new SpaceData
                {
                    objectInstance = objectInstanceReference,
                    objectClass = objectInstanceReference.objectClass,
                    objectLocation = objectInstanceReference.relativeTransform.CurrentLocation,
                    objectState = objectInstanceReference.totemState
                };
                return spaceData;
            }
        }

        /// <summary>
        /// Used to initialise the space instance
        /// </summary>
        /// <param name="instance"></param>
        /// <param name="space"></param>
        /// <param name="spaceIndex"></param>
        public void Initialise(ObjectInstance instance, SpaceClass space, int spaceIndex)
        {
            // Cache values
            spaceObject = instance;
            spaceClass = space;
            index = spaceIndex;
            // Set visual effects determined by this space
            spaceColour = spaceClass.spaceColours[index];
            spaceObject.ObjectTotem.SetTotemColour(spaceColour);
        }

        /// <summary>
        /// 
        /// </summary>
        public void OnBecomeActiveSpace()
        {
            // Change the background colour of the main camera to that of the scene
            SpaceManager.SetCameraColour(spaceColour);
            // todo make the colour of the space totem match the colour of the space
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="objectInstance"></param>
        public void AddObject(ObjectInstance objectInstance)
        {
            if (Extant(objectInstance)) return;
            objectInstances.Add(SpaceData.SetData(objectInstance));
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="objectInstance"></param>
        public void RemoveObject(ObjectInstance objectInstance)
        {
            //if (!Extant(objectInstance)) return;
            //objectInstances.Remove(objectInstance);
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="objectInstance"></param>
        /// <returns></returns>
        private bool Extant(ObjectInstance objectInstance)
        {
            return false;
            //return objectInstances.Contains(objectInstance);
        }
        /// <summary>
        /// Called when totemising a space
        /// Handles the logic for recalling all objects registered with that space
        /// Turns the space object into a totem
        /// Triggers direct manipulation of the space totem as well
        /// </summary>
        public void TotemiseSpace(Transform target, ControllerTransforms.Check check)
        {
            // Manipulation logic for the totem, comes before the next section because of DoTween references
            transform.Transforms(target);
            spaceObject.ObjectTotem.RecenterObjectTotem();
            spaceObject.GrabStart(check, Interaction.Mode.DIRECT, totemisedSpace: true);
            // Totemise each of the objects in the space
            foreach (SpaceData spaceData in objectInstances)
            {
                // Set all the states needed for the object
                spaceData.objectLocation = spaceData.objectInstance.relativeTransform.CurrentLocation;
                spaceData.objectState = spaceData.objectInstance.totemState;
                // Unload the objects
                spaceData.objectInstance.transform.DOMove(transform.position, .25f);
                spaceData.objectInstance.transform.DOScale(Vector3.zero, .25f).OnComplete(() =>
                {
                    spaceData.objectInstance.gameObject.SetActive(false);
                });
            }
            // Then turn the space into a totem
            spaceObject.SetTotemState(ObjectInstance.TotemState.TOTEM);
            // Register the totem in the new scene
            SpaceManager.ObjectRegistration(spaceObject);
            // Set the space state
            SetSpaceState(SpaceStates.TOTEMISED);
        }
        /// <summary>
        /// 
        /// </summary>
        public void LoadSpace(bool load)
        {
            // Reload in all the objects in this space
            foreach (SpaceData spaceData in objectInstances)
            {
                // Cache the reference
                SpaceData data = spaceData;
                // Reenable the objects in this space
                data.objectInstance.gameObject.SetActive(true);
                // Make them reappear from the space totem
                data.objectInstance.CreateExtantObject(data, transform.position, load);
            }
            // Then turn the space back into an object
            spaceObject.SetTotemState(ObjectInstance.TotemState.OBJECT);
            // Set the space state
            SetSpaceState(SpaceStates.LOADED);
        }
        /// <summary>
        /// Method called to unload a scene
        /// This is called when a child space is loaded and this space is its parent space
        /// </summary>
        public void UnloadSpace(SpaceInstance childSpace)
        {
            // Reload in all the objects in this space
            foreach (SpaceData spaceData in objectInstances.Where(spaceData => childSpace.gameObject != spaceData.objectInstance.gameObject))
            {
                spaceData.objectInstance.gameObject.SetActive(false);
            }
            // Set the space state
            SetSpaceState(SpaceStates.UNLOADED);
        }
        /// <summary>
        /// Wrapper used to set the state of the space
        /// For the time being this is just a way for me to keep track of the space state
        /// </summary>
        /// <param name="state"></param>
        private void SetSpaceState(SpaceStates state)
        {
            spaceState = state;
        }
        /// <summary>
        /// Sets the parent space of this space
        /// </summary>
        /// <param name="parent"></param>
        internal void SetParentSpace(SpaceInstance parent)
        {
            if (parent == this)
            {
                Debug.Log($"<b>{name}</b> Parenting: <b>{name}</b> is the active space, cannot set itself as its own parent space");
            }
            else
            {
                parentSpace = parent;
                Debug.Log($"<b>{name}</b> Parenting: <b>{parent.name}</b> set as parent space of <b>{name}</b>");
            }
        }
        /// <summary>
        /// Returns the parent space of this space
        /// </summary>
        /// <returns></returns>
        public SpaceInstance ParentSpace()
        {
            return parentSpace;
        }
        /// <summary>
        /// Sets the state of the holding check for this space
        /// This bool allows us to check whether or not to load the space with the space manager
        /// </summary>
        /// <param name="state"></param>
        public void SetHoldingState(bool state)
        {
            holding = state;
        }
        // ------------------------------------------------------------------------------------------------------------
        private void LateUpdate()
        {
            if (holding && SpaceManager.LoadSpace(transform.position))
            {
                SpaceManager.LoadSpace(this);
            }
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="other"></param>
        private void OnTriggerEnter(Collider other)
        {
            if (other.name == SpaceManager.SpaceButtonName && holding) SpaceManager.LoadSpace(this);
        }
    }
}
