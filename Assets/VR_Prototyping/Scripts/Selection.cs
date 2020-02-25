using System;
using System.Collections.Generic;
using Delayed_Messaging.Scripts.Interaction;
using Delayed_Messaging.Scripts.Player;
using UnityEngine;
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
		private Player player;
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
		public Transform CastLocationR { get; set; }
		public Transform CastLocationL { get; set; }
		public bool SelectHoldR { get; set; }
		public bool SelectHoldL { get; set; }

		private bool rTouch;
		private bool lTouch;
		private bool lSelectPrevious;
		private bool rSelectPrevious;
		private bool lSelectHoldPrevious;
		private bool rSelectHoldPrevious;
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
		private GameObject cN;

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

		public enum MultiSelect
		{
			LEFT,
			RIGHT
		}
		public struct MultiSelection
        {
	        public Bounds SelectionBounds { get; set; }
	        public Vector3 multiSelectS;
	        public Vector3 multiSelectM;
	        public Vector3 multiSelectE;
	        public GameObject selectionQuadObject;
	        public MeshRenderer selectionQuad;
	        public MeshFilter selectionQuadFilter;
	        public LineRenderer selectionLineRenderer;
        }
        
        public MultiSelection lMultiSelection;
        public MultiSelection rMultiSelection;
        
        #endregion
		
		private const float QuickSelectSensitivity = 20f;
		
		[Header("Selection Settings")]
		[SerializeField] private SelectionType selectionType;
		[Range(0f, 180f), Space(10)] public float gaze = 60f;
		[Range(0f, 180f)] public float manual = 25f;
		[Space(10)] public bool setSelectionRange;		
		[Range(0f, 250f)] public float selectionRange = 25f;
		[Space(10)] public bool disableLeftHand;
		public bool disableRightHand;
		
		[Header("Casting Settings")]
		[SerializeField, Range(0f, 180f)] private float minimumAngle = 60f;
		[SerializeField, Range(0f, 180f)] private float maximumAngle = 110f;
		public float castSelectionRadius;
		[SerializeField, Range(1, 15)] private int layerIndex = 10;

		[Header("Selection Aesthetics")]
		[SerializeField] private GameObject targetVisual;
		[SerializeField] private Material lineRenderMat;
		[SerializeField] private Material selectionLineRenderMat;
		[SerializeField] private Material selectionQuadMaterial;
		[SerializeField, Range(.001f, .1f)] private float lineRenderWidth = .005f;
		[Space(10)] public GameObject gazeCursor;
		[Range(3f, 30f), Space(10)] public int lineRenderQuality = 15;
		[Range(.1f, 2.5f)] public float inactiveLineRenderOffset = 1f;

		[HideInInspector] public List<GameObject> globalList;
		[HideInInspector] public List<BaseObject> baseObjectsList;
		[HideInInspector] public List<GameObject> gazeList;
		[HideInInspector] public List<GameObject> rHandList;
		[HideInInspector] public List<GameObject> lHandList;
		[HideInInspector] public List<GameObject> rCastList;
		[HideInInspector] public List<GameObject> lCastList;

		public Selection(bool rTouch, bool lTouch)
		{
			this.rTouch = rTouch;
			this.lTouch = lTouch;
		}

		#endregion
		private void Start ()
		{
			controllerTransforms = GetComponent<ControllerTransforms>();
			player = GetComponent<Player>();
			
			SetupGameObjects();
			SetupCastSelectGameObjects();

			//lLineRenderer = controllerTransforms.LeftTransform().gameObject.AddComponent<LineRenderer>();
			//rLineRenderer = controllerTransforms.RightTransform().gameObject.AddComponent<LineRenderer>();
			
			//lLineRenderer.SetupLineRender(lineRenderMat, lineRenderWidth, true);
			//rLineRenderer.SetupLineRender(lineRenderMat, lineRenderWidth, true);
			
			//rLineRendererMaterial = rLineRenderer.material;
			//lLineRendererMaterial = lLineRenderer.material;
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
	        //controllerTransforms.SetupCastObjects( targetVisual, "Selection", true);
			const string instanceName = "Cast Selection";
            
            parent = new GameObject("[" + instanceName + "/Calculations]");
            Transform parentTransform = parent.transform;
            parentTransform.SetParent(transform);

            rCf = new GameObject("[" + instanceName + "/Follow/Right]");
            rCp = new GameObject("[" + instanceName + "/Proxy/Right]");
            rCn = new GameObject("[" + instanceName + "/Normalised/Right]");
            rMp = new GameObject("[" + instanceName + "/MidPoint/Right]");
            rTs = new GameObject("[" + instanceName + "/Target/Right]");
            rHp = new GameObject("[" + instanceName + "/HitPoint/Right]");
            rRt = new GameObject("[" + instanceName + "/Rotation/Right]");
            
            lCf = new GameObject("[" + instanceName + "/Follow/Left]");
            lCp = new GameObject("[" + instanceName + "/Proxy/Left]");
            lCn = new GameObject("[" + instanceName + "/Normalised/Left]");
            lMp = new GameObject("[" + instanceName + "/MidPoint/Left]");
            lTs = new GameObject("[" + instanceName + "/Target/Left]");
            lHp = new GameObject("[" + instanceName + "/HitPoint/Left]");
            lRt = new GameObject("[" + instanceName + "/Rotation/Left]");
            
            rVisual = Instantiate(targetVisual, rHp.transform);
            rVisual.name = "[" + instanceName + "/Visual/Right]";
            rVisual.SetActive(true);
            
            lVisual = Instantiate(targetVisual, lHp.transform);
            lVisual.name = "[" + instanceName + "/Visual/Left]";
            lVisual.SetActive(true);

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

            rLineRenderer.SetupLineRender(lineRenderMat, lineRenderWidth, true);
            lLineRenderer.SetupLineRender(lineRenderMat, lineRenderWidth, true);
            
            lMultiSelection.selectionLineRenderer = lHp.AddComponent<LineRenderer>();
            rMultiSelection.selectionLineRenderer = rHp.AddComponent<LineRenderer>();
            
            lMultiSelection.selectionLineRenderer.SetupLineRender(selectionLineRenderMat, .01f, false);
            rMultiSelection.selectionLineRenderer.SetupLineRender(selectionLineRenderMat, .01f, false);
            
            lMultiSelection.selectionLineRenderer.positionCount = 5;
            rMultiSelection.selectionLineRenderer.positionCount = 5;

            lMultiSelection.selectionQuadObject = new GameObject("[Selection Quad/Left]", typeof(MeshFilter), typeof(MeshRenderer));
            lMultiSelection.selectionQuad = lMultiSelection.selectionQuadObject.GetComponent<MeshRenderer>();
            lMultiSelection.selectionQuad.material = selectionQuadMaterial;
            lMultiSelection.selectionQuadFilter = lMultiSelection.selectionQuadObject.GetComponent<MeshFilter>();
            lMultiSelection.selectionQuadFilter.mesh = Draw.GenerateQuadMesh();
            lMultiSelection.selectionQuadFilter.mesh = Draw.GenerateQuadMesh();
            
            rMultiSelection.selectionQuadObject = new GameObject("[Selection Quad/Right]", typeof(MeshFilter), typeof(MeshRenderer));
            rMultiSelection.selectionQuad = rMultiSelection.selectionQuadObject.GetComponent<MeshRenderer>();
            rMultiSelection.selectionQuad.material = selectionQuadMaterial;
            rMultiSelection.selectionQuadFilter = rMultiSelection.selectionQuadObject.GetComponent<MeshFilter>();
            rMultiSelection.selectionQuadFilter.mesh = Draw.GenerateQuadMesh();

            rLineRendererMaterial = rLineRenderer.material;
            lLineRendererMaterial = lLineRenderer.material;
        }

		private void Update()
		{
			SortLists();

			switch (selectionType)
			{
				case SelectionType.FUZZY:
					LFocusObject = lHandList.FuzzyFindFocusObject(LFocusObject, lTarget, lDefault, controllerTransforms.LeftGrab() || lTouch);
					RFocusObject = rHandList.FuzzyFindFocusObject(RFocusObject, rTarget, rDefault, controllerTransforms.RightGrab() || rTouch);
					lLineRenderer.DrawLineRenderer(LFocusObject, lMidPoint, controllerTransforms.LeftTransform(), lTarget, lineRenderQuality);
					rLineRenderer.DrawLineRenderer(RFocusObject, rMidPoint, controllerTransforms.RightTransform() ,rTarget, lineRenderQuality);
					lLineRendererMaterial.SetFloat(Distance, transform.TransformDistance(lTarget.transform));
					rLineRendererMaterial.SetFloat(Distance, transform.TransformDistance(rTarget.transform));
					break;
				case SelectionType.RAY_CAST:
					LFocusObject = lHandList.RayCastFindFocusObject(LFocusObject, lTarget, lDefault, controllerTransforms.LeftTransform(), setSelectionRange ? selectionRange : float.PositiveInfinity, controllerTransforms.LeftGrab() || lTouch);
					RFocusObject = rHandList.RayCastFindFocusObject(RFocusObject, rTarget, rDefault, controllerTransforms.RightTransform(), setSelectionRange ? selectionRange : float.PositiveInfinity, controllerTransforms.RightGrab() || rTouch);
					lLineRenderer.DrawLineRenderer(LFocusObject, lMidPoint, controllerTransforms.LeftTransform(), lTarget, lineRenderQuality);
					rLineRenderer.DrawLineRenderer(RFocusObject, rMidPoint, controllerTransforms.RightTransform() ,rTarget, lineRenderQuality);
					lLineRendererMaterial.SetFloat(Distance, transform.TransformDistance(lTarget.transform));
					rLineRendererMaterial.SetFloat(Distance, transform.TransformDistance(rTarget.transform));
					break;
				case SelectionType.FUSION:
					LFocusObject = lHandList.FusionFindFocusObject(LFocusObject, lTarget, lDefault, controllerTransforms.LeftTransform(), controllerTransforms.LeftForwardVector(), setSelectionRange ? selectionRange : float.PositiveInfinity, controllerTransforms.LeftGrab() || lTouch);
					RFocusObject = rHandList.FusionFindFocusObject(RFocusObject, rTarget, rDefault, controllerTransforms.RightTransform(), controllerTransforms.RightForwardVector(), setSelectionRange ? selectionRange : float.PositiveInfinity, controllerTransforms.RightGrab() || rTouch);
					gFocusObject = gazeList.FusionFindFocusObject(gFocusObject, gTarget, gDefault, controllerTransforms.CameraTransform(), controllerTransforms.CameraForwardVector(), setSelectionRange ? selectionRange : float.PositiveInfinity, false);
					lLineRenderer.DrawLineRenderer(LFocusObject, lMidPoint, controllerTransforms.LeftTransform(), lTarget, lineRenderQuality);
					rLineRenderer.DrawLineRenderer(RFocusObject, rMidPoint, controllerTransforms.RightTransform() ,rTarget, lineRenderQuality);
					lLineRendererMaterial.SetFloat(Distance, transform.TransformDistance(lTarget.transform));
					rLineRendererMaterial.SetFloat(Distance, transform.TransformDistance(rTarget.transform));
					break;
				case SelectionType.DISTANCE_CAST:
					CastUpdate();
					LFocusObject = lCastList.CastFindObject();
					RFocusObject = rCastList.CastFindObject();
					break;
				default:
					LFocusObject = null;
					RFocusObject = null;
					break;
			}

			lBaseObject = LFocusObject.FindBaseObject(lBaseObject, controllerTransforms.LeftGrab());
			rBaseObject = RFocusObject.FindBaseObject(rBaseObject, controllerTransforms.RightGrab());
			gBaseObject = gFocusObject.FindBaseObject(gBaseObject, false);
			
			lGrabPrevious = controllerTransforms.LeftGrab();
			rGrabPrevious = controllerTransforms.RightGrab();

			ResetGameObjects(controllerTransforms.LeftTransform(), lPrevious);
			ResetGameObjects(controllerTransforms.RightTransform(), rPrevious);

			lPrevious = controllerTransforms.LeftTransform();
			rPrevious = controllerTransforms.RightTransform();
		}
		
		private void CastUpdate()
		{
			CastLocationL = lHp.transform;
			CastLocationR = rHp.transform;

			rLastValidPosition = rTs.LastValidPosition(rLastValidPosition);
			lLastValidPosition = lTs.LastValidPosition(lLastValidPosition);
            
			Set.DistanceCast(rTs, rCf, rCp, rCn, rHp, rMp, rRt, rVisual, 
				controllerTransforms.RightTransform(), controllerTransforms.CameraTransform(), controllerTransforms.RightJoystick(), maximumAngle, minimumAngle,
				.1f, selectionRange, rLastValidPosition);
			Set.DistanceCast(lTs, lCf, lCp, lCn, lHp, lMp, lRt, lVisual, 
				controllerTransforms.LeftTransform(), controllerTransforms.CameraTransform(), controllerTransforms.LeftJoystick(), maximumAngle, minimumAngle,
				.1f, selectionRange, lLastValidPosition);
			
            rLineRenderer.BezierLineRenderer(controllerTransforms.RightTransform().position,rMp.transform.position,rHp.transform.position,lineRenderQuality);
            lLineRenderer.BezierLineRenderer(controllerTransforms.LeftTransform().position, lMp.transform.position,lHp.transform.position, lineRenderQuality);
            
            lLineRendererMaterial.SetFloat(Distance, transform.TransformDistance(lVisual.transform));
            rLineRendererMaterial.SetFloat(Distance, transform.TransformDistance(rVisual.transform));
		}

		private void LateUpdate()
		{
			// Calculate Hover States
			lBaseObject.Hover(pLBaseObject, rBaseObject);
			rBaseObject.Hover(pRBaseObject, lBaseObject);
			
			// Calculate Selection States
			lBaseObject.Selection(this, player, controllerTransforms.LeftSelect(), lSelectPrevious, lSelect, QuickSelectSensitivity, SelectHoldL, MultiSelect.LEFT, disableLeftHand);
			rBaseObject.Selection(this, player, controllerTransforms.RightSelect(), rSelectPrevious, rSelect, QuickSelectSensitivity, SelectHoldR,MultiSelect.RIGHT, disableRightHand);

			// Previous States
			lSelectPrevious = controllerTransforms.LeftSelect();
			rSelectPrevious = controllerTransforms.RightSelect();
			
			pLBaseObject = lBaseObject;
			pRBaseObject = rBaseObject;
		}

		private void SortLists()
		{
			switch (selectionType)
			{
				case SelectionType.DISTANCE_CAST:
					lCastList.Sort(SortBy.CastObjectL);
					rCastList.Sort(SortBy.CastObjectR);
					break;
				case SelectionType.FUSION:
					lHandList.Sort(SortBy.FocusObjectL);
					rHandList.Sort(SortBy.FocusObjectR);
					gazeList.Sort(SortBy.FocusObjectG);
					break;
				case SelectionType.FUZZY:
					lHandList.Sort(SortBy.FocusObjectL);
					rHandList.Sort(SortBy.FocusObjectR);
					gazeList.Sort(SortBy.FocusObjectG);
					break;
				case SelectionType.RAY_CAST:
					break;
				default:
					break;
			}
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
		
		public void SelectStart(MultiSelect side)
		{
			switch (side)
			{
				case MultiSelect.LEFT:
					player.ClearSelectedObjects(MultiSelect.LEFT, lBaseObject);
					lMultiSelection.multiSelectS = CastLocationL.position;
					lMultiSelection.selectionLineRenderer.enabled = true;
					lMultiSelection.selectionLineRenderer.DrawRectangularLineRenderer(lMultiSelection.multiSelectS, lMultiSelection.multiSelectS);
					lMultiSelection.selectionQuadFilter.mesh.DrawQuadMesh(lMultiSelection.multiSelectS, lMultiSelection.multiSelectS);
					lMultiSelection.selectionQuad.enabled = true;
					lMultiSelection.SelectionBounds = lMultiSelection.selectionLineRenderer.bounds;
					break;
				case MultiSelect.RIGHT:
					player.ClearSelectedObjects(MultiSelect.RIGHT, rBaseObject);
					rMultiSelection.multiSelectS = CastLocationR.position;
					rMultiSelection.selectionLineRenderer.enabled = true;
					rMultiSelection.selectionLineRenderer.DrawRectangularLineRenderer(rMultiSelection.multiSelectS, rMultiSelection.multiSelectS);
					rMultiSelection.selectionQuadFilter.mesh.DrawQuadMesh(rMultiSelection.multiSelectS, rMultiSelection.multiSelectS);
					rMultiSelection.selectionQuad.enabled = true;
					rMultiSelection.SelectionBounds = rMultiSelection.selectionLineRenderer.bounds;
					break;
				default:
					throw new ArgumentOutOfRangeException(nameof(side), side, null);
			}	
		}
		public void SelectHold(MultiSelect side)
		{
			switch (side)
			{
				case MultiSelect.LEFT:
					SelectHoldL = true;
					lMultiSelection.multiSelectE = CastLocationL.position;
					lMultiSelection.selectionLineRenderer.DrawRectangularLineRenderer(lMultiSelection.multiSelectS, lMultiSelection.multiSelectE);
					lMultiSelection.selectionQuadFilter.mesh.DrawQuadMesh(lMultiSelection.multiSelectS, lMultiSelection.multiSelectE);
					lMultiSelection.SelectionBounds = lMultiSelection.selectionLineRenderer.bounds;
					break;
				case MultiSelect.RIGHT:
					SelectHoldR = true;
					rMultiSelection.multiSelectE = CastLocationR.position;
					rMultiSelection.selectionLineRenderer.DrawRectangularLineRenderer(rMultiSelection.multiSelectS, rMultiSelection.multiSelectE);
					rMultiSelection.selectionQuadFilter.mesh.DrawQuadMesh(rMultiSelection.multiSelectS, rMultiSelection.multiSelectE);
					rMultiSelection.SelectionBounds = rMultiSelection.selectionLineRenderer.bounds;
					break;
				default:
					throw new ArgumentOutOfRangeException(nameof(side), side, null);
			}
		}

		public void SelectHoldEnd(MultiSelect side)
		{
			switch (side)
			{
				case MultiSelect.LEFT:
					SelectHoldL = false;
					lMultiSelection.multiSelectE = CastLocationL.position;
					lMultiSelection.selectionLineRenderer.enabled = false;
					lMultiSelection.selectionQuad.enabled = false;
					foreach (BaseObject baseObject in baseObjectsList)
					{
						if (lMultiSelection.SelectionBounds.Intersects(baseObject.ObjectBounds))
						{
							baseObject.SelectStart(MultiSelect.LEFT);
						}
					}
					break;
				case MultiSelect.RIGHT:
					SelectHoldR = false;
					rMultiSelection.multiSelectE = CastLocationR.position;
					rMultiSelection.selectionLineRenderer.enabled = false;
					rMultiSelection.selectionQuad.enabled = false;
					foreach (BaseObject baseObject in baseObjectsList)
					{
						if (rMultiSelection.SelectionBounds.Intersects(baseObject.ObjectBounds))
						{
							baseObject.SelectStart(MultiSelect.RIGHT);
						}
					}
					break;
				default:
					throw new ArgumentOutOfRangeException(nameof(side), side, null);
			}		
		}

		public void ToggleSelectionState(UserInterface.DominantHand hand, bool state)
		{
			switch (hand)
			{
				case UserInterface.DominantHand.LEFT:
					lLineRenderer.enabled = state;
					lMultiSelection.selectionQuad.enabled = state;
					lVisual.SetActive(state);
					disableLeftHand = !state;
					break;
				case UserInterface.DominantHand.RIGHT:
					rLineRenderer.enabled = state;
					rMultiSelection.selectionQuad.enabled = state;
					rVisual.SetActive(state);
					disableRightHand = !state;
					break;
				default:
					throw new ArgumentOutOfRangeException(nameof(hand), hand, null);
			}
		}

		private void OnDrawGizmos () 
		{
			if (controllerTransforms != null && controllerTransforms.debugActive)
			{
				DrawGizmos ();
			}
		}
		
		private void DrawGizmos ()
		{
			Gizmos.color = Color.cyan;
			Gizmos.DrawWireSphere(lHp.transform.position, castSelectionRadius);
			Gizmos.DrawLine(lMultiSelection.multiSelectS, lMultiSelection.multiSelectE);
			Gizmos.DrawWireCube(lMultiSelection.SelectionBounds.center, lMultiSelection.SelectionBounds.size);
			if (LFocusObject != null)
			{
				Gizmos.color = Color.cyan;
				Gizmos.DrawSphere(LFocusObject.transform.position, .2f);
			}
			Gizmos.color = Color.yellow;
			Gizmos.DrawWireSphere(rHp.transform.position, castSelectionRadius);
			Gizmos.DrawLine(rMultiSelection.multiSelectS, rMultiSelection.multiSelectE);
			Gizmos.DrawWireCube(rMultiSelection.SelectionBounds.center, rMultiSelection.SelectionBounds.size);
			if (RFocusObject != null)
			{
				Gizmos.color = Color.yellow;
				Gizmos.DrawSphere(RFocusObject.transform.position, .2f);
			}
		}
	}
}