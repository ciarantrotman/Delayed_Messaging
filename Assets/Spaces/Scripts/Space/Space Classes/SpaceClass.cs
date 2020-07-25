using System.Collections.Generic;
using Spaces.Scripts.Objects;
using UnityEngine;

namespace Spaces.Scripts.Space.Space_Classes
{
    [CreateAssetMenu(fileName = "Space", menuName = "Space", order = 2)]
    public class SpaceClass : ScriptableObject
    {
        public List<Color> spaceColours;
    }
}
