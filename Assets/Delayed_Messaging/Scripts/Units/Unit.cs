using System.Collections.Generic;
using Delayed_Messaging.Scripts.Utilities;
using Panda;
using Pathfinding;
using UnityEngine;
using VR_Prototyping.Scripts;
using VR_Prototyping.Scripts.Utilities;
using Draw = Pathfinding.Util.Draw;

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
        private bool overrideDirection;

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

            ObjectClass = unitClass;
            
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

        public override void SelectStart(Selection.MultiSelect side, List<BaseObject> list)
        {
            destinationLineRenderer.enabled = true;
            base.SelectStart(side, list);
        }

        public override void Deselect(Selection.MultiSelect side, List<BaseObject> list)
        {
            destinationLineRenderer.enabled = false;
            base.Deselect(side, list);
        }

        public void Damage(float damageTaken)
        {

        }

        [Task] public bool UnitDirected()
        {
            return overrideDirection;
        }

        public void SetDestination(Vector3 destination)
        {
            overrideDirection = true;
            
            if (unitDestination == null)
            {
                Debug.LogError(name + " tried to move but there is no <b>[Unit Destination]</b> defined!");
                return;
            }
            
            unitDestination.transform.position = destination;
            CancelCurrentTask();
        }

        [Task] public void DirectedMovement()
        {
            if (transform.Arrived(unitDestination.transform, .3f))
            {
                Task.current.Succeed();
                overrideDirection = false;
            }
        }
        /// <summary>
        /// This cancels whatever task the current unit is doing
        /// </summary>
        protected virtual void CancelCurrentTask()
        {
            
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
                Vector3 position = unitDestination.transform.position;
                Draw.Gizmos.Line(position, pos, Color.black);
                Draw.Gizmos.CircleXZ(position, .01f, Color.black);
            }
            
            // Vectors
            Gizmos.color = unitClass.forwardVectorColour;
            Gizmos.DrawRay(pos, r.normalized);
            Draw.Gizmos.CircleXZ(pos, unitClass.detectionRadius, unitClass.detectionColour);
            Draw.Gizmos.CircleXZ(pos, unitClass.avoidanceRadius, unitClass.avoidanceColour);
            Draw.Gizmos.CircleXZ(pos, unitClass.attackRadius, unitClass.attackColour);
        }
    }
}
