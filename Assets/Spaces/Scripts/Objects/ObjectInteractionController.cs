using Spaces.Scripts.Player;
using Spaces.Scripts.User_Interface;
using UnityEngine;

namespace Spaces.Scripts.Objects
{
    public class ObjectInteractionController : MonoBehaviour
    {
        public ObjectInstance objectInstance;
        
        [Header("Indirect Interaction Settings")]
        [SerializeField, Range(1f, 50f)] private float range;
        [SerializeField] private Material material;
        [Header("Direct Interaction Settings")]
        [Range(0f, .05f), SerializeField] private float radius = .025f;
        
        private ControllerTransforms Controller => GetComponent<ControllerTransforms>();

        /// <summary>
        /// Creates all required objects and adds global button event listeners
        /// </summary>
        private void Awake()
        {
            
        }

        private void Update()
        {

        }
        
        public ObjectInstance FocusObject()
        {
            return objectInstance == null ? null : objectInstance;
        }
    }
}
