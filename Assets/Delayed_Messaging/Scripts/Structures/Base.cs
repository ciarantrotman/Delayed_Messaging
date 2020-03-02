using System.Collections;
using System.Collections.Generic;
using Delayed_Messaging.Scripts.Structures;
using Pathfinding.Util;
using UnityEngine;
using VR_Prototyping.Scripts;

public class Base : Structure
{
        public Transform spawnOrigin;
        public Transform spawnDestination;

        public GameObject unit;

        public override void QuickSelect(Selection.MultiSelect side, List<BaseObject> list)
        {
            /*
            Instantiate(unit);
            unit.transform.position = spawnOrigin.position;
            unit.transform.forward = transform.forward;
            Unit unitUnit = unit.GetComponent<Unit>();
            unitUnit.InitialiseUnit();
            unitUnit.SetDestination(spawnDestination.position);
            */
            
            base.QuickSelect(side, list);
        }

        protected override void DrawGizmos ()
        {
            if (structureClass == null)
            {
                return;
            }
            
            base.DrawGizmos();
            
            // Cache
            Vector3 o = spawnOrigin.position;
            Vector3 d = spawnDestination.position;
            Draw.Gizmos.CircleXZ(o, .1f, structureClass.spawnLocationColour);
            Draw.Gizmos.CircleXZ(d, .1f, structureClass.spawnLocationColour);
            Gizmos.DrawLine(o, d);
        }
}
