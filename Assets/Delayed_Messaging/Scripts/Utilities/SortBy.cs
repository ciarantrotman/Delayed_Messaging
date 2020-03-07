using UnityEngine;

namespace VR_Prototyping.Scripts.Utilities
{
    public static class SortBy
    {
        public static int FocusObjectL(GameObject obj1, GameObject obj2)
        {
            return obj1.GetComponent<BaseObject>().AngleL.CompareTo(obj2.GetComponent<BaseObject>().AngleL);
        }
        public static int FocusObjectR(GameObject obj1, GameObject obj2)
        {
            return obj1.GetComponent<BaseObject>().AngleR.CompareTo(obj2.GetComponent<BaseObject>().AngleR);
        }
        public static int FocusObjectG(GameObject obj1, GameObject obj2)
        {
            return obj1.GetComponent<BaseObject>().AngleG.CompareTo(obj2.GetComponent<BaseObject>().AngleG);
        }
        public static int CastObjectR(GameObject obj1, GameObject obj2)
        {
            return obj1.GetComponent<BaseObject>().CastDistanceL.CompareTo(obj2.GetComponent<BaseObject>().CastDistanceR);
        }
        public static int CastObjectL(GameObject obj1, GameObject obj2)
        {
            return obj1.GetComponent<BaseObject>().CastDistanceL.CompareTo(obj2.GetComponent<BaseObject>().CastDistanceR);
        }
    }
}
