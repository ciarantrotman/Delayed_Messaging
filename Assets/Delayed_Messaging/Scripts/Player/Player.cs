using System.Collections.Generic;
using UnityEngine;
using VR_Prototyping.Scripts;

namespace Delayed_Messaging.Scripts.Player
{
    public class Player : MonoBehaviour
    {
        public PlayerClass playerClass;
        public PlayerClass.PlayerData playerData;

        internal List<BaseObject> selectedObjects = new List<BaseObject>();
    }
}
