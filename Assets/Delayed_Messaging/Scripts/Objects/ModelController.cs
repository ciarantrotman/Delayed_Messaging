using System.Collections.Generic;
using UnityEngine;

namespace Delayed_Messaging.Scripts.Objects
{
    public class ModelController : MonoBehaviour
    {
        [Header("Model Controller Reference")]
        [SerializeField] private Transform modelParent;

        public void SetModel(BaseClass.Model model)
        {
            foreach (GameObject child in modelParent)
            {
                Destroy(child);
            }
            
            Debug.Log($"{model.modelIndex} is being spawned from {name}");
            
            GameObject newModel = Instantiate(model.modelPrefab, modelParent);
            
            newModel.transform.localPosition = Vector3.zero;
            newModel.transform.localRotation = Quaternion.identity;
        }
    }
}
