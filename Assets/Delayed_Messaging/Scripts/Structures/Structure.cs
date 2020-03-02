using System.Collections.Generic;
using Delayed_Messaging.Scripts.Units;
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
        public void Damage(float damageTaken)
        {
            
        }
    }
}
