using System;
using System.Collections;
using UnityEditor;
using UnityEngine;
using UnityEngine.Events;

namespace VR_Prototyping.Scripts.UI_Blocks
{
    public abstract class DirectButton : BaseDirectBlock
    {
        private LineRenderer targetLr;

        private MeshFilter buttonSurface;
        protected MeshRenderer ButtonVisual;
        private MeshCollider buttonCollider;

        protected GameObject RestTarget;
        protected GameObject HoverTarget;
        protected GameObject ToggleTarget;

        private bool state;
        private bool statePrevious;

        //private bool disable = true;

        private const float Tolerance = .005f;
        const float SpawnDelayDuration = 1.5f;

        public enum ButtonState
        {
            INACTIVE,
            HOVER,
            ACTIVE
        }

        private ButtonState buttonState { get; set; }
        private ButtonState previousButtonState;

        protected GameObject Button;
        private GameObject parent;
        protected GameObject Target;
        private GameObject visual;

        protected bool Active;
        protected bool ToggleState;

        [SerializeField] public bool placeholderButton = true;

        [SerializeField] [Header("Button Function")]
        public bool toggle;

        [SerializeField] protected bool startsActive;
        [Range(.01f, .05f)] public float toggleDepth;

        [Header("Button Parameters")] [Space(5)] [Range(0f, 10f)]
        public float springiness = 10f;

        [Space(5)] [Range(.01f, .5f)] [SerializeField]
        private float hoverDistance = .01f;

        [Space(5)] [Range(0f, .1f)] public float restDepth = .02f;
        [Space(5)] public bool ignoreLeftHand;
        public bool ignoreRightHand;

        [Header("Button Setup")] [Space(10), SerializeField]
        public GameObject customButton;

        [SerializeField] public GameObject customHoverVisual;
        [Range(.01f, .5f)] public float hoverThreshold = .1f;

        [Space(10)] public float hoverDepth = .02f;
        [Range(.01f, .05f)] public float buttonRadius = .02f;
        [Range(.01f, .05f)] public float targetRadius = .025f;
        [SerializeField, Range(.001f, .005f)] private float targetLineRenderWidth = .002f;
        [SerializeField, Range(6, 360)] private int circleQuality = 360;
        [SerializeField, Space(10)] private Material buttonMaterial;
        [SerializeField] private Color buttonColor = new Color(255f, 255f, 255f, 255f);
        [SerializeField, Space(10)] private Material targetMaterial;

        private bool ValidateRadius(float value)
        {
            return value > buttonRadius;
        }

        private bool ValidateDepth(float value)
        {
            return value > restDepth;
        }

        public UnityEvent activate;
        public UnityEvent deactivate;
        public UnityEvent hoverStart;

        private static readonly int EmissionColor = Shader.PropertyToID("_EmissionColor");
        private static readonly int MatColor = Shader.PropertyToID("_Diffusecolor");

        private void Start()
        {
            InitialiseSelectableObject();
        }

        public void InitialiseSelectableObject()
        {
            SetupButton();
            ButtonSetup();

            if (!toggle) return;
            if (startsActive)
            {
                //disable = false;
            }
            else
            {
                //StartCoroutine(ToggleDelay());
            }
        }

        private IEnumerator ToggleDelay()
        {
            yield return new WaitForSeconds(SpawnDelayDuration);
            //disable = false;
            yield return null;
        }

        protected virtual void ButtonSetup()
        {

        }

        private void SetupButton()
        {
            parent = gameObject;
            parent.name += " // Button/Parent";
            Target = new GameObject("Button/Target");
            RestTarget = new GameObject("Button/RestTarget");
            HoverTarget = new GameObject("Button/HoverTarget");
            ToggleTarget = new GameObject("Button/ToggleTarget");
            Target.transform.SetParent(parent.transform);
            RestTarget.transform.SetParent(parent.transform);
            HoverTarget.transform.SetParent(parent.transform);
            ToggleTarget.transform.SetParent(parent.transform);
            Target.transform.LocalTransformZero();
            switch (placeholderButton)
            {
                case true:
                    visual = new GameObject("Button/Visual") {layer = controller.layerIndex};
                    Button = new GameObject("Button/Button") {layer = controller.layerIndex};

                    Button.transform.SetParent(parent.transform);
                    visual.transform.SetParent(Button.transform);

                    rb = Button.transform.AddOrGetRigidbody();

                    targetLr = LineRender(Target.transform, targetLineRenderWidth);
                    targetLr.CircleLineRenderer(targetRadius, Draw.Orientation.Forward, circleQuality);

                    buttonSurface = visual.AddComponent<MeshFilter>();
                    ButtonVisual = visual.AddComponent<MeshRenderer>();
                    ButtonVisual.material = buttonMaterial;
                    buttonSurface.mesh = buttonRadius.GenerateCircleMesh(Draw.Orientation.Forward);
                    ButtonVisual.material.SetColor(MatColor, buttonColor);
                    ButtonVisual.material.SetColor(EmissionColor, buttonColor);
                    buttonCollider = visual.AddComponent<MeshCollider>();
                    buttonCollider.convex = true;

                    Button.transform.LocalTransformZero();
                    visual.transform.LocalTransformZero();

                    RestTarget.transform.localPosition = Offset(Target.transform, restDepth);
                    HoverTarget.transform.localPosition = Offset(Target.transform, hoverDepth);
                    ToggleTarget.transform.localPosition = Offset(Target.transform, -toggleDepth);
                    break;
                default:
                    customButton.layer = controller.layerIndex;
                    rb = customButton.transform.AddOrGetRigidbody();
                    RestTarget.transform.localPosition = Offset(customButton.transform, restDepth);
                    HoverTarget.transform.localPosition = Offset(customButton.transform, hoverDepth);
                    ToggleTarget.transform.localPosition = Offset(customButton.transform, -toggleDepth);
                    //customButton.transform.localPosition = RestTarget.transform.localPosition;
                    Button = customButton;
                    break;
            }

            rb.RigidBody(.1f, 10f, true, false);
            rb.constraints = RigidbodyConstraints.FreezeRotation;

            buttonState = toggle && startsActive ? ButtonState.ACTIVE : ButtonState.INACTIVE;
        }

