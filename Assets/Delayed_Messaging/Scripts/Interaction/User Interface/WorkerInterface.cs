using System.Collections.Generic;
using Delayed_Messaging.Scripts.Interaction.Interface_Building_Blocks;
using Delayed_Messaging.Scripts.Objects.Units;
using Delayed_Messaging.Scripts.Units;
using UnityEngine;

namespace Delayed_Messaging.Scripts.Interaction.User_Interface
{
    public class WorkerInterface : BaseObjectInterface
    {
        private Worker worker;

        protected override void OverrideInitialise()
        {
            worker = (Worker)baseObjectBaseObject;
        }
    }
}
