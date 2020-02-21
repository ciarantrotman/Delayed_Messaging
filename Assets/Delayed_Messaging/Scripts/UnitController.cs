using System;
using System.Collections.Generic;
using Delayed_Messaging.Scripts.Units;
using UnityEngine;
using VR_Prototyping.Scripts;
using VR_Prototyping.Scripts.Utilities;

namespace Delayed_Messaging.Scripts
{
    [DisallowMultipleComponent, RequireComponent(typeof(ControllerTransforms))]
    public class UnitController : MonoBehaviour
    {
        private ControllerTransforms controllerTransforms;
        private Player.Player player;
        
        private GameObject parent;

        private GameObject rCf; // follow
        private GameObject rCp; // proxy
        private GameObject rCn; // normalised
        private GameObject rMp; // midpoint
        private GameObject rTs; // target
        private GameObject rHp; // hit
        private GameObject rVisual; // visual
        private GameObject rRt; // rotation

        private GameObject lCf; // follow
        private GameObject lCp; // proxy
        private GameObject lCn; // normalised
        private GameObject lMp; // midpoint
        private GameObject lTs; // target
        private GameObject lHp; // hit
        private GameObject lVisual; // visual
        private GameObject lRt; // rotation

        private Vector3 rLastValidPosition;
        private Vector3 lLastValidPosition;

        [HideInInspector] public LineRenderer lLineRenderer;
        [HideInInspector] public LineRenderer rLineRenderer;
        private Material lLineRendererMaterial;
        private Material rLineRendererMaterial;
        private static readonly int Distance = Shader.PropertyToID("_Distance");

        private bool pGrabR;
        private bool pGrabL;

        private Vector3 rRotTarget;
        private bool active;

        private Vector3 customRotation;
        private Vector3 customPosition;
        
        [Header("Casting Settings")]
        [SerializeField, Range(0f, 180f)] private float minimumAngle = 60f;
        [SerializeField, Range(0f, 180f)] private float maximumAngle = 110f;
        [SerializeField, Range(.1f, 1f)] private float minimumMoveDistance = .5f;
        [SerializeField, Range(1f, 100f)] private float maximumMoveDistance = 15f;
        [SerializeField, Range(1, 15), Space(10)]
        private int layerIndex = 10;

        [Header("Aesthetic Settings")]
        [SerializeField, Space(5)] private GameObject targetVisual;
        [SerializeField] private Material lineRenderMat;
        [SerializeField, Range(3f, 50f)] private int lineRenderQuality = 40;

        private Transform unitDestination;
        
        private void Start()
        {
            controllerTransforms = GetComponent<ControllerTransforms>();
            player = GetComponent<Player.Player>();
            SetupGameObjects();
        }

