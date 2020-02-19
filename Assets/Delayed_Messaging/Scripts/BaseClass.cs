using UnityEngine;

namespace Delayed_Messaging.Scripts
{
    public class BaseClass : ScriptableObject
    {
        public enum DebugType { NEVER, SELECTED_ONLY, ALWAYS }
        public enum Faction { RED, BLUE, NEUTRAL}
        
        [Header("Common Class Traits")] 
        public DebugType debugType;
        [Range(0, 100)] public int cost;
        [Range(0, 100)] public int healthMax;
        public Vector3 size;
        public Faction faction;
    }
}
