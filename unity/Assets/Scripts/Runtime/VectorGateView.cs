using UnityEngine;
using VectorTraffic3D.Core;

namespace VectorTraffic3D.Runtime
{
    public sealed class VectorGateView : MonoBehaviour
    {
        public int GateId { get; private set; }
        private Transform _beam;

        public static VectorGateView Create(Transform parent, int gateId, Vector3 position)
        {
            var root = new GameObject($"VectorGate_{gateId}");
            root.transform.SetParent(parent, false);
            root.transform.position = position;
            var view = root.AddComponent<VectorGateView>();
            view.GateId = gateId;
            view._beam = GameObject.CreatePrimitive(PrimitiveType.Cube).transform;
            view._beam.name = "GateBeam";
            view._beam.SetParent(root.transform, false);
            view._beam.localScale = new Vector3(.12f, .18f, 1.15f);
            var renderer = view._beam.GetComponent<Renderer>();
            renderer.material = MakeMaterial(new Color(.8f, .25f, .9f));
            return view;
        }

        public void Refresh(Direction direction)
        {
            transform.rotation = Quaternion.Euler(0f, WorldYaw(direction), 0f);
        }

        private static float WorldYaw(Direction d) => d switch
        {
            Direction.Up => 0f,
            Direction.Right => 90f,
            Direction.Down => 180f,
            Direction.Left => 270f,
            _ => 0f
        };

        private static Material MakeMaterial(Color color)
        {
            var material = new Material(Shader.Find("Universal Render Pipeline/Lit"));
            material.color = color;
            return material;
        }
    }
}
