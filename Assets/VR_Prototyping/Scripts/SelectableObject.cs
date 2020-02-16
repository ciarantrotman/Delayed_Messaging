using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using DG.Tweening;
using Leap.Unity.Interaction;
using Sirenix.OdinInspector;
using TMPro;
using UnityEditor;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.SceneManagement;
using VR_Prototyping.Interfaces;
using VR_Prototyping.Plugins.QuickOutline.Scripts;
using VR_Prototyping.Scripts.Accessibility;
using VR_Prototyping.Scripts.Icon_Scripts;
using Object = UnityEngine.Object;

namespace VR_Prototyping.Scripts
{
	[DisallowMultipleComponent, CanEditMultipleObjects]
	public abstract class SelectableObject : MonoBehaviour, ISelectableObject
	{
		#region Inspector and Variables

		internal ControllerTransforms controllerTransforms;
		internal ObjectSelection objectSelection;
		internal Locomotion locomotion;
		internal Manipulation manipulation;
		private Outline outline;
		
		private Vector3 defaultPosition;
		private Vector3 defaultLocalPosition;
		private Vector3 defaultLocalScale;
		private Vector3 scaleMax;
		private Vector3 scaleMin;

		private bool active;
		
		private bool pGrabR;
		private bool pGrabL;

		private Transform originalParent;

		private bool pDualGrab;
		
		private RotationLock rotLock;
		private float manualRef;

		private float buttonBlendShapeWeight = BlendShapeInactive;
		private float buttonBorderDepth;
		private MeshRenderer textRenderer;
		private Rigidbody rb;

		private InteractionBehaviour interactionBehaviour;
		
		public float AngleL { get; private set; }
		public float AngleR { get; private set; }
		public float AngleG { get; private set; }
		public Renderer Renderer { get; private set; }

		public Transform ResetState { get; private set; }

		internal bool toggleState;
		
		public enum RotationLock
		{
			FREE_ROTATION,
			Y_ROTATION_ONLY
		}
		private enum ButtonTrigger
		{
			ON_BUTTON_DOWN,
			ON_BUTTON_UP				
		}
		
		private enum HoverAxis
		{
			X,
			Y,
			Z
		}
		
		private readonly List<Vector3> positions = new List<Vector3>();
		private readonly List<Vector3> rotations = new List<Vector3>();
		private const float Sensitivity = 10f;
	
		[BoxGroup("Script Setup")] public bool instantiated;
		[BoxGroup("Script Setup")] [Required] [HideIf("instantiated")] public GameObject player;
		[Header("Define Object Behaviour")]
		[BoxGroup("Script Setup")] [HideIf("button")] [SerializeField] private bool grab;
		[BoxGroup("Script Setup")] [HideIf("grab")] [SerializeField] private bool button;
		[BoxGroup("Script Setup")] [ShowIf("button")] [SerializeField] [Indent] private bool toggle;
		[BoxGroup("Script Setup")] [ShowIf("button")] [SerializeField] [Indent(2)] private bool startsActive;
		[BoxGroup("Script Setup")] [ShowIf("button")] [SerializeField] [Indent] private bool menu;
		[BoxGroup("Script Setup")] [ShowIf("button")] [ShowIf("menu")] [Indent(2)] public GameObject menuItems;
		
