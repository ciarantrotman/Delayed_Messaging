using TMPro;
using UnityEngine;
using VR_Prototyping.Scripts.Tools.Measure;

namespace VR_Prototyping.Scripts.Tools.Memo
{
    public class MemoText : MonoBehaviour
    {
        private MemoTool MemoTool { get; set; }
        private TextMeshPro HighLevelText  { get; set; }

        private void Start()
        {
            if (MemoTool != null) return;
            MemoTool = transform.parent.transform.GetComponentInParent<MemoTool>();
            MemoTool.MemoText = MemoTool.MemoText == null ? this : MemoTool.MemoText;
            HighLevelText = GetComponent<TextMeshPro>();
        }

        public void SetText(string tapeName)
        {
            HighLevelText.SetText(tapeName);
        }
    }
}
