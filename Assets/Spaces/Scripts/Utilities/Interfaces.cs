namespace Spaces.Scripts.Utilities
{
    public interface ISelectable<in T>
    {
        /// <summary>
        /// This is when the select button is first pressed.
        /// </summary>
        void SelectStart(T side);
        /// <summary>
        /// This is called and maintained after a certain duration.
        /// </summary>
        void SelectHold(T side);
        /// <summary>
        /// This is analogue to QuickSelect but for a held select.
        /// </summary>
        void SelectHoldEnd(T side);
        /// <summary>
        /// This is when select is tapped rather than held.
        /// <b> This should be viewed as an event.</b>
        /// </summary>
        void QuickSelect(T side);
        /// <summary>
        /// Common function called after QuickSelect() and SelectHoldEnd()
        /// </summary>
        void Deselect(T side);
    }

    public interface IHoverable
    {
        /// <summary>
        /// Called once when the object becomes the focus object
        /// </summary>
        void HoverStart();
        /// <summary>
        /// Continuously called while the object is the focus object
        /// </summary>
        void HoverStay();
        /// <summary>
        /// Called the frame after an object which has been hovered on is no longer <b>any</b> focus object
        /// </summary>
        void HoverEnd();
    }
    
    public interface IInteractive<in T, in U, in V>
    {
        /// <summary>
        /// 
        /// </summary>
        void Select();
        /// <summary>
        /// 
        /// </summary>
        void GrabStart(T check, U mode, V totemisedSpace);
        /// <summary>
        /// 
        /// </summary>
        void GrabStay();
        /// <summary>
        /// 
        /// </summary>
        void GrabEnd();
    }
}