        private LineRenderer LineRender(Component a, float width)
        {
            LineRenderer lr = a.gameObject.AddComponent<LineRenderer>();
            lr.SetupLineRender(targetMaterial, width, true);
            return lr;
        }

        private void FixedUpdate()
        {
            SetState();
            ButtonAlignment();
            ButtonUpdate();

            Vector3 buttonPos = Button.transform.position;
            switch (buttonState)
            {
                case ButtonState.INACTIVE:
                    rb.AddForce(Force(RestTarget, buttonPos, springiness), ForceMode.Force);
                    break;
                case ButtonState.HOVER when placeholderButton:
                    rb.AddForce(Force(HoverTarget, buttonPos, springiness), ForceMode.Force);
                    break;
                case ButtonState.ACTIVE:
                    rb.AddForce(Force(RestTarget, buttonPos, springiness), ForceMode.Force);
                    break;
                default:
                    rb.AddForce(Force(RestTarget, buttonPos, springiness), ForceMode.Force);
                    break;
            }

            statePrevious = state;

            if (!controller.debugActive) return;
            Debug.DrawRay(buttonPos, Force(HoverTarget, buttonPos, springiness), Color.yellow);
            Debug.DrawRay(buttonPos, Force(RestTarget, buttonPos, springiness), Color.red);
            Debug.DrawRay(buttonPos, Force(Target, buttonPos, springiness), Color.green);
        }

        protected virtual void ButtonUpdate()
        {

        }

        private void ButtonAlignment()
        {
            Vector3 local = Button.transform.localPosition;
            Button.transform.localPosition = new Vector3(0, 0, local.z);
        }

        private void SetState()
        {
            if ((DirectCheck() && buttonState != ButtonState.ACTIVE) && placeholderButton)
            {
                buttonState = ButtonState.HOVER;
                hoverStart.Invoke();
            }
            else if ((!DirectCheck() && buttonState != ButtonState.ACTIVE) && placeholderButton)
            {
                buttonState = ButtonState.INACTIVE;
                state = false;
                return;
            }

            if (!Active && ActiveDistance() && buttonState == ButtonState.INACTIVE && !toggle)
            {
                Active = true;
                activate.Invoke();
                state = true;
            }

            if (ToggleDistance() && toggle && !ToggleState)
            {
                switch (buttonState)
                {
                    case ButtonState.INACTIVE:
                        SetToggleState(true, true);
                        break;
                    case ButtonState.HOVER:
                        return;
                    case ButtonState.ACTIVE:
                        SetToggleState(false, true);
                        break;
                    default:
                        throw new ArgumentOutOfRangeException();
                }
            }

            if (Active && RestDistance() && !toggle)
            {
                deactivate.Invoke();
                Active = false;
                return;
            }

            if (ToggleState && RestDistance() && toggle)
            {
                ToggleState = false;
            }
        }

        private bool ActiveDistance()
        {
            return Vector3.Distance(Button.transform.position, Target.transform.position) <= Tolerance;
        }

        private bool ToggleDistance()
        {
            return Vector3.Distance(Button.transform.position, ToggleTarget.transform.position) <= Tolerance;
        }

        private bool RestDistance()
        {
            return Vector3.Distance(Button.transform.position, RestTarget.transform.position) <= Tolerance;
        }

        private bool DirectCheck()
        {
            Vector3 buttonPos = Button.transform.position;

            if (ignoreLeftHand)
            {
                return Vector3.Distance(buttonPos, controller.RightPosition()) <= hoverDistance;
            }

            if (ignoreRightHand)
            {
                return Vector3.Distance(buttonPos, controller.LeftPosition()) <= hoverDistance;
            }

            return Vector3.Distance(buttonPos, controller.LeftPosition()) <= hoverDistance ||
                   Vector3.Distance(buttonPos, controller.RightPosition()) <= hoverDistance;
        }

        public void SetToggleState(bool active, bool trigger)
        {
            switch (active)
            {
                case true:
                    buttonState = ButtonState.ACTIVE;
                    Active = true;
                    if (trigger)
                    {
                        activate.Invoke();
                    }

                    break;
                default:
                    buttonState = ButtonState.INACTIVE;
                    Active = false;
                    if (trigger)
                    {
                        deactivate.Invoke();
                    }

                    break;
            }
        }

        public static Vector3 Offset(Transform local, float offset)
        {
            Vector3 pos = local.localPosition;
            return new Vector3(pos.x, pos.y, pos.z + offset);
        }

        private static Vector3 Force(GameObject a, Vector3 b, float force)
        {
            return (a.transform.position - b) * force;
        }
    }
}
