﻿using System.Collections;
using DG.Tweening;
using Spaces.Scripts.Player;
using Spaces.Scripts.Utilities;
using UnityEngine;
using VR_Prototyping.Scripts;

namespace Delayed_Messaging.Scripts.Player.Locomotion
{
    public class SceneWipe : MonoBehaviour
    {
        private ControllerTransforms controller;
        private Spaces.Scripts.Player.Locomotion.Locomotion locomotion;
        private MeshRenderer sceneWipeRenderer;
        private static readonly int Fade = Shader.PropertyToID("_Fade");
        
        private const float Value = .51f;
        private float value = -Value;

        public void Initialise(ControllerTransforms transforms, Spaces.Scripts.Player.Locomotion.Locomotion l)
        {
            controller = transforms;
            locomotion = l;
            sceneWipeRenderer = GetComponent<MeshRenderer>();
        }
        private void Update()
        {
            Transform thisTransform = transform;
            thisTransform.Position(controller.Transform(ControllerTransforms.Check.HEAD));
            thisTransform.up = Vector3.up;
            sceneWipeRenderer.material.SetFloat(Fade, value);
        }

        public IEnumerator SceneWipeStart(float duration)
        {
            DOTween.To(()=> value, x=> value = x, Value, duration);
            yield return new WaitForSeconds(duration);
            //locomotion.sceneWipeTrigger.Invoke();
            DOTween.To(()=> value, x=> value = x, -Value, duration);
            yield return null;
        }
    }
}
