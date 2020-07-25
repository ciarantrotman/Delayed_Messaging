using Spaces.Scripts.Objects.Object_Classes;
using Spaces.Scripts.Objects.Totem;
using Spaces.Scripts.Player;
using Spaces.Scripts.Space;
using Spaces.Scripts.Utilities;
using UnityEngine;

namespace Spaces.Scripts.Objects.Object_Creation
{
    [RequireComponent(typeof(ControllerTransforms))]
    public class ObjectCreatorManager : MonoBehaviour
    {
        private ControllerTransforms Controller => GetComponent<ControllerTransforms>();
        private GameObject head, creation;
        private bool initialised;

        [SerializeField, Range(1, 5)] private float defaultOffset = 2f;
        
        // ------------------------------------------------------------------------------------------------------------
        
        /// <summary>
        /// Creates a placeholder gameobject, then adds a blank object script to it , then configures it using the object class provided
        /// </summary>
        public void CreateObject(string objectName, ObjectClass objectClass, GameObject parent = null)
        {
            // Create placeholder object, named after the object
            GameObject placeholder = Set.Object(parent, objectName, CreationLocation());
            
            // Add required scripts (object totem has to be added first!)
            placeholder.AddComponent<ObjectTotem>();
            ObjectInstance objectInstance = placeholder.AddComponent<ObjectInstance>();

            // Create the object
            objectInstance.CreateNewObject(objectClass);
        }
        /// <summary>
        /// Creates a new object as standard, but adds a space instance to the created object, and returns that to whatever called it
        /// </summary>
        /// <param name="objectName"></param>
        /// <param name="objectClass"></param>
        /// <param name="parent"></param>
        /// <param name="spaceInstanceReference"></param>
        public static void CreateSpace(string objectName, ObjectClass objectClass, GameObject parent, out SpaceInstance spaceInstanceReference)
        {
            // Create placeholder object, named after the object
            GameObject placeholder = Set.Object(parent, objectName, Vector3.zero);
            
            // Add the space instance to the object
            SpaceInstance spaceInstance = placeholder.AddComponent<SpaceInstance>();
            spaceInstanceReference = spaceInstance;
            
            // Add required scripts (object totem has to be added first!)
            placeholder.AddComponent<ObjectTotem>();
            ObjectInstance objectInstance = placeholder.AddComponent<ObjectInstance>();
            
            // Initialise the space
            spaceInstance.Initialise(objectInstance);

            // Create the object, but don't register it with the scene - this avoids recursion
            objectInstance.CreateNewObject(objectClass, register: false);
        }
        /// <summary>
        /// Returns the position that the new object will be put
        /// </summary>
        /// <returns></returns>
        public Vector3 CreationLocation()
        {
            if (!initialised) return Vector3.up;
            
            // todo offset raycast position
            return Physics.Raycast(head.transform.position, head.transform.forward, out RaycastHit hit, defaultOffset) ? hit.point : creation.transform.position;
        }
        
        // ------------------------------------------------------------------------------------------------------------

        private void Awake()
        {
            head = Set.Object(gameObject, "[Creator Manager / Head]", Vector3.zero);
            creation = Set.Object(head, "[Creator Manager / Creation Location]", new Vector3(0,0, defaultOffset));
            initialised = true;
        }
        private void Update()
        {
            head.transform.LerpTransform(Controller.Transform(ControllerTransforms.Check.HEAD), .75f);
        }
    }
}
