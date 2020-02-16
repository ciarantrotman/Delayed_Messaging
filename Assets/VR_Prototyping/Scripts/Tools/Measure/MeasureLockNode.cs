using System.Collections;
using UnityEngine;
using VR_Prototyping.Scripts.UI_Blocks;

namespace VR_Prototyping.Scripts.Tools.Measure
{
    public class MeasureLockNode : ToolMenuButton
    {
        private MeasureTool measureTool;
        private MeshRenderer meshRenderer;       
        const float SpawnDelayDuration = 1.5f;
        private void Start()
        {
            measureTool = transform.parent.transform.GetComponentInParent<MeasureTool>();
            controller = measureTool.Controller;
            measureTool.MeasureLockNode = this;
            StartCoroutine(SpawnDelay());
            InitialiseSelectableObject();
        }

        private IEnumerator SpawnDelay()
        {
            yield return new WaitForSeconds(SpawnDelayDuration);
            activate.AddListener(measureTool.LockNode);
            deactivate.AddListener(measureTool.UnlockNode);
            yield return null;
        }
    }
}