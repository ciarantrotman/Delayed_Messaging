using System;
using Spaces.Scripts.Objects;
using Spaces.Scripts.Utilities;
using TMPro;
using UnityEngine;

namespace Spaces.Scripts.User_Interface.Object_Interface
{
    public class ContextualObjectMetadata : MonoBehaviour
    {
        private TextMeshPro display;
        private Transform target;
        /// <summary>
        /// 
        /// </summary>
        /// <param name="userInterface"></param>
        public void Initialise(GameObject userInterface)
        {
            GameObject placeholder = Instantiate(userInterface, transform);
            display = placeholder.GetComponent<TextMeshPro>();
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="objectInstance"></param>
        /// <param name="follow"></param>
        public void DisplayMetadata(ObjectInstance objectInstance, Transform follow)
        {
            // Set it to follow the hand you selected with for the time being
            target = follow;
            
            // Display the relevant information
            display.SetText($"Name:  {objectInstance.name}\n" +
                            $"State: {objectInstance.totemState}\n" +
                            $"Pos:   {objectInstance.relativeTransform.CurrentLocation.position}\n" +
                            $"Rot:   {objectInstance.relativeTransform.CurrentLocation.rotation}");
        }
        
        // ------------------------------------------------------------------------------------------------------------

        private void FixedUpdate()
        {
            transform.LerpTransform(target, .5f);
        }
    }
}
