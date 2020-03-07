using Delayed_Messaging.Scripts.Interaction.Cursors;
using UnityEngine;
using VR_Prototyping.Scripts;
using VR_Prototyping.Scripts.Utilities;

namespace Delayed_Messaging.Scripts.Utilities
{
	public class Cast: MonoBehaviour
	{
		public GameObject parent;
		private static readonly int Distance = Shader.PropertyToID("_Distance");
		private ControllerTransforms controllerTransforms;
		private float maxAngle;
		private float minAngle;
		private float maxDistance;
		private float minDistance;

		public class CastObjects
		{
			public BaseCursor cursor;
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

		public void SetupCastObjects(GameObject visualPrefab, Transform castTransform, string instanceName, Material lineRendererMat, float maxA, float minA, float maxD, float minD, float lineRendererThickness, ControllerTransforms controllerTransformsReference, bool startActive = false)
		{
			controllerTransforms = controllerTransformsReference;
			maxAngle = maxA;
			minAngle = minA;
			maxDistance = maxD;
			minDistance = minD;
			
			parent = new GameObject("[" + instanceName + "/Calculations]");
			Transform parentTransform = parent.transform;
			parentTransform.SetParent(castTransform);
			SetupCastObject(lCastObject, visualPrefab, castTransform, parent.transform, instanceName + "/Left", lineRendererMat, lineRendererThickness, startActive);
			SetupCastObject(rCastObject, visualPrefab, castTransform, parent.transform, instanceName + "/Right", lineRendererMat, lineRendererThickness, startActive);
		}
		private static void SetupCastObject(CastObjects castObjects, GameObject v, Transform castTransform, Transform parentTransform, string instanceName, Material lineRendererMat, float thickness, bool enabled)
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
			castObjects.visual.SetActive(enabled);
			castObjects.cursor = castObjects.visual.GetComponent<BaseCursor>();

			if (castObjects.cursor == null)
			{
				Debug.LogWarning($"{instanceName} has no <b>BaseCursor</b> attached to {castObjects.visual.name}");
			}

			castObjects.follow.transform.SetParent(parentTransform);
			castObjects.proxy.transform.SetParent(castObjects.follow.transform);
			castObjects.normalised.transform.SetParent(castObjects.follow.transform);
			castObjects.midpoint.transform.SetParent(castObjects.proxy.transform);
			castObjects.target.transform.SetParent(castObjects.normalised.transform);
			castObjects.hitPoint.transform.SetParent(castTransform);
			castObjects.rotation.transform.SetParent(castObjects.hitPoint.transform);

			castObjects.lineRenderer = castObjects.proxy.AddComponent<LineRenderer>();
			castObjects.lineRenderer.SetupLineRender(lineRendererMat,  thickness, enabled);
		}
		private void Update()
		{
			CastUpdate(lCastObject, controllerTransforms.LeftTransform(), controllerTransforms.LeftJoystick());
			CastUpdate(rCastObject, controllerTransforms.RightTransform(), controllerTransforms.RightJoystick());
		}
		private void CastUpdate(CastObjects castObjects, Transform hand, Vector2 joystick)
		{
			castObjects.lastValidPosition = castObjects.target.LastValidPosition(castObjects.lastValidPosition);
			Set.DistanceCast(castObjects.target, castObjects.follow, castObjects.proxy, castObjects.normalised, castObjects.hitPoint, castObjects.midpoint, castObjects.rotation, castObjects.visual, 
				hand, controllerTransforms.CameraTransform(), joystick, maxAngle, minAngle,
				minDistance, maxDistance, castObjects.lastValidPosition);
			castObjects.lineRenderer.BezierLineRenderer(hand.position,castObjects.midpoint.transform.position,castObjects.hitPoint.transform.position);
			castObjects.lineRenderer.material.SetFloat(Distance, hand.TransformDistance(castObjects.hitPoint.transform));
		}
	}
}
