using UnityEngine;
[RequireComponent(typeof(BoxCollider2D))]
public class CastHandler : MonoBehaviour
{
    [HideInInspector]public BoxCollider2D boxCollider;
    [SerializeField] private float skinWidth = 0.001f;
    public LayerMask collidableLayers;

    void Awake()
    {
        boxCollider = GetComponent<BoxCollider2D>();
    }

    public CastControllerResult CastBox(Vector2 center, Vector2 direction, float distance)
    {
        CastControllerResult result = new CastControllerResult
        {
            hit = false,
            distance = distance,
            normal = Vector2.zero
        };

        Bounds bounds = boxCollider.bounds;
        bounds.Expand(skinWidth * -2);
        
        RaycastHit2D hit = Physics2D.BoxCast(
            center,
            bounds.size,
            0f,
            direction.normalized,
            distance + skinWidth,
            collidableLayers
        );

        if (hit.collider)
        {
            result.hit = true;
            result.normal = hit.normal;
            result.distance = Mathf.Max(0f, hit.distance - skinWidth);
            
        }

        return result;
    }

    
}

public struct CastControllerResult
{
    public bool hit;
    public float distance;
    public Vector2 normal;
}
