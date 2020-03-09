using System;
using System.Collections;
using System.Collections.Generic;
using Delayed_Messaging.Scripts.Interaction.Cursors;
using Delayed_Messaging.Scripts.Objects;
using Delayed_Messaging.Scripts.Player;
using Delayed_Messaging.Scripts.Player.Selection;
using UnityEngine;
using VR_Prototyping.Scripts;

namespace Delayed_Messaging.Scripts.Utilities
{
    public static class Check
    {
        public static void MoveUnit(this UnitController unitController, bool current, bool previous, Cast.CastObjects castObjects, IEnumerable<BaseObject> units)
        {
            if (current && !previous)
            {
                UnitController.UnitMoveStart(castObjects);
                return;
            }
            if (current && previous)
            {
                unitController.UnitMoveStay(castObjects);
                return;
            }
            if (!current && previous)
            {
                unitController.UnitMoveEnd(castObjects, units);
                return;
            }
        }

        /// <summary>
        /// Used to determine the state of selection for focus objects.
        /// </summary>
        /// <param name="selectableObject"></param>
        /// <param name="selection"></param>
        /// <param name="selectionObjects"></param>
        /// <param name="cursorDistance"></param>
        /// <param name="currentSelectionState"></param>
        /// <param name="previousSelectionState"></param>
        /// <param name="multiSelectDistance"></param>
        /// <param name="disabled"></param>
        public static void Selection(BaseObject selectableObject, Selection selection, SelectionObjects selectionObjects, float cursorDistance, bool currentSelectionState, bool previousSelectionState, float multiSelectDistance, bool disabled)
        {
            if (disabled) return;

            //string debugName = selectableObject == null ? "Null" : selectableObject.name;
            //Debug.Log($"{debugName}, on {selectionObjects.side}: [{currentSelectionState}|{previousSelectionState}]");// [{Math.Round(cursorDistance, 1)}|{Math.Round(multiSelectDistance, 1)}]");

            // Is true if select is called for the first time, sets the cursor start position
            if (currentSelectionState && !previousSelectionState)
            {
                //Debug.Log("Selection Start");
                selection.SelectStart(selectionObjects);
                return;
            }
            // When you have a focus object, you have let go of the select button, and your cursor is still within the selection threshold
            if (selectableObject != null && !currentSelectionState && previousSelectionState && cursorDistance <= multiSelectDistance)
            {
                //Debug.Log("Object Quick Select");
                selectableObject.QuickSelect(selectionObjects);
                selection.UserInterface.SetObjectHeaderState(true);
                selection.UserInterface.SelectObject(selectableObject);
                return;
            }
            // Called if you are still holding select and the cursor is outside the threshold distance
            if (currentSelectionState && cursorDistance >= multiSelectDistance)
            {
                //Debug.Log("Select Multi Select Start / Hold");
                selection.MultiSelectStart(selectionObjects);
                selection.MultiSelectHold(selectionObjects);
                return;
            }
            // When you release the select button, but are still within the threshold
            if (selectableObject == null && !currentSelectionState && previousSelectionState && cursorDistance <= multiSelectDistance)
            {
                //Debug.Log("Select End");
                selection.SelectEnd(selectionObjects);
                return;
            }
            // When you release the select button, and are outside of the threshold
            if (/*selectableObject == null && */!currentSelectionState && previousSelectionState && cursorDistance >= multiSelectDistance)
            {
                //Debug.Log("Select Multi Select End");
                selection.MultiSelectEnd(selectionObjects);
                return;
            }
            //Debug.Log("No Selection");
        }

