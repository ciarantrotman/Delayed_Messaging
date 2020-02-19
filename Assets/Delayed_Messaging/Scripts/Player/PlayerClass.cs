using UnityEngine;

namespace Delayed_Messaging.Scripts.Player
{
    [CreateAssetMenu(fileName = "PlayerClass", menuName = "Class/Player/PlayerClass")]
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
