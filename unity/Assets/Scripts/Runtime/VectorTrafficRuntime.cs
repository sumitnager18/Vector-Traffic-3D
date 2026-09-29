using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using VectorTraffic3D.Board;
using VectorTraffic3D.Core;
using VectorTraffic3D.Puzzle;

namespace VectorTraffic3D.Runtime
{
    public sealed class VectorTrafficRuntime : MonoBehaviour
    {
        [Header("Board")]
        [SerializeField] private int width = 7;
        [SerializeField] private int height = 7;
        [SerializeField] private float cellSize = 1.5f;
        [SerializeField] private int seed = 20260929;

        [Header("Movement")]
        [SerializeField] private float moveDuration = .18f;
        [SerializeField] private float exitDuration = .12f;

        public BoardState Board { get; private set; }

        private GridWorldMapper _mapper;
        private readonly Dictionary<int, VehicleView> _views = new();
        private readonly Dictionary<int, VectorGateView> _gateViews = new();
        private Camera _camera;
        private RuntimeInputController _input;
        private RuntimeCameraController _cameraController;
        private int _selectedVehicle = -1;
        private bool _busy;
        private Transform _worldRoot;
        private Transform _vehiclesRoot;
        private Transform _focus;

        private void Start()
        {
            BuildLevel();
            BuildWorld();
            BuildInput();
        }

        private void BuildLevel()
        {
            Board = RuntimeLevelFactory.Create(seed, width, height);

            if (!Board.TryValidateStaticState(out var error))
                Debug.LogError($"Runtime level rejected: {error}");
        }

        private void BuildWorld()
        {
            _worldRoot = new GameObject("World").transform;
            _mapper = new GridWorldMapper(
                cellSize,
                new Vector3(-(width - 1) * cellSize * .5f, 0f,
                    -(height - 1) * cellSize * .5f));

            CreateGround();
            CreateCamera();

            var visualBuilder = new BoardVisualBuilder(_mapper, _worldRoot);
            visualBuilder.Build(Board);

            _vehiclesRoot = new GameObject("Vehicles").transform;
            _vehiclesRoot.SetParent(_worldRoot, false);

            foreach (var state in Board.Vehicles.Values)
            {
                var view = VehicleView.Create(_vehiclesRoot, state, _mapper);
                view.SyncImmediate(state);
                _views[state.Id] = view;
            }

            foreach (var gate in Board.Gates.Values)
            {
                var gateView = _worldRoot.GetComponentInChildren<VectorGateView>();
                if (gateView != null && gateView.GateId == gate.Id)
                    _gateViews[gate.Id] = gateView;
            }

            _focus = new GameObject("CameraFocus").transform;
            _focus.SetParent(_worldRoot, false);
            _focus.position = Vector3.zero;
            _cameraController.Initialize(_camera, _focus);
        }

        private void BuildInput()
        {
            _input = gameObject.GetComponent<RuntimeInputController>();
            if (_input == null) _input = gameObject.AddComponent<RuntimeInputController>();
            _input.PointerReleased += OnPointerReleased;
        }

        private void OnDestroy()
        {
            if (_input != null)
                _input.PointerReleased -= OnPointerReleased;
        }

        private void OnPointerReleased(Vector2 start, Vector2 end)
        {
            if (_busy || _camera == null) return;

            var view = RaycastVehicle(end);
            if (view != null)
            {
                SelectVehicle(view.VehicleId);
                var delta = end - start;
                var direction = delta.magnitude < 30f
                    ? Board.Vehicles[view.VehicleId].CurrentVector
                    : ScreenDeltaToDirection(delta);
                StartCoroutine(AttemptMove(view.VehicleId, direction));
                return;
            }

            var gateView = RaycastGate(end);
            if (gateView != null && Board.Gates.TryGetValue(gateView.GateId, out var gate))
            {
                gate.CycleDirection();
                gateView.Refresh(gate.AllowedDirection);
            }
        }

        private void SelectVehicle(int vehicleId)
        {
            if (_selectedVehicle == vehicleId) return;
            if (_selectedVehicle >= 0 && _views.TryGetValue(_selectedVehicle, out var previous))
                previous.SetSelected(false);
            _selectedVehicle = vehicleId;
            if (_views.TryGetValue(vehicleId, out var current))
                current.SetSelected(true);
        }

