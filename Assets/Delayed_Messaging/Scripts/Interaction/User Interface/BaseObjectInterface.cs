using System.Xml.Serialization;
using Delayed_Messaging.Scripts.Utilities;
using UnityEngine;
using VR_Prototyping.Scripts;

namespace Delayed_Messaging.Scripts.Interaction.User_Interface
{
    public abstract class BaseObjectInterface : MonoBehaviour, IInitialiseObjectInterface<BaseObject>
    {
        internal BaseObject baseObject;
        
        public void Initialise(BaseObject b)
        {
            baseObject = b;
            OverrideInitialise();
        }

        protected virtual void OverrideInitialise()
        {
            
        }
    }
}
