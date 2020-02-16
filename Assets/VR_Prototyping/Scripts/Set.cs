using System;
using System.Collections.Generic;
//using Leap;
//using Leap.Unity;
using TMPro;
using UnityEditor;
using UnityEngine;
using VR_Prototyping.Plugins.QuickOutline.Scripts;

namespace VR_Prototyping.Scripts
{
    public static class Set
    {
        private static readonly int LeftHand = Shader.PropertyToID("_LeftHand");
        private static readonly int RightHand = Shader.PropertyToID("_RightHand"); 
        
        public enum Axis
        {
            X,
            Y,
            Z
        }
        /// <summary>
        /// Sets transform A's position to transform B's position
        /// </summary>
        /// <param name="a"></param>
        /// <param name="b"></param>
        public static void Position(this Transform a, Transform b)
        {
            if (a == null || b == null) return;
            a.transform.position = b.transform.position;
        }
        /// <summary>
        /// Sets transform A's rotation to transform B's rotation
        /// </summary>
        /// <param name="a"></param>
        /// <param name="b"></param>
        public static void Rotation(this Transform a, Transform b)
        {
            if (a == null || b == null) return;
            a.transform.rotation = b.transform.rotation;
        }
        /// <summary>
        /// Sets transform A's local position and rotation to zero
        /// </summary>
        /// <param name="a"></param>
        public static void LocalTransformZero(this Transform a)
        {
            Transform transform = a.transform;
            transform.localPosition = Vector3.zero;
            transform.localRotation = Quaternion.identity;
        }
        /// <summary>
        /// Sets transform A's local position and rotation to transform B's local position and rotation
        /// </summary>
        /// <param name="a"></param>
        /// <param name="b"></param>
        public static void LocalTransforms(this Transform a, Transform b)
        {
            Transform transform = a.transform;
            transform.localPosition = b.localPosition;
            transform.localRotation = b.localRotation;
        }
        /// <summary>
        /// Transform A looks at transform B, but maintains it's vertical axis
        /// </summary>
        /// <param name="a"></param>
        /// <param name="b"></param>
        public static void LookAtVertical(this Transform a, Transform b)
        {
            a.LookAwayFrom(b, Vector3.up);
            a.eulerAngles = new Vector3(0, a.eulerAngles.y,0);
        }
        /// <summary>
        /// Adds rotational force to a rigid-body to keep transform A and B in line
        /// </summary>
        /// <param name="rb"></param>
        /// <param name="a"></param>
        /// <param name="b"></param>
        /// <param name="force"></param>
        public static void AddForceRotation(this Rigidbody rb, Transform a, Transform b, float force)
        {
            if (a == null || b == null || rb == null) return;
            
            Quaternion r = Quaternion.FromToRotation(a.forward, b.forward);
            rb.AddTorque(r.eulerAngles * force, ForceMode.Force);
        }
        /// <summary>
        /// Transform C will follow transform XZ's x and z position, and transform Y's y position
        /// </summary>
        /// <param name="xz"></param>
        /// <param name="y"></param>
        /// <param name="c"></param>
        public static void SplitPosition(this Transform xz, Transform y, Transform c) // messed up the thing here
        {
            if (xz == null || y == null || c == null) return;
            Vector3 position = xz.position;
            c.transform.position = new Vector3(position.x, y.position.y, position.z);
        }
        /// <summary>
        /// Transform C will follow transform XZ's x and z position, and transform Y's y position
        /// </summary>
        /// <param name="c"></param>
        /// <param name="y"></param>
        /// <param name="xz"></param>
        public static void SplitPositionVector(this Transform c, float y, Transform xz)
        {
            if (xz == null || c == null) return;
            Vector3 position = xz.position;
            c.transform.position = new Vector3(position.x, y, position.z);
        }
        /// <summary>
        /// Transform C will follow transform XZ's x and z position, and transform Y's y position
        /// </summary>
        /// <param name="c"></param>
        /// <param name="xz"></param>
        /// <param name="y"></param>
        public static void PositionSplit(this Transform c, Transform xz, Transform y)
        {
            if (xz == null || y == null || c == null) return;
            Vector3 position = xz.position;
            c.transform.position = new Vector3(position.x, y.position.y, position.z);
        }
        /// <summary>
        /// Controller will follow the y-rotation of target, follow determines of it follows the position
        /// </summary>
        /// <param name="controller"></param>
        /// <param name="target"></param>
        /// <param name="follow"></param>
        public static void SplitRotation(this Transform controller, Transform target, bool follow)
        {
            if (controller == null || target == null) return;
            Vector3 c = controller.eulerAngles;
            target.transform.eulerAngles = new Vector3(0, c.y, 0);
            
            if(!follow) return;
            Position(target, controller);
        }
        /// <summary>
        /// Sets the width and colour of a trail renderer
        /// </summary>
        /// <param name="tr"></param>
        /// <param name="start"></param>
        /// <param name="end"></param>
        /// <param name="c"></param>
        public static void TrailRender(this TrailRenderer tr, float start, float end, Color c)
        {
            tr.startWidth = start;
            tr.endWidth = end;
            tr.material.color = c;
        }
        /// <summary>
        /// Transform A will lerp to transform B's position
        /// </summary>
        /// <param name="a"></param>
        /// <param name="b"></param>
        /// <param name="l"></param>
        public static void TransformLerpPosition(this Transform a, Transform b, float l)
        {
            if (a == null || b == null) return;
            a.position = Vector3.Lerp(a.position, b.position, l);
        }
        /// <summary>
        /// Transform A will lerp to transform B's rotation
        /// </summary>
        /// <param name="a"></param>
        /// <param name="b"></param>
        /// <param name="l"></param>
        public static void TransformLerpRotation(this Transform a, Transform b, float l)
        {
            if (a == null || b == null) return;
            a.rotation = Quaternion.Lerp(a.rotation, b.rotation, l);
        }
        /// <summary>
        /// Transform A will lerp to transform B's position
        /// </summary>
        /// <param name="a"></param>
        /// <param name="b"></param>
        /// <param name="l"></param>
        public static void VectorLerpPosition(this Transform a, Vector3 b, float l)
        {
            if (a == null) return;
            a.position = Vector3.Lerp(a.position, b, l);
        }
        /// <summary>
        /// Transform A will lerp to transform B's local position
        /// </summary>
        /// <param name="a"></param>
        /// <param name="b"></param>
        /// <param name="l"></param>
        public static void VectorLerpLocalPosition(this Transform a, Vector3 b, float l)
        {
            if (a == null) return;
            a.localPosition = Vector3.Lerp(a.localPosition, b, l);
        }
        /// <summary>
        /// Sets transform A's position and rotation to transform B's position and rotation
        /// </summary>
        /// <param name="a"></param>
        /// <param name="b"></param>
        public static void Transforms(this Transform a, Transform b)
        {
            if (a == null || b == null) return;
            Position(a, b);
            Rotation(a, b);
        }
        /// <summary>
        /// Sets transform A's position to transform B's position, but wil lerp their rotations
        /// </summary>
        /// <param name="a"></param>
        /// <param name="b"></param>
        /// <param name="l"></param>
        public static void StableTransforms(this Transform a, Transform b, float l)
        {
            Position(a, b);
            TransformLerpRotation(a, b, l);
        }
        /// <summary>
        /// Sets transform A's position to transform B's position, but wil lerp their rotations
        /// </summary>
        /// <param name="a"></param>
        /// <param name="position"></param>
        /// <param name="look"></param>
        /// <param name="away"></param>
        public static void StableTransformLook(this Transform a, Transform position, Transform look, bool away)
        {
            Position(a, position);
            if (!away)
            {
                a.LookAt(look, a.up);
            }
            else
            {
                a.LookAwayFrom(look, a.up);
            }
        }
        /// <summary>
        /// Sets transform A's position to transform B's position, but wil lerp their rotations
        /// </summary>
        /// <param name="a"></param>
        /// <param name="position"></param>
        /// <param name="look"></param>
        /// <param name="away"></param>
        public static void StablePositionLook(this Transform a, Vector3 position, Transform look, bool away)
        {
            a.transform.position = position;
            if (!away)
            {
                a.LookAt(look, a.up);
            }
            else
            {
                a.LookAwayFrom(look, a.up);
            }
        }
        /// <summary>
        /// Lerps transform A's position and rotation to transform B's position and rotation
        /// </summary>
        /// <param name="a"></param>
        /// <param name="b"></param>
        /// <param name="l"></param>
        public static void LerpTransform(this Transform a, Transform b, float l)
        {
            if (a == null || b == null) return;
            TransformLerpPosition(a, b, l);
            TransformLerpRotation(a, b, l);
        }
        /// <summary>
        /// Returns distance to the midpoint of transform A and B
        /// </summary>
        /// <param name="a"></param>
        /// <param name="b"></param>
        /// <returns></returns>
        public static float Midpoint(this Transform a, Transform b)
        {
            if (a == null || b == null) return 0f;
            return Vector3.Distance(a.position, b.position) *.5f;
        }

