using System.Collections.Generic;
using VR_Prototyping.Scripts;

namespace Delayed_Messaging.Scripts.Structures
{
    public class Base : Structure
    { 
        public override void QuickSelect(Selection.MultiSelect side, List<BaseObject> list)
        {
            /*
            Instantiate(unit);
            unit.transform.position = spawnOrigin.position;
            unit.transform.forward = transform.forward;
            Unit unitUnit = unit.GetComponent<Unit>();
            unitUnit.InitialiseUnit();
            unitUnit.SetDestination(spawnDestination.position);
            */
            
            base.QuickSelect(side, list);
        }
    }
}
