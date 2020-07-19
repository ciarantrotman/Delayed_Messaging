using UnityEngine;

namespace Spaces.Scripts.Objects
{
    public class ObjectTotem : MonoBehaviour
    {
        private bool extant;
        public GameObject objectObject, totemObject;

        /// <summary>
        /// Called only once, when the object is first created
        /// </summary>
        /// <param name="parent"></param>
        /// <param name="objectClass"></param>
        public void InstantiateObjectTotem(Transform parent, ObjectClass objectClass)
        {
            if (extant) return;
                
            objectObject = Instantiate(objectClass.objectObject, parent);
            totemObject = Instantiate(objectClass.totemObject, parent);
                
            extant = true;
        }
        // Handles the logic for transitioning between totem and object
        public void SetState(BaseObject.TotemState state)
        {
            // Only allow this to be called once the objects have been spawned
            if (!extant) return;
                
            switch (state)
            {
                case BaseObject.TotemState.TOTEM:
                    objectObject.SetActive(false);
                    totemObject.SetActive(true);
                    return;
                case BaseObject.TotemState.OBJECT:
                    objectObject.SetActive(true);
                    totemObject.SetActive(false);
                    return;
                default:
                    return;
            }
        }
    }
}
