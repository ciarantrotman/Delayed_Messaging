using UnityEngine;

namespace VR_Prototyping.Scripts.UI_Blocks
{
    public abstract class BaseDirectBlock : MonoBehaviour
    {
        [SerializeField] private bool instantiatedElement;
        public ControllerTransforms controller;
        protected Rigidbody rb;
    }
}
