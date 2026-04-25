using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
[RequireComponent(typeof(Collider2D))]
public class Bullet : MonoBehaviour
{
    public float speed;
    [Tooltip("Direction Of Bullet In Degrees")][Range(0f, 360f)] public float bulletAngle;
    public LayerMask collidableLayers;
    [HideInInspector] public Vector2 direction;
    private Rigidbody2D rb;
    private Collider2D boxCollider;

    void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        boxCollider = GetComponent<Collider2D>();
    }
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        float directionRadian = bulletAngle * Mathf.Deg2Rad;
        direction = new Vector2(Mathf.Cos(directionRadian), Mathf.Sin(directionRadian));
    }

    void FixedUpdate()
    {
        Vector2 desiredDelta = direction.normalized * speed * Time.fixedDeltaTime;

        float distanceToCheck = speed * Time.fixedDeltaTime;

        RaycastHit2D hit = Physics2D.BoxCast(
            boxCollider.bounds.center,
            boxCollider.bounds.size,
            rb.rotation,
            direction.normalized,
            distanceToCheck,
            collidableLayers
        );

        if (hit)
        {
            distanceToCheck = hit.distance;
        }

        //Check Enemies
        RaycastHit2D[] hitEntities = Physics2D.BoxCastAll(
            boxCollider.bounds.center,
            boxCollider.bounds.size,
            rb.rotation,
            direction.normalized,
            distanceToCheck,
            Physics2D.AllLayers
        );

        foreach (RaycastHit2D hitEntity in hitEntities)
        {
            if (hitEntity.collider.TryGetComponent<Player>(out Player player))
            {
                player.DebugKillPlayer();

            }
        }
        if (hit)
        {
            rb.MovePosition(rb.position + direction.normalized * hit.distance);
            Destroy(gameObject);
        }
        else
        {
            rb.MovePosition(rb.position + desiredDelta);
        }

    }

    void OnDrawGizmos()
    {
        if (Application.isPlaying) return;
        Gizmos.color = Color.magenta;
        float startRadians = bulletAngle * Mathf.Deg2Rad;
        Vector2 rayDirection = new Vector2(Mathf.Cos(startRadians), Mathf.Sin(startRadians));
        Gizmos.DrawLine(transform.position, transform.position + (Vector3)rayDirection * 2);

        //Triangle
        Vector2 right = Quaternion.Euler(0, 0, 25f) * -rayDirection;
        Vector2 left = Quaternion.Euler(0, 0, -25f) * -rayDirection;

        Gizmos.DrawLine(transform.position + (Vector3)rayDirection * 2, (transform.position + (Vector3)rayDirection * 2) + (Vector3)right * 0.4f);
        Gizmos.DrawLine(transform.position + (Vector3)rayDirection * 2, (transform.position + (Vector3)rayDirection * 2) + (Vector3)left * 0.4f);

    }
}
