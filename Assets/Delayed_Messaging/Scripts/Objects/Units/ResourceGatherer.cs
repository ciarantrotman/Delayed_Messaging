using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Delayed_Messaging.Scripts.Objects.Resources;
using Delayed_Messaging.Scripts.Objects.Structures;
using Delayed_Messaging.Scripts.Resources;
using Delayed_Messaging.Scripts.Utilities;
using Panda;
using UnityEngine;

namespace Delayed_Messaging.Scripts.Objects.Units
{
    public class ResourceGatherer : Unit
    {
        [Header("Resource Gatherer Specific Settings")]
        [SerializeField] private float gatherDuration;
        public int capacity;
        
        public ResourceDepot resourceDepot;
        public Resource currentResource;

        private List<Resource> detectedResources = new List<Resource>();
        
        private bool gather;
        private bool searching;

        protected override void Spawn()
        {
            resourceDepot = FindResourceDepot();
        }

        private ResourceDepot FindResourceDepot()
        {
            Collider[] depots = Physics.OverlapSphere(transform.position, unitClass.detectionRadius, 1 << 12);
            return depots.Select(overlapObject => overlapObject.GetComponent<ResourceDepot>()).FirstOrDefault(depot => depot != null);
        }

        protected override void CancelCurrentTask()
        {
            gather = false;
            searching = false;
        }
        public void StartGathering(Resource resource)
        {
            currentResource = resource;
            gather = true;
        }
        public void StartSearching()
        {
            gather = true;
            searching = true;
        }
        [Task] bool Gather()
        {
            return gather;
        }
        [Task] bool Search()
        {
            return searching;
        }
        [Task] bool HasResource()
        {
            return currentResource != null;
        }
        [Task] bool Loaded;
        [Task] void DetectResource()
        { 
            if (currentResource != null && !searching)
            {
                Task.current.Fail();
                return;
            }

            detectedResources = Resources();
            
            if (detectedResources.Count > 0)
            {
                // make this the closest one... (sort the list...)
                currentResource = detectedResources[0];
                //searching = false;
                detectedResources.Clear();
                Task.current.Succeed();
            }
            else
            {
                CancelCurrentTask();
                Task.current.Fail();
            }
        }

        private List<Resource> Resources()
        {
            Collider[] overlap = Physics.OverlapSphere(transform.position, unitClass.detectionRadius, 1 << 12);
            List<Resource> resources = overlap.Select(overlapObject => overlapObject.GetComponent<Resource>()).Where(resource => resource != null && resource.HasCapacity()).ToList();
            return resources;
        }

        [Task] void MoveToResource()
        {
            if (currentResource == null || unitDestination == null)
            {
                Task.current.Fail();
            }
            
            unitDestination.transform.position = currentResource.transform.position;
            
            if (transform.Arrived(currentResource.transform, .3f))
            {
                Debug.LogWarning(name + " have <b>ARRIVED</b> at the resource");
                Task.current.Succeed();
            }
        }
        [Task] void GatherResource()
        {
            if (currentResource != null && currentResource.GatherResource(this))
            {
                Loaded = true;
                Debug.LogWarning(name + " have <b>GATHERED</b> a resource");
                //StartCoroutine(GatherResourceDelay(Task.current, gatherDuration));
                Task.current.Succeed();
                // AOE2 Bug
                // currentResource = currentResource.HasCapacity() ? currentResource : null;
            }
            else
            {
                currentResource = null;
                Task.current.Fail();
            }
        }
        [Task] void MoveToDeposit()
        {
            unitDestination.transform.position = resourceDepot.DepositLocation();
            
            if (transform.Arrived(resourceDepot.DepositLocation(), .3f))
            {
                Debug.LogWarning(name + " have <b>ARRIVED</b> at the deposit");
                Task.current.Succeed();
            }
        }
        [Task] void DepositResource()
        {
            Loaded = false;
            Debug.LogWarning(name + " have <b>DEPOSITED</b> a resource");
            Task.current.Succeed();
            //StartCoroutine(GatherResourceDelay(Task.current, gatherDuration));
        }

        protected override void DrawGizmos ()
        {
            if (currentResource != null && resourceDepot != null)
            {
                Gizmos.color = new Color(1f, .5f, 0f, 1f);
                Gizmos.DrawWireSphere(currentResource.transform.position, .05f);
                Gizmos.DrawWireSphere(resourceDepot.DepositLocation(), .05f);
                Gizmos.DrawLine(resourceDepot.DepositLocation(), currentResource.transform.position);
            }
            base.DrawGizmos();
        }

        private IEnumerator GatherResourceDelay(Task task, float delay)
        {
            yield return new WaitForSeconds(delay);
            task.Succeed();
            yield return null;
        }
    }
}
