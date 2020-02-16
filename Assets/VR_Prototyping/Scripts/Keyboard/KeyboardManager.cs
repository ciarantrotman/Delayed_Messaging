using System.Collections.Generic;
using Sirenix.OdinInspector;
using UnityEngine;
using UnityEngine.Events;
using VR_Prototyping.Scripts.Tools;

namespace VR_Prototyping.Scripts.Keyboard
{
    public class KeyboardManager : MonoBehaviour
    { 
        public KeyboardTarget KeyboardTarget { get; private set; }
        private ToolMenu ToolMenu { get; set; }
        private ControllerTransforms ControllerTransforms { get; set; }
        public enum KeyboardKeyValues
        {
            Q = 'Q',
            W = 'W',
            E = 'E',
            R = 'R',
            T = 'T',
            Y = 'Y',
            U = 'U',
            I = 'I',
            O = 'O',
            P = 'P',
            A = 'A',
            S = 'S',
            D = 'D',
            F = 'F',
            G = 'G',
            H = 'H',
            J = 'J',
            K = 'K',
            L = 'L',
            Z = 'Z',
            X = 'X',
            C = 'C',
            V = 'V',
            B = 'B',
            N = 'N',
            M = 'M',
            SPACE = ' ', 
            COMMA = ',', 
            PERIOD = '.', 
            QUESTION_MARK = '?', 
            EXCLAMATION_MARK = '!',
            ONE = '1', 
            TWO = '2', 
            THREE = '3', 
            FOUR = '4', 
            FIVE = '5', 
            SIX = '6', 
            SEVEN = '7', 
            EIGHT = '8', 
            NINE = '9', 
            ZERO = '0',
            BACK = '<', 
            ENTER = '¬'
        }
        
        [BoxGroup] [SerializeField] [Required] private Transform keyboardParent;
        [BoxGroup] [HideInEditorMode] [Space(10)] public List<KeyboardKey> keyboardKeys = new List<KeyboardKey>();
        [HideInInspector] public UnityEvent enter;

        public void InitialiseKeyboard(ControllerTransforms c, ToolMenu t, Transform parent)
        {
            ControllerTransforms = c;
            ToolMenu = t;
            transform.SetParent(parent);
            
            int index = 0;
            foreach (Transform child in keyboardParent)
            {
                if (child.GetComponent<KeyboardKey>() == null) continue;
                KeyboardKey key = child.GetComponent<KeyboardKey>();
                key.KeyboardManager = this;
                key.player = ControllerTransforms.Player();
                key.Index = index;
                key.SetupKey(this, ControllerTransforms.Player());
                keyboardKeys.Add(key);
                index++;
            }

            KeyboardTarget = GetComponentInChildren<KeyboardTarget>();
            
            ToggleKeyboard(false);
        }
        
        public void ToggleKeyboard(bool state)
        {
            foreach (KeyboardKey key in keyboardKeys)
            {
                key.enabled = state;
                key.gameObject.SetActive(state);
                KeyboardTarget.gameObject.SetActive(state);
            }
        }

        public void Keystroke(int index, KeyboardKeyValues key)
        {
            if (KeyboardTarget == null) return;

            switch (key)
            {
                case KeyboardKeyValues.BACK:
                    KeyboardTarget.DeleteText();
                    break;
                case KeyboardKeyValues.ENTER:
                    enter.Invoke();
                    break;
                default:
                    KeyboardTarget.SetText((char)keyboardKeys[index].keyValue);
                    break;
            }
        }
    }
}
