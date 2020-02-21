using System.Collections.Generic;
using Delayed_Messaging.Scripts.Utilities;
using UnityEngine;
using UnityEngine.Events;
using VR_Prototyping.Scripts.Utilities;
using Object = UnityEngine.Object;

namespace VR_Prototyping.Scripts
{
	[DisallowMultipleComponent]
	[RequireComponent(typeof(ControllerTransforms))]
	public class Selection : MonoBehaviour
	{
		#region 01 Inspector and Variables
		public ControllerTransforms Controller { get; private set; }
		public enum SelectionType
		{
			FUSION,
			FUZZY,
			RAY_CAST
		}
		private GameObject lTarget; // left hand target
		private GameObject rTarget; // right hand target
		private GameObject gTarget; // gaze target
		
		private GameObject lDefault;
		private GameObject rDefault;
		private GameObject gDefault;
		
		private BaseObject pLBaseObject;
		private BaseObject pRBaseObject;
		private BaseObject pGBaseObject;

		private bool initialised;

		public GameObject LFocusObject { get; set; }
		public GameObject RFocusObject { get; set; }
		public bool DisableSelection { get; set; }
		
		private bool rTouch;
		private bool lTouch;
		private bool lSelectPrevious;
		private bool rSelectPrevious;
		private bool lGrabPrevious;
		private bool rGrabPrevious;
		
		public List<bool> lSelect = new List<bool> {Capacity = (int) QuickSelectSensitivity};
		public List<bool> rSelect = new List<bool> {Capacity = (int) QuickSelectSensitivity};
		
		private GameObject lMidPoint;
		private GameObject rMidPoint;
		private GameObject gFocusObject;
		private BaseObject rBaseObject;
		private BaseObject lBaseObject;
		private BaseObject gBaseObject;
		private BaseObject rPreviousBaseObject;
		private BaseObject lPreviousBaseObject;
		private BaseObject gPreviousBaseObject;
		private LineRenderer lLineRenderer;
		private LineRenderer rLineRenderer;
		private Material lLineRendererMaterial;
		private Material rLineRendererMaterial;
		private static readonly int Distance = Shader.PropertyToID("_Distance");

		private Transform lPrevious;
		private Transform rPrevious;
		
		private const float QuickSelectSensitivity = 20f;
		
		[Header("Selection Settings")]
		[SerializeField] private SelectionType selectionType;
		[Range(0f, 180f)] public float gaze = 60f;
		[Range(0f, 180f)] public float manual = 25f;
		[Space(10)] public bool setSelectionRange;		
		[Range(0f, 250f)] public float selectionRange = 25f;
		public bool disableLeftHand;
		public bool disableRightHand;

		[Header("Selection Aesthetics")]
		[Space(10)] public GameObject gazeCursor;
		[Range(3f, 30f), Space(10)] public int lineRenderQuality = 15;
		[Range(.1f, 2.5f)] public float inactiveLineRenderOffset = 1f;
		
		[HideInInspector] public List<GameObject> globalList;
		[HideInInspector] public List<GameObject> gazeList;
		[HideInInspector] public List<GameObject> rHandList;
		[HideInInspector] public List<GameObject> lHandList;
		
		/// <summary>
		/// Called when the user performs a quick select and there is no focus object
		/// </summary>
		[HideInInspector] public UnityEvent quickSelect;

		public Selection(bool rTouch, bool lTouch)
		{
			this.rTouch = rTouch;
			this.lTouch = lTouch;
		}

		#endregion
		private void Start ()
		{
			Controller = GetComponent<ControllerTransforms>();
			
			SetupGameObjects();
			
			lLineRenderer = Controller.LeftTransform().gameObject.AddComponent<LineRenderer>();
			rLineRenderer = Controller.RightTransform().gameObject.AddComponent<LineRenderer>();
			
			lLineRenderer.SetupLineRender(Controller.lineRenderMaterial, .005f, true);
			rLineRenderer.SetupLineRender(Controller.lineRenderMaterial, .005f, true);
			
			rLineRendererMaterial = rLineRenderer.material;
			lLineRendererMaterial = lLineRenderer.material;
		}
		private void SetupGameObjects()
		{
			if (!initialised)
			{
				lMidPoint = Set.NewGameObject(Controller.LeftTransform().gameObject, "MidPoint/Left");
				rMidPoint = Set.NewGameObject(Controller.RightTransform().gameObject, "MidPoint/Right");

				lTarget = Set.NewGameObject(gameObject, "[Target Left]");
				rTarget = Set.NewGameObject(gameObject, "[Target Right]");
				gTarget = Set.NewGameObject(gameObject, "[Target Gaze]");

				lDefault = Set.NewGameObject(Controller.LeftTransform().gameObject, "Target/LineRender/Left/Default");
				rDefault = Set.NewGameObject(Controller.RightTransform().gameObject, "Target/LineRender/Right/Default");
				gDefault = Set.NewGameObject(Controller.RightTransform().gameObject, "Target/LineRender/Right/Default");

				gazeCursor = Instantiate(gazeCursor, gTarget.transform);
			}

			lDefault.transform.SetOffsetPosition(Controller.LeftTransform(), inactiveLineRenderOffset);
			rDefault.transform.SetOffsetPosition(Controller.RightTransform(), inactiveLineRenderOffset);
			
			lMidPoint.transform.SetOffsetPosition(Controller.LeftTransform(), 0f);
			rMidPoint.transform.SetOffsetPosition(Controller.RightTransform(), 0f);

			initialised = true;
		}

