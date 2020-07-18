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
            public Vector3 start, middle, end;

            // Known Variables
            public Vector3 initialVelocity;
            public float initialHeight, theta;

            // Unknown Variables
            public float range, height, flight, terminalVelocity;

            // Calculation Variables
            public Vector3 thetaVector; // flattened forward vector for initial velocity
            
            // Equation Variables
            public float horizontalComponent, verticalComponent, rise, fall;
        }
        
        [Serializable] public class BallisticVariables
        {
            public Vector3 handPosition, anchorPosition;
            public float speedModifier, launchSpeed;
        }
        
        /// <summary>
        /// Sets the defining variables for the ballistic curve
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
        /// Calculates all required variables based on the input variables
        /// </summary>
        /// <param name="data"></param>
        /// <param name="variables"></param>
        private static void SolveBallistics(this BallisticTrajectoryData data, BallisticVariables variables)
        {
            // Calculate Known Variables
            data.initialVelocity = ((variables.handPosition - variables.anchorPosition).normalized) * (variables.launchSpeed * Mathf.Pow(variables.speedModifier, 2));
            data.initialHeight = variables.anchorPosition.y;
            data.thetaVector = (variables.handPosition - new Vector3(variables.anchorPosition.x, variables.handPosition.y, variables.anchorPosition.z)).normalized; 
            data.theta = Vector3.Angle(data.initialVelocity.normalized, data.thetaVector.normalized) * Mathf.Deg2Rad;
            
            // Calculate Equation Variables
            data.horizontalComponent = (data.initialVelocity.magnitude) * Mathf.Cos(data.theta);
            data.verticalComponent = (data.initialVelocity.magnitude) * Mathf.Sin(data.theta);
            data.rise = data.initialVelocity.magnitude / G;

            // Solve Unknown Variables
            data.height = data.initialHeight + (data.verticalComponent * data.rise) - 0.5f * G * Mathf.Pow(data.rise, 2);
            data.fall = Mathf.Sqrt(2 * (data.height / G));
            data.flight = data.rise + data.fall;
            data.range = data.horizontalComponent * data.flight;
            data.terminalVelocity = Mathf.Sqrt(Mathf.Pow(data.horizontalComponent, 2) + Mathf.Pow((-G * data.fall), 2));
        }
        /// <summary>
        /// Public wrapper to set input variables and make calculations 
        /// </summary>
        /// <param name="data"></param>
        /// <param name="variables"></param>
        public static void Calculate(this BallisticTrajectoryData data, BallisticVariables variables)
        {
            data.SolveBallistics(variables);
        }
    }
}
