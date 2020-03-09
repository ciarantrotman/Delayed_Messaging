using Delayed_Messaging.Scripts.Objects.Structures;
using UnityEngine;

namespace Delayed_Messaging.Scripts.Interaction.User_Interface
{
    public class ResourceDepotInterface : BaseObjectInterface
    {
        private ResourceDepot resourceDepot;

        protected override void OverrideInitialise()
        {
            resourceDepot = (ResourceDepot) baseObjectBaseObject;
        }

        internal override void SetupSpawn()
        {
            foreach (SpawnObject spawnObject in spawnObjects)
            {
                Debug.Log(
                    $"<b>{baseObjectBaseObject.name}</b> has added a listener to {spawnObject.frameButton.name} to spawn <b>{spawnObject.reference}</b>");
                spawnObject.frameButton.OnSelect.AddListener(() =>
                {
                    resourceDepot.player.Spawn(
                        spawnObject.reference,
                        baseObjectBaseObject.name,
                        baseObjectBaseObject.spawnableObjects,
                        resourceDepot.spawnOrigin.transform.position);
                });
            }
        }
    }
}