        public static void ForwardVector(this Transform a, Transform b)
        {
            if (a == null || b == null) return;

            a.forward = b.forward;
        }

        public static void MidpointPosition(this Transform target, Transform a, Transform b, bool lookAt)
        {
            if (a == null || b == null) return;
            Vector3 posA = a.position;
            Vector3 posB = b.position;

            target.position = Vector3.Lerp(posA, posB, .5f);
            
            if (!lookAt) return;
            
            target.LookAt(b);
        }
        
        public static Vector3 MidpointPosition(Transform a, Transform b)
        {
            Vector3 posA = a.position;
            Vector3 posB = b.position;
            return Vector3.Lerp(posA, posB, .5f);
        }
        
        public static void ThreePointMidpointPosition(this Transform target, Transform a, Transform b, Transform c)
        {
            Vector3 posA = a.position;
            Vector3 posB = b.position;
            Vector3 posC = c.position;
            Vector3 aC = Vector3.Lerp(posA, posC, .5f);
            Vector3 bC = Vector3.Lerp(posB, posC, .5f);
            Vector3 midpoint = Vector3.Lerp(aC, bC, .5f);
            target.position = midpoint;
        }
        
        public static Vector3 ThreePointMidpointPosition(Transform a, Transform b, Transform c)
        {
            Vector3 posA = a.position;
            Vector3 posB = b.position;
            Vector3 posC = c.position;
            Vector3 aC = Vector3.Lerp(posA, posC, .5f);
            Vector3 bC = Vector3.Lerp(posB, posC, .5f);
            return Vector3.Lerp(aC, bC, .5f);
        }
        
