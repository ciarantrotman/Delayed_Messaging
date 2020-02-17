using System;
using Pathfinding;
using UnityEngine;
using VR_Prototyping.Scripts;
using VR_Prototyping.Scripts.Utilities;

namespace Delayed_Messaging.Scripts.Units
{
    public class Unit : MonoBehaviour
    {
        public UnitClass unitClass;
        public UnitClass.UnitData unitData;

        // 
        private AIDestinationSetter destinationSetter;
        private AIPath aiPath;
        
        private void Start()
        {
            InitialiseUnit(unitClass);
        }

        public void InitialiseUnit(UnitClass u)
        {
            unitClass = u;
            aiPath = transform.AddOrGetAIPath();
            destinationSetter = transform.AddOrGetAIDestinationSetter();

            aiPath.maxSpeed = unitClass.moveSpeed;
        }

        #region Gizmos
        private void OnDrawGizmos () 
        {
            if (unitClass == null)
            {
                return;
            }
            if (unitClass.unitDebugType == UnitClass.DebugType.ALWAYS)
            {
                DrawGizmos ();
            }
        }

        private void OnDrawGizmosSelected ()
        {
            if (unitClass == null)
            {
                return;
            }
            if (unitClass.unitDebugType == UnitClass.DebugType.SELECTED_ONLY)
            {
                DrawGizmos ();
            }
        }

        private void DrawGizmos ()
        {
            if (unitClass == null)
            {
                return;
            }
            
            // Cache
            Transform t = transform;
            Vector3 pos = t.position;
            Vector3 r = t.right;
            
            // Sphere Radius
            Gizmos.color = unitClass.troupeCenterColour;
            Gizmos.DrawSphere(unitData.troupeCenter, unitClass.unitRadius);

            // Vectors
            Gizmos.color = unitClass.forwardVectorColour;
            Gizmos.DrawRay(pos, r.normalized);
        }
        #endregion
    }
}
