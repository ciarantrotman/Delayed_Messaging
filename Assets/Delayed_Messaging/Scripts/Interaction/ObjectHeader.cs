using System;
using TMPro;
using UnityEngine;

namespace Delayed_Messaging.Scripts.Interaction
{
    public class ObjectHeader : MonoBehaviour
    {
        [Header("References")]
        [SerializeField] private TextMeshPro name;
        [SerializeField] private TextMeshPro team;
        [SerializeField] private MeshRenderer header;
        [SerializeField] private MeshRenderer frame;
        [SerializeField] private Transform modelViewerCenter;
        [SerializeField, Range(1f, 0f)] private float modelScaleDownFactor;

        private const string Blank = "";
        private GameObject model;

        private void Start()
        {
            DisableHeader();
        }

        public void SetHeader(BaseClass baseClass)
        {
            name.SetText(baseClass.objectName);
            team.SetText(baseClass.team.ToString());
            Destroy(model);
            
            model = Instantiate(baseClass.objectModel, modelViewerCenter);
            model.transform.position = modelViewerCenter.position;
            model.transform.localScale = new Vector3(modelScaleDownFactor,modelScaleDownFactor,modelScaleDownFactor);
        }

        public void EnableHeader()
        {
            header.enabled = true;
            frame.enabled = true;
        }
        
        public void DisableHeader()
        {
            name.SetText(Blank);
            team.SetText(Blank);
            header.enabled = false;
            frame.enabled = false;
            Destroy(model);
        }
    }
}
