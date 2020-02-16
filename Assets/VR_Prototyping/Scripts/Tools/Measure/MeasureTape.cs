using System;
using System.Collections.Generic;
using UnityEngine;

namespace VR_Prototyping.Scripts.Tools.Measure
{
    public class MeasureTape : MonoBehaviour
    {
        public MeasureTool MeasureTool { private get; set; }
        public ControllerTransforms Controller { private get; set; }
        public List<MeasureNode> measureNodes = new List<MeasureNode>();
        public LineRenderer TapeLr { get; set; }
        public string TapeName { get; set; }

        private Vector3 x;
        private Vector3 xP;
        private bool rSelectP;
        private bool lSelectP;

        public Color tapeColor;
        private static readonly int LineRenderDiffuse = Shader.PropertyToID("_Diffusecolor");
        private static readonly int LineRenderBase = Shader.PropertyToID("_BaseColor");
        private static readonly int LineRenderEmission = Shader.PropertyToID("_EmissionColor");

        public float TapeDistance()
        {
            int count = 0;
            float distance = 0f;
            Vector3 previousNode = Vector3.zero;
            
            foreach (MeasureNode node in measureNodes)
            {
                Vector3 position = node.transform.position;
                
                TapeLr.SetPosition(count, position);

                previousNode = previousNode == Vector3.zero ? position : previousNode;
                node.Distance = Vector3.Distance(position, previousNode);
                switch (node.LockNode)
                {
                    case true:
                        node.Text.SetText("<b>Locked</b>\n\nNode {0}\n{1:2}m", node.NodeIndex, node.Distance);
                        break;
                    default:
                        node.Text.SetText("Node {0}\n{1:2}m", node.NodeIndex, node.Distance);
                        break;
                }
                previousNode = position;
                distance += node.Distance;
                count++;
            }

            return distance;
        }

        private void Update()
        {
            NodeSnapping();
            
            if (measureNodes.Count < 2 || MeasureTool.MeasureTape != this || !MeasureTool.Active) return;
            
            NodeInsertion();
        }

        private void NodeInsertion()
        {
            int index = 0;
            int intersectionCount = 0;
            
            for (int i = 0; i < measureNodes.Count - 1; i++)
            {
                Vector3 currNodePos = measureNodes[i].transform.position;
                Vector3 nextNodePos = measureNodes[i + 1].transform.position;
                Vector3 line = measureNodes[i + 1].transform.position - measureNodes[i].transform.position;
                switch (MeasureTool.ToolMenu.dominantHand)
                {
                    case ToolMenu.Handedness.LEFT:
                        x = Intersection.Line(
                            Controller.LeftPosition(),
                            Controller.LeftForwardVector(),
                            currNodePos,
                            line,
                            MeasureTool.tolerance);
                        if (xP != Vector3.zero)
                        {
                            if (currNodePos.IsCollinear(nextNodePos, xP, MeasureTool.tolerance))
                            {
                                x = xP;
                                intersectionCount++;
                            }
                            
                            if (currNodePos.IsCollinear(nextNodePos, xP, MeasureTool.tolerance) && !Controller.LeftSelect() && lSelectP)
                            {
                                index = i + 1;
                            }
                        }
                        break;
                    case ToolMenu.Handedness.RIGHT:
                        xP = Intersection.Line(
                            Controller.RightPosition(),
                            Controller.RightForwardVector(),
                            currNodePos,
                            line,
                            MeasureTool.tolerance);
                        if (xP != Vector3.zero)
                        {
                            if (currNodePos.IsCollinear(nextNodePos, xP, MeasureTool.tolerance))
                            {
                                x = xP;
                                intersectionCount++;
                            }
                            
                            if (currNodePos.IsCollinear(nextNodePos, xP, MeasureTool.tolerance) && !Controller.RightSelect() && rSelectP)
                            {
                                index = i + 1;
                            }
                        }
                        break;
                    default:
                        throw new ArgumentOutOfRangeException();
                }
            }

            MeasureTool.Insertion = MeasureTool.Active && 
                                    MeasureTool.nodeInsertion && 
                                    intersectionCount > 0 
                                    && MeasureTool.Placing == false && 
                                    MeasureTool.Grabbing == false && 
                                    Vector3.Distance(x, MeasureTool.dominant.transform.position) <= MeasureTool.insertionThreshold;
            
            if (MeasureTool.Insertion)
            {
                MeasureTool.intersectionPointPrefab.transform.position = x;
                MeasureTool.intersectionPointPrefab.transform.LookAwayFrom(Controller.CameraTransform(), Vector3.up);
                switch (MeasureTool.ToolMenu.dominantHand)
                {
                    case ToolMenu.Handedness.RIGHT when !Controller.RightSelect() && rSelectP && index > 0 && Vector3.Distance(x, Controller.RightPosition())  > .02f:
                        MeasureTool.InsertNode(this, x, index);
                        break;
                    case ToolMenu.Handedness.LEFT when !Controller.LeftSelect() && lSelectP && index > 0 && Vector3.Distance(x, Controller.LeftPosition()) > .02f:
                        MeasureTool.InsertNode(this, x, index);
                        break;
                }
            }
            else
            {
                MeasureTool.intersectionPointPrefab.transform.position = Vector3.zero;
            }

            rSelectP = Controller.RightSelect();
            lSelectP = Controller.LeftSelect();
        }

