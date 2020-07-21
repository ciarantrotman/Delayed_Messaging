using Spaces.Scripts.Player;
using UnityEngine;

namespace Spaces.Scripts.Objects.Object_Interaction
{
    public class ObjectInteractionController : MonoBehaviour
    {
        [Header("Indirect Interaction Settings")]
        [SerializeField, Range(1f, 50f)] private float range;
        [SerializeField, Range(.1f, 10f)] private float indirectRadius;
        [SerializeField] private Material material;
        [Header("Direct Interaction Settings")]
        [Range(0f, .05f), SerializeField] private float directRadius = .025f;
        
        private ControllerTransforms Controller => GetComponent<ControllerTransforms>();
        private IndirectObject dominantIndirect, nondominantIndirect;
        private ObjectInstance focusObject;

        /// <summary>
        /// Creates all required objects and adds global button event listeners
        /// </summary>
        private void Awake()
        {
            // Add indirect interface interaction to each controller parent
            nondominantIndirect = Controller.Transform(ControllerTransforms.Check.LEFT).gameObject.AddComponent<IndirectObject>();
            dominantIndirect = Controller.Transform(ControllerTransforms.Check.RIGHT).gameObject.AddComponent<IndirectObject>();
            
            // Initialise the indirect interfaces
            nondominantIndirect.Initialise(gameObject, "[Object] [Indirect / Non-Dominant]", material, range);
            dominantIndirect.Initialise(gameObject, "[Object] [Indirect / Dominant]", material, range);
            
            // Add indirect event listeners
            nondominantIndirect.AddEventListeners(Controller, ControllerTransforms.Check.LEFT);
            dominantIndirect.AddEventListeners(Controller, ControllerTransforms.Check.RIGHT);
        }

        private void Update()
        {
            dominantIndirect.Check(
                Controller.Position(ControllerTransforms.Check.RIGHT), 
                Controller.ForwardVector(ControllerTransforms.Check.RIGHT), 
                indirectRadius,
                range,
                true);
            nondominantIndirect.Check(
                Controller.Position(ControllerTransforms.Check.LEFT),
                Controller.ForwardVector(ControllerTransforms.Check.LEFT), 
                indirectRadius,
                range,
                true);
        }

        public ObjectInstance FocusObject()
        {
            return focusObject;
        }

        public void SetFocusObject(ObjectInstance objectInstance)
        {
            Debug.Log($"{objectInstance.name} set as focus object");
            focusObject = objectInstance;
        }
    }
}
