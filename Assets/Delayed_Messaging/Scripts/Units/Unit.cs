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
            if (unitClass.debugType == BaseClass.DebugType.ALWAYS)
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
            if (unitClass.debugType == BaseClass.DebugType.SELECTED_ONLY)
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
            
            // Object
            Gizmos.color = Color.black;
            Gizmos.DrawWireCube(pos, new Vector3(
                unitClass.size.x,
                unitClass.size.y,
                unitClass.size.z));

            // Debug
            if (destinationSetter != null && destinationSetter.target != null)
            {
                Gizmos.color = unitClass.destinationColour;
                Gizmos.DrawWireSphere(destinationSetter.target.position, 1f);
            }
            
            // Vectors
            Gizmos.color = unitClass.forwardVectorColour;
            Gizmos.DrawRay(pos, r.normalized);
        }
        #endregion
    }
}
