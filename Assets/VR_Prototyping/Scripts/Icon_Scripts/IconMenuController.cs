using System;
using System.Collections.Generic;
using DG.Tweening;
using Sirenix.OdinInspector;
using Sirenix.Serialization;
using UnityEngine;
using Valve.Newtonsoft.Json.Utilities;
using VR_Prototyping.Scripts.Tools;

namespace VR_Prototyping.Scripts.Icon_Scripts
{
    public class IconMenuController : SerializedMonoBehaviour
    {
        [OdinSerialize] public Dictionary<string, BaseIconControl> sceneControls;
        [OdinSerialize] public Dictionary<string, BaseIconControl> modelControls;

        [BoxGroup("Tool Controls")] [Required] public Transform interactiveMenu;
        [BoxGroup("Tool Controls")] [Required] public GameObject controlHeader;
        [BoxGroup("Tool Controls")] [Indent] [Range(-1f, 1f)] [SerializeField] private float headerHeight = .07f;
        [BoxGroup("Tool Controls")] [Indent] [Range(-1f, 1f)] [SerializeField] private float headerIndent = -.55f;
        [BoxGroup("Tool Controls")] [Required] public GameObject menuDivider;
        [BoxGroup("Tool Controls")] [Space(10)] [Range(0, 5)] [SerializeField] private int gridSize = 3;
        [BoxGroup("Tool Controls")] [Range(0f, 1f)] [SerializeField] private float horizontalSpacing = .1f;
        [BoxGroup("Tool Controls")] [Range(0f, 1f)] [SerializeField] private float verticalSpacing = .1f;
        [BoxGroup("Tool Controls")] [Range(0f, 1f)] [SerializeField] private float splitSpacing = .1f;
        [BoxGroup("Tool Controls")] [Space(10)] [Range(0f, 1f)] [SerializeField] private float animationTime = .3f;

        private const float OffsetDepth = 0f;
        private const float ScaleFactor = .75f;
        private int controlSetIndex = 0;

        private float constantSpacing;
        
        public IconMenu IconMenu { get; set; }

        private void Awake()
        {
            constantSpacing = horizontalSpacing;
        }

        public void Initialise(Dictionary<string, BaseIconControl> controlSet, GameObject player, ControllerTransforms controller, IconMenu iconMenu)
        {
            IconMenu = iconMenu;
            
            float x = horizontalSpacing;
            float y = verticalSpacing;
            int controlNumber = 0;

            interactiveMenu = GetComponentInChildren<IconMenuSideBar>().transform;

            controlHeader = Instantiate(controlHeader, interactiveMenu);
            controlHeader.name = "Control/Header";
            controlHeader.transform.localPosition = new Vector3(horizontalSpacing + headerIndent, verticalSpacing + headerHeight, OffsetDepth);

            foreach (KeyValuePair<string, BaseIconControl> item in controlSet)
            {
                // set up indices for the tool

                controlNumber++;
                string n = item.Key;
                BaseIconControl control = item.Value;

                // assign all values for the instantiated tool in here

                control.MenuController = this;
                control.ControllerTransforms = controller;
                control.IconMenu = iconMenu;
                control.ControlSetIndex = controlSet.ToString();

                // initialise the tool here

                Vector3 panelPosition = new Vector3(constantSpacing + constantSpacing, 0f, -.05f);
                control.SetupMenuItems(panelPosition, interactiveMenu);

                // reference those initialised components here

                control.IconButton.player = player;
                control.IconButton.buttonText.SetText(item.Key);
                control.IconButton.Index = controlNumber;
                control.IconButton.ControlModel = control.buttonModel;
                control.IconButton.enabled = true;

                control.buttonPrefab.name = n + "/Button";
                control.buttonPrefab.transform.SetParent(interactiveMenu);
                control.buttonPrefab.transform.localPosition = new Vector3(x, y, OffsetDepth);
                control.ButtonPos = control.buttonPrefab.transform.localPosition;

                // check for menus

                control.toolControlPanel.name = n + "/ControlPanel";
                control.toolControlPanel.transform.SetParent(interactiveMenu);

                // tool horizontal grid spacing

                x += constantSpacing;

                // tool vertical grid spacing

                if (controlNumber % gridSize != 0) continue;
                x = horizontalSpacing;
                y -= verticalSpacing;
            }

            controlSetIndex++;
            
            return;
            if (controlSetIndex > 1) return; // figure out a better way to do this

            menuDivider = Instantiate(menuDivider, interactiveMenu);
            menuDivider.name = "Control/Divider";
            menuDivider.transform.localPosition = new Vector3(x + (splitSpacing), 0, OffsetDepth);

            horizontalSpacing += x + splitSpacing; //+ splitSpacing;
        }

        public void ToggleControl(BaseIconControl activeControl)
        {
            string index = activeControl.ControlSetIndex;
            foreach (KeyValuePair<string, BaseIconControl> control in modelControls)
            {
                BaseIconControl tool = control.Value;
                bool match = activeControl == tool;
                tool.SetControlState(match);
            }
            foreach (KeyValuePair<string, BaseIconControl> control in sceneControls)
            {
                BaseIconControl tool = control.Value;
                bool match = activeControl == tool;
                tool.SetControlState(match);
            }
        }