		[Header("Grab Settings")]
		[FoldoutGroup("Manipulation Settings"), ShowIf("grab"), Range(0, 1f)] public float moveForce = .15f;
		[FoldoutGroup("Manipulation Settings"), ShowIf("grab"), Range(0, 10f)] public float latency = 4.5f;
		[FoldoutGroup("Manipulation Settings"), ShowIf("grab"), SerializeField] private bool gravity;
		[FoldoutGroup("Manipulation Settings"), ShowIf("grab"), ShowIf("grab")] public bool directGrab = true;
		[Header("Rotation Settings")]
		[FoldoutGroup("Manipulation Settings"), ShowIf("grab"), Space(5), SerializeField] public bool freeRotationEnabled;
		[FoldoutGroup("Manipulation Settings"), ShowIf("grab"), HideIf("freeRotationEnabled"), Indent, SerializeField] public RotationLock rotationLock;
		[Header("Scaling Settings")]
		[FoldoutGroup("Manipulation Settings"), ShowIf("grab"), Space(5), SerializeField] public bool scalingEnabled;
		[FoldoutGroup("Manipulation Settings"), ShowIf("grab"), HideInPlayMode, ShowIf("scalingEnabled"), Indent, Range(.01f, 1f)] public float minScaleFactor;
		[FoldoutGroup("Manipulation Settings"), ShowIf("grab"), HideInPlayMode, ShowIf("scalingEnabled"), Indent, Range(1f, 10f)] public float maxScaleFactor;
		[Header("Visual Effects")]
		[FoldoutGroup("Manipulation Settings"), ShowIf("grab"), Space(5)] [SerializeField] private bool genericGrabEffect;
		[FoldoutGroup("Manipulation Settings"), ShowIf("grab"), ShowIf("genericGrabEffect"), Indent] [SerializeField] private bool grabOutline;
		[FoldoutGroup("Manipulation Settings"), ShowIf("grab"), ShowIf("genericGrabEffect"), ShowIf("grabOutline"), Indent(2), Range(1f, 10f), SerializeField] private float grabOutlineWidth;
		[FoldoutGroup("Manipulation Settings"), ShowIf("grab"), ShowIf("genericGrabEffect"), ShowIf("grabOutline"), Indent(2), SerializeField] private Color grabOutlineColor = new Color(0,0,0,255);
		[FoldoutGroup("Manipulation Settings"), ShowIf("grab"), ShowIf("genericGrabEffect"), ShowIf("grabOutline"), Indent(2), SerializeField] private Outline.Mode grabOutlineMode;
		[FoldoutGroup("Manipulation Settings"), ShowIf("grab"), ShowIf("genericGrabEffect"), ShowIf("directGrab"), Indent] [SerializeField] private bool touchOutline;
		[FoldoutGroup("Manipulation Settings"), ShowIf("grab"), ShowIf("genericGrabEffect"), ShowIf("directGrab"), ShowIf("touchOutline"), Indent(2), Range(1f, 10f), SerializeField] private float touchOutlineWidth;
		[FoldoutGroup("Manipulation Settings"), ShowIf("grab"), ShowIf("genericGrabEffect"), ShowIf("directGrab"), ShowIf("touchOutline"), Indent(2), SerializeField] private Color touchOutlineColor = new Color(0,0,0,255);
		[FoldoutGroup("Manipulation Settings"), ShowIf("grab"), ShowIf("genericGrabEffect"), ShowIf("directGrab"), ShowIf("touchOutline"), Indent(2), SerializeField] private Outline.Mode touchOutlineMode;
		[FoldoutGroup("Manipulation Settings"), ShowIf("grab"), HideIf("genericGrabEffect"), ShowIf("directGrab"), Indent] public UnityEvent grabStart;
		[FoldoutGroup("Manipulation Settings"), ShowIf("grab"), HideIf("genericGrabEffect"), ShowIf("directGrab"), Indent] public UnityEvent grabStay;
		[FoldoutGroup("Manipulation Settings"), ShowIf("grab"), HideIf("genericGrabEffect"), ShowIf("directGrab"), Indent] public UnityEvent grabEnd;
		[FoldoutGroup("Manipulation Settings"), ShowIf("grab"), HideIf("genericGrabEffect"), ShowIf("directGrab"), Indent] public UnityEvent dualGrabStart;
		[FoldoutGroup("Manipulation Settings"), ShowIf("grab"), HideIf("genericGrabEffect"), ShowIf("directGrab"), Indent] public UnityEvent dualGrabStay;
		[FoldoutGroup("Manipulation Settings"), ShowIf("grab"), HideIf("genericGrabEffect"), ShowIf("directGrab"), Indent] public UnityEvent dualGrabEnd;
		
