using System;
using Spaces.Scripts.Utilities;
using UnityEngine;

namespace Spaces.Scripts.Player
{
    [RequireComponent(typeof(ControllerTransforms))]
    public class ObjectCreatorManager : MonoBehaviour
    {
        private ControllerTransforms Controller => GetComponent<ControllerTransforms>();
        private GameObject head, creation;

        [SerializeField, Range(1, 5)] private float defaultOffset = 2f;

        private void Awake()
        {
            head = Set.Object(gameObject, "[Creator Manager / Head]", Vector3.zero);
            creation = Set.Object(head, "[Creator Manager / Creation Location]", new Vector3(0,0, defaultOffset));
        }

        private void Update()
        {
            head.transform.LerpTransform(Controller.Transform(ControllerTransforms.Check.HEAD), .75f);
        }
        /// <summary>
        /// Returns the position that the new object will be put
        /// </summary>
        /// <returns></returns>
        public Vector3 CreationLocation()
        {
            // todo offset raycast position
            return Physics.Raycast(head.transform.position, head.transform.forward, out RaycastHit hit, defaultOffset) ? hit.point : creation.transform.position;
        }
    }
}
