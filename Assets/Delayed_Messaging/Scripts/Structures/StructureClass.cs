using UnityEngine;

namespace Delayed_Messaging.Scripts.Structures
{
    [CreateAssetMenu(fileName = "StructureClass", menuName = "Class/Structure/StructureClass")]
    public class StructureClass : BaseClass
    {
        [Header("Structure Specific Traits")]
        public Color spawnLocationColour = new Color(0,0,0,1);
        
        public struct StructureData
        {
            
        }
    }
}
