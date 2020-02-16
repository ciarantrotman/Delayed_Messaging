using System.Collections;
using Sirenix.OdinInspector;
using UnityEngine;
using VR_Prototyping.Scripts.UI_Blocks;

namespace VR_Prototyping.Scripts.Tools.Memo
{
    public class MemoPlayButton : ToolMenuButton
    {
        [BoxGroup("Button Settings")] [SerializeField] private GameObject pauseText;
        private MemoNode memoNode;
        private const float SpawnDelayDuration = 1.5f;
        private void Start()
        {
            memoNode = transform.parent.transform.GetComponentInParent<MemoNode>();
            controller = memoNode.Controller;
            pauseText.GetComponent<MeshRenderer>().material.color = activeColor;
            InitialiseSelectableObject();
            pauseText.transform.SetParent(Button.transform);
            StartCoroutine(SpawnDelay());
        }
        
        private IEnumerator SpawnDelay()
        {
            yield return new WaitForSeconds(SpawnDelayDuration);
            activate.AddListener(memoNode.PlayMemo);
            activate.AddListener(SetActiveState);
            deactivate.AddListener(memoNode.PauseMemo);
            deactivate.AddListener(SetInactiveState);
            yield return null;
        }

        private void SetActiveState()
        {
            pauseText.SetActive(true);
            buttonText.SetActive(false);
        }
        private void SetInactiveState()
        {
            pauseText.SetActive(false);
            buttonText.SetActive(true);
        }
    }
}
