using Delayed_Messaging.Scripts.Units;
using Delayed_Messaging.Scripts.Utilities;
using UnityEngine;
using VR_Prototyping.Scripts;

namespace Delayed_Messaging.Scripts.Structures
{
    public class Structure : BaseObject, IDamageable<float>
    {
        [Header("Structure Specific Settings")]
        public StructureClass structureClass;
        public StructureClass.StructureData structureData;
        public Transform spawnOrigin;
        public Transform spawnDestination;

        public GameObject unit;
        
        public override void QuickSelect()
        {
            Instantiate(unit);
            unit.transform.position = spawnOrigin.position;
            unit.transform.forward = transform.forward;
            Unit unitUnit = unit.GetComponent<Unit>();
            unitUnit.InitialiseUnit();
            unitUnit.Move(spawnDestination.position);
        }
        
        public void Damage(float damageTaken)
        {
            
        }
        
        #region Gizmos
        private void OnDrawGizmos () 
        {
            if (structureClass == null)
            {
                return;
            }
            if (structureClass.debugType == StructureClass.DebugType.ALWAYS)
            {
                DrawGizmos ();
            }
        }
        private void OnDrawGizmosSelected ()
        {
            if (structureClass == null)
            {
                return;
            }
            if (structureClass.debugType == StructureClass.DebugType.SELECTED_ONLY)
            {
                DrawGizmos ();
            }
        }
        private void DrawGizmos ()
        {
            if (structureClass == null)
            {
                return;
            }
            
            // Cache
            Transform t = transform;
            Vector3 pos = t.position;
            Vector3 r = t.forward;
            
            // Object
            Gizmos.color = Color.black;
            Gizmos.DrawWireCube(new Vector3(pos.x, pos.y*.5f, pos.z), new Vector3(structureClass.size.x, structureClass.size.y, structureClass.size.z));

            Gizmos.color = structureClass.spawnLocationColour;
            Vector3 o = spawnOrigin.position;
            Vector3 d = spawnDestination.position;
            Gizmos.DrawWireSphere(o, .1f);
            Gizmos.DrawWireSphere(d, .1f);
            Gizmos.DrawLine(o, d);
        }
        #endregion
    }
}
