using UnityEngine;

namespace Delayed_Messaging.Scripts.Units
{
    [CreateAssetMenu(fileName = "UnitClass", menuName = "Class/Unit/UnitClass")]
    public class UnitClass : BaseClass
    {
        [Header("Unit Specific Traits")]
        [Range(0, 10)] public float moveSpeed = 5;
        [Range(0, 10)] public float rotationSpeed = 5;

        public Color forwardVectorColour = new Color(0,0,0,1);
        public Color destinationColour = new Color(0,0,0,1);
        
        public struct UnitData
        {
            public Vector3 destination;
        }
    }
}
