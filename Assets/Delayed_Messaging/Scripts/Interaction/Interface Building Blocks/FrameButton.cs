using UnityEngine;

namespace Delayed_Messaging.Scripts.Interaction.Interface_Building_Blocks
{
    public class FrameButton : RaycastButton
    {
        [Header("Raycast Button Settings")]
        [SerializeField] private GameObject model;
        [SerializeField] private Transform modelViewerCenter;
        [SerializeField, Range(1f, 0f)] private float modelScaleDownFactor;
        
        protected override void Initialise()
        {
            if (model != null)
            {
                model = Instantiate(model, modelViewerCenter);
                model.transform.position = modelViewerCenter.position;
                model.transform.localScale = new Vector3(modelScaleDownFactor, modelScaleDownFactor, modelScaleDownFactor);
            }
            
            base.Initialise();
        }
    }
}
