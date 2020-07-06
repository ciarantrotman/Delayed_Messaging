using Delayed_Messaging.Scripts.Utilities;
using UnityEngine;

namespace Grapple.Scripts
{
    public class LineRendererDebug : MonoBehaviour
    {
        public LineRenderer lineRendererA;
        public Transform a;

        public LineRenderer lineRendererB;
        public Transform b;

    
        private void Update()
        {
            lineRendererA.DrawStraightLineRender(transform, a);
            lineRendererB.DrawStraightLineRender(transform, b);
        }
    }
}
