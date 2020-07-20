using UnityEngine;

namespace Spaces.Scripts.Objects
{
    public class ObjectSelectionController : MonoBehaviour
    {
        public ObjectInstance objectInstance;
        
        public ObjectInstance FocusObject()
        {
            return objectInstance == null ? null : objectInstance;
        }
    }
}
