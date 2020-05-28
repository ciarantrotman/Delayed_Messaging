using System;
using Delayed_Messaging.Scripts.Utilities;
using UnityEngine;
using UnityEngine.SceneManagement;
using VR_Prototyping.Plugins.QuickOutline.Scripts;
using Side = Gravity_Gloves.Scripts.Selection.TargetObjects.Side;

namespace Gravity_Gloves.Scripts
{
    [RequireComponent(typeof(Rigidbody))]
    public class GravityObject : MonoBehaviour
    {
        private Selection selection;
        private Outline outline;
        private Rigidbody gravityRigidBody;

        public GravityObjectData leftHandData;
        public GravityObjectData rightHandData;
        
        public struct GravityObjectData
        {
            public float deviation;
            public float distance;
            public bool isTarget;

            public void SetData(float dev, float dis)
            {
                deviation = dev;
                distance = dis;
            }
        }
        
        public void SetTargetData(Side side, bool state)
        {
            switch (side)
            {
                case Side.RIGHT:
                    rightHandData.isTarget = state;
                    break;
                case Side.LEFT:
                    leftHandData.isTarget = state;
                    break;
                default:
                    break;
            }
            ToggleOutline(state);
        }
        
        public GravityObjectData GetData(Side side)
        {
            switch (side)
            {
                case Side.RIGHT:
                    return rightHandData;
                case Side.LEFT:
                    return leftHandData;
                default:
                    return new GravityObjectData();
            }
        }
        
        private void Start()
        {
            AssignComponents();
            SetupOutline();
        }

        private void Update()
        {
            ResetTargetState();
            
            CheckIntersection(selection.leftHand, leftHandData);
            CheckIntersection(selection.rightHand, rightHandData);
        }

        private void CheckIntersection(Selection.SelectionData selectionData, GravityObjectData data)
        {
            if (!selectionData.primarySelectionCone.Intersection(transform, data)) return;
            selectionData.targetObjects.objects.Add(this);
        }

        public void LaunchGravityObject(Vector3 vector, float force = 20f)
        {
            Vector3 f = vector * force;
            gravityRigidBody.AddForce(f, ForceMode.Impulse);
        }

        private void ToggleOutline(bool state)
        {
            outline.enabled = state;
        }

        private void AssignComponents()
        {
            if (selection != null) return;
            foreach (GameObject rootGameObject in SceneManager.GetActiveScene().GetRootGameObjects())
            {
                if (rootGameObject.name != "[Experimental Player]") continue;
                selection = rootGameObject.GetComponent<Selection>();
            }

            gravityRigidBody = GetComponent<Rigidbody>();
            
            if (selection != null) return;
            Debug.LogError("No <b>Selection Component</b> was Assigned");
        }
        private void SetupOutline()
        {
            outline = transform.AddOrGetOutline();
            outline.OutlineWidth = 10;
            outline.OutlineColor = Color.white;
            outline.enabled = false;
            outline.precomputeOutline = true;
        }

        private void ResetTargetState()
        {
            leftHandData.isTarget = false;
            rightHandData.isTarget = false;
            outline.enabled = false;
        }
    }
}
