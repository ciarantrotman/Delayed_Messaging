using System;
using System.Collections.Generic;
using Delayed_Messaging.Scripts.Interaction;
using Delayed_Messaging.Scripts.Utilities;
using UnityEngine;
using VR_Prototyping.Scripts.Utilities;

namespace VR_Prototyping.Scripts
{
	[DisallowMultipleComponent, RequireComponent(typeof(ControllerTransforms))]
	public class Selection : MonoBehaviour
	{
		private ControllerTransforms controllerTransforms;
		private Cast cast;
		public UserInterface UserInterface { get; private set; }
		[HideInInspector] public SelectionObjects selectionObjectsL;
		[HideInInspector] public SelectionObjects selectionObjectsR;
		public enum MultiSelect
		{
			LEFT,
			RIGHT
		}
		
		[Header("Selection Settings")]
		[Range(0f, 250f)] public float selectionRange = 25f;
		[Space(10)] 
		public bool disableLeftHand;
		public bool disableRightHand;
		
		[Header("Casting Settings")]
		[SerializeField, Range(0f, 180f)] private float minimumAngle = 60f;
		[SerializeField, Range(0f, 180f)] private float maximumAngle = 110f;
		[SerializeField, Range(0f, 1f)] private float multiSelectDistanceThreshold = .2f;
		[Range(0f, 1f)] public float castSelectionRadius;

		[Header("Selection Aesthetics")]
		[SerializeField] private GameObject castCursorObject;
		[SerializeField] private Material selectionLineRenderMat;
		[SerializeField] private Material selectionQuadMaterial;

		[HideInInspector] public List<GameObject> globalList;
		[HideInInspector] public List<BaseObject> baseObjectsList;
		[HideInInspector] public List<GameObject> rCastList;
		[HideInInspector] public List<GameObject> lCastList;

		private void Start ()
		{
			controllerTransforms = GetComponent<ControllerTransforms>();
			UserInterface = GetComponent<UserInterface>();
			cast = gameObject.AddComponent<Cast>();
			
			cast.SetupCastObjects(castCursorObject, transform, "Cast Selection", selectionLineRenderMat, 
				maximumAngle, minimumAngle, selectionRange, 0.1f, .005f, controllerTransforms, 
				true);

			selectionObjectsL = gameObject.AddComponent<SelectionObjects>();
			selectionObjectsR = gameObject.AddComponent<SelectionObjects>();
			
			selectionObjectsL.SetupSelectionObjects(cast.lCastObject, MultiSelect.LEFT, selectionLineRenderMat, selectionQuadMaterial);
			selectionObjectsR.SetupSelectionObjects(cast.rCastObject, MultiSelect.RIGHT, selectionLineRenderMat, selectionQuadMaterial);

			// Set initial states
			ToggleSelectionState(UserInterface.DominantHand.LEFT, !disableLeftHand);
			ToggleSelectionState(UserInterface.DominantHand.RIGHT, !disableRightHand);
		}

		private void Update()
		{
			SortLists();
			selectionObjectsL.SelectUpdate(this, cast.lCastObject, selectionObjectsR.currentBaseObject, lCastList, multiSelectDistanceThreshold, controllerTransforms.LeftSelect(), disableLeftHand);
			selectionObjectsR.SelectUpdate(this, cast.rCastObject, selectionObjectsL.currentBaseObject, rCastList, multiSelectDistanceThreshold, controllerTransforms.RightSelect(), disableRightHand);
		}

		private void SortLists()
		{
			lCastList.Sort(SortBy.CastObjectL);
			rCastList.Sort(SortBy.CastObjectR);
		}

		public void SelectStart(SelectionObjects selectionObjects)
		{
			selectionObjects.selectionStart = selectionObjects.castLocation.position;
			selectionObjects.multiSelect.multiSelectS = selectionObjects.selectionStart;
		}
		public void SelectEnd(SelectionObjects selectionObjects)
		{
			ClearSelectedObjects(selectionObjects);
		}
		public void MultiSelectStart(SelectionObjects selectionObjects)
		{
			if (selectionObjects.multiSelect.multiSelectActive)
			{
				return;
			}
			
			UserInterface.SetObjectHeaderState(false);
			selectionObjects.multiSelect.multiSelectActive = true;
			selectionObjects.multiSelect.selectionLineRenderer.enabled = false;
			selectionObjects.multiSelect.selectionQuadRenderer.enabled = true;
			
			selectionObjects.multiSelect.multiSelectE = selectionObjects.castLocation.position;
			selectionObjects.multiSelect.selectionLineRenderer.DrawRectangularLineRenderer(selectionObjects.multiSelect.multiSelectS, selectionObjects.multiSelect.multiSelectE);
			selectionObjects.multiSelect.selectionQuadFilter.mesh.DrawQuadMesh(selectionObjects.multiSelect.multiSelectS, selectionObjects.multiSelect.multiSelectE);
			
			Bounds bounds = selectionObjects.multiSelect.selectionQuadRenderer.bounds;
			selectionObjects.multiSelect.selectionBounds = new Bounds(new Vector3(bounds.center.x, .5f, bounds.center.z), new Vector3(bounds.size.x, 1f, bounds.size.z));
		}
		public void MultiSelectHold(SelectionObjects selectionObjects)
		{
			selectionObjects.multiSelect.multiSelectE = selectionObjects.castLocation.position;
			selectionObjects.multiSelect.selectionLineRenderer.DrawRectangularLineRenderer(selectionObjects.multiSelect.multiSelectS, selectionObjects.multiSelect.multiSelectE);
			selectionObjects.multiSelect.selectionQuadFilter.mesh.DrawQuadMesh(selectionObjects.multiSelect.multiSelectS, selectionObjects.multiSelect.multiSelectE);
			
			Bounds bounds = selectionObjects.multiSelect.selectionQuadRenderer.bounds;
			selectionObjects.multiSelect.selectionBounds = new Bounds(new Vector3(bounds.center.x, .5f, bounds.center.z), new Vector3(bounds.size.x, 1f, bounds.size.z));
		}

