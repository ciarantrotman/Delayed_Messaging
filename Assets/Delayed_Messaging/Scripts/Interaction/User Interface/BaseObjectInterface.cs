using System;
using System.Collections.Generic;
using Delayed_Messaging.Scripts.Interaction.Interface_Building_Blocks;
using Delayed_Messaging.Scripts.Utilities;
using UnityEngine;
using VR_Prototyping.Scripts;

namespace Delayed_Messaging.Scripts.Interaction.User_Interface
{
    public abstract class BaseObjectInterface : MonoBehaviour, IInitialiseObjectInterface<BaseObject>
    {
        internal BaseObject baseObjectBaseObject;
        
        [Header("Base Interface References")] 
        public List<SpawnObject> spawnObjects;
        
        [Serializable] public struct SpawnObject
        {
            public FrameButton frameButton;
            public string reference;
        }
        
        public void Initialise(BaseObject b)
        {
            baseObjectBaseObject = b;
            OverrideInitialise();
            SetupSpawn();
        }

        internal virtual void SetupSpawn()
        {
            foreach (SpawnObject spawnObject in spawnObjects)
            {
                Debug.Log($"<b>{baseObjectBaseObject.name}</b> has added a listener to {spawnObject.frameButton.name} to spawn <b>{spawnObject.reference}</b>");
                spawnObject.frameButton.OnSelect.AddListener(() =>
                    {
                        Player.Player.Spawn(
                            spawnObject.reference, 
                            baseObjectBaseObject.name,
                            baseObjectBaseObject.spawnableObjects);
                    });
            }
        }

        protected abstract void OverrideInitialise();
    }
}
