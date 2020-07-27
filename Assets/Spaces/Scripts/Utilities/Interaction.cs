using Spaces.Scripts.Player;
using UnityEngine;
using Event = Spaces.Scripts.Player.ControllerTransforms.EventTracker.EventType;

namespace Spaces.Scripts.Utilities
{
    public abstract class Interaction : MonoBehaviour
    {
        public string tagComparison;
        public enum Mode { DIRECT, INDIRECT }
        protected ControllerTransforms.Check orientation;
        public void AddEventListeners(ControllerTransforms controller, ControllerTransforms.Check side, string tag)
        {
            // Cache references
            tagComparison = tag;
            
            // Add event listeners 
            controller.SelectEvent(side, Event.END).AddListener(Select);
            controller.GrabEvent(side, Event.START).AddListener(GrabStart);
            controller.GrabEvent(side, Event.STAY).AddListener(GrabStay);
            controller.GrabEvent(side, Event.END).AddListener(GrabEnd);
        }   

        protected abstract void Select();
        protected abstract void GrabStart();
        protected abstract void GrabStay();
        protected abstract void GrabEnd();
    }
}
