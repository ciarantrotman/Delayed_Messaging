using System;
using Delayed_Messaging.Scripts.Player;
using Delayed_Messaging.Scripts.Utilities;
using DG.Tweening;
using UnityEditor;
using UnityEngine;
using UnityEngine.SceneManagement;
using VR_Prototyping.Plugins.QuickOutline.Scripts;
using VR_Prototyping.Scripts.Utilities;

namespace VR_Prototyping.Scripts
{
	[DisallowMultipleComponent, CanEditMultipleObjects]
	public abstract class BaseObject : MonoBehaviour, IHoverable, ISelectable<Selection.MultiSelect>
	{
		internal GameObject playerObject;
		internal ControllerTransforms controllerTransforms;
		internal Selection selection;
		internal Player player;

		private Outline outline;
		private Vector3 defaultPosition;
		private Vector3 defaultLocalPosition;

		private Vector3 lClosestPoint;
		private Vector3 rClosestPoint;

		private bool active;
		private bool hover;
		private bool selected;

		public float AngleL { get; private set; }
		public float AngleR { get; private set; }
		public float AngleG { get; private set; }
		public float CastDistanceR { get; private set; }
		public float CastDistanceL { get; private set; }

		public Bounds ObjectBounds { get; set; }

		[Header("Base Object Settings")] 
		[Header("Selection Settings")]
		public Selection.SelectionType selectionType;
		
		[Header("Hover Aesthetics")]
		[SerializeField] private Color hoverOutlineColour = new Color(0,0,0,1);
		[SerializeField, Range(0,10)] private float hoverOutlineWidth = 10f;
		[SerializeField] private Outline.Mode hoverOutlineMode = Outline.Mode.OutlineAll;

		[Header("Selection Aesthetics")]
		[SerializeField] private GameObject selectionVisual;
		[SerializeField] private float selectionScale;
		[SerializeField] private Color selectionColour = new Color(1,1,1,1);
		private Material selectionRing;
		private static readonly int BorderThickness = Shader.PropertyToID("_BorderThickness");
		private static readonly int SelectionColour = Shader.PropertyToID("_SelectionColour");

		private void Start ()
		{
			InitialiseObject();
			Initialise();
		}
		protected virtual void Initialise()
		{
			
		}
		public void OnEnable()
		{
			InitialiseObject();
		}

		private void OnDestroy()
		{
			DestroySelectableObject();
		}

		public void OnDisable()
		{
			DestroySelectableObject();
		}

		private void DestroySelectableObject()
		{
			if (selection == null) return;
			selection.ResetObjects();
			
			GameObject g = gameObject;
			g.ToggleList(selection.globalList, false);
			selection.baseObjectsList.Remove(this);
			
			g.ToggleList(selection.gazeList, false);
			g.ToggleList(selection.lHandList, false);
			g.ToggleList(selection.rHandList, false);
			
			g.ToggleList(selection.rCastList, false);
			g.ToggleList(selection.rCastList, false);
		}
		private void InitialiseObject()
		{
			AssignComponents();
			SetupOutline();
			SetupSelectedVisual(transform.position);
			
			gameObject.ToggleList(selection.globalList, true);
			selection.baseObjectsList.Add(this);
			
			selection.lDeselect.AddListener(LeftDeselect);
			selection.rDeselect.AddListener(RightDeselect);
		}
		private void AssignComponents()
		{
			if (playerObject == null)
			{
				foreach (GameObject rootGameObject in SceneManager.GetActiveScene().GetRootGameObjects())
				{
					if (rootGameObject.name != "[VR Player]") continue;
					playerObject = rootGameObject;
					Debug.Log("<b>[Base Object] </b>" + name + " player set to " + rootGameObject.name);
				}
			}
			selection = playerObject.GetComponent<Selection>();
			player = playerObject.GetComponent<Player>();
			controllerTransforms = playerObject.GetComponent<ControllerTransforms>();
		}
		private void SetupOutline()
		{
			outline = transform.AddOrGetOutline();
			outline.enabled = false;
			outline.precomputeOutline = true;
		}
		private void SetupSelectedVisual(Vector3 position)
		{
			selectionVisual = Instantiate(selectionVisual, transform);
			selectionVisual.transform.position = new Vector3(position.x, 0, position.z);
			selectionVisual.transform.localScale = new Vector3(selectionScale, selectionScale, selectionScale);
			
			selectionRing = selectionVisual.GetComponentInChildren<MeshRenderer>().material;
			selectionRing.SetFloat(BorderThickness, 0f);
			selectionRing.SetColor(SelectionColour, selectionColour);
		}
		private void Update()
		{					
			GetSortingValues();
			ObjectBounds = transform.ObjectBounds(ObjectBounds);
			
			GameObject o = gameObject;
			
			/*
			o.CheckGaze(AngleG, selection.gaze, selection.gazeList, selection.lHandList, selection.rHandList, selection.globalList);
			o.ManageList(selection.lHandList, o.WithinHandCone(selection.gazeList, selection.manual, AngleL), selection.disableLeftHand, transform.WithinRange(selection.setSelectionRange, controllerTransforms.LeftTransform(), selection.selectionRange));
			o.ManageList(selection.rHandList, o.WithinHandCone(selection.gazeList, selection.manual, AngleR), selection.disableRightHand, transform.WithinRange(selection.setSelectionRange, controllerTransforms.RightTransform(), selection.selectionRange));
			*/
			
			o.ManageList(selection.lCastList, o.WithinCastDistance(selection.globalList, selection.castSelectionRadius, CastDistanceL));
			o.ManageList(selection.rCastList, o.WithinCastDistance(selection.globalList, selection.castSelectionRadius, CastDistanceR));
			
			ObjectUpdate();
		}

