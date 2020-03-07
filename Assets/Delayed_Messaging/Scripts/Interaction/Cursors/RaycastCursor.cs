using UnityEngine;

namespace Delayed_Messaging.Scripts.Interaction.Cursors
{
    public class RaycastCursor : BaseCursor
    {
        public enum CursorState
        {
            DEFAULT,
            SLIDER,
            TEXT,
            GRAB
        }

        [Header("Cursor References")]
        [SerializeField] private MeshFilter meshFilter;
        [SerializeField] private MeshRenderer cursor;
        
        [Header("Cursor States")]
        [SerializeField] private Mesh defaultCursor;
        [SerializeField] private Mesh sliderCursor;
        [SerializeField] private Mesh textCursor;
        [SerializeField] private Mesh grabCursor;

        public void EnableCursor(bool state)
        {
            cursor.enabled = state;
        }
        
        public void SetCursorStateState(CursorState state)
        {
            switch (state)
            {
                case CursorState.DEFAULT:
                    meshFilter.mesh = defaultCursor;
                    break;
                case CursorState.SLIDER:
                    meshFilter.mesh = sliderCursor;
                    break;
                case CursorState.TEXT:
                    meshFilter.mesh = textCursor;
                    break;
                case CursorState.GRAB:
                    meshFilter.mesh = grabCursor;
                    break;
                default:
                    break;
            }
        }
    }
}
