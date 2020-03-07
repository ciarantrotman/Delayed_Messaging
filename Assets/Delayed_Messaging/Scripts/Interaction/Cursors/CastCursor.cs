using System;
using UnityEngine;

namespace Delayed_Messaging.Scripts.Interaction.Cursors
{
    public class CastCursor : BaseCursor
    {
        public enum CursorState
        {
            DEFAULT,
            UNIT,
            STRUCTURE,
            ENEMY,
            RESOURCE
        }

        [Header("Cursor References")]
        [SerializeField] private MeshFilter meshFilter;
        [SerializeField] private MeshRenderer cursor;
        
        [Header("Cursor States")]
        [SerializeField] private Mesh defaultCursor;
        [SerializeField] private Mesh unitCursor;
        [SerializeField] private Mesh structureCursor;
        [SerializeField] private Mesh resourceCursor;
        [SerializeField] private Mesh enemyCursor;

        public void SetCursorState(CursorState cursorState)
        {
            switch (cursorState)
            {
                case CursorState.DEFAULT:
                    meshFilter.mesh = defaultCursor;
                    break;
                case CursorState.UNIT:
                    meshFilter.mesh = unitCursor;
                    break;
                case CursorState.STRUCTURE:
                    meshFilter.mesh = structureCursor;
                    break;
                case CursorState.ENEMY:
                    meshFilter.mesh = enemyCursor;
                    break;
                case CursorState.RESOURCE:
                    meshFilter.mesh = resourceCursor;
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(cursorState), cursorState, null);
            }
        }
    }
}
