using UnityEngine;
using VectorTraffic3D.Board;
using VectorTraffic3D.Core;
using VectorTraffic3D.Gates;

namespace VectorTraffic3D.Runtime
{
    public sealed class BoardVisualBuilder
    {
        private readonly GridWorldMapper _mapper;
        private readonly Transform _root;

        public BoardVisualBuilder(GridWorldMapper mapper, Transform root)
        {
            _mapper = mapper;
            _root = root;
        }

        public void Build(BoardState board)
        {
            BuildRoadTiles(board);
            BuildExits(board);
            BuildGates(board);
        }

        private void BuildRoadTiles(BoardState board)
        {
            for (var x = 0; x < board.Width; x++)
            for (var y = 0; y < board.Height; y++)
            {
                var p = new GridPosition(x, y);
                if (!board.RoadGraph.IsConnected(p)) continue;

                var tile = GameObject.CreatePrimitive(PrimitiveType.Cube);
                tile.name = $"Road_{x}_{y}";
                tile.transform.SetParent(_root, false);
                tile.transform.position = _mapper.ToWorld(p, -0.02f);
                tile.transform.localScale = new Vector3(
                    _mapper.CellSize * 0.94f, 0.08f, _mapper.CellSize * 0.94f);
                tile.GetComponent<Renderer>().material = MakeMaterial(new Color(.18f, .2f, .23f));
                Object.Destroy(tile.GetComponent<Collider>());
            }
        }

        private void BuildExits(BoardState board)
        {
            foreach (var exit in board.Exits)
            {
                var portal = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
                portal.name = $"Exit_{exit.Value}";
                portal.transform.SetParent(_root, false);
                portal.transform.position = _mapper.ToWorld(exit.Key, .06f);
                portal.transform.localScale = new Vector3(.72f, .06f, .72f);
                portal.GetComponent<Renderer>().material =
                    MakeMaterial(exit.Value % 2 == 0
                        ? new Color(.2f, .75f, .95f)
                        : new Color(.95f, .65f, .18f));
                Object.Destroy(portal.GetComponent<Collider>());
            }
        }

        private void BuildGates(BoardState board)
        {
            foreach (var gate in board.Gates.Values)
            {
                var root = new GameObject($"VectorGate_{gate.Id}");
                root.transform.SetParent(_root, false);
                root.transform.position = _mapper.ToWorld(gate.Position, .15f);

                var beam = GameObject.CreatePrimitive(PrimitiveType.Cube);
                beam.name = "GateBeam";
                beam.transform.SetParent(root.transform, false);
                beam.transform.localScale = new Vector3(.12f, .18f, _mapper.CellSize * .78f);
                beam.transform.localRotation = Quaternion.Euler(
                    0f, WorldYaw(gate.AllowedDirection), 0f);
                beam.GetComponent<Renderer>().material =
                    MakeMaterial(new Color(.8f, .25f, .9f));
                Object.Destroy(beam.GetComponent<Collider>());
            }
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