        public void ControlPanelAnimationActivate(BaseIconControl activeControl)
        {
            float x = constantSpacing;
            float y = (verticalSpacing * ScaleFactor) * 2f;// + (verticalSpacing + (verticalSpacing * .5f));
            controlHeader.SetActive(false);
            foreach (KeyValuePair<string, BaseIconControl> control in modelControls)
            {
                BaseIconControl tool = control.Value;
                tool.buttonPrefab.transform.DOLocalMove(new Vector3(x, y, OffsetDepth), animationTime);
                tool.buttonPrefab.transform.DOScale(new Vector3(tool.ButtonScale.x * ScaleFactor, tool.ButtonScale.y * ScaleFactor, tool.ButtonScale.z * ScaleFactor), animationTime);
                y -= verticalSpacing * ScaleFactor;
            }
            foreach (KeyValuePair<string, BaseIconControl> control in sceneControls)
            {
                BaseIconControl tool = control.Value;
                tool.SetControlState(false);
                tool.buttonPrefab.SetActive(false);
            }
            return;
            if (activeControl.ControlSetIndex == modelControls.ToString())
            {
                foreach (KeyValuePair<string, BaseIconControl> control in modelControls)
                {
                    BaseIconControl tool = control.Value;
                    tool.buttonPrefab.transform.DOLocalMove(new Vector3(x, y, OffsetDepth), animationTime);
                    tool.buttonPrefab.transform.DOScale(new Vector3(tool.ButtonScale.x * ScaleFactor, tool.ButtonScale.y * ScaleFactor, tool.ButtonScale.z * ScaleFactor), animationTime);
                    y -= verticalSpacing * ScaleFactor;
                }
                foreach (KeyValuePair<string, BaseIconControl> control in sceneControls)
                {
                    BaseIconControl tool = control.Value;
                    tool.SetControlState(false);
                }
            }
            else
            {
                foreach (KeyValuePair<string, BaseIconControl> control in sceneControls)
                {
                    BaseIconControl tool = control.Value;
                    tool.buttonPrefab.transform.DOLocalMove(new Vector3(x, y, OffsetDepth), animationTime);
                    tool.buttonPrefab.transform.DOScale(new Vector3(tool.ButtonScale.x * ScaleFactor, tool.ButtonScale.y * ScaleFactor, tool.ButtonScale.z * ScaleFactor), animationTime);
                    y -= verticalSpacing * ScaleFactor;
                }
                foreach (KeyValuePair<string, BaseIconControl> control in modelControls)
                {
                    BaseIconControl tool = control.Value;
                    tool.SetControlState(false);
                }
            }
        }
        
        public void ControlPanelAnimationDeactivate(BaseIconControl activeControl)
        {
            controlHeader.SetActive(true);
            foreach (KeyValuePair<string, BaseIconControl> control in modelControls)
            {
                BaseIconControl tool = control.Value;
                tool.buttonPrefab.transform.DOLocalMove(tool.ButtonPos, animationTime);
                tool.buttonPrefab.transform.DOScale(new Vector3(tool.ButtonScale.x, tool.ButtonScale.y, tool.ButtonScale.z), animationTime);
            }
            foreach (KeyValuePair<string, BaseIconControl> control in sceneControls)
            {
                BaseIconControl tool = control.Value;
                tool.SetControlState(true);
                tool.buttonPrefab.SetActive(true);
            }
            return;
            if (activeControl.ControlSetIndex == modelControls.ToString())
            {
                foreach (KeyValuePair<string, BaseIconControl> control in modelControls)
                {
                    BaseIconControl tool = control.Value;
                    tool.buttonPrefab.transform.DOLocalMove(tool.ButtonPos, animationTime);
                    tool.buttonPrefab.transform.DOScale(new Vector3(tool.ButtonScale.x, tool.ButtonScale.y, tool.ButtonScale.z), animationTime);
                }
                foreach (KeyValuePair<string, BaseIconControl> control in sceneControls)
                {
                    BaseIconControl tool = control.Value;
                    tool.SetControlState(true);
                }
            }
            else
            {
                foreach (KeyValuePair<string, BaseIconControl> control in sceneControls)
                {
                    BaseIconControl tool = control.Value;
                    tool.buttonPrefab.transform.DOLocalMove(tool.ButtonPos, animationTime);
                    tool.buttonPrefab.transform.DOScale(new Vector3(tool.ButtonScale.x, tool.ButtonScale.y, tool.ButtonScale.z), animationTime);
                }
                foreach (KeyValuePair<string, BaseIconControl> control in modelControls)
                {
                    BaseIconControl tool = control.Value;
                    tool.SetControlState(true);
                }
            }
            return;
            foreach (KeyValuePair<string, BaseIconControl> control in modelControls)
            {
                BaseIconControl tool = control.Value;
                tool.buttonPrefab.transform.DOLocalMove(tool.ButtonPos, animationTime);
                tool.buttonPrefab.transform.DOScale(new Vector3(tool.ButtonScale.x, tool.ButtonScale.y, tool.ButtonScale.z), animationTime);
            }
        }

        public void SetAllControlStates(bool state)
        {
            foreach (KeyValuePair<string, BaseIconControl> control in modelControls)
            {
                BaseIconControl tool = control.Value;
                tool.SetControlState(state);
            }
            foreach (KeyValuePair<string, BaseIconControl> control in sceneControls)
            {
                BaseIconControl tool = control.Value;
                tool.SetControlState(state);
            }
        }
    }
}