        private VehicleView RaycastVehicle(Vector2 screen)
        {
            var ray = _camera.ScreenPointToRay(screen);
            if (!Physics.Raycast(ray, out var hit, 500f)) return null;
            return hit.collider.GetComponentInParent<VehicleView>();
        }

        private VectorGateView RaycastGate(Vector2 screen)
        {
            var ray = _camera.ScreenPointToRay(screen);
            if (!Physics.Raycast(ray, out var hit, 500f)) return null;
            return hit.collider.GetComponentInParent<VectorGateView>();
        }

        private IEnumerator AttemptMove(int vehicleId, Direction direction)
        {
            if (!Board.Vehicles.TryGetValue(vehicleId, out var vehicle)) yield break;
            if (vehicle.IsExited) yield break;

            if (!MoveValidator.TryExecuteStep(Board, vehicleId, direction, out _, out var result))
            {
                Debug.Log($"Move blocked: {result}");
                yield break;
            }

            _busy = true;
            yield return _views[vehicleId].AnimateTo(vehicle, moveDuration);
            _busy = false;

            if (Board.Exits.ContainsKey(vehicle.HeadPosition))
                yield return AttemptExit(vehicleId);
        }

        private IEnumerator AttemptExit(int vehicleId)
        {
            if (!Board.Vehicles.TryGetValue(vehicleId, out var vehicle)) yield break;
            if (!MoveValidator.CanCompleteExit(
                    Board, vehicleId, out var footprints, out var failure))
            {
                Debug.Log($"Exit blocked: {failure}");
                yield break;
            }

            _busy = true;
            foreach (var footprint in footprints)
            {
                if (footprint.Length == 0) continue;
                vehicle.HeadPosition = footprint[0];
                yield return _views[vehicleId].AnimateTo(vehicle, exitDuration);
            }

            vehicle.IsExited = true;
            _views[vehicleId].gameObject.SetActive(false);
            _busy = false;

            if (Board.IsSolved())
                Debug.Log("VECTOR TRAFFIC 3D: puzzle solved.");
        }

        private Direction ScreenDeltaToDirection(Vector2 delta)
        {
            if (Mathf.Abs(delta.x) > Mathf.Abs(delta.y))
                return delta.x > 0 ? Direction.Right : Direction.Left;
            return delta.y > 0 ? Direction.Up : Direction.Down;
        }

        private void CreateGround()
        {
            var go = GameObject.CreatePrimitive(PrimitiveType.Cube);
            go.name = "DioramaBase";
            go.transform.SetParent(_worldRoot, false);
            go.transform.position = new Vector3(0f, -.25f, 0f);
            go.transform.localScale = new Vector3(
                width * cellSize + .8f, .5f, height * cellSize + .8f);
            go.GetComponent<Renderer>().material =
                MakeMaterial(new Color(.07f, .08f, .1f));
        }

        private void CreateCamera()
        {
            var cameraObject = new GameObject("Main Camera");
            cameraObject.transform.SetParent(_worldRoot, false);
            _camera = cameraObject.AddComponent<Camera>();
            _camera.tag = "MainCamera";
            _camera.fieldOfView = 45f;
            _camera.nearClipPlane = .05f;
            _camera.farClipPlane = 100f;

            var lightObject = new GameObject("Key Light");
            lightObject.transform.SetParent(_worldRoot, false);
            var light = lightObject.AddComponent<Light>();
            light.type = LightType.Directional;
            light.intensity = 1.15f;
            light.shadows = LightShadows.Soft;
            lightObject.transform.rotation = Quaternion.Euler(50f, -30f, 0f);

            var fillObject = new GameObject("Fill Light");
            fillObject.transform.SetParent(_worldRoot, false);
            var fill = fillObject.AddComponent<Light>();
            fill.type = LightType.Directional;
            fill.intensity = .35f;
            fillObject.transform.rotation = Quaternion.Euler(25f, 145f, 0f);

            _cameraController = cameraObject.AddComponent<RuntimeCameraController>();
        }

        private static Material MakeMaterial(Color color)
        {
            var material = new Material(Shader.Find("Universal Render Pipeline/Lit"));
            material.color = color;
            return material;
        }
    }
}
