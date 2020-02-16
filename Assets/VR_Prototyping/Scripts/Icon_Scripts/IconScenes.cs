using Sirenix.OdinInspector;
using UnityEditor;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace VR_Prototyping.Scripts.Icon_Scripts
{
    [DisallowMultipleComponent]
    public class IconScenes : MonoBehaviour
    {
        [Header("Icon Scenes")] 
        [BoxGroup] [SerializeField] private bool launchIntroScene;
        [BoxGroup] public SceneAsset introScene;
        [BoxGroup] public SceneAsset dioramaScene;
        [BoxGroup] public SceneAsset fullScene;

        private void Awake()
        {
            if(!launchIntroScene) return;
            LoadScene(introScene);
        }

        public static void LoadScene(SceneAsset scene)
        {
            if (SceneLoaded(scene))
            {
                Debug.LogError(">>> " + scene.name + " was not loaded, is already loaded");
                return;
            }
            Debug.Log(">>> " + scene.name + " was loaded.");
            SceneManager.LoadScene(scene.name, LoadSceneMode.Additive);
        }
        
        public static void UnloadScene(SceneAsset scene)
        {
            if (!SceneLoaded(scene))
            {
                Debug.LogError(">>> " + scene.name + " was not unloaded, it isn't loaded");
                return;
            }
            Debug.Log("<<< " + scene.name + " was unloaded.");
            SceneManager.UnloadScene(scene.name);
        }

        private static bool SceneLoaded(SceneAsset sceneAsset)
        {
            foreach (Scene loadedScene in SceneManager.GetAllScenes())
            {
                Debug.Log(loadedScene.name);

                if (loadedScene.name == sceneAsset.name) return true;
            }
            Scene scene = SceneManager.GetSceneByName(sceneAsset.name);
            return scene.isLoaded;
        }
    }
}
