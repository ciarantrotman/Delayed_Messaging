using Spaces.Scripts.Objects.Object_Interaction;
using Spaces.Scripts.User_Interface.Interface_Elements;
using TMPro;
using UnityEngine;

namespace Spaces.Scripts.Objects.Totem
{
    public class ObjectTotemiser : MonoBehaviour
    { 
        [Header("Label Settings")]
        [SerializeField] private string labelText;
        private static ObjectInteractionController ObjectInteractionController => Reference.Player().GetComponent<ObjectInteractionController>();
        private Button Button => GetComponentInChildren<Button>();
        private TextMeshPro Label => GetComponentInChildren<TextMeshPro>();

        private void Awake()
        {
            Label.SetText(labelText);
            Button.buttonSelect.AddListener(ToggleState);
        }
        private static void ToggleState()
        {
            ObjectInteractionController.FocusObject()?.ToggleTotemState();
        }
    }
}
