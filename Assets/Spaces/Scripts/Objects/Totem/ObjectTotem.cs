using UnityEngine;

namespace Spaces.Scripts.Objects
{
    public class ObjectTotem : MonoBehaviour
    {
        private bool extant;
        private ObjectInstance objectInstance;
        public GameObject objectObject, totemObject;

        /// <summary>
        /// Called only once, when the object is first created
        /// </summary>
        /// <param name="parent"></param>
        /// <param name="objectClass"></param>
        /// <param name="instance"></param>
        public void InstantiateObjectTotem(Transform parent, ObjectClass objectClass, ObjectInstance instance)
        {
            if (extant) return;
            extant = true;
            
            objectObject = Create(objectClass.objectObject, parent);
            totemObject = Create(objectClass.totemObject, parent);

            objectInstance = instance;
        }
        /// <summary>
        /// Creates a new object and sets it to inactive in the same frame
        /// </summary>
        /// <param name="prefab"></param>
        /// <param name="parent"></param>
        /// <returns></returns>
        private static GameObject Create(GameObject prefab, Transform parent)
        {
            GameObject placeholder = Instantiate(prefab, parent);
            placeholder.tag = ObjectInstance.Object;
            placeholder.SetActive(false);
            return placeholder;
        }
        /// <summary>
        /// Public wrapper to set the state of the object to a totem
        /// </summary>
        public void Totemise()
        {
            SetState(ObjectInstance.TotemState.TOTEM);
        }
        /// <summary>
        /// Public wrapper to set the state of the object to an object
        /// </summary>
        public void Objectise()
        {
            SetState(ObjectInstance.TotemState.OBJECT);
        }
        // Handles the logic for transitioning between totem and object
        private void SetState(ObjectInstance.TotemState state)
        {
            // Only allow this to be called once the objects have been spawned
            if (!extant) return;
                
            switch (state)
            {
                case ObjectInstance.TotemState.TOTEM:
                    objectObject.SetActive(false);
                    totemObject.SetActive(true);
                    return;
                case ObjectInstance.TotemState.OBJECT:
                    objectObject.SetActive(true);
                    totemObject.SetActive(false);
                    return;
                default:
                    return;
            }
        }
    }
}
