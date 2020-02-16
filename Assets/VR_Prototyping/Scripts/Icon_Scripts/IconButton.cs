using DG.Tweening;
using Sirenix.OdinInspector;
using UnityEngine;

namespace VR_Prototyping.Scripts.Icon_Scripts
{
    public class IconButton : SelectableObject
    {
        [BoxGroup("Tool Settings")] [SerializeField] private GameObject mask;
        [BoxGroup("Tool Settings")] [SerializeField] private GameObject back;
        [BoxGroup("Tool Settings")] [SerializeField] private Transform hoverBorder;
        [BoxGroup("Tool Settings")] [Space(10)] [SerializeField] [Range(0f, .5f)] private float activeRestDepth;
        [BoxGroup("Tool Settings")] [SerializeField] [Range(.01f, .5f)] private float activeHoverDepth;

        private float originalRestDepth;
        private float originalHoverDepth;
        public GameObject ControlModel { private get; set; }
        private static readonly int StencilReference = Shader.PropertyToID("_StencilReferenceID");
        public int Index { private get; set; }
        private const float ModelOffset = -.1f;
        protected override void Initialise()
        {
            ControlModel = Instantiate(ControlModel, transform, true);
            ControlModel.transform.localPosition = new Vector3(0,0,ModelOffset);
            ControlModel.transform.localRotation = Quaternion.identity;
            SetupStencilIndex();
            originalRestDepth = restDepth;
            originalHoverDepth = hoverDepth;
            toggleStart.AddListener(ToggleStart);
            toggleEnd.AddListener(ToggleEnd);
        }

        public void ToggleStart()
        {
            DOTween.To(()=> restDepth, x=> restDepth = x, activeRestDepth, buttonAnimationDuration);
            DOTween.To(()=> hoverDepth, x=> hoverDepth = x, activeHoverDepth, buttonAnimationDuration);
        }

        public void ToggleEnd()
        {
            DOTween.To(()=> restDepth, x=> restDepth = x, originalRestDepth, buttonAnimationDuration);
            DOTween.To(()=> hoverDepth, x=> hoverDepth = x, originalHoverDepth, buttonAnimationDuration);
        }
        
        private void SetupStencilIndex()
        {
            CheckRender(ControlModel.transform);
            foreach (Transform child in ControlModel.transform)
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
