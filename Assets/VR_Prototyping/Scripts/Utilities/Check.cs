using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using VR_Prototyping.Scripts.Accessibility;
using Object = UnityEngine.Object;

namespace VR_Prototyping.Scripts.Utilities
{
    public static class Check
    {
        public static void Manipulation(this Object focusObject, Object oppFocusObject, BaseObject baseObject, BaseObject previous, bool grip, bool pGrip, Transform con, bool touch, bool oppTouch)
        {
            if (focusObject == null || baseObject == null || touch) return;
            /*
            if (oppTouch && oppFocusObject  == focusObject) return;
            
            if (grip && !pGrip)
            {
                baseObject.GrabStart(con);
            }
            if (grip && pGrip)
            {
                baseObject.GrabStay(con);
            }
            if (!grip && pGrip)
            {
                previous.GrabEnd(con);
            }*/
        }
        
        public static void Selection(this Object focusObject, BaseObject selectableObject, bool select, bool pSelect)
        {
            if (focusObject == null || selectableObject == null) return;
            if (select && !pSelect)
            {
                selectableObject.SelectStart();
            }
            if (select && pSelect)
            {
                selectableObject.SelectStay();
            }
            if (!select && pSelect)
            {
                selectableObject.SelectEnd();
            }
        }

        /// <summary>
        /// Checks for the state of hovering for the Base Object
        /// </summary>
        /// <param name="current"></param>
        /// <param name="previous"></param>
        /// <param name="other"></param>
        public static void Hover(this BaseObject current, BaseObject previous, BaseObject other)
        {
            if (current != previous && current != null)
            {
                current.HoverStart();
                return;
            }
            if (current != null && (current == other || current == previous))
            {
                current.HoverStay();
                return;
            }
            if (previous != other && previous != null && current != previous)
            {
                previous.HoverEnd();
            }
        }

        public static void JoystickTracking(this List<Vector2> list, Vector2 current, float sensitivity)
        {
            list.Add(current);
            CullList(list, sensitivity);
        }
        
        public static void PositionTracking(this List<Vector3> list, Vector3 current, float sensitivity)
        {
            list.Add(current);
            CullList(list, sensitivity);
        }
        
        public static void RotationTracking(this List<Vector3> list, Vector3 current, float sensitivity)
        {
            list.Add(current);
            CullList(list, sensitivity);
        }

        
        private static void CullList(this IList list, float sensitivity)
        {
            if (list.Count > sensitivity)
            {
                list.RemoveAt(0);
            }
        }

        public static void Target(this GameObject visual, GameObject parent, Transform normal, Vector2 pos, GameObject target, bool advanced)
        {
            visual.transform.LookAt(RotationTarget(pos, target, advanced));
            
            parent.transform.forward = normal.forward;
        }
        
        private static Transform RotationTarget(this Vector2 pos, GameObject target, bool advanced)
        {
            target.transform.localPosition = advanced? Vector3.Lerp(target.transform.localPosition, new Vector3(pos.x, 0, pos.y), .1f) : Vector3.forward;
            return target.transform;
        }
        
        public static float ControllerAngle(this GameObject follow, GameObject proxy, GameObject normal, Transform controller, Transform head, bool debug)
        {
            proxy.transform.Position(controller);
            proxy.transform.ForwardVector(controller);
            proxy.transform.SplitRotation(normal.transform, true);
            head.SplitPosition(controller, follow.transform);
            follow.transform.LookAt(proxy.transform);
            
            Vector3 normalDown = -normal.transform.up;
            Vector3 proxyForward = proxy.transform.forward;
            Vector3 position = proxy.transform.position;
            
            if (!debug) return Vector3.Angle(normalDown, proxyForward);
            
            Debug.DrawLine(follow.transform.position, position, Color.red);
            Debug.DrawRay(normal.transform.position, normalDown, Color.blue);
            Debug.DrawRay(position, proxyForward, Color.blue);

            return Vector3.Angle(normalDown, proxyForward);
        }
        
        public static float CalculateDepth(this float angle, float maxAngle, float minAngle, float max, float min, Transform proxy)
        {
            float a = angle;

            a = a > maxAngle ? maxAngle : a;
            a = a < minAngle ? minAngle : a;
            
            float proportion = Mathf.InverseLerp(maxAngle, minAngle, a);
            return Mathf.SmoothStep(max, min, proportion);
        }
        
        public static void TargetLocation(this GameObject target, GameObject hitPoint, Vector3 lastValidPosition, int layerIndex)
        {
            Transform t = target.transform;
            Vector3 position = t.position;
            Vector3 up = t.up;
            hitPoint.transform.position = Vector3.Lerp(hitPoint.transform.position, Physics.Raycast(position, -up, out RaycastHit hit) && hit.transform.gameObject.layer == layerIndex ? hit.point : lastValidPosition, .25f);
            hitPoint.transform.up = Physics.Raycast(position, -up, out RaycastHit h) ? h.normal : Vector3.up;
        }

        public static GameObject RayCastFindFocusObject(this List<GameObject> objects, GameObject current, GameObject target, GameObject inactive, Transform controller, float distance, bool disable)
        {
            if (disable) return current == null ? null : current;
            
            var position = controller.position;
            var forward = controller.forward;

            if (Physics.Raycast(position, forward, out var hit, distance) && objects.Contains(hit.transform.gameObject))
            {
                target.transform.SetParent(hit.transform);
                target.transform.VectorLerpPosition(hit.point, .15f);
                return hit.transform.gameObject;
            }

            target.transform.SetParent(null);
            target.transform.TransformLerpPosition(inactive.transform, .1f);
            return null;
        }
        
