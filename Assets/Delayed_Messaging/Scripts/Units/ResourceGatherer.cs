using Delayed_Messaging.Scripts.Resources;
using Panda;
using UnityEngine;
using VR_Prototyping.Scripts;
using VR_Prototyping.Scripts.Utilities;

namespace Delayed_Messaging.Scripts.Units
{
    public class ResourceGatherer : Unit
    {
        [Header("Resource Gatherer Specific Settings")]
        public Transform resourceLocation;
        public Transform depositLocation;
        public Resource currentResource;

        public bool gather;

        public int capacity = 0;

        private bool Arrived(Transform target)
        {
            return transform.TransformDistanceCheck(target, .2f);
        }
        
        [Task] bool Gather()
        {
            return gather;
        }
        [Task] bool Loaded;
        [Task] void MoveToResource()
        {
            Task task = Task.current;
            unitDestination.transform.position = resourceLocation.position;
            
            if (Arrived(resourceLocation))
            {
                Debug.LogWarning(name + " have <b>ARRIVED</b> at the resource");
                task.Succeed();
            }
        }
        [Task] void GatherResource()
        {
            Task task = Task.current;
            
            if (capacity <= 100)
            {
                capacity++;
            }
            else
            {
                Loaded = true;

                Debug.LogWarning(name + " have <b>GATHERED</b> a resource");
                task.Succeed();
            }
        }
        [Task] void MoveToDeposit()
        {
            Task task = Task.current;
            unitDestination.transform.position = depositLocation.position;
            
            if (Arrived(depositLocation))
            {
                Debug.LogWarning(name + " have <b>ARRIVED</b> at the deposit");
                task.Succeed();
            }
        }
        [Task] void DepositResource()
        {
            Task task = Task.current;

            if (capacity >= 0)
            {
                capacity--;
            }
            else
            {
                Loaded = false;
                Debug.LogWarning(name + " have <b>DEPOSITED</b> a resource");
                task.Succeed();
            }
        }

        protected override void DrawGizmos ()
        {
            if (resourceLocation != null && depositLocation != null)
            {
                Gizmos.color = new Color(1f, .5f, 0f, 1f);
                Gizmos.DrawWireSphere(resourceLocation.position, .05f);
                Gizmos.DrawWireSphere(depositLocation.position, .05f);
                Gizmos.DrawLine(depositLocation.position, resourceLocation.position);
            }
            base.DrawGizmos();
        }
    }
}
