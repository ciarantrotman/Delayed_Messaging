using Delayed_Messaging.Scripts.Interaction.Interface_Building_Blocks;
using Delayed_Messaging.Scripts.Structures;
using UnityEngine;

namespace Delayed_Messaging.Scripts.Interaction.User_Interface
{
    public class BaseInterface : BaseObjectInterface
    {
        private Base baseObject;
        
        [Header("Base Interface References")]
        [SerializeField] private FrameButton workerSpawnButton;
        [SerializeField] private string workerIndex = "Worker";

        protected override void OverrideInitialise()
        {
            baseObject = (Base)baseObjectBaseObject;
            workerSpawnButton.OnSelect.AddListener(WorkerSpawn);
        }

        private void WorkerSpawn()
        {
            baseObject.SpawnUnit(workerIndex);
        }
    }
}