        public static Vector3 FivePointMidpointPosition(Transform a, Transform b, Transform c, Transform d, Transform e)
        {
            Vector3 posA = a.position;
            Vector3 posB = b.position;
            Vector3 posC = c.position;
            Vector3 posD = d.position;
            Vector3 posE = e.position;
            Vector3 aE = Vector3.Lerp(posA, posE, .5f);
            Vector3 bE = Vector3.Lerp(posB, posE, .5f);
            Vector3 cE = Vector3.Lerp(posC, posE, .5f);
            Vector3 dE = Vector3.Lerp(posD, posE, .5f);
            Vector3 aEbE = Vector3.Lerp(aE, bE, .5f);
            Vector3 cEdE = Vector3.Lerp(cE, dE, .5f);
            return Vector3.Lerp(aEbE, cEdE, .5f);
        }

        public static void AddForcePosition(this Rigidbody rb, Transform a, Transform b, bool debug)
        {
            if (a == null || b == null) return;
            
            Vector3 aPos = a.position;
            Vector3 bPos = b.position;
            Vector3 x = bPos - aPos;
            
            float d = Vector3.Distance(aPos, bPos);
            
            float p = Mathf.Pow(d, 1f);
            float y = p;
            
            if (debug)
            {
                Debug.DrawRay(aPos, -x * y, Color.cyan);   
                Debug.DrawRay(aPos, x * p, Color.yellow);
            }
            
            rb.AddForce(x, ForceMode.Force);
            
            if (!(d < 1f)) return;
            
            rb.AddForce(-x * d, ForceMode.Force);
        }
        public static Vector3 Velocity(this List<Vector3> list)
        {
            return (list[list.Count - 1] - list[0]) / Time.deltaTime;
        }
        
        public static Vector3 AngularVelocity(this List<Vector3> list)
        {
            return Vector3.Cross(list[list.Count - 1], list[0]);
        }
        
        public static void RigidBody(this Rigidbody rb, float force, float drag, bool stop, bool gravity)
        {
            rb.mass = force;
            rb.drag = drag;
            rb.angularDrag = drag;
            rb.velocity = stop? Vector3.zero : rb.velocity;
            rb.useGravity = gravity;
        }

        public static void ReactiveMaterial(this Renderer r,  Transform leftHand, Transform rightHand)
        {
            Material material = r.material;
            material.SetVector(LeftHand, leftHand.position);
            material.SetVector(RightHand, rightHand.position);
        }

        public static void LineRenderWidth(this LineRenderer lr, float start, float end)
        {
            lr.startWidth = start;
            lr.endWidth = end;
        }

        public static Vector3 LocalScale(this Vector3 originalScale, float factor)
        {
            return new Vector3(
                originalScale.x + originalScale.x * factor,
                originalScale.y + originalScale.y * factor,
                originalScale.z + originalScale.z * factor);
        }

        public static Vector3 LocalPosition(this Vector3 originalPos, float factor)
        {
            return new Vector3(
                originalPos.x,
                originalPos.y,
                originalPos.z + factor);
        }

        public static void LocalDepth(this Transform a, float z, bool lerp, float speed)
        {
            if (a == null) return;
            Vector3 p = a.localPosition;

            switch (lerp)
            {
                case false:
                    a.localPosition = new Vector3(p.x,p.y, z);
                    break;
                case true:
                    Vector3.Lerp(a.localPosition, new Vector3(p.x, p.y, z), speed);
                    break;
                default:
                    throw new ArgumentException();
            }   
        }

