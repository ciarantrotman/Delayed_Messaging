using System;
using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using Sirenix.OdinInspector;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.Rendering.PostProcessing;
using VR_Prototyping.Scripts.Icon_Scripts;

namespace VR_Prototyping.Scripts
{
    [DisallowMultipleComponent]
    [RequireComponent(typeof(ControllerTransforms))]
    public class Locomotion : MonoBehaviour
    {
        private GameObject parent;
        private GameObject cN;  // camera normalised
        
        private GameObject rCf; // follow
        private GameObject rCp; // proxy
        private GameObject rCn; // normalised
        private GameObject rMp; // midpoint
        private GameObject rTs; // target
        private GameObject rHp; // hit
        private GameObject rVo; // visual
        private GameObject rRt; // rotation
        
        private GameObject lCf; // follow
        private GameObject lCp; // proxy
        private GameObject lCn; // normalised
        private GameObject lMp; // midpoint
        private GameObject lTs; // target
        private GameObject lHp; // hit
        private GameObject lVo; // visual
        private GameObject lRt; // rotation
        
        private Vector3 rLastValidPosition;
        private Vector3 lLastValidPosition;

        private Vignette vignetteLayer;

        private LocomotionPositionPreview positionPreview;
        
        [HideInInspector] public LineRenderer lLr;
        [HideInInspector] public LineRenderer rLr;
        
        private const float MaxAngle = 110f;
        private const float MinAngle = 80f;
        
        private const float Trigger = .7f;
        private const float Sensitivity = 10f;
        private const float Tolerance = .1f;
        
        private readonly List<Vector2> rJoystickValues = new List<Vector2>();
        private readonly List<Vector2> lJoystickValues = new List<Vector2>();
        
        private bool cTouchR;
        private bool cTouchL;
        private bool pTouchR;
        private bool pTouchL;

        private Vector3 rRotTarget;
        private bool active;

        private Vector3 customRotation;
        private Vector3 customPosition;
        
        public ViewpointManager ViewpointManager { get; set; }
        
        public enum Method
        {
            DASH,
            BLINK
        }
        private static bool TypeCheck(Method type)
        {
            return type != Method.DASH;
        }
        
        [BoxGroup("Distance Settings")] [Range(.1f, 1f)] [SerializeField] private float min = .5f;
        [BoxGroup("Distance Settings")] [Range(1f, 100f)] [SerializeField] private float max = 15f;
        
        [BoxGroup("References")] [Range(1, 15)] [SerializeField] private int layerIndex = 10;
        
        [ValidateInput("TypeCheck", "Dash is the recommended locomotion type, but should be disabled for motion sickness prone users.", InfoMessageType.Info)]
        [TabGroup("Locomotion Settings")] [Space(5)] [SerializeField] private Method locomotionMethod = Method.DASH;
        [TabGroup("Locomotion Settings")] [Tooltip("This controls the ability to control the direction you face when moving, it is recommended, but should be disabled for the Vive.")] [Space(5)] [SerializeField] private bool advancedLocomotion = true;
        [TabGroup("Locomotion Settings")] [Space(10)][SerializeField] private bool rotation = true;
        [TabGroup("Locomotion Settings")] [ShowIf("rotation")] [Indent] [Range(15f, 90f)] [SerializeField] private float angle = 45f;
        [TabGroup("Locomotion Settings")] [ShowIf("rotation")] [Indent] [Range(0f, 1f)] [SerializeField] private float rotateSpeed = .15f;
        [TabGroup("Locomotion Settings")] [Space(10)][SerializeField] private bool disableLeftHand;
        [TabGroup("Locomotion Settings")] [SerializeField] private bool disableRightHand;

        [TabGroup("Aesthetic Settings")] [Required] [SerializeField] private GameObject ghost;
        [TabGroup("Aesthetic Settings")] [Required] [SerializeField] private PostProcessVolume volume;
        [TabGroup("Aesthetic Settings")] [SerializeField] private bool motionSicknessVignette;
        [TabGroup("Aesthetic Settings")] [ShowIf("motionSicknessVignette")] [Indent] [Range(0f, 1f)] [SerializeField] private float vignetteStrength = .35f;
        [TabGroup("Aesthetic Settings")] [Range(0f, 1f)] [SerializeField] private float moveSpeed = .75f;
        [TabGroup("Aesthetic Settings")] [Space(5)] [SerializeField] [Required] private GameObject targetVisual;
        [TabGroup("Aesthetic Settings")] [SerializeField] [Required] private AnimationCurve locomotionEasing;
        [TabGroup("Aesthetic Settings")] [SerializeField] [Required] private Material lineRenderMat;
        [TabGroup("Aesthetic Settings")] [Range(3f, 50f)] [SerializeField] private int lineRenderQuality = 40;
        [TabGroup("Aesthetic Settings")] [Space(10)] [SerializeField] [Required] private GameObject sceneChangeWipe;
        [TabGroup("Aesthetic Settings")] [Indent] [SerializeField] [Range(.25f, 5f)] private float sceneWipeDuration;

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
            volume.profile.TryGetSettings(out vignetteLayer);
            parent = new GameObject("Locomotion/Calculations");
            Transform parentTransform = parent.transform;
            parentTransform.SetParent(transform);
            
