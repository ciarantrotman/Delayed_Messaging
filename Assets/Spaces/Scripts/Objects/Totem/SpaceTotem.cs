using UnityEngine;

namespace Spaces.Scripts.Objects.Totem
{
    public class SpaceTotem : MonoBehaviour
    {
        private MeshRenderer TotemRenderer => GetComponentInChildren<MeshRenderer>();
        private static readonly int TotemColour = Shader.PropertyToID("_TotemColour");

        public void SetTotemColour(Color color)
        {
            // todo, figure out how to do this with a single material
            //TotemRenderer.material.SetColor(TotemColour, color);
        }
    }
}
