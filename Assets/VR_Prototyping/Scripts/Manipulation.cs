using System;
using System.Collections.Generic;
using System.Runtime.InteropServices.WindowsRuntime;
using Sirenix.OdinInspector;
using UnityEngine;

namespace VR_Prototyping.Scripts
{
	[DisallowMultipleComponent]
	[RequireComponent(typeof(ObjectSelection))]
	public class Manipulation : MonoBehaviour
	{
		#region Inspector and Variables
		private GameObject player;
		
		private ObjectSelection c;
		
		private GameObject fM;
		[HideInInspector] public GameObject mP; // dual grab pos target
		[HideInInspector] public GameObject mRp; // dual grab rot target
		[HideInInspector] public GameObject mRc; // dual grab rot target
		
		[HideInInspector] public GameObject cR; // controller proxy
		private GameObject cPr; // controller proxy
		private GameObject oPr;	// object proxy
		private GameObject cOr; // controller original
		private GameObject oOr; // object original
		private GameObject tr;  // target
		private GameObject cFr; // follow
		[HideInInspector] public GameObject tSr;

		[HideInInspector] public GameObject cL; // controller proxy
		private GameObject cPl; // controller proxy
		private GameObject oPl;	// object proxy
		private GameObject cOl; // controller original
		private GameObject oOl; // object original
		private GameObject tl;  // target
		private GameObject cFl; // follow
		[HideInInspector] public GameObject tSl;
		
		private float initialDistance;
		private float m;
		private float z;
		
		private Quaternion pRot;
		private const float Scalar = 1f;

		private LineRenderer scaleLr;	
		private Vector3 initialScale;
		private float startDistance;
		private float initialScaleFactor;
		private float minDistance;
		private float maxDistance;
		
		[HideInInspector] public SphereCollider sCl;
		[HideInInspector] public SphereCollider sCr;

		public const string RTag = "Controller/Right";
		public const string LTag = "Controller/Left";
		
		public enum ManipulationType
		{
			Physics,
			Lerp		
		}
		
		[BoxGroup("Grab Settings")] public ManipulationType manipulationType;
		[BoxGroup("Grab Settings")] public bool directGrab;
		[BoxGroup("Grab Settings")] [ShowIf("directGrab")] [Indent] [Range(0f, 1f)] [SerializeField] private float directGrabDistance = .5f;
		[BoxGroup("Grab Settings")] public bool disableLeftGrab;
		[BoxGroup("Grab Settings")] public bool disableRightGrab;
		
		[BoxGroup("Rotation Settings")] public bool enableRotation;
		[BoxGroup("Rotation Settings")] [ShowIf("enableRotation")] [Indent] [Range(1f, 10f)] public float rotationForce;
		
		[BoxGroup("Scaling Settings")] public bool enableScaling;
		[BoxGroup("Scaling Settings")] [ShowIf("enableScaling")] [Indent] [Range(.001f, .005f)] public float lineRendererThickness;
		
		[BoxGroup("Snapping")] [SerializeField] private bool distanceSnapping = true;
		[BoxGroup("Snapping")] [ShowIf("distanceSnapping")] [Indent] [Range(0f, 5f)] [SerializeField] private float snapDistance = 1f;
		[BoxGroup("Snapping")] [ShowIf("distanceSnapping")] [Indent] [SerializeField] private bool maximumDistance;
		
		#endregion
		private void Start () 
		{
			c = GetComponent<ObjectSelection>();
			player = c.gameObject;
			SetupGameObjects();
		}

