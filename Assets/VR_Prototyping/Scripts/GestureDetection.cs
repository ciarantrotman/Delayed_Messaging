using System.Collections.Generic;
using UnityEngine;

namespace VR_Prototyping.Scripts
{
    public static class GestureDetection
    {
        public static void JoystickGestureDetection(this Locomotion l, Vector2 current, Vector2 previous, float rot, float speed, float triggerValue, float toleranceValue, GameObject visual, LineRenderer lr, bool currentTouch, bool previousTouch, bool disabled, bool locomotionActive)
        {
            if (disabled) return;
            
            bool trigger = Mathf.Abs(current.x) > triggerValue || Mathf.Abs(current.y) > triggerValue;
            bool tolerance = Mathf.Abs(previous.x) - 0 <= toleranceValue && Mathf.Abs(previous.y) - 0 <= toleranceValue;
            bool triggerEnd = Mathf.Abs(current.x) - 0 <= toleranceValue && Mathf.Abs(current.y) - 0 <= toleranceValue;
            bool toleranceEnd = Mathf.Abs(previous.x) > triggerValue || Mathf.Abs(previous.y) > triggerValue;

            bool latch = trigger && toleranceEnd;
            
            if ((trigger && tolerance && !locomotionActive && !latch) || (currentTouch && !previousTouch && !locomotionActive))
            {
                if (current.x > triggerValue)
                {
                    l.RotateUser(rot, speed);
                    
                }
                else if (current.x < -triggerValue)
                {
                    l.RotateUser(-rot, speed);
                }
                else if (current.y < -triggerValue)
                {
                    l.RotateUser(180f, speed);
                }
            }
            else if ((currentTouch && previousTouch && !locomotionActive) || (trigger && toleranceEnd && !locomotionActive))
            {
                l.LocomotionStart(visual, lr);
            }
            else if ((triggerEnd && toleranceEnd && locomotionActive) || (!currentTouch && previousTouch && locomotionActive))
            {
                l.LocomotionEnd(visual, visual.transform.position, visual.transform.eulerAngles, lr);
            }
        }
        
        public static bool Grab(this Transform palm, Transform index, Transform middle, Transform ring, Transform little, float threshold)
        {
            float average = (palm.TransformDistance(index) +
                             palm.TransformDistance(middle) +
                             palm.TransformDistance(ring) +
                             palm.TransformDistance(little)) / 4;

            Vector3 position = palm.position;
            Debug.DrawRay(position, index.position - position, Color.red);
            Debug.DrawRay(position, middle.position - position, Color.yellow);
            Debug.DrawRay(position, ring.position - position, Color.green);
            Debug.DrawRay(position, little.position - position, Color.blue);
            
            return average < threshold;
        }
        
        public static bool DualSelect(this Transform thumb, Transform index, Transform middle, float threshold)
        {
            float average = (thumb.TransformDistance(index) +
                             thumb.TransformDistance(middle));
            Vector3 position = thumb.position;
            Debug.DrawRay(position, index.position - position, Color.red);
            Debug.DrawRay(position, middle.position - position, Color.blue);
            return average < threshold;
        }
        
        public static bool Select(this Transform thumb, Transform finger, float threshold)
        {
            float average = (thumb.TransformDistance(finger));
            return average < threshold;
        }

        public static bool PalmDown(this Transform palm, List<Vector3> palmDirection, float tolerance, int tracking)
        {
            Vector3 down = -palm.up;
            palmDirection.PositionTracking(down, tracking);
            Vector3 position = palm.position;
            Debug.DrawRay(position, down, Color.magenta);
            Debug.DrawRay(position, Vector3.down, Color.red);
            return Vector3.Angle(Vector3.down, palmDirection[0]) < tolerance && Vector3.Angle(Vector3.down, palmDirection[palmDirection.Count]) < tolerance;
        }
    }
}
