using System.Collections.Generic;
using DG.Tweening;
using Sirenix.OdinInspector;
using UnityEngine;

namespace VR_Prototyping.Scripts.Icon_Scripts
{
    [RequireComponent(typeof(ControllerTransforms)), DisallowMultipleComponent]
    public class IconMenu : MonoBehaviour
    {
        public bool Active { get; set; }
        public bool rubberBanded  { get; set; }
        
        private bool cMenu;
        private bool pMenu;
        
        private bool trigger;
        
        private ControllerTransforms controller;
        private IconMenuController iconMenuController;
        
        [BoxGroup("Script Setup")] [Required] [SerializeField] private GameObject menuPrefab;
        [BoxGroup("Script Setup")] [Required] [SerializeField] [Range(.5f, 2.5f)] private float menuSpawnOffset;

        [BoxGroup("RubberBanding Settings")] [SerializeField] [Range(0,1)] private float moveSpeed = .5f;
        [BoxGroup("RubberBanding Settings")] [Space(10)] [SerializeField] private float angleThreshold = 45f;
        [BoxGroup("RubberBanding Settings")] [SerializeField] private float distanceThreshold = .1f;

        private void Awake()
        {
            controller = GetComponent<ControllerTransforms>();
            SetupMenu();
        }

        private void SetupMenu()
        {
            menuPrefab = Instantiate(menuPrefab, transform);
            menuPrefab.name = "[ICON Menu]";
            iconMenuController = menuPrefab.GetComponent<IconMenuController>();
            iconMenuController.Initialise(iconMenuController.modelControls, gameObject,  controller, this);
            //iconMenuController.Initialise(iconMenuController.sceneControls, gameObject, controller, this);
            
            // Initialise everything, then disable them
            SetState(true);
            SetState(false);
        }

        private void Update()
        {
            cMenu = controller.LeftMenu();
            CheckState(cMenu, pMenu);
            RubberBanded(controller.CameraTransform());
            pMenu = cMenu;
        }

        private void CheckState(bool current, bool previous)
        {
            if (current || !previous) return;
            
            Active = !Active;
            SetState(Active);
        }

        public void SetState(bool state)
        {
            Active = state;
            rubberBanded = state;
            menuPrefab.SetActive(Active);
            
//            foreach (Dictionary<string, BaseIconControl> controlSet in iconMenuController.controlSets)
//            {
//                foreach (KeyValuePair<string, BaseIconControl> control in controlSet)
//                {
//                    BaseIconControl tool = control.Value;
//                    tool.IconButton.enabled = state;
//                }
//            }
            
            if(!state) return;
            menuPrefab.transform.position = controller.CameraPosition();
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