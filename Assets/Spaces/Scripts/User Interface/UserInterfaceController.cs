using Spaces.Scripts.Objects;
using Spaces.Scripts.User_Interface.Object_Interface;
using Spaces.Scripts.Utilities;
using UnityEngine;

namespace Spaces.Scripts.User_Interface
{
    public class UserInterfaceController : MonoBehaviour
    {
        [SerializeField] private GameObject contextualObjectMetadata;
        private ContextualObjectMetadata objectMetadata;
        
        /// <summary>
        /// 
        /// </summary>
        /// <param name="objectInstance"></param>
        /// <param name="target"></param>
        public void SetObjectMetadata(ObjectInstance objectInstance, Transform target)
        {
            objectMetadata.DisplayMetadata(objectInstance, target);
        }

        // ------------------------------------------------------------------------------------------------------------
        
        private void Awake()
        {
            InitialiseContextualObjectMetadata();
        }
        /// <summary>
        /// 
        /// </summary>
        private void InitialiseContextualObjectMetadata()
        {
            GameObject metadata = Set.Object(gameObject, "[Contextual Object Metadata]", Vector3.zero);
            objectMetadata = metadata.AddComponent<ContextualObjectMetadata>();
            objectMetadata.Initialise(contextualObjectMetadata);
        }
    }
}