        private void SetupGameObjects()
        {
            parent = new GameObject("[Unit Controller/Calculations]");
            Transform parentTransform = parent.transform;
            parentTransform.SetParent(transform);

            rCf = new GameObject("[Unit Controller/Follow/Right]");
            rCp = new GameObject("[Unit Controller/Proxy/Right]");
            rCn = new GameObject("[Unit Controller/Normalised/Right]");
            rMp = new GameObject("[Unit Controller/MidPoint/Right]");
            rTs = new GameObject("[Unit Controller/Target/Right]");
            rHp = new GameObject("[Unit Controller/HitPoint/Right]");
            rRt = new GameObject("[Unit Controller/Rotation/Right]");
            
            lCf = new GameObject("[Unit Controller/Follow/Left]");
            lCp = new GameObject("[Unit Controller/Proxy/Left]");
            lCn = new GameObject("[Unit Controller/Normalised/Left]");
            lMp = new GameObject("[Unit Controller/MidPoint/Left]");
            lTs = new GameObject("[Unit Controller/Target/Left]");
            lHp = new GameObject("[Unit Controller/HitPoint/Left]");
            lRt = new GameObject("[Unit Controller/Rotation/Left]");
            
            rVisual = Instantiate(targetVisual, rHp.transform);
            rVisual.name = "[Unit Controller/Visual/Right]";
            rVisual.SetActive(false);
            
            lVisual = Instantiate(targetVisual, lHp.transform);
            lVisual.name = "[Unit Controller/Visual/Left]";
            lVisual.SetActive(false);

            rCf.transform.SetParent(parentTransform);
            rCp.transform.SetParent(rCf.transform);
            rCn.transform.SetParent(rCf.transform);
            rMp.transform.SetParent(rCp.transform);
            rTs.transform.SetParent(rCn.transform);
            rHp.transform.SetParent(transform);
            rRt.transform.SetParent(rHp.transform);
            
            lCf.transform.SetParent(parentTransform);
            lCp.transform.SetParent(lCf.transform);
            lCn.transform.SetParent(lCf.transform);
            lMp.transform.SetParent(lCp.transform);
            lTs.transform.SetParent(lCn.transform);
            lHp.transform.SetParent(transform);
            lRt.transform.SetParent(lHp.transform);
            
            rLineRenderer = rCp.AddComponent<LineRenderer>();
            lLineRenderer = lCp.AddComponent<LineRenderer>();

            rLineRenderer.SetupLineRender(lineRenderMat, .005f, false);
            lLineRenderer.SetupLineRender(lineRenderMat, .005f, false);
            
            rLineRendererMaterial = rLineRenderer.material;
            lLineRendererMaterial = lLineRenderer.material;
        }
        
        private void Update()
        {
            rLastValidPosition = rTs.LastValidPosition(rLastValidPosition);
            lLastValidPosition = lTs.LastValidPosition(lLastValidPosition);
            
            Set.DistanceCast(rTs, rCf, rCp, rCn, rHp, rMp, rRt, rVisual, 
                controllerTransforms.RightTransform(), controllerTransforms.CameraTransform(), controllerTransforms.RightJoystick(), maximumAngle, minimumAngle,
                minimumMoveDistance, maximumMoveDistance, rLastValidPosition);
            Set.DistanceCast(lTs, lCf, lCp, lCn, lHp, lMp, lRt, lVisual, 
                controllerTransforms.LeftTransform(), controllerTransforms.CameraTransform(), controllerTransforms.LeftJoystick(), maximumAngle, minimumAngle,
                minimumMoveDistance, maximumMoveDistance, lLastValidPosition);

            // draw the line renderer
            rLineRenderer.BezierLineRenderer(controllerTransforms.RightTransform().position,rMp.transform.position,rHp.transform.position,lineRenderQuality);
            lLineRenderer.BezierLineRenderer(controllerTransforms.LeftTransform().position, lMp.transform.position, lHp.transform.position, lineRenderQuality);
            lLineRendererMaterial.SetFloat(Distance, transform.TransformDistance(lVisual.transform));
            rLineRendererMaterial.SetFloat(Distance, transform.TransformDistance(rVisual.transform));
        }

        private void LateUpdate()
        {
            this.MoveUnit(controllerTransforms.LeftGrab(),pGrabL, lVisual, lLineRenderer);
            this.MoveUnit(controllerTransforms.RightGrab(),pGrabR, rVisual, rLineRenderer);

            pGrabL = controllerTransforms.LeftGrab();
            pGrabR = controllerTransforms.RightGrab();
        }

        public static void UnitMoveStart(GameObject visual, LineRenderer lr)
        {
            visual.SetActive(true);
            lr.enabled = true;
        }
        
        public void UnitMoveEnd(GameObject visual, LineRenderer lr)
        {
            visual.SetActive(false);
            lr.enabled = false;

            foreach (BaseObject selectedObject in player.selectedObjects)
            {
                if (selectedObject is Unit)
                {
                    selectedObject.GetComponent<Unit>().Move(visual.transform.position);
                }
            }
        }
    }
}