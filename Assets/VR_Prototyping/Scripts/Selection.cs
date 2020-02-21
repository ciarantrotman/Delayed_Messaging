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
		#region Inspector and Variables

		private ControllerTransforms controllerTransforms;
		public enum SelectionType
		{
			FUSION,
			FUZZY,
			RAY_CAST,
			DISTANCE_CAST
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

		private bool rTouch;
		private bool lTouch;
		private bool lSelectPrevious;
		private bool rSelectPrevious;
		private bool lGrabPrevious;
		private bool rGrabPrevious;
		
		[HideInInspector] public List<bool> lSelect = new List<bool> {Capacity = (int) QuickSelectSensitivity};
		[HideInInspector] public List<bool> rSelect = new List<bool> {Capacity = (int) QuickSelectSensitivity};
		
		private GameObject lMidPoint;
		private GameObject rMidPoint;
		private GameObject gFocusObject;
		private BaseObject rBaseObject;
		private BaseObject lBaseObject;
		private BaseObject gBaseObject;
		private LineRenderer lLineRenderer;
		private LineRenderer rLineRenderer;
		private Material lLineRendererMaterial;
		private Material rLineRendererMaterial;
		private static readonly int Distance = Shader.PropertyToID("_Distance");

		private Transform lPrevious;
		private Transform rPrevious;

		#region Distance Cast Variables
		private GameObject parent;
		
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
        #endregion
		
		private const float QuickSelectSensitivity = 20f;
		
		[Header("Selection Settings")]
		[SerializeField] private SelectionType selectionType;
		[Range(0f, 180f), Space(10)] public float gaze = 60f;
		[Range(0f, 180f)] public float manual = 25f;
		[Space(10)] public bool setSelectionRange;		
		[Range(0f, 250f)] public float selectionRange = 25f;
		[SerializeField, Space(10)] private float castSelectionRadius;
		[SerializeField, Range(1, 15)] private int layerIndex = 10;
		[SerializeField] private GameObject targetVisual;
		[Space(10)]public bool disableLeftHand;
		public bool disableRightHand;

		[Header("Selection Aesthetics")]
		[SerializeField] private Material lineRenderMat;
		[SerializeField, Range(.001f, .1f)] private float lineRenderWidth = .005f;
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
			controllerTransforms = GetComponent<ControllerTransforms>();
			
			SetupGameObjects();
			SetupCastSelectGameObjects();

			lLineRenderer = controllerTransforms.LeftTransform().gameObject.AddComponent<LineRenderer>();
			rLineRenderer = controllerTransforms.RightTransform().gameObject.AddComponent<LineRenderer>();
			
			lLineRenderer.SetupLineRender(lineRenderMat, lineRenderWidth, true);
			rLineRenderer.SetupLineRender(lineRenderMat, lineRenderWidth, true);
			
			rLineRendererMaterial = rLineRenderer.material;
			lLineRendererMaterial = lLineRenderer.material;
		}
		private void SetupGameObjects()
		{
			if (!initialised)
			{
				lMidPoint = Set.NewGameObject(controllerTransforms.LeftTransform().gameObject, "MidPoint/Left");
				rMidPoint = Set.NewGameObject(controllerTransforms.RightTransform().gameObject, "MidPoint/Right");

				lTarget = Set.NewGameObject(gameObject, "[Target Left]");
				rTarget = Set.NewGameObject(gameObject, "[Target Right]");
				gTarget = Set.NewGameObject(gameObject, "[Target Gaze]");

				lDefault = Set.NewGameObject(controllerTransforms.LeftTransform().gameObject, "Target/LineRender/Left/Default");
				rDefault = Set.NewGameObject(controllerTransforms.RightTransform().gameObject, "Target/LineRender/Right/Default");
				gDefault = Set.NewGameObject(controllerTransforms.RightTransform().gameObject, "Target/LineRender/Right/Default");

				gazeCursor = Instantiate(gazeCursor, gTarget.transform);
			}

			lDefault.transform.SetOffsetPosition(controllerTransforms.LeftTransform(), inactiveLineRenderOffset);
			rDefault.transform.SetOffsetPosition(controllerTransforms.RightTransform(), inactiveLineRenderOffset);
			
			lMidPoint.transform.SetOffsetPosition(controllerTransforms.LeftTransform(), 0f);
			rMidPoint.transform.SetOffsetPosition(controllerTransforms.RightTransform(), 0f);

			initialised = true;
		}
		
		private void SetupCastSelectGameObjects()
        {
            parent = new GameObject("[Cast Selection/Calculations]");
            Transform parentTransform = parent.transform;
            parentTransform.SetParent(transform);

            rCf = new GameObject("[Cast Selection/Follow/Right]");
            rCp = new GameObject("[Cast Selection/Proxy/Right]");
            rCn = new GameObject("[Cast Selection/Normalised/Right]");
            rMp = new GameObject("[Cast Selection/MidPoint/Right]");
            rTs = new GameObject("[Cast Selection/Target/Right]");
            rHp = new GameObject("[Cast Selection/HitPoint/Right]");
            rRt = new GameObject("[Cast Selection/Rotation/Right]");
            
            lCf = new GameObject("[Cast Selection/Follow/Left]");
            lCp = new GameObject("[Cast Selection/Proxy/Left]");
            lCn = new GameObject("[Cast Selection/Normalised/Left]");
            lMp = new GameObject("[Cast Selection/MidPoint/Left]");
            lTs = new GameObject("[Cast Selection/Target/Left]");
            lHp = new GameObject("[Cast Selection/HitPoint/Left]");
            lRt = new GameObject("[Cast Selection/Rotation/Left]");
            
            rVisual = Instantiate(targetVisual, rHp.transform);
            rVisual.name = "[Cast Selection/Visual/Right]";
            rVisual.SetActive(false);
            
            lVisual = Instantiate(targetVisual, lHp.transform);
            lVisual.name = "[Cast Selection/Visual/Left]";
            lVisual.SetActive(false);

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
            
            rLineRenderer = rCp.AddComponent<LineRenderer>();
            lLineRenderer = lCp.AddComponent<LineRenderer>();

            rLineRenderer.SetupLineRender(lineRenderMat, lineRenderWidth, false);
            lLineRenderer.SetupLineRender(lineRenderMat, lineRenderWidth, false);
            
            rLineRendererMaterial = rLineRenderer.material;
            lLineRendererMaterial = lLineRenderer.material;
        }

		private void Update()
		{
			SortLists();
			CastUpdate();

			switch (selectionType)
			{
				case SelectionType.FUZZY:
					LFocusObject = lHandList.FuzzyFindFocusObject(LFocusObject, lTarget, lDefault, controllerTransforms.LeftGrab() || lTouch);
					RFocusObject = rHandList.FuzzyFindFocusObject(RFocusObject, rTarget, rDefault, controllerTransforms.RightGrab() || rTouch);
					break;
				case SelectionType.RAY_CAST:
					LFocusObject = lHandList.RayCastFindFocusObject(LFocusObject, lTarget, lDefault, controllerTransforms.LeftTransform(), setSelectionRange ? selectionRange : float.PositiveInfinity, controllerTransforms.LeftGrab() || lTouch);
					RFocusObject = rHandList.RayCastFindFocusObject(RFocusObject, rTarget, rDefault, controllerTransforms.RightTransform(), setSelectionRange ? selectionRange : float.PositiveInfinity, controllerTransforms.RightGrab() || rTouch);
					break;
				case SelectionType.FUSION:
					LFocusObject = lHandList.FusionFindFocusObject(LFocusObject, lTarget, lDefault, controllerTransforms.LeftTransform(), controllerTransforms.LeftForwardVector(), setSelectionRange ? selectionRange : float.PositiveInfinity, controllerTransforms.LeftGrab() || lTouch);
					RFocusObject = rHandList.FusionFindFocusObject(RFocusObject, rTarget, rDefault, controllerTransforms.RightTransform(), controllerTransforms.RightForwardVector(), setSelectionRange ? selectionRange : float.PositiveInfinity, controllerTransforms.RightGrab() || rTouch);
					gFocusObject = gazeList.FusionFindFocusObject(gFocusObject, gTarget, gDefault, controllerTransforms.CameraTransform(), controllerTransforms.CameraForwardVector(), setSelectionRange ? selectionRange : float.PositiveInfinity, false);
					break;
				default:
					LFocusObject = null;
					RFocusObject = null;
					break;
			}
			
			lLineRenderer.DrawLineRenderer(LFocusObject, lMidPoint, controllerTransforms.LeftTransform(), lTarget, lineRenderQuality);
			rLineRenderer.DrawLineRenderer(RFocusObject, rMidPoint, controllerTransforms.RightTransform() ,rTarget, lineRenderQuality);
			
			lLineRendererMaterial.SetFloat(Distance, transform.TransformDistance(lTarget.transform));
			rLineRendererMaterial.SetFloat(Distance, transform.TransformDistance(rTarget.transform));
			
			LFocusObject.Manipulation(RFocusObject, lBaseObject, pLBaseObject, controllerTransforms.LeftGrab(), lGrabPrevious, controllerTransforms.LeftTransform(), lTouch, rTouch);
			RFocusObject.Manipulation(LFocusObject, rBaseObject, pRBaseObject, controllerTransforms.RightGrab(), rGrabPrevious, controllerTransforms.RightTransform(), rTouch, lTouch);
			
			lBaseObject = LFocusObject.FindSelectableObject(lBaseObject, controllerTransforms.LeftGrab());
			rBaseObject = RFocusObject.FindSelectableObject(rBaseObject, controllerTransforms.RightGrab());
			gBaseObject = gFocusObject.FindSelectableObject(gBaseObject, false);
			
			lGrabPrevious = controllerTransforms.LeftGrab();
			rGrabPrevious = controllerTransforms.RightGrab();

			ResetGameObjects(controllerTransforms.LeftTransform(), lPrevious);
			ResetGameObjects(controllerTransforms.RightTransform(), rPrevious);

			lPrevious = controllerTransforms.LeftTransform();
			rPrevious = controllerTransforms.RightTransform();
		}

		/// <summary>
		/// 
		/// </summary>
		private void CastUpdate()
		{
			// set the positions of the local objects and calculate the depth based on the angle of the controller
            rTs.transform.LocalDepth(
                rCf.ControllerAngle(
                    rCp, 
                    rCn, 
                    controllerTransforms.RightTransform(), 
                    controllerTransforms.CameraTransform(),
                    controllerTransforms.debugActive).CalculateDepth(ControllerTransforms.MaxAngle, 60, 0f, selectionRange, rCp.transform),
                false, 
                .2f);
            lTs.transform.LocalDepth(
                lCf.ControllerAngle(
                    lCp, 
                    lCn, 
                    controllerTransforms.LeftTransform(), 
                    controllerTransforms.CameraTransform(), 
                    controllerTransforms.debugActive).CalculateDepth(ControllerTransforms.MaxAngle, 60, 0f, selectionRange, lCp.transform), 
                false, 
                .2f);
            
            // detect valid positions for the target
            rTs.TargetLocation(rHp, rLastValidPosition = rTs.LastValidPosition(rLastValidPosition), layerIndex, true);
            lTs.TargetLocation(lHp, lLastValidPosition = lTs.LastValidPosition(lLastValidPosition),layerIndex, true);

            // set the midpoint position
            rMp.transform.LocalDepth(rCp.transform.Midpoint(rTs.transform), false, 0f);
            lMp.transform.LocalDepth(lCp.transform.Midpoint(lTs.transform), false, 0f);
            
            // set the rotation of the target based on the joystick values
            rVisual.Target(rHp, rCn.transform, controllerTransforms.RightJoystick(), rRt);
            lVisual.Target(lHp, lCn.transform, controllerTransforms.LeftJoystick(), lRt);
            
            // draw the line renderer
            rLineRenderer.BezierLineRenderer(controllerTransforms.RightTransform().position,rMp.transform.position,rHp.transform.position,lineRenderQuality);
            lLineRenderer.BezierLineRenderer(controllerTransforms.LeftTransform().position, lMp.transform.position, lHp.transform.position, lineRenderQuality);
            
            lLineRendererMaterial.SetFloat(Distance, transform.TransformDistance(lVisual.transform));
            rLineRendererMaterial.SetFloat(Distance, transform.TransformDistance(rVisual.transform));
		}

		private void LateUpdate()
		{
			// Calculate Hover States
			lBaseObject.Hover(pLBaseObject, rBaseObject);
			rBaseObject.Hover(pRBaseObject, lBaseObject);
			
			// Calculate Selection States
			lBaseObject.Selection(this, controllerTransforms.LeftSelect(), lSelectPrevious, lSelect, QuickSelectSensitivity);
			rBaseObject.Selection(this, controllerTransforms.RightSelect(), rSelectPrevious, rSelect, QuickSelectSensitivity);

			// Previous States
			lSelectPrevious = controllerTransforms.LeftSelect();
			rSelectPrevious = controllerTransforms.RightSelect();
			
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