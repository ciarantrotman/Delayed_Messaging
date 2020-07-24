using Spaces.Scripts.Objects.Object_Classes;
using Spaces.Scripts.Objects.Totem;
using Spaces.Scripts.Player;
using Spaces.Scripts.Space;
using Spaces.Scripts.User_Interface.Interface_Elements;
using Spaces.Scripts.Utilities;
using TMPro;
using UnityEngine;

namespace Spaces.Scripts.Objects.Object_Creation
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

        /// <summary>
        /// Sets the state of the button at start and adds listeners for object creation
        /// </summary>
        private void Awake()
        {
            Label.SetText(labelText);
            Button.buttonSelect.AddListener(CreateObject);
        }
        
        /// <summary>
        /// Wrapper for object creation manager
        /// </summary>
        private void CreateObject()
        {
            ObjectCreatorManager.CreateObject(labelText, objectClass);
        }
    }
}
