using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

public class PathfindingGridController : MonoBehaviour
{
    [SerializeField] private bool shouldDrawDebugGrid;
    [SerializeField] private Tilemap gridReference;
    [SerializeField] private LayerMask detectableLayers;
    [Range(0.1f, 1f)][SerializeField] private float cellSizeToSample = 0.5f;
    [SerializeField] private bool includeTriggers = false;
    public class Node
    {
        public Vector3Int cell;
        public int presentLayerMask;
    }
    public Dictionary<Vector3Int, Node> nodes = new();
    public BoundsInt bounds;
    private readonly Collider2D[] _hits = new Collider2D[16];
    private static readonly Vector3Int[] Dir4 =
    {
      new(1,0,0),
      new(-1,0,0),
      new(0,1,0),
      new(0,-1,0),
    };
    private static readonly Vector3Int[] Dir8 =
    {
      new(1,0,0),
      new(-1,0,0),
      new(0,1,0),
      new(0,-1,0),
      new(1,1,0),
      new(-1,1,0),
      new(1,-1,0),
      new(-1,-1,0),
    };

    //----------------------------------------------------------------------------------------------------

    // ---    GRAPH CONSTRUCTION

    //----------------------------------------------------------------------------------------------------
    public void Build()
    {
        nodes.Clear();
        if (!gridReference) return;
        gridReference.CompressBounds();
        bounds = gridReference.cellBounds;

        var filter = new ContactFilter2D
        {
            useLayerMask = true,
            layerMask = detectableLayers,
            useTriggers = includeTriggers,
        };

        Vector3 cellSize = gridReference.cellSize;
        Vector2 boxSize = new Vector2(cellSize.x * cellSizeToSample, cellSize.y * cellSizeToSample);

        foreach (var cell in bounds.allPositionsWithin)
        {
            Vector2 center = gridReference.GetCellCenterWorld(cell);
            int mask = 0;
            
            if (SampleHits(center, filter))
            {
                int count = Physics2D.OverlapBox(center, boxSize, 0f, filter, _hits);
                for (int i = 0; i < count; i++)
                {
                    var col = _hits[i];
                    if (col == null) continue;
                    int layer = col.gameObject.layer;
                    mask |= (1 << layer);
                    _hits[i] = null;
                }
            }

            nodes[cell] = new Node
            {
                cell = cell,
                presentLayerMask = mask
            };
            // print("Position: "+nodes[cell].cell+" Mask: "+mask);
        }
    }

    private bool SampleHits(Vector2 position, ContactFilter2D filter)
    {
        bool blocked = (
            SampleHit(position, filter) &&
            SampleHit(position + Vector2.left * 0.1f, filter) &&
            SampleHit(position + Vector2.up * 0.1f, filter) &&
            SampleHit(position + Vector2.right * 0.1f, filter) &&
            SampleHit(position + Vector2.down * 0.1f, filter)
        );
        return blocked;
    }

    private bool SampleHit(Vector2 p, ContactFilter2D filter)
    {
        int count = Physics2D.OverlapPoint(p, filter, _hits);
        for (int i = 0; i < count; i++) _hits[i] = null;
        return count > 0;
    }

    public void RebindAndBuild(Tilemap tilemap = null)
    {
        gridReference = tilemap != null ? tilemap : FindFirstObjectByType<Tilemap>();
        Build();
    }

    //----------------------------------------------------------------------------------------------------

    //---   HELPERS

    //----------------------------------------------------------------------------------------------------
    public Vector3Int WorldToCell(Vector3 worldPosition)
    {
        return gridReference.WorldToCell(worldPosition);
    }

    public Vector3 CellToWorldCenter(Vector3Int cell)
    {
        return gridReference.GetCellCenterWorld(cell);
    }

    public bool TryGetNodeAtWorld(Vector3 worldPosition, out Node node)
    {
        Vector3Int cell = WorldToCell(worldPosition);
        return nodes.TryGetValue(cell, out node);
    }

