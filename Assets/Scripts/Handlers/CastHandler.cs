using UnityEngine;
[RequireComponent(typeof(BoxCollider2D))]
public class CastHandler : MonoBehaviour
{
    [HideInInspector]public BoxCollider2D boxCollider;
    [SerializeField] private float skinWidth = 0.001f;
    public LayerMask collidableLayers;

    [SerializeField] private float offsetCorrectionThreshold = 0.1f;
    [SerializeField] private float offsetCorrectionCheckDistance = 0.3f;
    public RaycastOrigins raycastOrigins;

    void Awake()
    {
        boxCollider = GetComponent<BoxCollider2D>();
    }

    public CastHandlerResult CastBox(Vector2 center, Vector2 direction, float distance)
    {
        CastHandlerResult result = new CastHandlerResult
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

    public Vector2 GetOffsetCorrection(Vector2 direction, float distance)
    {
        UpdateRaycastOrigins();

        float distanceA = distance+offsetCorrectionCheckDistance;
        float distanceB = distance+offsetCorrectionCheckDistance;

        Vector2 rayOriginA = Vector2.zero;
        Vector2 rayOriginAOffset = Vector2.zero;
        Vector2 rayOriginB = Vector2.zero;
        Vector2 rayOriginBOffset = Vector2.zero;

        //Set Raycasts based on direction
        if (direction == Vector2.up)
        {
            rayOriginA = raycastOrigins.topLeft;
            rayOriginAOffset = raycastOrigins.topLeft + new Vector2(offsetCorrectionThreshold, 0);

            rayOriginB = raycastOrigins.topRight;
            rayOriginBOffset = raycastOrigins.topRight - new Vector2(offsetCorrectionThreshold, 0);
        }
        else if (direction == Vector2.right)
        {
            rayOriginA = raycastOrigins.topRight;
            rayOriginAOffset = raycastOrigins.topRight - new Vector2(0, offsetCorrectionThreshold);

            rayOriginB = raycastOrigins.bottomRight;
            rayOriginBOffset = raycastOrigins.bottomRight + new Vector2(0, offsetCorrectionThreshold);
        }
        else if (direction == Vector2.left)
        {
            rayOriginA = raycastOrigins.bottomLeft;
            rayOriginAOffset = raycastOrigins.bottomLeft + new Vector2(0, offsetCorrectionThreshold);

            rayOriginB = raycastOrigins.topLeft;
            rayOriginBOffset = raycastOrigins.topLeft - new Vector2(0, offsetCorrectionThreshold);
        }
        else if (direction == Vector2.down)
        {
            rayOriginA = raycastOrigins.bottomRight;
            rayOriginAOffset = raycastOrigins.bottomRight - new Vector2(offsetCorrectionThreshold, 0);

            rayOriginB = raycastOrigins.bottomLeft;
            rayOriginBOffset = raycastOrigins.bottomLeft + new Vector2(offsetCorrectionThreshold, 0);
        }

        RaycastHit2D hitA = Physics2D.Raycast(rayOriginA, direction, distanceA, collidableLayers);
        if (hitA)
        {
            distanceA = hitA.distance + skinWidth;
            float angle = Vector2.Angle(-direction, hitA.normal);
            if (angle != 0) return Vector2.zero;
        }
        RaycastHit2D hitAOffset = Physics2D.Raycast(rayOriginAOffset, direction, distanceA, collidableLayers);

        RaycastHit2D hitB = Physics2D.Raycast(rayOriginB, direction, distanceB, collidableLayers);
        if (hitB)
        {
            distanceB = hitB.distance + skinWidth;
            float angle = Vector2.Angle(-direction, hitB.normal);
            if (angle != 0) return Vector2.zero;
        }
        RaycastHit2D hitBOffset = Physics2D.Raycast(rayOriginBOffset, direction, distanceB, collidableLayers);

        if (hitA && !hitAOffset)
        {
            Vector2 raycastStart = rayOriginAOffset + direction.normalized * distanceA;

            Vector2 raycastDirection = (rayOriginA - rayOriginAOffset).normalized;
            RaycastHit2D hit = Physics2D.Raycast(raycastStart, raycastDirection, offsetCorrectionThreshold, collidableLayers);
            if (hit)
            {
                float distanceToShift = offsetCorrectionThreshold - hit.distance + skinWidth;
                if (direction == Vector2.up) return new Vector2(distanceToShift, 0);
                if (direction == Vector2.right) return new Vector2(0, -distanceToShift);
                if (direction == Vector2.left) return new Vector2(0, distanceToShift);
                if (direction == Vector2.down) return new Vector2(-distanceToShift, 0);
            }
        }
        if (hitB && !hitBOffset)
        {
            Vector2 raycastStart = rayOriginBOffset + direction.normalized * distanceB;

            Vector2 raycastDirection = (rayOriginB - rayOriginBOffset).normalized;
            RaycastHit2D hit = Physics2D.Raycast(raycastStart, raycastDirection, offsetCorrectionThreshold, collidableLayers);
            if (hit)
            {
                float distanceToShift = offsetCorrectionThreshold - hit.distance + skinWidth;
                if (direction == Vector2.up) return new Vector2(-distanceToShift, 0);
                if (direction == Vector2.right) return new Vector2(0, distanceToShift);
                if (direction == Vector2.left) return new Vector2(0, -distanceToShift);
                if (direction == Vector2.down) return new Vector2(distanceToShift, 0);
            }
        }
        return Vector2.zero;
    }

    public void UpdateRaycastOrigins()
    {
        Bounds bounds = boxCollider.bounds;
        bounds.Expand(skinWidth * -2);

        raycastOrigins.topRight = new Vector2(bounds.max.x, bounds.max.y);
        raycastOrigins.topLeft = new Vector2(bounds.min.x, bounds.max.y);
        raycastOrigins.bottomLeft = new Vector2(bounds.min.x, bounds.min.y);
        raycastOrigins.bottomRight = new Vector2(bounds.max.x, bounds.min.y);
    }

    public struct RaycastOrigins
    {
        public Vector2 topLeft, topRight, bottomLeft, bottomRight;
    }
}

public struct CastHandlerResult
{
    public bool hit;
    public float distance;
    public Vector2 normal;
}
