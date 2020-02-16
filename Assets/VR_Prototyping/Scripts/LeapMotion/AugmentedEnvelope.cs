using System;
using Sirenix.OdinInspector;
using UnityEngine;

namespace VR_Prototyping.Scripts.LeapMotion
{
    [RequireComponent(typeof(ControllerTransforms))]
    public class AugmentedEnvelope : MonoBehaviour
    {
        private ControllerTransforms controller;

        private GameObject anchor; // common parent
        private GameObject lAnchor; // local parent
        private GameObject aAnchor; // augmented parent

        private GameObject lHl; // left hand local
        private GameObject lTl; // left thumb local
        private GameObject lIl; // left index local
        private GameObject lMl; // left middle local
        private GameObject lRl; // left ring local
        private GameObject lLl; // left little local

        private GameObject rHl; // right hand local
        private GameObject rTl; // right thumb local
        private GameObject rIl; // right index local
        private GameObject rMl; // right middle local
        private GameObject rRl; // right ring local
        private GameObject rLl; // right little local

        private GameObject lHa; // left hand augmented
        private GameObject lTa; // left thumb augmented
        private GameObject lIa; // left index augmented
        private GameObject lMa; // left middle augmented
        private GameObject lRa; // left ring augmented
        private GameObject lLa; // left little augmented

        private GameObject rHa; // right hand augmented
        private GameObject rTa; // right thumb augmented
        private GameObject rIa; // right index augmented
        private GameObject rMa; // right middle augmented
        private GameObject rRa; // right ring augmented
        private GameObject rLa; // right little augmented

        private LineRenderer lTLr;
        private LineRenderer lILr;
        private LineRenderer lMLr;
        private LineRenderer lRLr;
        private LineRenderer lLLr;
        
        private LineRenderer rTLr;
        private LineRenderer rILr;
        private LineRenderer rMLr;
        private LineRenderer rRLr;
        private LineRenderer rLLr;

        private void Start()
        {
            SetupReferences();
            SetupLocalObjects();
            SetupAugmentedObjects();
            SetupLineRenders();
        }
        private void SetupReferences()
        {
            controller = GetComponent<ControllerTransforms>();
            anchor = Set.NewGameObject(gameObject, "[Augmented Rig]");
        }
        private void SetupLocalObjects()
        {
            lAnchor = Set.NewGameObject(anchor, "Local/Anchor");
            
            lHl = Set.NewGameObject(lAnchor, "Local/Hand/Left");
            lTl = Set.NewGameObject(lHl, "Local/Thumb/Left");
            lIl = Set.NewGameObject(lHl, "Local/Index/Left");
            lMl = Set.NewGameObject(lHl, "Local/Middle/Left");
            lRl = Set.NewGameObject(lHl, "Local/Ring/Left");
            lLl = Set.NewGameObject(lHl, "Local/Little/Left");
            
            rHl = Set.NewGameObject(lAnchor, "Local/Hand/Right");
            rTl = Set.NewGameObject(rHl, "Local/Thumb/Right");
            rIl = Set.NewGameObject(rHl, "Local/Index/Right");
            rMl = Set.NewGameObject(rHl, "Local/Middle/Right");
            rRl = Set.NewGameObject(rHl, "Local/Ring/Right");
            rLl = Set.NewGameObject(rHl, "Local/Little/Right");
        }
        private void SetupAugmentedObjects()
        {
            aAnchor = Set.NewGameObject(anchor, "Augmented/Anchor");
            
            lHa = Set.NewGameObject(aAnchor, "Augmented/Hand/Left");
            lTa = Set.NewGameObject(lHa, "Augmented/Thumb/Left");
            lIa = Set.NewGameObject(lHa, "Augmented/Index/Left");
            lMa = Set.NewGameObject(lHa, "Augmented/Middle/Left");
            lRa = Set.NewGameObject(lHa, "Augmented/Ring/Left");
            lLa = Set.NewGameObject(lHa, "Augmented/Little/Left");
            
            rHa = Set.NewGameObject(aAnchor, "Augmented/Hand/Right");
            rTa = Set.NewGameObject(rHa, "Augmented/Thumb/Right");
            rIa = Set.NewGameObject(rHa, "Augmented/Index/Right");
            rMa = Set.NewGameObject(rHa, "Augmented/Middle/Right");
            rRa = Set.NewGameObject(rHa, "Augmented/Ring/Right");
            rLa = Set.NewGameObject(rHa, "Augmented/Little/Right");
        }

