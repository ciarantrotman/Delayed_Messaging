using UnityEngine;

namespace Delayed_Messaging.Scripts.Resources
{
    [CreateAssetMenu(fileName = "ResourceClass", menuName = "Class/Resource/ResourceClass")]
    public class ResourceClass : BaseClass
    {
        [Header("Resource Specific Traits")] 
        public int resourceCapacity;
        [Range(0f, 1f)] public float baseGatherDuration;
    }
}
