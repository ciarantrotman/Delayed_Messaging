using System;
using System.Collections.Generic;
using System.Linq;
using Delayed_Messaging.Scripts.Objects.Units;
using Delayed_Messaging.Scripts.Player;
using Delayed_Messaging.Scripts.Utilities;
using UnityEngine;
using VR_Prototyping.Scripts;
using Gizmos = Popcron.Gizmos;
using Side = Gravity_Gloves.Scripts.Selection.TargetObjects.Side;

namespace Gravity_Gloves.Scripts
{
    [DisallowMultipleComponent, RequireComponent(typeof(ControllerTransforms))]
    public class Selection : MonoBehaviour
    {
        private ControllerTransforms controllerTransforms;

        [Header("Selection Settings")]
        [Range(0f, 250f)] public float selectionRange = 25f;
        [SerializeField, Range(0f, 180f)] private float selectionConeAngle = 45f;
        [SerializeField, Range(0f, 90f)] private float preferentialSelectionConeAngle = 20f;

        public SelectionData rightHand;
        public SelectionData leftHand;

        public struct SelectionData
        {
            public SelectionCone primarySelectionCone;
            public SelectionCone preferentialSelectionCone;
            public TargetObjects targetObjects;
            //public GravityObject targetGravityObject;

            public void ConstructSelectionData(float primaryAngle, float preferentialAngle, float range)
            {
                primarySelectionCone.InitialiseConeData(primaryAngle, range);
                preferentialSelectionCone.InitialiseConeData(preferentialAngle, range);
                targetObjects.objects = new List<GravityObject>();
            }

            public void SetConesData(Vector3 axis, Vector3 tipPosition)
            {
                primarySelectionCone.SetConeData(axis, tipPosition);
                preferentialSelectionCone.SetConeData(axis, tipPosition);
            }

            public void ResetTargetObjects()
            {
                targetObjects.objects.Clear();
                //targetObjects.target = null;
            }
        }
        public struct TargetObjects
        {
            public List<GravityObject> objects;
            public GravityObject targetGravityObject;
            public enum Side { RIGHT, LEFT }
        }
        public struct SelectionCone
        {
            public float coneAngle;
            public float coneHeight;
            public Vector3 coneAxis;
            public Vector3 coneTipPosition;

            public void InitialiseConeData(float angle, float height)
            {
                coneAngle = angle;
                coneHeight = height;
            }
            
            public void SetConeData(Vector3 axis, Vector3 tipPosition)
            {
                coneAxis = axis;
                coneTipPosition = tipPosition;
            }

            public bool Intersection(Transform point, GravityObject.GravityObjectData gravityObjectData)
            {
                return point.IsPointInCone(this, gravityObjectData);
            }
        }
        
        private void Start()
        {
            controllerTransforms = transform.GetComponent<ControllerTransforms>();

            // Initialise Selection Data
            rightHand.ConstructSelectionData(selectionConeAngle, preferentialSelectionConeAngle, selectionRange);
            leftHand.ConstructSelectionData(selectionConeAngle, preferentialSelectionConeAngle, selectionRange);
        }

        private void Update()
        {
            // Keep the selection cones synced with controllers
            rightHand.SetConesData(controllerTransforms.RightForwardVector(), controllerTransforms.RightPosition());
            leftHand.SetConesData(controllerTransforms.LeftForwardVector(), controllerTransforms.LeftPosition());
            
            // Reset the target object information
            rightHand.ResetTargetObjects();
            leftHand.ResetTargetObjects();
        }

        private void LateUpdate()
        {
            // Find target objects, has to be in late update to make sure that all targets have been added
            rightHand.targetObjects.targetGravityObject = Check.FindTarget(rightHand, Side.RIGHT);
            leftHand.targetObjects.targetGravityObject = Check.FindTarget(leftHand, Side.LEFT);
        }

        public GravityObject GetTarget(Side side)
        {
            switch (side)
            {
                case Side.RIGHT:
                    return rightHand.targetObjects.targetGravityObject;
                case Side.LEFT:
                    return leftHand.targetObjects.targetGravityObject;
                default:
                    return null;
            }
        }

        private void OnDrawGizmos()
        {
            if (controllerTransforms == null) return;
            DrawGizmos();
        }

        private void DrawGizmos()
        {
            Transform leftCone = controllerTransforms.LeftTransform();
            Transform rightCone = controllerTransforms.RightTransform();
            
            Gizmos.Cone(leftCone.position, leftCone.rotation, selectionRange, selectionConeAngle, Color.magenta);
            Gizmos.Cone(leftCone.position, leftCone.rotation, selectionRange, preferentialSelectionConeAngle, Color.red);
            Gizmos.Cone(rightCone.position, rightCone.rotation, selectionRange, selectionConeAngle, Color.cyan);
            Gizmos.Cone(rightCone.position, rightCone.rotation, selectionRange, preferentialSelectionConeAngle, Color.blue);

            if (leftHand.targetObjects.targetGravityObject != null)
            {
                Gizmos.Cube(leftHand.targetObjects.targetGravityObject.transform.position, Quaternion.identity, new Vector3(.2f,.2f,.2f), Color.red);
                Gizmos.Line(leftCone.position, leftHand.targetObjects.targetGravityObject.transform.position, Color.red);
            }
            if (rightHand.targetObjects.targetGravityObject != null)
            {
                Gizmos.Cube(rightHand.targetObjects.targetGravityObject.transform.position, Quaternion.identity, new Vector3(.2f,.2f,.2f), Color.blue);
                Gizmos.Line(rightCone.position, rightHand.targetObjects.targetGravityObject.transform.position, Color.blue);
            }
        }
    }
}
