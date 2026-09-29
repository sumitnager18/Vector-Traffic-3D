using System.Collections;
using UnityEngine;
using VectorTraffic3D.Core;
using VectorTraffic3D.Vehicles;
namespace VectorTraffic3D.Runtime
{
    public sealed class VehicleView : MonoBehaviour
    {
        public int VehicleId { get; private set; }
        private GridWorldMapper _mapper;
        private float _height;
        public static VehicleView Create(Transform parent, VehicleState state, GridWorldMapper mapper)
        {
            var go = new GameObject($"Vehicle_{state.Id}_{state.Type}");
            go.transform.SetParent(parent, false);
            var view = go.AddComponent<VehicleView>();
            view.VehicleId = state.Id;
            view._mapper = mapper;
            view._height = .35f;
            view.BuildVisual(state);
            return view;
        }
        private void BuildVisual(VehicleState state)
        {
            var root = new GameObject("Body").transform;
            root.SetParent(transform, false);
            float length = state.Length * _mapper.CellSize * .82f;
            var body = GameObject.CreatePrimitive(PrimitiveType.Cube);
            body.name = "BodyMesh";
            body.transform.SetParent(root, false);
            body.transform.localScale = new Vector3(.82f * _mapper.CellSize, .45f, length);
            body.transform.localPosition = new Vector3(0f, _height, 0f);
            body.transform.localRotation = Quaternion.Euler(0f, WorldYaw(state.Orientation), 0f);
            body.GetComponent<Renderer>().material = MakeMaterial(ColorFor(state.Type));
            var collider = GetComponent<BoxCollider>() ?? gameObject.AddComponent<BoxCollider>();
            collider.center = new Vector3(0f, _height, 0f);
            collider.size = new Vector3(_mapper.CellSize, .7f, length);
        }
        public void SyncImmediate(VehicleState state)
        {
            transform.position = _mapper.ToWorld(state.HeadPosition, 0f);
            transform.rotation = Quaternion.Euler(0f, WorldYaw(state.Orientation), 0f);
        }
        public IEnumerator AnimateTo(VehicleState state, float duration = .18f)
        {
            Vector3 start = transform.position;
            Vector3 end = _mapper.ToWorld(state.HeadPosition, 0f);
            Quaternion startRot = transform.rotation;
            Quaternion endRot = Quaternion.Euler(0f, WorldYaw(state.Orientation), 0f);
            float t = 0f;
            while (t < 1f)
            {
                t += Time.deltaTime / Mathf.Max(.01f, duration);
                float eased = t * t * (3f - 2f * t);
                transform.position = Vector3.Lerp(start, end, eased);
                transform.rotation = Quaternion.Slerp(startRot, endRot, eased);
                yield return null;
            }
            transform.position = end;
            transform.rotation = endRot;
        }
        private static float WorldYaw(Direction d) => d switch
        {
            Direction.Up => 0f, Direction.Right => 90f,
            Direction.Down => 180f, Direction.Left => 270f, _ => 0f
        };
        private static Color ColorFor(VehicleType t) => t switch
        {
            VehicleType.Taxi => new Color(.95f,.75f,.15f),
            VehicleType.Bus => new Color(.15f,.45f,.9f),
            VehicleType.Van => new Color(.7f,.25f,.8f),
            VehicleType.SUV => new Color(.15f,.7f,.45f),
            VehicleType.DeliveryTruck => new Color(.9f,.35f,.2f),
            _ => new Color(.25f,.65f,.95f)
        };
        private static Material MakeMaterial(Color color)
        {
            var m = new Material(Shader.Find("Universal Render Pipeline/Lit"));
            m.color = color;
            return m;
        }
    }
}
