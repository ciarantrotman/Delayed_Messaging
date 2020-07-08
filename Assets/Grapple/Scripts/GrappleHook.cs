using UnityEngine;
using UnityEngine.Events;

namespace Grapple.Scripts
{
    public class GrappleHook : MonoBehaviour
    {
        public Rigidbody hookRigidBody;
        [HideInInspector] public UnityEvent collide;
        [HideInInspector] public Vector3 grapplePoint, offsetGrapplePoint;
        private GameObject anchorTarget;
        private LayerMask grappleLayer;
        /// <summary>
        /// Initial configuration of the grapple, assigns the layer mask for collisions
        /// </summary>
        /// <param name="layerMask"></param>
        public void ConfigureGrappleHook(LayerMask layerMask)
        {
            grappleLayer = layerMask;
        }
        /// <summary>
        /// Spawns a hook, its position is reset and is set to kinematic
        /// </summary>
        public void SpawnHook()
        {
            Transform hook = transform;
            hook.localPosition = Vector3.zero;
            hookRigidBody.isKinematic = true;
        }
        /// <summary>
        /// Launches the hook in the provided vector
        /// </summary>
        /// <param name="vector"></param>
        public void LaunchHook(Vector3 vector)
        {
            Transform hook = transform;
            hook.SetParent(null);
            hookRigidBody.isKinematic = false;
            hookRigidBody.velocity = vector;
            //hookRigidBody.AddForce(vector, ForceMode.VelocityChange);
        }
        private void OnTriggerEnter(Collider wall)
        {
            if (((1 << wall.gameObject.layer) & grappleLayer) == 0) return;
            grapplePoint = wall.ClosestPoint(transform.position);
            hookRigidBody.isKinematic = true;
            collide.Invoke();
        }
    }
}
