using System;
using UnityEngine;
using VR_Prototyping.Scripts;
using VR_Prototyping.Scripts.Utilities;

namespace Delayed_Messaging.Scripts.Interaction
{
    [RequireComponent(typeof(ControllerTransforms)), RequireComponent(typeof(Selection))]
    public class UserInterface : MonoBehaviour
    {
        private ControllerTransforms controllerTransforms;
        private Selection selection;
        public enum DominantHand
        {
            LEFT,
            RIGHT
        }
        
        [Header("User Interface Settings")]
        [SerializeField] private DominantHand dominantHand;
        [SerializeField, Range(0f, 20f)] private int userInterfaceInteractionLayer;
        [SerializeField, Space(10), Range(0f, 10f)] private float interactionRange;
        [SerializeField, Space(10)] private Material lineRendererMat;
        [SerializeField] private GameObject cursorObject;

        private GameObject uiSelectionOrigin;
        private RaycastCursor cursor;
        private LineRenderer uiLineRenderer;

        private RaycastInterface currentInterface;

        private void Start()
        {
            controllerTransforms = GetComponent<ControllerTransforms>();
            selection = GetComponent<Selection>();
            SetupGameObjects();
        }

        private void SetupGameObjects()
        {
            uiSelectionOrigin = new GameObject("[UI Selection Origin]");
            uiSelectionOrigin.transform.SetParent(dominantHand == DominantHand.LEFT ? controllerTransforms.LeftTransform() : controllerTransforms.RightTransform());

            cursorObject = Instantiate(cursorObject);
            cursor = cursorObject.GetComponent<RaycastCursor>();

            uiLineRenderer = cursorObject.AddComponent<LineRenderer>();
            uiLineRenderer.SetupLineRender(lineRendererMat, .001f, false);
        }

        private void Update()
        {
            switch (Physics.Raycast(uiSelectionOrigin.transform.position, uiSelectionOrigin.transform.forward, out RaycastHit hit, interactionRange, 1 << userInterfaceInteractionLayer))
            {
                case true when currentInterface == null:
                    cursor.EnableCursor(true);
                    uiLineRenderer.enabled = true;
                    currentInterface = hit.collider.gameObject.GetComponent<RaycastInterface>();
                    currentInterface.HoverStart();
                    selection.ToggleSelectionState(dominantHand, false);
                    break;
                case true when currentInterface != null:
                    if (currentInterface != hit.collider.gameObject.GetComponent<RaycastInterface>())
                    {
                        currentInterface.HoverEnd();
                        currentInterface = hit.collider.gameObject.GetComponent<RaycastInterface>();
                        currentInterface.HoverStart();
                    }
                    currentInterface.HoverStay();
                    cursorObject.transform.position = hit.point;
                    uiLineRenderer.DrawStraightLineRender(cursorObject.transform, uiSelectionOrigin.transform);
                    break;
                case false when currentInterface != null:
                    currentInterface.HoverEnd();
                    cursor.EnableCursor(false);
                    uiLineRenderer.enabled = false;
                    currentInterface = null;
                    selection.ToggleSelectionState(dominantHand, true);
                    break;
                default:
                    break;
            }
        }
        
        private void OnDrawGizmos () 
        {
            if (controllerTransforms != null && controllerTransforms.debugActive)
            {
                DrawGizmos ();
            }
        }

        private void DrawGizmos ()
        {
            Gizmos.color = Color.yellow;
            Gizmos.DrawRay(uiSelectionOrigin.transform.position, (dominantHand == DominantHand.LEFT ? controllerTransforms.LeftForwardVector() : controllerTransforms.RightForwardVector()) * interactionRange);
        }
    }
}
