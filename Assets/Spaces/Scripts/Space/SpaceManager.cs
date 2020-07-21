using System;
using System.Collections.Generic;
using Spaces.Scripts.Objects;
using UnityEngine;

namespace Spaces.Scripts.Space
{
    public class SpaceManager : MonoBehaviour
    {
        public List<SpaceInstance> spaces = new List<SpaceInstance>();
        
        [SerializeField] private SpaceInstance activeSpace;
        
        private void Awake()
        {
            gameObject.tag = Reference.SpaceManagerTag;
            LoadSpace(ActiveSpace());
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="spaceInstance"></param>
        private void LoadSpace(SpaceInstance spaceInstance)
        {
            spaces.Add(spaceInstance == null ? CreateNewSpace() : ActiveSpace());
        }
        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        private SpaceInstance CreateNewSpace()
        {
            SetActiveSpace(gameObject.AddComponent<SpaceInstance>());
            return ActiveSpace();
        }
        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        private SpaceInstance ActiveSpace()
        {
            return activeSpace;
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="spaceInstance"></param>
        private void SetActiveSpace(SpaceInstance spaceInstance)
        {
            activeSpace = spaceInstance;
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="objectInstance"></param>
        public void ObjectRegistration(ObjectInstance objectInstance)
        {
            ActiveSpace().AddObject(objectInstance);
        }
    }
}
