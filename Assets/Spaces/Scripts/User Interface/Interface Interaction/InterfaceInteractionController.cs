using Spaces.Scripts.Player;
using Spaces.Scripts.User_Interface.Interface_Elements;
using Spaces.Scripts.Utilities;
using UnityEngine;

namespace Spaces.Scripts.User_Interface.Interface_Interaction
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
        private IndirectInterface dominantIndirect, nondominantIndirect;
        private DirectInterface dominantDirect, nondominantDirect;

        /// <summary>
        /// Creates all required objects and adds global button event listeners
        /// </summary>
        private void Awake()
        {
            // Add indirect interface interaction to each controller parent
            nondominantIndirect = Controller.Transform(ControllerTransforms.Check.LEFT).gameObject.AddComponent<IndirectInterface>();
            dominantIndirect = Controller.Transform(ControllerTransforms.Check.RIGHT).gameObject.AddComponent<IndirectInterface>();
            
            // Create a cached parent
            GameObject interactionParent = Set.Object(gameObject, "[Interface Interaction Parent]", Vector3.zero);
            
            // Initialise the indirect interfaces
            nondominantIndirect.Initialise(interactionParent, "[Interface] [Indirect / Non-Dominant]", material, ControllerTransforms.Check.LEFT);
            dominantIndirect.Initialise(interactionParent, "[Interface] [Indirect / Dominant]", material, ControllerTransforms.Check.RIGHT);
            
            // Add indirect event listeners
            nondominantIndirect.AddEventListeners(Controller, ControllerTransforms.Check.LEFT, BaseInterface.Interface);
            dominantIndirect.AddEventListeners(Controller, ControllerTransforms.Check.RIGHT, BaseInterface.Interface);
            
            // Add direct interaction to each controller
            nondominantDirect = Controller.Transform(ControllerTransforms.Check.LEFT).gameObject.AddComponent<DirectInterface>();
            dominantDirect = Controller.Transform(ControllerTransforms.Check.RIGHT).gameObject.AddComponent<DirectInterface>();
            
            // Initialise the direct interfaces
            nondominantDirect.Initialise(radius, ControllerTransforms.Check.LEFT);
            dominantDirect.Initialise(radius, ControllerTransforms.Check.RIGHT);
            
            // Add direct event listeners
            nondominantDirect.AddEventListeners(Controller, ControllerTransforms.Check.LEFT, BaseInterface.Interface);
            dominantDirect.AddEventListeners(Controller, ControllerTransforms.Check.RIGHT, BaseInterface.Interface);
        }

        private void Update()
        {
            nondominantIndirect.Check(
                Controller.Position(ControllerTransforms.Check.LEFT),
                Controller.ForwardVector(ControllerTransforms.Check.LEFT), 
                range,
                true);
            dominantIndirect.Check(
                Controller.Position(ControllerTransforms.Check.RIGHT), 
                Controller.ForwardVector(ControllerTransforms.Check.RIGHT), 
                range,
                true);
        }
    }
}