        private void NodeSnapping()
        {
            if (!MeasureTool.axisSnapping) return;
            
            foreach (MeasureNode node in measureNodes)
            {
                switch (node.NodeIndex == 0)
                {
                    case true:
                        node.transform.rotation = Quaternion.identity;
                        break;
                    default:
                        OrientNodes(node.transform, measureNodes[node.NodeIndex - 1].transform);
                        break;
                }
            }
        }
        
        private void OrientNodes(Transform currentNode, Transform previousNode)
        {
            switch (MeasureTool.nodeLockingType)
            {
                case MeasureTool.NodeLockingType.RELATIVE:
                    currentNode.LookAt(previousNode);
                    break;
                case MeasureTool.NodeLockingType.RELATIVE_VERTICAL:
                    currentNode.LookAtVertical(previousNode);
                    break;
                case MeasureTool.NodeLockingType.GLOBAL:
                    currentNode.rotation = Quaternion.identity;
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
            
            if(!Controller.debugActive) return;
            
            Transform nodeTransform = currentNode.transform;
            Vector3 nodePosition = nodeTransform.position;
            
            Debug.DrawRay(nodePosition, nodeTransform.up * .25f, Color.green);
            Debug.DrawRay(nodePosition, nodeTransform.right * .25f, Color.red);
            Debug.DrawRay(nodePosition, nodeTransform.forward * .25f, Color.blue);
        }
        
        public void SetTapeState(bool state)
        {
            foreach (MeasureNode node in measureNodes)
            {
                node.LockNode = state;
            }
        }

        public void RefactorNodes()
        {
            int index = 0;
            foreach (MeasureNode node in measureNodes)
            {
                node.NodeIndex = index;
                node.name = "Node_" + index;
                index++;
            }
            AdjustTape();
        }

        public void AdjustTape()
        {
            TapeDistance();
        }

        public void SetColor(Color color)
        {
            tapeColor = color;
            
            TapeLr.material.SetColor(LineRenderDiffuse, color);
            TapeLr.material.SetColor(LineRenderBase, color);
            TapeLr.material.SetColor(LineRenderEmission, color);
            
            foreach (MeasureNode node in measureNodes)
            {
                node.SetColor(color);
            }
        }

        public void DeactivateAllNodes()
        {
            foreach (MeasureNode node in measureNodes)
            {
                node.NodeEnd();
            }
        }
    }
}
