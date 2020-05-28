using System;
using Delayed_Messaging.Scripts.Objects;
using UnityEngine;
using UnityEngine.SceneManagement;
using VR_Prototyping.Plugins.QuickOutline.Scripts;
using VR_Prototyping.Scripts.Utilities;

namespace Gravity_Gloves.Scripts
{
    public class GravityObject : MonoBehaviour
    {
        private Selection selection;
        private Outline outline;

        public GravityObjectData leftHandData;
        public GravityObjectData rightHandData;
        
        public struct GravityObjectData
        {
            public float deviation;
            public float distance;
            public bool target;

            public void SetData(float dev, float dis)
            {
                deviation = dev;
                distance = dis;
            }
        }
        
        public void SetTargetData(GravityObjectData data, bool state)
        {
            data.target = state;
            ToggleOutline(state);
        }
        
        public GravityObjectData GetData(Selection.TargetObjects.Side side)
        {
            switch (side)
            {
                case Selection.TargetObjects.Side.RIGHT:
                    return rightHandData;
                case Selection.TargetObjects.Side.LEFT:
                    return leftHandData;
                default:
                    return new GravityObjectData();
            }
        }
        
        private void Start()
        {
            AssignComponents();
            SetupOutline();
        }

        private void Update()
        {
            ResetTargetState();
            
            if (selection.leftHand.primarySelectionCone.Intersection(transform, leftHandData))
            {
                selection.leftHand.targetObjects.objects.Add(this);
            }
            if (selection.rightHand.primarySelectionCone.Intersection(transform, rightHandData))
            {
                selection.rightHand.targetObjects.objects.Add(this);
            }
        }

        private void ToggleOutline(bool state)
        {
            outline.enabled = state;
        }

        private void AssignComponents()
        {
            if (selection != null) return;
            foreach (GameObject rootGameObject in SceneManager.GetActiveScene().GetRootGameObjects())
            {
                if (rootGameObject.name != "[Experimental Player]") continue;
                selection = rootGameObject.GetComponent<Selection>();
            }
            if (selection != null) return;
            Debug.LogError("No <b>Selection Component</b> was Assigned");
        }
        private void SetupOutline()
        {
            outline = transform.AddOrGetOutline();
            outline.OutlineWidth = 10;
            outline.enabled = false;
            outline.precomputeOutline = true;
        }

        private void ResetTargetState()
        {
            leftHandData.target = false;
            rightHandData.target = false;
            outline.enabled = false;
        }
    }
}
