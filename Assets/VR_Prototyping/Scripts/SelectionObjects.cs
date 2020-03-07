using System.Collections.Generic;
using Delayed_Messaging.Scripts.Interaction;
using Delayed_Messaging.Scripts.Interaction.Cursors;
using Delayed_Messaging.Scripts.Utilities;
using UnityEngine;
using VR_Prototyping.Scripts.Utilities;

namespace VR_Prototyping.Scripts
{
	public class SelectionObjects: MonoBehaviour
	{
		public Selection.MultiSelect side;
		public BaseObject currentBaseObject;
		public BaseObject previousBaseObject;
			
		public GameObject currentFocusObject;
		public Transform castLocation;

		public bool selectPrevious;

		public List<BaseObject> list = new List<BaseObject>();

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
			multiSelect.selectionQuadFilter.mesh = Draw.GenerateQuadMesh();
			multiSelect.selectionQuadRenderer = multiSelect.selectionQuadObject.GetComponent<MeshRenderer>();
			multiSelect.selectionQuadRenderer.material = quadMat;
			multiSelect.selectionQuadRenderer.enabled = false;
		}

		public void SelectUpdate(Selection selection, Cast.CastObjects castObjects, BaseObject otherBaseObject, List<GameObject> castList, float threshold, bool select, bool disable)
		{
			castLocation = castObjects.hitPoint.transform;
			currentFocusObject = castList.CastFindObject();
			currentBaseObject = currentFocusObject.FindBaseObject(otherBaseObject);
			selectionDistance = Vector3.Distance(castLocation.transform.position, selectionStart);

			currentBaseObject.Hover(previousBaseObject, otherBaseObject, castCursor);
			
			Check.Selection(currentBaseObject, selection, this, selectionDistance,select, selectPrevious, threshold, disable);

			selectPrevious = select;
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