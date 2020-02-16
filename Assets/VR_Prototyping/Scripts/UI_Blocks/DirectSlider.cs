using Sirenix.OdinInspector;
using UnityEditor;
using UnityEngine;
using UnityEngine.Events;

namespace VR_Prototyping.Scripts.UI_Blocks
{
    [DisallowMultipleComponent]
    public class DirectSlider : BaseDirectBlock
    {
        public LineRenderer ActiveLr { get; private set; }
        public LineRenderer InactiveLr { get; private set; }
        
        private const float DirectDistance = .05f;

        private GameObject slider;
        private GameObject min;
        private GameObject max;
        private GameObject handle;
        private GameObject handleNormalised;

        private bool cHover;
        private bool pHover;
        private bool cGrab;
        private bool pGrab;
        
        [HideInInspector] public Vector3 sliderMaxPos;
        [HideInInspector] public Vector3 sliderMinPos;
        [HideInInspector] public float sliderValue;

        [TabGroup("Slider Settings")] [Range(.01f, .5f)] [SerializeField] private float directGrabDistance = .02f;
        [TabGroup("Slider Settings")] [Header("Slider Values")] [Space(5)] [SerializeField] [Range(0f, 1f)] private float startingValue;
        [TabGroup("Slider Settings")] [HideInPlayMode] [Indent] [Range(.01f, 2f)] public float sliderMax;
        [TabGroup("Slider Settings")] [HideInPlayMode] [Indent] [Range(.01f, 2f)] public float sliderMin;
        [TabGroup("Slider Settings")] [Space(5)] public bool ignoreLeftHand;
        [TabGroup("Slider Settings")] public bool ignoreRightHand;
        
        [TabGroup("Aesthetics Settings")] [SerializeField] [HideInPlayMode] [Range(.001f, .005f)] private float activeWidth;
        [TabGroup("Aesthetics Settings")] [SerializeField] [HideInPlayMode] [Range(.001f, .005f)] private float inactiveWidth;
        [TabGroup("Aesthetics Settings")] [SerializeField] [Required] [Space(10)] private Material sliderMaterial;
        [TabGroup("Aesthetics Settings")] [Required] [Space(5)] public GameObject sliderCap;
        [TabGroup("Aesthetics Settings")] [Required] public GameObject sliderHandle;
        
        [FoldoutGroup("Slider Events")] [SerializeField] private UnityEvent hoverStart;
        [FoldoutGroup("Slider Events")] [SerializeField] private UnityEvent hoverStay;
        [FoldoutGroup("Slider Events")] [SerializeField] private UnityEvent hoverEnd;
        [FoldoutGroup("Slider Events")] [Space(10)] [SerializeField] private UnityEvent grabStart;
        [FoldoutGroup("Slider Events")] [SerializeField] private UnityEvent grabStay;
        [FoldoutGroup("Slider Events")] [SerializeField] private UnityEvent grabEnd;
        
        private void Start()
        {
            InitialiseSelectableObject();
        }

        private void InitialiseSelectableObject()
        {
            SetupSlider();
        }

        public void SetupSlider()
        {
            GameObject o = gameObject;
            slider = o;
            o.name = "Slider/Slider";
            min = Instantiate(sliderCap, slider.transform);
            min.name = "Slider/Min";
            max = Instantiate(sliderCap, slider.transform);
            max.name = "Slider/Max";
            handle = Instantiate(sliderHandle, slider.transform);
            handleNormalised = new GameObject("Slider/Handle/Follow");

            min.transform.SetParent(slider.transform);
            max.transform.SetParent(slider.transform);
            handle.transform.SetParent(slider.transform);
            handleNormalised.transform.SetParent(slider.transform);

            ActiveLr = LineRender(min.transform, activeWidth);
            InactiveLr = LineRender(max.transform, inactiveWidth);

            rb = handle.transform.AddOrGetRigidbody();
            rb.RigidBody(.1f, 4.5f, true, false);
            
            min.transform.localPosition = new Vector3(-sliderMin, 0, 0);
            max.transform.localPosition = new Vector3(sliderMax, 0, 0);
            handle.transform.localPosition = new Vector3(Mathf.Lerp(-sliderMin, sliderMax, startingValue), 0, 0);
            handleNormalised.transform.localPosition = handle.transform.localPosition;
            
            sliderValue = SliderValue(sliderMax, sliderMin, handleNormalised.transform.localPosition.x);
        }
        
