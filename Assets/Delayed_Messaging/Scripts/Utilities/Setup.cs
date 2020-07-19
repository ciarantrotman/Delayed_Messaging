using Pathfinding;
using UnityEngine;
using UnityEngine.Rendering;
using VR_Prototyping.Plugins.QuickOutline.Scripts;

namespace Delayed_Messaging.Scripts.Utilities
{
    public static class Setup
    {
        /// <summary>
        /// Setup a Line renderer
        /// </summary>
        /// <param name="lr"></param>
        /// <param name="material"></param>
        /// <param name="width"></param>
        /// <param name="startEnabled"></param>
        public static void SetupLineRender(this LineRenderer lr, Material material, float width, bool startEnabled)
        {
            lr.material = material;
            lr.shadowCastingMode = ShadowCastingMode.Off;
            lr.receiveShadows = false;
            lr.startWidth = width;
            lr.endWidth = width;
            lr.numCapVertices = 32;
            lr.numCornerVertices = 32;
            lr.useWorldSpace = true;
            lr.enabled = startEnabled;
        }
        public static LineRenderer LineRender(this GameObject parent, Material material, float startWidth, float endWidth, bool startEnabled, bool worldSpace)
        {
            LineRenderer lr = parent.AddComponent<LineRenderer>();
            lr.material = material;
            lr.shadowCastingMode = ShadowCastingMode.Off;
            lr.receiveShadows = false;
            lr.startWidth = startWidth;
            lr.endWidth = endWidth;
            lr.numCapVertices = 32;
            lr.numCornerVertices = 32;
            lr.useWorldSpace = worldSpace;
            lr.enabled = startEnabled;
            return lr;
        }
        /// <summary>
        /// Sets up a trail renderer component
        /// </summary>
        /// <param name="tr"></param>
        /// <param name="m"></param>
        /// <param name="time"></param>
        /// <param name="widthCurve"></param>
        /// <param name="e"></param>
        public static void SetupTrailRender(this TrailRenderer tr, Material m, float time, AnimationCurve widthCurve, bool e)
        {
            tr.material = m;
            tr.minVertexDistance = .01f;
            tr.time = time;
            tr.widthCurve = widthCurve;
            tr.numCapVertices = 32;
            tr.enabled = e;
        }
        /// <summary>
        /// Add a Rigidbody
        /// </summary>
        /// <param name="a"></param>
        /// <returns></returns>
        public static Rigidbody AddOrGetRigidbody(this Transform a)
        {
            return !a.GetComponent<Rigidbody>() ? a.gameObject.AddComponent<Rigidbody>() : a.gameObject.GetComponent<Rigidbody>();
        }
        /// <summary>
        /// Add the Outline Componant
        /// </summary>
        /// <param name="a"></param>
        /// <returns></returns>
        public static Outline AddOrGetOutline(this Transform a)
        {
            return !a.GetComponent<Outline>() ? a.gameObject.AddComponent<Outline>() : a.gameObject.GetComponent<Outline>();
        }
        /// <summary>
        /// Add the AI Destination Setter Component
        /// </summary>
        /// <param name="a"></param>
        /// <returns></returns>
        public static AIDestinationSetter AddOrGetAIDestinationSetter(this Transform a)
        {
            return !a.GetComponent<AIDestinationSetter>() ? a.gameObject.AddComponent<AIDestinationSetter>() : a.gameObject.GetComponent<AIDestinationSetter>();
        }
        /// <summary>
        /// Add the AI Path component
        /// </summary>
        /// <param name="a"></param>
        /// <returns></returns>
        public static AIPath AddOrGetAIPath(this Transform a)
        {
            return !a.GetComponent<AIPath>() ? a.gameObject.AddComponent<AIPath>() : a.gameObject.GetComponent<AIPath>();
        }
        /// <summary>
        /// Add a line renderer
        /// </summary>
        /// <param name="a"></param>
        /// <returns></returns>
        public static LineRenderer AddOrGetLineRenderer(this Transform a)
        {
            return !a.GetComponent<LineRenderer>() ? a.gameObject.AddComponent<LineRenderer>() : a.gameObject.GetComponent<LineRenderer>();
        }
        /// <summary>
        /// Add a mesh renderer component
        /// </summary>
        /// <param name="a"></param>
        /// <returns></returns>
        public static MeshRenderer AddOrGetMeshRenderer(this Transform a)
        {
            return !a.GetComponent<MeshRenderer>() ? a.gameObject.AddComponent<MeshRenderer>() : a.gameObject.GetComponent<MeshRenderer>();
        }
        /// <summary>
        /// Checks if a mesh renderer exists on an object
        /// </summary>
        /// <param name="a"></param>
        /// <returns></returns>
        public static MeshRenderer GetMeshRenderer(this Transform a)
        {
            return !a.GetComponent<MeshRenderer>() ? null : a.gameObject.GetComponent<MeshRenderer>();
        }
        /// <summary>
        /// Sets an offset position at runtime
        /// </summary>
        /// <param name="thisTransform"></param>
        /// <param name="parent"></param>
        /// <param name="offset"></param>
        public static void SetOffsetPosition(this Transform thisTransform, Transform parent, float offset)
        {
            thisTransform.position = parent.position;
            thisTransform.SetParent(parent);
            thisTransform.localRotation = Quaternion.identity;
            thisTransform.localPosition = new Vector3(0, 0, offset);
        }
        /// <summary>
        /// Setup a sphere collider
        /// </summary>
        /// <param name="sc"></param>
        /// <param name="trigger"></param>
        /// <param name="radius"></param>
        public static void SetupSphereCollider(this SphereCollider sc, bool trigger, float radius)
        {
            sc.isTrigger = trigger;
            sc.radius = radius;
        }
    }
}
