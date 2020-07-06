using System;
using System.Collections.Generic;
using UnityEngine;
using VR_Prototyping.Plugins.QuickOutline.Scripts;
//using Leap;
//using Leap.Unity;

namespace Delayed_Messaging.Scripts.Utilities
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
        /// Sets a transform to (0,0,0) and (0,0,0,0)
        /// </summary>
        /// <param name="transform"></param>
        public static void DefaultTransform(this Transform transform)
        {
            transform.localPosition = Vector3.zero;
            transform.localRotation = Quaternion.identity;
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
        /// <summary>
        /// Returns distance to the midpoint of Vector3s A and B
        /// </summary>
        /// <param name="a"></param>
        /// <param name="b"></param>
        /// <returns></returns>
        public static float Midpoint(this Vector3 a, Vector3 b)
        {
            return Vector3.Distance(a, b) *.5f;
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
        
        public static Vector3 MidpointPosition(Vector3 a, Vector3 b)
        {
            Vector3 posA = a;
            Vector3 posB = b;
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

        public static void SetOutline(this Outline outline, Outline.Mode mode, float width, Color color, bool state)
        {
            outline.enabled = state;
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
        /// <summary>
        /// Will either add or remove the object from the list provided
        /// </summary>
        /// <param name="gameObject"></param>
        /// <param name="list"></param>
        /// <param name="add"></param>
        public static void ToggleList(this GameObject gameObject, ICollection<GameObject> list, bool add)
        {
            switch (add)
            {
                case true when !list.Contains(gameObject):
                    //Debug.Log(gameObject.name + " <b>added to</b> " + list);
                    list.Add(gameObject);
                    return;
                case false when list.Contains(gameObject):
                    //Debug.Log(gameObject.name + " <b>removed from</b> " + list);
                    list.Remove(gameObject);
                    return;
                default:
                    return;
            }
        }
        /// <summary>
        /// A static version of the distance - angle casting method, where the users hand angle controls the distance of the visual object
        /// </summary>
        /// <param name="target"></param>
        /// <param name="follow"></param>
        /// <param name="proxy"></param>
        /// <param name="normalised"></param>
        /// <param name="hitPoint"></param>
        /// <param name="midPoint"></param>
        /// <param name="rotation"></param>
        /// <param name="visual"></param>
        /// <param name="controllerTransform"></param>
        /// <param name="cameraTransform"></param>
        /// <param name="joystick"></param>
        /// <param name="maxAngle"></param>
        /// <param name="minAngle"></param>
        /// <param name="minDistance"></param>
        /// <param name="maxDistance"></param>
        /// <param name="lastValidPosition"></param>
        public static void DistanceCast(GameObject target, GameObject follow, GameObject proxy, GameObject normalised, GameObject hitPoint, GameObject midPoint, GameObject rotation, GameObject visual, Transform controllerTransform, Transform cameraTransform, Vector2 joystick, float maxAngle, float minAngle, float minDistance, float maxDistance, Vector3 lastValidPosition)
        {
            // set the positions of the local objects and calculate the depth based on the angle of the controller
            target.transform.LocalDepth(follow.ControllerAngle(proxy,normalised,controllerTransform,cameraTransform,true).CalculateDepth(maxAngle, minAngle, maxDistance, minDistance, proxy.transform), false, .2f);
            target.TargetLocation(hitPoint, lastValidPosition, 15);
            midPoint.transform.LocalDepth(proxy.transform.Midpoint(target.transform), false, 0f);
            visual.Target(hitPoint, normalised.transform, joystick, rotation);
        }
        /// <summary>
        /// Creates a bounds using the MeshRenderers of the provided transform
        /// </summary>
        /// <param name="parentObject"></param>
        /// <param name="bounds"></param>
        /// <returns></returns>
        public static Bounds BoundsOfChildren(this Transform parentObject, Bounds bounds)
        {
            bounds = new Bounds (parentObject.position, Vector3.zero);
            MeshRenderer[] renderers = parentObject.GetComponentsInChildren<MeshRenderer>();
            foreach (MeshRenderer renderer in renderers)
            {
                bounds.Encapsulate(renderer.bounds);
            }
            return bounds;
        }

        /// <summary>
        /// Finds the largest cardinal dimension of the model and scales the supplied GameObject accordingly
        /// </summary>
        /// <param name="model"></param>
        /// <param name="boundsMin"></param>
        /// <returns></returns>
        public static void ScaleFactor(this GameObject model, float boundsMin = .35f)
        {
            model.transform.position = Vector3.zero;
            model.transform.eulerAngles = Vector3.zero;
            model.transform.localScale = Vector3.one;
            
            // Find the bounds of the model
            Bounds modelBounds = new Bounds();
            Bounds frameBounds = new Bounds();
            
            modelBounds = model.transform.BoundsOfChildren(modelBounds);
            frameBounds.size = Vector3.one * boundsMin;

            float boundsMax = modelBounds.size.LargestValue();
            float scaleMax = model.transform.localScale.LargestValue();

            float scaleFactor = Mathf.InverseLerp(0f, boundsMax, boundsMin);
            
            Vector3 boundsModelSize = modelBounds.size;
            Vector3 frameModelSize = frameBounds.size;

            // Vector3 scale = new Vector3(boundsModelSize.x / frameModelSize.x, boundsModelSize.y / frameModelSize.y, boundsModelSize.z / frameModelSize.z);
            
            float scaleRatio = scaleMax / boundsMax;
            
            // Scale base on the frame bounds
            float boundsRatio = boundsMin / boundsMax;
            
            // Adjust the original scale to create the new scale factor
            float adjustedBounds = boundsMax * boundsRatio;

            /*Debug.Log(model.name + 
                      " : Bounds Ratio: " + boundsRatio + " = " + boundsMin + " / " + boundsMax + 
                      " | Adjusted Bounds: " + adjustedBounds + " = " + boundsMax + " * " + boundsRatio + 
                      " | Scale Ratio: " + scaleRatio + " = " + scaleMax + " / " + boundsMax);*/

            //model.transform.localScale = scale;
            //model.transform.localScale = Vector3.one * scaleFactor;
            model.transform.localScale = Vector3.one * .1f;
            //model.transform.localScale = new Vector3(adjustedBounds, adjustedBounds, adjustedBounds);
        }
        
        public static void SetSpringJointValues(this SpringJoint springJoint, Rigidbody connectedRigidBody = null, bool auto = false, float spring = 10f, float damper = .2f, float minDistance = 0f, float maxDistance = 0f, float tolerance = .05f)
        {
            // Set these values
            springJoint.spring = spring;
            springJoint.damper = damper;
            springJoint.maxDistance = maxDistance;
            springJoint.minDistance = minDistance;
            springJoint.tolerance = tolerance;
            springJoint.autoConfigureConnectedAnchor = auto;
            
            // Adjust connected body
            springJoint.connectedBody = connectedRigidBody;
        }

        public static void SetSpringJointAnchor(this SpringJoint springJoint, Vector3 anchor)
        {
            springJoint.connectedAnchor = anchor;
        }
    }
}
