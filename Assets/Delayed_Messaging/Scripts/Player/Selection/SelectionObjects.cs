using System.Collections.Generic;
using Delayed_Messaging.Scripts.Interaction.Cursors;
using Delayed_Messaging.Scripts.Objects;
using Delayed_Messaging.Scripts.Utilities;
using Pathfinding;
using UnityEngine;
using UnityEngine.Events;
using VR_Prototyping.Scripts.Utilities;

namespace Delayed_Messaging.Scripts.Player.Selection
{
	public class SelectionObjects: MonoBehaviour
	{
		public Selection.MultiSelect side;
		public BaseObject currentBaseObject;
		public BaseObject previousBaseObject;

		public GraphNode graphNode;
			
		public GameObject currentFocusObject;
		public Transform castLocation;

		public bool disabled;
		public bool selectionCurrent;
		public bool selectPrevious;

		public List<BaseObject> list = new List<BaseObject>();

		public UnityEvent selectStart = new UnityEvent();
		public UnityEvent selectEnd = new UnityEvent();

		public Vector3 selectionStart;
		public float selectionDistance;

		public MultiSelection multiSelect = new MultiSelection();
		public CastCursor castCursor;
		
		public void SetupSelectionObjects(Cast.CastObjects castObjects, Selection.MultiSelect sideReference, Material lineRenderMat, Material quadMat)
		{
			side = sideReference;
			castCursor = castObjects.visual.GetComponent<CastCursor>();

			multiSelect.selectionLineRenderer = castObjects.hitPoint.AddComponent<LineRenderer>();
			multiSelect.selectionLineRenderer.SetupLineRender(lineRenderMat, .01f, false);
			multiSelect.selectionLineRenderer.positionCount = 5;
			multiSelect.selectionLineRenderer.material = lineRenderMat;
			multiSelect.selectionQuadObject = new GameObject($"[Selection Quad/{side}]", typeof(MeshFilter), typeof(MeshRenderer));
			multiSelect.selectionQuadFilter = multiSelect.selectionQuadObject.GetComponent<MeshFilter>();
			multiSelect.selectionQuadFilter.mesh = Draw.QuadMesh();
			multiSelect.selectionQuadRenderer = multiSelect.selectionQuadObject.GetComponent<MeshRenderer>();
			multiSelect.selectionQuadRenderer.material = quadMat;
			multiSelect.selectionQuadRenderer.enabled = false;
		}

		public void SelectUpdate(Selection selection, Cast.CastObjects castObjects, BaseObject otherBaseObject, List<GameObject> castList, float threshold, bool select, bool disable)
		{
			selectionCurrent = select;
			disabled = disable;
			
			castLocation = castObjects.hitPoint.transform;
			currentFocusObject = castList.CastFindObject();
			currentBaseObject = currentFocusObject.FindBaseObject(otherBaseObject);
			Vector3 position = castLocation.transform.position;
			
			selectionDistance = Vector3.Distance(position, selectionStart);
			graphNode = AstarPath.active.GetNearest(position).node;
			
			currentBaseObject.Hover(previousBaseObject, otherBaseObject, castCursor);
			
			Check.Selection(currentBaseObject, selection, this, selectionDistance,select, selectPrevious, threshold, disable);

			if (selectionCurrent && !selectPrevious)
			{
				selectStart.Invoke();
			}

			if (!selectionCurrent && selectPrevious)
			{
				selectEnd.Invoke();
			}
			
			selectPrevious = selectionCurrent;
			previousBaseObject = currentBaseObject;
		}
	}

	public class MultiSelection
	{
		public GameObject selectionQuadObject;
		public MeshRenderer selectionQuadRenderer;
		public MeshFilter selectionQuadFilter;
		public LineRenderer selectionLineRenderer;
				
		public Bounds selectionBounds;
			
		public Vector3 multiSelectS;
		public Vector3 multiSelectE;
			
		public bool multiSelectActive;
	}
}