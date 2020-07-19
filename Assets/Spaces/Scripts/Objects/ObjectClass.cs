using UnityEngine;

namespace Spaces.Scripts.Objects
{
    [CreateAssetMenu(fileName = "Object", menuName = "Object", order = 1)]
    public class ObjectClass : ScriptableObject
    {
        public GameObject objectObject, totemObject;
        public BaseObject.TotemState spawnState = BaseObject.TotemState.OBJECT;
    }
}
