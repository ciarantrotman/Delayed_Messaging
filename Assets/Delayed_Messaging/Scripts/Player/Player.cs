using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using VR_Prototyping.Scripts;

namespace Delayed_Messaging.Scripts.Player
{
    [RequireComponent(typeof(Selection))]
    public class Player : MonoBehaviour
    {
        public PlayerClass playerClass;
        public PlayerClass.PlayerData playerData;

        private GameObject playerObject;
        private Selection selection;

        public List<BaseObject> rSelectedObjects = new List<BaseObject>();
        public List<BaseObject> lSelectedObjects = new List<BaseObject>();

        private void Start()
        {
            AssignComponents();
        }
        
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
        public void ClearSelectedObjects(Selection.MultiSelect side, List<BaseObject> list, BaseObject focusObject)
        {
            foreach (BaseObject selectedObject in list)
            {
                if (selectedObject != focusObject || focusObject == null)
                {
                    Debug.Log(selectedObject.name + " was <b>REMOVED</b> from Player List");
                    selectedObject.Deselect(side, list);
                    lSelectedObjects.Remove(selectedObject);
                }
            }
        }
    }
}
