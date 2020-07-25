using System;
using System.Collections.Generic;
using Spaces.Scripts.Objects;
using Spaces.Scripts.Objects.Object_Classes;
using Spaces.Scripts.Objects.Object_Creation;
using Spaces.Scripts.Player;
using Spaces.Scripts.Space.Space_Classes;
using Spaces.Scripts.User_Interface.Interface_Elements;
using Spaces.Scripts.Utilities;
using UnityEngine;
using Object = UnityEngine.Object;

namespace Spaces.Scripts.Space
{
    public class SpaceManager : MonoBehaviour
    {
        [SerializeField] private ObjectClass spaceClass;
        public Button spaceButton;
        
        [Space(10)] public List<SpaceInstance> spaces = new List<SpaceInstance>();
        
        private GameObject spaceButtonParent;
        private SpaceInstance activeSpace;

        private static ControllerTransforms Controller => Reference.Player().GetComponent<ControllerTransforms>();

        // ------------------------------------------------------------------------------------------------------------
        
        private void InitialiseSpaceButton()
        {
            // Create the parent for the space button
            spaceButtonParent = Set.Object(gameObject, "[Space Button]", Vector3.zero);
            
            // Add and configure the collider
            SphereCollider buttonCollider = spaceButtonParent.AddComponent<SphereCollider>();
            buttonCollider.radius = .15f;
            buttonCollider.isTrigger = true;
            
            // Add and configure the button component
            //spaceButton = spaceButtonParent.AddComponent<Button>();
            //spaceButton.ConfigureInterface(BaseInterface.TriggerType.GRAB, BaseInterface.InteractionType.DIRECT);
            
            // Add listener to create new scene
            spaceButton.buttonSelect.AddListener(CreateSpace);
        }
        /// <summary>
        /// Wrapper to create a new scene
        /// </summary>
        private void CreateSpace()
        {
            SpaceInstance cachedSpace = ActiveSpace();
            
            // Load a new space, if the active space is null, or the active space doesn't have a parent,
            // a new space will be made
            string parent = cachedSpace.ParentSpace() == null ? "NO PARENT" : cachedSpace.ParentSpace().name;
            Debug.Log($"<b>{cachedSpace.name}</b> is loading its parent space, <b>{parent}</b>");
            LoadSpace(cachedSpace.ParentSpace());
            
            // Take the active scene and totemise it, that will then be added to the new space
            // Feed in the parent of the current space so it can be unloaded
            cachedSpace.TotemiseSpace();
        }
        /// <summary>
        /// Loads the supplied space
        /// If there isn't one supplied it will create a new one
        /// </summary>
        /// <param name="spaceInstance"></param>
        internal void LoadSpace(SpaceInstance spaceInstance = null)
        {
            // If a space is supplied, set that to active, otherwise create a new one
            if (spaceInstance != null)
            {
                Debug.Log($"Loading a supplied space: {spaceInstance.name}");
                SetActiveSpace(spaceInstance);
                spaces.Add(ActiveSpace());
            }
            else
            {
                Debug.Log($"Tried to load a null space, created a new space instead!");
                spaces.Add(NewActiveSpace());
            }

            // Load that active scene up
            ActiveSpace().LoadSpace(load: false);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="unloadSpace"></param>
        /// <param name="loadSpace"></param>
        public static void UnloadSpace(SpaceInstance unloadSpace, SpaceInstance loadSpace)
        {
            Debug.Log($"The space <b>{unloadSpace.name}</b> is being unloaded by <b>{loadSpace.name}</b>");
            unloadSpace.UnloadSpace(loadSpace);
        }
        /// <summary>
        /// Generates a new space and makes it the active space
        /// </summary>
        /// <returns></returns>
        private SpaceInstance NewActiveSpace()
        {
            // Create a new gameobject, and parent it to the space manager
            ObjectCreatorManager.CreateSpace($"[Space {spaces.Count + 1}]", spaceClass, gameObject, out SpaceInstance spaceInstance);

            // Create a new space instance, then add it to that placeholder gameobject, then make it the active scene
            SetActiveSpace(spaceInstance);
            
            // Return the active space
            return ActiveSpace();
        }
        /// <summary>
        /// Returns the active space
        /// </summary>
        /// <returns></returns>
        internal SpaceInstance ActiveSpace()
        {
            return activeSpace;
        }
        /// <summary>
        /// Sets a supplied space to the active space
        /// </summary>
        /// <param name="spaceInstance"></param>
        public void SetActiveSpace(SpaceInstance spaceInstance)
        {
            Debug.Log($"<b>{spaceInstance.name}</b> set as Active Space");
            activeSpace = spaceInstance;
            
            return;
            // todo this is a really janky way of doing this
            activeSpace.gameObject.SetActive(true);
            
            /*
             // i tried to do this in the unloading loop in space instance but it didnt work and i dont know why
            // todo add a check to make sure you don't unload the scene you just loaded
                if (childSpace == spaceData.objectInstance)
                {
                    Debug.Log($"Skipped unloading {childSpace.name}");
                    continue;
                }
            */
        }
        /// <summary>
        /// Adds a supplied object to the active scene
        /// </summary>
        /// <param name="objectInstance"></param>
        public void ObjectRegistration(ObjectInstance objectInstance)
        {
            // Only gets called when making your first scene, after that there will always be an active scene
            if (ActiveSpace() == null) return;
            Debug.Log($"Registering {objectInstance.name} with {ActiveSpace().name}");
            ActiveSpace().AddObject(objectInstance);
        }
        
        // ------------------------------------------------------------------------------------------------------------
        
        private void Awake()
        {
            // Set the tag of this so it can be found
            gameObject.tag = Reference.SpaceManagerTag;

            // Initialise the button to trigger a new scene 
            InitialiseSpaceButton();
            
            // Load in the active scene - if no active scene is provided, an empty one will be created
            //
            // NewActiveSpace(defaultSpace.GetComponent<SpaceInstance>());
            LoadSpace(ActiveSpace());
        }

        private void FixedUpdate()
        {
            spaceButtonParent.transform.LerpTransform(Controller.Transform(ControllerTransforms.Check.HEAD), .75f);
        }
    }
}
