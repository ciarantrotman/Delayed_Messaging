using System.Collections.Generic;
using Delayed_Messaging.Scripts.Units;
using Delayed_Messaging.Scripts.Utilities;
using UnityEngine;
using VR_Prototyping.Scripts;
using VR_Prototyping.Scripts.Utilities;

namespace Delayed_Messaging.Scripts.Player
{
    [DisallowMultipleComponent, RequireComponent(typeof(ControllerTransforms))]
    public class UnitController : MonoBehaviour
    {
        private ControllerTransforms controllerTransforms;
        private Player player;
        private Selection selection;
        private Cast cast;

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
        private int layerIndex = 15;

        [Header("Aesthetic Settings")]
        [SerializeField] private GameObject targetVisual;
        [SerializeField] private Material lineRenderMat;

        private Transform unitDestination;
        
        private void Start()
        {
            controllerTransforms = GetComponent<ControllerTransforms>();
            player = GetComponent<Player>();
            selection = GetComponent<Selection>();
            cast = gameObject.AddComponent<Cast>();
            cast.SetupCastObjects(targetVisual, transform, "Unit Controller", lineRenderMat, maximumAngle, minimumAngle, maximumMoveDistance, minimumMoveDistance,  .0075f, controllerTransforms);
        }

        private void LateUpdate()
        {
            this.MoveUnit(controllerTransforms.LeftGrab(),pGrabL, cast.lCastObject.visual, cast.lCastObject.lineRenderer, selection.selectionObjectsL.list);
            this.MoveUnit(controllerTransforms.RightGrab(),pGrabR,cast.rCastObject.visual, cast.rCastObject.lineRenderer, selection.selectionObjectsR.list);

            pGrabL = controllerTransforms.LeftGrab();
            pGrabR = controllerTransforms.RightGrab();
        }

        public static void UnitMoveStart(GameObject visual, LineRenderer lr)
        {
            visual.SetActive(true);
            lr.enabled = true;
        }
        
        public void UnitMoveEnd(GameObject visual, LineRenderer lr, IEnumerable<BaseObject> units)
        {
            visual.SetActive(false);
            lr.enabled = false;

            foreach (BaseObject baseObject in units)
            {
                if (baseObject is Unit)
                {
                    Unit selectedUnit = baseObject.GetComponent<Unit>();
                    if (selectedUnit == null)
                    {
                        continue;
                    }
                    selectedUnit.SetDestination(visual.transform.position);
                }
            }
        }
    }
}