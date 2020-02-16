using UnityEngine;
using UnityEngine.SceneManagement;

namespace VR_Prototyping.Scripts.Icon_Scripts
{
    public class FullModel : MonoBehaviour
    {
        private Locomotion locomotion;
        private ViewpointManager viewpointManager;

        private void Start()
        {
            foreach (GameObject rootGameObject in SceneManager.GetActiveScene().GetRootGameObjects())
            {
                if (rootGameObject.name != "[VR Player]") continue;
                locomotion = rootGameObject.GetComponent<Locomotion>();
                viewpointManager = locomotion.ViewpointManager;
                viewpointManager.FullModel = this;
            }
            
            ModelLoaded();
        }
        
        private void ModelLoaded()
        {
            viewpointManager.EnterModel();
        }
    }
}
