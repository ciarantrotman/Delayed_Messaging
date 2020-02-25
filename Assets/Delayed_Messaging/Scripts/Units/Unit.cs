using Delayed_Messaging.Scripts.Utilities;
using Panda;
using Pathfinding;
using UnityEngine;
using VR_Prototyping.Scripts;
using VR_Prototyping.Scripts.Utilities;

namespace Delayed_Messaging.Scripts.Units
{
    public class Unit : BaseObject, IDamageable<float>, IMovable<Vector3>
    {
        [Header("Unit Specific Settings")]
        public UnitClass unitClass;
        public UnitClass.UnitData unitData;
        
        [SerializeField, Space(10)] private Material destinationLineRendererMat;
        
        internal AIDestinationSetter destinationSetter;
        internal AIPath aiPath;

        internal GameObject unitDestination;
        private LineRenderer destinationLineRenderer;

        private bool intialised;

        protected override void Initialise()
        {
            InitialiseUnit();
        }
        
        public void InitialiseUnit()
        {
            if (intialised)
            {
                return;
            }
            
            Transform t = transform;

            aiPath = GetComponent<AIPath>();
            //aiPath.SetupAIPath(unitClass);
            
            unitDestination = new GameObject("[" + name + " Destination]");

            unitDestination.transform.position = t.position;

            destinationSetter = transform.GetComponent<AIDestinationSetter>();
            destinationSetter.target = unitDestination.transform;

            destinationLineRenderer = unitDestination.transform.AddOrGetLineRenderer();
            destinationLineRenderer.SetupLineRender(destinationLineRendererMat, .005f, false);

            intialised = true;
        }
        protected override void ObjectUpdate()
        {
            destinationLineRenderer.DrawStraightLineRender(transform, unitDestination.transform);
        }

        public override void SelectStart(Selection.MultiSelect side)
        {
            destinationLineRenderer.enabled = true;
            base.SelectStart(side);
        }

        public override void Deselect(Selection.MultiSelect side)
        {
            destinationLineRenderer.enabled = false;
            base.Deselect(side);
        }

        public void Damage(float damageTaken)
        {

        }
        
        [Task]
        public void Move(Vector3 destination)
        {
            if (unitDestination == null)
            {
                Debug.LogError(name + " tried to move but there is no <b>[Unit Destination]</b> defined!");
                return;
            }
            unitDestination.transform.position = destination;
        }

        protected override void DrawGizmos ()
        {
            base.DrawGizmos();

            // Cache
            Transform t = transform;
            Vector3 pos = t.position;
            Vector3 r = t.forward;

            // Debug
            if (destinationSetter != null && destinationSetter.target != null)
            {
                Gizmos.color = unitClass.destinationColour;
                Gizmos.DrawWireSphere(destinationSetter.target.position, .05f);
            }
            
            // Vectors
            Gizmos.color = unitClass.forwardVectorColour;
            Gizmos.DrawRay(pos, r.normalized);
        }
    }
}
