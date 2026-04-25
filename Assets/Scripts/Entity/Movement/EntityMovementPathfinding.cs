using UnityEngine;
using System.Collections.Generic;

public class EntityMovementPathfinding : MonoBehaviour
{
    public float moveSpeed = 1f;
    public float reachDist = 0.05f;
    public bool canMoveDiagonal;
    public bool preventCornerCutting;

    private Rigidbody2D rb;
    public LayerMask collidableLayers;
    private PathfindingGridController grid;
    private List<Vector3> path;
    private int pathIndex;

    private Player player;

     // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        player = GameObject.FindGameObjectWithTag("Player").GetComponent<Player>();
        if (!player) Destroy(gameObject);

        rb = GetComponent<Rigidbody2D>();
        grid = GameController.Instance.PathfindingGrid;
        FindTarget();
    }

    void OnDisable()
    {
        path = null;
    }

    void FixedUpdate()
    {
        if (!grid) return;

        if (path == null || pathIndex >= path.Count)
        {
            FindTarget();
            return;
        }

        Vector2 pos = rb.position;
        Vector2 wp = path[pathIndex];

        if((wp-pos).sqrMagnitude <= reachDist * reachDist)
        {
            FindTarget();
            return;
        }

        Vector2 direction = wp - pos;
        float distance = direction.magnitude;
        float stepDistance = moveSpeed * Time.fixedDeltaTime;

        if (distance <= reachDist || distance <= stepDistance)
        {
            rb.MovePosition(wp);
            pathIndex++;
            FindTarget();

            return;
        }
        Vector2 step = direction.normalized * moveSpeed * Time.fixedDeltaTime;
        rb.MovePosition(pos + step);
    }
   

    void FindTarget()
    {
        
        bool foundPath = grid.TryToFindPath(
            transform.position,
            player.castHandler.boxCollider.bounds.center,
            collidableLayers,
            canMoveDiagonal,
            preventCornerCutting,
            out var newPath,
            50000
        );
        
        if (!foundPath || newPath == null || newPath.Count == 0)
        {
            path = null;
            pathIndex = 0;
            return;
        }
        path = newPath;
        pathIndex = 0;
        //Likely need to set the targtet path to one more than the starting one
        if (path.Count > 1 && (path[0] - (Vector3)rb.position).sqrMagnitude < 0.01f) pathIndex = 1;
    }

    void OnDrawGizmos()
    {

        if (path == null || path.Count < 2) return;
        Gizmos.color = Color.red;
        for (int i = 0; i < path.Count - 1; i++)
            Gizmos.DrawLine(path[i], path[i + 1]);

    }
}
