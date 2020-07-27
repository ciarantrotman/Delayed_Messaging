using System;
using Spaces.Scripts.Utilities;
using UnityEngine;

namespace Spaces.Scripts.Objects.Manipulation
{
    public class ManipulationVisual : MonoBehaviour
    {
        public Material material;
        private LineRenderer local, projected;
        private GameObject objectStart, objectCurrent, controllerStart;
        private Transform controllerCurrent;
        private bool extant;
        /// <summary>
        /// 
        /// </summary>
        /// <param name="startObject"></param>
        /// <param name="currentObject"></param>
        /// <param name="startController"></param>
        /// <param name="currentController"></param>
        public void Initialise(GameObject startObject, GameObject currentObject, GameObject startController, Transform currentController)
        {
            // Cache all references
            objectStart = startObject;
            objectCurrent = currentObject;
            controllerStart = startController;
            controllerCurrent = currentController;
            // Create line renderers
            local = controllerStart.LineRender(material, .005f, .005f, true, true);
            projected = objectStart.LineRender(material, .005f, .005f, true, true);
            // Latch
            extant = true;
        }

        private void Update()
        {
            if (!extant) return;
            local.DrawStraightLineRender(local.transform, controllerCurrent);
            projected.DrawStraightLineRender(projected.transform, objectCurrent.transform);
        }
    }
}
