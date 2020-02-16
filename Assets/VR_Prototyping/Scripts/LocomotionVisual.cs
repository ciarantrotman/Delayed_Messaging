using System;
using Sirenix.OdinInspector;
using UnityEngine;

namespace VR_Prototyping.Scripts
{
    public class LocomotionVisual : MonoBehaviour
    {
        [BoxGroup] [SerializeField] [Required] private Transform targetCross;

        private void Update()
        {
            targetCross.transform.forward = Vector3.forward;
        }
    }
}
