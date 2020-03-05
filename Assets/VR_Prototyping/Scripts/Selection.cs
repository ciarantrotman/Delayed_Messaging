using System;
using System.Collections.Generic;
using Delayed_Messaging.Scripts.Interaction;
using Delayed_Messaging.Scripts.Player;
using Delayed_Messaging.Scripts.Structures;
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
		public UserInterface UserInterface { get; set; }
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

		//private bool rTouch;
		//private bool lTouch;
		
		private bool lSelectPrevious;
		private bool rSelectPrevious;
		private bool lSelectHoldPrevious;
		private bool rSelectHoldPrevious;
		private bool lGrabPrevious;
		private bool rGrabPrevious;
		
		[HideInInspector] public List<float> lSelect = new List<float> {Capacity = (int) QuickSelectSensitivity};
		[HideInInspector] public List<float> rSelect = new List<float> {Capacity = (int) QuickSelectSensitivity};

		#region Selection Visuals

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

		#endregion

		private Transform lPrevious;
		private Transform rPrevious;

		#region Distance Cast Variables
		private GameObject parent;
		private GameObject cN;
		
		public class CastClass
		{
			public GameObject parent;
			
			public class CastObjects
			{
				public GameObject follow;
				public GameObject proxy;
				public GameObject normalised;
				public GameObject midpoint;
				public GameObject target;
				public GameObject hitPoint;
				public GameObject visual;
				public GameObject rotation;
				public Vector3 lastValidPosition;
				public LineRenderer lineRenderer;
			}

			public CastObjects lCastObject = new CastObjects();
			public CastObjects rCastObject = new CastObjects();

			public void SetupCastObjects(GameObject v, Transform castTransform, string instanceName, Material lineRendererMat)
			{
				parent = new GameObject("[" + instanceName + "/Calculations]");
				Transform parentTransform = parent.transform;
				parentTransform.SetParent(castTransform);
				CastObject(lCastObject, v, castTransform, parent.transform, instanceName, lineRendererMat);
				CastObject(rCastObject, v, castTransform, parent.transform, instanceName, lineRendererMat);
			}

			public void CastUpdate(CastObjects castObjects, Transform hand, Transform head, Vector2 joystick, float maxAngle, float minAngle, float minDistance, float maxDistance)
			{
				castObjects.lastValidPosition = castObjects.target.LastValidPosition(castObjects.lastValidPosition);
				Set.DistanceCast(castObjects.target, castObjects.follow, castObjects.proxy, castObjects.normalised, castObjects.hitPoint, castObjects.midpoint, castObjects.rotation, castObjects.visual, 
					hand, head, joystick, maxAngle, minAngle,
					minDistance, maxDistance, castObjects.lastValidPosition);
				castObjects.lineRenderer.BezierLineRenderer(hand.position,castObjects.midpoint.transform.position,castObjects.hitPoint.transform.position);
				castObjects.lineRenderer.material.SetFloat(Distance, hand.TransformDistance(castObjects.hitPoint.transform));
			}

			private void CastObject(CastObjects castObjects, GameObject v, Transform castTransform, Transform parentTransform, string instanceName, Material lineRendererMat)
			{
				castObjects.follow = new GameObject("[" + instanceName + "/Follow]");
				castObjects.proxy = new GameObject("[" + instanceName + "/Proxy]");
				castObjects.normalised = new GameObject("[" + instanceName + "/Normalised]");
				castObjects.midpoint = new GameObject("[" + instanceName + "/MidPoint]");
				castObjects.target = new GameObject("[" + instanceName + "/Target]");
				castObjects.hitPoint = new GameObject("[" + instanceName + "/HitPoint]");
				castObjects.rotation = new GameObject("[" + instanceName + "/Rotation]");

				castObjects.visual = Instantiate(v, castObjects.hitPoint.transform);
				castObjects.visual.name = "[" + instanceName + "/Visual/Right]";
				castObjects.visual.SetActive(true);

				castObjects.follow.transform.SetParent(parentTransform);
				castObjects.proxy.transform.SetParent(castObjects.follow.transform);
				castObjects.normalised.transform.SetParent(castObjects.follow.transform);
				castObjects.midpoint.transform.SetParent(castObjects.proxy.transform);
				castObjects.target.transform.SetParent(castObjects.normalised.transform);
				castObjects.hitPoint.transform.SetParent(castTransform);
				castObjects.rotation.transform.SetParent(castObjects.hitPoint.transform);

				castObjects.lineRenderer = castObjects.proxy.AddComponent<LineRenderer>();
				castObjects.lineRenderer.SetupLineRender(lineRendererMat, .01f, false);
			}
		}
		
		public CastClass cast = new CastClass();
		/*
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
		private GameObject lRt; // rotation*/

		private float lSelectionDistance;
		private float rSelectionDistance;
		private Vector3 rLastValidPosition;
		private Vector3 lLastValidPosition;
		private Vector3 rSelectStartPoint;
		private Vector3 lSelectStartPoint;

		private CastCursor rCastCursor;
		private CastCursor lCastCursor;

		public enum MultiSelect
		{
			LEFT,
			RIGHT
		}

		public class MultiSelection
		{
			public Bounds selectionBounds;
			public Vector3 multiSelectS;
			public Vector3 multiSelectE;
			public bool multiSelectActive;
			public GameObject selectionQuadObject;
			public MeshRenderer selectionQuad;
			public MeshFilter selectionQuadFilter;
			public LineRenderer selectionLineRenderer;
		}

		public MultiSelection lMultiSelection = new MultiSelection();
        public MultiSelection rMultiSelection = new MultiSelection();
        
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
		[SerializeField, Range(0f, 1f)] private float multiSelectDistanceThreshold = .2f;
		public float castSelectionRadius;
		[SerializeField, Range(1, 15)] private int layerIndex = 10;

		[Header("Selection Aesthetics")]
		[SerializeField] private GameObject castCursorObject;
		public GameObject gazeCursorObject;
		//[SerializeField] private GameObject targetVisual;
		[SerializeField, Space(10)] private Material lineRenderMat;
		[SerializeField] private Material selectionLineRenderMat;
		[SerializeField] private Material selectionQuadMaterial;
		[SerializeField, Range(.001f, .1f), Space(10)] private float lineRenderWidth = .005f;
		[Range(3f, 30f)] public int lineRenderQuality = 15;
		[Range(.1f, 2.5f)] public float inactiveLineRenderOffset = 1f;

		[HideInInspector] public List<GameObject> globalList;
		[HideInInspector] public List<BaseObject> baseObjectsList;
		[HideInInspector] public List<GameObject> gazeList;
		[HideInInspector] public List<GameObject> rHandList;
		[HideInInspector] public List<GameObject> lHandList;
		[HideInInspector] public List<GameObject> rCastList;
		[HideInInspector] public List<GameObject> lCastList;

		#endregion
		private void Start ()
		{
			controllerTransforms = GetComponent<ControllerTransforms>();
			player = GetComponent<Player>();
			UserInterface = GetComponent<UserInterface>();
			
			SetupGameObjects();
			SetupCastSelectGameObjects();
			
			// Set initial states
			ToggleSelectionState(UserInterface.DominantHand.LEFT, !disableLeftHand);
			ToggleSelectionState(UserInterface.DominantHand.RIGHT, !disableRightHand);
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

				gazeCursorObject = Instantiate(gazeCursorObject, gTarget.transform);
			}

			lDefault.transform.SetOffsetPosition(controllerTransforms.LeftTransform(), inactiveLineRenderOffset);
			rDefault.transform.SetOffsetPosition(controllerTransforms.RightTransform(), inactiveLineRenderOffset);
			
			lMidPoint.transform.SetOffsetPosition(controllerTransforms.LeftTransform(), 0f);
			rMidPoint.transform.SetOffsetPosition(controllerTransforms.RightTransform(), 0f);

			initialised = true;
		}
		private void SetupCastSelectGameObjects()
        {
	        cast.SetupCastObjects(castCursorObject, transform, "Cast Selection", selectionLineRenderMat);
	        /*
	        const string instanceName = "Cast Selection";
            
            parent = new GameObject("[" + instanceName + "/Calculations]");
            Transform parentTransform = parent.transform;
            parentTransform.SetParent(transform);*/
/*
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
            
            rVisual = Instantiate(castCursorObject, rHp.transform);
            rVisual.name = "[" + instanceName + "/Visual/Right]";
            rCastCursor = rVisual.GetComponent<CastCursor>();
            rVisual.SetActive(true);
            
            lVisual = Instantiate(castCursorObject, lHp.transform);
            lVisual.name = "[" + instanceName + "/Visual/Left]";
            lCastCursor = lVisual.GetComponent<CastCursor>();
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
            */

            lMultiSelection.selectionLineRenderer = cast.lCastObject.hitPoint.AddComponent<LineRenderer>();
            rMultiSelection.selectionLineRenderer = cast.rCastObject.hitPoint.AddComponent<LineRenderer>();
            
            lMultiSelection.selectionLineRenderer.SetupLineRender(selectionLineRenderMat, .01f, false);
            rMultiSelection.selectionLineRenderer.SetupLineRender(selectionLineRenderMat, .01f, false);

            lCastCursor = cast.lCastObject.visual.GetComponent<CastCursor>();
	        rCastCursor = cast.rCastObject.visual.GetComponent<CastCursor>();

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
            
            rLineRendererMaterial = selectionLineRenderMat;
            lLineRendererMaterial = selectionLineRenderMat;
        }

		private void Update()
		{
			SortLists();

			switch (selectionType)
			{
				case SelectionType.FUZZY:
					break;
				case SelectionType.RAY_CAST:
					break;
				case SelectionType.FUSION:
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
			//gBaseObject = gFocusObject.FindBaseObject(gBaseObject, false);

			ResetGameObjects(controllerTransforms.LeftTransform(), lPrevious);
			ResetGameObjects(controllerTransforms.RightTransform(), rPrevious);

			// Calculate Hover States
			lBaseObject.Hover(pLBaseObject, rBaseObject, lCastCursor);
			rBaseObject.Hover(pRBaseObject, lBaseObject, rCastCursor);
			
			// Calculate Selection States
			//lBaseObject.Selection(this, player,  player.lSelectedObjects, controllerTransforms.LeftSelect(), lSelectPrevious, lSelect, QuickSelectSensitivity, SelectHoldL, MultiSelect.LEFT, disableLeftHand);
			//rBaseObject.Selection(this, player,  player.rSelectedObjects, controllerTransforms.RightSelect(), rSelectPrevious, rSelect, QuickSelectSensitivity, SelectHoldR,MultiSelect.RIGHT, disableRightHand);
			rBaseObject.Selection(this, player,  player.rSelectedObjects, rSelectionDistance, controllerTransforms.RightSelect(), rSelectPrevious, multiSelectDistanceThreshold, rMultiSelection, MultiSelect.RIGHT, disableRightHand);

			// Previous States
			lSelectPrevious = controllerTransforms.LeftSelect();
			rSelectPrevious = controllerTransforms.RightSelect();
			
			lGrabPrevious = controllerTransforms.LeftGrab();
			rGrabPrevious = controllerTransforms.RightGrab();
			
			lPrevious = controllerTransforms.LeftTransform();
			rPrevious = controllerTransforms.RightTransform();
			
			pLBaseObject = lBaseObject;
			pRBaseObject = rBaseObject;
		}
		
		private void CastUpdate()
		{
			CastLocationL = cast.lCastObject.hitPoint.transform;
			CastLocationR = cast.rCastObject.hitPoint.transform;
			
			cast.CastUpdate(cast.lCastObject, controllerTransforms.LeftTransform(), controllerTransforms.CameraTransform(), controllerTransforms.LeftJoystick(), maximumAngle, minimumAngle, .1f, selectionRange);
			cast.CastUpdate(cast.rCastObject, controllerTransforms.RightTransform(), controllerTransforms.CameraTransform(), controllerTransforms.RightJoystick(), maximumAngle, minimumAngle, .1f, selectionRange);
			
			lSelectionDistance = Vector3.Distance(CastLocationL.transform.position, lSelectStartPoint);
			rSelectionDistance = Vector3.Distance(CastLocationR.transform.position, rSelectStartPoint);

/*
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
            rLineRendererMaterial.SetFloat(Distance, transform.TransformDistance(rVisual.transform));*/
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
					lSelectStartPoint = CastLocationL.position;
					break;
				case MultiSelect.RIGHT:
					rSelectStartPoint = CastLocationR.position;
					break;
				default:
					throw new ArgumentOutOfRangeException(nameof(side), side, null);
			}
		}
		public void SelectEnd(MultiSelect side, List<BaseObject> list)
		{
			player.ClearSelectedObjects(side, list);
		}
		public void MultiSelectStart(MultiSelection multiSelect)
		{
			if (multiSelect.multiSelectActive)
			{
				return;
			}
			
			UserInterface.SetObjectHeaderState(false);
			
			multiSelect.multiSelectActive = true;
			multiSelect.multiSelectS = CastLocationR.position;
			multiSelect.selectionLineRenderer.enabled = true;
			multiSelect.selectionLineRenderer.DrawRectangularLineRenderer(multiSelect.multiSelectS, multiSelect.multiSelectS);
			multiSelect.selectionQuadFilter.mesh.DrawQuadMesh(multiSelect.multiSelectS, multiSelect.multiSelectS);
			multiSelect.selectionQuad.enabled = true;
			
			Bounds bounds = multiSelect.selectionQuad.bounds;
			multiSelect.selectionBounds = new Bounds(bounds.center, new Vector3(bounds.size.x, 1f, bounds.size.z));
		}
		public void MultiSelectHold(MultiSelection multiSelect)
		{
			multiSelect.multiSelectE = CastLocationR.position;
			multiSelect.selectionLineRenderer.DrawRectangularLineRenderer(multiSelect.multiSelectS, multiSelect.multiSelectE);
			multiSelect.selectionQuadFilter.mesh.DrawQuadMesh(multiSelect.multiSelectS, multiSelect.multiSelectE);
			
			Bounds bounds = multiSelect.selectionQuad.bounds;
			multiSelect.selectionBounds = new Bounds(bounds.center, new Vector3(bounds.size.x, 1f, bounds.size.z));
		}

		public void MultiSelectEnd(MultiSelection multiSelect)
		{
			multiSelect.multiSelectActive = false;
			multiSelect.multiSelectE = CastLocationR.position;
			multiSelect.selectionLineRenderer.enabled = false;
			multiSelect.selectionQuad.enabled = false;
			
			Bounds bounds = multiSelect.selectionQuad.bounds;
			multiSelect.selectionBounds = new Bounds(bounds.center, new Vector3(bounds.size.x, 1f, bounds.size.z));
			
			player.ClearSelectedObjects(MultiSelect.RIGHT, player.rSelectedObjects);
			foreach (BaseObject baseObject in baseObjectsList)
			{
				if (multiSelect.selectionBounds.Intersects(baseObject.ObjectBounds))
				{
					baseObject.SelectStart(MultiSelect.RIGHT, player.rSelectedObjects);
				}
			}
		}

		public void ToggleSelectionState(UserInterface.DominantHand hand, bool state)
		{
			switch (hand)
			{
				case UserInterface.DominantHand.LEFT:
					cast.lCastObject.lineRenderer.enabled = state;
					lMultiSelection.selectionQuad.enabled = state;
					cast.lCastObject.visual.SetActive(state);
					disableLeftHand = !state;
					break;
				case UserInterface.DominantHand.RIGHT:
					cast.rCastObject.lineRenderer.enabled = state;
					rMultiSelection.selectionQuad.enabled = state;
					cast.rCastObject.visual.SetActive(state);
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
			Pathfinding.Util.Draw.Gizmos.CircleXZ(cast.lCastObject.hitPoint.transform.position, castSelectionRadius, Color.cyan);
			Gizmos.DrawLine(lMultiSelection.multiSelectS, lMultiSelection.multiSelectE);
			Gizmos.DrawWireCube(lMultiSelection.selectionBounds.center, lMultiSelection.selectionBounds.size);
			if (LFocusObject != null)
			{
				Gizmos.color = Color.cyan;
				Gizmos.DrawSphere(LFocusObject.transform.position, .2f);
			}
			Pathfinding.Util.Draw.Gizmos.CircleXZ(lSelectStartPoint, .01f, Color.black);
			Gizmos.color = Color.yellow;
			Pathfinding.Util.Draw.Gizmos.CircleXZ(cast.rCastObject.hitPoint.transform.position, castSelectionRadius, Color.yellow);
			Gizmos.DrawLine(rMultiSelection.multiSelectS, rMultiSelection.multiSelectE);
			Gizmos.DrawWireCube(rMultiSelection.selectionBounds.center, rMultiSelection.selectionBounds.size);
			if (RFocusObject != null)
			{
				Gizmos.color = Color.yellow;
				Gizmos.DrawSphere(RFocusObject.transform.position, .2f);
			}
		}
	}
}