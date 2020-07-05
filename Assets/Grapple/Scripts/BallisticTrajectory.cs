using System;
using UnityEngine;

namespace Grapple.Scripts
{
    public static class BallisticTrajectory
    {
        private const float G = 9.81f;
        [Serializable] public class BallisticTrajectoryData
        {
            // Outputs
            public Vector3 start;
            public Vector3 middle;
            public Vector3 end;
            
            // Known Variables
            public Vector3 initialVelocity;
            public float initialHeight;
            public float theta; // initial angle

            // Unknown Variables
            public float range;
            public float height;
            public float flight;
            public float terminalVelocity;
            
            // Calculation Variables
            public Vector3 thetaVector; // flattened forward vector for initial velocity
            
            // Equation Variables
            public float horizontalComponent;
            public float verticalComponent;
            public float rise;
            public float fall;
        }
        
        [Serializable] public class BallisticVariables
        {
            public Vector3 handPosition;
            public Vector3 anchorPosition;
            public float speedModifier;
            public float launchSpeed;
        }
        
        /// <summary>
        /// 
        /// </summary>
        /// <param name="variables"></param>
        /// <param name="hand"></param>
        /// <param name="anchor"></param>
        /// <param name="speed"></param>
        public static void SetBallisticVariables(this BallisticVariables variables, Vector3 hand, Vector3 anchor, float speed)
        {
            variables.handPosition = hand;
            variables.anchorPosition = anchor;
            variables.launchSpeed = speed;
            variables.speedModifier = Vector3.Distance(hand, anchor);
        }
        
        /// <summary>
        /// 
        /// </summary>
        /// <param name="data"></param>
        /// <param name="variables"></param>
        private static void SolveBallistics(this BallisticTrajectoryData data, BallisticVariables variables)
        {
            // Calculate Known Variables
            data.initialVelocity = ((variables.handPosition - variables.anchorPosition).normalized) * (variables.launchSpeed * variables.speedModifier);
            data.initialHeight = variables.handPosition.y;
            data.thetaVector = ((variables.handPosition - new Vector3(variables.anchorPosition.x, variables.handPosition.y, variables.anchorPosition.z)).normalized) * (variables.launchSpeed * variables.speedModifier); 
            data.theta = Vector3.Angle(data.initialVelocity, data.thetaVector);

            // Calculate Equation Variables
            data.horizontalComponent = (data.initialVelocity.magnitude) * Mathf.Cos(data.theta * Mathf.Deg2Rad);
            data.verticalComponent = (data.initialVelocity.magnitude) * Mathf.Sin(data.theta * Mathf.Deg2Rad);
            data.rise = data.initialVelocity.magnitude / G;

            // Solve Unknown Variables
            data.height = data.initialHeight + (data.verticalComponent * data.rise) - 0.5f * G * Mathf.Pow(data.rise, 2);
            data.fall = Mathf.Sqrt(2 * (data.height / G));
            data.flight = data.rise + data.fall;
            data.range = data.horizontalComponent * data.flight;
            data.terminalVelocity = Mathf.Sqrt(Mathf.Pow(data.horizontalComponent, 2) + Mathf.Pow((-G * data.fall), 2));
        }

        public static void Calculate(this BallisticTrajectoryData data, BallisticVariables variables)
        {
            data.SolveBallistics(variables);
        }
    }
}
