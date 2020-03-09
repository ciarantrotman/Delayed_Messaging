using System;
using System.Collections;
using Delayed_Messaging.Scripts.Interaction;
using Delayed_Messaging.Scripts.Objects;
using Delayed_Messaging.Scripts.Objects.Structures;
using UnityEngine;
using Delayed_Messaging.Scripts.Player.Selection;
using Delayed_Messaging.Scripts.Utilities;

namespace Delayed_Messaging.Scripts.Player
{
    public class StructureController : MonoBehaviour
    {
        private Selection.Selection selection;
        private SelectionObjects selectionObjects;
        private UserInterface userInterface;
        private bool placingStructure;

        private bool cooldown;
        
        private Structure spawnStructure;
        private GameObject spawnGameObject;
        private GameObject spawnLocation;

        public class Spawn
        {
            //public GameObject spawnLocation;
            public Transform spawnTarget;
            //private SpawnCursor spawnCursor;
        }

        private Spawn spawn = new Spawn();

        private void Start()
        {
            selection = GetComponent<Selection.Selection>();
            userInterface = GetComponent<UserInterface>();
            spawnLocation = new GameObject("[Spawn Location]");
            
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
            
            spawnLocation.transform.Position(userInterface.dominantHand == UserInterface.DominantHand.LEFT
                ? selection.selectionObjectsL.castLocation
                : selection.selectionObjectsR.castLocation);
        }

        public void SpawnStructureStart(BaseObject.SpawnableObject spawnableObject)
        {
            if (placingStructure) return;
            
            placingStructure = true;
            selection.disableRightHand = true;

            StartCoroutine(SpawnCooldown());
            
            spawnGameObject = Instantiate(spawnableObject.objectPrefab, spawnLocation.transform);
            spawnStructure = spawnGameObject.GetComponent<Structure>();
            spawnStructure.InitialiseBaseObject();
            spawnStructure.SetModel(spawnStructure.structureClass.structureModels.Find((x) => x.modelIndex == BaseClass.ModelIndex.GHOST));
        }

        public void SpawnStructureStay()
        {
            if (!placingStructure || cooldown) return;
            spawnGameObject.transform.position = (Vector3)selectionObjects.graphNode.position;
        }

        private void SpawnStructureEnd()
        {
            return;
            if (!placingStructure || cooldown) return;
            
            placingStructure = false;
            selection.disableRightHand = false;
            
            Debug.Log($"{spawnStructure.name} was placed.");

            spawnStructure.SetModel(spawnStructure.structureClass.structureModels.Find((x) => x.modelIndex == BaseClass.ModelIndex.SITE));
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