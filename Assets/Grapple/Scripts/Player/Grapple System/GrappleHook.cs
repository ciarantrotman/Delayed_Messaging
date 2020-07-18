using System.Collections;
using DG.Tweening;
using Grapple.Scripts.Player.Grapple_System;
using UnityEngine;
using UnityEngine.Events;

namespace Grapple.Scripts
{
    public class GrappleHook : MonoBehaviour
    {
        public Rigidbody hookRigidBody;
        [HideInInspector] public UnityEvent collide;
        private const float SpeedFactor = 25f;
        /// <summary>
        /// Spawns a hook, its position is reset and is set to kinematic
        /// </summary>
        public void SpawnHook()
        {
            transform.localPosition = Vector3.zero;
        }
        /// <summary>
        /// Launches the hook in the provided vector, time is calculated based on distance
        /// </summary>
        /// <param name="data"></param>
        public void LaunchHook(GrappleSystem.Location.GrappleLocationData data)
        {
            Transform hook = transform;
            float duration = Vector3.Distance(data.grappleLocation.point, hook.position) / SpeedFactor;
            
            hook.SetParent(null);
            transform.DOMove(data.grappleLocation.point, duration);
            transform.DORotate(data.grappleLocation.normal, duration);

            StartCoroutine(Grapple(duration, data.grappleLocation.transform));
        }
        /// <summary>
        /// Used to simulate the time taken to 
        /// </summary>
        /// <param name="duration"></param>
        /// <param name="parent"></param>
        /// <returns></returns>
        private IEnumerator Grapple(float duration, Transform parent)
        {
            yield return new WaitForSeconds(duration);
            transform.SetParent(parent);
            hookRigidBody.isKinematic = true;
            collide.Invoke();
        }
    }
}
