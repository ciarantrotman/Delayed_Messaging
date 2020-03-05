using System.Collections.Generic;
using Delayed_Messaging.Scripts.Units;
using UnityEngine;
using VR_Prototyping.Scripts;

namespace Delayed_Messaging.Scripts.Structures
{
    public class Base : Structure
    {
        [Header("Spawnable Units")] 
        public List<SpawnableObject> spawnableObjects;
        
        public void SpawnUnit(string index)
        {
            SpawnableObject spawnableObject = spawnableObjects.Find((x) => x.objectName == index);

            switch (spawnableObject.objectPrefab != null)
            {
                case true when spawnableObject.objectType == SpawnableObjectType.UNIT:
                    Debug.Log("[Unit] Spawned [" + index + "] from " + name + "!");
                    Instantiate(spawnableObject.objectPrefab);
                    spawnableObject.objectPrefab.GetComponent<Unit>().SpawnUnit(spawnOrigin.transform.position, spawnDestination.transform.position);
                    break;
                case true when spawnableObject.objectType == SpawnableObjectType.STRUCTURE:
                    Debug.Log("[Structure] Spawned [" + index + "] from " + name + "!");
                    Instantiate(spawnableObject.objectPrefab);
                    spawnableObject.objectPrefab.GetComponent<Structure>().SpawnStructure(spawnOrigin.transform.position);
                    break;
                default:
                    Debug.LogWarning("Tried to spawn [" + index + "] from " + name + ", but couldn't find any SpawnableObject with that index!");
                    break;
            }
        }
    }
}
