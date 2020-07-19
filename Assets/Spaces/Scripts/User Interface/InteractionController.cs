using Delayed_Messaging.Scripts.Player;
using Spaces.Scripts.Player;
using UnityEngine;

namespace Spaces.Scripts.User_Interface
{
    [RequireComponent(typeof(ControllerTransforms))]
    public class InteractionController : MonoBehaviour
    {
        [Header("Indirect Interaction Settings")]
        [SerializeField, Range(1f, 50f)] private float range;
        [SerializeField] private Material material;
        [Header("Direct Interaction Settings")]
        [Range(0f, .05f), SerializeField] private float radius = .025f;
        
        private ControllerTransforms Controller => GetComponent<ControllerTransforms>();
        private IndirectInteraction dominantIndirect, nonDominantIndirect;
        private DirectInteraction dominantDirect, nonDominantDirect;

        private void Awake()
        {
            // Initialise Indirect Interaction
            nonDominantIndirect = Controller.Transform(ControllerTransforms.Check.LEFT).gameObject.AddComponent<IndirectInteraction>();
            dominantIndirect = Controller.Transform(ControllerTransforms.Check.RIGHT).gameObject.AddComponent<IndirectInteraction>();
            nonDominantIndirect.Initialise(
                gameObject, 
                "[Indirect / Non-Dominant]", 
                material);
            dominantIndirect.Initialise(
                gameObject, 
                "[Indirect / Dominant]", 
                material);
            
            // Initialise Direct Interaction
            nonDominantDirect = Controller.Transform(ControllerTransforms.Check.LEFT).gameObject.AddComponent<DirectInteraction>();
            dominantDirect = Controller.Transform(ControllerTransforms.Check.RIGHT).gameObject.AddComponent<DirectInteraction>();
            nonDominantDirect.Initialise(radius);
            dominantDirect.Initialise(radius);
            
            // Add event listeners for indirect interaction
            Controller.SelectEvent(
                ControllerTransforms.Check.LEFT, 
                ControllerTransforms.EventTracker.EventType.END).AddListener(nonDominantIndirect.ButtonSelect);
            Controller.SelectEvent(
                ControllerTransforms.Check.RIGHT, 
                ControllerTransforms.EventTracker.EventType.END).AddListener(dominantIndirect.ButtonSelect);
            
            // Add event listeners for direct interaction
            Controller.GrabEvent(
                ControllerTransforms.Check.LEFT, 
                ControllerTransforms.EventTracker.EventType.START).AddListener(nonDominantDirect.ButtonGrabStart);
            Controller.GrabEvent(
                ControllerTransforms.Check.LEFT, 
                ControllerTransforms.EventTracker.EventType.STAY).AddListener(nonDominantDirect.ButtonGrabStay);
            Controller.GrabEvent(
                ControllerTransforms.Check.LEFT, 
                ControllerTransforms.EventTracker.EventType.END).AddListener(nonDominantDirect.ButtonGrabEnd);
            Controller.GrabEvent(
                ControllerTransforms.Check.RIGHT, 
                ControllerTransforms.EventTracker.EventType.START).AddListener(dominantDirect.ButtonGrabStart);
            Controller.GrabEvent(
                ControllerTransforms.Check.RIGHT, 
                ControllerTransforms.EventTracker.EventType.STAY).AddListener(dominantDirect.ButtonGrabStay);
            Controller.GrabEvent(
                ControllerTransforms.Check.RIGHT, 
                ControllerTransforms.EventTracker.EventType.END).AddListener(dominantDirect.ButtonGrabEnd);
        }

        private void Update()
        {
            dominantIndirect.Check(
                Controller.Position(ControllerTransforms.Check.RIGHT), 
                Controller.ForwardVector(ControllerTransforms.Check.RIGHT), 
                range,
                true);
            nonDominantIndirect.Check(
                Controller.Position(ControllerTransforms.Check.LEFT),
                Controller.ForwardVector(ControllerTransforms.Check.LEFT), 
                range,
                true);
        }
    }
}
