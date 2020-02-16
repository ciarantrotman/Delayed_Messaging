using Leap.Unity.Interaction;
using UnityEngine;
using UnityEngine.Rendering;
using VR_Prototyping.Plugins.QuickOutline.Scripts;

namespace VR_Prototyping.Scripts
{
    public static class Setup
    {
        public static LineRenderer AddOrGetLineRenderer(this Transform a)
        {
            return !a.GetComponent<LineRenderer>() ? a.gameObject.AddComponent<LineRenderer>() : a.gameObject.GetComponent<LineRenderer>();
        }
        public static MeshRenderer AddOrGetMeshRenderer(this Transform a)
        {
            return !a.GetComponent<MeshRenderer>() ? a.gameObject.AddComponent<MeshRenderer>() : a.gameObject.GetComponent<MeshRenderer>();
        }
        public static MeshRenderer GetMeshRenderer(this Transform a)
        {
            return !a.GetComponent<MeshRenderer>() ? null : a.gameObject.GetComponent<MeshRenderer>();
        }
        public static void SetupLineRender(this LineRenderer lr, Material material, float width, bool startEnabled)
        {
            lr.material = material;
            lr.shadowCastingMode = ShadowCastingMode.Off;
            lr.receiveShadows = false;
            lr.startWidth = width;
            lr.endWidth = width;
            lr.numCapVertices = 32;
            lr.useWorldSpace = true;
            lr.enabled = startEnabled;
        }
        
        public static void SetupTrailRender(this TrailRenderer tr, Material m, float time, AnimationCurve widthCurve, bool e)
        {
            tr.material = m;
            tr.minVertexDistance = .01f;
            tr.time = time;
            tr.widthCurve = widthCurve;
            tr.numCapVertices = 32;
            tr.enabled = e;
        }

        public static Rigidbody AddOrGetRigidbody(this Transform a)
        {
            return !a.GetComponent<Rigidbody>() ? a.gameObject.AddComponent<Rigidbody>() : a.gameObject.GetComponent<Rigidbody>();
        }
        
        public static Outline AddOrGetOutline(this Transform a)
        {
            return !a.GetComponent<Outline>() ? a.gameObject.AddComponent<Outline>() : a.gameObject.GetComponent<Outline>();
        }
        
        public static InteractionBehaviour AddOrGetInteractionBehavior(this Transform a)
        {
            return !a.GetComponent<InteractionBehaviour>() ? a.gameObject.AddComponent<InteractionBehaviour>() : a.gameObject.GetComponent<InteractionBehaviour>();
        }
        
        public static void SetOffsetPosition(this Transform thisTransform, Transform parent, float offset)
        {
            thisTransform.position = parent.position;
            thisTransform.SetParent(parent);
            thisTransform.localRotation = Quaternion.identity;
            thisTransform.localPosition = new Vector3(0, 0, offset);
        }

        public static void SetupSphereCollider(this SphereCollider sc, bool trigger, float radius)
        {
            sc.isTrigger = trigger;
            sc.radius = radius;
        }
    }
}
