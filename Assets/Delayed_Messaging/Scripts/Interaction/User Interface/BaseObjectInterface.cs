using Delayed_Messaging.Scripts.Utilities;
using UnityEngine;
using VR_Prototyping.Scripts;

namespace Delayed_Messaging.Scripts.Interaction.User_Interface
{
    public abstract class BaseObjectInterface : MonoBehaviour, IInitialiseObjectInterface<BaseObject>
    {
        internal BaseObject baseObjectBaseObject;
        
        public void Initialise(BaseObject b)
        {
            baseObjectBaseObject = b;
            OverrideInitialise();
        }

        protected abstract void OverrideInitialise();
    }
}
