using Sirenix.OdinInspector;
using TMPro;
using UnityEngine;

namespace VR_Prototyping.Scripts.Tools.Note
{
    public class NoteObject : MonoBehaviour
    {
        private NoteTool NoteTool { get; set; }
        private ControllerTransforms Controller { get; set; }
        private LineRenderer noteLineRenderer;
        
        [BoxGroup("Note Objects Settings")] [SerializeField] [Required] private TextMeshPro noteText;
        [BoxGroup("Note Objects Settings")] [SerializeField] [Required] private TextMeshPro noteTitle;
        [BoxGroup("Note Objects Settings")] [SerializeField] [Required] private TextMeshPro noteHeader;
        [BoxGroup("Note Objects Settings")] [SerializeField] [Required] private Transform anchorTop;
        [BoxGroup("Note Objects Settings")] [SerializeField] [Required] private Transform anchorBottom;
        [BoxGroup("Note Objects Settings")] [SerializeField] [Range(.001f, .005f)] private float lineRenderWidth = .002f;
        [BoxGroup("Note Objects Settings")] [SerializeField] private Material lineRenderMat;

        public void Initialise(NoteTool noteTool, ControllerTransforms controllerTransforms)
        {
            Controller = controllerTransforms;
            NoteTool = noteTool;
            noteLineRenderer = anchorTop.AddOrGetLineRenderer();
            noteLineRenderer.SetupLineRender(lineRenderMat, lineRenderWidth, true);
        }

        private void FixedUpdate()
        {
            anchorBottom.SplitPositionVector(0, anchorBottom);
            transform.LookAtVertical(Controller.CameraTransform());
            noteLineRenderer.SetPosition(0, anchorTop.position);
            noteLineRenderer.SetPosition(1, anchorBottom.position);
        }

        public void SetNote(string text, string title, string header, Vector3 position)
        {
            transform.position = position;
            noteText.SetText(text);
            noteTitle.SetText(title);
            noteHeader.SetText(header);
        }
    }
}
