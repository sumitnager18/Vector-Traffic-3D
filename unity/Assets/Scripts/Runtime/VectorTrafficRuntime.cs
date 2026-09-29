using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using VectorTraffic3D.Board;
using VectorTraffic3D.Core;
using VectorTraffic3D.Gates;
using VectorTraffic3D.Vehicles;
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
        private Camera _camera;
        private int _selectedVehicle = -1;
        private Vector2 _pointerDown;
        private float _pointerDownTime;
        private bool _busy;
        private Transform _vehiclesRoot;
        private const float SwipePixels = 30f;
        private void Start()
        {
            BuildDemoLevel();
            BuildWorld();
        }
        private void Update()
        {
            if (_busy) return;
            if (Input.GetMouseButtonDown(0))
            {
                _pointerDown = Input.mousePosition;
                _pointerDownTime = Time.unscaledTime;
            }
            if (Input.GetMouseButtonUp(0))
            {
                var delta = (Vector2)Input.mousePosition - _pointerDown;
                var view = RaycastVehicle(Input.mousePosition);
                if (view != null) _selectedVehicle = view.VehicleId;
                if (_selectedVehicle < 0) return;
                Direction direction = delta.magnitude < SwipePixels
                    ? Board.Vehicles[_selectedVehicle].CurrentVector
                    : ScreenDeltaToDirection(delta);
                StartCoroutine(AttemptMove(_selectedVehicle, direction));
            }
        }
        private VehicleView RaycastVehicle(Vector2 screen)
        {
            if (_camera == null) return null;
            Ray ray = _camera.ScreenPointToRay(screen);
            if (!Physics.Raycast(ray, out var hit, 500f)) return null;
            return hit.collider.GetComponentInParent<VehicleView>();
        }
        private IEnumerator AttemptMove(int vehicleId, Direction direction)
        {
            if (!Board.Vehicles.TryGetValue(vehicleId, out var vehicle)) yield break;
            if (Board.Exits.ContainsKey(vehicle.HeadPosition))
            {
                yield break;
            }
            var before = vehicle.Clone();
            if (!MoveValidator.TryExecuteStep(Board, vehicleId, direction, out _, out var result))
            {
                Debug.Log($"Move blocked: {result}");
                yield break;
            }
            _busy = true;
            yield return _views[vehicleId].AnimateTo(vehicle, moveDuration);
            _busy = false;
            if (Board.Exits.ContainsKey(vehicle.HeadPosition))
                StartCoroutine(AttemptExit(vehicleId));
        }
        private IEnumerator AttemptExit(int vehicleId)
        {
            if (!Board.Vehicles.TryGetValue(vehicleId, out var vehicle)) yield break;
            if (!MoveValidator.CanCompleteExit(Board, vehicleId, out var footprints, out var failure))
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
            Debug.Log($"Vehicle {vehicleId} exited.");
        }
        private Direction ScreenDeltaToDirection(Vector2 delta)
        {
            if (Mathf.Abs(delta.x) > Mathf.Abs(delta.y))
                return delta.x > 0 ? Direction.Right : Direction.Left;
            return delta.y > 0 ? Direction.Up : Direction.Down;
        }
        private void BuildDemoLevel()
        {
            Board = new BoardState(width, height);
            for (int x=0;x<width;x++) for (int y=0;y<height;y++)
                Board.AddRoadNode(new GridPosition(x,y));
            for (int x=0;x<width;x++) for (int y=0;y<height;y++)
            {
                var p=new GridPosition(x,y);
                if (x+1<width) Board.AddBidirectionalRoadSegment(p,new GridPosition(x+1,y));
                if (y+1<height) Board.AddBidirectionalRoadSegment(p,new GridPosition(x,y+1));
            }
            int mid=height/2;
            Board.AddExit(new GridPosition(width-1,mid),1);
            Board.AddExit(new GridPosition(0,mid),2);
            Board.AddVehicle(new VehicleState(1,VehicleType.Sedan,new GridPosition(2,mid),Direction.Right,Direction.Right,1));
            Board.AddVehicle(new VehicleState(2,VehicleType.SUV,new GridPosition(5,mid-2),Direction.Up,Direction.Up,1));
            Board.AddVehicle(new VehicleState(3,VehicleType.Hatchback,new GridPosition(3,mid+2),Direction.Down,Direction.Down,2));
            Board.AddGate(new VectorGate(1,new GridPosition(4,mid),Direction.Right));
            if (!Board.TryValidateStaticState(out var error)) Debug.LogError($"Demo level invalid: {error}");
        }
        private void BuildWorld()
        {
            _mapper = new GridWorldMapper(cellSize, new Vector3(-(width-1)*cellSize*.5f,0f,-(height-1)*cellSize*.5f));
            CreateCamera();
            CreateGround();
            _vehiclesRoot = new GameObject("Vehicles").transform;
            foreach (var state in Board.Vehicles.Values)
            {
                var view = VehicleView.Create(_vehiclesRoot,state,_mapper);
                view.SyncImmediate(state);
                _views[state.Id]=view;
            }
        }
        private void CreateGround()
        {
            var go=GameObject.CreatePrimitive(PrimitiveType.Cube);
            go.name="RoadBase";
            go.transform.position=new Vector3(0,-.25f,0);
            go.transform.localScale=new Vector3(width*cellSize,.5f,height*cellSize);
            go.GetComponent<Renderer>().material=MakeMaterial(new Color(.11f,.13f,.16f));
        }
        private void CreateCamera()
        {
            var go=new GameObject("Main Camera");
            _camera=go.AddComponent<Camera>();
            go.tag="MainCamera";
            go.transform.position=new Vector3(0,Mathf.Max(width,height)*1.25f,-Mathf.Max(width,height)*.55f);
            go.transform.rotation=Quaternion.Euler(52f,0f,0f);
            _camera.fieldOfView=45f;
            var light=new GameObject("Key Light");
            var dl=light.AddComponent<Light>();
            dl.type=LightType.Directional;
            dl.intensity=1.15f;
            light.transform.rotation=Quaternion.Euler(50f,-30f,0f);
        }
        private static Material MakeMaterial(Color color)
        {
            var m=new Material(Shader.Find("Universal Render Pipeline/Lit"));
            m.color=color; return m;
        }
    }
}
