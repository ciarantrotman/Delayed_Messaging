using UnityEngine;
using VR_Prototyping.Scripts;

namespace Delayed_Messaging.Scripts.Resources
{
    public class Resource : BaseObject
    {
        [Header("Resource Specific Settings")]
        public ResourceClass resourceClass;

        protected override void Initialise()
        {
            ObjectClass = resourceClass;
        }
    }
}
