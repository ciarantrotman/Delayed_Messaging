using System.Collections;
using UnityEngine;
using VR_Prototyping.Scripts.UI_Blocks;

namespace VR_Prototyping.Scripts.Tools.Measure
{
    public class MeasureDeleteNode : ToolMenuButton
    {
        private MeasureTool measureTool;
        private MeshRenderer meshRenderer;       
        const float SpawnDelayDuration = 1.5f;
        private void Start()
        {
            measureTool = transform.parent.transform.GetComponentInParent<MeasureTool>();
            controller = measureTool.Controller;
            StartCoroutine(SpawnDelay());
            InitialiseSelectableObject();
        }
        
        private IEnumerator SpawnDelay()
        {
            yield return new WaitForSeconds(SpawnDelayDuration);
            activate.AddListener(measureTool.DeleteNode);
            yield return null;
        }
    }
}