		[Header("Visual Effects")]
		[FoldoutGroup("Button Settings"), ShowIf("button"), HideIf("blendShapeButton"), Space(5)] [SerializeField] private bool genericSelectState;
		[FoldoutGroup("Button Settings"), ShowIf("button"), HideIf("blendShapeButton"), ShowIf("genericSelectState"), Required] public TextMeshPro buttonText;
		[FoldoutGroup("Button Settings"), ShowIf("button"), HideIf("blendShapeButton"), ShowIf("genericSelectState"), Required] public MeshRenderer buttonBack;
		[FoldoutGroup("Button Settings"), ShowIf("button"), HideIf("blendShapeButton"), ShowIf("genericSelectState"), Indent, Range(0, 1f), SerializeField] private float selectOffset;
		[FoldoutGroup("Button Settings"), ShowIf("button"), HideIf("blendShapeButton"), ShowIf("genericSelectState"), Indent, Range(0, 1f), SerializeField] private float selectScale;
		[FoldoutGroup("Button Settings"), ShowIf("button"), HideIf("blendShapeButton"), ShowIf("genericSelectState"), Indent, Range(0, 1f), SerializeField] private float selectEffectDuration = .1f;
		[FoldoutGroup("Button Settings"), ShowIf("button"), HideIf("blendShapeButton"), ShowIf("genericSelectState"), Indent, Required, SerializeField] private TMP_FontAsset activeFont;
		[FoldoutGroup("Button Settings"), ShowIf("button"), HideIf("blendShapeButton"), ShowIf("genericSelectState"), Indent, Required, SerializeField] private TMP_FontAsset inactiveFont;
		[FoldoutGroup("Button Settings"), ShowIf("button"), HideIf("blendShapeButton"), ShowIf("genericSelectState"), Indent, SerializeField] private Color activeColor = new Color(0,0,0,255);
		[FoldoutGroup("Button Settings"), ShowIf("button"), HideIf("blendShapeButton"), ShowIf("genericSelectState"), Indent, SerializeField] private Color inactiveColor = new Color(0,0,0,255);
		[Header("Visual Effects")]
		[FoldoutGroup("Button Settings"), ShowIf("button"), ShowIf("button"), HideIf("genericSelectState"), Space(5), SerializeField] private bool blendShapeButton;
		[FoldoutGroup("Button Settings"), ShowIf("button"), ShowIf("blendShapeButton"), HideIf("genericSelectState"), SerializeField, Required] internal SkinnedMeshRenderer buttonSkinnedMeshRenderer;
		[FoldoutGroup("Button Settings"), ShowIf("button"), ShowIf("blendShapeButton"), HideIf("genericSelectState"), SerializeField, Required] internal GameObject text;
		[FoldoutGroup("Button Settings"), ShowIf("button"), ShowIf("blendShapeButton"), HideIf("genericSelectState"), SerializeField, Required] internal Transform border;
		[FoldoutGroup("Button Settings"), ShowIf("button"), ShowIf("blendShapeButton"), HideIf("genericSelectState"), Space(10)] [SerializeField] private HoverAxis hoverAxis = HoverAxis.Z;
		[FoldoutGroup("Button Settings"), ShowIf("button"), ShowIf("blendShapeButton"), HideIf("genericSelectState"), Indent, SerializeField, Range(0f, .01f)] internal float restDepth;
		[FoldoutGroup("Button Settings"), ShowIf("button"), ShowIf("blendShapeButton"), HideIf("genericSelectState"), Indent, SerializeField, Range(.01f, .5f)] internal float hoverDepth;
		[FoldoutGroup("Button Settings"), ShowIf("button"), ShowIf("blendShapeButton"), HideIf("genericSelectState"), Indent, SerializeField, Range(.1f, 1f)] internal float hoverDuration;
		[FoldoutGroup("Button Settings"), ShowIf("button"), ShowIf("blendShapeButton"), HideIf("genericSelectState"), Space(10)] [Range(.01f, 1)] [SerializeField] internal float buttonAnimationDuration = .5f;
		[FoldoutGroup("Button Settings"), ShowIf("button"), ShowIf("blendShapeButton"), HideIf("genericSelectState"), Indent, SerializeField] internal Color activeFontColor = new Color(255f,255f,255f, 255f);
		[FoldoutGroup("Button Settings"), ShowIf("button"), ShowIf("blendShapeButton"), HideIf("genericSelectState"), Indent, SerializeField] internal Color inactiveFontColor = new Color(45f,45f,45f, 255f);
		
		[FoldoutGroup("Button Settings"), ShowIf("button"), Space(5)] private ButtonTrigger buttonTrigger;
		[FoldoutGroup("Button Settings"), ShowIf("button"), Space(10)] public UnityEvent selectStart;
		[FoldoutGroup("Button Settings"), ShowIf("button")] public UnityEvent selectStay;
		[FoldoutGroup("Button Settings"), ShowIf("button")] public UnityEvent selectEnd;
		[FoldoutGroup("Button Settings"), ShowIf("button"), ShowIf("toggle")] public UnityEvent toggleStart;
		[FoldoutGroup("Button Settings"), ShowIf("button"), ShowIf("toggle")] public UnityEvent toggleEnd;

