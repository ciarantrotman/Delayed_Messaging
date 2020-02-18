using System.Collections.Generic;
using UnityEngine;
using VR_Prototyping.Scripts.Accessibility;
using VR_Prototyping.Scripts.Utilities;
using Object = UnityEngine.Object;

namespace VR_Prototyping.Scripts
{
	[DisallowMultipleComponent]
	[RequireComponent(typeof(ControllerTransforms))]
	public class ObjectSelection : MonoBehaviour
	{
		#region 01 Inspector and Variables
		public ControllerTransforms Controller { get; private set; }
		private enum SelectionType
		{
			FUSION,
			FUZZY,
			RAY_CAST
		}
		private static bool TypeCheck(SelectionType type)
		{
			return type == SelectionType.FUSION;
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

		public bool RTouch { get; set; }
		public bool LTouch { get; set; }
		public GameObject LMidPoint { get; private set; }
		public GameObject RMidPoint { get; private set; }
		public GameObject LFocusObject { get; set; }
		public GameObject RFocusObject { get; set; }
		public GameObject GFocusObject { get; set; }
		public BaseObject RBaseObject { get; set; }
		public BaseObject LBaseObject { get; set; }
		public BaseObject GBaseObject { get; set; }
		public BaseObject RPreviousBaseObject { get; set; }
		public BaseObject LPreviousBaseObject { get; set; }
		public BaseObject GPreviousBaseObject { get; set; }
		public LineRenderer LLr { get; private set; }
		public LineRenderer RLr { get; private set; }
		public bool DisableSelection { get; set; }
		public bool LSelectPrevious { get; set; }
		public bool RSelectPrevious { get; set; }
		public bool LGrabPrevious { get; set; }
		public bool RGrabPrevious { get; set; }

		private Transform lPrevious;
		private Transform rPrevious;
		
		[SerializeField] private SelectionType selectionType;
		[Range(0f, 180f)] public float gaze = 60f;
		[Range(0f, 180f)] public float manual = 25f;
		[Space(10)] public bool setSelectionRange;		
		[Range(0f, 250f)] public float selectionRange = 25f;		
		public bool disableLeftHand;
		public bool disableRightHand;
		
		public GameObject gazeCursor;
		[Range(3f, 30f), Space(10)] public int lineRenderQuality = 15;
		[Range(.1f, 2.5f)] public float inactiveLineRenderOffset = 1f;
		
		[HideInInspector] public List<GameObject> globalList;
		[HideInInspector] public List<GameObject> gazeList;
		[HideInInspector] public List<GameObject> rHandList;
		[HideInInspector] public List<GameObject> lHandList;

		#endregion
		private void Start ()
		{
			Controller = GetComponent<ControllerTransforms>();
			
			SetupGameObjects();
			
			LLr = Controller.LeftTransform().gameObject.AddComponent<LineRenderer>();
			RLr = Controller.RightTransform().gameObject.AddComponent<LineRenderer>();
			
			LLr.SetupLineRender(Controller.lineRenderMaterial, .005f, true);
			RLr.SetupLineRender(Controller.lineRenderMaterial, .005f, true);
		}
		private void SetupGameObjects()
		{
			if (!initialised)
			{
				LMidPoint = Set.NewGameObject(Controller.LeftTransform().gameObject, "MidPoint/Left");
				RMidPoint = Set.NewGameObject(Controller.RightTransform().gameObject, "MidPoint/Right");

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
			
			LMidPoint.transform.SetOffsetPosition(Controller.LeftTransform(), 0f);
			RMidPoint.transform.SetOffsetPosition(Controller.RightTransform(), 0f);

			initialised = true;
		}

		private void FixedUpdate()
		{
			SortLists();

			switch (selectionType)
			{
				case SelectionType.FUZZY:
					LFocusObject = lHandList.FuzzyFindFocusObject(LFocusObject, lTarget, lDefault, Controller.LeftGrab() || LTouch);
					RFocusObject = rHandList.FuzzyFindFocusObject(RFocusObject, rTarget, rDefault, Controller.RightGrab() || RTouch);
					break;
				case SelectionType.RAY_CAST:
					LFocusObject = lHandList.RayCastFindFocusObject(LFocusObject, lTarget, lDefault, Controller.LeftTransform(), setSelectionRange ? selectionRange : float.PositiveInfinity, Controller.LeftGrab() || LTouch);
					RFocusObject = rHandList.RayCastFindFocusObject(RFocusObject, rTarget, rDefault, Controller.RightTransform(), setSelectionRange ? selectionRange : float.PositiveInfinity, Controller.RightGrab() || RTouch);
					break;
				case SelectionType.FUSION:
					LFocusObject = lHandList.FusionFindFocusObject(LFocusObject, lTarget, lDefault, Controller.LeftTransform(), Controller.LeftForwardVector(), setSelectionRange ? selectionRange : float.PositiveInfinity, Controller.LeftGrab() || LTouch);
					RFocusObject = rHandList.FusionFindFocusObject(RFocusObject, rTarget, rDefault, Controller.RightTransform(), Controller.RightForwardVector(), setSelectionRange ? selectionRange : float.PositiveInfinity, Controller.RightGrab() || RTouch);
					GFocusObject = gazeList.FusionFindFocusObject(GFocusObject, gTarget, gDefault, Controller.CameraTransform(), Controller.CameraForwardVector(), setSelectionRange ? selectionRange : float.PositiveInfinity, false);
					break;
				default:
					LFocusObject = null;
					RFocusObject = null;
					break;
			}
			
			LLr.DrawLineRenderer(LFocusObject, LMidPoint, Controller.LeftTransform(), lTarget, lineRenderQuality);
			RLr.DrawLineRenderer(RFocusObject, RMidPoint, Controller.RightTransform() ,rTarget, lineRenderQuality);
			
			LFocusObject.Manipulation(RFocusObject, LBaseObject, pLBaseObject, Controller.LeftGrab(), LGrabPrevious, Controller.LeftTransform(), LTouch, RTouch);
			RFocusObject.Manipulation(LFocusObject, RBaseObject, pRBaseObject, Controller.RightGrab(), RGrabPrevious, Controller.RightTransform(), RTouch, LTouch);
			
			LBaseObject = LFocusObject.FindSelectableObject(LBaseObject, Controller.LeftGrab());
			RBaseObject = RFocusObject.FindSelectableObject(RBaseObject, Controller.RightGrab());
			GBaseObject = GFocusObject.FindSelectableObject(GBaseObject, false);
			
			LGrabPrevious = Controller.LeftGrab();
			RGrabPrevious = Controller.RightGrab();

			LPreviousBaseObject = LBaseObject;
			RPreviousBaseObject = RBaseObject;
			GPreviousBaseObject = GBaseObject;
			
			ResetGameObjects(Controller.LeftTransform(), lPrevious);
			ResetGameObjects(Controller.RightTransform(), rPrevious);

			lPrevious = Controller.LeftTransform();
			rPrevious = Controller.RightTransform();
		}

		private void LateUpdate()
		{
			LFocusObject.Selection(LBaseObject, Controller.LeftSelect(), LSelectPrevious);
			RFocusObject.Selection(RBaseObject, Controller.RightSelect(), RSelectPrevious);
			
			LBaseObject.Hover(pLBaseObject, RBaseObject);
			RBaseObject.Hover(pRBaseObject, LBaseObject);

			LSelectPrevious = Controller.LeftSelect();
			RSelectPrevious = Controller.RightSelect();

			pLBaseObject = LBaseObject;
			pRBaseObject = RBaseObject;
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