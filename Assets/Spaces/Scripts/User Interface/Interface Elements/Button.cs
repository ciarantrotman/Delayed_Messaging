using UnityEngine;
using UnityEngine.Events;

namespace Spaces.Scripts.User_Interface.Interface_Elements
{
    public class Button : BaseInterface
    {
        [Space] public UnityEvent buttonSelect, grabStart, grabStay, grabEnd;
        protected override void Initialise()
        {
            // ¯\_(ツ)_/¯
        }
    }
}
