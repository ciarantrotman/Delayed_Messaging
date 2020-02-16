using System.Collections;
using Sirenix.OdinInspector;
using UnityEngine;
using VR_Prototyping.Scripts.UI_Blocks;

namespace VR_Prototyping.Scripts.Tools
{
    public class CloseButton : ToolMenuButton
    {
        private BaseTool tool;
        const float SpawnDelayDuration = 1.5f;

        private void Start()
        {
            tool = transform.parent.transform.GetComponentInParent<BaseTool>();
            controller = tool.Controller;
            InitialiseSelectableObject();
            StartCoroutine(SpawnDelay());
        }

        private IEnumerator SpawnDelay()
        {
            yield return new WaitForSeconds(SpawnDelayDuration);
            activate.AddListener(DeactivateTool);
            yield return null;
        }
        
        private void DeactivateTool()
        {
            tool.ToolController.SetAllToolState(false);
        }
    }
}