		[FoldoutGroup("Hover Settings"), SerializeField] private bool reactiveMat;
		[FoldoutGroup("Hover Settings"), ShowIf("reactiveMat"), SerializeField, Indent, Range(0, 1f)] private float clippingDistance;
		[FoldoutGroup("Hover Settings"), SerializeField] private bool hover;
		[FoldoutGroup("Hover Settings"), ShowIf("hover")] public bool toolTip;
		[FoldoutGroup("Hover Settings"), ShowIf("hover"), ShowIf("toolTip")] [Indent] public string toolTipText = "Enter hover state text here.";
		[Header("Visual Effects")]
		[FoldoutGroup("Hover Settings"), ShowIf("hover"), Space(5)] [Indent] [SerializeField] private bool genericHoverEffect;
		[FoldoutGroup("Hover Settings"), ShowIf("hover"), ShowIf("genericHoverEffect"), Indent(2), Range(0, 1f)] [SerializeField] private float hoverOffset;
		[FoldoutGroup("Hover Settings"), ShowIf("hover"), ShowIf("genericHoverEffect"), Indent(2), Range(0, 1f)] [SerializeField] private float hoverScale;
		[FoldoutGroup("Hover Settings"), ShowIf("hover"), ShowIf("genericHoverEffect"), Indent(2), Range(0, 1f)] [SerializeField] private float hoverEffectDuration;
		[FoldoutGroup("Hover Settings"), ShowIf("hover"), ShowIf("genericHoverEffect"), Indent, SerializeField] private bool hoverOutline;
		[FoldoutGroup("Hover Settings"), ShowIf("hover"), ShowIf("genericHoverEffect"), ShowIf("hoverOutline"), Indent(2), Range(1f, 10f), SerializeField] private float hoverOutlineWidth;
		[FoldoutGroup("Hover Settings"), ShowIf("hover"), ShowIf("genericHoverEffect"), ShowIf("hoverOutline"), Indent(2), SerializeField] private Color hoverOutlineColor = new Color(0,0,0,255);
		[FoldoutGroup("Hover Settings"), ShowIf("hover"), ShowIf("genericHoverEffect"), ShowIf("hoverOutline"), Indent(2), SerializeField] private Outline.Mode hoverOutlineMode;
		[FoldoutGroup("Hover Settings"), ShowIf("hover"), HideIf("genericHoverEffect"), Space(10), SerializeField] public UnityEvent hoverStart;
		[FoldoutGroup("Hover Settings"), ShowIf("hover"), HideIf("genericHoverEffect"), SerializeField] public UnityEvent hoverStay;
		[FoldoutGroup("Hover Settings"), ShowIf("hover"), HideIf("genericHoverEffect"), SerializeField] public UnityEvent hoverEnd;
		
		private static readonly int Threshold = Shader.PropertyToID("_ClipThreshold");
		internal const float BlendShapeActive = 0f;
		internal const float BlendShapeInactive = 100f;
		#endregion
		private void Start ()
		{
			InitialiseSelectableObject();
			Initialise();
		}
		protected virtual void Initialise()
		{
			
		}
		public void OnEnable()
		{
			InitialiseSelectableObject();
		}

		private void OnDestroy()
		{
			DestructSelectableObject();
		}

		public void OnDisable()
		{
			DestructSelectableObject();
		}

		private void DestructSelectableObject()
		{
			if (objectSelection == null) return;
			objectSelection.ResetObjects();
			GameObject g = gameObject;
			ToggleList(g, objectSelection.globalList, false);
			ToggleList(g, objectSelection.gazeList, false);
			ToggleList(g, objectSelection.lHandList, false);
			ToggleList(g, objectSelection.rHandList, false);
		}

