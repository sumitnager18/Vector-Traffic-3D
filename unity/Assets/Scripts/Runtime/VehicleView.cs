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
        private Transform _visualRoot;
        private Vector3 _baseScale = Vector3.one;

        public static VehicleView Create(Transform parent, VehicleState state, GridWorldMapper mapper)
        {
            var go = new GameObject($"Vehicle_{state.Id}_{state.Type}");
            go.transform.SetParent(parent, false);
            var view = go.AddComponent<VehicleView>();
            view.VehicleId = state.Id;
            view._mapper = mapper;
            view.BuildVisual(state);
            return view;
        }

        private void BuildVisual(VehicleState state)
        {
            _visualRoot = new GameObject("Visual").transform;
            _visualRoot.SetParent(transform, false);

            var length = state.Length * _mapper.CellSize * .82f;
            var width = _mapper.CellSize * .68f;
            var height = .42f;

            CreateCube("Body", new Vector3(width, height, length),
                new Vector3(0f, .42f, 0f), ColorFor(state.Type), _visualRoot);

            var cabinLength = Mathf.Max(.45f, length * (state.Type == VehicleType.Bus ? .72f : .48f));
            CreateCube("Cabin", new Vector3(width * .82f, height * .78f, cabinLength),
                new Vector3(0f, .42f + height * .55f, length * .08f),
                new Color(.78f, .84f, .9f), _visualRoot);

            if (state.Type != VehicleType.Bus)
            {
                var glass = CreateCube("Windshield", new Vector3(width * .7f, .12f, cabinLength * .22f),
                    new Vector3(0f, .72f, length * .22f),
                    new Color(.08f, .14f, .2f), _visualRoot);
                glass.transform.localRotation = Quaternion.Euler(18f, 0f, 0f);
            }

            CreateWheel(-width * .48f, length * .30f, _visualRoot);
            CreateWheel(width * .48f, length * .30f, _visualRoot);
            CreateWheel(-width * .48f, -length * .30f, _visualRoot);
            CreateWheel(width * .48f, -length * .30f, _visualRoot);

            if (state.Type == VehicleType.Bus || state.Type == VehicleType.DeliveryTruck)
            {
                CreateWheel(-width * .48f, 0f, _visualRoot);
                CreateWheel(width * .48f, 0f, _visualRoot);
            }

            _baseScale = transform.localScale;
        }

        private void CreateWheel(float x, float z, Transform parent)
        {
            var wheel = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            wheel.name = "Wheel";
            wheel.transform.SetParent(parent, false);
            wheel.transform.localScale = new Vector3(.13f, .10f, .13f);
            wheel.transform.localPosition = new Vector3(x, .25f, z);
            wheel.transform.localRotation = Quaternion.Euler(90f, 0f, 0f);
            wheel.GetComponent<Renderer>().material = MakeMaterial(new Color(.06f, .07f, .08f));
            Object.Destroy(wheel.GetComponent<Collider>());
        }

        private static GameObject CreateCube(
            string name, Vector3 scale, Vector3 position, Color color, Transform parent)
        {
            var cube = GameObject.CreatePrimitive(PrimitiveType.Cube);
            cube.name = name;
            cube.transform.SetParent(parent, false);
            cube.transform.localScale = scale;
            cube.transform.localPosition = position;
            cube.GetComponent<Renderer>().material = MakeMaterial(color);
            Object.Destroy(cube.GetComponent<Collider>());
            return cube;
        }

        public void SyncImmediate(VehicleState state)
        {
            transform.position = _mapper.ToWorld(state.HeadPosition, .02f);
            transform.rotation = Quaternion.Euler(0f, WorldYaw(state.Orientation), 0f);
        }

        public IEnumerator AnimateTo(VehicleState state, float duration = .18f)
        {
            var start = transform.position;
            var end = _mapper.ToWorld(state.HeadPosition, .02f);
            var startRot = transform.rotation;
            var endRot = Quaternion.Euler(0f, WorldYaw(state.Orientation), 0f);
            var t = 0f;

            while (t < 1f)
            {
                t += Time.deltaTime / Mathf.Max(.01f, duration);
                var eased = t * t * (3f - 2f * t);
                transform.position = Vector3.Lerp(start, end, eased);
                transform.rotation = Quaternion.Slerp(startRot, endRot, eased);
                yield return null;
            }

            transform.position = end;
            transform.rotation = endRot;
        }

        public void SetSelected(bool selected)
        {
            transform.localScale = selected ? _baseScale * 1.06f : _baseScale;
        }

        private static float WorldYaw(Direction d) => d switch
        {
            Direction.Up => 0f,
            Direction.Right => 90f,
            Direction.Down => 180f,
            Direction.Left => 270f,
            _ => 0f
        };

        private static Color ColorFor(VehicleType t) => t switch
        {
            VehicleType.Taxi => new Color(.95f, .75f, .15f),
            VehicleType.Bus => new Color(.15f, .45f, .9f),
            VehicleType.Van => new Color(.7f, .25f, .8f),
            VehicleType.SUV => new Color(.15f, .7f, .45f),
            VehicleType.DeliveryTruck => new Color(.9f, .35f, .2f),
            VehicleType.Pickup => new Color(.75f, .45f, .2f),
            _ => new Color(.25f, .65f, .95f)
        };

        private static Material MakeMaterial(Color color)
        {
            var material = new Material(Shader.Find("Universal Render Pipeline/Lit"));
            material.color = color;
            return material;
        }
    }
}
