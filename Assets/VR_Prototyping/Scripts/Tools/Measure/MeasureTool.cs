using System.Collections.Generic;
using Sirenix.OdinInspector;
using UnityEngine;

namespace VR_Prototyping.Scripts.Tools.Measure
{
    public class MeasureTool : BaseTool
    {
        public enum NodeLockingType
        {
            RELATIVE,
            RELATIVE_VERTICAL,
            GLOBAL
        }
        [BoxGroup("Tape Tool Settings")] [Required] public GameObject tapeNodePrefab;
        [BoxGroup("Tape Tool Settings")] [Space(10)] public bool axisSnapping;
        [BoxGroup("Tape Tool Settings")] [ShowIf("axisSnapping")] [Indent] [Required] public GameObject snapObject;
        [BoxGroup("Tape Tool Settings")] [ShowIf("axisSnapping")] [Indent] public NodeLockingType nodeLockingType;
        [BoxGroup("Tape Tool Settings")] [ShowIf("axisSnapping")] [Indent] [Range(.01f, .5f)] public float snapTolerance;
        [BoxGroup("Tape Tool Settings")] [ShowIf("axisSnapping")] [Indent(2)] [Range(.0001f, .003f)] public float activeWidth;
        [BoxGroup("Tape Tool Settings")] [ShowIf("axisSnapping")] [Indent(2)] [Range(.0001f, .003f)] public float inactiveWidth;
        [BoxGroup("Tape Tool Settings")] [Space(10)] public bool nodeInsertion;
        [BoxGroup("Tape Tool Settings")] [ShowIf("nodeInsertion")] [Indent] [Required] public GameObject intersectionPointPrefab;
        [BoxGroup("Tape Tool Settings")] [ShowIf("nodeInsertion")] [Indent] [Range(.00001f, .1f)] public float tolerance = .01f;
        [BoxGroup("Tape Tool Settings")] [ShowIf("nodeInsertion")] [Indent] [Range(0f, .1f)] public float insertionThreshold = .1f;
        [BoxGroup("Tape Tool Settings")] [Space(10)] public bool grabNode;
        [BoxGroup("Tape Tool Settings")] [ShowIf("grabNode")] [Indent] [Range(.001f, .05f)] public float nodeGrabDistance = .1f;
        [BoxGroup("Tape Tool Settings")] [Space(10)] public Material tapeMaterial;
        [BoxGroup("Tape Tool Settings")] [Indent] [Range(.001f, .05f)] public float tapeWidth;
        [BoxGroup("Tape Tool Settings")] [Space(10)] public float nodeTextFocusHeight = .2f;
        [BoxGroup("Tape Tool Settings")] public float nodeTextStandardHeight = .15f;

        private readonly List<MeasureTape> measureTapes = new List<MeasureTape>();
        public MeasureText MeasureText { get; set; }
        public MeasureTape MeasureTape { get; set; }
        public MeasureTape FocusMeasureTape { get; set; }
        public MeasureNode MeasureNode  { get; set; }
        public MeasureNode FocusMeasureNode  { get; set; }
        public MeasureNode PreviousMeasureNode  { get; set; }
        public MeasureVisual MeasureVisual { get; set; }
        public MeasureLockNode MeasureLockNode { get; set; }
        public bool Insertion { get; set; }
        public bool Grabbing { get; set; }
        public bool Placing { get; private set; }
        
        private int tapeCount;

        private bool validNode;
        
        private GameObject tapeObject;
        private GameObject node;

        protected override void Initialise()
        {
            intersectionPointPrefab = Instantiate(intersectionPointPrefab, transform);
            intersectionPointPrefab.name = "Measure/Intersection";
        }

        protected override void ToolUpdate()
        {
            intersectionPointPrefab.SetActive(Insertion);
            
            if (MeasureText == null || MeasureTape == null) return;
            MeasureText.SetText(MeasureTape.TapeDistance(), MeasureTape.TapeName);
        }

        protected override void ToolStart()
        {
            if (Insertion || !ValidNodePlacement()) return;
            Placing = true;
            InsertNode(MeasureTape, dominant.transform.position, MeasureTape.measureNodes.Count);
        }

        protected override void ToolStay()
        {
            if (Insertion || !validNode) return;
            NodeSnap(dominant.transform, FocusMeasureNode, PreviousMeasureNode, FocusMeasureTape);
        }

        protected override void ToolEnd()
        {
            Placing = false;
            if (Insertion || !validNode) return;
            NodeSnap(dominant.transform, FocusMeasureNode, PreviousMeasureNode, FocusMeasureTape);
            ReleaseNode();
        }

        public void NodeSnap(Transform defaultTarget, MeasureNode currentNode, MeasureNode previousNode, MeasureTape currentTape)
        {
            if (previousNode == null)
            {
                SetNodePosition(currentNode, currentTape, defaultTarget);
                return;
            }
            switch (axisSnapping)
            {
                case true when previousNode.XSnap:
                    SetNodePosition(currentNode, currentTape, previousNode.X.transform);
                    return;
                case true when previousNode.YSnap:
                    SetNodePosition(currentNode, currentTape, previousNode.Y.transform);
                    return;
                case true when previousNode.ZSnap:
                    SetNodePosition(currentNode, currentTape, previousNode.Z.transform);
                    return;
                default:
                    SetNodePosition(currentNode, currentTape, defaultTarget);
                    return;
            }
        }

