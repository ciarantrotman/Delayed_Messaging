using System;
using System.Collections.Generic;
using Sirenix.OdinInspector;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace VR_Prototyping.Scripts.Icon_Scripts
{
    public class ViewpointManager : MonoBehaviour
    {
        [BoxGroup] [SerializeField] private GameObject viewpointPreview;
        private GameObject rViewpointPreview;
        private Scene mainScene;
        private IconScenes scenes;
        private Locomotion locomotion;
        private ControllerTransforms controllerTransforms;

        private List<ViewpointTarget> viewpointTargets = new List<ViewpointTarget>();

        public FullModel FullModel { get; set; }
        public ViewpointPreview RViewpointPreview { get; set; }

        private Transform target;

        private void Start()
        {
            foreach (GameObject rootGameObject in SceneManager.GetActiveScene().GetRootGameObjects())
            {
                if (rootGameObject.name != "[VR Player]") continue;
                controllerTransforms = rootGameObject.GetComponent<ControllerTransforms>();
                scenes = rootGameObject.GetComponent<IconScenes>();
                locomotion = rootGameObject.GetComponent<Locomotion>();
                locomotion.ViewpointManager = this;
            }

            foreach (Transform child in transform)
            {
                if (child.GetComponent<ViewpointTarget>() == null) continue;
                ViewpointTarget viewpointTarget = child.GetComponent<ViewpointTarget>();
                viewpointTargets.Add(viewpointTarget);
                viewpointTarget.ViewpointManager = this;
                viewpointTarget.Index = viewpointTargets.Count;
                viewpointTarget.SetupViewpointTarget();
            }

            rViewpointPreview = Instantiate(viewpointPreview);
            RViewpointPreview = rViewpointPreview.GetComponentInChildren<ViewpointPreview>();
            RViewpointPreview.DeactivatePreview();
        }

        private void Update()
        {
            rViewpointPreview.transform.Transforms(controllerTransforms.RightTransform());
        }

        public void TriggerEnterModel(Transform reference)
        {
            IconScenes.LoadScene(scenes.fullScene);
            target = reference;
        }

        public void EnterModel()
        {
            GameObject targetProxy = new GameObject();
            targetProxy.transform.SetParent(FullModel.transform);
            Transform targetTransform = target.transform;
            targetProxy.transform.localPosition = targetTransform.localPosition;
            targetProxy.transform.localRotation = targetTransform.localRotation;
            locomotion.CustomLocomotion(
                targetProxy.transform.position, 
                targetProxy.transform.eulerAngles,
                Locomotion.Method.BLINK, 
                true);
            locomotion.sceneWipeTrigger.AddListener(UnloadDiorama);
            RenderSettings.skybox = controllerTransforms.environmentSkyBox;
        }

        private void UnloadDiorama()
        {
            IconScenes.UnloadScene(scenes.dioramaScene);
        }
    }
}