        public static GameObject FuzzyFindFocusObject(this List<GameObject> objects, GameObject current, GameObject target, GameObject inactive, bool disable)
        {
            if (disable)
            {
                target.transform.TransformLerpPosition(inactive.transform, .05f);
                return current == null ? null : current;
            }
            
            target.transform.TransformLerpPosition(objects.Count > 0 ? objects[0].gameObject.transform: inactive.transform, .1f);
            return objects.Count > 0 ? objects[0].gameObject : null;
        }
        /// <summary>
        /// Finds an active object, it prioritises ray-cast, then uses the fuzzy method to find a center of an object
        /// </summary>
        /// <param name="objects"></param>
        /// <param name="current"></param>
        /// <param name="target"></param>
        /// <param name="inactive"></param>
        /// <param name="controller"></param>
        /// <param name="forward"></param>
        /// <param name="distance"></param>
        /// <param name="disable"></param>
        /// <returns></returns>
        public static GameObject FusionFindFocusObject(this List<GameObject> objects, GameObject current, GameObject target, GameObject inactive, Transform controller, Vector3 forward, float distance, bool disable)
        {
            if (disable && current == null)
            {
                target.transform.TransformLerpPosition(inactive.transform, .2f);
                return null;
            }

            if (disable) return current == null ? null : current;

            Vector3 position = controller.position;

            if (Physics.Raycast(position, forward, out RaycastHit hit, distance) && objects.Contains(hit.transform.gameObject))
            {
                target.transform.SetParent(hit.transform);
                target.transform.VectorLerpPosition(hit.point, .25f);
                target.transform.forward = hit.normal;
                return hit.transform.gameObject;
            }

            if (objects.Count > 0)
            {
                target.transform.SetParent(objects[0].gameObject.transform);
                target.transform.TransformLerpPosition(objects[0].gameObject.transform, .25f);
                return objects[0].gameObject;
            }
            
            target.transform.SetParent(null);
            target.transform.TransformLerpPosition(inactive.transform, .2f);
            return null;
        }

        public static BaseObject FindSelectableObject(this GameObject focusObject, BaseObject current, bool disable)
        {
            if (disable) return current == null ? null : current;
            
            if (focusObject == null) return null;
            return focusObject.GetComponent<BaseObject>() != null ? focusObject.GetComponent<BaseObject>() : null;
        }
        
        public static void DrawLineRenderer(this LineRenderer lr, GameObject focus, GameObject midpoint, Transform controller, GameObject target, int quality)
        {
            midpoint.transform.localPosition = new Vector3(0, 0, controller.Midpoint(target.transform));
            
            lr.LineRenderWidth(.001f, focus != null ? .01f : 0f);
            
            lr.BezierLineRenderer(controller.position,
                midpoint.transform.position, 
                target.transform.position,
                quality);
        }
        
        public static void GrabStart(this GameObject f, GameObject p, GameObject target, GameObject o, Transform con)
        {
            f.transform.LookAt(con);
            p.transform.position = con.position;
            p.transform.LookAt(target.transform);
            target.transform.SetParent(con);
            o.transform.position = con.position;
        }
        
        public static void FocusObjectFollow(this Transform focus, Transform con, Transform tar, Transform tarS, Transform objO, Transform conO, Transform objP, bool d)
        {
            if (focus.transform.gameObject == null || d) return;
			
            tar.Transforms(focus);
            tarS.Transforms(focus);
            objO.Transforms(focus);
            conO.Position(con);
            objP.Position(con);
        }
        public static bool IsCollinear(this Vector3 a, Vector3 b, Vector3 x, float tolerance)
        {
            return Math.Abs(Vector3.Distance(a, x) + Vector3.Distance(b, x) - Vector3.Distance(a, b)) < tolerance;
        }
        public static void CheckGaze(this GameObject o, float a, float c, ICollection<GameObject> g, ICollection<GameObject> l, ICollection<GameObject> r, ICollection<GameObject> global)
        {
            if (!global.Contains(o))
            {
                g.Remove(o);
                l.Remove(o);
                r.Remove(o);
            }
				
            if (a < c/2 && !g.Contains(o))
            {
                g.Add(o);
            }
			
            else if (a > c/2)
            {
                g.Remove(o);
                l.Remove(o);
                r.Remove(o);
            }
        }
        /// <summary>
        /// Checks the current manual angle against the allowed angle
        /// </summary>
        /// <param name="gameObject"></param>
        /// <param name="gazeList"></param>
        /// <param name="manualAngle"></param>
        /// <param name="currentAngle"></param>
        /// <returns></returns>
        public static bool CheckHand(this GameObject gameObject, ICollection<GameObject> gazeList, float manualAngle, float currentAngle)
        {
            if (!gazeList.Contains(gameObject)) return false;
            return manualAngle > currentAngle / 2;
        }
        public static void ManageList(this GameObject g, ICollection<GameObject> l, bool b, bool d, bool r)
        {
            if (d || !r) return;
			
            if (b && !l.Contains(g))
            {
                l.Add(g);
            }
            else if (!b && l.Contains(g))
            {
                l.Remove(g);
            }
        }
        public static float TransformDistance(this Transform a, Transform b)
        {
            return Vector3.Distance(a.position, b.position);
        }
        
        public static bool TransformDistanceCheck(this Transform a, Transform b, float distance)
        {
            return Vector3.Distance(a.position, b.position) < distance;
        }
        /// <summary>
        /// Checks if an object is within range
        /// </summary>
        /// <param name="enabled"></param>
        /// <param name="self"></param>
        /// <param name="user"></param>
        /// <param name="range"></param>
        /// <returns></returns>
        public static bool WithinRange(this Transform self, bool enabled, Transform user, float range)
        {
            if (!enabled) return true;
            return Vector3.Distance(self.position, user.position) <= range;
        }
    }
}
