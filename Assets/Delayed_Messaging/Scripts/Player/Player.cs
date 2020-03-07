using System;
using System.Collections.Generic;
using Delayed_Messaging.Scripts.Objects.Units;
using Delayed_Messaging.Scripts.Structures;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.SceneManagement;
using VR_Prototyping.Scripts;

namespace Delayed_Messaging.Scripts.Player
{
    [RequireComponent(typeof(Selection))]
    public class Player : MonoBehaviour
    {
        public PlayerClass playerClass;
        public PlayerClass.PlayerData playerData;

        public UnityAction spawn;
        
        private GameObject playerObject;
        private Selection selection;
        
        private void Start()
        {
            //spawn += Spawn();
            //AssignComponents();
        }
        /*
        private void AssignComponents()
        {
            if (playerObject == null)
            {
                foreach (GameObject rootGameObject in SceneManager.GetActiveScene().GetRootGameObjects())
                {
                    if (rootGameObject.name != "[VR Player]") continue;
                    playerObject = rootGameObject;
                    Debug.Log("<b>[Player] </b>" + name + " player set to " + rootGameObject.name);
                }
            }
            selection = playerObject.GetComponent<Selection>();
        }
        */
        
        public static void Spawn(string index, string spawnName, List<BaseObject.SpawnableObject> spawnableObjects, Vector3 spawnOrigin = default, Vector3 spawnDestination = default)
        {
            BaseObject.SpawnableObject spawnableObject = spawnableObjects.Find((x) => x.objectName == index);

            switch (spawnableObject.objectPrefab != null)
            {
                case true when spawnableObject.objectType == BaseObject.SpawnableObjectType.UNIT:
                    Debug.Log($"[Unit] Spawned <b>{index}</b> from {spawnName}!");
                    Instantiate(spawnableObject.objectPrefab);
                    spawnableObject.objectPrefab.GetComponent<Unit>().SpawnUnit(spawnOrigin, spawnDestination);
                    break;
                case true when spawnableObject.objectType == BaseObject.SpawnableObjectType.STRUCTURE:
                    Debug.Log($"[Structure] Spawned <b>{index}</b> from {spawnName}!");
                    Instantiate(spawnableObject.objectPrefab);
                    spawnableObject.objectPrefab.GetComponent<Structure>().SpawnStructure(spawnOrigin);
                    break;
                default:
                    Debug.LogError($"Tried to spawn <b>{index}</b> from {spawnName}, but couldn't find any SpawnableObject with that index!");
                    break;
            }
        }
    }
}
