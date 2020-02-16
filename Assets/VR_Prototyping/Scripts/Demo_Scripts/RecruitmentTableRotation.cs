using UnityEngine;
using VR_Prototyping.Scripts.UI_Blocks;

namespace VR_Prototyping.Scripts.Demo_Scripts
{
    public class RecruitmentTableRotation : DirectSlider
    {
        public Transform tableParent;
        private void LateUpdate()
        {
            var y = Mathf.Lerp(0, 360, sliderValue);
            tableParent.transform.eulerAngles =  new Vector3(0, y,0);
        }
    }
}