		private void SetupGameObjects()
		{
			fM = new GameObject("Manipulation/Manipulation");
			mP = new GameObject("Manipulation/MidPoint/Position");
			mRc = new GameObject("Manipulation/MidPoint/Rotation");
			mRp = new GameObject("Manipulation/MidPoint/Rotation");
			
			tr = new GameObject("Manipulation/Target/Right");
			cFr = new GameObject("Manipulation/Controller/Follow/Right");
			cOr = new GameObject("Manipulation/Controller/Original/Right");
			cPr = new GameObject("Manipulation/Controller/Proxy/Right");
			tSr = new GameObject("Manipulation/Target/Scaled/Right");
			oPr = new GameObject("Manipulation/Object/Proxy/Right");
			oOr = new GameObject("Manipulation/Object/Original/Right");
			
			fM.transform.parent = player.transform;
			
			mP.transform.SetParent(fM.transform);
			mRc.transform.SetParent(fM.transform);
			mRp.transform.SetParent(fM.transform);
			
			tr.transform.SetParent(fM.transform);
			cFr.transform.SetParent(fM.transform);
			cOr.transform.SetParent(cFr.transform);
			cPr.transform.SetParent(cFr.transform);
			tSr.transform.SetParent(cPr.transform);
			oPr.transform.SetParent(cPr.transform);
			oOr.transform.SetParent(cPr.transform);
			
			tl = new GameObject("Manipulation/Target/Left");
			cFl = new GameObject("Manipulation/Controller/Follow/Left");
			cOl = new GameObject("Manipulation/Controller/Original/Left");
			cPl = new GameObject("Manipulation/Controller/Proxy/Left");
			tSl = new GameObject("Manipulation/Target/Scaled/Left");
			oPl = new GameObject("Manipulation/Object/Proxy/Left");
			oOl = new GameObject("Manipulation/Object/Original/Left");
			
			tl.transform.SetParent(fM.transform);
			cFl.transform.SetParent(fM.transform);
			cOl.transform.SetParent(cFl.transform);
			cPl.transform.SetParent(cFl.transform);
			tSl.transform.SetParent(cPl.transform);
			oPl.transform.SetParent(cPl.transform);
			oOl.transform.SetParent(cPl.transform);
			
			cR = new GameObject(RTag);
			cL = new GameObject(LTag);
			cR.transform.SetParent(fM.transform);
			cL.transform.SetParent(fM.transform);
			
			//SetupLineRender();
			
			if(!directGrab) return;
			sCr = cR.AddComponent<SphereCollider>();
			sCl = cL.AddComponent<SphereCollider>();
			sCr.SetupSphereCollider(true, directGrabDistance);
			sCl.SetupSphereCollider(true, directGrabDistance);
		}

		private void SetupLineRender()
		{
			//scaleLr = fM.AddComponent<LineRenderer>();
			//Setup.LineRender(scaleLr, c.Controller.lineRenderMat, lineRendererThickness, false);
		}
		
		private void Update()
		{				
			c.Controller.CameraTransform().SplitPosition(c.Controller.LeftTransform(), cFl.transform);
			c.Controller.CameraTransform().SplitPosition(c.Controller.RightTransform(), cFr.transform);
			mP.transform.MidpointPosition(tSl.transform, tSr.transform, true);
			cR.transform.Transforms(c.Controller.RightTransform());
			cL.transform.Transforms(c.Controller.LeftTransform());
			
			FollowFocusObjects();
		}

		private void FollowFocusObjects()
		{
			if (c.LFocusObject != null)
			{
				c.LFocusObject.transform.FocusObjectFollow(c.Controller.LeftTransform(), tl.transform, tSl.transform, oOl.transform, cOl.transform, oPl.transform, c.Controller.LeftGrab());
			}

			if (c.RFocusObject != null)
			{
				c.RFocusObject.transform.FocusObjectFollow(c.Controller.RightTransform(), tr.transform, tSr.transform, oOr.transform, cOr.transform, oPr.transform, c.Controller.RightGrab());
			}
		}
		public void OnStart(Transform con)
		{
			switch (con == c.Controller.LeftTransform())
			{
				case true:
					cFl.GrabStart(cPl, tl, cOl, con);
					tl.transform.Transforms(c.LFocusObject.transform);
					tSl.transform.Transforms(tl.transform);
					oPl.transform.Position(c.LFocusObject.transform);
					oOl.transform.Position(c.LFocusObject.transform);
					break;
				case false:
					cFr.GrabStart(cPr, tr, cOr, con);
					tr.transform.Transforms(c.RFocusObject.transform);
					tSr.transform.Transforms(tr.transform);
					oPr.transform.Position(c.RFocusObject.transform);
					oOr.transform.Position(c.RFocusObject.transform);
					break;
				default:
					throw new ArgumentException();
			}
		}

		public void OnStay(Transform con)
		{
			switch (con == c.Controller.LeftTransform())
			{
				case true:
					ControllerFollowing(con, cFl, cPl, tl);
					tSl.transform.localPosition = new Vector3(0, 0, cPl.MagnifiedDepth(cOl, oOl, tSl, snapDistance, c.selectionRange - c.selectionRange * .25f, maximumDistance));
					break;
				case false:
					ControllerFollowing(con, cFr, cPr, tr);
					tSr.transform.localPosition = new Vector3(0, 0, cPr.MagnifiedDepth(cOr, oOr, tSr, snapDistance, c.selectionRange - c.selectionRange * .25f, maximumDistance));
					break;
				default:
					throw new ArgumentException();
			}
		}

