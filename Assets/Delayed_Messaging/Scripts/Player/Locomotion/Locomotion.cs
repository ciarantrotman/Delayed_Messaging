using System;
using System.Collections;
using System.Collections.Generic;
using Delayed_Messaging.Scripts.Utilities;
using DG.Tweening;
using UnityEngine;
using UnityEngine.Events;
using VR_Prototyping.Scripts.Utilities;

namespace VR_Prototyping.Scripts
{
    [DisallowMultipleComponent]
    [RequireComponent(typeof(ControllerTransforms))]
    public class Locomotion : MonoBehaviour
    {
        private LocomotionPositionPreview positionPreview;
        private Cast cast;
        private GameObject cameraNormalised;
        
        private bool cTouchR;
        private bool cTouchL;
        private bool pTouchR;
        private bool pTouchL;

        private Vector3 rRotTarget;
        private bool active;

        private Vector3 customRotation;
        private Vector3 customPosition;

        public enum Method
        {
            DASH,
            BLINK
        }

        [SerializeField, Range(.1f, 1f)] private float minimumMoveDistance = .5f;
        [SerializeField, Range(1f, 100f)] private float maximumMoveDistance = 15f;
        [SerializeField, Range(1, 15), Space(10)] private int layerIndex = 10;

        [SerializeField, Space(5)] private Method locomotionMethod = Method.DASH;
        [SerializeField, Space(10)] private bool rotation = true;
        [SerializeField, Range(15f, 90f)] private float angle = 45f;
        [SerializeField, Range(0f, 1f)] private float rotateSpeed = .15f;
        [SerializeField, Range(0f, 1f)] private float moveSpeed = .75f;
        [SerializeField, Space(10)] private bool disableLeftHand;
        [SerializeField] private bool disableRightHand;

        [Header("Locomotion Aesthetics")]
        [SerializeField] private GameObject ghost;
        [SerializeField] private bool motionSicknessVignette;
        [SerializeField, Range(0f, 1f)] private float vignetteStrength = .35f;
        [SerializeField, Space(5)] private GameObject targetVisual;
        [SerializeField] private Material lineRenderMat;
        [SerializeField, Space(10)] private GameObject sceneChangeWipe;
        [SerializeField, Range(.25f, 5f)] private float sceneWipeDuration;

        [HideInInspector] public UnityEvent sceneWipeTrigger;
        
        private SceneWipe sceneWipe;
        private ControllerTransforms controllerTransforms;

        private void Start()
        {
            controllerTransforms = GetComponent<ControllerTransforms>();
            cast = gameObject.AddComponent<Cast>();
            cast.SetupCastObjects(targetVisual, transform, "Locomotion", lineRenderMat, ControllerTransforms.MaxAngle, ControllerTransforms.MinAngle, maximumMoveDistance, minimumMoveDistance, .01f, controllerTransforms);
            cameraNormalised = new GameObject("[Locomotion/Temporary]");
            SetupGameObjects();
            sceneWipeTrigger.AddListener(SceneWipeDebug);
        }

        private void SetupGameObjects()
        {
            ghost = Instantiate(ghost);
            ghost.name = "[Locomotion/Ghost]";
            positionPreview = ghost.GetComponent<LocomotionPositionPreview>();
            positionPreview.ControllerTransforms = controllerTransforms;
            positionPreview.GhostToggle(null, false);

            sceneChangeWipe = Instantiate(sceneChangeWipe, controllerTransforms.Player().transform);
            sceneWipe = sceneChangeWipe.AddComponent<SceneWipe>();
            sceneWipe.Initialise(controllerTransforms, this);
        }

        private void LateUpdate()
        {
            cTouchR = controllerTransforms.RightLocomotion();
            cTouchL = controllerTransforms.LeftLocomotion();

            this.JoystickGestureDetection(
                controllerTransforms.RightJoystick(), controllerTransforms.rJoystickValues[0], angle, rotateSpeed, 
                ControllerTransforms.Trigger, ControllerTransforms.Tolerance,
                cast.rCastObject.visual, cast.rCastObject.lineRenderer, cTouchR, pTouchR, disableRightHand, active);
            this.JoystickGestureDetection(
                controllerTransforms.LeftJoystick(), controllerTransforms.lJoystickValues[0], angle, rotateSpeed,
                ControllerTransforms.Trigger, ControllerTransforms.Tolerance,
                cast.lCastObject.visual, cast.lCastObject.lineRenderer, cTouchL, pTouchL, disableLeftHand, active);
            
            pTouchR = controllerTransforms.RightLocomotion();
            pTouchL = controllerTransforms.LeftLocomotion();
        }

