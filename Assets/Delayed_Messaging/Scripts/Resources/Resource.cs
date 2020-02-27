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

        protected override void Initialise()
        {
            ObjectClass = resourceClass;
            currentVolume = startingVolume;
        }

        public bool HasCapacity()
        {
            return currentVolume > 0;
        }

        public bool GatherResource(float gatherRate)
        {
            if (currentVolume == 0)
            {
                Debug.LogWarning(name + " is <b>OUT OF RESOURCES</b>");
                return false;
            }
            else
            {
                currentVolume--;
                return true;
            }
        }
    }
}
