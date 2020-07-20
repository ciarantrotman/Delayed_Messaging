using System;
using System.Collections.Generic;
using UnityEngine;

namespace Spaces.Scripts.Space
{
    public class SpaceManager : MonoBehaviour
    {
        public const string SpaceManagerTag = "SpaceManager";
        private List<SpaceInstance> spaces = new List<SpaceInstance>();
        
        [SerializeField] private SpaceInstance activeSpace;
        
        private void Awake()
        {
            gameObject.tag = SpaceManagerTag;
            
            if (activeSpace == null)
            {
                CreateSpace();
            }
            else
            {
                LoadSpace(activeSpace);
            }
        }

        public void CreateSpace()
        {
            
        }

        public void LoadSpace(SpaceInstance spaceInstance)
        {
            
        }
    }
}
