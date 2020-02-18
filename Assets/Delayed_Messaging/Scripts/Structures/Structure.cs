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

        public GameObject unit;
        
        public override void SelectEnd()
        {
            Instantiate(unit);
            Debug.Log("<b>OH YEAH</b>");
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
        }
        #endregion
    }
}
