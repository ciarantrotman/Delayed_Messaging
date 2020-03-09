using System;
using System.Collections.Generic;
using UnityEngine;

namespace Delayed_Messaging.Scripts.Objects.Structures
{
    [CreateAssetMenu(fileName = "StructureClass", menuName = "Class/Structure/StructureClass")]
    public class StructureClass : BaseClass
    {
        [Header("Structure Specific Traits")] 
        public List<Model> structureModels;
        public Color spawnLocationColour = new Color(0,0,0,1);
        
        public struct StructureData
        {
            
        }
    }
}
