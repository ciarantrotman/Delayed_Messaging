using System;
using Delayed_Messaging.Scripts.Utilities;
using Pathfinding.Util;
using UnityEngine;
using VR_Prototyping.Scripts;

namespace Delayed_Messaging.Scripts.Objects.Structures
{
    public class Structure : BaseObject, IDamageable<float>, ISpawnableStructure<Vector3>
    {
        [Header("Structure Specific Settings")]
        public StructureClass structureClass;
        public StructureClass.StructureData structureData;
        [Space(10)] public Transform spawnOrigin;
        public Transform spawnDestination;
        private bool initialised;
        protected override void Initialise()
        {
            InitialiseStructure();
        }

        protected override void ObjectUpdate()
        {
            
        }

        private void InitialiseStructure()
        {
            if (initialised) return;
            initialised = true;

            ObjectClass = structureClass;

            if (nonSpawnedObject)
            {
                SetModel(structureClass.structureModels.Find((x) => x.modelIndex == BaseClass.ModelIndex.BUILT));
            }
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

        public void SpawnStructure(Vector3 spawnLocation = default)
        {
            
        }
    }
}
