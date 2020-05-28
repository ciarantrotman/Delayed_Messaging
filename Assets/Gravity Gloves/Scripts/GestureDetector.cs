using System;
using Delayed_Messaging.Scripts.Player;
using UnityEngine;
using VR_Prototyping.Scripts;
using Gizmos = Popcron.Gizmos;

namespace Gravity_Gloves.Scripts
{
    [RequireComponent(typeof(ControllerTransforms), typeof(Selection))]
    public class GestureDetector : MonoBehaviour
    {
        private ControllerTransforms controllerTransforms;
        private Selection selection;
     
        [Header("Grab Gesture Detection Settings")]
        [Range(0f, 1f)] public float grabGestureDistanceThreshold = .15f;

        public GrabGesture rightGrab;
        public GrabGesture leftGrab;
        
        public struct GrabGesture
        {
            public bool previousGrabState;
            public bool currentGrabState;
            private bool detecting;

            private float startGrabTime;
            public Vector3 startGrabPosition;
            
            private float endGrabTime;
            public Vector3 endGrabPosition;

            public Vector3 grabDirection;
            public float gestureDistance;

            public void CheckState(bool current, Vector3 position)
            {
                SetCurrentState(current);
                if (currentGrabState && !previousGrabState)
                {
                    GrabStart(position);
                    return;
                }
                if (detecting && !currentGrabState && previousGrabState)
                {
                    detecting = false;
                }

                if (!detecting) return;
                if (Vector3.Distance(position, startGrabPosition) >= gestureDistance)
                {
                    endGrabTime = Time.time;
                    endGrabPosition = position;
                    GrabGestureDetected();
                }
            }
            private void SetCurrentState(bool state)
            {
                currentGrabState = state;
            }
            private void GrabStart(Vector3 position)
            {
                startGrabTime = Time.time;
                startGrabPosition = position;
                detecting = true;
            }
            private void GrabGestureDetected()
            {
                grabDirection = startGrabPosition - endGrabPosition;
            }
        }

        private void Start()
        {
            controllerTransforms = GetComponent<ControllerTransforms>();
            selection = GetComponent<Selection>();
        }

        private void Update()
        {
            rightGrab.CheckState(controllerTransforms.RightGrab(), controllerTransforms.RightPosition());
            leftGrab.CheckState(controllerTransforms.LeftGrab(), controllerTransforms.LeftPosition());
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
            Gizmos.Sphere(leftGrab.startGrabPosition, grabGestureDistanceThreshold, Color.green);
            Gizmos.Line(leftGrab.startGrabPosition, controllerTransforms.LeftPosition(), Color.green);
            
            Gizmos.Sphere(rightGrab.startGrabPosition, grabGestureDistanceThreshold, Color.yellow);
            Gizmos.Line(rightGrab.startGrabPosition, controllerTransforms.RightPosition(), Color.yellow);
        }
    }
}