        public static void VisualState(this Transform t, SelectableObject s, Vector3 scale, Vector3 pos, TMP_FontAsset font, Color color)
        {
            if (s.buttonText != null)
            {
                s.buttonText.font = font;
            }

            if (s.buttonBack != null)
            {
                s.buttonBack.material.color = color;
            }
        }

        public static Vector3 Offset(this Transform a, Transform b)
        {
            Vector3 x = a.position;
            Vector3 y = b.position;
            Vector3 xN = new Vector3(x.x, 0, x.z);
            Vector3 yN = new Vector3(y.x, 0, y.z);
            
            return yN - xN;
        }
        
        public static float Divergence(this Transform a, Transform b)
        {           
            return Vector3.Angle(a.forward, b.forward);
        }
        
        public static float MagnifiedDepth(this GameObject conP, GameObject conO, GameObject objO, GameObject objP, float snapDistance, float max, bool limit)
        {
            float depth = conP.transform.localPosition.z / conO.transform.localPosition.z;
            float distance = Vector3.Distance(objO.transform.position, objP.transform.position);
				
            if (distance >= max && limit) return max;
            if (distance < snapDistance) return objO.transform.localPosition.z * Mathf.Pow(depth, 2);														
            return objO.transform.localPosition.z * Mathf.Pow(depth, 2.5f);
        }

        public static void Outline(this Outline outline, Outline.Mode mode, float width, Color color)
        {
            outline.OutlineColor = color;
            outline.OutlineWidth = width;
            outline.OutlineMode = mode;
        }

        public static Vector3 ScaledScale(this Vector3 initialScale, float factor)
        {
            return new Vector3(
                initialScale.x * factor,
                initialScale.y * factor,
                initialScale.z * factor);
        }
        
        public static void LookAwayFrom(this Transform thisTransform, Transform transform, Vector3 upwards) 
        {
            thisTransform.rotation = Quaternion.LookRotation(thisTransform.position - transform.position, upwards);
        }

        public static void ReverseNormals(this MeshFilter filter)
        {
            Mesh mesh = filter.mesh;
            Vector3[] normals = mesh.normals;
            
            for (int i = 0; i < normals.Length; i++)
                normals[i] = -normals[i];
            mesh.normals = normals;

            for (int m=0;m<mesh.subMeshCount;m++)
            {
                int[] triangles = mesh.GetTriangles(m);
                
                for (int i=0;i<triangles.Length;i+=3)
                {
                    int temp = triangles[i + 0];
                    triangles[i + 0] = triangles[i + 1];
                    triangles[i + 1] = temp;
                }
                
                mesh.SetTriangles(triangles, m);
            }
        }
        public static Vector3 LastValidPosition(this GameObject target, Vector3 lastValidPosition)
        {
            Transform t = target.transform;
            Vector3 position = t.position;
            Vector3 up = t.up;
            lastValidPosition = Physics.Raycast(position, -up, out RaycastHit hit) ? hit.point : lastValidPosition;
            return lastValidPosition;
        }

        public static void LockAxis(this Transform transform, Transform target, Axis axis)
        {
            Vector3 targetLocalPosition = target.localPosition;
            switch (axis)
            {
                case Axis.X:
                    transform.localPosition = new Vector3(targetLocalPosition.x, 0, 0);
                    break;
                case Axis.Y:
                    transform.localPosition = new Vector3(0, targetLocalPosition.y, 0);
                    break;
                case Axis.Z:
                    transform.localPosition = new Vector3(0, 0, targetLocalPosition.z);
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(axis), axis, null);
            }
        }
        /*
        public static void HandPosition(bool armDisabled, GameObject elbow, Arm arm, GameObject transformPosition, Vector3 position, GameObject lookAt, Transform wrist)
        {
            if (armDisabled) return;
            elbow.transform.position = arm.ElbowPosition.ToVector3();
            lookAt.transform.MidpointPosition(elbow.transform, wrist.transform, true);
            transformPosition.transform.StablePositionLook(position, lookAt.transform, true);
            Debug.DrawLine(elbow.transform.position, wrist.transform.position, Color.yellow);
            Debug.DrawLine(transformPosition.transform.position, lookAt.transform.position, Color.white);
        }
        */
        /// <summary>
        /// Returns a GameObject, sets its parent, and names it
        /// </summary>
        /// <param name="parent"></param>
        /// <param name="name"></param>
        /// <returns></returns>
        public static GameObject NewGameObject(GameObject parent, string name)
        {
            GameObject gameObject = new GameObject();
            gameObject.transform.parent = parent.transform;
            gameObject.name = name;
            return gameObject;
        }
    }
}
