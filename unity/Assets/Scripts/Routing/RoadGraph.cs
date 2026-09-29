using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using VectorTraffic3D.Core;

namespace VectorTraffic3D.Routing
{
    public enum RoadNodeType
    {
        Normal = 0,
        Junction = 1,
        Exit = 2
    }

    [Serializable]
    public sealed class RoadNode
    {
        public int Id { get; }
        public GridPosition Position { get; }
        public RoadNodeType Type { get; internal set; }

        internal RoadNode(int id, GridPosition position, RoadNodeType type = RoadNodeType.Normal)
        {
            Id = id;
            Position = position;
            Type = type;
        }

        public RoadNode Clone() => new RoadNode(Id, Position, Type);
    }

    [Serializable]
    public sealed class RoadSegment
    {
        public int Id { get; }
        public int FromNodeId { get; }
        public int ToNodeId { get; }
        public Direction Direction { get; }

        internal RoadSegment(int id, int fromNodeId, int toNodeId, Direction direction)
        {
            Id = id;
            FromNodeId = fromNodeId;
            ToNodeId = toNodeId;
            Direction = direction;
        }

        public RoadSegment Clone() => new RoadSegment(Id, FromNodeId, ToNodeId, Direction);
    }

    [Serializable]
    public sealed class Junction
    {
        public int NodeId { get; }
        public GridPosition Position { get; }
        public IReadOnlyList<int> IncomingSegmentIds { get; }
        public IReadOnlyList<int> OutgoingSegmentIds { get; }

        internal Junction(int nodeId, GridPosition position, IEnumerable<int> incoming, IEnumerable<int> outgoing)
        {
            NodeId = nodeId;
            Position = position;
            IncomingSegmentIds = incoming.OrderBy(x => x).ToArray();
            OutgoingSegmentIds = outgoing.OrderBy(x => x).ToArray();
        }
    }

    /// <summary>
    /// Deterministic, grid-backed logical road network.
    /// Nodes represent traversable cells. Directed segments represent legal head-to-head
    /// transitions. The graph is gameplay data; presentation must consume it rather than
    /// inventing roads independently.
    /// </summary>
    [Serializable]
    public sealed class RouteGraph
    {
        private readonly Dictionary<int, RoadNode> _nodes = new Dictionary<int, RoadNode>();
        private readonly Dictionary<int, RoadSegment> _segments = new Dictionary<int, RoadSegment>();
        private readonly Dictionary<GridPosition, int> _nodeByPosition = new Dictionary<GridPosition, int>();
        private readonly Dictionary<int, List<int>> _outgoing = new Dictionary<int, List<int>>();
        private readonly Dictionary<int, List<int>> _incoming = new Dictionary<int, List<int>>();

        public IReadOnlyDictionary<int, RoadNode> Nodes => _nodes;
        public IReadOnlyDictionary<int, RoadSegment> Segments => _segments;
        public int NodeCount => _nodes.Count;
        public int SegmentCount => _segments.Count;
        public bool IsEmpty => _nodes.Count == 0;

        public int AddNode(GridPosition position, RoadNodeType type = RoadNodeType.Normal)
        {
            if (_nodeByPosition.TryGetValue(position, out var existing))
            {
                if (type == RoadNodeType.Junction && _nodes[existing].Type == RoadNodeType.Normal)
                    _nodes[existing].Type = type;
                return existing;
            }

            int id = _nodes.Count == 0 ? 1 : _nodes.Keys.Max() + 1;
            _nodes[id] = new RoadNode(id, position, type);
            _nodeByPosition[position] = id;
            _outgoing[id] = new List<int>();
            _incoming[id] = new List<int>();
            return id;
        }

        public bool TryGetNodeAt(GridPosition position, out RoadNode node)
        {
            if (_nodeByPosition.TryGetValue(position, out var id) && _nodes.TryGetValue(id, out node))
                return true;
            node = null;
            return false;
        }

        public RoadNode GetNode(int nodeId)
        {
            return _nodes[nodeId];
        }

