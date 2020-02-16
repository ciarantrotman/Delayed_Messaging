using Sirenix.OdinInspector;
using UnityEngine;

namespace VR_Prototyping.Scripts.Demo_Scripts
{
    public class IsometricCameraFollow : MonoBehaviour
    {
        [BoxGroup] [SerializeField] private Transform target;

        private void Update()
        {
            Vector3 position = target.position;
            Transform thisTransform = transform;
            thisTransform.position = Vector3.Lerp(thisTransform.position, new Vector3(position.x, position.y - 1.5f, position.z), .5f);
            thisTransform.rotation = Quaternion.identity;
        }
    }
}
