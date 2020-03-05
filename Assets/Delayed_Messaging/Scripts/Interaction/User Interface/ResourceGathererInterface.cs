using System;
using Delayed_Messaging.Scripts.Units;
using Delayed_Messaging.Scripts.Utilities;
using UnityEngine;
using VR_Prototyping.Scripts;

namespace Delayed_Messaging.Scripts.Interaction.User_Interface
{
    public class ResourceGathererInterface : BaseObjectInterface
    {
        private ResourceGatherer resourceGatherer;
        [SerializeField] private RaycastButton searchButton;

        protected override void OverrideInitialise()
        {
            resourceGatherer = (ResourceGatherer)baseObjectBaseObject;
            
            searchButton.OnSelect.AddListener(Search);
        }

        private void Search()
        {
            resourceGatherer.StartSearching();
        }
    }
}