        public int AddDirectedSegment(GridPosition from, GridPosition to)
        {
            int fromId = AddNode(from);
            int toId = AddNode(to);
            var direction = DirectionFromTo(from, to);

            if (direction == Direction.None)
                throw new ArgumentException("Road segments must connect adjacent orthogonal cells.");

            foreach (var existingId in _outgoing[fromId])
            {
                var existing = _segments[existingId];
                if (existing.ToNodeId == toId)
                    return existing.Id;
            }

            int id = _segments.Count == 0 ? 1 : _segments.Keys.Max() + 1;
            _segments[id] = new RoadSegment(id, fromId, toId, direction);
            _outgoing[fromId].Add(id);
            _incoming[toId].Add(id);
            RefreshNodeTypes(fromId);
            RefreshNodeTypes(toId);
            return id;
        }

        public void AddBidirectionalSegment(GridPosition a, GridPosition b)
        {
            AddDirectedSegment(a, b);
            AddDirectedSegment(b, a);
        }

        public IReadOnlyList<RoadSegment> GetOutgoingSegments(GridPosition position)
        {
            if (!TryGetNodeAt(position, out var node))
                return Array.Empty<RoadSegment>();

            return _outgoing[node.Id]
                .Select(id => _segments[id])
                .OrderBy(s => (int)s.Direction)
                .ThenBy(s => s.Id)
                .ToArray();
        }

        public IReadOnlyList<Direction> GetOutgoingDirections(GridPosition position)
        {
            return GetOutgoingSegments(position)
                .Select(s => s.Direction)
                .Distinct()
                .OrderBy(d => (int)d)
                .ToArray();
        }

        public bool HasOutgoingDirection(GridPosition position, Direction direction)
        {
            return GetOutgoingSegments(position).Any(s => s.Direction == direction);
        }

        public bool IsJunction(GridPosition position)
        {
            if (!TryGetNodeAt(position, out var node))
                return false;

            return _incoming[node.Id].Count > 1 || _outgoing[node.Id].Count > 1;
        }

        public Junction GetJunction(GridPosition position)
        {
            if (!TryGetNodeAt(position, out var node))
                return null;

            if (!IsJunction(position))
                return null;

            return new Junction(node.Id, position, _incoming[node.Id], _outgoing[node.Id]);
        }

        public bool IsConnected(GridPosition position)
        {
            return _nodeByPosition.ContainsKey(position);
        }

        public bool IsValidTransition(GridPosition from, Direction direction, out GridPosition to)
        {
            to = from.Offset(direction);
            return HasOutgoingDirection(from, direction) && IsConnected(to);
        }

        public string GetCanonicalTopologyHash()
        {
            var sb = new StringBuilder();
            foreach (var node in _nodes.Values.OrderBy(n => n.Id))
                sb.Append($"N{node.Id}:{node.Position.X},{node.Position.Y}:T{(int)node.Type};");

            foreach (var segment in _segments.Values
                .OrderBy(s => s.FromNodeId)
                .ThenBy(s => s.ToNodeId)
                .ThenBy(s => s.Id))
            {
                sb.Append($"S{segment.Id}:{segment.FromNodeId}>{segment.ToNodeId}:D{(int)segment.Direction};");
            }

            return sb.ToString();
        }

        public RouteGraph DeepClone()
        {
            var clone = new RouteGraph();
            foreach (var node in _nodes.Values.OrderBy(n => n.Id))
                clone.AddNode(node.Position, node.Type);

            foreach (var segment in _segments.Values.OrderBy(s => s.Id))
            {
                var from = _nodes[segment.FromNodeId].Position;
                var to = _nodes[segment.ToNodeId].Position;
                clone.AddDirectedSegment(from, to);
            }

            return clone;
        }

        private void RefreshNodeTypes(int nodeId)
        {
            if (!_nodes.TryGetValue(nodeId, out var node) || node.Type == RoadNodeType.Exit)
                return;

            if (_incoming[nodeId].Count > 1 || _outgoing[nodeId].Count > 1)
                node.Type = RoadNodeType.Junction;
            else
                node.Type = RoadNodeType.Normal;
        }

        private static Direction DirectionFromTo(GridPosition from, GridPosition to)
        {
            int dx = to.X - from.X;
            int dy = to.Y - from.Y;

            if (dx == 1 && dy == 0) return Direction.Right;
            if (dx == -1 && dy == 0) return Direction.Left;
            if (dx == 0 && dy == 1) return Direction.Up;
            if (dx == 0 && dy == -1) return Direction.Down;
            return Direction.None;
        }
    }
}
