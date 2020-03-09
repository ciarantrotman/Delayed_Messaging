using System;
using Delayed_Messaging.Scripts.Interaction.Cursors;
using UnityEngine;
using VR_Prototyping.Scripts;

namespace Delayed_Messaging.Scripts.Objects
{
    public class BaseClass : ScriptableObject
    {
        public enum Team { RED, BLUE, NEUTRAL}
        
        [Header("Base Class Traits")] 
        public ControllerTransforms.DebugType debugType;

        [Header("Object Information")] 
        public string objectName = "Placeholder Name";
        public GameObject objectSpecificInterface;
        public GameObject objectModel;
        public CastCursor.CursorState cursorState;
        
        [Header("Object Settings")]
        public Team team;
        public Cost cost;
        public uint weight = 1;
        
        [Serializable] public struct Cost
        {
            public int resourceCost;
            public int productionCost;
        }
        public enum ModelIndex
        {
            GHOST,
            SITE,
            BUILT
        }
        [Serializable] public struct Model
        {
            public ModelIndex modelIndex;
            public GameObject modelPrefab;
        }
    }
}