        private static Vector3 RotationAngle(Transform target, float a)
        {
            Vector3 t = target.eulerAngles;
            return new Vector3(t.x, t.y + a, t.z);
        }

        public void RotateUser(float a, float time)
        {
            if(transform.parent == cameraNormalised.transform || !rotation) return;
            active = true;
            
            controllerTransforms.CameraTransform().SplitRotation(cameraNormalised.transform, false);
            controllerTransforms.CameraTransform().SplitPosition(transform, cameraNormalised.transform);
            
            transform.SetParent(cameraNormalised.transform);
            cameraNormalised.transform.DORotate(RotationAngle(cameraNormalised.transform, a), time);
            StartCoroutine(Uncouple(transform, time));
        }

        public void LocomotionStart(GameObject visual, LineRenderer lr)
        {
            visual.SetActive(true);
            positionPreview.GhostToggle(visual.transform, true);
            lr.enabled = true;
            active = true;
        }
        
        public void LocomotionEnd(GameObject visual, Vector3 posTarget, Vector3 rotTarget, LineRenderer lr)
        {
            if (transform.parent == cameraNormalised.transform) return;
            
            controllerTransforms.CameraTransform().SplitRotation(cameraNormalised.transform, false);
            controllerTransforms.CameraTransform().SplitPosition(transform, cameraNormalised.transform);
            
            transform.SetParent(cameraNormalised.transform);
            switch (locomotionMethod)
            {
                case Method.DASH:
                    cameraNormalised.transform.DOMove(posTarget, moveSpeed);
                    cameraNormalised.transform.DORotate(rotTarget, moveSpeed);
                    StartCoroutine(Uncouple(transform, moveSpeed));
                    break;
                case Method.BLINK:
                    cameraNormalised.transform.position = posTarget;
                    cameraNormalised.transform.eulerAngles = rotTarget;
                    transform.SetParent(null);
                    active = false;
                    break;
                default:
                    throw new ArgumentException();
            }
            
            visual.SetActive(false);
            positionPreview.GhostToggle(null, false);
            lr.enabled = false;
        }

        public void CustomLocomotion(Vector3 targetPosition, Vector3 targetRotation, Method method, bool wipe)
        {
            controllerTransforms.CameraTransform().SplitRotation(cameraNormalised.transform, false);
            controllerTransforms.CameraTransform().SplitPosition(transform, cameraNormalised.transform);
            transform.SetParent(cameraNormalised.transform);
            switch (method)
            {
                case Method.DASH when !wipe:
                    cameraNormalised.transform.DOMove(targetPosition, moveSpeed);
                    cameraNormalised.transform.DORotate(targetRotation, moveSpeed);
                    StartCoroutine(Uncouple(transform, moveSpeed));
                    break;
                case Method.BLINK when !wipe:
                    cameraNormalised.transform.position = targetPosition;
                    cameraNormalised.transform.eulerAngles = targetRotation;
                    transform.SetParent(null);
                    active = false;
                    break;
                case Method.BLINK:
                    customPosition = targetPosition;
                    customRotation = targetRotation;
                    SceneWipe();
                    sceneWipeTrigger.AddListener(CustomWipe);
                    break;
                default:
                    throw new ArgumentException();
            }
        }

        private void CustomWipe()
        {
            cameraNormalised.transform.position = customPosition;
            cameraNormalised.transform.eulerAngles = customRotation;
            transform.SetParent(null);
            active = false;
        }
        
        private IEnumerator Uncouple(Transform a, float time)
        {
            SetVignette(vignetteStrength);
            yield return new WaitForSeconds(time);
            a.SetParent(null);
            active = false;
            SetVignette(0);
            yield return null;
        }

        private void SetVignette(float intensity)
        {
            if (!motionSicknessVignette) return;
            //vignetteLayer.intensity.value = intensity;
        }
        
        public void SceneWipe()
        {
            StartCoroutine(sceneWipe.SceneWipeStart(sceneWipeDuration));
        }

        private static void SceneWipeDebug()
        {
            Debug.Log("Scene wipe was called.");
        }
    }
}