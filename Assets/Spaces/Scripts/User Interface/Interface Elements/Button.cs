using UnityEngine;
using UnityEngine.Events;

namespace Spaces.Scripts.User_Interface.Interface_Elements
{
    public class Button : BaseInterface
    {
        [Space] public UnityEvent buttonSelect, grabStart, grabStay, grabEnd;

        protected override void Initialise()
        {
            gameObject.tag = Button;
        }

        public void HoverStart()
        {
            outline.enabled = true;
        }
        public void HoverEnd()
        {
            outline.enabled = false;
        }
    }
}
