using System;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace VR_Prototyping.Scripts.Icon_Scripts
{
    public class StartLocation : MonoBehaviour
    {
        private void Start()
        {
            foreach (GameObject rootGameObject in SceneManager.GetActiveScene().GetRootGameObjects())
            {
                if (rootGameObject.name != "[VR Player]") continue;
                Transform values = transform;
                rootGameObject.GetComponent<Locomotion>().CustomLocomotion(values.position, values.eulerAngles, Locomotion.Method.BLINK, false);
            }
        }
    }
}