            cN = new GameObject("Locomotion/Temporary");
            
            rCf = new GameObject("Locomotion/Follow/Right");
            rCp = new GameObject("Locomotion/Proxy/Right");
            rCn = new GameObject("Locomotion/Normalised/Right");
            rMp = new GameObject("Locomotion/MidPoint/Right");
            rTs = new GameObject("Locomotion/Target/Right");
            rHp = new GameObject("Locomotion/HitPoint/Right");
            rRt = new GameObject("Locomotion/Rotation/Right");
            
            lCf = new GameObject("Locomotion/Follow/Left");
            lCp = new GameObject("Locomotion/Proxy/Left");
            lCn = new GameObject("Locomotion/Normalised/Left");
            lMp = new GameObject("Locomotion/MidPoint/Left");
            lTs = new GameObject("Locomotion/Target/Left");
            lHp = new GameObject("Locomotion/HitPoint/Left");
            lRt = new GameObject("Locomotion/Rotation/Left");
            
            rVo = Instantiate(targetVisual, rHp.transform);
            rVo.name = "Locomotion/Visual/Right";
            rVo.SetActive(false);
            
            lVo = Instantiate(targetVisual, lHp.transform);
            lVo.name = "Locomotion/Visual/Left";
            lVo.SetActive(false);
            
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
        }

        private void Update()
        {
            // set the positions of the local objects and calculate the depth based on the angle of the controller
            rTs.transform.LocalDepth(
                rCf.ControllerAngle(
                    rCp, 
                    rCn, 
                    controllerTransforms.RightTransform(), 
                    controllerTransforms.CameraTransform(),
                    controllerTransforms.debugActive).CalculateDepth(MaxAngle, MinAngle, max, min, rCp.transform),
                false, 
                .2f);
            lTs.transform.LocalDepth(
                lCf.ControllerAngle(
                    lCp, 
                    lCn, 
                    controllerTransforms.LeftTransform(), 
                    controllerTransforms.CameraTransform(), 
                    controllerTransforms.debugActive).CalculateDepth(MaxAngle, MinAngle, max, min, lCp.transform), 
                false, 
                .2f);
            
            // detect valid positions for the target
            rTs.TargetLocation(rHp,
                rLastValidPosition = rTs.LastValidPosition(rLastValidPosition), 
                layerIndex);
            lTs.TargetLocation(lHp, 
                lLastValidPosition = lTs.LastValidPosition(lLastValidPosition),
                layerIndex);

            // set the midpoint position
            rMp.transform.LocalDepth(rCp.transform.Midpoint(rTs.transform), false, 0f);
            lMp.transform.LocalDepth(lCp.transform.Midpoint(lTs.transform), false, 0f);
            
            // set the rotation of the target based on the joystick values
            rVo.Target(rHp, rCn.transform, controllerTransforms.RightJoystick(), rRt, advancedLocomotion);
            lVo.Target(lHp, lCn.transform, controllerTransforms.LeftJoystick(), lRt, advancedLocomotion);
            
            // draw the line renderer
            rLr.BezierLineRenderer(controllerTransforms.RightTransform().position,rMp.transform.position,rHp.transform.position,lineRenderQuality);
            lLr.BezierLineRenderer(controllerTransforms.LeftTransform().position, lMp.transform.position, lHp.transform.position, lineRenderQuality);
        }

        private void LateUpdate()
        {
            cTouchR = controllerTransforms.RightLocomotion();
            cTouchL = controllerTransforms.LeftLocomotion();
            
            rJoystickValues.JoystickTracking(
                controllerTransforms.RightJoystick(),
                Sensitivity);
            lJoystickValues.JoystickTracking(
                controllerTransforms.LeftJoystick(),
                Sensitivity);
            
            this.JoystickGestureDetection(
                controllerTransforms.RightJoystick(), 
                rJoystickValues[0], 
                angle, 
                rotateSpeed, 
                Trigger, 
                Tolerance,
                rVo,
                rLr, 
                cTouchR, 
                pTouchR, 
                disableRightHand,
                active);
            this.JoystickGestureDetection(
                controllerTransforms.LeftJoystick(), 
                lJoystickValues[0], 
                angle,
                rotateSpeed,
                Trigger,
                Tolerance,
                lVo,
                lLr,
                cTouchL,
                pTouchL,
                disableLeftHand,
                active);
            
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
            if(transform.parent == cN.transform || !rotation || controllerTransforms.LeftHandEnabled() || controllerTransforms.RightHandEnabled()) return;
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
            vignetteLayer.intensity.value = intensity;
        }
        
        [Button, HideInEditorMode]
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
