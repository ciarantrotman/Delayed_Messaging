using Delayed_Messaging.Scripts.Player;
using UnityEngine;
using Gizmos = Popcron.Gizmos;
using Side = Gravity_Gloves.Scripts.Selection.TargetObjects.Side;

namespace Gravity_Gloves.Scripts
{
    [RequireComponent(typeof(ControllerTransforms), typeof(Selection))]
    public class GestureDetector : MonoBehaviour
    {
        private ControllerTransforms controllerTransforms;
        private Selection selection;

        private const float GrabGestureDistanceThreshold = .125f;
        private const float GrabGestureDetectionTimeout = 2f;

        private const float CatchGestureDistanceThreshold = .35f;

        public GrabGesture rightGrab;
        public GrabGesture leftGrab;
        
        public struct GrabGesture
        {
            public bool previousGrabState;
            public bool currentGrabState;
            private bool detecting;
            private bool catching;

            private float startGrabTime;
            public Vector3 startGrabPosition;
            private Vector3 endGrabPosition;
            private Vector3 grabDirection;

            private GravityObject targetGravityObject;

            public void CheckState(bool current, Vector3 position, Side side, Selection selection, Transform hand)
            {
                // Set the current grab states
                SetCurrentState(current);

                if (catching)
                {
                    if (currentGrabState && !previousGrabState && Vector3.Distance(position, targetGravityObject.transform.position) < CatchGestureDistanceThreshold)
                    {
                        targetGravityObject.WarpToHand(hand);
                        catching = false;
                    }
                }
                
                switch (detecting)
                {
                    case false when currentGrabState && !previousGrabState:
                        // When you grab
                        GrabStart(position, selection, side);
                        return;
                    case true when !currentGrabState && previousGrabState && Vector3.Distance(position, startGrabPosition) < GrabGestureDistanceThreshold:
                        // You let go and you haven't triggered the gesture
                        detecting = false;
                        return;
                    case true when Time.time - startGrabTime >= GrabGestureDetectionTimeout:
                        // The gesture detection has timed out and you haven't triggered the gesture
                        detecting = false;
                        return;
                    case true when Vector3.Distance(position, startGrabPosition) >= GrabGestureDistanceThreshold:
                        GrabGestureDetected(position);
                        return;
                    default:
                        return;
                }
            }
            private void SetCurrentState(bool state)
            {
                currentGrabState = state;
            }
            private void GrabStart(Vector3 position, Selection selection, Side side)
            {
                if (selection.GetTarget(side) == null) return;
                startGrabTime = Time.time;
                startGrabPosition = position;
                detecting = true;
                targetGravityObject = selection.GetTarget(side);
            }
            private void GrabGestureDetected(Vector3 position)
            {
                endGrabPosition = position;
                grabDirection = endGrabPosition - startGrabPosition;
                detecting = false;
                targetGravityObject.LaunchGravityObject(grabDirection.normalized, endGrabPosition);
                catching = true;
            }
        }

        private void Start()
        {
            controllerTransforms = GetComponent<ControllerTransforms>();
            selection = GetComponent<Selection>();
        }

        private void Update()
        {
            rightGrab.CheckState(controllerTransforms.RightGrab(), controllerTransforms.RightPosition(), Side.RIGHT, selection, controllerTransforms.RightTransform());
            leftGrab.CheckState(controllerTransforms.LeftGrab(), controllerTransforms.LeftPosition(), Side.LEFT, selection, controllerTransforms.LeftTransform());
        }

        private void LateUpdate()
        {
            SetPreviousState();
        }

        private void SetPreviousState()
        {
            rightGrab.previousGrabState = rightGrab.currentGrabState;
            leftGrab.previousGrabState = leftGrab.currentGrabState;
        }
        
        private void OnDrawGizmos()
        {
            if (controllerTransforms == null) return;
            DrawGizmos();
        }

        private void DrawGizmos()
        {
            Gizmos.Sphere(leftGrab.startGrabPosition, GrabGestureDistanceThreshold, Color.green);
            Gizmos.Line(leftGrab.startGrabPosition, controllerTransforms.LeftPosition(), Color.green);
            
            Gizmos.Sphere(rightGrab.startGrabPosition, GrabGestureDistanceThreshold, Color.yellow);
            Gizmos.Line(rightGrab.startGrabPosition, controllerTransforms.RightPosition(), Color.yellow);
        }
    }
}
