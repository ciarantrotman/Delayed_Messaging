using System.Collections.Generic;
using Sirenix.OdinInspector;
using UnityEngine;
using VR_Prototyping.Scripts.Tools;

namespace VR_Prototyping.Scripts.Icon_Scripts
{
    public class BaseIconControl : MonoBehaviour
    {
        [FoldoutGroup("Generic Tool Prefabs")] [Required] public GameObject buttonPrefab;
        [FoldoutGroup("Generic Tool Prefabs")] [Required] [Indent] public GameObject buttonModel;
        [FoldoutGroup("Generic Tool Prefabs")] [Required] public GameObject toolControlPanel;
        
        public ControllerTransforms ControllerTransforms { get; set; }
        public IconMenuController MenuController { get; set; }
        public IconMenu IconMenu { get; set; }
        public string ControlSetIndex { get; set; }
        public IconButton IconButton { get; set; }
        public Vector3 ButtonPos { get; set; }
        public Vector3 ButtonScale { get; set; }

        public void SetupMenuItems(Vector3 panelPosition, Transform panelParent)
        {
            buttonPrefab = Instantiate(buttonPrefab);
            toolControlPanel = Instantiate(toolControlPanel, panelParent);
            toolControlPanel.transform.localPosition = panelPosition;
            toolControlPanel.SetActive(false);

            IconButton = buttonPrefab.GetComponent<IconButton>();
            IconButton.toggleStart.AddListener(SelectControl);
            IconButton.toggleEnd.AddListener(DeselectControl);
            ButtonScale = buttonPrefab.transform.localScale;

            Initialise();
        }
        
        protected virtual void Initialise()
        {
            
        }
        private void SelectControl()
        {
            MenuController.ToggleControl(this);
            MenuController.ControlPanelAnimationActivate(this);
        }
        private void DeselectControl()
        {
            MenuController.SetAllControlStates(false);
            MenuController.ControlPanelAnimationDeactivate(this);
        }

        public void SetControlState(bool state)
        {
            IconButton.toggleState = state;
            switch (state)
            {
                case true:
                    IconButton.ToggleStart();
                    toolControlPanel.SetActive(true);
                    IconButton.hoverStart.Invoke();
                    break;
                default:
                    IconButton.ToggleEnd();
                    toolControlPanel.SetActive(false);
                    IconButton.hoverEnd.Invoke();
                    break;
            }
        }
    }
}
