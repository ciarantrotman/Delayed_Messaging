using Spaces.Scripts.Player;
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
        [SerializeField] private Button button;
        [Header("Label Settings")]
        [SerializeField] private TextMeshPro label;
        [SerializeField] private string labelText;

        private ObjectCreatorManager manager;
        
        private void Awake()
        {
            label.SetText(labelText);
            button.buttonSelect.AddListener(CreateObject);
            
            //todo add reference to object creator manager
        }
        /// <summary>
        /// Creates a placeholder gameobject, then adds a blank object script to it , then configures it using the object class provided
        /// </summary>
        private void CreateObject()
        {
            // Create placeholder object, named after the object
            GameObject placeholder = Set.Object(null, labelText);
            placeholder.transform.position = new Vector3(-1.5f, 1.5f, 1.5f); // todo change this to object creator manager reference
            // Add required scripts (object totem has to be added first!)
            ObjectTotem objectTotem = placeholder.AddComponent<ObjectTotem>();
            BaseObject baseObject = placeholder.AddComponent<BaseObject>();
            // Create the object
            baseObject.CreateObject(objectClass);
        }
    }
}
