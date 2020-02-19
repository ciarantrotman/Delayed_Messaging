using Delayed_Messaging.Scripts.Units;
using Delayed_Messaging.Scripts.Utilities;
using UnityEngine;
using VR_Prototyping.Scripts;

namespace Delayed_Messaging.Scripts.Structures
{
    public class Structure : BaseObject, IDamageable<float>
    {
        public StructureClass structureClass;
        public StructureClass.StructureData structureData;
        public Transform spawnLocation;

        public GameObject unit;
        
        public override void QuickSelect()
        {
            Instantiate(unit);
            unit.transform.position = spawnLocation.position;
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
            Gizmos.DrawWireCube(pos, new Vector3(
                structureClass.size.x,
                structureClass.size.y,
                structureClass.size.z));

            Gizmos.color = structureClass.spawnLocationColour;
            Vector3 spawn = spawnLocation.position;
            Gizmos.DrawWireSphere(spawn, .1f);
            Gizmos.DrawLine(new Vector3(pos.x, 0, pos.z), spawn);
        }
        #endregion
    }
}
