using Delayed_Messaging.Scripts.Utilities;
using UnityEditor;
using UnityEngine;
using UnityEngine.SceneManagement;
using VR_Prototyping.Plugins.QuickOutline.Scripts;
using VR_Prototyping.Scripts.Utilities;

namespace VR_Prototyping.Scripts
{
	[DisallowMultipleComponent, CanEditMultipleObjects]
	public abstract class BaseObject : MonoBehaviour, ISelectable
	{
		internal GameObject player;
		internal ObjectSelection objectSelection;

		private Outline outline;
		private Vector3 defaultPosition;
		private Vector3 defaultLocalPosition;

		private bool active;

		public float AngleL { get; private set; }
		public float AngleR { get; private set; }
		public float AngleG { get; private set; }
		
		[Header("Base Object Settings")]
		[SerializeField] private Color hoverOutlineColour = new Color(0,0,0,1);
		[SerializeField, Range(0,10)] private float hoverOutlineWidth;
		[SerializeField] private Outline.Mode hoverOutlineMode;
			
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
			if (objectSelection == null) return;
			objectSelection.ResetObjects();
			GameObject g = gameObject;
			g.ToggleList(objectSelection.globalList, false);
			g.ToggleList(objectSelection.gazeList, false);
			g.ToggleList(objectSelection.lHandList, false);
			g.ToggleList(objectSelection.rHandList, false);
		}
		private void InitialiseObject()
		{
			AssignComponents();
			SetupOutline();
			GameObject g = gameObject;
			g.ToggleList(objectSelection.globalList, true);
			g.ToggleList(objectSelection.globalList, true);
		}
		private void AssignComponents()
		{
			if (player == null)
			{
				foreach (GameObject rootGameObject in SceneManager.GetActiveScene().GetRootGameObjects())
				{
					if (rootGameObject.name != "[VR Player]") continue;
					player = rootGameObject;
					Debug.Log(name + " player set to " + rootGameObject.name);
				}
			}
			objectSelection = player.GetComponent<ObjectSelection>();
		}
		private void SetupOutline()
		{
			outline = transform.AddOrGetOutline();
			outline.enabled = false;
			outline.precomputeOutline = true;
		}
		private void Update()
		{					
			GetAngles();
			
			GameObject o = gameObject;
			o.CheckGaze(AngleG, objectSelection.gaze, objectSelection.gazeList, objectSelection.lHandList, objectSelection.rHandList, objectSelection.globalList);
			o.ManageList(objectSelection.lHandList, o.CheckHand(objectSelection.gazeList, objectSelection.manual, AngleL), objectSelection.disableLeftHand, transform.WithinRange(objectSelection.setSelectionRange, objectSelection.Controller.LeftTransform(), objectSelection.selectionRange));
			o.ManageList(objectSelection.rHandList, o.CheckHand(objectSelection.gazeList, objectSelection.manual, AngleR), objectSelection.disableRightHand, transform.WithinRange(objectSelection.setSelectionRange, objectSelection.Controller.RightTransform(), objectSelection.selectionRange));
			
			ObjectUpdate();
		}

		protected virtual void ObjectUpdate()
		{
			
		}
		private void GetAngles()
		{
			Vector3 position = transform.position;
			AngleG = Vector3.Angle(position - objectSelection.Controller.CameraPosition(), objectSelection.Controller.CameraForwardVector());
			AngleL = Vector3.Angle(position - objectSelection.Controller.LeftTransform().position, objectSelection.Controller.LeftForwardVector());
			AngleR = Vector3.Angle(position - objectSelection.Controller.RightTransform().position, objectSelection.Controller.RightForwardVector());
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
			
		}
		public virtual void SelectStay()
		{

		}
		public virtual void SelectEnd()
		{
			
		}
	}
}
