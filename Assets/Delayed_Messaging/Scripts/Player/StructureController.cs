using System;
using System.Collections;
using Delayed_Messaging.Scripts.Interaction;
using Delayed_Messaging.Scripts.Objects;
using Delayed_Messaging.Scripts.Objects.Structures;
using UnityEngine;
using Delayed_Messaging.Scripts.Player.Selection;
using Delayed_Messaging.Scripts.Utilities;
using VR_Prototyping.Scripts;

namespace Delayed_Messaging.Scripts.Player
{
    public class StructureController : MonoBehaviour
    {
        private Selection.Selection selection;
        private SelectionObjects selectionObjects;
        private UserInterface userInterface;
        private bool placingStructure;

        private bool cooldown;

        private BaseObject.SpawnableObject spawnObject;
        private Structure spawnStructure;
        private GameObject spawnGameObject;

        public class Spawn
        {
            public GameObject spawnLocation;
            public Transform spawnTarget;
            //private SpawnCursor spawnCursor;
        }

        private Spawn spawn = new Spawn();

        private void Start()
        {
            selection = GetComponent<Selection.Selection>();
            userInterface = GetComponent<UserInterface>();
            spawn.spawnLocation = new GameObject("[SpawnLocation]");
            
            selection.selectionInitialised.AddListener(InitialiseStructureController);
        }

        private void InitialiseStructureController()
        {
            Debug.Log("StructureController initialised!");
            spawn.spawnTarget = userInterface.dominantHand == UserInterface.DominantHand.LEFT
                ? selection.selectionObjectsL.castLocation
                : selection.selectionObjectsR.castLocation;

            selectionObjects = userInterface.dominantHand == UserInterface.DominantHand.LEFT
                ? selection.selectionObjectsL
                : selection.selectionObjectsR;
            
            selectionObjects.selectStart.AddListener(SpawnStructureEnd);
        }

        private void Update()
        {
            //spawn.spawnLocation.transform.Position(spawn.spawnTarget);
            
            spawn.spawnLocation.transform.Position(userInterface.dominantHand == UserInterface.DominantHand.LEFT
                ? selection.selectionObjectsL.castLocation
                : selection.selectionObjectsR.castLocation);
        }

        public void SpawnStructureStart(BaseObject.SpawnableObject spawnableObject)
        {
            if (placingStructure) return;
            
            placingStructure = true;
            selection.disableRightHand = true;

            StartCoroutine(SpawnCooldown());
            
            spawnGameObject = Instantiate(spawnableObject.objectPrefab, spawn.spawnTarget);
            spawnObject = spawnableObject;
            spawnStructure = spawnObject.objectPrefab.GetComponent<Structure>();
            spawnStructure.SetModel(spawnStructure.structureClass.structureModels.Find((x) => x.modelIndex == "Ghost"));
            Debug.Log($"{spawnableObject.objectName} was spawned.");
        }

        public void SpawnStructureStay()
        {
            if (!placingStructure || cooldown) return;
            
            spawnGameObject.transform.position = (Vector3)selectionObjects.graphNode.position;
        }

        private void SpawnStructureEnd()
        {
            if (!placingStructure || cooldown) return;
            
            placingStructure = false;
            selection.disableRightHand = false;
            
            Debug.Log($"{spawnStructure.name} was placed.");

            spawnStructure.SetModel(spawnStructure.structureClass.structureModels.Find((x) => x.modelIndex == "Site"));
        }

        public void SpawnStructureCancel()
        {
            if (!placingStructure || cooldown) return;
            
            placingStructure = false;
            selection.disableRightHand = false;
        }

        private IEnumerator SpawnCooldown()
        {
            cooldown = true;
            yield return new WaitForSeconds(1);
            cooldown = false;
        }
    }
}