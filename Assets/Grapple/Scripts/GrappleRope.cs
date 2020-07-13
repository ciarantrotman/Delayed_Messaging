using System;
using System.Collections.Generic;
using Delayed_Messaging.Scripts.Utilities;
using UnityEngine;
using UnityEngine.Events;

namespace Grapple.Scripts
{
    public class GrappleRope : MonoBehaviour
    {
        private Material ropeMaterial;
        private LineRenderer rope;
        private Transform anchor, hook;
        private LayerMask layer;
        private bool ropeConnected, launched;
        private const float WrapTolerance = .1f;
        private Vector3 ropeCenter;

        [HideInInspector] public UnityEvent wrap;
        private readonly List<RaycastHit> ropePoints = new List<RaycastHit>();
        
        /// <summary>
        /// Assigns values to cached variables
        /// </summary>
        /// <param name="anchorTransform"></param>
        /// <param name="material"></param>
        /// <param name="layerMask"></param>
        public void ConfigureRope(Transform anchorTransform, Material material, LayerMask layerMask)
        {
            layer = layerMask;
            ropeMaterial = material;
            anchor = anchorTransform;
            rope = gameObject.AddComponent<LineRenderer>();
            rope.SetupLineRender(ropeMaterial, .015f, true);
        }
        /// <summary>
        /// Used to transfer information to draw the rope when it has been launched
        /// </summary>
        /// <param name="hookTransform"></param>
        public void LaunchRope(Transform hookTransform)
        {
            hook = hookTransform;
            launched = true;
        }
        /// <summary>
        /// 
        /// </summary>
        public void CreateRope(RaycastHit grappleLocation)
        {
            ropePoints.Add(grappleLocation);
            ropeConnected = true;
            launched = false;
        }
        /// <summary>
        /// Used to allow us to wrap the logic for returning the grapple location in this separate class
        /// </summary>
        /// <returns></returns>
        public Vector3 GrappleLocation()
        {
            return ropePoints[ropePoints.Count - 1].point;
        }
        /// <summary>
        /// Returns the vector to the functional grapple location
        /// </summary>
        /// <returns></returns>
        public Vector3 RopeVector()
        {
            return GrappleLocation() - anchor.position;
        }
        /// <summary>
        /// Returns the length of the functional rope
        /// </summary>
        /// <returns></returns>
        public float RopeLength()
        {
            return Vector3.Distance(GrappleLocation(), anchor.position);
        }
        /// <summary>
        /// Checks the state of the rope and sees if it can wrap or unwrap
        /// </summary>
        private void CheckWrap()
        {
            return;
            if (ropePoints.Count <= 0) return;

            // Cache the values used for all the calculations
            Vector3 anchorPosition = anchor.position, ropeVector = GrappleLocation() - anchorPosition;

            // If there is an obstacle in the way and it's between the anchor and the last grapple point, that's your new grapple point
            if (Physics.Raycast(anchorPosition, ropeVector, out RaycastHit hit, RopeLength() - WrapTolerance, layer))
            {
                wrap.Invoke();
                ropePoints.Add(hit);
                return;
            }
            // If there's no obstacle then you can check to see if the rope can unwrap
            int index = ropePoints.Count - 1;
            // Work backwards to see how far the rope can unfurl
            for (int i = index; i > 0; i--)
            {
                Vector3 previousGrapple = ropePoints[i-1].point;
                float segmentLength = Vector3.Distance(anchorPosition, previousGrapple) - WrapTolerance;
                
                // If there is an obstacle in the way of the last point 
                if (Physics.Raycast(anchorPosition, previousGrapple - anchorPosition, segmentLength, layer)) return;

                // If there is no obstacle between the anchor and the grapple point it will unfurl to that point
                ropePoints.RemoveAt(i);
                return;
                // todo: stop premature unwrapping
            }
        }
        /// <summary>
        /// 
        /// </summary>
        private void DrawRope()
        {
            Vector3 anchorPosition = anchor.position;
            Vector3 hookPosition = launched || ropeConnected ? hook.position : anchorPosition;
            ropeCenter = Vector3.Lerp(ropeCenter, Vector3.Lerp(anchorPosition, hookPosition, .5f), .1f);
            rope.BezierLineRenderer(anchorPosition, ropeCenter, hookPosition);
            return;
            switch (launched)
            {
                case true:
                    //Vector3 anchorPosition = anchor.position;
                    //Vector3 hookPosition = hook.position;
                    ropeCenter = Vector3.Lerp(ropeCenter, Vector3.Lerp(anchorPosition, hookPosition, .5f), .1f);
                    rope.BezierLineRenderer(anchorPosition, ropeCenter, hookPosition);
                    return;
                case false:
                    rope.positionCount = ropePoints.Count + 1;
                    for (int i = 0; i < ropePoints.Count; i++)
                    {
                        rope.SetPosition(i, ropePoints[i].point);
                    }
                    rope.SetPosition(ropePoints.Count, anchor.position);
                    return;
            }
        }
        /// <summary>
        /// Called to reset the rope
        /// </summary>
        public void DisconnectRope()
        {
            rope.positionCount = 2;
            ropePoints.Clear();
            ropeConnected = false;
        }
        private void Update()
        {
            DrawRope();
            if (!ropeConnected) return;
            CheckWrap();
        }
    }
}