		private void InitialiseSelectableObject()
		{
			SetupScaling();
			AssignComponents();
			SetupRigidBody();
			SetupManipulation();
			SetupOutline();
			SetupInteractionBehaviour(controllerTransforms.InteractionManager());
			ToggleList(gameObject, objectSelection.globalList, true);
			ToggleList(gameObject, objectSelection.gazeList, true);
			InitialisePostSetup();
			SetupButton();
			originalParent = transform.parent;
		}
		protected virtual void InitialisePostSetup()
		{
			
		}
		private void AssignComponents()
		{
			if (player == null)
			{
				foreach (GameObject rootGameObject in SceneManager.GetActiveScene().GetRootGameObjects())
				{
					if (rootGameObject.name != "[VR Player]") continue;
					player = rootGameObject;
					Debug.Log(name + " player set to " + rootGameObject.name);
				}
			}
			objectSelection = player.GetComponent<ObjectSelection>();
			controllerTransforms = player.GetComponent<ControllerTransforms>();
			manipulation = player.GetComponent<Manipulation>();
			locomotion = player.GetComponent<Locomotion>();
			Renderer = GetComponent<Renderer>();
			
			
		}
		private void SetupRigidBody()
		{
			if (!grab) return; // this seems like a band aid to be honest, figure out nested rigid bodies dude
			rb = transform.AddOrGetRigidbody();
			rb.freezeRotation = true;
			rb.useGravity = !button && gravity;
		}
		private void SetupManipulation()
		{
			if (freeRotationEnabled)
			{
				rotationLock = RotationLock.FREE_ROTATION;
			}
		}
		private void SetupOutline()
		{
			outline = transform.AddOrGetOutline();
			outline.enabled = false;
			outline.precomputeOutline = true;
		}
		private void SetupScaling()
		{
			defaultLocalScale = transform.localScale;
			scaleMin = defaultLocalScale.ScaledScale(minScaleFactor);
			scaleMax = defaultLocalScale.ScaledScale(maxScaleFactor);
		}

		private void SetupButton()
		{
			if(!button) return;
			toggleState = toggle && startsActive;
			if (toggleState)
			{
				toggleStart.Invoke();
			}
			else if (toggle && !startsActive)
			{
				toggleEnd.Invoke();
			}
			SetState(toggle && toggleState);
			active = startsActive;
			ResetState = transform;
			
			if(!blendShapeButton) return;
			SetupBlendShape();
		}
		private void SetupBlendShape()
		{
			hoverStart.AddListener(HoverBorderStart);
			hoverEnd.AddListener(HoverBorderEnd);

			if (buttonText != null)
			{
				textRenderer = text.GetComponent<MeshRenderer>();
				selectStart.AddListener(ButtonTextActive);
			}

			if (buttonSkinnedMeshRenderer != null)
			{
				selectStart.AddListener(SetBlendShapeActive);
			}
		}
		private void SetupInteractionBehaviour(InteractionManager interactionManager)
		{
			if (!controllerTransforms.leapMotionEnabled || !controllerTransforms.InteractionManager().enabled) return;
			interactionBehaviour = transform.AddOrGetInteractionBehavior();
			interactionBehaviour.manager = interactionManager;
			interactionBehaviour.ignoreGrasping = !grab;
		}
		private static void ToggleList(GameObject g, ICollection<GameObject> l, bool add)
		{
			switch (add)
			{
				case true when !l.Contains(g):
					l.Add(g);
					return;
				case false when l.Contains(g):
					l.Remove(g);
					return;
				default:
					return;
			}
		}
		private void Update()
		{					
			GetAngles();
			ReactiveMaterial();
			SetButtonStates();
			
			GameObject o = gameObject;
			o.CheckGaze(AngleG, objectSelection.gaze, objectSelection.gazeList, objectSelection.lHandList, objectSelection.rHandList, objectSelection.globalList);
			o.ManageList(objectSelection.lHandList, o.CheckHand(objectSelection.gazeList, objectSelection.manual, AngleL,manipulation.disableRightGrab, button), objectSelection.disableLeftHand, WithinRange(objectSelection.setSelectionRange, transform, objectSelection.Controller.LeftTransform(), objectSelection.selectionRange));
			o.ManageList(objectSelection.rHandList, o.CheckHand(objectSelection.gazeList, objectSelection.manual, AngleR,manipulation.disableLeftGrab, button), objectSelection.disableRightHand, WithinRange(objectSelection.setSelectionRange, transform, objectSelection.Controller.RightTransform(), objectSelection.selectionRange));
			
			ObjectUpdate();
		}

