using Sirenix.OdinInspector;
using Sirenix.OdinInspector.Editor.Drawers;
using UnityEngine;

namespace VR_Prototyping.Scripts.UI_Blocks
{
    public abstract class BaseDirectBlock : MonoBehaviour
    {
        [BoxGroup("Script Setup")] [SerializeField] private bool instantiatedElement;
        [BoxGroup("Script Setup")] [HideIf("instantiatedElement")] [Required] public ControllerTransforms controller;
        protected Rigidbody rb;
    }
}
