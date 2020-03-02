﻿using System.Collections.Generic;
using Delayed_Messaging.Scripts.Units;
using UnityEngine;
using VR_Prototyping.Scripts;

namespace Delayed_Messaging.Scripts.Resources
{
    public class Resource : BaseObject
    {
        [Header("Resource Specific Settings")]
        public ResourceClass resourceClass;
        [SerializeField] private int startingVolume = 2;

        private int currentVolume;

        public override void QuickSelect(Selection.MultiSelect side, List<BaseObject> list)
        {
            foreach (BaseObject unit in list)
            {
                ResourceGatherer resourceGatherer = unit.GetComponent<ResourceGatherer>();
                
                if (resourceGatherer == null) continue;
                Debug.LogWarning(resourceGatherer.name + " set to gather resources from " + name);
                resourceGatherer.StartGathering(this);
            }
            
            base.QuickSelect(side, list);
        }

        protected override void Initialise()
        {
            ObjectClass = resourceClass;
            currentVolume = startingVolume;
        }

        public bool HasCapacity()
        {
            return currentVolume > 0;
        }

        public bool GatherResource(ResourceGatherer resourceGatherer)
        {
            if (currentVolume == 0)
            {
                Debug.LogWarning(name + " is <b>OUT OF RESOURCES</b>");
                return false;
            }
            else if (currentVolume < resourceGatherer.capacity)
            {
                Debug.LogWarning(name + " is <b>OUT OF RESOURCES</b>");
                currentVolume = 0;
                return true;
            }
            else
            {
                Debug.LogWarning(name + " has " + currentVolume + " resources left");
                currentVolume -= resourceGatherer.capacity;
                return true;
            }
        }
    }
}
