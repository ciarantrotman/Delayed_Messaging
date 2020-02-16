using Sirenix.OdinInspector;
using UnityEditor;
using UnityEngine;

namespace VR_Prototyping.Scripts.UI_Blocks
{
    [DisallowMultipleComponent]
    public class DirectDial : BaseDirectBlock
    {
        protected LineRenderer activeCircleLr;
        protected LineRenderer inactiveCircleLr;
        protected LineRenderer spokeLr;
        
        private const float DirectDistance = .05f;

        protected GameObject dial;
        protected GameObject center;
        protected GameObject anchor;
        protected GameObject handle;
        protected GameObject handleNormalised;

        [HideInInspector] public float dialValue;

        [TabGroup("Dial Settings")] [Range(.01f, .5f)] [SerializeField] private float directGrabDistance;
        [TabGroup("Dial Settings")] [Header("Dial Values")] [Space(5)] [SerializeField] [Range(0f, 1f)] protected float startingValue;
        [TabGroup("Dial Settings")] [Range(.01f, .25f)] public float dialRadius;
        [TabGroup("Dial Settings")] [Space(5)] public bool ignoreLeftHand;
        [TabGroup("Dial Settings")] public bool ignoreRightHand;
        
        [TabGroup("Aesthetics Settings")] [SerializeField] [Range(.001f, .005f)] protected float activeCircleLineRendererWidth;
        [TabGroup("Aesthetics Settings")] [SerializeField] [Range(.001f, .005f)] protected float inactiveCircleLineRendererWidth;
        [TabGroup("Aesthetics Settings")] [SerializeField] [Range(.001f, .005f)] protected float spokeLineRendererWidth;
        [TabGroup("Aesthetics Settings")] [SerializeField] [Indent] [Range(6, 360)] protected int circleQuality;
        [TabGroup("Aesthetics Settings")] [SerializeField] [Required] [Space(10)] protected Material dialMaterial;
        [TabGroup("Aesthetics Settings")] [Required] [Space(5)] [SerializeField] protected GameObject dialCap;
        [TabGroup("Aesthetics Settings")] [Required] [SerializeField] protected GameObject dialHandle;

        private void Start()
        {
            InitialiseSelectableObject();
        }

        private void InitialiseSelectableObject()
        {
            SetupDial();
        }

        public void SetupDial()
        {
            GameObject o = gameObject;
            dial = o;
            o.name += "//Dial/Dial";
            center = new GameObject("Dial/Center");
            handle = Instantiate(dialHandle, dial.transform);
            handle.name = "Dial/Handle";
            handleNormalised = new GameObject("Dial/Handle/Follow");
            anchor = new GameObject("Dial/Anchor");
            GameObject vis = Instantiate(dialCap, anchor.transform);
            vis.name = "Dial/Cap";
            
            vis.transform.SetParent(anchor.transform);
            center.transform.SetParent(dial.transform);
            anchor.transform.SetParent(center.transform);
            handle.transform.SetParent(dial.transform);
            handleNormalised.transform.SetParent(dial.transform);

            activeCircleLr = LineRender(dial.transform, activeCircleLineRendererWidth);
            inactiveCircleLr = LineRender(center.transform, inactiveCircleLineRendererWidth);
            spokeLr = LineRender(handle.transform, spokeLineRendererWidth);

            inactiveCircleLr.CircleLineRenderer(dialRadius, Draw.Orientation.Right, circleQuality);
            
            rb = handle.transform.AddOrGetRigidbody();
            rb.RigidBody(.1f, 4.5f, true, false);
            
            center.transform.localPosition = Vector3.zero;
            center.transform.localEulerAngles = new Vector3(0, Mathf.Lerp(0,360, startingValue), 0);
            vis.transform.localPosition = Vector3.zero;
            anchor.transform.localPosition = new Vector3(0, 0, dialRadius);
            handle.transform.position = anchor.transform.position;
            handleNormalised.transform.localPosition = handle.transform.localPosition;

            dialValue = DialValue(0, 360, center.transform.localEulerAngles.y);
        }

        protected LineRenderer LineRender(Component a, float width)
        {
            LineRenderer lr = a.gameObject.AddComponent<LineRenderer>();
            lr.SetupLineRender(dialMaterial, width, true);
            return lr;
        }
        
        private void FixedUpdate()
        {
            spokeLr.StraightLineRender(anchor.transform, handle.transform);
            activeCircleLr.ArcLineRenderer(dialRadius, 0, center.transform.localEulerAngles.y, Draw.Orientation.Right, circleQuality);

            if (!ignoreRightHand)
            {
                DirectDialCheck(controller.RightTransform(), controller.RightGrab());
            }

            if (!ignoreLeftHand)
            {
                DirectDialCheck(controller.LeftTransform(), controller.LeftGrab());
            }
        }

        private void DirectDialCheck(Transform activeController, bool grab)
        {          
            if (Vector3.Distance(anchor.transform.position, activeController.position) < directGrabDistance && !grab)
            {
                handle.transform.TransformLerpPosition(activeController, .05f);
            }
            if (Vector3.Distance(handle.transform.position, activeController.position) < DirectDistance && grab)
            {
                dialValue = DialValue(360, 0, HandleFollow());
                handle.transform.TransformLerpPosition(activeController, .5f);
                return;
            }
            handle.transform.TransformLerpPosition(anchor.transform, .05f);
        }
        
        private static float DialValue(float max, float min, float current)
        {
            return Mathf.InverseLerp(-min, max, current);
        }

        private float HandleFollow()
        {
            Vector3 value = handle.transform.localPosition;
            Vector3 target = new Vector3(value.x, 0, value.z);
            handleNormalised.transform.VectorLerpLocalPosition(target, .2f);
            center.transform.LookAt(handleNormalised.transform);
            Vector3 localEulerAngles = center.transform.localEulerAngles;
            localEulerAngles = new Vector3(0, localEulerAngles.y, 0);
            center.transform.localEulerAngles = localEulerAngles;
            return localEulerAngles.y;
        }
    }
    #if UNITY_EDITOR
    [CustomEditor(typeof(DirectDial)), CanEditMultipleObjects]
    public sealed class DirectDialSetup : Sirenix.OdinInspector.Editor.OdinEditor
    {
        private void OnSceneGUI()
        {
            DirectDial dial = (DirectDial)target;
            Transform transform = dial.transform;
            Vector3 up = transform.up;
            Vector3 position = transform.position;
            const float arc = 360f;

            Handles.DrawWireArc(position, up, transform.forward, arc, dial.dialRadius);
        }
    }
    #endif
}