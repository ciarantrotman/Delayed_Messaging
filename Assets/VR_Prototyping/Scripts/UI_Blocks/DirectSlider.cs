using UnityEngine;
using UnityEngine.Events;
using VR_Prototyping.Scripts.Utilities;

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

        [Range(.01f, .5f)] [SerializeField] private float directGrabDistance = .02f;
        [Header("Slider Values")] [Space(5)] [SerializeField] [Range(0f, 1f)] private float startingValue;
        [Range(.01f, 2f)] public float sliderMax;
        [Range(.01f, 2f)] public float sliderMin;
        [Space(5)] public bool ignoreLeftHand;
        public bool ignoreRightHand;
        
        [SerializeField, Range(.001f, .005f)] private float activeWidth;
        [SerializeField, Range(.001f, .005f)] private float inactiveWidth;
        [SerializeField, Space(10)] private Material sliderMaterial;
        [Space(5)] public GameObject sliderCap;
        public GameObject sliderHandle;
        
        [SerializeField] private UnityEvent hoverStart;
        [SerializeField] private UnityEvent hoverStay;
        [SerializeField] private UnityEvent hoverEnd;
        [Space(10)] [SerializeField] private UnityEvent grabStart;
        [SerializeField] private UnityEvent grabStay;
        [SerializeField] private UnityEvent grabEnd;
        
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
}