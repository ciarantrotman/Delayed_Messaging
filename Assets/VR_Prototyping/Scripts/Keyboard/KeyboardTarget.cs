using TMPro;
using UnityEngine;

namespace VR_Prototyping.Scripts.Keyboard
{
    public class KeyboardTarget : MonoBehaviour
    {
        private string text;
        private TextMeshPro textField;

        private void Awake()
        {
            textField = GetComponentInChildren<TextMeshPro>();
        }

        public void SetText(char newText)
        {
            text += newText;
            textField.SetText(text);
        }

        public string CheckText()
        {
            return text;
        }

    public void DeleteText()
        {
            text = text.Substring(0, text.Length - 1);
            textField.SetText(text);
        }
    
    public void ClearText()
        {
            text = "";
            textField.SetText(text);
        }
    }
}
