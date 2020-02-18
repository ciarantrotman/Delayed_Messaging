using System;
using Delayed_Messaging.Scripts.Utilities;
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
            aiPath.SetupAIPath(unitClass);
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
            Vector3 r = t.forward;
            
            // Sphere Radius
            Gizmos.color = unitClass.troupeCenterColour;
            Gizmos.DrawWireCube(pos, new Vector3(
                unitClass.unitRadius,
                unitClass.unitHeight,
                unitClass.unitRadius));

            // Vectors
            Gizmos.color = unitClass.forwardVectorColour;
            Gizmos.DrawRay(pos, r.normalized);
        }
        #endregion
    }
}
