using UnityEngine;
using VR_Prototyping.Plugins.QuickOutline.Scripts;

namespace Delayed_Messaging.Scripts
{
    public class BaseClass : ScriptableObject
    {
        public enum DebugType { NEVER, SELECTED_ONLY, ALWAYS }
        [Header("Common Class Traits")] 
        public DebugType debugType;
        [Range(0, 100)] public int cost;
        public Vector3 size;
    }
}
