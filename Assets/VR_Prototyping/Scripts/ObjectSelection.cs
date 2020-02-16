using System;
using System.Collections.Generic;
using System.Xml.Serialization;
using Sirenix.OdinInspector;
using TMPro;
using UnityEngine;
using VR_Prototyping.Scripts.Accessibility;
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
		
		private SelectableObject pLSelectableObject;
		private SelectableObject pRSelectableObject;
		private SelectableObject pGSelectableObject;
		
		private GameObject lTooltipObject;
		private GameObject rTooltipObject;
		
		private Tooltip lTooltip;
		private Tooltip rTooltip;

		private bool initialised;

		public bool RTouch { get; set; }
		public bool LTouch { get; set; }
		public GameObject LMidPoint { get; private set; }
		public GameObject RMidPoint { get; private set; }
		public GameObject LFocusObject { get; set; }
		public GameObject RFocusObject { get; set; }
		public GameObject GFocusObject { get; set; }
		public SelectableObject RSelectableObject { get; set; }
		public SelectableObject LSelectableObject { get; set; }
		public SelectableObject GSelectableObject { get; set; }
		public SelectableObject RPreviousSelectableObject { get; set; }
		public SelectableObject LPreviousSelectableObject { get; set; }
		public SelectableObject GPreviousSelectableObject { get; set; }
		public LineRenderer LLr { get; private set; }
		public LineRenderer RLr { get; private set; }
		public bool DisableSelection { get; set; }
		public bool LSelectPrevious { get; set; }
		public bool RSelectPrevious { get; set; }
		public bool LGrabPrevious { get; set; }
		public bool RGrabPrevious { get; set; }

		private Transform lPrevious;
		private Transform rPrevious;
		
		[ValidateInput("TypeCheck", "Recommended Selection Type is Fusion", InfoMessageType.Warning)]
		[BoxGroup("Selection Settings"), SerializeField] private SelectionType selectionType;
		[BoxGroup("Selection Settings"), HideIf("selectionType", SelectionType.RAY_CAST), Indent, Range(0f, 180f)] public float gaze = 60f;
		[BoxGroup("Selection Settings"), HideIf("selectionType", SelectionType.RAY_CAST), Indent, Range(0f, 180f)] public float manual = 25f;
		[BoxGroup("Selection Settings"), Space(10)] public bool setSelectionRange;		
		[BoxGroup("Selection Settings"), ShowIf("setSelectionRange"), Indent, Range(0f, 250f)] public float selectionRange = 25f;		
		[BoxGroup("Selection Settings")] public bool disableLeftHand;
		[BoxGroup("Selection Settings")] public bool disableRightHand;
		
		[BoxGroup("Aesthetics")] public GameObject gazeCursor;
		[BoxGroup("Aesthetics"), Range(3f, 30f), Space(10)] public int lineRenderQuality = 15;
		[BoxGroup("Aesthetics"), Range(.1f, 2.5f), Indent] public float inactiveLineRenderOffset = 1f;
		
		[BoxGroup("Object Lists"), HideInEditorMode] public List<GameObject> globalList;
		[BoxGroup("Object Lists"), HideInEditorMode] public List<GameObject> gazeList;
		[BoxGroup("Object Lists"), HideInEditorMode] public List<GameObject> rHandList;
		[BoxGroup("Object Lists"), HideInEditorMode] public List<GameObject> lHandList;

		[BoxGroup("Accessibility Settings")] public bool toolTips;
		[BoxGroup("Accessibility Settings"), ShowIf("toolTips"), Indent, SerializeField, Required] private GameObject toolTipPrefab;
		[BoxGroup("Accessibility Settings"), ShowIf("toolTips"), Indent, SerializeField, Range(.01f, .2f)] private float toolTipOffset;

		#endregion
		private void Start ()
		{
			Controller = GetComponent<ControllerTransforms>();
			
			SetupGameObjects();
			
			LLr = Controller.LeftTransform().gameObject.AddComponent<LineRenderer>();
			RLr = Controller.RightTransform().gameObject.AddComponent<LineRenderer>();
			
			LLr.SetupLineRender(Controller.lineRenderMat, .005f, true);
			RLr.SetupLineRender(Controller.lineRenderMat, .005f, true);
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
				
				lTooltipObject = Instantiate(toolTipPrefab);
				rTooltipObject = Instantiate(toolTipPrefab);

				gazeCursor = Instantiate(gazeCursor, gTarget.transform);
			}

			lDefault.transform.SetOffsetPosition(Controller.LeftTransform(), inactiveLineRenderOffset);
			rDefault.transform.SetOffsetPosition(Controller.RightTransform(), inactiveLineRenderOffset);
			
			LMidPoint.transform.SetOffsetPosition(Controller.LeftTransform(), 0f);
			RMidPoint.transform.SetOffsetPosition(Controller.RightTransform(), 0f);

			lTooltipObject.transform.SetOffsetPosition(Controller.LeftTransform(), toolTipOffset);
			rTooltipObject.transform.SetOffsetPosition(Controller.RightTransform(), toolTipOffset);

			lTooltip = lTooltipObject.GetComponent<Tooltip>();
			rTooltip = rTooltipObject.GetComponent<Tooltip>();

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
			
			LFocusObject.Manipulation(RFocusObject, LSelectableObject, pLSelectableObject, Controller.LeftGrab(), LGrabPrevious, Controller.LeftTransform(), LTouch, RTouch);
			RFocusObject.Manipulation(LFocusObject, RSelectableObject, pRSelectableObject, Controller.RightGrab(), RGrabPrevious, Controller.RightTransform(), RTouch, LTouch);
			
			LSelectableObject = LFocusObject.FindSelectableObject(LSelectableObject, Controller.LeftGrab());
			RSelectableObject = RFocusObject.FindSelectableObject(RSelectableObject, Controller.RightGrab());
			GSelectableObject = GFocusObject.FindSelectableObject(GSelectableObject, false);
			
			LGrabPrevious = Controller.LeftGrab();
			RGrabPrevious = Controller.RightGrab();

			if (LPreviousSelectableObject != null && LSelectableObject != LPreviousSelectableObject)
			{
				LPreviousSelectableObject.HoverEnd(lTooltip);
			}
			if (RPreviousSelectableObject != null && RSelectableObject != RPreviousSelectableObject)
			{
				RPreviousSelectableObject.HoverEnd(rTooltip);
			}
			if (GPreviousSelectableObject != null && GSelectableObject != GPreviousSelectableObject)
			{
				GPreviousSelectableObject.HoverEnd(null);
			}

			LPreviousSelectableObject = LSelectableObject;
			RPreviousSelectableObject = RSelectableObject;
			GPreviousSelectableObject = GSelectableObject;
			
			ResetGameObjects(Controller.LeftTransform(), lPrevious);
			ResetGameObjects(Controller.RightTransform(), rPrevious);

			lPrevious = Controller.LeftTransform();
			rPrevious = Controller.RightTransform();
		}

		private void LateUpdate()
		{
			LFocusObject.Selection(LSelectableObject, Controller.LeftSelect(), LSelectPrevious);
			RFocusObject.Selection(RSelectableObject, Controller.RightSelect(), RSelectPrevious);
			
			LSelectableObject.Hover(pLSelectableObject, lTooltip);
			RSelectableObject.Hover(pRSelectableObject, rTooltip);
			GSelectableObject.Hover(pGSelectableObject, rTooltip);
		
			LSelectPrevious = Controller.LeftSelect();
			RSelectPrevious = Controller.RightSelect();

			pLSelectableObject = LSelectableObject;
			pRSelectableObject = RSelectableObject;
			pGSelectableObject = GSelectableObject;
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
