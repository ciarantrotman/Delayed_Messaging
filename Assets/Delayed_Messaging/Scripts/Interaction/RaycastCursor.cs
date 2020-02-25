using System;
using UnityEngine;

namespace Delayed_Messaging.Scripts.Interaction
{
    public class RaycastCursor : MonoBehaviour
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
        
        public void SetCursorState(CursorState state)
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
