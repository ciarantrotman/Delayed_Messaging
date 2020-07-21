using UnityEngine;
using UnityEngine.Events;

namespace Spaces.Scripts.User_Interface.Interface_Elements
{
    public class Button : BaseInterface
    {
        [Space] public UnityEvent buttonSelect, grabStart, grabStay, grabEnd;
        /// <summary>
        /// 
        /// </summary>
        protected override void Initialise()
        {
            gameObject.tag = Interface;
        }
        /// <summary>
        /// 
        /// </summary>
        public void HoverStart()
        {
            outline.enabled = true;
        }
        /// <summary>
        /// 
        /// </summary>
        public void HoverEnd()
        {
            outline.enabled = false;
        }
    }
}