		protected virtual void ObjectUpdate()
		{
			
		}

		private void GetSortingValues()
		{
			Vector3 position = transform.position;
			AngleG = Vector3.Angle(position - controllerTransforms.CameraPosition(),
				controllerTransforms.CameraForwardVector());
			AngleL = Vector3.Angle(position - controllerTransforms.LeftTransform().position,
				controllerTransforms.LeftForwardVector());
			AngleR = Vector3.Angle(position - controllerTransforms.RightTransform().position,
				controllerTransforms.RightForwardVector());

			if (selection == null) return;
			
			Vector3 castLocationL = selection.CastLocationL.position;
			Vector3 castLocationR = selection.CastLocationR.position;
			CastDistanceL = Vector3.Distance(lClosestPoint = ObjectBounds.ClosestPoint(castLocationL), castLocationL);
			CastDistanceR = Vector3.Distance(rClosestPoint = ObjectBounds.ClosestPoint(castLocationR), castLocationR);
		}
		public virtual void HoverStart()
		{
			outline.SetOutline(hoverOutlineMode, hoverOutlineWidth, hoverOutlineColour, true);
		}
		public virtual void HoverStay()
		{

		}
		public virtual void HoverEnd()
		{
			outline.SetOutline(hoverOutlineMode, hoverOutlineWidth, hoverOutlineColour, false);
		}
		public virtual void SelectStart(Selection.MultiSelect side)
		{
			switch (side)
			{
				case Selection.MultiSelect.LEFT:
					if (!player.lSelectedObjects.Contains(this))
					{
						player.lSelectedObjects.Add(this);
					}
					break;
				case Selection.MultiSelect.RIGHT:
					if (!player.rSelectedObjects.Contains(this))
					{
						player.rSelectedObjects.Add(this);
					}
					break;
				default:
					throw new ArgumentOutOfRangeException(nameof(side), side, null);
			}
			selected = true;
			selectionRing.SetFloat(BorderThickness, .05f);
		}
		public virtual void SelectHold(Selection.MultiSelect side)
		{

		}
		public void SelectHoldEnd(Selection.MultiSelect side)
		{
			
		}
		public virtual void QuickSelect(Selection.MultiSelect side)
		{
			
		}
		private void LeftDeselect()
		{
			Deselect(Selection.MultiSelect.LEFT);
		}
		private void RightDeselect()
		{
			Deselect(Selection.MultiSelect.RIGHT);
		}
		public virtual void Deselect(Selection.MultiSelect side)
		{
			Debug.Log(name + " was <b>DESELECTED</b>");
			switch (side)
			{
				case Selection.MultiSelect.LEFT when player.lSelectedObjects.Contains(this):
					player.lSelectedObjects.Remove(this);
					break;
				case Selection.MultiSelect.RIGHT when player.rSelectedObjects.Contains(this):
					player.rSelectedObjects.Remove(this);
					break;
				default:
					break;
			}
			selected = false;
			selectionRing.SetFloat(BorderThickness, 0f);
		}
		
		public virtual void DrawGizmos ()
		{
			if (selection == null)
			{
				return;
			}
			
			Gizmos.color = Color.green;
			Gizmos.DrawWireCube(ObjectBounds.center, ObjectBounds.size);
			
			Gizmos.color = Color.red;
			Vector3 position = transform.position;
			
			Gizmos.DrawLine(lClosestPoint, selection.CastLocationL.position);
			Gizmos.DrawWireSphere(lClosestPoint, .05f);
			Gizmos.DrawLine(rClosestPoint, selection.CastLocationR.position);
			Gizmos.DrawWireSphere(rClosestPoint, .05f);
			Gizmos.color = Color.black;
			Gizmos.DrawRay(position, Vector3.Normalize(selection.CastLocationR.position - position) * selection.castSelectionRadius);
			Gizmos.DrawRay(position, Vector3.Normalize(selection.CastLocationL.position - position) * selection.castSelectionRadius);

			if (hover)
			{
				Gizmos.color = hoverOutlineColour;
				Gizmos.DrawRay(position, Vector3.up);
			}
		}
	}
}
