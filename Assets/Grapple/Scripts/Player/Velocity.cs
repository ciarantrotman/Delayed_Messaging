using System;
using Delayed_Messaging.Scripts.Player;
using Delayed_Messaging.Scripts.Utilities;
using UnityEngine;

namespace Grapple.Scripts
{
    public class Velocity : MonoBehaviour
    {
        [SerializeField] private Material visualMaterial;

        private Rigidbody player;
        private ControllerTransforms controller;
        private GameObject parent;
        private LineRenderer visual;

        private float magnitude;
        private Vector3 center;
        private const float VelocityThreshold = .1f, Height = .5f;
        
        /// <summary>
        /// 
        /// </summary>
        /// <param name="position"></param>
        /// <param name="forward"></param>
        private void CalculateParentPosition(Vector3 position, Vector3 forward)
        {
            parent.transform.position = new Vector3(position.x, position.y - Height, position.z);
            parent.transform.forward = Vector3.Lerp(parent.transform.forward, forward, .75f);
        }
        /// <summary>
        /// 
        /// </summary>
        private void DrawVisual()
        {
            magnitude = Mathf.Lerp(magnitude, player.velocity.magnitude, .75f);
            Vector3 end = new Vector3(0, 0, magnitude);
            center = Vector3.Lerp(center, Vector3.Lerp(Vector3.zero, end, .5f), .25f);
            
            visual.BezierLineRenderer(Vector3.zero, center, end);
        }
        private void Start()
        {
            // Cache values
            controller = GetComponent<ControllerTransforms>();
            player = GetComponent<Rigidbody>();
            
            // Create objects
            parent = new GameObject("[Velocity Parent]");
            parent.transform.SetParent(transform);
            visual = parent.AddComponent<LineRenderer>();
            
            // Configuration
            visual.SetupLineRender(visualMaterial, .0025f, true);
            visual.startWidth = .0025f;
            visual.endWidth = 0f;
            visual.useWorldSpace = false;
        }

        private void Update()
        {
            CalculateParentPosition(
                controller.Position(ControllerTransforms.Check.HEAD), 
                player.velocity);
            DrawVisual();
        }
    }
}
