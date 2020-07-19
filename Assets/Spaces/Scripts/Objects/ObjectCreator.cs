using System;
using Delayed_Messaging.Scripts.Player;
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

        private void Awake()
        {
            label.SetText(labelText);
            button.buttonSelect.AddListener(CreateObject);
        }

        private void CreateObject()
        {
            GameObject placeholder = Set.Object(null, labelText);
            BaseObject baseObject = placeholder.AddComponent<BaseObject>();
            baseObject.CreateObject();
        }
    }
}
