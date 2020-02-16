using Sirenix.OdinInspector;
using UnityEngine;
using VR_Prototyping.Scripts.UI_Blocks;

namespace VR_Prototyping.Scripts.Tools.Sketch
{
    public class SketchBrushColor : DirectDial
    {
        private SketchTool sketchTool;
        private MeshRenderer dialCapMeshRenderer; 
        private MeshRenderer dialHandleMeshRenderer; 
        private  LineRenderer colorGuideCircle;
        
        [TabGroup("Aesthetics Settings")] [Required] [SerializeField] private GameObject dialSelection;

        private void Start()
        {
            sketchTool = transform.parent.transform.GetComponentInParent<SketchTool>();
            controller = sketchTool.Controller;
            
            SetupDial();

            dialHandleMeshRenderer = SetupRenderer(dialHandle);
            spokeLr.material = sketchTool.sketchMaterial;
            dialSelection = Instantiate(dialSelection, anchor.transform);
            dialSelection.transform.localPosition = Vector3.zero;
        }

        private MeshRenderer SetupRenderer(GameObject target)
        {
            MeshRenderer meshRenderer= target.GetComponent<MeshRenderer>();
            meshRenderer.material = sketchTool.sketchMaterial;
            return meshRenderer;
        }

        private void LateUpdate()
        {
            SetColor(dialValue);
            dialSelection.transform.LookAt(center.transform);
            dialSelection.transform.localEulerAngles = new Vector3(0, dialSelection.transform.localEulerAngles.y,0);
        }

        private void SetColor(float colorValue)
        {
            Color color = Color.HSVToRGB(colorValue, 1, 1, true);
            sketchTool.SetColor(color);
            //dialCapMeshRenderer.sharedMaterial.color = color;
            //dialHandleMeshRenderer.sharedMaterial.color = color;
            //spokeLr.startColor = color;
            //spokeLr.endColor = color;
        }
    }
}