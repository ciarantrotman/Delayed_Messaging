using System;
using UnityEngine;

namespace VR_Prototyping.Scripts.Tools.Sketch
{
    public class SketchObject : MonoBehaviour
    {
        private SketchTool sketchTool;
        public LineRenderer SketchLineRenderer { get; set; }
        public Color SketchColor { get; set; }
        public int SketchIndex { get; set; }
        private AnimationCurve SketchCurve { get; set; }
        public void Initialise(SketchTool tool, Color color, LineRenderer lineRenderer, AnimationCurve profile, int index)
        {
            sketchTool = tool;
            SketchLineRenderer = lineRenderer;
            SketchColor = color;
            SketchIndex = index;
            SketchCurve = profile;
            SketchLineRenderer.SetupLineRender(sketchTool.sketchMaterial, sketchTool.brushWidth, true);
            SketchLineRenderer.material.color = SketchColor;
            if (!sketchTool.sketchProfile) return;
            SketchLineRenderer.widthCurve = SketchCurve;
        }

        public void AdjustWidthCurve()
        {
            SketchLineRenderer.widthCurve = SketchCurve;
        }
    }
}
