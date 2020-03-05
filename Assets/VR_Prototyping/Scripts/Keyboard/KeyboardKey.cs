﻿using DG.Tweening;
//using Sirenix.OdinInspector;
using UnityEngine;
using UnityEngine.Rendering;

namespace VR_Prototyping.Scripts.Keyboard
{
    public class KeyboardKey : BaseObject
    {
        public KeyboardManager KeyboardManager { private get; set; }
        public int Index { private get; set; }
        private float borderDepth;

        [SerializeField] private Transform hoverBorder;
        [Space(10)] [SerializeField] [Range(0f, .01f)] private float hoverRestDepth;
        [SerializeField] [Range(.01f, .05f)] private float hoverActiveDepth;
        [SerializeField] [Range(.1f, 1f)] private float duration;
        [Space(10)] public KeyboardManager.KeyboardKeyValues keyValue;

        private void Awake()
        {
            //selectStart.AddListener(KeyStroke);
        }

        public void SetupKey(KeyboardManager manager, GameObject p)
        {
            KeyboardManager = manager;
            playerObject = p;
            name = "Key_" + (char)keyValue;
            //buttonText.SetText(""+(char)keyValue);
            //buttonBack.shadowCastingMode = ShadowCastingMode.Off;
            //toolTipText = keyValue.ToString();
            
            //hoverStart.AddListener(HoverBorderStart);
            //hoverEnd.AddListener(HoverBorderEnd);
        }
        
        private void KeyStroke()
        {
            KeyboardManager.Keystroke(Index, keyValue);
        }

        protected override void Initialise()
        {
            throw new System.NotImplementedException();
        }

        protected override void ObjectUpdate()
        {
            hoverBorder.localPosition = new Vector3(0, 0, borderDepth);
        }

        private void HoverBorderStart()
        {
            borderDepth = hoverBorder.localPosition.z;
            DOTween.To(()=> borderDepth, x=> borderDepth = x, -hoverActiveDepth, duration);
        }

        private void HoverBorderEnd()
        {
            borderDepth = hoverBorder.localPosition.z;
            DOTween.To(()=> borderDepth, x=> borderDepth = x, -hoverRestDepth, duration);
        }
    }
}