    public bool TryGetNode(Vector3Int cell, out Node node)
    {
        return nodes.TryGetValue(cell, out node);
    }

    //----------------------------------------------------------------------------------------------------

    //---   PATHFINDING

    //----------------------------------------------------------------------------------------------------

    public bool TryToFindPath(
        Vector3 startWorld,
        Vector3 goalWorld,
        LayerMask blockedLayers,
        bool canMoveDiagonal,
        bool preventCuttingCorners,
        out List<Vector3> worldPath,
        int maxExpandedNodes = 400000
    )
    {
        worldPath = null;
        if (nodes.Count == 0)
        {
            Debug.LogWarning("PATHFINDING NODES ARE EMPTY! Did you make sure to call Build()?");
            return false;
        }

        Vector3Int startCell = WorldToCell(startWorld);
        Vector3Int goalCell = WorldToCell(goalWorld);

        if (!nodes.TryGetValue(startCell, out var startNode)) return false;
        if (!nodes.TryGetValue(goalCell, out var goalNode)) return false;

        if (!CanEnter(startNode, blockedLayers)) return false;
        if (!CanEnter(goalNode, blockedLayers))
        {
            if (!TryToGetAdjacentEnterableCell(startCell, goalCell, blockedLayers, canMoveDiagonal, out var newGoalCell)) return false;
            goalCell = newGoalCell;
            goalNode = nodes[goalCell];
        }

        if (!TryAStar(startNode, goalNode, blockedLayers, canMoveDiagonal, preventCuttingCorners, maxExpandedNodes, out var cellPath)) return false;

        worldPath = new List<Vector3>(cellPath.Count);
        for (int i = 0; i < cellPath.Count; i++)
        {
            worldPath.Add(CellToWorldCenter(cellPath[i]));
        }
        return true;
    }

    private bool TryToGetAdjacentEnterableCell(
        Vector3Int startingCell,
        Vector3Int goalCell,
        LayerMask blockedLayers,
        bool canMoveDiagonal,
        out Vector3Int newGoalCell
    )
    {
        newGoalCell = default;
        var directions = canMoveDiagonal ? Dir8 : Dir4;
        bool found = false;
        int bestH = int.MaxValue;

        for (int i = 0; i < directions.Length; i++)
        {
            var c = goalCell + directions[i];
            if (!nodes.TryGetValue(c, out var n)) continue;
            if (!CanEnter(n, blockedLayers)) continue;

            int h = Heuristic(startingCell, c, canMoveDiagonal);
            if (h < bestH)
            {
                bestH = h;
                newGoalCell = c;
                found = true;
            }
        }
        return found;
    }

    private bool TryAStar(
        Node start,
        Node goal,
        LayerMask blockedLayers,
        bool canMoveDiagonal,
        bool preventCuttingCorners,
        int maxExpandedNodes,
        out List<Vector3Int> cellPath
    )
    {
        cellPath = null;

        Vector3Int startCell = start.cell;
        Vector3Int goalCell = goal.cell;

        var cameFrom = new Dictionary<Vector3Int, Vector3Int>(1024);
        var gScore = new Dictionary<Vector3Int, int>(1024);

        //Can be optimised using a priority queue if need be
        var openList = new List<Vector3Int>(1024);
        var openSet = new HashSet<Vector3Int>();
        var closedSet = new HashSet<Vector3Int>();

        gScore[startCell] = 0;
        openList.Add(startCell);
        openSet.Add(startCell);

        //Will be used to check if grid ever gets too big
        int expanded = 0;

        while (openList.Count > 0)
        {
            int bestIndex = 0;
            int bestF = int.MaxValue;

            for (int i = 0; i < openList.Count; i++)
            {
                var c = openList[i];

                int g = gScore.TryGetValue(c, out var gv) ? gv : int.MaxValue;
                int f = g + Heuristic(c, goalCell, canMoveDiagonal);

                if (f < bestF)
                {
                    bestF = f;
                    bestIndex = i;
                }
            }

            Vector3Int current = openList[bestIndex];

            openList.RemoveAt(bestIndex);
            openSet.Remove(current);
            closedSet.Add(current);

            if (current == goalCell)
            {
                cellPath = ReconstructPath(cameFrom, current);
                return true;
            }

            expanded++;
            if (expanded > maxExpandedNodes) return false;

            foreach (var neighborCell in GetNeighbors(current, canMoveDiagonal, preventCuttingCorners, blockedLayers))
            {
                if (closedSet.Contains(neighborCell)) continue;
                int tentativeG = gScore[current] + MoveCost(current, neighborCell);
                if (!gScore.TryGetValue(neighborCell, out var existingG) || tentativeG < existingG)
                {
                    cameFrom[neighborCell] = current;
                    gScore[neighborCell] = tentativeG;
                    if (!openSet.Contains(neighborCell))
                    {
                        openList.Add(neighborCell);
                        openSet.Add(neighborCell);
                    }
                }
            }
        }

        return false;
    }

