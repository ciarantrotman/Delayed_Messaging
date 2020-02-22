using System;
using System.Collections;
using System.Collections.Generic;
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
        
        private GameObject parent;
        private GameObject cN;
            
        private GameObject rCf; // follow
        private GameObject rCp; // proxy
        private GameObject rCn; // normalised
        private GameObject rMp; // midpoint
        private GameObject rTs; // target
        private GameObject rHp; // hit
        private GameObject rVisual; // visual

        private GameObject rRt; // rotation
        private GameObject lCf; // follow
        private GameObject lCp; // proxy
        private GameObject lCn; // normalised
        private GameObject lMp; // midpoint
        private GameObject lTs; // target
        private GameObject lHp; // hit
        private GameObject lVisual; // visual
        private GameObject lRt; // rotation

        private Vector3 rLastValidPosition;
        private Vector3 lLastValidPosition;
        
        [HideInInspector] public LineRenderer lLr;
        [HideInInspector] public LineRenderer rLr;
        private Material lLineRendererMaterial;
        private Material rLineRendererMaterial;
        private static readonly int Distance = Shader.PropertyToID("_Distance");
        
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
        [SerializeField] private bool advancedLocomotion = true;
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
        [SerializeField, Range(3f, 50f)] private int lineRenderQuality = 40;
        [SerializeField, Space(10)] private GameObject sceneChangeWipe;
        [SerializeField, Range(.25f, 5f)] private float sceneWipeDuration;

        [HideInInspector] public UnityEvent sceneWipeTrigger;
        
        private SceneWipe sceneWipe;
        private ControllerTransforms controllerTransforms;

        private void Start()
        {
            controllerTransforms = GetComponent<ControllerTransforms>();
            SetupGameObjects();
            sceneWipeTrigger.AddListener(SceneWipeDebug);
        }

        private void SetupGameObjects()
        {
            parent = new GameObject("[Locomotion/Calculations]");
            Transform parentTransform = parent.transform;
            parentTransform.SetParent(transform);
            
            cN = new GameObject("[Locomotion/Temporary]");
            
            rCf = new GameObject("[Locomotion/Follow/Right]");
            rCp = new GameObject("[Locomotion/Proxy/Right]");
            rCn = new GameObject("[Locomotion/Normalised/Right]");
            rMp = new GameObject("[Locomotion/MidPoint/Right]");
            rTs = new GameObject("[Locomotion/Target/Right]");
            rHp = new GameObject("[Locomotion/HitPoint/Right]");
            rRt = new GameObject("[Locomotion/Rotation/Right]");
            
            lCf = new GameObject("[Locomotion/Follow/Left]");
            lCp = new GameObject("[Locomotion/Proxy/Left]");
            lCn = new GameObject("[Locomotion/Normalised/Left]");
            lMp = new GameObject("[Locomotion/MidPoint/Left]");
            lTs = new GameObject("[Locomotion/Target/Left]");
            lHp = new GameObject("[Locomotion/HitPoint/Left]");
            lRt = new GameObject("[Locomotion/Rotation/Left]");
            
            rVisual = Instantiate(targetVisual, rHp.transform);
            rVisual.name = "[Locomotion/Visual/Right]";
            rVisual.SetActive(false);
            
            lVisual = Instantiate(targetVisual, lHp.transform);
            lVisual.name = "[Locomotion/Visual/Left]";
            lVisual.SetActive(false);
            
            ghost = Instantiate(ghost);
            ghost.name = "Locomotion/Ghost";
            positionPreview = ghost.GetComponent<LocomotionPositionPreview>();
            positionPreview.ControllerTransforms = controllerTransforms;
            positionPreview.GhostToggle(null, false);

            sceneChangeWipe = Instantiate(sceneChangeWipe, controllerTransforms.Player().transform);
            sceneWipe = sceneChangeWipe.AddComponent<SceneWipe>();
            sceneWipe.Initialise(controllerTransforms, this);

            rCf.transform.SetParent(parentTransform);
            rCp.transform.SetParent(rCf.transform);
            rCn.transform.SetParent(rCf.transform);
            rMp.transform.SetParent(rCp.transform);
            rTs.transform.SetParent(rCn.transform);
            rHp.transform.SetParent(transform);
            rRt.transform.SetParent(rHp.transform);
            
            lCf.transform.SetParent(parentTransform);
            lCp.transform.SetParent(lCf.transform);
            lCn.transform.SetParent(lCf.transform);
            lMp.transform.SetParent(lCp.transform);
            lTs.transform.SetParent(lCn.transform);
            lHp.transform.SetParent(transform);
            lRt.transform.SetParent(lHp.transform);
            
            rLr = rCp.AddComponent<LineRenderer>();
            rLr.SetupLineRender(lineRenderMat, .005f, false);
            
            lLr = lCp.AddComponent<LineRenderer>();
            lLr.SetupLineRender(lineRenderMat, .005f, false);
            
            rLineRendererMaterial = rLr.material;
            lLineRendererMaterial = lLr.material;
        }

        private void Update()
        {       
            rLastValidPosition = rTs.LastValidPosition(rLastValidPosition);
            lLastValidPosition = lTs.LastValidPosition(lLastValidPosition);
            
            Set.DistanceCast(rTs, rCf, rCp, rCn, rHp, rMp, rRt, rVisual, 
                controllerTransforms.RightTransform(), controllerTransforms.CameraTransform(), controllerTransforms.RightJoystick(), ControllerTransforms.MaxAngle, ControllerTransforms.MinAngle,
                minimumMoveDistance, maximumMoveDistance, rLastValidPosition);
            Set.DistanceCast(lTs, lCf, lCp, lCn, lHp, lMp, lRt, lVisual, 
                controllerTransforms.LeftTransform(), controllerTransforms.CameraTransform(), controllerTransforms.LeftJoystick(), ControllerTransforms.MaxAngle, ControllerTransforms.MinAngle,
                minimumMoveDistance, maximumMoveDistance, lLastValidPosition);
            
            // draw the line renderer
            rLr.BezierLineRenderer(controllerTransforms.RightTransform().position,rMp.transform.position,rHp.transform.position,lineRenderQuality);
            lLr.BezierLineRenderer(controllerTransforms.LeftTransform().position, lMp.transform.position, lHp.transform.position, lineRenderQuality);
            lLineRendererMaterial.SetFloat(Distance, transform.TransformDistance(lVisual.transform) + lVisual.transform.TransformDistance(lMp.transform));
            rLineRendererMaterial.SetFloat(Distance, transform.TransformDistance(rVisual.transform) + rVisual.transform.TransformDistance(rMp.transform));
        }

        private void LateUpdate()
        {
            cTouchR = controllerTransforms.RightLocomotion();
            cTouchL = controllerTransforms.LeftLocomotion();

            this.JoystickGestureDetection(
                controllerTransforms.RightJoystick(), controllerTransforms.rJoystickValues[0], angle, rotateSpeed, 
                ControllerTransforms.Trigger, ControllerTransforms.Tolerance,
                rVisual, rLr, cTouchR, pTouchR, disableRightHand, active);
            this.JoystickGestureDetection(
                controllerTransforms.LeftJoystick(), controllerTransforms.lJoystickValues[0], angle, rotateSpeed,
                ControllerTransforms.Trigger, ControllerTransforms.Tolerance,
                lVisual, lLr, cTouchL, pTouchL, disableLeftHand, active);
            
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
            if(transform.parent == cN.transform || !rotation) return;
            active = true;
            
            controllerTransforms.CameraTransform().SplitRotation(cN.transform, false);
            controllerTransforms.CameraTransform().SplitPosition(transform, cN.transform);
            
            transform.SetParent(cN.transform);
            cN.transform.DORotate(RotationAngle(cN.transform, a), time);
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
            if (transform.parent == cN.transform) return;
            
            controllerTransforms.CameraTransform().SplitRotation(cN.transform, false);
            controllerTransforms.CameraTransform().SplitPosition(transform, cN.transform);
            
            transform.SetParent(cN.transform);
            switch (locomotionMethod)
            {
                case Method.DASH:
                    cN.transform.DOMove(posTarget, moveSpeed);
                    cN.transform.DORotate(rotTarget, moveSpeed);
                    StartCoroutine(Uncouple(transform, moveSpeed));
                    break;
                case Method.BLINK:
                    cN.transform.position = posTarget;
                    cN.transform.eulerAngles = rotTarget;
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
            controllerTransforms.CameraTransform().SplitRotation(cN.transform, false);
            controllerTransforms.CameraTransform().SplitPosition(transform, cN.transform);
            transform.SetParent(cN.transform);
            switch (method)
            {
                case Method.DASH when !wipe:
                    cN.transform.DOMove(targetPosition, moveSpeed);
                    cN.transform.DORotate(targetRotation, moveSpeed);
                    StartCoroutine(Uncouple(transform, moveSpeed));
                    break;
                case Method.BLINK when !wipe:
                    cN.transform.position = targetPosition;
                    cN.transform.eulerAngles = targetRotation;
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
            cN.transform.position = customPosition;
            cN.transform.eulerAngles = customRotation;
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