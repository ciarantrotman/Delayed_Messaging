using UnityEngine;
using UnityEngine.SceneManagement;

namespace VR_Prototyping.Scripts.Icon_Scripts
{
    public class ViewpointTarget : SelectableObject
    {
        private new Camera camera;
        private RenderTexture renderTexture;
        public ViewpointManager ViewpointManager { get; set; }
        public int Index { get; set; }

        public void SetupViewpointTarget()
        {
            camera = GetComponentInChildren<Camera>();
            renderTexture = new RenderTexture(720, 720, 16, RenderTextureFormat.Default);
            camera.targetTexture = renderTexture;
            selectEnd.AddListener(EnterModel);
            hoverStart.AddListener(ViewpointPreviewStart);
            hoverEnd.AddListener(ViewpointPreviewEnd);
        }

        private void ViewpointPreviewStart()
        {
            Transform cam = camera.transform;
            cam.localPosition = Vector3.zero;
            cam.rotation = Quaternion.identity;
            ViewpointManager.RViewpointPreview.ActivatePreview(renderTexture, Index);
        }
        private void ViewpointPreviewEnd()
        {
            ViewpointManager.RViewpointPreview.DeactivatePreview();
        }

        private void EnterModel()
        {
            ViewpointManager.TriggerEnterModel(transform);
        }
    }
}
