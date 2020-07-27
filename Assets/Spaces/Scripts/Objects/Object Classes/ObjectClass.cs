using System;
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
        public ObjectPhysics objectPhysics;
        
        [Serializable] public struct ObjectPhysics
        {
            public bool gravity, kinematic;
            public float mass, drag, angularDrag;

            public void SetObjectPhysics(bool useGravity, bool isKinematic, float objectMass = 1f, float objectDrag = 1f, float objectAngularDrag = 1f)
            {
                gravity = useGravity;
                kinematic = isKinematic;
                mass = objectMass;
                drag = objectDrag;
                angularDrag = objectAngularDrag;
            }
        }
    }
}
