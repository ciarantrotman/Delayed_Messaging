using System;
using UnityEngine;
using UnityEngine.Events;

namespace Grapple.Scripts
{
    public class GrappleHook : MonoBehaviour
    {
        public Rigidbody hookRigidBody;
        [HideInInspector] public UnityEvent collide;
        [HideInInspector] public Vector3 grapplePoint;
        private GameObject anchorTarget;
        private LayerMask grappleLayer;
        /// <summary>
        /// 
        /// </summary>
        /// <param name="layerMask"></param>
        public void ConfigureGrappleHook(LayerMask layerMask)
        {
            grappleLayer = layerMask;
        }
        /// <summary>
        /// 
        /// </summary>
        public void SpawnHook()
        {
            Transform hook = transform;
            hook.localPosition = Vector3.zero;
            hookRigidBody.isKinematic = true;
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="vector"></param>
        public void LaunchHook(Vector3 vector)
        {
            Transform hook = transform;
            hook.SetParent(null);
            hookRigidBody.isKinematic = false;
            hookRigidBody.AddForce(vector, ForceMode.VelocityChange);
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
