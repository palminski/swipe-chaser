using UnityEngine;

public class EntityMovePatrol : MonoBehaviour
{
    public Vector3[] localWaypoints;
    private Vector3[] globalWaypoints;

    public bool shouldReverse;
    public float waitTime = 0;
    [Range(0, 2)] public float easeAmount = 0;
    public float speed = 1;
    private int fromWaypointIndex = 0;
    private float percentBetweenWaypoints;
    private float nextMoveTime;

    private Rigidbody2D rb;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        percentBetweenWaypoints = 0;
        globalWaypoints = new Vector3[localWaypoints.Length];
        for (int i = 0; i < localWaypoints.Length; i++)
        {
            globalWaypoints[i] = localWaypoints[i] + transform.position;
        }
    }

    void FixedUpdate()
    {
        Vector3 movement = CalculatePlatformMovement();
        rb.MovePosition(transform.position + movement);
    }

    public virtual Vector3 CalculatePlatformMovement()
    {

        if (Time.time < nextMoveTime)
        {
            return Vector3.zero;
        }
        fromWaypointIndex %= globalWaypoints.Length;
        int toWaypointIndex = (fromWaypointIndex + 1) % globalWaypoints.Length;
        float distBetweenWaypoints = Vector3.Distance(globalWaypoints[fromWaypointIndex], globalWaypoints[toWaypointIndex]);
        percentBetweenWaypoints += speed * Time.fixedDeltaTime / distBetweenWaypoints;
        percentBetweenWaypoints = Mathf.Clamp01(percentBetweenWaypoints);
        float easedPercentBetweenWaypoints = Ease(percentBetweenWaypoints);

        Vector3 newPos = Vector3.Lerp(globalWaypoints[fromWaypointIndex], globalWaypoints[toWaypointIndex], easedPercentBetweenWaypoints);

        if (percentBetweenWaypoints >= 1)
        {
            percentBetweenWaypoints = 0;

            fromWaypointIndex++;

            if (shouldReverse)
            {
                if (fromWaypointIndex >= globalWaypoints.Length - 1)
                {
                    fromWaypointIndex = 0;
                    System.Array.Reverse(globalWaypoints);
                }
            }
            nextMoveTime = Time.time + waitTime;
        }
        return newPos - transform.position;
    }

    public float Ease(float x)
    {
        float a = easeAmount + 1;
        return Mathf.Pow(x, a) / (Mathf.Pow(x, a) + Mathf.Pow(1 - x, a));
    }

    void OnDrawGizmos()
    {
        if (localWaypoints != null && localWaypoints.Length >= 2)
        {
            Gizmos.color = Color.magenta;
            float size = 0.3f;

            

            for (int i = 0; i < localWaypoints.Length; i++)
            {
                Vector3 globalWaypointPosition = localWaypoints[i] + transform.position;
                Gizmos.DrawSphere(globalWaypointPosition, size);
                if (i < localWaypoints.Length - 1)
                {
                    Vector3 next = localWaypoints[i + 1] + transform.position;
                    Gizmos.DrawLine(globalWaypointPosition, next);
                }
            }
        }
    }
}
