﻿using System;
using Delayed_Messaging.Scripts.Utilities;
using UnityEngine;
using UnityEngine.Events;
using VR_Prototyping.Plugins.QuickOutline.Scripts;

namespace Grapple.Scripts.User_Interface
{
    public class Button : DirectInterface
    {
        [SerializeField] private GameObject buttonModel;
        [HideInInspector] public UnityEvent buttonPress;
        private Outline outline;

        protected override void Initialise()
        {
            // Configure button
            gameObject.tag = "Button";
            
            // Configure outline
            outline = buttonModel.AddComponent<Outline>();
            outline.Outline(Outline.Mode.OutlineAll, 10f, Color.white, false);
        }

        private void OnTriggerEnter(Collider other)
        {
            outline.enabled = true;
        }

        private void OnTriggerExit(Collider other)
        {
            outline.enabled = false;
        }
    }
}
