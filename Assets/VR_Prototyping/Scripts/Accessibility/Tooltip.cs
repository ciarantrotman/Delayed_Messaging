using System;
using Sirenix.OdinInspector;
using TMPro;
using UnityEngine;

namespace VR_Prototyping.Scripts.Accessibility
{
    public class Tooltip : MonoBehaviour
    {
        [BoxGroup("Tooltip Settings")] [SerializeField] [Required] private TextMeshPro tooltipText;
        [BoxGroup("Tooltip Settings")] [SerializeField] [Required] private MeshRenderer backing;
        
        const int RenderQueueOrder = 2500;
        
        void Start()
        {
            backing.material.renderQueue = RenderQueueOrder;
        }

        public void SetTooltipText(bool tooltipsActive, string text)
        {
            if(!tooltipsActive) return;
            
            tooltipText.SetText(text);
            backing.enabled = true;
        }

        public void ClearTooltipText()
        {
            tooltipText.SetText("");
            backing.enabled = false;
        }
    }
}
