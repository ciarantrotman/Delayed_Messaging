using System;
using UnityEngine;

namespace Grapple.Scripts.Testing
{
    public class JointTesting : MonoBehaviour
    {
        public LineRenderer rope;
        public Transform a, b;
        public SpringJoint c, d;

        private void Update()
        {
            rope.positionCount = 3;
            rope.SetPosition(0, a.position);
            rope.SetPosition(1, transform.position);
            rope.SetPosition(2, b.position);
            
            Debug.DrawRay(c.anchor, Vector3.up, Color.red);
            Debug.DrawRay(c.connectedAnchor, Vector3.up, Color.yellow);
            Debug.DrawRay(d.anchor, Vector3.up, Color.blue);
            Debug.DrawRay(d.connectedAnchor, Vector3.up, Color.cyan);
        }
    }
}
