using Sirenix.OdinInspector;
using TMPro;
using UnityEngine;
using UnityEngine.AI;

namespace VR_Prototyping.Scripts.Tools.Measure
{
    public class MeasureNode : MonoBehaviour
    {
        private MeasureTool MeasureTool { get; set; }
        private MeasureTape MeasureTape { get; set; }
        public bool LockNode { get; set; }
        private ControllerTransforms Controller { get; set; }      
        public TextMeshPro Text { get; private set; }
        public float Distance { get; set; }
        public int NodeIndex { get; set; }
        private MeasureNode previousNode;
        private SkinnedMeshRenderer skinnedMeshRenderer;
        private GameObject dominantFollow;
        
        public GameObject X {get; private set;}
        public GameObject Y {get; private set;}
        public GameObject Z {get; private set;}
        
        private float xDistance;
        private float yDistance;
        private float zDistance;
        
        private LineRenderer xLr;
        private LineRenderer yLr;
        private LineRenderer zLr;

        private Transform grabController;
        
        public bool XSnap { get; private set; }
        public bool YSnap { get; private set; }
        public bool ZSnap { get; private set; }
        
        private const float DirectDistance = .05f;
        private const float TriggerDistance = DirectDistance + DirectDistance + DirectDistance;
        
        private bool rGrabP;
        private bool lGrabP;

        public void Initialise(MeasureTool tool, ControllerTransforms c, MeasureTape tape)
        {
            MeasureTool = tool;
            Controller = c;
            MeasureTape = tape;
            Text = GetComponentInChildren<TextMeshPro>();
            skinnedMeshRenderer = GetComponentInChildren<SkinnedMeshRenderer>();
            SetupAxis();
        }

        private void SetupAxis()
        {
            dominantFollow = new GameObject("Dominant_Follow");
            dominantFollow.transform.SetParent(transform);
            dominantFollow.transform.localPosition = Vector3.zero;

            X = Instantiate(MeasureTool.snapObject, transform, true);
            X.name = "Axis_X";
            X.transform.localPosition = Vector3.zero;
            xLr = X.transform.AddOrGetLineRenderer();
            xLr.SetupLineRender(Controller.lineRenderMat, .001f, true);
            X.SetActive(false);
            
            Y = Instantiate(MeasureTool.snapObject, transform, true);
            Y.name = "Axis_Y";
            Y.transform.localPosition = Vector3.zero;
            yLr = Y.transform.AddOrGetLineRenderer();
            yLr.SetupLineRender(Controller.lineRenderMat, .001f, true);
            Y.SetActive(false);
            
            Z = Instantiate(MeasureTool.snapObject, transform, true);
            Z.name = "Axis_Z";
            Z.transform.localPosition = Vector3.zero;
            zLr = Z.transform.AddOrGetLineRenderer();
            zLr.SetupLineRender(Controller.lineRenderMat, .001f, true);
            Z.SetActive(false);
        }

        private void FixedUpdate()
        {
            Text.transform.LookAwayFrom(Controller.CameraTransform(), Vector3.up);
            Text.transform.eulerAngles = new Vector3(0, Text.transform.eulerAngles.y, 0);
            
            if (!MeasureTool.Active) return;
            
            DirectGrabCheck(Controller.RightTransform(), Controller.RightGrab(), Controller.LeftGrab(), rGrabP);
            DirectGrabCheck(Controller.LeftTransform(), Controller.LeftGrab(), Controller.RightGrab(), lGrabP);

            rGrabP = Controller.RightGrab();
            lGrabP = Controller.LeftGrab();
            
            NodeEvents();
            NodeBlendShape();
        }

        private void NodeBlendShape()
        {
            skinnedMeshRenderer.SetBlendShapeWeight(0, BlendShapeWeight());
        }

        private float BlendShapeWeight()
        {
            Vector3 position = transform.position;
            
            float domDistance = Vector3.Distance(position, MeasureTool.dominant.transform.position);
            float nonDistance = Vector3.Distance(position, MeasureTool.nonDominant.transform.position);

            if (domDistance < nonDistance)
            {
                return domDistance < DirectDistance ? 0f : (Mathf.InverseLerp(DirectDistance, TriggerDistance, domDistance)) * 100f;
            }
            return nonDistance < DirectDistance ? 0f : (Mathf.InverseLerp(DirectDistance, TriggerDistance, nonDistance)) * 100f;
        }

        private void NodeEvents()
        {
            switch (CurrentNode())
            {
                case true when !PreviousNode(): // spawned but not released
                    NodeStay();
                    break;
                case true when PreviousNode(): // spawned and released
                    NodeStay();
                    break;
                case false when PreviousNode(): // released and no new node
                    NodeStay();
                    break;
                case false when !PreviousNode(): // new node and released
                    NodeEnd();
                    break;
                default:
                    return;
            }
        }

        private bool PreviousNode()
        {
            return MeasureTool.PreviousMeasureNode == this;
        }
        private bool CurrentNode()
        {
            return MeasureTool.FocusMeasureNode == this;
        }
        
