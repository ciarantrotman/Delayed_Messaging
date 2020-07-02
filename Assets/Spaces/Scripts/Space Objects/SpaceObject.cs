using System;
using DG.Tweening;
using UnityEngine;
using Gizmos = Popcron.Gizmos;

namespace Spaces.Scripts.Space_Objects
{
    public abstract class SpaceObject : MonoBehaviour
    {
        public Vector3 activePosition;
        public Vector3 activeRotation;

        private void Start()
        {
            SetActiveTransform();
        }

        public void SetActiveTransform()
        {
            Transform t = transform;
            activePosition = t.position;
            activeRotation = t.eulerAngles;
        }
    }
}