		public void MultiSelectEnd(SelectionObjects selectionObjects)
		{
			selectionObjects.multiSelect.multiSelectActive = false;
			selectionObjects.multiSelect.multiSelectE = selectionObjects.castLocation.position;
			selectionObjects.multiSelect.selectionLineRenderer.enabled = false;
			selectionObjects.multiSelect.selectionQuadRenderer.enabled = false;
			
			Bounds bounds = selectionObjects.multiSelect.selectionQuadRenderer.bounds;
			selectionObjects.multiSelect.selectionBounds = new Bounds(bounds.center, new Vector3(bounds.size.x, 1f, bounds.size.z));
			
			ClearSelectedObjects(selectionObjects);
			foreach (BaseObject baseObject in baseObjectsList)
			{
				if (selectionObjects.multiSelect.selectionBounds.Intersects(baseObject.ObjectBounds))
				{
					Debug.Log($"{baseObject.name} was selected: {selectionObjects.side}");
					baseObject.SelectStart(selectionObjects);
				}
			}
		}
		public static void ClearSelectedObjects(SelectionObjects selectionObjects, BaseObject focusObject = null)
		{
			for (int i = 0; i < selectionObjects.list.Count; i++)
			{
				if (selectionObjects.list[i] != focusObject)
				{
					Debug.Log($"<b>{selectionObjects.list[i].name}</b> was <b>removed</b> from {selectionObjects.side} Selection List");
					selectionObjects.list[i].Deselect(selectionObjects);
					selectionObjects.list.Remove(selectionObjects.list[i]);
				}
				else
				{
					Debug.Log($"<b>{selectionObjects.list[i].name}</b> was <b>kept in</b> {selectionObjects.side} Selection List");
				}
			}
			/*
			foreach (BaseObject selectedObject in selectionObjects.list)
			{
				if (selectedObject != focusObject || focusObject == null)
				{
					Debug.Log($"<b>{selectedObject.name}</b> was <b>REMOVED</b> from {selectionObjects.side} Selection List");
					selectedObject.Deselect(selectionObjects);
					selectionObjects.list.Remove(selectedObject);
				}
			}
			*/
		}
		public void ToggleSelectionState(UserInterface.DominantHand hand, bool state)
		{
			switch (hand)
			{
				case UserInterface.DominantHand.LEFT:
					cast.lCastObject.lineRenderer.enabled = state;
					selectionObjectsL.multiSelect.selectionQuadRenderer.enabled = state;
					cast.lCastObject.visual.SetActive(state);
					disableLeftHand = !state;
					break;
				case UserInterface.DominantHand.RIGHT:
					cast.rCastObject.lineRenderer.enabled = state;
					selectionObjectsR.multiSelect.selectionQuadRenderer.enabled = state;
					cast.rCastObject.visual.SetActive(state);
					disableRightHand = !state;
					break;
				default:
					throw new ArgumentOutOfRangeException(nameof(hand), hand, null);
			}
		}
		private void OnDrawGizmos() 
		{
			if (controllerTransforms != null && controllerTransforms.debugActive)
			{
				DrawGizmos ();
			}
		}
		private void DrawGizmos()
		{
			DrawSide(selectionObjectsL, cast.lCastObject, Color.yellow);
			DrawSide(selectionObjectsR, cast.rCastObject, Color.cyan);
			
			void DrawSide(SelectionObjects selectionObjects, Cast.CastObjects castObjects, Color sideColour)
			{
				Gizmos.color = sideColour;
				Pathfinding.Util.Draw.Gizmos.CircleXZ(selectionObjects.selectionStart, .01f, sideColour);
				Pathfinding.Util.Draw.Gizmos.CircleXZ(castObjects.hitPoint.transform.position, castSelectionRadius, sideColour);
				
				Gizmos.DrawLine(selectionObjects.multiSelect.multiSelectS, selectionObjects.multiSelect.multiSelectE);
				Gizmos.DrawWireCube(selectionObjects.multiSelect.selectionBounds.center, selectionObjects.multiSelect.selectionBounds.size);
				
				if (selectionObjects.currentFocusObject == null) return;
				Gizmos.color = Color.yellow;
				Gizmos.DrawSphere(selectionObjects.currentFocusObject.transform.position, .2f);
			}
		}
	}
}