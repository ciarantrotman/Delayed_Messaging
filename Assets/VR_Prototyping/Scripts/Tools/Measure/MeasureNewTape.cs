using UnityEngine;
using VR_Prototyping.Scripts.UI_Blocks;

namespace VR_Prototyping.Scripts.Tools.Measure
{
    public class MeasureNewTape : ToolMenuButton
    {
        private MeasureTool measureTool;
        private void Start()
        {
            measureTool = transform.parent.transform.GetComponentInParent<MeasureTool>();
            controller = measureTool.Controller;
            activate.AddListener(measureTool.NewTape);
            InitialiseSelectableObject();
        }
    }
}
