using UnityEngine;

namespace Spaces.Scripts.Objects
{
    [CreateAssetMenu(fileName = "Object", menuName = "Object", order = 1)]
    public class ObjectClass : ScriptableObject
    {
        public GameObject objectObject, totemObject;
        public ObjectInstance.TotemState spawnState = ObjectInstance.TotemState.OBJECT;
    }
}