		private void SetButtonStates()
		{
			if (!blendShapeButton) return;
			switch (hoverAxis)
			{
				case HoverAxis.X:
					border.localPosition = new Vector3(buttonBorderDepth, 0, 0);
					break;
				case HoverAxis.Y:
					border.localPosition = new Vector3(0, buttonBorderDepth, 0);
					break;
				case HoverAxis.Z:
					border.localPosition = new Vector3(0, 0, buttonBorderDepth);
					break;
				default:
					throw new ArgumentOutOfRangeException();
			}
			if(buttonSkinnedMeshRenderer == null) return;
			buttonSkinnedMeshRenderer.SetBlendShapeWeight(0, buttonBlendShapeWeight);
		}

		protected virtual void ObjectUpdate()
		{
			
		}
		private void ReactiveMaterial()
		{
			if (!reactiveMat) return;
			
			Renderer.material.SetFloat(Threshold, clippingDistance);
			Renderer.ReactiveMaterial(objectSelection.Controller.LeftTransform(), objectSelection.Controller.RightTransform());
		}
		private void OnTriggerEnter(Collider col)
		{
			if(!directGrab) return;
			
			switch (col.gameObject.name)
			{
				case Manipulation.RTag when !objectSelection.RTouch && !objectSelection.Controller.RightGrab():
					objectSelection.RTouch = true;
					objectSelection.RLr.enabled = false;
					objectSelection.RFocusObject = gameObject;
					break;
				case Manipulation.LTag when !objectSelection.LTouch && !objectSelection.Controller.LeftGrab():
					objectSelection.LTouch = true;
					objectSelection.LLr.enabled = false;
					objectSelection.LFocusObject = gameObject;
					break;
				default:
					return;
			}
			
			if (!touchOutline) return;
			outline.Outline(touchOutlineMode, touchOutlineWidth, touchOutlineColor);
			outline.enabled = true;
		}
		private void OnTriggerStay(Collider col)
		{
			if(!directGrab) return;
			
			switch (col.gameObject.name)
			{
				case Manipulation.RTag when objectSelection.Controller.RightGrab() && !pGrabR && objectSelection.RFocusObject == gameObject:
					Manipulation.DirectGrabStart(rb, transform, manipulation.cR.transform);
					break;
				case Manipulation.RTag when !objectSelection.Controller.RightGrab() && pGrabR && objectSelection.RFocusObject == gameObject:
					Manipulation.DirectGrabEnd(rb, transform, originalParent, gravity, positions, rotations, moveForce, objectSelection.RLr);
					break;
				case Manipulation.LTag when objectSelection.Controller.LeftGrab() && !pGrabL && objectSelection.LFocusObject == gameObject:
					Manipulation.DirectGrabStart(rb, transform, manipulation.cL.transform);
					objectSelection.RTouch = false;
					objectSelection.RFocusObject = null;
					break;
				case Manipulation.LTag when !objectSelection.Controller.LeftGrab() && pGrabL && objectSelection.LFocusObject == gameObject:
					Manipulation.DirectGrabEnd(rb, transform, originalParent, gravity, positions, rotations, moveForce, objectSelection.LLr);
					objectSelection.LTouch = false;
					objectSelection.LFocusObject = null;
					break;
				default:
					positions.PositionTracking(transform.position, Sensitivity);
					rotations.RotationTracking(transform.forward, Sensitivity);
					break;
			}

			pGrabR = objectSelection.Controller.RightGrab();
			pGrabL = objectSelection.Controller.LeftGrab();
		}
		private void OnTriggerExit(Collider col)
		{
			if(!directGrab) return;
			
			switch (col.gameObject.name)
			{
				case Manipulation.RTag:
					objectSelection.RLr.enabled = true;
					objectSelection.RTouch = false;
					objectSelection.RFocusObject = null;
					break;
				case Manipulation.LTag:
					objectSelection.LLr.enabled = true;
					objectSelection.LTouch = false;
					objectSelection.LFocusObject = null;
					break;
				default:
					return;
			}
			
			outline.enabled = false;
		}
		private void GetAngles()
		{
			Vector3 position = transform.position;
			AngleG = Vector3.Angle(position - objectSelection.Controller.CameraPosition(), objectSelection.Controller.CameraForwardVector());
			AngleL = Vector3.Angle(position - objectSelection.Controller.LeftTransform().position, objectSelection.Controller.LeftForwardVector());
			AngleR = Vector3.Angle(position - objectSelection.Controller.RightTransform().position, objectSelection.Controller.RightForwardVector());
		}
		private static bool WithinRange(bool enabled, Transform self, Transform user, float range)
		{
			if (!enabled) return true;
			return Vector3.Distance(self.position, user.position) <= range;
		}
		public void SetState(bool state)
		{
			if (!genericSelectState) return;
			
			switch (state)
			{
				case true:
					transform.VisualState(this, defaultLocalScale.LocalScale(selectScale), defaultLocalPosition.LocalPosition(selectOffset), activeFont, activeColor);
					break;
				case false:
					transform.VisualState(this, defaultLocalScale, defaultLocalPosition, inactiveFont, inactiveColor);
					break;
			}
		}	
		public void GrabStart(Transform con)
		{
			if (!grab || rb == null) return;
			rb.RigidBody(moveForce, latency,false, gravity);
			manipulation.OnStart(con);
			
			if(!grabOutline) return;
			outline.Outline(grabOutlineMode, grabOutlineWidth, grabOutlineColor);
			outline.enabled = true;
		}	
		public void GrabStay(Transform con)
		{
			if (!grab || rb == null) return;
			
			manipulation.OnStay(con);
			
			switch (manipulation.manipulationType)
			{
				case Manipulation.ManipulationType.Lerp:
					if (DualGrab())
					{
						transform.TransformLerpPosition(manipulation.mP.transform, .1f);
						break;
					}
					if (objectSelection.Controller.RightGrab() && objectSelection.RSelectableObject == this)
					{
						transform.TransformLerpPosition(manipulation.tSr.transform, .1f);
						break;
					}
					if (objectSelection.Controller.LeftGrab() && objectSelection.LSelectableObject == this)
					{
						transform.TransformLerpPosition(manipulation.tSl.transform, .1f);
					}
					break;
				case Manipulation.ManipulationType.Physics:
					if (DualGrab() && !pDualGrab)
					{
						manipulation.DualGrabStart(transform, freeRotationEnabled, scalingEnabled, maxScaleFactor, minScaleFactor, scaleMax, scaleMin);
					}
					if (DualGrab() && pDualGrab)
					{
						rb.AddForcePosition(transform, manipulation.mP.transform, objectSelection.Controller.debugActive);
						manipulation.DualGrabStay(rb, transform, freeRotationEnabled, scalingEnabled, scaleMin, scaleMax);
						break;
					}
					if (!DualGrab() && pDualGrab)
					{
						defaultLocalScale = transform.localScale;
						manipulation.DualGrabEnd();
					}
					if (objectSelection.Controller.RightGrab() && objectSelection.RSelectableObject == this)
					{
						rb.AddForcePosition(transform, manipulation.tSr.transform, objectSelection.Controller.debugActive);
						break;
					}
					if (objectSelection.Controller.LeftGrab() && objectSelection.LSelectableObject == this)
					{
						rb.AddForcePosition(transform, manipulation.tSl.transform, objectSelection.Controller.debugActive);
					}
					break;
				default:
					throw new ArgumentException();
			}
			pDualGrab = DualGrab();
		}
		private bool DualGrab()
		{
			return objectSelection.Controller.LeftGrab() && objectSelection.Controller.RightGrab() && objectSelection.LSelectableObject == this && objectSelection.RSelectableObject == this;
		}
		public void GrabEnd(Transform con)
		{
			if (!grab || rb == null) return;
			
			objectSelection.gazeList.Clear();
			
			rb.RigidBody(moveForce, latency,false, gravity);
			outline.enabled = false;
			
			manipulation.OnEnd(con);
		}

