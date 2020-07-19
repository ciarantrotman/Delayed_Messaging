using UnityEngine;
using UnityEngine.Events;

namespace Spaces.Scripts.User_Interface.Interface_Elements
{
    public class Button : BaseInterface
    {
        public UnityEvent select;
        public UnityEvent grabStart, grabStay, grabEnd;

        protected override void Initialise()
        {
            gameObject.tag = Button;
        }

        private void OnTriggerEnter(Collider userCollider)
        {
            outline.enabled = true;
        }

        private void OnTriggerExit(Collider userCollider)
        {
            outline.enabled = false;
        }
    }
}
