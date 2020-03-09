using System;
using System.Collections.Generic;
using Delayed_Messaging.Scripts.Utilities;
using UnityEngine;

namespace Delayed_Messaging.Scripts.Objects
{
    public class ModelController : MonoBehaviour
    {
        [Header("Model Controller Reference")]
        private bool initialised;
        private BaseObject baseObject;

        public void InitialiseModelController(BaseObject b)
        {
            Debug.Log($"ModelController Initialised by {b.gameObject.name}");
            baseObject = b;
        }

        public void SetModel(BaseClass.Model model)
        {
            Debug.Log($"{baseObject.name}: {baseObject.model.name}");
            foreach (Transform child in baseObject.model.transform)
            {
                Destroy(child.gameObject);
            }
            
            GameObject newModel = Instantiate(model.modelPrefab, baseObject.model.transform);
            newModel.transform.DefaultTransform();
            
            Debug.Log($"<b>{name} [{model.modelIndex}]</b> has been spawned, parented to {newModel.transform.parent}");
            
            baseObject.SetObjectBounds();
        }
    }
}