		public void HoverStart(Tooltip tooltip)
		{
			if(!hover) return;
			hoverStart.Invoke();
			if (toolTip)
			{
				tooltip.SetTooltipText(objectSelection.toolTips, toolTipText);
			}
			if (!genericHoverEffect) return;
			if (hoverOutline)
			{
				outline.Outline(hoverOutlineMode, hoverOutlineWidth, hoverOutlineColor);
				outline.enabled = true;
			}
			
			Transform t = transform;
			defaultLocalScale = t.localScale;
			defaultLocalPosition = t.localPosition;
			t.DOScale(defaultLocalScale.LocalScale(hoverScale), hoverEffectDuration);
			if (rb.velocity != Vector3.zero || hoverOffset <= 0) return;
			t.DOLocalMove(defaultLocalPosition.LocalPosition(hoverOffset), hoverEffectDuration);
		}
		public void HoverStay()
		{
			if(!hover) return;
			hoverStay.Invoke();
		}
		public void HoverEnd(Tooltip tooltip)
		{
			if(!hover) return;
			hoverEnd.Invoke();
			
			if (toolTip)
			{
				tooltip.ClearTooltipText();
			}
			
			outline.enabled = false;
			if (!genericHoverEffect) return;
			
			Transform t = transform;
			
			t.DOScale(defaultLocalScale, hoverEffectDuration);
			
			if (rb.velocity != Vector3.zero || hoverOffset <= 0) return;
			t.DOLocalMove(defaultLocalPosition, hoverEffectDuration);
		}
		public void SelectStart()
		{
			selectStart.Invoke();
			switch (toggleState)
			{
				case true:
					toggleState = false;
					toggleEnd.Invoke();
					break;
				default:
					toggleState = true;
					toggleStart.Invoke();
					break;
			}
			if (genericSelectState && button)
			{
				switch (buttonTrigger)
				{
					case ButtonTrigger.ON_BUTTON_DOWN:
						active = !active;
						SetState(active);
						break;
					case ButtonTrigger.ON_BUTTON_UP:
						break;
					default:
						throw new ArgumentException();
				}
			}
		}
		public void SelectStay()
		{
			selectStay.Invoke();
		}
		public void SelectEnd()
		{
			selectEnd.Invoke();
			
			if (!genericSelectState || !button) return;
			switch (buttonTrigger)
			{
				case ButtonTrigger.ON_BUTTON_DOWN:
					break;
				case ButtonTrigger.ON_BUTTON_UP:
					active = !active;
					SetState(active);
					break;
				default:
					throw new ArgumentException();
			}
		}
		
