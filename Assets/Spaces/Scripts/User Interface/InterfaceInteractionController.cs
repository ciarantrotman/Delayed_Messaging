using Delayed_Messaging.Scripts.Player;
using Spaces.Scripts.Player;
using UnityEngine;

namespace Spaces.Scripts.User_Interface
{
    [RequireComponent(typeof(ControllerTransforms))]
    public class InterfaceInteractionController : MonoBehaviour
    {
        [Header("Indirect Interaction Settings")]
        [SerializeField, Range(1f, 50f)] private float range;
        [SerializeField] private Material material;
        [Header("Direct Interaction Settings")]
        [Range(0f, .05f), SerializeField] private float radius = .025f;
        
        private ControllerTransforms Controller => GetComponent<ControllerTransforms>();
        private IndirectInterface dominantIndirectInterface, nonDominantIndirectInterface;
        private DirectInterface dominantDirectInterface, nonDominantDirectInterface;

        /// <summary>
        /// Creates all required objects and adds global button event listeners
        /// </summary>
        private void Awake()
        {
            // Add indirect interface interaction to each controller parent
            nonDominantIndirectInterface = Controller.Transform(ControllerTransforms.Check.LEFT).gameObject.AddComponent<IndirectInterface>();
            dominantIndirectInterface = Controller.Transform(ControllerTransforms.Check.RIGHT).gameObject.AddComponent<IndirectInterface>();
            
            // Initialise the indirect interfaces
            nonDominantIndirectInterface.Initialise(gameObject, "[Indirect / Non-Dominant]", material);
            dominantIndirectInterface.Initialise(gameObject, "[Indirect / Dominant]", material);
            
            // Add indirect event listeners
            nonDominantIndirectInterface.AddEventListeners(Controller, ControllerTransforms.Check.LEFT);
            dominantIndirectInterface.AddEventListeners(Controller, ControllerTransforms.Check.RIGHT);
            
            // Add direct interaction to each controller
            nonDominantDirectInterface = Controller.Transform(ControllerTransforms.Check.LEFT).gameObject.AddComponent<DirectInterface>();
            dominantDirectInterface = Controller.Transform(ControllerTransforms.Check.RIGHT).gameObject.AddComponent<DirectInterface>();
            
            // Initialise the direct interfaces
            nonDominantDirectInterface.Initialise(radius);
            dominantDirectInterface.Initialise(radius);
            
            // Add direct event listeners
            nonDominantDirectInterface.AddEventListeners(Controller, ControllerTransforms.Check.LEFT);
            dominantDirectInterface.AddEventListeners(Controller, ControllerTransforms.Check.RIGHT);
        }

        private void Update()
        {
            dominantIndirectInterface.Check(
                Controller.Position(ControllerTransforms.Check.RIGHT), 
                Controller.ForwardVector(ControllerTransforms.Check.RIGHT), 
                range,
                true);
            nonDominantIndirectInterface.Check(
                Controller.Position(ControllerTransforms.Check.LEFT),
                Controller.ForwardVector(ControllerTransforms.Check.LEFT), 
                range,
                true);
        }
    }
}
