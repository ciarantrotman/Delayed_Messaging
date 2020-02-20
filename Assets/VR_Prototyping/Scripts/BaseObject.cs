using Delayed_Messaging.Scripts.Player;
using Delayed_Messaging.Scripts.Utilities;
using UnityEditor;
using UnityEngine;
using UnityEngine.SceneManagement;
using VR_Prototyping.Plugins.QuickOutline.Scripts;
using VR_Prototyping.Scripts.Utilities;

namespace VR_Prototyping.Scripts
{
	[DisallowMultipleComponent, CanEditMultipleObjects]
	public abstract class BaseObject : MonoBehaviour, IHoverable, ISelectable
	{
		internal GameObject playerObject;
		internal Selection selection;
		internal Player player;

		private Outline outline;
		private Vector3 defaultPosition;
		private Vector3 defaultLocalPosition;

		private bool active;
		private bool hover;
		private bool selected;

		public float AngleL { get; private set; }
		public float AngleR { get; private set; }
		public float AngleG { get; private set; }

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
			g.ToggleList(selection.gazeList, false);
			g.ToggleList(selection.lHandList, false);
			g.ToggleList(selection.rHandList, false);
		}
		private void InitialiseObject()
		{
			AssignComponents();
			SetupOutline();
			SetupSelectedVisual(transform.position);
			
			GameObject g = gameObject;
			g.ToggleList(selection.globalList, true);
			g.ToggleList(selection.globalList, true);
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
			
			selectionRing = selectionVisual.GetComponent<MeshRenderer>().material;
			selectionRing.SetFloat(BorderThickness, 0f);
			selectionRing.SetColor(SelectionColour, selectionColour);
		}
		private void Update()
		{					
			GetAngles();
			
			GameObject o = gameObject;
			o.CheckGaze(AngleG, selection.gaze, selection.gazeList, selection.lHandList, selection.rHandList, selection.globalList);
			o.ManageList(selection.lHandList, o.CheckHand(selection.gazeList, selection.manual, AngleL), selection.disableLeftHand, transform.WithinRange(selection.setSelectionRange, selection.Controller.LeftTransform(), selection.selectionRange));
			o.ManageList(selection.rHandList, o.CheckHand(selection.gazeList, selection.manual, AngleR), selection.disableRightHand, transform.WithinRange(selection.setSelectionRange, selection.Controller.RightTransform(), selection.selectionRange));
			
			ObjectUpdate();
		}

		protected virtual void ObjectUpdate()
		{
			
		}
		private void GetAngles()
		{
			Vector3 position = transform.position;
			AngleG = Vector3.Angle(position - selection.Controller.CameraPosition(), selection.Controller.CameraForwardVector());
			AngleL = Vector3.Angle(position - selection.Controller.LeftTransform().position, selection.Controller.LeftForwardVector());
			AngleR = Vector3.Angle(position - selection.Controller.RightTransform().position, selection.Controller.RightForwardVector());
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
		public virtual void SelectStart()
		{
			player.selectedObjects.Add(this);
			selected = true;
			selectionRing.SetFloat(BorderThickness, .05f);
		}
		public virtual void SelectHold()
		{

		}

		public void SelectHoldEnd()
		{
			
		}

		public virtual void QuickSelect()
		{
			
		}

		public void Deselect()
		{
			selected = false;
			
			switch (hover)
			{
				case true:
					HoverStart();
					break;
				default:
					selectionRing.SetFloat(BorderThickness, 0f);
					break;
			}
		}
	}
}
