using UnityEngine;

namespace Delayed_Messaging.Scripts.Player
{
    public class PlayerClass : ScriptableObject
    {
        [Header("Base Player Traits")] 
        [Range(0, 1000)] public int startingResources;
        public BaseClass.Faction faction;

        public struct PlayerData
        {
            
        }
    }
}
