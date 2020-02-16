using DG.Tweening;
using Sirenix.OdinInspector;
using TMPro;
using UnityEngine;

namespace VR_Prototyping.Scripts.Tools.Memo
{
    public class MemoNode : MonoBehaviour
    {
        private MemoTool MemoTool { get; set; }
        public AudioSource AudioSource { get; private set; }
        public ControllerTransforms Controller { get; private set; }
        private float memoLength;
        private int nodeIndex;
        private float blendShapeWeight;

        private LineRenderer memoLineRenderer;
        
        private const float BlendShapeActive = 0f;
        private const float BlendShapeInactive = 100f;

        [BoxGroup("Memo Objects Settings")] [SerializeField] private SkinnedMeshRenderer progressBar;
        [BoxGroup("Memo Objects Settings")] [Indent] [Range(.01f, 1)] [SerializeField] private float resetDuration = .5f;
        [BoxGroup("Memo Objects Settings")] [Space(10)] [SerializeField] [Required] private Transform anchorTop;
        [BoxGroup("Memo Objects Settings")] [SerializeField] [Required] private Transform anchorBottom;
        [BoxGroup("Memo Objects Settings")] [SerializeField] [Range(.001f, .005f)] private float lineRenderWidth = .002f;
        [BoxGroup("Memo Objects Settings")] [SerializeField] private Material lineRenderMat;
        
        [Button]
        public void PlayAudioDebug()
        {
            AudioSource.Play();
        }
        
        public void Initialise(int index, MemoTool memoTool)
        {
            name = "Memo_" + index;
            MemoTool = memoTool;
            Controller = MemoTool.Controller;
            SetIndex(index);
            SetupAudio();
            memoLineRenderer = anchorTop.AddOrGetLineRenderer();
            memoLineRenderer.SetupLineRender(lineRenderMat, lineRenderWidth, true);
        }

        private void SetupAudio()
        {
            AudioSource = gameObject.AddComponent<AudioSource>();
            AudioSource.spatialize = true;
            AudioSource.spatializePostEffects = true;
        }

        private void SetIndex(int index)
        {
            nodeIndex = index;
        }
        
        private void SetLength(float length)
        {
            memoLength = length;
        }

        private void FixedUpdate()
        {
            SetPosition();
            SetBlendShape();
            SetAnchors();
        }
        private void SetPosition()
        {
            Transform x = transform;
            x.LookAwayFrom(Controller.CameraTransform(), Vector3.up);
            x.eulerAngles = new Vector3(0, x.eulerAngles.y, 0);
        }
        private void SetAnchors()
        {
            anchorBottom.SplitPositionVector(0, anchorBottom);
            transform.LookAtVertical(Controller.CameraTransform());
            memoLineRenderer.SetPosition(0, anchorTop.position);
            memoLineRenderer.SetPosition(1, anchorBottom.position);
        }
        
        private void SetBlendShape()
        {
            progressBar.SetBlendShapeWeight(0, blendShapeWeight);
        }
        public void ReleaseMemo(AudioClip memo, float length)
        {
            transform.position = Controller.CameraTransform().position + (Controller.CameraForwardVector() * .5f);
            AudioSource.clip = memo;
            SetLength(length);
            memoLength = length;
            GetComponentInChildren<TextMeshPro>().SetText("<b>Memo " + nodeIndex + "</b>\n{0:2} seconds", length);
        }

        public void PlayMemo()
        {
            BlendShapePlay();
            MemoTool.PlayAudio(nodeIndex);
        }
        
        public void PauseMemo()
        {
            BlendShapeReset();
            MemoTool.PauseAudio();
        }
        private void BlendShapePlay()
        {
            blendShapeWeight = BlendShapeActive;
            DOTween.To(()=> blendShapeWeight, x=> blendShapeWeight = x, BlendShapeInactive, memoLength);
        }
        
        private void BlendShapeReset()
        {
            blendShapeWeight = BlendShapeActive;
        }
    }
}
