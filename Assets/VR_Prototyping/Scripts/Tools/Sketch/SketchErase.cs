using System.Collections;
using UnityEngine;
using VR_Prototyping.Scripts.UI_Blocks;

namespace VR_Prototyping.Scripts.Tools.Sketch
{
    public class SketchErase : ToolMenuButton
    {
        private SketchTool sketchTool;
        const float SpawnDelayDuration = 1.5f;

        private void Start()
        {
            sketchTool = transform.parent.transform.GetComponentInParent<SketchTool>();
            controller = sketchTool.Controller;
            InitialiseSelectableObject();
            StartCoroutine(SpawnDelay());
        }
        
        private IEnumerator SpawnDelay()
        {
            yield return new WaitForSeconds(SpawnDelayDuration);
            activate.AddListener(EraseActivate);
            deactivate.AddListener(EraseDeactivate);
            yield return null;
        }

        private void EraseActivate()
        {
            sketchTool.EraseToggle(true);
        }
        
        private void EraseDeactivate()
        {
            sketchTool.EraseToggle(false);
        }
    }
}
