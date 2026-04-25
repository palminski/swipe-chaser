using UnityEngine;

[RequireComponent(typeof(CastHandler))]
public class EntityMoveBounce : MonoBehaviour
{
    public float moveSpeed = 1f;
    [SerializeField] private float startAngle;
    private Vector2 direction;
    private CastHandler castHandler;
    private Rigidbody2D rb;

    [SerializeField] private bool shouldPong;

    void FixedUpdate()
    {
        if (direction == Vector2.zero) return;

        Vector2 desiredDelta = moveSpeed * Time.fixedDeltaTime * direction.normalized;
        float distance = desiredDelta.magnitude;
        Vector2 virtualCenter = castHandler.boxCollider.bounds.center;
        CastHandlerResult hit = castHandler.CastBox(virtualCenter, direction, distance);

        if (hit.hit)
        {
            Vector2 moveToHit = direction * hit.distance;
            desiredDelta = moveToHit;
            if (shouldPong)
            {
                direction = Vector2.Reflect(direction.normalized, hit.normal).normalized;
            }
            else
            {
                direction *= -1;
            }
        }
        rb.MovePosition(rb.position + desiredDelta);
    }
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        castHandler = GetComponent<CastHandler>();
        float startRadians = startAngle * Mathf.Deg2Rad;
        direction = new Vector2(Mathf.Cos(startRadians), Mathf.Sin(startRadians));
        rb = GetComponent<Rigidbody2D>();
    }

    void OnDrawGizmos()
    {
        Gizmos.color = Color.magenta;
        float startRadians = startAngle * Mathf.Deg2Rad;
        Vector2 rayDirection = new Vector2(Mathf.Cos(startRadians), Mathf.Sin(startRadians));
        Gizmos.DrawLine(transform.position, transform.position + (Vector3)rayDirection * 2);

    }
}
