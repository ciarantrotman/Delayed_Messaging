using System.Collections;
using DG.Tweening;
using UnityEngine;
using UnityEngine.AI;

namespace VR_Prototyping.Scripts
{
    public class SceneWipe : MonoBehaviour
    {
        private ControllerTransforms controllerTransforms;
        private Locomotion locomotion;
        private MeshRenderer sceneWipeRenderer;
        private static readonly int Fade = Shader.PropertyToID("_Fade");
        
        private const float Value = .51f;
        private float value = -Value;

        public void Initialise(ControllerTransforms transforms, Locomotion l)
        {
            controllerTransforms = transforms;
            locomotion = l;
            sceneWipeRenderer = GetComponent<MeshRenderer>();
        }
        private void Update()
        {
            Transform thisTransform = transform;
            thisTransform.Position(controllerTransforms.CameraTransform());
            thisTransform.up = Vector3.up;
            sceneWipeRenderer.material.SetFloat(Fade, value);
        }

        public IEnumerator SceneWipeStart(float duration)
        {
            DOTween.To(()=> value, x=> value = x, Value, duration);
            yield return new WaitForSeconds(duration);
            locomotion.sceneWipeTrigger.Invoke();
            DOTween.To(()=> value, x=> value = x, -Value, duration);
            yield return null;
        }
    }
}
