using System.Collections.Generic;
using Sirenix.OdinInspector;
using Sirenix.Serialization;
using UnityEngine;
using VR_Prototyping.Scripts.Keyboard;

namespace VR_Prototyping.Scripts.Tools
{
    public class ToolController : SerializedMonoBehaviour
    {
        [OdinSerialize] public Dictionary<string, BaseTool> tools;
        
        [BoxGroup("Tool Controls")] [Required] public GameObject toolMenuHeader;
        [BoxGroup("Tool Controls")] [Indent] [Range(-1f, 1f)] [SerializeField] private float headerHeight = .07f;
        [BoxGroup("Tool Controls")] [Indent] [Range(-1f, 1f)] [SerializeField] private float headerIndent = -.55f;
        [BoxGroup("Tool Controls")] [Required] public GameObject toolMenuFooter;
        [BoxGroup("Tool Controls")] [Space(10)] [Range(0, 5)] [SerializeField] private int gridSize = 3;
        [BoxGroup("Tool Controls")] [Range(0f, 1f)] [SerializeField] private float horizontalSpacing = .1f;
        [BoxGroup("Tool Controls")] [Range(0f, 1f)] [SerializeField] private float verticalSpacing = .1f;

        const float OffsetDepth = .2f;

        public void Initialise(GameObject player, bool startsActive, ControllerTransforms controller, ToolMenu.Handedness handedness, ToolMenu toolMenu)
        {
            float x = -horizontalSpacing;
            float y = verticalSpacing;
            int toolNumber = 0;

            toolMenuHeader = Instantiate(toolMenuHeader, transform);
            toolMenuHeader.name = "Tool/Header";
            toolMenuHeader.transform.localPosition = new Vector3(-horizontalSpacing + headerIndent,verticalSpacing + headerHeight, OffsetDepth);
            toolMenuHeader.SetActive(false);
            
            foreach (KeyValuePair<string, BaseTool> item in tools)
            {
                // set up indices for the tool
                
                toolNumber++;
                string n = item.Key;
                BaseTool tool = item.Value;
                
                // assign all values for the instantiated tool in here
                
                tool.ToolController = this;
                tool.Controller = controller;
                tool.ToolMenu = toolMenu;
                tool.Handedness = handedness;

                // initialise the tool here
                
                tool.SetupMenuItems();
                
                // reference those initialised components here
                
                tool.ToolButton.player = player;
                tool.ToolButton.ToolModel = tool.buttonModel;
                tool.ToolButton.enabled = startsActive;
                tool.ToolButton.buttonText.SetText(item.Key + " Tool");
                tool.ToolButton.Index = toolNumber;
                
                tool.buttonPrefab.name = n + "/Button";
                tool.buttonPrefab.transform.SetParent(transform);
                tool.buttonPrefab.transform.localPosition = new Vector3(x, y, OffsetDepth);
                
                tool.SetToolState(false);

                // check for menus
                
                if (tool.dominant != null)
                {
                    tool.dominant.name = n + "/Dominant";
                    tool.dominant.transform.SetParent(transform);
                }

                if (tool.nonDominant != null)
                {
                    tool.nonDominant.transform.SetParent(transform);
                    tool.nonDominant.name = n + "/Non-Dominant";
                }

                // tool horizontal grid spacing
                
                x += horizontalSpacing;

                // tool vertical grid spacing
                
                if (toolNumber % gridSize != 0) continue;
                x = -horizontalSpacing;
                y -= verticalSpacing;
            }
            
            toolMenuFooter = Instantiate(toolMenuFooter, transform);
            toolMenuFooter.name = "Tool/Footer";
            toolMenuFooter.transform.localPosition = new Vector3(-horizontalSpacing + headerIndent, y + (verticalSpacing * .2f), OffsetDepth);
            toolMenuFooter.SetActive(false);
        }

        public void ToggleTool(BaseTool activeTool)
        {
            foreach (KeyValuePair<string, BaseTool> item in tools)
            {
                BaseTool tool = item.Value;
                tool.SetToolState(activeTool == tool);
                SetButtonVisualState(tool, activeTool == tool);
            }
        }

        public void SetAllToolState(bool state)
        {
            foreach (KeyValuePair<string, BaseTool> item in tools)
            {
                BaseTool tool = item.Value;
                tool.SetToolState(state);
                SetButtonVisualState(tool, state);
            }
        }

        public void ToggleButtonState(bool state)
        {
            foreach (KeyValuePair<string, BaseTool> item in tools)
            {
                BaseTool tool = item.Value;
                tool.buttonPrefab.SetActive(state);
                tool.ToolButton.enabled = state;
            }
        }

        private static void SetButtonVisualState(BaseTool tool, bool active)
        {
            tool.ToolButton.SetState(active);
        }
    }
}