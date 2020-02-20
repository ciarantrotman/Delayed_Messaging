﻿using System;
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

        public List<BaseObject> selectedObjects = new List<BaseObject>();

        private void Start()
        {
            AssignComponents();
            
            selection.quickSelect.AddListener(ClearSelectedObjects);
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

        private void ClearSelectedObjects()
        {
            foreach (BaseObject selectedObject in selectedObjects)
            {
                selectedObject.Deselect();
            }
            
            selectedObjects.Clear();
        }
    }
}
