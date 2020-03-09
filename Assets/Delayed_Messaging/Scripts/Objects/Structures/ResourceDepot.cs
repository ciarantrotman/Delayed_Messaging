using UnityEngine;

namespace Delayed_Messaging.Scripts.Objects.Structures
{
    public class ResourceDepot : Structure
    {
        [SerializeField] private GameObject depositLocation;

        public Vector3 DepositLocation()
        {
            return depositLocation.transform.position;
        }
    }
}
