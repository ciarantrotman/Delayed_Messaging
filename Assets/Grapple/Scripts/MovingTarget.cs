using System;
using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using UnityEngine;

namespace Grapple.Scripts
{
    public class MovingTarget : MonoBehaviour
    {
        [SerializeField] private List<Transform> nodes;
        [SerializeField] private float timeFactor = 2f;
        private int index = 0;

        private void Start()
        {
            StartCoroutine(MoveToNode(Vector3.Distance(transform.position, nodes[index].position) / timeFactor, nodes[index]));
        }

        private IEnumerator MoveToNode(float duration, Transform node)
        {
            Transform t = transform;
            
            t.DOMove(node.position, duration);
            t.DORotate(node.eulerAngles, duration);
            yield return new WaitForSeconds(duration);

            index = index < nodes.Count ? index + 1 : 0;
            StartCoroutine(MoveToNode(Vector3.Distance(t.position, nodes[index].position) / timeFactor, nodes[index]));
        }
    }
}