        private void SetupLineRenders()
        {
            lTLr = lTa.transform.AddOrGetLineRenderer();
            lILr = lIa.transform.AddOrGetLineRenderer();
            lMLr = lMa.transform.AddOrGetLineRenderer();
            lRLr = lRa.transform.AddOrGetLineRenderer();
            lLLr = lLa.transform.AddOrGetLineRenderer();
            
            rTLr = rTa.transform.AddOrGetLineRenderer();
            rILr = rIa.transform.AddOrGetLineRenderer();
            rMLr = rMa.transform.AddOrGetLineRenderer();
            rRLr = rRa.transform.AddOrGetLineRenderer();
            rLLr = rLa.transform.AddOrGetLineRenderer();
            
            lTLr.SetupLineRender(controller.doubleSidedLineRenderMat, .005f, true);
            lILr.SetupLineRender(controller.doubleSidedLineRenderMat, .005f, true);
            lMLr.SetupLineRender(controller.doubleSidedLineRenderMat, .005f, true);
            lRLr.SetupLineRender(controller.doubleSidedLineRenderMat, .005f, true);
            lLLr.SetupLineRender(controller.doubleSidedLineRenderMat, .005f, true);
            
            rTLr.SetupLineRender(controller.doubleSidedLineRenderMat, .005f, true);
            rILr.SetupLineRender(controller.doubleSidedLineRenderMat, .005f, true);
            rMLr.SetupLineRender(controller.doubleSidedLineRenderMat, .005f, true);
            rRLr.SetupLineRender(controller.doubleSidedLineRenderMat, .005f, true);
            rLLr.SetupLineRender(controller.doubleSidedLineRenderMat, .005f, true);
        }

        private void Update()
        {
            anchor.transform.Transforms(controller.CameraTransform());
            SetLocalPositions(lHl, lTl, lIl, lMl, lRl, lLl, controller.leftWrist, controller.leftThumb, controller.leftIndex, controller.leftMiddle, controller.leftRing, controller.leftLittle);
            SetLocalPositions(rHl, rTl, rIl, rMl, rRl, rLl, controller.rightWrist, controller.rightThumb, controller.rightIndex, controller.rightMiddle, controller.rightRing, controller.rightLittle);
            SetAugmentedPositions(lHa, lTa, lIa, lMa, lRa, lLa, lHl, lTl, lIl, lMl, lRl, lLl);
            SetAugmentedPositions(rHa, rTa, rIa, rMa, rRa, rLa, rHl, rTl, rIl, rMl, rRl, rLl);
            SetHandLineRenders(lHa, lTa, lIa, lMa, lRa, lLa, lTLr, lILr, lMLr, lRLr, lLLr);
            SetHandLineRenders(rHa, rTa, rIa, rMa, rRa, rLa, rTLr, rILr, rMLr, rRLr, rLLr);
        }

        private static void SetLocalPositions(GameObject h, GameObject t, GameObject i, GameObject m, GameObject r, GameObject l, Transform rH, Transform rT, Transform rI, Transform rM, Transform rR, Transform rL)
        {
            h.transform.Transforms(rH);
            t.transform.Transforms(rT);
            i.transform.Transforms(rI);
            m.transform.Transforms(rM);
            r.transform.Transforms(rR);
            l.transform.Transforms(rL);
        }
        
        private static void SetAugmentedPositions(GameObject h, GameObject t, GameObject i, GameObject m, GameObject r, GameObject l, GameObject rH, GameObject rT, GameObject rI, GameObject rM, GameObject rR, GameObject rL)
        {
            h.transform.LocalTransforms(rH.transform);
            t.transform.LocalTransforms(rT.transform);
            i.transform.LocalTransforms(rI.transform);
            m.transform.LocalTransforms(rM.transform);
            r.transform.LocalTransforms(rR.transform);
            l.transform.LocalTransforms(rL.transform);
        }

        private static void SetHandLineRenders(GameObject h, GameObject t, GameObject i, GameObject m, GameObject r, GameObject l, LineRenderer tL, LineRenderer iL, LineRenderer mL, LineRenderer rL, LineRenderer lL)
        {
            tL.StraightLineRender(h.transform, t.transform);
            iL.StraightLineRender(h.transform, i.transform);
            mL.StraightLineRender(h.transform, m.transform);
            rL.StraightLineRender(h.transform, r.transform);
            lL.StraightLineRender(h.transform, l.transform);
        }
    }
}
