using System;
using UnityEngine;

namespace Delayed_Messaging.Scripts.Interaction.Cursors
{
    public class UnitCursor : BaseCursor
    {
        public enum CursorState
        {
            VALID_POSITION,
            INVALID_POSITION
        }
        
        [Header("Cursor References")]
        [SerializeField] private MeshFilter meshFilter;
        [SerializeField] private MeshRenderer cursor;
        
        [Header("Cursor States")]
        [SerializeField] private Mesh validCursor;
        [SerializeField] private Mesh invalidCursor;
        
        public void SetCursorState(CursorState cursorState)
        {
            switch (cursorState)
            {
                case CursorState.VALID_POSITION:
                    meshFilter.mesh = validCursor;
                    break;
                case CursorState.INVALID_POSITION:
                    meshFilter.mesh = invalidCursor;
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(cursorState), cursorState, null);
            }
        }
    }
}