        private void DirectGrabCheck(Transform controller, bool grab, bool altGrab, bool pGrab)
        {
            if (!MeasureTool.grabNode) return;
            if (!MeasureTool.Grabbing)
            {
                if (!(Vector3.Distance(transform.position, controller.position) < DirectDistance))
                {
                    return;
                }
            }
            if (MeasureTool.FocusMeasureNode != null && MeasureTool.FocusMeasureNode != this && MeasureTool.Grabbing) return;
            if (grab && !pGrab)
            {
                MeasureTool.GrabNode(this, MeasureTape);
                MeasureTool.DeactivateAllTapes();
                MeasureTool.Grabbing =  !LockNode;
                switch (NodeIndex)
                {
                    case 0 when MeasureTape.measureNodes.Count > 1:
                        previousNode = MeasureTape.measureNodes[NodeIndex + 1];
                        MeasureTool.PreviousMeasureNode = previousNode;
                        previousNode.NodeStart();
                        break;
                    case 0:
                        return;
                    default:
                        previousNode = MeasureTape.measureNodes[NodeIndex - 1];
                        MeasureTool.PreviousMeasureNode = previousNode;
                        previousNode.NodeStart();
                        break;
                }
                return;
            }
            if (grab && !LockNode)
            {
                switch (NodeIndex)
                {
                    case 0 when MeasureTape.measureNodes.Count == 0:
                        transform.TransformLerpPosition(controller, .85f);
                        MeasureTape.AdjustTape();
                        break;
                    default:
                        MeasureTool.NodeSnap(controller, this,  previousNode, MeasureTape);
                        break;
                }
            }
            else if (MeasureTool.Grabbing && !altGrab)
            {
                MeasureTool.Grabbing = false;
                MeasureTool.MeasureNode = null;
                MeasureTool.PreviousMeasureNode = MeasureTape.measureNodes[MeasureTape.measureNodes.Count - 1];
                MeasureTool.PreviousMeasureNode.NodeStart();
                if (previousNode == null) return;
                previousNode.NodeEnd();
            }
        }

        public void DeleteNode()
        {
            switch (MeasureTape.measureNodes.Count)
            {
                case 0:
                    break;
                default:
                    MeasureTool.PreviousMeasureNode = MeasureTape.measureNodes[MeasureTape.measureNodes.Count - 1];
                    MeasureTool.PreviousMeasureNode.NodeStart();
                    break;
            }
            MeasureTape.measureNodes.RemoveAt(NodeIndex);
            Destroy(transform.gameObject);
        }

        public void NodeStart()
        {
            if (!MeasureTool.axisSnapping) return;
            X.SetActive(true);
            Y.SetActive(true);
            Z.SetActive(true);
        }

        private void NodeStay()
        {
            if (!MeasureTool.axisSnapping) return;
            SnappingTransforms();

            XSnap = CheckSnap(xDistance, yDistance, zDistance, MeasureTool.snapTolerance);
            YSnap = CheckSnap(yDistance, xDistance, zDistance, MeasureTool.snapTolerance);
            ZSnap = CheckSnap(zDistance, xDistance, yDistance, MeasureTool.snapTolerance);
            
            xLr.StraightLineRender(transform, X.transform);
            yLr.StraightLineRender(transform, Y.transform);
            zLr.StraightLineRender(transform, Z.transform);
            
            xLr.LineRenderWidth(XSnap ? MeasureTool.activeWidth : MeasureTool.inactiveWidth, XSnap ? MeasureTool.activeWidth : MeasureTool.inactiveWidth);
            yLr.LineRenderWidth(YSnap ? MeasureTool.activeWidth : MeasureTool.inactiveWidth, YSnap ? MeasureTool.activeWidth : MeasureTool.inactiveWidth);
            zLr.LineRenderWidth(ZSnap ? MeasureTool.activeWidth : MeasureTool.inactiveWidth, ZSnap ? MeasureTool.activeWidth : MeasureTool.inactiveWidth);
        }

        public void NodeEnd()
        {
            if (!MeasureTool.axisSnapping) return;
            X.SetActive(false);
            Y.SetActive(false);
            Z.SetActive(false);
        }

        private void SnappingTransforms()
        {
            dominantFollow.transform.Transforms(MeasureTool.dominant.transform);
            
            X.transform.LockAxis(dominantFollow.transform, Set.Axis.X);
            Y.transform.LockAxis(dominantFollow.transform, Set.Axis.Y);
            Z.transform.LockAxis(dominantFollow.transform, Set.Axis.Z);

            xDistance = SnapDistance(X);
            yDistance = SnapDistance(Y);
            zDistance = SnapDistance(Z);
        }

        private float SnapDistance(GameObject axis)
        {
            return Vector3.Distance(axis.transform.position, dominantFollow.transform.position);
        }

        private static bool CheckSnap(float a, float b, float c, float threshold)
        {
            return a < threshold && a < b && a < c;
        }

        public void SetColor(Color color)
        {
            skinnedMeshRenderer.material.color = color;
        }
    }
}
