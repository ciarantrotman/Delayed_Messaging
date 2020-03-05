using TMPro;
using UnityEngine;
using VR_Prototyping.Scripts;

namespace Delayed_Messaging.Scripts.Interaction
{
    public class ObjectHeader : MonoBehaviour
    {
        [Header("References")]
        [SerializeField] private TextMeshPro objectName;
        [SerializeField] private TextMeshPro objectTeam;
        [SerializeField] private MeshRenderer header;
        [SerializeField] private MeshRenderer frame;
        [SerializeField] private Transform modelViewerCenter;

        private const string Blank = "";
        private GameObject model;

        private void Start()
        {
            DisableHeader();
        }

        public void SetHeader(BaseClass baseClass)
        {
            objectName.SetText(baseClass.objectName);
            objectTeam.SetText(baseClass.team.ToString());
            Destroy(model);
            
            model = Instantiate(baseClass.objectModel, modelViewerCenter);
            model.ScaleFactor();
            model.transform.position = modelViewerCenter.position;
        }

        public void EnableHeader()
        {
            header.enabled = true;
            frame.enabled = true;
        }
        
        public void DisableHeader()
        {
            objectName.SetText(Blank);
            objectTeam.SetText(Blank);
            header.enabled = false;
            frame.enabled = false;
            Destroy(model);
        }
    }
}