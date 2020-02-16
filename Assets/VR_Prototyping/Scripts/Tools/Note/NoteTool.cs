using System;
using System.Globalization;
using Sirenix.OdinInspector;
using UnityEngine;
using VR_Prototyping.Scripts.Keyboard;

namespace VR_Prototyping.Scripts.Tools.Note
{
    public class NoteTool : BaseTool
    {
        [BoxGroup("Note Tool Settings")] [Required]
        public GameObject notePrefab;

        private float index;
        private KeyboardManager KeyboardManager { get; set; }

        private GameObject note;
        private NoteObject noteObject;

        protected override void OnStart()
        {
            KeyboardManager = ToolMenu.KeyboardManager;
        }

        protected override void ToolActivate()
        {
            KeyboardManager.ToggleKeyboard(true);
            KeyboardManager.enter.AddListener(FinishNote);
        }

        protected override void ToolDeactivate()
        {
            if (KeyboardManager == null) return;
            KeyboardManager.ToggleKeyboard(false);
            KeyboardManager.enter.RemoveListener(FinishNote);
        }

        private void NewNote()
        {
            note = Instantiate(notePrefab);
            noteObject = note.GetComponent<NoteObject>();
            noteObject.Initialise(this, Controller);
        }

        private void FinishNote()
        {
            NewNote();
            Vector3 position = Controller.CameraTransform().position + Controller.CameraForwardVector();
            noteObject.SetNote(KeyboardManager.KeyboardTarget.CheckText(),
                DateTime.UtcNow.ToString(CultureInfo.InvariantCulture),
                "Note " + index,
                position);
            KeyboardManager.KeyboardTarget.ClearText();
            note.name = "Note_" + index;
            note = null;
            noteObject = null;
            index++;
        }
    }
}