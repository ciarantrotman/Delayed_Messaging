using System;
using Sirenix.OdinInspector;
using UnityEngine;
using VR_Prototyping.Scripts.UI_Blocks;

namespace VR_Prototyping.Scripts.Tools
{
    [Serializable]
    public abstract class BaseTool : MonoBehaviour
    {
        public ControllerTransforms Controller { get; set; }
        public ToolController ToolController { get; set; }
        public ToolMenu ToolMenu { get; set; }
        public ToolButton ToolButton { get; private set; }
        public ToolMenu.Handedness Handedness { private get; set; }
        public bool Active { get; set; }
        protected bool CTrigger { get; set; }
        protected  bool PTrigger { get; set; }

        private bool initialised;

        private BaseDirectBlock[] directInterfaceBlocks;
        
        [FoldoutGroup("Generic Tool Prefabs")] [Required] public GameObject buttonPrefab;
        [FoldoutGroup("Generic Tool Prefabs")] [Required] [Indent] public GameObject buttonModel;
        [FoldoutGroup("Generic Tool Prefabs")] [Required] public GameObject nonDominant;
        [FoldoutGroup("Generic Tool Prefabs")] [Required] public GameObject dominant;
        
        [FoldoutGroup("Generic Tool Settings")]  [SerializeField] [Range(.01f, 1f)] protected float dominantSpeed = 1;
        [FoldoutGroup("Generic Tool Settings")]  [SerializeField] [Range(.01f, 1f)] protected float nonDominantSpeed = 1;


        public void SetupMenuItems()
        {
            buttonPrefab = Instantiate(buttonPrefab);
            
            ToolButton = buttonPrefab.GetComponent<ToolButton>();
            ToolButton.enabled = false;
            ToolButton.selectEnd.AddListener(SelectTool);

            if (dominant != null)
            {
                dominant = Instantiate(dominant);
                dominant.SetActive(false);   
            }

            if (nonDominant != null)
            {
                nonDominant = Instantiate(nonDominant);
                nonDominant.SetActive(false);   
            }
            
            InitialiseDirectInterface();
            
            Initialise();

            initialised = true;
        }
        
        protected virtual void Initialise()
        {
            
        }

        private void Start()
        { 
            OnStart();   
        }
        
        protected virtual void OnStart()
        {
            
        }

        private void InitialiseDirectInterface()
        {
            if (nonDominant == null) return;
            
            directInterfaceBlocks = nonDominant.GetComponentsInChildren<BaseDirectBlock>();

            foreach (BaseDirectBlock block in directInterfaceBlocks)
            {
                block.controller = Controller;
            }
        }

        public void SetToolState(bool state)
        {
            Active = state;

            if (dominant != null)
            {
                dominant.SetActive(state);
            }

            if (nonDominant != null)
            {
                nonDominant.SetActive(state);
            }

            switch (state)
            {
                case true:
                    ToolActivate();
                    break;
                case false:
                    ToolDeactivate();
                    break;
            }

            if(!state) return;
            ToolMenu.SetState(false, transform);
        }

        protected virtual void ToolActivate()
        {
            
        }
        
        protected virtual void ToolDeactivate()
        {
            
        }
        
        protected virtual void ToolUpdate()
        {
            
        }
        
        protected virtual void ToolStart()
        {
            
        }

        protected virtual void ToolStay()
        {
            
        }

        protected virtual void ToolEnd()
        {
            
        }

        protected virtual void ToolInactive()
        {
            
        }

        private void SelectTool()
        {
            Handedness = ToolMenu.dominantHand;
            ToolController.ToggleTool(this);
        }

        private void Update()
        {
            if (!initialised) return;
            
            ToolUpdate();
            
            switch (Handedness)
            {
                case ToolMenu.Handedness.RIGHT when dominant != null && nonDominant != null:
                    CTrigger = Controller.RightSelect();
                    dominant.transform.LerpTransform(Controller.RightTransform(), dominantSpeed);
                    nonDominant.transform.LerpTransform(Controller.LeftTransform(), nonDominantSpeed);
                    break;
                case ToolMenu.Handedness.LEFT when dominant != null && nonDominant != null:
                    CTrigger = Controller.LeftSelect();
                    dominant.transform.LerpTransform(Controller.LeftTransform(), dominantSpeed);
                    nonDominant.transform.LerpTransform(Controller.RightTransform(), nonDominantSpeed);
                    break;
                default:
                    return;
            }
            
            if(!Active) return;
            
            if (CTrigger && !PTrigger)
            {
                ToolStart();
            }

            if (CTrigger && PTrigger)
            {
                ToolStay();
            }

            if (!CTrigger && PTrigger)
            {
                ToolEnd();
            }
            
            if (!CTrigger && !PTrigger)
            {
                ToolInactive();
            }

            PTrigger = CTrigger;
        }
    }
}
