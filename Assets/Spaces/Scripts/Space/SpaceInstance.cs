using System.Collections.Generic;
using Spaces.Scripts.Objects;
using UnityEngine;

namespace Spaces.Scripts.Space
{
    public class SpaceInstance : MonoBehaviour
    {
        public List<ObjectInstance> objectInstances = new List<ObjectInstance>();
        
        public void AddObject(ObjectInstance objectInstance)
        {
            if (Extant(objectInstance)) return;
            objectInstances.Add(objectInstance);
        }
        public void RemoveObject(ObjectInstance objectInstance)
        {
            if (!Extant(objectInstance)) return;
            objectInstances.Remove(objectInstance);
        }

        private bool Extant(ObjectInstance objectInstance)
        {
            return objectInstances.Contains(objectInstance);
        }
    }
}
