using System;
using System.Collections.Generic;
using System.Linq;
using DG.Tweening;
using Spaces.Scripts.Space_Objects;
using UnityEngine;
using SpaceState = Spaces.Scripts.SpaceManager.SpaceState;
using Gizmos = Popcron.Gizmos;

namespace Spaces.Scripts
{
    public class Space : MonoBehaviour
    {
        private SpaceManager spaceManager;
        private SpaceData spaceData;
        private InactiveTransform inactiveTransform;
        
        private const float QuaternionIdentity = 0.7071068f;
        private static readonly Quaternion FlatQuaternion = new Quaternion(QuaternionIdentity, 0, 0, QuaternionIdentity);

        public struct InactiveTransform
        {
            public Vector3 position;
            public Quaternion rotation;

            public void SetTransform(Vector3 p, Quaternion r)
            {
                position = p;
                rotation = r;
            }
        }

        private struct SpaceData
        {
            public SpaceState spaceState;
            public Color spaceColour;
            public Vector3 spacePosition;
            public Vector3 spaceRotation;
        }
        
        public List<SpaceObject> spaceObjects = new List<SpaceObject>();

        public void ConfigureSpace(SpaceManager manager)
        {
            spaceManager = manager;
        }
        public void SetSpaceData(Color color, SpaceState state, Vector3 position, Vector3 rotation)
        {
            spaceData.spaceColour = color;
            spaceData.spacePosition = position;
            spaceData.spaceRotation = rotation;
            SetSpaceState(state);
        }

        private void Update()
        {
            Transform t = transform;
            inactiveTransform.SetTransform(t.position, t.rotation);
        }

        public void SetSpaceState(SpaceState state)
        {
            switch (state)
            {
                case SpaceState.ACTIVE:
                    SetSpaceActive();
                    break;
                case SpaceState.INACTIVE:
                    SetSpaceInactive();
                    break;
                case SpaceState.PREVIEW:
                    SetSpacePreview();
                    break;
                default:
                    break;
            }
        }

        private void SetSpaceActive()
        {
            spaceData.spaceState = SpaceState.ACTIVE;
            
            foreach (SpaceObject spaceObject in spaceObjects)
            {
                spaceObject.transform.DOMove(spaceObject.activePosition, spaceManager.active.time).SetEase(spaceManager.active.curve);
            }
        }

        private void SetSpaceInactive()
        {
            spaceData.spaceState = SpaceState.INACTIVE;
            
            foreach (SpaceObject spaceObject in spaceObjects)
            {
                spaceObject.transform.DOMove(inactiveTransform.position, spaceManager.inactive.time).SetEase(spaceManager.inactive.curve);
            }
        }

        private void SetSpacePreview()
        {
            spaceData.spaceState = SpaceState.PREVIEW;
            
            foreach (SpaceObject spaceObject in spaceObjects)
            {
                
            }
        }
        
        private void OnDrawGizmos()
        {
            DrawGizmos();
        }

        private void DrawGizmos()
        {
            Vector3 position = transform.position;
            Vector3 adjustedPosition = new Vector3(position.x, 0, position.z);

            foreach (Vector3 adjustedObjectPosition in spaceObjects.Select(spaceObject => spaceObject.transform.position).Select(objectPosition => new Vector3(objectPosition.x, 0, objectPosition.z)))
            {
                Gizmos.Circle(adjustedObjectPosition, .15f, FlatQuaternion, spaceData.spaceColour);
                Gizmos.Line(adjustedPosition, adjustedObjectPosition, spaceData.spaceColour);
            }
            
            Gizmos.Circle(adjustedPosition, 1f, FlatQuaternion, spaceData.spaceColour);
        }
    }
}