        /// <summary>
        /// Checks for the state of hovering for the Base Object
        /// </summary>
        /// <param name="current"></param>
        /// <param name="previous"></param>
        /// <param name="other"></param>
        /// <param name="castCursor"></param>
        public static void Hover(this BaseObject current, BaseObject previous, BaseObject other, CastCursor castCursor)
        {
            /*
             switch (Physics.Raycast(uiSelectionOrigin.transform.position, uiSelectionOrigin.transform.forward, out RaycastHit hit, interactionRange, 1 << userInterfaceInteractionLayer))
            {
                case true when currentInterface == null:
                    cursor.EnableCursor(true);
                    uiLineRenderer.enabled = true;
                    currentInterface = hit.collider.gameObject.GetComponent<RaycastInterface>();
                    currentInterface.HoverStart();
                    selection.ToggleSelectionState(dominantHand, false);
                    break;
                case true when currentInterface != null:
                    if (currentInterface != hit.collider.gameObject.GetComponent<RaycastInterface>())
                    {
                        currentInterface.HoverEnd();
                        currentInterface = hit.collider.gameObject.GetComponent<RaycastInterface>();
                        currentInterface.HoverStart();
                    }
                    currentInterface.HoverStay();
                    cursorObject.transform.position = hit.point;
                    uiLineRenderer.DrawStraightLineRender(cursorObject.transform, uiSelectionOrigin.transform);

                    if (currentSelect && !previousSelect)
                    {
                        currentInterface.OnSelect.Invoke();
                    }
                    
                    break;
                case false when currentInterface != null:
                    currentInterface.HoverEnd();
                    cursor.EnableCursor(false);
                    uiLineRenderer.enabled = false;
                    currentInterface = null;
                    selection.ToggleSelectionState(dominantHand, true);
                    break;
                default:
                    break;
            }
             */
            if (current == null)
            {
                castCursor.SetCursorState(CastCursor.CursorState.DEFAULT);
                return;
            }
            if (current != previous)
            {
                castCursor.SetCursorState(current.ObjectClass.cursorState);
                current.HoverStart();
                return;
            }
            if (current != null && (current == other || current == previous))
            {
                current.HoverStay();
                return;
            }
            if ((previous != other || previous != current) &&  previous != null)
            {
                previous.HoverEnd();
            }
        }

        /// <summary>
        /// Maintains a list of booleans at a certain capacity
        /// </summary>
        /// <param name="list"></param>
        /// <param name="current"></param>
        /// <param name="sensitivity"></param>
        public static void BoolListCull(this List<bool> list, bool current, float sensitivity)
        {
            list.Add(current);
            CullList(list, sensitivity);
        }
        /// <summary>
        /// Maintains a list of booleans at a certain capacity
        /// </summary>
        /// <param name="list"></param>
        /// <param name="current"></param>
        /// <param name="sensitivity"></param>
        public static void FloatListCull(this List<float> list, float current, float sensitivity)
        {
            list.Add(current);
            CullList(list, sensitivity);
        }
        /// <summary>
        /// Maintains a list of Vector2 at a certain capacity
        /// </summary>
        /// <param name="list"></param>
        /// <param name="current"></param>
        /// <param name="sensitivity"></param>
        public static void Vector2ListCull(this List<Vector2> list, Vector2 current, float sensitivity)
        {
            list.Add(current);
            CullList(list, sensitivity);
        }
        /// <summary>
        /// Maintains a list of Vector3 at a certain capacity
        /// </summary>
        /// <param name="list"></param>
        /// <param name="current"></param>
        /// <param name="sensitivity"></param>
        public static void Vector3ListCull(this List<Vector3> list, Vector3 current, float sensitivity)
        {
            list.Add(current);
            CullList(list, sensitivity);
        }
        
        public static void RotationTracking(this List<Vector3> list, Vector3 current, float sensitivity)
        {
            list.Add(current);
            CullList(list, sensitivity);
        }

        /// <summary>
        /// Culls a list to maintain a stable volume of values
        /// </summary>
        /// <param name="list"></param>
        /// <param name="sensitivity"></param>
        private static void CullList(this IList list, float sensitivity)
        {
            if (list.Count > sensitivity)
            {
                list.RemoveAt(0);
            }
        }

        public static void Target(this GameObject visual, GameObject parent, Transform normal, Vector2 pos, GameObject target, bool advanced = true)
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

