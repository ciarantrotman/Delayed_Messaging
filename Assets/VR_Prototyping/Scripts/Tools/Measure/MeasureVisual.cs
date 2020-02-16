using System;
using UnityEngine;

namespace VR_Prototyping.Scripts.Tools.Measure
{
    public class MeasureVisual : MonoBehaviour
    {
        private MeasureTool measureTool;
        private MeshRenderer meshRenderer;
        private void Start()
        {
            measureTool = transform.parent.transform.GetComponentInParent<MeasureTool>();
            meshRenderer = transform.GetComponentInChildren<MeshRenderer>();
            meshRenderer.material = measureTool.tapeMaterial;
            measureTool.MeasureVisual = this;
        }

        private void Update()
        {
            transform.rotation = Quaternion.identity;
        }

        public void SetColor(Color color)
        {
            meshRenderer.material.color = color;
        }
    }
}
