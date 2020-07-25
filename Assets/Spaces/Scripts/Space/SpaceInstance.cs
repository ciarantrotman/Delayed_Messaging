using System;
using System.Collections.Generic;
using DG.Tweening;
using Spaces.Scripts.Objects;
using Spaces.Scripts.Objects.Object_Classes;
using Spaces.Scripts.Objects.Object_Creation;
using Spaces.Scripts.Objects.Totem;
using Spaces.Scripts.Player;
using Spaces.Scripts.Space.Space_Classes;
using UnityEngine;
using Random = UnityEngine.Random;

namespace Spaces.Scripts.Space
{
    public class SpaceInstance : MonoBehaviour
    {
        private ObjectInstance spaceObject;
        private SpaceInstance parentSpace;
        private SpaceClass spaceClass;
        private Color spaceColour;
        public List<SpaceData> objectInstances = new List<SpaceData>();
        private static SpaceManager SpaceManager => Reference.SpaceManager();
        private static ObjectCreatorManager ObjectCreatorManager => Reference.Player().GetComponent<ObjectCreatorManager>();
        private enum SpaceStates { LOADED, UNLOADED, TOTEMISED }
        private SpaceStates SpaceState { set; get; }
        private int index;

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
            spaceColour = spaceClass.spaceColours[Random.Range(0, spaceClass.spaceColours.Count - 1)];
            spaceObject.ObjectTotem.SetTotemColour(spaceColour);
            
            //spaceObject.objectise.AddListener(LoadSpace);
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
        /// 
        /// </summary>
        public void TotemiseSpace(Vector3 position)
        {
            // todo, something better than this, it will need to go to the hand that you used to grab, need to wait to implement manipulation to do this though
            transform.position = position;
            
            // If this space has a parent space, then load it up
            if (parentSpace != null)
            {
                Debug.Log($"<b>{parentSpace.name}</b> was loaded by <b>{name}</b> because it's its parent space");
                SpaceManager.LoadSpace(parentSpace);
            }
            
            // Totemise each of the objects in the space
            foreach (SpaceData spaceData in objectInstances)
            {
                // Cache the reference
                // Set all the states needed for the object
                spaceData.objectLocation = spaceData.objectInstance.relativeTransform.CurrentLocation;
                spaceData.objectState = spaceData.objectInstance.totemState;
                
                // Totemise the objects in the space
                //data.objectInstance.SetTotemState(ObjectInstance.TotemState.TOTEM);
                
                // Unload the objects
                // todo, move this to its own method
                Debug.Log($"Totemising <b>{name}</b>: <b>{spaceData.objectInstance.name}</b> was unloaded from the space");
                spaceData.objectInstance.transform.DOMove(transform.position, 1f);
                spaceData.objectInstance.transform.DOScale(Vector3.zero, 1f);
                //data.objectInstance.gameObject.SetActive(false);
                //Destroy(data.objectInstance.gameObject);
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
            // todo: cache the current active space, but then when the space is totemised it reloads that parent scene
            // This will set the current active space to the parent of this space, but not if its itself
            SetParentSpace(SpaceManager.ActiveSpace() == this ? null : SpaceManager.ActiveSpace());
            if (parentSpace != null)
            {
                SpaceManager.UnloadSpace(unloadSpace: parentSpace, loadSpace: this);
            }

            // Sets this as the current active space 
            SpaceManager.SetActiveSpace(this);
            
            // Reload in all the objects in this space
            foreach (SpaceData spaceData in objectInstances)
            {
                // Cache the reference
                SpaceData data = spaceData;
                
                Debug.Log($"<b>{name}</b> Loading: <b>{data.objectInstance.name}</b> was loaded ");
                
                // Create the extant object
                data.objectInstance.gameObject.SetActive(true);
                // todo, supply a meaningful position for the object to spawn from here
                data.objectInstance.CreateExtantObject(data, spaceObject.transform.position, load);
            }
            
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
            foreach (SpaceData spaceData in objectInstances)
            {
                Debug.Log($"<b>{name}</b> Unloading: <b>{spaceData.objectInstance.name}</b> is being unloaded");
                
                // todo add a check to make sure you don't unload the scene you just loaded
                // ugh this is really messy but I don't have much choice here?
                // This is totally dependant on spaces having separate gameobjects...
                if (childSpace.gameObject == spaceData.objectInstance.gameObject)
                {
                    Debug.Log($"Skipped unloading <b>{childSpace.name}</b> from <b>{name}</b>, as it is being loaded as the active space");
                    continue;
                }
                
                // Unload the object
                //data.objectInstance.CreateExtantObject(data, spaceObject.transform.position);
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
            SpaceState = state;
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="parent"></param>
        public void SetParentSpace(SpaceInstance parent)
        {
            parentSpace = parent;
        }
        public SpaceInstance ParentSpace()
        {
            return parentSpace;
        }
    }
}
