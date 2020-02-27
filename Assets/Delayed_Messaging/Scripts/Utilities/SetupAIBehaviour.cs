using Delayed_Messaging.Scripts.Units;
using Pathfinding;

namespace Delayed_Messaging.Scripts.Utilities
{
    public static class SetUnitBehaviour
    {
        /// <summary>
        /// Initial setup of the AIPath script, referencing the Unit Class
        /// </summary>
        /// <param name="aiPath"></param>
        /// <param name="unitClass"></param>
        public static void SetupAIPath(this AIPath aiPath, UnitClass unitClass)
        {
            // Movement
            aiPath.maxSpeed = unitClass.moveSpeed;
            aiPath.rotationSpeed = unitClass.rotationSpeed;
        }
    }
}
