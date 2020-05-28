using System;
using System.Collections.Generic;
using System.Linq;
using Delayed_Messaging.Scripts.Objects.Units;
using Delayed_Messaging.Scripts.Player;
using Delayed_Messaging.Scripts.Utilities;
using UnityEngine;
using VR_Prototyping.Scripts;
using Gizmos = Popcron.Gizmos;

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
                targetObjects.target = null;
            }
            
            public void FindTarget(SelectionData selectionData, TargetObjects.Side side)
            {
                IEnumerable<GravityObject> targets = selectionData.targetObjects.objects;
                IEnumerable<GravityObject> gravityObjects = targets as GravityObject[] ?? targets.ToArray();
                
                if (!gravityObjects.Any()) return;
                if (selectionData.targetObjects.target == null)
                {
                    selectionData.targetObjects.target = gravityObjects.First();
                }
                
                // Find the closest and the least deviant gravity objects
                GravityObject closest = Check.FindClosest(gravityObjects, side);
                GravityObject leastDeviant = Check.FindLeastDeviant(gravityObjects, side);

                // Work out which should be the target
                bool prefClosest = closest.transform.IsPointInCone(selectionData.preferentialSelectionCone, selectionData.targetObjects.target.GetData(side));
                bool prefLeastDeviant = leastDeviant.transform.IsPointInCone(selectionData.preferentialSelectionCone, selectionData.targetObjects.target.GetData(side));

                // If both fall within the preferential cone, choose the closest
                if (prefClosest && prefLeastDeviant)
                {
                    targetObjects.target = closest;
                }
                // If neither do then choose the least deviant
                else if (!prefClosest && !prefLeastDeviant)
                {
                    // Give preference to deviance, but take into account closer objects
                    targetObjects.target = leastDeviant.GetData(side).distance <= closest.GetData(side).distance * .5f /*  */ ? leastDeviant : closest;
                }
                // Otherwise pick whichever is within the preferential selection cone
                else if (prefLeastDeviant)
                {
                    targetObjects.target = leastDeviant;
                }
                else
                {
                    targetObjects.target = closest;
                }
                
                targetObjects.target.SetTargetData(targetObjects.target.GetData(side), true);

                Debug.Log(targetObjects.target != null
                    ? $"{side}: <b>{targetObjects.target.name}</b>: {closest.name}, {prefClosest} | {leastDeviant.name}, {prefLeastDeviant}"
                    : $"{side}: No Valid Target");
            }
        }
        
        public struct TargetObjects
        {
            public List<GravityObject> objects;
            public GravityObject target;
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
            rightHand.SetConesData(controllerTransforms.RightForwardVector(), controllerTransforms.RightPosition());
            leftHand.SetConesData(controllerTransforms.LeftForwardVector(), controllerTransforms.LeftPosition());
            
            rightHand.ResetTargetObjects();
            leftHand.ResetTargetObjects();
        }

        private void LateUpdate()
        {
            // Find target objects
            rightHand.FindTarget(rightHand, TargetObjects.Side.RIGHT);
            leftHand.FindTarget(leftHand, TargetObjects.Side.LEFT);
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

            if (leftHand.targetObjects.target != null)
            {
                Gizmos.Cube(leftHand.targetObjects.target.transform.position, Quaternion.identity, new Vector3(.2f,.2f,.2f), Color.red);
                Gizmos.Line(leftCone.position, leftHand.targetObjects.target.transform.position, Color.red);
            }
            if (leftHand.targetObjects.target != null)
            {
                Gizmos.Cube(rightHand.targetObjects.target.transform.position, Quaternion.identity, new Vector3(.2f,.2f,.2f), Color.blue);
                Gizmos.Line(rightCone.position, rightHand.targetObjects.target.transform.position, Color.blue);
            }
        }
    }
}
