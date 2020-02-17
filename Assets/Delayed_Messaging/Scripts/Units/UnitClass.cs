using UnityEngine;

namespace Delayed_Messaging.Scripts.Units
{
    [CreateAssetMenu(fileName = "UnitClass", menuName = "Unit/UnitClass")]
    public class UnitClass : ScriptableObject
    {
        [Header("Movement")] 
        [Range(0, 10)] public float moveSpeed = 5;

        [Header("Space")] 
        [Range(0, 10)] public float unitRadius;  
        [Range(0, 10)] public float unitHeight;  
        
        public enum DebugType { NEVER, SELECTED_ONLY, ALWAYS }
        [Header("Debug Visuals")] 
        public DebugType unitDebugType;
        
        public Color troupeCenterColour = new Color(0,0,0,1);
        public Color forwardVectorColour = new Color(0,0,0,1);

        public struct UnitData
        {
            public Vector3 placeholder;
            public Vector3 troupeCenter;
        }
    }
}
