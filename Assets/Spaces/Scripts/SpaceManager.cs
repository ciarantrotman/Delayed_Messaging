using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Random = UnityEngine.Random;

namespace Spaces.Scripts
{
    public class SpaceManager : MonoBehaviour
    {
        public enum SpaceState { ACTIVE, INACTIVE, PREVIEW }
        public struct StateChangeAnimation
        {
            public float time;
            public AnimationCurve curve;
        }

        public StateChangeAnimation active;
        public StateChangeAnimation inactive;
        
        [SerializeField] private List<Color> spaceColours;

        private List<Space> spaces = new List<Space>();

        private void Start()
        {
            CreateSpace();
        }

        public void CreateSpace()
        {
            GameObject spaceObject = new GameObject {name = $"[Space {spaces.Count()}]"};
            Space space = spaceObject.AddComponent<Space>();
            space.ConfigureSpace(this);
            space.SetSpaceData(
                spaceColours[Random.Range(0, spaceColours.Count)], 
                SpaceState.ACTIVE, 
                Vector3.zero, 
                Vector3.zero);
            spaces.Add(space);
        }
    }
}