		public void DualGrabStart(Transform target, bool rot, bool sca, float max, float min, Vector3 scaleMax, Vector3 scaleMin)
		{
			if (rot)
			{
				mRp.transform.MidpointPosition(c.Controller.LeftTransform(), c.Controller.RightTransform(), true);
				pRot = mRp.transform.rotation;
				mRc.transform.rotation = target.rotation;
				mRc.transform.position = mRp.transform.position;
			}
			if (sca)
			{
				initialScale = target.localScale;
				initialScaleFactor = Mathf.InverseLerp(scaleMin.x, scaleMax.x, initialScale.x);
				startDistance = c.Controller.ControllerDistance();

				if (Math.Abs(initialScaleFactor) < float.Epsilon)
				{
					maxDistance = startDistance * max;
					minDistance = startDistance;
				}
				else
				{
					minDistance = startDistance * min;
					maxDistance = ((startDistance - minDistance) / initialScaleFactor) + minDistance;
				}
				
				if(scaleLr == null) return;
				scaleLr.enabled = true;
			}
		}

		public void DualGrabStay(Rigidbody rb, Transform target, bool rot, bool sca, Vector3 scaleMin, Vector3 scaleMax)
		{
			if (rot && enableRotation)
			{
				mRp.transform.MidpointPosition(c.Controller.LeftTransform(), c.Controller.RightTransform(), true);
			
				Quaternion rotation = mRp.transform.rotation;
			
				Quaternion aRot = pRot * Quaternion.Inverse(rotation);
				mRc.transform.rotation = Quaternion.Inverse(Quaternion.LerpUnclamped(Quaternion.identity, aRot, Scalar)) * mRc.transform.rotation;
				mRc.transform.position = mRp.transform.position;

				rb.AddTorque(Vector3.Cross(rb.transform.forward, mRc.transform.forward) * rotationForce, ForceMode.Acceleration);
				rb.AddTorque(Vector3.Cross(rb.transform.right, mRc.transform.right) * rotationForce, ForceMode.Acceleration);
				rb.AddTorque(Vector3.Cross(rb.transform.up, mRc.transform.up) * rotationForce, ForceMode.Acceleration);

				pRot = rotation;	
			}
			if (sca && enableScaling)
			{
				float scaleFactor = Mathf.InverseLerp(minDistance, maxDistance, c.Controller.ControllerDistance());
            
				scaleFactor = scaleFactor <= 0 ? 0 : scaleFactor;
				scaleFactor = scaleFactor >= 1 ? 1 : scaleFactor;
            
				target.transform.localScale = Vector3.Lerp(scaleMin, scaleMax, scaleFactor);
				
				if(scaleLr == null) return;
				scaleLr.StraightLineRender(c.Controller.LeftTransform(), c.Controller.RightTransform());
			}
		}

		public void DualGrabEnd()
		{
			if(scaleLr == null) return;
			scaleLr.enabled = false;
		}
		
		public static void DirectGrabStart(Rigidbody rigid, Transform target, Transform controller)
		{
			rigid.useGravity = false;
			rigid.velocity = Vector3.zero;
			target.SetParent(controller);
		}
		
		public static void DirectGrabEnd(Rigidbody rigid, Transform target, Transform originalParent, bool g, List<Vector3> pos, List<Vector3> rot, float f, LineRenderer lr)
		{
			rigid.useGravity = g;
			target.SetParent(originalParent);
			rigid.AddForce(pos.Velocity() * (f + f), ForceMode.VelocityChange);
			rigid.AddTorque(rot.AngularVelocity() * (f + f), ForceMode.VelocityChange);
			lr.enabled = true;
		}
		
		public void OnEnd(Transform con)
		{
			DualGrabEnd();
			switch (con == c.Controller.LeftTransform())
			{
				case true:
					tl.transform.SetParent(null);
					break;
				case false:
					tr.transform.SetParent(null);
					break;
				default:
					throw new ArgumentException();
			}
		}
		
		private static void ControllerFollowing(Transform con, GameObject f, GameObject p, GameObject target)
		{
			f.transform.LookAt(con);
			p.transform.position = con.position;
			p.transform.LookAt(target.transform);
		}
	}
}