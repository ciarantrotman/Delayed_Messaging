using Delayed_Messaging.Scripts.Utilities;
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
        
        [Header("Aesthetics")]
        [SerializeField] private GameObject destinationVisual;
        
        private AIDestinationSetter destinationSetter;
        private AIPath aiPath;

        private GameObject unitDestination;

        private bool intialised;
        
        private void Start()
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
            
            //aiPath = transform.AddOrGetAIPath();
            //aiPath.SetupAIPath(unitClass);

            destinationSetter = t.AddOrGetAIDestinationSetter();

            unitDestination = new GameObject("[" + name + " Destination]");
            unitDestination.transform.position = t.position;
            
            destinationVisual = Instantiate(destinationVisual, unitDestination.transform);
            destinationVisual.SetActive(false);
            
            destinationSetter.target = unitDestination.transform;

            intialised = true;
        }

        public override void SelectStart()
        {
            base.SelectStart();
            destinationVisual.SetActive(true);
        }

        public override void Deselect()
        {
            base.Deselect();
            destinationVisual.SetActive(false);
        }

        public override void QuickSelect()
        {
            
        }

        public void Damage(float damageTaken)
        {

        }
        
        public void Move(Vector3 destination)
        {
            if (unitDestination == null)
            {
                Debug.LogError(name + " tried to move but there is no <b>UnitDestination</b> defined!");
                return;
            }
            unitDestination.transform.position = destination;
        }
        
        private void OnDrawGizmos () 
        {
            if (unitClass == null)
            {
                return;
            }
            if (unitClass.debugType == BaseClass.DebugType.ALWAYS)
            {
                DrawGizmos ();
            }
        }
        private void OnDrawGizmosSelected ()
        {
            if (unitClass == null)
            {
                return;
            }
            if (unitClass.debugType == BaseClass.DebugType.SELECTED_ONLY)
            {
                DrawGizmos ();
            }
        }
        private void DrawGizmos ()
        {
            if (unitClass == null)
            {
                return;
            }
            
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