        private LineRenderer LineRender(Component a, float width)
        {
            LineRenderer lr = a.gameObject.AddComponent<LineRenderer>();
            lr.SetupLineRender(sliderMaterial, width, true);
            return lr;
        }

        private void FixedUpdate()
        {
            ActiveLr.StraightLineRender(min.transform, handle.transform);
            InactiveLr.StraightLineRender(max.transform, handle.transform);

            if (!ignoreRightHand)
            {
                DirectSliderCheck(controller.RightTransform(), controller.RightGrab());
            }

            if (!ignoreLeftHand)
            {
                DirectSliderCheck(controller.LeftTransform(), controller.LeftGrab());
            }

            TriggerEvent(hoverStart, hoverStay, hoverEnd, cHover, pHover);
            TriggerEvent(grabStart, grabStay, grabEnd, cGrab, pGrab);

        }

        private void DirectSliderCheck(Transform controllerTransform, bool grab)
        {          
            if (Vector3.Distance(handleNormalised.transform.position, controllerTransform.position) < directGrabDistance && !grab)
            {
                handle.transform.TransformLerpPosition(controllerTransform, .05f);
                cHover = true;
            }
            if (Vector3.Distance(handle.transform.position, controllerTransform.position) < DirectDistance && grab)
            {
                sliderValue = SliderValue(sliderMax, sliderMin, HandleFollow());
                handle.transform.TransformLerpPosition(controllerTransform, .5f);
                cGrab = true;
                return;
            }
            handle.transform.TransformLerpPosition(handleNormalised.transform, .2f);
            
            pHover = cHover;
            pGrab = cGrab;
        }
        
        private static float SliderValue(float max, float min, float current)
        {
            return Mathf.InverseLerp(-min, max, current);
        }

        private float HandleFollow()
        {
            Vector3 value = handle.transform.localPosition;
            Vector3 target = new Vector3(value.x, 0, 0);
            if (value.x <= -sliderMin) target = new Vector3(-sliderMin, 0, 0);
            if (value.x >= sliderMax) target = new Vector3(sliderMax, 0, 0);
            handleNormalised.transform.VectorLerpLocalPosition(target, .2f);
            return handleNormalised.transform.localPosition.x;
        }
        
        public void AlignHandles(float pos, float neg)
        {
            Vector3 p = transform.localPosition;
            sliderMaxPos = new Vector3(p.x + pos, p.y, p.z);
            sliderMinPos = new Vector3(p.x - neg, p.y, p.z);
        }

        private static void TriggerEvent(UnityEvent start, UnityEvent stay, UnityEvent end, bool current, bool previous)
        {
            if (current && !previous)
            {
                start.Invoke();
            }

            if (current && previous)
            {
                stay.Invoke();
            }

            if (!current && previous)
            {
                end.Invoke();
            }
        }
    }
    #if UNITY_EDITOR
    [CustomEditor(typeof(DirectSlider)), CanEditMultipleObjects]
    public sealed class DirectSliderSetup : Sirenix.OdinInspector.Editor.OdinEditor
    {
        private void OnSceneGUI()
        {
            DirectSlider slider = (DirectSlider)target;
            Transform transform = slider.transform;
            Vector3 right = transform.right;
            Vector3 position = transform.position;
            float size = HandleUtility.GetHandleSize(slider.sliderMaxPos) * .25f;
            const float snap = .1f;
            
            EditorGUI.BeginChangeCheck();
            
            Vector3 max = Handles.Slider(slider.sliderMaxPos, right, size, Handles.ConeHandleCap, snap);
            Vector3 min = Handles.Slider(slider.sliderMinPos, -right, size, Handles.ConeHandleCap, snap);
            
            Handles.DrawLine(position, slider.sliderMaxPos);
            Handles.DrawLine(position, slider.sliderMinPos);
            
            slider.AlignHandles(slider.sliderMax, slider.sliderMin);

            if (!EditorGUI.EndChangeCheck()) return;
            slider.sliderMaxPos = max;
            slider.sliderMinPos = min;
        }
    }
    #endif
}