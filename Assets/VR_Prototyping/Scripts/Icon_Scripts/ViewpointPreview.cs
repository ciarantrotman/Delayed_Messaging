using Sirenix.OdinInspector;
using TMPro;
using UnityEngine;

namespace VR_Prototyping.Scripts.Icon_Scripts
{
    public class ViewpointPreview : MonoBehaviour
    {
        [BoxGroup] [SerializeField] private TextMeshPro textMeshPro;
        [BoxGroup] [SerializeField] private MeshRenderer previewRenderer;
        [BoxGroup] [SerializeField] private MeshRenderer text;
        [BoxGroup] [SerializeField] private MeshRenderer frameTop;
        [BoxGroup] [SerializeField] private MeshRenderer frameBottom;

        public void ActivatePreview(RenderTexture renderTexture, int index)
        {
            text.enabled = true;
            frameTop.enabled = true;
            frameBottom.enabled = true;
            previewRenderer.enabled = true;
            textMeshPro.renderer.enabled = true;
            textMeshPro.SetText(index.ToString());
            previewRenderer.material.mainTexture = renderTexture;
        }

        public void DeactivatePreview()
        {
            text.enabled = false;
            frameTop.enabled = false;
            frameBottom.enabled = false;
            previewRenderer.enabled = false;
            textMeshPro.renderer.enabled = false;
        }
    }
}
