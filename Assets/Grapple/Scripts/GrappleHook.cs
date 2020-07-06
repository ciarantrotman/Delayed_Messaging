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

        public void SpawnHook()
        {
            Transform hook = transform;
            hook.localPosition = Vector3.zero;
            hookRigidBody.isKinematic = true;
        }
        public void LaunchHook(Vector3 vector)
        {
            Transform hook = transform;
            hook.SetParent(null);
            hookRigidBody.isKinematic = false;
            hookRigidBody.AddForce(vector, ForceMode.VelocityChange);
        }
        private void OnCollisionEnter(Collision grapple)
        { 
            grapplePoint = grapple.GetContact(0).point;
            hookRigidBody.isKinematic = true;
            collide.Invoke();
        }
    }
}
