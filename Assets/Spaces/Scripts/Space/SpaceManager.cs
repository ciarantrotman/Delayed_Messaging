using System.Collections.Generic;
using DG.Tweening;
using Spaces.Scripts.Objects;
using Spaces.Scripts.Objects.Object_Classes;
using Spaces.Scripts.Objects.Object_Creation;
using Spaces.Scripts.Player;
using Spaces.Scripts.Space.Space_Classes;
using Spaces.Scripts.User_Interface.Interface_Elements;
using Spaces.Scripts.Utilities;
using UnityEngine;

namespace Spaces.Scripts.Space
{
    public class SpaceManager : MonoBehaviour
    {
        [SerializeField] private ObjectClass objectClass;
        [SerializeField] private SpaceClass spaceClass;
        [SerializeField] private GameObject spaceButton;
        [SerializeField, Range(.1f, .5f)] private float headRadius = .175f;

        [Space(10)] public List<SpaceInstance> spaces = new List<SpaceInstance>();

        private static ControllerTransforms Controller => Reference.Controller();
        private static Camera Camera => Reference.Camera();
        public const string SpaceButtonName = "[Space Button]";

        private Button SpaceButton => spaceButton.GetComponent<Button>();
        private SpaceInstance activeSpace;
        
        // ------------------------------------------------------------------------------------------------------------
        
        private void InitialiseSpaceButton()
        {
            // Create the parent for the space button
            spaceButton = Instantiate(spaceButton, transform);
            spaceButton.name = SpaceButtonName;
            
            // Add and configure the collider, must be done before adding button component
            SphereCollider buttonCollider = spaceButton.GetComponent<SphereCollider>();
            buttonCollider.radius = headRadius;
            buttonCollider.isTrigger = true;
            
            // Add and configure the button component
            SpaceButton.ConfigureInterface(BaseInterface.TriggerType.GRAB, BaseInterface.InteractionType.DIRECT);
            
            // Add listener to create new scene
            SpaceButton.grabStart.AddListener(CreateSpace);
        }
        /// <summary>
        /// Wrapper to create a new scene
        /// </summary>
        private void CreateSpace()
        {
            SpaceInstance cachedSpace = ActiveSpace();
            // Load a new space, if the active space is null, or the active space doesn't have a parent, a new space will be made
            string debugText = cachedSpace.ParentSpace() == null ? "is a root space, cannot load a parent space" : $"is loading its parent space, <b>{cachedSpace.ParentSpace().name}</b>";
            Debug.Log($"Space Totemisation <b>{cachedSpace.name}</b> {debugText}");
            LoadSpace(cachedSpace.ParentSpace(), setParent: false);
            // Take the active scene and totemise it, that will then be added to the new space
            cachedSpace.TotemiseSpace(Controller.Transform(SpaceButton.check), SpaceButton.check);
        }

        /// <summary>
        /// Loads the supplied space
        /// If there isn't one supplied it will create a new one
        /// </summary>
        /// <param name="spaceInstance"></param>
        /// <param name="setParent"></param>
        internal void LoadSpace(SpaceInstance spaceInstance = null, bool setParent = true)
        {
            // If a space is supplied, set that to active, otherwise create a new one
            if (spaceInstance != null)
            {
                Debug.Log($"Loading a supplied space: <b>{spaceInstance.name}</b>");
                // Only do this if you want to register the current active space
                if (setParent)
                {
                    // Cache the reference to the current active space
                    spaceInstance.SetParentSpace(parent: ActiveSpace());
                }
                // Unload the active space (which would be the parent you dolt)
                UnloadSpace(unloadSpace: ActiveSpace(), loadSpace: spaceInstance);
                // Set the supplied space to the active space
                SetActiveSpace(spaceInstance: spaceInstance);
                // Register that new active space
                SpaceRegistration(space: ActiveSpace());
            }
            else
            {
                SpaceRegistration(NewActiveSpace());
                Debug.Log($"Created root space: <b>{ActiveSpace().name}</b>");
            }
            // Load that new active space up
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
        /// This is a check 
        /// </summary>
        /// <param name="spacePosition"></param>
        /// <returns></returns>
        public bool LoadSpace(Vector3 spacePosition)
        {
            return Vector3.Distance(spacePosition, spaceButton.transform.position) <= headRadius;
        }
        /// <summary>
        /// Generates a new space and makes it the active space
        /// </summary>
        /// <returns></returns>
        private SpaceInstance NewActiveSpace()
        {
            // Create a new gameobject, and parent it to the space manager
            int index = spaces.Count + 1;
            ObjectCreatorManager.CreateSpace($"[Space {index}]", objectClass, spaceClass, index - 1, gameObject, out SpaceInstance spaceInstance);

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
            if(spaceInstance == activeSpace) return;
            
            // Set the active space to the supplied space
            Debug.Log($"<b>{spaceInstance.name}</b> set as Active Space");
            activeSpace = spaceInstance;
            
            // Make any changes required of the new space
            activeSpace.OnBecomeActiveSpace();
        }
        /// <summary>
        /// Adds a supplied object to the active scene
        /// </summary>
        /// <param name="objectInstance"></param>
        public void ObjectRegistration(ObjectInstance objectInstance)
        {
            // Only gets called when making your first scene, after that there will always be an active scene
            if (ActiveSpace() == null) return;
            
            Debug.Log($"Registering Object: <b>{objectInstance.name}</b> with Space: <b>{ActiveSpace().name}</b>");
            
            // Register it with the active space
            ActiveSpace().AddObject(objectInstance);
            
            // Communicate that downwards too
            objectInstance.SetRegisteredSpace(ActiveSpace());
        }
        /// <summary>
        /// Add a supplied space to the master list of spaces
        /// </summary>
        /// <param name="space"></param>
        private void SpaceRegistration(SpaceInstance space)
        {
            // Only add a space to spaces if it isn't already registered
            if (!spaces.Contains(space))
            {
                spaces.Add(space);
            }
        }
        /// <summary>
        /// Transitions the colour of the camera background colour
        /// </summary>
        /// <param name="color"></param>
        public static void SetCameraColour(Color color)
        {
            float lerp = 0;
            DOTween.To(() => lerp, x => lerp = x, 1f, 1.5f).OnUpdate(() =>
            {
                Camera.backgroundColor = Color.Lerp(Camera.backgroundColor, color, lerp);
            });
        }
        
        // ------------------------------------------------------------------------------------------------------------
        
        private void Awake()
        {
            // Set the tag of this so it can be found
            gameObject.tag = Reference.SpaceManagerTag;

            // Initialise the button to trigger a new scene 
            InitialiseSpaceButton();
            
            // Load in the active scene - if no active scene is provided, an empty one will be created
            LoadSpace(ActiveSpace());
        }

        private void FixedUpdate()
        {
            spaceButton.transform.LerpTransform(Controller.Transform(ControllerTransforms.Check.HEAD), .75f);
        }
    }
}