        /// <summary>
        /// Calculate the target location
        /// </summary>
        /// <param name="target"></param>
        /// <param name="hitPoint"></param>
        /// <param name="lastValidPosition"></param>
        /// <param name="layerIndex"></param>
        /// <param name="normal"></param>
        public static void TargetLocation(this GameObject target, GameObject hitPoint, Vector3 lastValidPosition, int layerIndex, bool normal = false)
        {
            Transform t = target.transform;
            Vector3 position = t.position;
            Vector3 up = t.up;
            hitPoint.transform.position = Vector3.Lerp(hitPoint.transform.position, Physics.Raycast(position, -up, out RaycastHit hit, 100, 1 << layerIndex) ? hit.point : lastValidPosition, .25f);
            hitPoint.transform.up = Physics.Raycast(position, -up, out RaycastHit h) && normal ? h.normal : Vector3.up;
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

            if (Physics.SphereCast(position, .1f, forward, out RaycastHit hit, distance) && objects.Contains(hit.transform.gameObject))
            {
                target.transform.SetParent(hit.transform);
                target.transform.VectorLerpPosition(hit.point, .25f);
                target.transform.forward = hit.normal;
                /*
                switch (hit.transform.gameObject.GetComponent<BaseObject>().selectionType == Scripts.Selection.SelectionType.FUSION)
                {
                    
                }
                */
                return hit.transform.gameObject;
            }

            if (objects.Count > 0)
            {
                target.transform.SetParent(objects[0].gameObject.transform);
                target.transform.TransformLerpPosition(objects[0].gameObject.transform, .25f);
                return objects[0];
            }
            
            target.transform.SetParent(null);
            target.transform.TransformLerpPosition(inactive.transform, .2f);
            return null;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="castObjects"></param>
        /// <returns></returns>
        public static GameObject CastFindObject(this List<GameObject> castObjects)
        {
            return castObjects.Count > 0 ? castObjects[0] : null;
        }
        public static BaseObject FindBaseObject(this GameObject focusObject, BaseObject current, bool disable = false)
        {
            if (disable) return current == null ? null : current;
            
            if (focusObject == null) return null;
            return focusObject.GetComponent<BaseObject>() != null ? focusObject.GetComponent<BaseObject>() : null;
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
        public static bool WithinHandCone(this GameObject gameObject, ICollection<GameObject> gazeList, float manualAngle, float currentAngle)
        {
            if (!gazeList.Contains(gameObject)) return false;
            return manualAngle > currentAngle / 2;
        }
        /// <summary>
        /// Compares selection distance with the cast distance
        /// </summary>
        /// <param name="baseObject"></param>
        /// <param name="globalList"></param>
        /// <param name="castDistance"></param>
        /// <param name="currentDistance"></param>
        /// <returns></returns>
        public static bool WithinCastDistance(this GameObject baseObject, ICollection<GameObject> globalList, float castDistance, float currentDistance)
        {
            if (!globalList.Contains(baseObject))
            {
                return false;
            }
            
            return currentDistance <= castDistance;
        }
        /// <summary>
        /// Controls the appearance of game objects based on selection criteria
        /// </summary>
        /// <param name="gameObject"></param>
        /// <param name="list"></param>
        /// <param name="selectionConditionsMet"></param>
        /// <param name="disabled"></param>
        /// <param name="selectionRangeEnabled"></param>
        public static void ManageList(this GameObject gameObject, ICollection<GameObject> list, bool selectionConditionsMet, bool disabled = false, bool selectionRangeEnabled = true)
        {
            if (disabled || !selectionRangeEnabled) return;
			
            if (selectionConditionsMet && !list.Contains(gameObject))
            {
                list.Add(gameObject);
            }
            else if (!selectionConditionsMet && list.Contains(gameObject))
            {
                list.Remove(gameObject);
            }
        }
        public static float TransformDistance(this Transform a, Transform b)
        {
            if (a == null || b == null)
            {
                return 0f;
            }
            return Vector3.Distance(a.position, b.position);
        }
        
        public static bool DistanceCheck(this Transform a, Transform b, float distance)
        {
            return Vector3.Distance(a.position, b.position) < distance;
        }
        public static bool DistanceCheck(this Vector3 a, Vector3 b, float distance)
        {
            return Vector3.Distance(a, b) < distance;
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

        /// <summary>
        /// Placeholder to determine if an object has reached its destination, will not work for large objects so needs to be fixed
        /// </summary>
        /// <param name="unit"></param>
        /// <param name="target"></param>
        /// <param name="distance"></param>
        /// <returns></returns>
        public static bool Arrived(this Transform unit, Transform target, float distance)
        {
            return unit.DistanceCheck(target, distance);
        }
        public static bool Arrived(this Transform unit, Vector3 target, float distance)
        {
            return unit.position.DistanceCheck(target, distance);
        }
        /// <summary>
        /// Returns the largest value based on a Vector3
        /// </summary>
        /// <param name="vector3"></param>
        /// <returns></returns>
        public static float LargestValue(this Vector3 vector3)
        {
            float x = vector3.x;
            float y = vector3.y;
            float z = vector3.z;
            
            float largestValue = x > y ? x : y;
            largestValue = z > largestValue ? z : largestValue;

            return largestValue;
        }
    }
}
