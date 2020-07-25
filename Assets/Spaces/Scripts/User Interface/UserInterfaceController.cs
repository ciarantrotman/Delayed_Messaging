using System;
using Spaces.Scripts.Objects;
using Spaces.Scripts.Player;
using Spaces.Scripts.User_Interface.Object_Interface;
using Spaces.Scripts.Utilities;
using UnityEngine;

namespace Spaces.Scripts.User_Interface
{
    public class UserInterfaceController : MonoBehaviour
    {
        [SerializeField] private GameObject contextualObjectMetadata, temporaryUserInterface;
        private ContextualObjectMetadata objectMetadata;
        private static GameObject Player => Reference.Player();
        
        // ------------------------------------------------------------------------------------------------------------
        
        /// <summary>
        /// 
        /// </summary>
        /// <param name="objectInstance"></param>
        /// <param name="target"></param>
        public void SetObjectMetadata(ObjectInstance objectInstance, Transform target)
        {
            objectMetadata.DisplayMetadata(objectInstance, target);
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

        // ------------------------------------------------------------------------------------------------------------
        
        private void Awake()
        {
            InitialiseContextualObjectMetadata();
        }
        private void FixedUpdate()
        {
            temporaryUserInterface.transform.LerpTransform(Player.transform, .5f);
        }
    }
}
