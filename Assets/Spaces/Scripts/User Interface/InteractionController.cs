using System;
using Delayed_Messaging.Scripts.Player;
using Delayed_Messaging.Scripts.Utilities;
using Spaces.Scripts.User_Interface.Interface_Elements;
using UnityEngine;

namespace Spaces.Scripts.User_Interface
{
    [RequireComponent(typeof(ControllerTransforms))]
    public class InteractionController : MonoBehaviour
    {
        [Header("Interaction Settings")]
        [SerializeField, Range(1, 50)] private float range;
        [SerializeField] private Material material;
        
        private ControllerTransforms Controller => GetComponent<ControllerTransforms>();
        private IndirectInteraction dominant, nonDominant;

        private void Awake()
        {
            // Initialise
            nonDominant = Controller.Transform(ControllerTransforms.Check.LEFT).gameObject.AddComponent<IndirectInteraction>();
            dominant = Controller.Transform(ControllerTransforms.Check.RIGHT).gameObject.AddComponent<IndirectInteraction>();
            nonDominant.Initialise(
                gameObject, 
                "[Non-Dominant]", 
                material);
            dominant.Initialise(
                gameObject, 
                "[Dominant]", 
                material);
            
            // Add event listeners for both controllers
            Controller.SelectEvent(
                ControllerTransforms.Check.LEFT, 
                ControllerTransforms.EventTracker.EventType.END).AddListener(nonDominant.ButtonSelect);
            Controller.SelectEvent(
                ControllerTransforms.Check.RIGHT, 
                ControllerTransforms.EventTracker.EventType.END).AddListener(dominant.ButtonSelect);
        }

        private void Update()
        {
            dominant.Check(
                Controller.Position(ControllerTransforms.Check.RIGHT), 
                Controller.ForwardVector(ControllerTransforms.Check.RIGHT), 
                range,
                true);
            nonDominant.Check(
                Controller.Position(ControllerTransforms.Check.LEFT),
                Controller.ForwardVector(ControllerTransforms.Check.LEFT), 
                range,
                true);
        }
    }
}
