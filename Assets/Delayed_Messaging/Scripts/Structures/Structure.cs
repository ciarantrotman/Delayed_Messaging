using Delayed_Messaging.Scripts.Utilities;
using Pathfinding.Util;
using UnityEngine;
using VR_Prototyping.Scripts;

namespace Delayed_Messaging.Scripts.Structures
{
    public class Structure : BaseObject, IDamageable<float>
    {
        [Header("Structure Specific Settings")]
        public StructureClass structureClass;
        public StructureClass.StructureData structureData;
        [Space(10)] public Transform spawnOrigin;
        public Transform spawnDestination;

        private bool intialised;
        
        protected override void Initialise()
        {
            InitialiseStructure();
        }
        
        public void InitialiseStructure()
        {
            if (intialised)
            {
                return;
            }

            ObjectClass = structureClass;
            intialised = true;
        }
        
        public void Damage(float damageTaken)
        {
            
        }

        protected override void DrawGizmos()
        {
            // Cache
            Vector3 o = spawnOrigin.position;
            Vector3 d = spawnDestination.position;
            Draw.Gizmos.CircleXZ(o, .1f, structureClass.spawnLocationColour);
            Draw.Gizmos.CircleXZ(d, .1f, structureClass.spawnLocationColour);
            Gizmos.DrawLine(o, d);
            
            base.DrawGizmos();
        }
    }
}
