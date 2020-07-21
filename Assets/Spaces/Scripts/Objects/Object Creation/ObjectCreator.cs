using Spaces.Scripts.Player;
using Spaces.Scripts.Space;
using Spaces.Scripts.User_Interface.Interface_Elements;
using Spaces.Scripts.Utilities;
using TMPro;
using UnityEngine;

namespace Spaces.Scripts.Objects
{
    public class ObjectCreator : MonoBehaviour
    {
        [Header("Core References")]
        [SerializeField] private ObjectClass objectClass;
        [Header("Label Settings")]
        [SerializeField] private string labelText;

        private Button Button => GetComponentInChildren<Button>();
        private TextMeshPro Label => GetComponentInChildren<TextMeshPro>();
        private static ObjectCreatorManager ObjectCreatorManager => Reference.Player().GetComponent<ObjectCreatorManager>();
        private static SpaceManager SpaceManager => Reference.Player().GetComponent<SpaceManager>();
        
        /// <summary>
        /// Sets the state of the button at start and adds listeners for object creation
        /// </summary>
        private void Awake()
        {
            Label.SetText(labelText);
            Button.buttonSelect.AddListener(CreateObject);
        }
        
        /// <summary>
        /// Creates a placeholder gameobject, then adds a blank object script to it , then configures it using the object class provided
        /// </summary>
        private void CreateObject()
        {
            // Create placeholder object, named after the object
            GameObject placeholder = Set.Object(null, labelText, ObjectCreatorManager.CreationLocation());
            // Add required scripts (object totem has to be added first!)
            placeholder.AddComponent<ObjectTotem>();
            ObjectInstance objectInstance = placeholder.AddComponent<ObjectInstance>();
            // Create the object
            objectInstance.CreateObject(objectClass);
        }
    }
}
