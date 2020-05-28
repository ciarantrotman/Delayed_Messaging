using System;
using System.Collections.Generic;
using Delayed_Messaging.Scripts.Player;
using Delayed_Messaging.Scripts.Utilities;
using UnityEngine;
using VR_Prototyping.Scripts;

namespace Gravity_Gloves.Scripts
{
    public class InverseKinematicRig : MonoBehaviour
    {
        private ControllerTransforms controllerTransforms;

        [Header("Inverse Kinematic Rig Settings")] 
        [SerializeField, Range(0f, 1f)] private float torsoStability = .5f;
        [SerializeField, Range(0f, 1f)] private float torsoOffset;

        [Header("Inverse Kinematic Rig References")] 
        [SerializeField] private List<Transform> torsos = new List<Transform>();

        private GameObject torso;
        private void Start()
        {
            Transform t = transform;
            controllerTransforms = t.parent.GetComponent<ControllerTransforms>();
            
            torso = new GameObject("[IK Rig / Torso]");
            torso.transform.SetParent(t);
        }

        private void Update()
        {
            CalculateTorsoPosition();
            
            Debug.DrawLine(controllerTransforms.CameraPosition(), torso.transform.position, Color.cyan);
            
            foreach (Transform torsoTransform in torsos)
            {
                torsoTransform.StableTransforms(torso.transform, torsoStability);
            }
        }

        private void CalculateTorsoPosition()
        {
            Vector3 pos;
            Quaternion rot;
            
            // Calculate the torso's possible position
            pos = controllerTransforms.CameraPosition();
            pos = new Vector3(pos.x, pos.y - torsoOffset, pos.z);
            
            // Then calculate its potential rotation
            rot = controllerTransforms.CameraTransform().rotation;

            torso.transform.position = pos;
            //torso.transform.rotation = rot;
        }
    }
}