		private void HoverBorderStart()
		{
			switch (hoverAxis)
			{
				case HoverAxis.X:
					buttonBorderDepth = border.localPosition.x;
					break;
				case HoverAxis.Y:
					buttonBorderDepth = border.localPosition.y;
					break;
				case HoverAxis.Z:
					buttonBorderDepth = border.localPosition.z;
					break;
				default:
					throw new ArgumentOutOfRangeException();
			}
			DOTween.To(()=> buttonBorderDepth, x=> buttonBorderDepth = x, hoverDepth, hoverDuration);
		}

		private void HoverBorderEnd()
		{
			switch (hoverAxis)
			{
				case HoverAxis.X:
					buttonBorderDepth = border.localPosition.x;
					break;
				case HoverAxis.Y:
					buttonBorderDepth = border.localPosition.y;
					break;
				case HoverAxis.Z:
					buttonBorderDepth = border.localPosition.z;
					break;
				default:
					throw new ArgumentOutOfRangeException();
			}
			DOTween.To(()=> buttonBorderDepth, x=> buttonBorderDepth = x, restDepth, hoverDuration);
		}
		
		private void SetBlendShapeActive()
		{
			buttonBlendShapeWeight = buttonSkinnedMeshRenderer.GetBlendShapeWeight(0);
			DOTween.To(()=> buttonBlendShapeWeight, x=> buttonBlendShapeWeight = x, BlendShapeActive, buttonAnimationDuration);
			if (toggle) return;
			StartCoroutine(ResetButton());
		}

		private IEnumerator ResetButton()
		{
			yield return new WaitForSeconds(buttonAnimationDuration + (buttonAnimationDuration * .25f));
			SetBlendShapeInactive();
			ButtonTextInactive();
		}
		private void SetBlendShapeInactive()
		{
			buttonBlendShapeWeight = buttonSkinnedMeshRenderer.GetBlendShapeWeight(0);
			DOTween.To(()=> buttonBlendShapeWeight, x=> buttonBlendShapeWeight = x, BlendShapeInactive, buttonAnimationDuration);
		}
		
		private void ButtonTextActive()
		{
			textRenderer.material.color = activeFontColor;
		}
        
		private void ButtonTextInactive()
		{
			textRenderer.material.color = inactiveFontColor;
		}
	}
}