        private static void SetNodePosition(Component measureNode, MeasureTape tape, Transform target)
        {
            Transform nodeTransform = measureNode.transform;
            nodeTransform.LerpTransform(target, .5f);
            tape.TapeLr.SetPosition(tape.TapeLr.positionCount - 1, nodeTransform.position);
            tape.AdjustTape(); 
        }

        public void NewTape()
        {
            if (MeasureTape != null && MeasureTape.measureNodes.Count == 0) return;
            
            tapeCount++;
            Color color = Color.HSVToRGB(Random.Range(0f, 1f), 1, 1, true);
            
            tapeObject = new GameObject("Tape_" + tapeCount);
            tapeObject.transform.Transforms(dominant.transform);
            MeasureTape = tapeObject.AddComponent<MeasureTape>();
            measureTapes.Add(MeasureTape);
            MeasureTape.Controller = Controller;
            MeasureTape.MeasureTool = this;

            PreviousMeasureNode = null;

            MeasureTape.TapeLr = tapeObject.AddComponent<LineRenderer>();
            MeasureTape.TapeLr.SetupLineRender(Controller.doubleSidedLineRenderMat, tapeWidth, true);
            
            MeasureTape.SetColor(color);
            
            if (tapeCount > 1)
            {
                MeasureVisual.SetColor(color);
            }
            
            MeasureTape.TapeLr.positionCount = 0;
            MeasureTape.TapeName = tapeCount.ToString();
        }

        private bool ValidNodePlacement()
        {
            validNode = true;
            return validNode;
            
            if (PreviousMeasureNode == null)
            {
                validNode = true;
                return validNode;
            }
            bool valid = Vector3.Distance(dominant.transform.position, PreviousMeasureNode.transform.position) >= tolerance;
            validNode = valid;
            return validNode;
        }
        public void InsertNode(MeasureTape tape, Vector3 position, int index)
        {
            node = Instantiate(tapeNodePrefab, position, Quaternion.identity, tapeObject.transform);
            
            MeasureNode = node.GetComponent<MeasureNode>();
            FocusMeasureNode = MeasureNode;
            
            MeasureLockNode.SetToggleState(false, true);
            
            FocusMeasureTape = tape;
            FocusMeasureTape.Controller = Controller;
            
            MeasureNode.Initialise(this, Controller, tape);
            MeasureNode.SetColor(FocusMeasureTape.tapeColor);
            
            tape.measureNodes.Insert(index, MeasureNode);
            
            tape.RefactorNodes();
            
            AddLineRenderNode(tape.TapeLr, position);
            tape.AdjustTape();
        }

        private void ReleaseNode()
        {
            PreviousMeasureNode = FocusMeasureNode;
            FocusMeasureNode.NodeStart();
        }

        public void DeleteNode()
        {
            if (FocusMeasureNode == null) return;
            FocusMeasureNode.DeleteNode();
            FocusMeasureTape.RefactorNodes();
            RemoveLineRenderNode(FocusMeasureTape.TapeLr);
            FocusMeasureTape.AdjustTape();
        }
        
        public void LockNode()
        {
            if (FocusMeasureNode == null) return;
            
            FocusMeasureNode.LockNode = true;
            FocusMeasureTape.RefactorNodes();
        }
        
        public void UnlockNode()
        {
            if ((FocusMeasureNode == null && FocusMeasureNode.NodeIndex > 0) || FocusMeasureTape == null) return;
            
            FocusMeasureNode.LockNode = false;
            FocusMeasureTape.RefactorNodes();
        }

        private static void AddLineRenderNode(LineRenderer lr, Vector3 position)
        {
            int positionCount = lr.positionCount;
            positionCount++;
            lr.positionCount = positionCount;
            lr.SetPosition(positionCount - 1, position);
        }
        
        private static void RemoveLineRenderNode(LineRenderer lr)
        {
            int positionCount = lr.positionCount;
            positionCount--;
            lr.positionCount = positionCount;
        }

        public void SetColor(Color color)
        {
            if (FocusMeasureTape == null) return;
            FocusMeasureTape.SetColor(color);
        }

        public void GrabNode(MeasureNode measureNode, MeasureTape measureTape)
        {
            MeasureNode = measureNode;
            FocusMeasureNode = measureNode;
            MeasureTape = measureTape;
            FocusMeasureTape = measureTape;
            MeasureLockNode.SetToggleState(measureNode.LockNode, true);
            MeasureVisual.SetColor(MeasureTape.tapeColor);
        }

        protected override void ToolActivate()
        {
            if (measureTapes.Count == 0)
            {
                NewTape();
            }
            if (PreviousMeasureNode == null) return;
            PreviousMeasureNode.NodeStart();
        }
        
        protected override void ToolDeactivate()
        {
            DeactivateAllTapes();
        }

        public void DeactivateAllTapes()
        {
            foreach (MeasureTape tape in measureTapes)
            {
                foreach (MeasureNode measureNode in tape.measureNodes)
                {
                    measureNode.NodeEnd();
                }
            }
        }
    }
}
