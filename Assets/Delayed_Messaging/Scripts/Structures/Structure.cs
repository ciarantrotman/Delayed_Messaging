using Delayed_Messaging.Scripts.Utilities;
using UnityEngine;

namespace Delayed_Messaging.Scripts.Structures
{
    public class Structure : MonoBehaviour, ISelectable, IDamageable<float>
    {
        public StructureClass structureClass;
        public StructureClass.StructureData structureData;
        
        public void Select()
        {
            throw new System.NotImplementedException();
        }

        public void Damage(float damageTaken)
        {
            throw new System.NotImplementedException();
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