		private void FixedUpdate()
		{
			SortLists();

			switch (selectionType)
			{
				case SelectionType.FUZZY:
					LFocusObject = lHandList.FuzzyFindFocusObject(LFocusObject, lTarget, lDefault, Controller.LeftGrab() || lTouch);
					RFocusObject = rHandList.FuzzyFindFocusObject(RFocusObject, rTarget, rDefault, Controller.RightGrab() || rTouch);
					break;
				case SelectionType.RAY_CAST:
					LFocusObject = lHandList.RayCastFindFocusObject(LFocusObject, lTarget, lDefault, Controller.LeftTransform(), setSelectionRange ? selectionRange : float.PositiveInfinity, Controller.LeftGrab() || lTouch);
					RFocusObject = rHandList.RayCastFindFocusObject(RFocusObject, rTarget, rDefault, Controller.RightTransform(), setSelectionRange ? selectionRange : float.PositiveInfinity, Controller.RightGrab() || rTouch);
					break;
				case SelectionType.FUSION:
					LFocusObject = lHandList.FusionFindFocusObject(LFocusObject, lTarget, lDefault, Controller.LeftTransform(), Controller.LeftForwardVector(), setSelectionRange ? selectionRange : float.PositiveInfinity, Controller.LeftGrab() || lTouch);
					RFocusObject = rHandList.FusionFindFocusObject(RFocusObject, rTarget, rDefault, Controller.RightTransform(), Controller.RightForwardVector(), setSelectionRange ? selectionRange : float.PositiveInfinity, Controller.RightGrab() || rTouch);
					gFocusObject = gazeList.FusionFindFocusObject(gFocusObject, gTarget, gDefault, Controller.CameraTransform(), Controller.CameraForwardVector(), setSelectionRange ? selectionRange : float.PositiveInfinity, false);
					break;
				default:
					LFocusObject = null;
					RFocusObject = null;
					break;
			}
			
			lLineRenderer.DrawLineRenderer(LFocusObject, lMidPoint, Controller.LeftTransform(), lTarget, lineRenderQuality);
			rLineRenderer.DrawLineRenderer(RFocusObject, rMidPoint, Controller.RightTransform() ,rTarget, lineRenderQuality);
			
			lLineRendererMaterial.SetFloat(Distance, transform.TransformDistance(lTarget.transform));
			rLineRendererMaterial.SetFloat(Distance, transform.TransformDistance(rTarget.transform));
			
			LFocusObject.Manipulation(RFocusObject, lBaseObject, pLBaseObject, Controller.LeftGrab(), lGrabPrevious, Controller.LeftTransform(), lTouch, rTouch);
			RFocusObject.Manipulation(LFocusObject, rBaseObject, pRBaseObject, Controller.RightGrab(), rGrabPrevious, Controller.RightTransform(), rTouch, lTouch);
			
			lBaseObject = LFocusObject.FindSelectableObject(lBaseObject, Controller.LeftGrab());
			rBaseObject = RFocusObject.FindSelectableObject(rBaseObject, Controller.RightGrab());
			gBaseObject = gFocusObject.FindSelectableObject(gBaseObject, false);
			
			lGrabPrevious = Controller.LeftGrab();
			rGrabPrevious = Controller.RightGrab();

			lPreviousBaseObject = lBaseObject;
			rPreviousBaseObject = rBaseObject;
			gPreviousBaseObject = gBaseObject;
			
			ResetGameObjects(Controller.LeftTransform(), lPrevious);
			ResetGameObjects(Controller.RightTransform(), rPrevious);

			lPrevious = Controller.LeftTransform();
			rPrevious = Controller.RightTransform();
		}

		private void LateUpdate()
		{
			// Calculate Hover States
			lBaseObject.Hover(pLBaseObject, rBaseObject);
			rBaseObject.Hover(pRBaseObject, lBaseObject);
			
			// Calculate Selection States
			lBaseObject.Selection(this, Controller.LeftSelect(), lSelectPrevious, lSelect, QuickSelectSensitivity);
			rBaseObject.Selection(this, Controller.RightSelect(), rSelectPrevious, rSelect, QuickSelectSensitivity);

			// Previous States
			lSelectPrevious = Controller.LeftSelect();
			rSelectPrevious = Controller.RightSelect();
			
			pLBaseObject = lBaseObject;
			pRBaseObject = rBaseObject;
		}

		private void SortLists()
		{
			lHandList.Sort(SortBy.FocusObjectL);
			rHandList.Sort(SortBy.FocusObjectR);
			gazeList.Sort(SortBy.FocusObjectG);
		}

		private void ResetGameObjects(Object current, Object previous)
		{
			if (current == previous) return;
			SetupGameObjects();
		}

		public void ResetObjects()
		{
			if (lTarget == null || rTarget == null) return;
			lTarget.transform.SetParent(transform);
			rTarget.transform.SetParent(transform);
			lTarget.transform.SetParent(null);
			rTarget.transform.SetParent(null);
		}
	}
}