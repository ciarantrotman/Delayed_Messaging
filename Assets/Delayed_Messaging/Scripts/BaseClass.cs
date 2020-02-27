using UnityEngine;
using VR_Prototyping.Scripts;

namespace Delayed_Messaging.Scripts
{
    public class BaseClass : ScriptableObject
    {
        public enum Team { RED, BLUE, NEUTRAL}
        
        [Header("Base Class Traits")] 
        public ControllerTransforms.DebugType debugType;

        [Header("Object Information")] 
        public string objectName = "Placeholder Name";
        public GameObject objectModel;
        
        [Header("Object Settings")]
        public Team team;
        public int cost;
        public int healthMax;
    }
}
