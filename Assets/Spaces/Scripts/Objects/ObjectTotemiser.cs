using System;
using Spaces.Scripts.User_Interface.Interface_Elements;
using TMPro;
using UnityEngine;

namespace Spaces.Scripts.Objects
{
    public class ObjectTotemiser : MonoBehaviour
    { 
        [Header("Label Settings")]
        [SerializeField] private string labelText;
        private ObjectSelectionController ObjectSelectionController => Reference.Player().GetComponent<ObjectSelectionController>();
        private Button Button => GetComponentInChildren<Button>();
        private TextMeshPro Label => GetComponentInChildren<TextMeshPro>();

        private void Awake()
        {
            Label.SetText(labelText);
            Button.buttonSelect.AddListener(ToggleState);
        }

        private void ToggleState()
        {
            ObjectSelectionController.FocusObject().ToggleTotemState();
        }
    }
}