    private IEnumerable<Vector3Int> GetNeighbors(
        Vector3Int cell,
        bool canMoveDiagonal,
        bool preventCuttingCorners,
        LayerMask blockedLayers
    )
    {
        var dirs = canMoveDiagonal ? Dir8 : Dir4;

        for (int i = 0; i < dirs.Length; i++)
        {
            Vector3Int dir = dirs[i];
            Vector3Int n = cell + dir;

            if (!nodes.TryGetValue(n, out var neighborNode)) continue;
            if (!CanEnter(neighborNode, blockedLayers)) continue;

            //Prevent Corner Cut if needed
            if (canMoveDiagonal && preventCuttingCorners && dir.x != 0 && dir.y != 0)
            {
                var sideA = cell + new Vector3Int(dir.x, 0, 0);
                var sideB = cell + new Vector3Int(0, dir.y, 0);

                if (nodes.TryGetValue(sideA, out var a) && !CanEnter(a, blockedLayers)) continue;
                if (nodes.TryGetValue(sideB, out var b) && !CanEnter(b, blockedLayers)) continue;
            }
            yield return n;
        }
    }

    private bool CanEnter(Node node, LayerMask blockedLayers)
    {
        return (node.presentLayerMask & blockedLayers.value) == 0;
    }

    private int MoveCost(Vector3Int from, Vector3Int to)
    {
        int dx = Mathf.Abs(from.x - to.x);
        int dy = Mathf.Abs(from.y - to.y);
        return (dx == 1 && dy == 1) ? 14 : 10;
    }

    private int Heuristic(Vector3Int from, Vector3Int to, bool canMoveDiagonal)
    {
        int dx = Mathf.Abs(from.x - to.x);
        int dy = Mathf.Abs(from.y - to.y);

        if (!canMoveDiagonal) return 10 * (dx + dy);

        int min = Mathf.Min(dx, dy);
        int max = Mathf.Max(dx, dy);
        return 14 * min + 10 * (max - min);
    }

    private List<Vector3Int> ReconstructPath(Dictionary<Vector3Int, Vector3Int> cameFrom, Vector3Int current)
    {
        var total = new List<Vector3Int> { current };
        while (cameFrom.TryGetValue(current, out var prev))
        {
            current = prev;
            total.Add(current);
        }
        total.Reverse();
        return total;
    }

    void OnDrawGizmos()
    {
        if (!shouldDrawDebugGrid) return;
        if (nodes == null || nodes.Count == 0) return;

        Vector3 cellSize = gridReference.cellSize;

        foreach (var kvp in nodes)
        {
            var node = kvp.Value;
            Vector3 worldCenter = gridReference.GetCellCenterWorld(node.cell);
            bool isBlocked = node.presentLayerMask != 0;
            Gizmos.color = isBlocked ? Color.red : Color.green;

            Gizmos.DrawCube(worldCenter, cellSize * 0.25f);
        }
    }

}
