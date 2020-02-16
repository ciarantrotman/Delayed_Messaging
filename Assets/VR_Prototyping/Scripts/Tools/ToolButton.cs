using DG.Tweening;
using Leap.Unity.Infix;
using Sirenix.OdinInspector;
using Sirenix.Utilities;
using UnityEngine;

namespace VR_Prototyping.Scripts.Tools
{
    public class ToolButton : SelectableObject
    {
        [BoxGroup("Tool Settings")] [SerializeField] private GameObject mask;
        [BoxGroup("Tool Settings")] [SerializeField] private GameObject back;
        [BoxGroup("Tool Settings")] [SerializeField] private Transform hoverBorder;
        [BoxGroup("Tool Settings")] [Space(10)] [SerializeField] [Range(0f, .01f)] private float hoverRestDepth;
        [BoxGroup("Tool Settings")] [SerializeField] [Range(.01f, .05f)] private float hoverActiveDepth;
        [BoxGroup("Tool Settings")] [SerializeField] [Range(.1f, 1f)] private float duration;
    
        private float borderDepth;
        public GameObject ToolModel { private get; set; }
        private static readonly int StencilReference = Shader.PropertyToID("_StencilReferenceID");
        public int Index { private get; set; }
        private const float ModelOffset = .1f;
        protected override void Initialise()
        {
            ToolModel = Instantiate(ToolModel, transform, true);
            ToolModel.transform.localPosition = new Vector3(0,0,ModelOffset);
            ToolModel.transform.localRotation = Quaternion.identity;
            SetupStencilIndex();
            hoverStart.AddListener(HoverBorderStart);
            hoverEnd.AddListener(HoverBorderEnd);
        }
        
        protected override void ObjectUpdate()
        {
			hoverBorder.localPosition = new Vector3(0, 0, borderDepth);
        }

        private void HoverBorderStart()
        {
            borderDepth = hoverBorder.localPosition.z;
            DOTween.To(()=> borderDepth, x=> borderDepth = x, -hoverActiveDepth, duration);
        }

        private void HoverBorderEnd()
        {
            borderDepth = hoverBorder.localPosition.z;
            DOTween.To(()=> borderDepth, x=> borderDepth = x, -hoverRestDepth, duration);
        }
        
        private void SetupStencilIndex()
        {
            CheckRender(ToolModel.transform);
            foreach (Transform child in ToolModel.transform)
            {
                CheckRender(child);
            }
            mask.GetComponent<MeshRenderer>().material.SetFloat(StencilReference, Index);
            back.GetComponent<MeshRenderer>().material.SetFloat(StencilReference, Index);
        }

        private void CheckRender(Component a)
        {
            if (a.GetComponent<MeshRenderer>() != null)
            {
                a.GetComponent<MeshRenderer>().material.SetFloat(StencilReference, Index);
            }
        }
    }
}
