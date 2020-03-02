using System.Collections;
using System.Collections.Generic;
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
        [SerializeField] private float gatherDuration;
        public int capacity;
        
        public Transform depositLocation;
        public Resource currentResource;

        private bool gather;
        private bool searching;

        private List<Resource> detectedResources = new List<Resource>();

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
            
            Collider[] overlap = Physics.OverlapSphere(transform.position, unitClass.detectionRadius, 1 << 12);
            
            foreach (Collider overlapObject in overlap)
            {
                Resource resource = overlapObject.GetComponent<Resource>();
                if (resource != null && resource.HasCapacity())
                {
                    detectedResources.Add(resource);
                }
            }

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
            unitDestination.transform.position = depositLocation.position;
            
            if (transform.Arrived(depositLocation, .3f))
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
            if (currentResource != null && depositLocation != null)
            {
                Gizmos.color = new Color(1f, .5f, 0f, 1f);
                Gizmos.DrawWireSphere(currentResource.transform.position, .05f);
                Gizmos.DrawWireSphere(depositLocation.position, .05f);
                Gizmos.DrawLine(depositLocation.position, currentResource.transform.position);
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
