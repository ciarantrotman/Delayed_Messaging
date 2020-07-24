using Spaces.Scripts.User_Interface.Interface_Elements;
using UnityEngine;

namespace Spaces.Scripts.Objects.Object_Classes
{
    [CreateAssetMenu(fileName = "Object", menuName = "Object", order = 1)]
    public class ObjectClass : ScriptableObject
    {
        public GameObject objectObject, totemObject;
        public ObjectInstance.TotemState spawnState = ObjectInstance.TotemState.OBJECT;
        public BaseInterface.OutlineConfiguration objectOutline, totemOutline;
    }
}
