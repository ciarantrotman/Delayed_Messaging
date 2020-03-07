using Delayed_Messaging.Scripts.Objects.Structures;
using UnityEngine;

namespace Delayed_Messaging.Scripts.Interaction.User_Interface
{
    public class BaseInterface : BaseObjectInterface
    {
        private Base baseObject;

        protected override void OverrideInitialise()
        {
            baseObject = (Base)baseObjectBaseObject;
        }
        
        internal override void SetupSpawn()
        {
            foreach (SpawnObject spawnObject in spawnObjects)
            {
                Debug.Log($"<b>{baseObjectBaseObject.name}</b> has added a listener to {spawnObject.frameButton.name} to spawn <b>{spawnObject.reference}</b>");
                spawnObject.frameButton.OnSelect.AddListener(() =>
                {
                    Player.Player.Spawn(
                        spawnObject.reference, 
                        baseObjectBaseObject.name,
                        baseObjectBaseObject.spawnableObjects,
                        baseObject.spawnOrigin.transform.position);
                });
            }
        }
    }
}
