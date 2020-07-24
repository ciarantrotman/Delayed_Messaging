﻿using Spaces.Scripts.Player;
using Spaces.Scripts.Space;
using Spaces.Scripts.User_Interface;
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
        
        private static ControllerTransforms Controller => Reference.Player().GetComponent<ControllerTransforms>();
        private static UserInterfaceController UserInterfaceController => Reference.Player().GetComponent<UserInterfaceController>();
        private IndirectObject dominantIndirect, nondominantIndirect;
        private ObjectInstance focusObject;
        
        // ------------------------------------------------------------------------------------------------------------
        
        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public ObjectInstance FocusObject()
        {
            return focusObject;
        }
        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public SpaceInstance FocusSpace()
        {
            return focusObject.GetComponent<SpaceInstance>();
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="objectInstance"></param>
        /// <param name="check"></param>
        public void SetFocusObject(ObjectInstance objectInstance, ControllerTransforms.Check check)
        {
            focusObject = objectInstance;
            UserInterfaceController.SetObjectMetadata(focusObject, Controller.Transform(check));
        }

        // ------------------------------------------------------------------------------------------------------------
        
        /// <summary>
        /// Creates all required objects and adds global button event listeners
        /// </summary>
        private void Awake()
        {
            // Add indirect interface interaction to each controller parent
            nondominantIndirect = Controller.Transform(ControllerTransforms.Check.LEFT).gameObject.AddComponent<IndirectObject>();
            dominantIndirect = Controller.Transform(ControllerTransforms.Check.RIGHT).gameObject.AddComponent<IndirectObject>();
            
            // Initialise the indirect interfaces
            nondominantIndirect.Initialise(gameObject, "[Object] [Indirect / Non-Dominant]", material, range, ControllerTransforms.Check.LEFT);
            dominantIndirect.Initialise(gameObject, "[Object] [Indirect / Dominant]", material, range, ControllerTransforms.Check.RIGHT);
            
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
    }
}
