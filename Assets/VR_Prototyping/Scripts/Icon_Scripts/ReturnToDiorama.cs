using Sirenix.OdinInspector;
using UnityEngine;

namespace VR_Prototyping.Scripts.Icon_Scripts
{
    public class ReturnToDiorama : SelectableObject
    {
        private IconScenes iconScenes;
        private IconMenu iconMenu;
        protected override void InitialisePostSetup()
        {
            iconScenes = player.GetComponent<IconScenes>();
            iconMenu = player.GetComponent<IconMenu>();
			selectEnd.AddListener(Return);
        }

        private void Return()
        {
            iconMenu.SetState(false);
            IconScenes.UnloadScene(iconScenes.fullScene);
            IconScenes.LoadScene(iconScenes.dioramaScene);
            RenderSettings.skybox = controllerTransforms.voidSkyBox;
        }
    }
}
