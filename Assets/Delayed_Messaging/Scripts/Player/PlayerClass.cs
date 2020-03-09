using Delayed_Messaging.Scripts.Objects;
using UnityEngine;

namespace Delayed_Messaging.Scripts.Player
{
    [CreateAssetMenu(fileName = "PlayerClass", menuName = "Class/Player/PlayerClass")]
    public class PlayerClass : ScriptableObject
    {
        [Header("Base Player Traits")] 
        [Range(0, 1000)] public int startingResources;
        public BaseClass.Team team;

        public struct PlayerData
        {
            
        }
    }
}
