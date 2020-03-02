using System;
using TMPro;
using UnityEngine;


namespace Delayed_Messaging.Scripts.Interaction
{
    public class RaycastButton : RaycastInterface
    {
        [Header("Raycast Button Settings")]
        [SerializeField] private TextMeshPro buttonLabel;
        [SerializeField] private string buttonText;

        protected override void Initialise()
        {
            SetButtonText(buttonText);
        }

        public void SetButtonText(string text)
        {
            buttonLabel.SetText(text);
        }
    }
}
