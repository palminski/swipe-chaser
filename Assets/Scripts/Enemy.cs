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
                player.AllowChargeDirectionChange(transform.position);
                StartCoroutine(ImpactFrame());
            }
            else
            {
                player.DebugKillPlayer();
            }
        }
    }
    private IEnumerator ImpactFrame()
    {
        Time.timeScale = 0;
        yield return new WaitForSecondsRealtime(0.2f);
        Time.timeScale = 1;
        Destroy(gameObject);
    }
}
