using System;
using DG.Tweening;
using Sirenix.OdinInspector;
using UnityEngine;
using VR_Prototyping.Scripts.Keyboard;

namespace VR_Prototyping.Scripts.Tools
{
    [RequireComponent(typeof(ControllerTransforms))]
    public class ToolMenu : MonoBehaviour
    {
        [HideInInspector] public bool active;
        [HideInInspector] public bool rubberBanded;
        private bool cMenu;
        private bool pMenu;
        
        private bool trigger;
        private bool initialised;

        private GameObject keyboard;
        
        private ControllerTransforms controller;
        private ToolController toolController;
        
        [BoxGroup("Script Setup")] [Required] [SerializeField] private GameObject menuPrefab;
        
        [BoxGroup("Keyboard Controls")] public GameObject keyboardPrefab;
        
        [BoxGroup("RubberBanding Settings")] [ShowIf("rubberBanded")] [SerializeField][Range(0,1)] private float moveSpeed = .5f;
        [BoxGroup("RubberBanding Settings")] [ShowIf("rubberBanded")] [Space(10)] [SerializeField] private float angleThreshold = 45f;
        [BoxGroup("RubberBanding Settings")] [ShowIf("rubberBanded")] [SerializeField] private float distanceThreshold = .1f;
        [SerializeField] public enum Handedness
        {
            RIGHT,
            LEFT
        }
        [BoxGroup("Tool Settings")] public Handedness dominantHand;

        public KeyboardManager KeyboardManager { get; private set; }
        
        private void Awake()
        {
            controller = GetComponent<ControllerTransforms>();
            SetupMenu();
            SetupKeyboard();
        }

        private void SetupMenu()
        {
            menuPrefab = Instantiate(menuPrefab);
            menuPrefab.name = "[ICON Tool Menu]";
            toolController = menuPrefab.GetComponent<ToolController>();
            toolController.Initialise(gameObject, active, controller, dominantHand, this);
            toolController.ToggleButtonState(false);
        }

        private void SetupKeyboard()
        {
            if (!initialised)
            {
                keyboard = Instantiate(keyboardPrefab);
                keyboard.name = "[Indirect Keyboard]";
                KeyboardManager = keyboard.GetComponent<KeyboardManager>();
            }
            
            switch (dominantHand)
            {
                case Handedness.RIGHT:
                    KeyboardManager.InitialiseKeyboard(controller, this, controller.LeftTransform());
                    break;
                case Handedness.LEFT:
                    KeyboardManager.InitialiseKeyboard(controller, this, controller.RightTransform());
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }

            keyboard.transform.localPosition = new Vector3(.1f, -.02f, -.05f);

            initialised = true;
        }

        private void Update()
        {
            switch (dominantHand)
            {
                case Handedness.RIGHT:
                    cMenu = controller.RightMenu();
                    CheckState(cMenu, pMenu, controller.RightTransform());
                    RubberBanded(controller.RightTransform());
                    pMenu = cMenu;
                    return;
                case Handedness.LEFT:
                    cMenu = controller.LeftMenu();
                    CheckState(cMenu, pMenu, controller.LeftTransform());
                    RubberBanded(controller.LeftTransform());
                    pMenu = cMenu;
                    return;
                default:
                    return;
            }
        }

        private void CheckState(bool current, bool previous, Transform c)
        {
            if (current || !previous) return;
            
            active = !active;
            SetState(active, c);
        }

        public void SetState(bool state, Transform c)
        {
            toolController.ToggleButtonState(state);
            toolController.toolMenuHeader.SetActive(state);
            toolController.toolMenuFooter.SetActive(state);
            active = state;
            rubberBanded = state;
            
            if(!state) return;
            menuPrefab.transform.position = c.position;
            c.SplitRotation(menuPrefab.transform, false);
            toolController.SetAllToolState(false);
        }

        private void RubberBanded(Transform target)
        {
            if(!rubberBanded) return;
            
            float d = Vector3.Distance(menuPrefab.transform.position, target.position);
            float a = menuPrefab.transform.Divergence(target);

            if (!(a >= angleThreshold) && !(d >= distanceThreshold)) return;
            
            var rotTarget = new Vector3(0, target.rotation.eulerAngles.y, 0);
            
            menuPrefab.transform.DORotate(rotTarget, moveSpeed);
            menuPrefab.transform.DOMove(target.position, moveSpeed);
        }
    }
}
