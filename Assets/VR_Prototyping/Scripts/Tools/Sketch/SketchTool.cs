using System.Collections.Generic;
using Sirenix.OdinInspector;
using UnityEngine;
using UnityEngine.AI;

namespace VR_Prototyping.Scripts.Tools.Sketch
{
    public class SketchTool : BaseTool
    {        
        [BoxGroup("Sketch Tool Settings")] [Range(.001f, .1f)] public float erasingDistance = .1f;
        [BoxGroup("Sketch Tool Settings")] [Range(.001f, .05f)] public float minWidth;
        [BoxGroup("Sketch Tool Settings")] [Range(.05f, .1f)] public float maxWidth;
        [BoxGroup("Sketch Tool Settings")] [Space(5)] public Material sketchMaterial;
        [BoxGroup("Sketch Tool Settings")] [Space(5)] public bool sketchTrail;
        [BoxGroup("Sketch Tool Settings")] [Indent] [ShowIf("sketchTrail")] public AnimationCurve trailWidth;
        [BoxGroup("Sketch Tool Settings")] [Space(5)] public bool sketchProfile;
        [BoxGroup("Sketch Tool Settings")] [Indent] [ShowIf("sketchProfile")] public AnimationCurve profileCurve;
        private bool Erasing { get; set; }
        public SketchBrushVisual SketchVisual { private get; set; }

        private readonly List<SketchObject> sketches = new List<SketchObject>();

        private Color brushColor;
        internal float brushWidth;

        private int sketchCount;
        private int position = 1;
        
        private GameObject sketchGameObject;
        private SketchObject sketchObject;
        private LineRenderer sketchLineRenderer;
        protected override void ToolUpdate()
        {
            if (!Erasing) return;
            
            foreach (SketchObject sketch in sketches)
            {
                int proximity = 0;
                
                for (int i = 0; i < sketch.SketchLineRenderer.positionCount; i++)
                {
                    if (EraseDistance(sketch.SketchLineRenderer, i))
                    {
                        proximity++;
                    }
                }
                
                if (proximity > 0)
                {
                    sketch.SketchLineRenderer.material.color = Color.white;
                    if (CTrigger)
                    {
                        EraseSketch(sketch);
                    }
                    return;
                }

                sketch.SketchLineRenderer.material.color = sketch.SketchColor;
            }
        }

        private bool EraseDistance(LineRenderer lr, int index)
        {
            return Vector3.Distance(lr.GetPosition(index), dominant.transform.position) <= erasingDistance;
        }

        protected override void ToolStart()
        {
            if (Erasing) return;
            NewTape();
        }

        protected override void ToolStay()
        {
            if (Erasing) return;
            sketchObject.AdjustWidthCurve();
            sketchLineRenderer.positionCount = position + 1;
            sketchLineRenderer.SetPosition(position, dominant.transform.position);
            position++;
        }

        protected override void ToolEnd()
        {
            if (Erasing) return;
            sketchObject.AdjustWidthCurve();
            sketchLineRenderer.BakeMesh(new Mesh(), true);
            sketchObject = null;
            sketchLineRenderer = null;
            sketchTrail = true;
        }

        protected override void ToolInactive()
        {
            if(SketchVisual == null) return;

            SketchVisual.SetVisual(brushColor, brushWidth);
        }

        private void NewTape()
        {
            sketchGameObject = new GameObject("Sketch_" + sketchCount);
            sketchObject = sketchGameObject.AddComponent<SketchObject>();
            sketchLineRenderer = sketchGameObject.AddComponent<LineRenderer>();
            AnimationCurve normalisedCurve = new AnimationCurve();
            foreach (Keyframe key in profileCurve.keys)
            {
                Keyframe keyframe = key;
                keyframe.value *= brushWidth;
                keyframe.outWeight *= brushWidth;
                keyframe.inTangent *= brushWidth;
                keyframe.inWeight *= brushWidth;
                keyframe.outTangent *= brushWidth;
                keyframe.time = key.time;
                normalisedCurve.AddKey(keyframe);
            }
            sketchObject.Initialise(this, brushColor, sketchLineRenderer, normalisedCurve, sketchCount);
            sketchGameObject.transform.Transforms(dominant.transform);
            sketches.Add(sketchObject);
            position = 0;
            sketchTrail = false;
            sketchCount++;
        }

        private void EraseSketch(SketchObject sketch)
        {
            sketch.SketchLineRenderer.enabled = false;
            //sketches.RemoveAt(sketch.SketchIndex);
            //Destroy(sketch.gameObject);
        }
        
        public void SetColor(Color color)
        {
            brushColor = color;
        }

        public void EraseToggle(bool state)
        {
            Erasing = state;
            sketchTrail = !state;
        }
        
        public void SetWidth(float widthPercentage)
        {
            brushWidth = Mathf.Lerp(minWidth, maxWidth, widthPercentage);
            if (sketchLineRenderer != null)
            {
                sketchLineRenderer.LineRenderWidth(brushWidth, brushWidth);
            }
        }
    }
}
