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

        protected override void Initialise()
        {
            //objectClass = structureClass;
        }

        public override void QuickSelect(Selection.MultiSelect side)
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
            if (structureClass.debugType == ControllerTransforms.DebugType.ALWAYS)
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
            if (structureClass.debugType == ControllerTransforms.DebugType.SELECTED_ONLY)
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
            
            base.DrawGizmos();
            
            // Cache
            Transform t = transform;
            Vector3 pos = t.position;
            Vector3 r = t.forward;

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
