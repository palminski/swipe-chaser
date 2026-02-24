using System.Collections;
using UnityEngine;
[System.Flags]
public enum HittableDirections
{
    None = 0,
    Top = 1 << 0,
    Left = 1 << 1,
    Right = 1 << 2,
    Bottom = 1 << 3,
    All = Top | Right | Left | Bottom
}
public class Enemy : MonoBehaviour
{
    [SerializeField] public HittableDirections vulnerableFrom = HittableDirections.All;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {

    }

    void OnTriggerEnter2D(Collider2D collider)
    {
        if (collider.gameObject.TryGetComponent<Player>(out Player player))
        {
            if (
                player.isCharging &&
                (
                    vulnerableFrom.HasFlag(HittableDirections.Top) && player.move.normalized == Vector2.down ||
                    vulnerableFrom.HasFlag(HittableDirections.Left) && player.move.normalized == Vector2.right ||
                    vulnerableFrom.HasFlag(HittableDirections.Right) && player.move.normalized == Vector2.left ||
                    vulnerableFrom.HasFlag(HittableDirections.Bottom) && player.move.normalized == Vector2.up
                )
            )
            {

                StartCoroutine(ImpactFrame(player));
            }
            else
            {
                player.DebugKillPlayer();
            }
        }
    }
    private IEnumerator ImpactFrame(Player player)
    {
        player.RefundCharge();

        // Collider2D enemyColider = GetComponent<Collider2D>();
        // Collider2D playerColider = player.GetComponent<Collider2D>();

        // Vector2 closestPointOnEnemyToPlayer = enemyColider.ClosestPoint(playerColider.bounds.center);
        // Vector2 dir = ((Vector2)playerColider.bounds.center - closestPointOnEnemyToPlayer).normalized;

        // float gap = 0.05f;
        // Vector2 pointToJumpTo = closestPointOnEnemyToPlayer + dir * gap;

        // player.SnapToPosition(pointToJumpTo);


        Time.timeScale = 0;
        Vector2 finalPoint = player.move.x == 0 ? new(player.transform.position.x, transform.position.y) : new(transform.position.x, player.transform.position.y);
        player.SnapToPosition(finalPoint);
        yield return new WaitForSecondsRealtime(0.15f);


        Time.timeScale = 1;



        Destroy(gameObject);
    }
}
