using System;
using Spaces.Scripts.Utilities;
using UnityEngine;
using Outline = VR_Prototyping.Plugins.QuickOutline.Scripts.Outline;

namespace Spaces.Scripts.User_Interface.Interface_Elements
{
    [RequireComponent(typeof(Collider))]
    public abstract class BaseInterface : MonoBehaviour
    {
        public enum TriggerType { GRAB, SELECT, BOTH }
        public enum InteractionType { DIRECT, INDIRECT, BOTH }
        // is true by default, is set to false if there is no model supplied
        private bool corporeal;
        
        [Header("Base Interface Options")]
        public TriggerType triggerType = TriggerType.SELECT;
        public InteractionType interactionType = InteractionType.INDIRECT;

        [Header("References")]
        [SerializeField] protected GameObject model;
        
        [Header("Visual Options")] 
        public OutlineConfiguration outlineConfiguration;
        
        // Protected Variables
        protected Collider TriggerCollider => GetComponent<Collider>();
        public const string Interface = "Interface", Direct = "Direct";
        private Outline outline;
        [Serializable] public class OutlineConfiguration
        {
            [Range(1, 10)] public float width = 10f;
            public Outline.Mode mode = Outline.Mode.OutlineAll;
            public Color color = new Color(1,1,1,1);
        }

        /// <summary>
        /// Use this to check if a button should be triggered given the button invocation parameters
        /// </summary>
        /// <param name="trigger"></param>
        /// <param name="interaction"></param>
        /// <returns></returns>
        public bool CheckInvocation(TriggerType trigger, InteractionType interaction)
        {
            // If the input matches, or interface is set to both
            if (trigger != triggerType && triggerType != TriggerType.BOTH)
            {
                return false;
            }
            return interaction == interactionType || triggerType == TriggerType.BOTH;
        }
        
        
        private void Awake()
        {
            // Set the tag of the interface 
            gameObject.tag = Interface;
            
            // Create visual effect for hovering, but only if there is a model
            if (model != null)
            {
                corporeal = true;
                outline = model.Outline(outlineConfiguration);
            }
            
            // Call abstract initialisation method
            Initialise();
        }

        public void ConfigureInterface(TriggerType trigger, InteractionType interaction)
        {
            triggerType = trigger;
            interactionType = interaction;
        }

        /// <summary>
        /// I have built this in such a way that it shouldn't need to reference anything to work
        /// </summary>
        protected abstract void Initialise();
        
        /// <summary>
        /// Called when the interface is hovered over
        /// </summary>
        public void HoverStart()
        {
            ToggleOutline(true);
        }
        /// <summary>
        /// Called when the interface is no longer being hovered over
        /// </summary>
        public void HoverEnd()
        {
            ToggleOutline(false);
        }
        /// <summary>
        /// Checks if the interface has a model, if it doesn't this isn't called
        /// </summary>
        /// <param name="state"></param>
        private void ToggleOutline(bool state)
        {
            if (!corporeal) return;
            outline.enabled = state;
        }
    }
}
