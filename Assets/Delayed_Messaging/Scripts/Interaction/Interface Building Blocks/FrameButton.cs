using Delayed_Messaging.Scripts.Utilities;
using UnityEngine;
using VR_Prototyping.Scripts;
using VR_Prototyping.Scripts.Utilities;

namespace Delayed_Messaging.Scripts.Interaction.Interface_Building_Blocks
{
    public class FrameButton : RaycastButton
    {
        [Header("Frame Button Settings")]
        [SerializeField] private GameObject model;
        [SerializeField] private Transform modelViewerCenter;

        protected override void Initialise()
        {
            if (model != null)
            {
                model = Instantiate(model, modelViewerCenter);
                model.ScaleFactor();
                model.transform.Transforms(modelViewerCenter);
            }
            
            base.Initialise();
        }
    }
}
