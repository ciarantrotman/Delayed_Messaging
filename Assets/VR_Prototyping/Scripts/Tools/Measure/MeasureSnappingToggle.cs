using System.Collections;
using UnityEngine;
using VR_Prototyping.Scripts.UI_Blocks;

namespace VR_Prototyping.Scripts.Tools.Measure
{
    public class MeasureSnappingToggle : ToolMenuButton
    {
        private MeasureTool measureTool;
        const float SpawnDelayDuration = 1.5f;
        private void Start()
        {
            measureTool = transform.parent.transform.GetComponentInParent<MeasureTool>();
            controller = measureTool.Controller;
            startsActive = measureTool.axisSnapping;
            InitialiseSelectableObject();
            StartCoroutine(SpawnDelay());
        }
        private IEnumerator SpawnDelay()
        {
            yield return new WaitForSeconds(SpawnDelayDuration);
            activate.AddListener(ToggleSnapOn);
            deactivate.AddListener(ToggleSnapOff);
            yield return null;
        }
        private void ToggleSnapOn()
        {
            measureTool.axisSnapping = true;
        }
        
        private void ToggleSnapOff()
        {
            measureTool.axisSnapping = false;
        }
    }
}
