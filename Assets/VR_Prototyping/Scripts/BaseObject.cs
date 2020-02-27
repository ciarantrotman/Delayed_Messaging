using System;
using Delayed_Messaging.Scripts;
using Delayed_Messaging.Scripts.Player;
using Delayed_Messaging.Scripts.Utilities;
using UnityEditor;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.VFX;
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
		private bool initialised;

		internal VisualEffect selectionVisualEffect;
		internal float health;
			
		public float AngleL { get; private set; }
		public float AngleR { get; private set; }
		public float AngleG { get; private set; }
		public float CastDistanceR { get; private set; }
		public float CastDistanceL { get; private set; }

		public Bounds ObjectBounds { get; set; }

		private const string SelectEvent = "Select";
		private const string DeselectEvent = "Deselect";

		public BaseClass ObjectClass { get; set; }

		[Header("Base Object Settings")] 
		[SerializeField] private ControllerTransforms.DebugType debugType;
		
		[Header("Selection Settings")]
		public Selection.SelectionType selectionType;
		
		[Header("Hover Aesthetics")]
		[SerializeField] private Color hoverOutlineColour = new Color(0,0,0,1);
		[SerializeField, Range(0,10)] private float hoverOutlineWidth = 10f;
		[SerializeField] private Outline.Mode hoverOutlineMode = Outline.Mode.OutlineAll;

		[Header("Selection Aesthetics")] 
		[SerializeField] private GameObject selectionVisual;
		[SerializeField] internal float selectionRadius;

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
			DestroyBaseObject();
		}

		public void OnDisable()
		{
			DestroyBaseObject();
		}

		private void DestroyBaseObject()
		{
			if (selection == null) return;
			selection.ResetObjects();

			initialised = false;
			
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
			Debug.Log("<b>[BASE OBJECT]</b> " + name + " attempting to initialise");
			if (initialised)
			{
				Debug.LogError("<b>[BASE OBJECT]</b> " + name + " tried to initialise, but had already been initialised");
				return;
			}
			AssignComponents();
			SetupOutline();
			SetupSelectedVisual();

			//health = objectClass.healthMax;
			
			gameObject.ToggleList(selection.globalList, true);
			selection.baseObjectsList.Add(this);
			initialised = true;
			Debug.Log("<b>[BASE OBJECT]</b> " + name + " initialised");
		}
		private void AssignComponents()
		{
			if (playerObject == null)
			{
				foreach (GameObject rootGameObject in SceneManager.GetActiveScene().GetRootGameObjects())
				{
					if (rootGameObject.name != "[VR Player]") continue;
					playerObject = rootGameObject;
					Debug.Log("<b>[BASE OBJECT] </b>" + name + " player set to " + rootGameObject.name);
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
		private void SetupSelectedVisual()
		{
			selectionVisual = Instantiate(selectionVisual, transform);
			selectionVisualEffect = selectionVisual.GetComponent<VisualEffect>();
			selectionVisualEffect.SetFloat("SelectionRadius", selectionRadius);
			//selectionVisualEffect.SetFloat("Health", Mathf.InverseLerp(0, objectClass.healthMax, health));
		}
		private void Update()
		{					
			GetSortingValues();
			
			GameObject o = gameObject;
			Transform t = transform;
			
			ObjectBounds = t.BoundsOfChildren(ObjectBounds);
			//selectionVisualEffect.SetFloat("Health", Mathf.InverseLerp(0, objectClass.healthMax, health));

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
		private void SelectionVisual(string eventName)
		{
			selectionVisualEffect.SendEvent(eventName);
		}
		public virtual void HoverStart()
		{
			//outline.SetOutline(hoverOutlineMode, hoverOutlineWidth, hoverOutlineColour, true);
		}
		public virtual void HoverStay()
		{

		}
		public virtual void HoverEnd()
		{
			//outline.SetOutline(hoverOutlineMode, hoverOutlineWidth, hoverOutlineColour, false);
		}
		public virtual void SelectStart(Selection.MultiSelect side)
		{
			SelectionVisual(SelectEvent);
			selected = true;
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
					break;
			}
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
		public virtual void Deselect(Selection.MultiSelect side)
		{
			SelectionVisual(DeselectEvent);
			selected = false;
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
		}
		
		private void OnDrawGizmos () 
		{
			if (debugType == ControllerTransforms.DebugType.ALWAYS)
			{
				DrawGizmos ();
			}
		}
		private void OnDrawGizmosSelected ()
		{
			if (debugType == ControllerTransforms.DebugType.SELECTED_ONLY)
			{
				DrawGizmos ();
			}
		}

		protected virtual void DrawGizmos ()
		{
			if (selection != null)
			{		
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
			}
		}
	}
}
