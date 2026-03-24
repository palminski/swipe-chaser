using UnityEngine;

public class RaycastController : MonoBehaviour
{
    private BoxCollider2D boxCollider;
    [SerializeField] private float skinWidth = 0.001f;
    [SerializeField] private float offsetCorrectionThreshold = 0.1f;
    public RaycastOrigins raycastOrigins;
    public float topBottomRaySpacing;
    public float sideRaySpacing;
    public LayerMask collidableLayers;
    [SerializeField] private bool shouldDrawRaysForDebug = false;

    [SerializeField][Min(2)] private int raysAcrossSide = 5;
    [SerializeField][Min(2)] private int raysAcrossTop = 5;

    void Awake()
    {
        boxCollider = GetComponent<BoxCollider2D>();
    }
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {

    }

    public RaycastControllerResult CastRays(Vector2 direction, float distance, Vector2? moveDirection = null)
    {
        UpdateRaySpacing();

        RaycastControllerResult result = new RaycastControllerResult();
        result.distance = float.MaxValue;
        result.hit = false;

        float raySpacing = 0;
        Vector2 rayOrigin = Vector2.zero;
        Vector2 offsetStep = Vector2.zero;

        Vector2 _moveDirection = Vector2.zero;
        if (!moveDirection.HasValue)
        {
            _moveDirection = direction;
        }
        else
        {
            _moveDirection = moveDirection.Value;
        }
        int rayCount = _moveDirection.x == 0 ? raysAcrossTop : raysAcrossSide;

        if (_moveDirection == Vector2.up)
        {
            offsetStep = Vector2.right;
            raySpacing = topBottomRaySpacing;
            rayOrigin = raycastOrigins.topLeft;
        }
        else if (_moveDirection == Vector2.right)
        {
            offsetStep = Vector2.down;
            raySpacing = sideRaySpacing;
            rayOrigin = raycastOrigins.topRight;
        }
        else if (_moveDirection == Vector2.left)
        {
            offsetStep = Vector2.up;
            raySpacing = sideRaySpacing;
            rayOrigin = raycastOrigins.bottomLeft;
        }
        else if (_moveDirection == Vector2.down)
        {
            offsetStep = Vector2.left;
            raySpacing = topBottomRaySpacing;
            rayOrigin = raycastOrigins.bottomRight;
        }


        for (int i = 0; i < rayCount; i++)
        {
            RaycastHit2D hit = Physics2D.Raycast(
                rayOrigin + (offsetStep * raySpacing * i),
                direction,
                distance,
                collidableLayers
            );

            if (shouldDrawRaysForDebug) Debug.DrawRay(rayOrigin + (offsetStep * raySpacing * i), direction, Color.rebeccaPurple, distance);

            if (hit.collider)
            {
                result.hitNumber += 1;
                result.hit = true;
                if (hit.distance < result.distance)
                {
                    result.normal = hit.normal;
                    result.distance = hit.distance - skinWidth;
                }
            }
        }
        return result;
    }

    public Vector2 GetOffsetCorrection(Vector2 direction, float distance)
    {
        float distanceA = distance;
        float distanceB = distance;

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
        if (hitA) distanceA = hitA.distance + skinWidth;
        RaycastHit2D hitAOffset = Physics2D.Raycast(rayOriginAOffset, direction, distanceA, collidableLayers);

        RaycastHit2D hitB = Physics2D.Raycast(rayOriginB, direction, distanceB, collidableLayers);
        if (hitB) distanceB = hitB.distance + skinWidth;
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

    public void TranslateRaycastOrigins(Vector2 translateAmount)
    {
        raycastOrigins.topRight += translateAmount;
        raycastOrigins.topLeft += translateAmount;
        raycastOrigins.bottomLeft += translateAmount;
        raycastOrigins.bottomRight += translateAmount;
    }

    public void UpdateRaySpacing()
    {
        Bounds bounds = boxCollider.bounds;
        bounds.Expand(skinWidth * -2);

        sideRaySpacing = bounds.size.x / (raysAcrossSide - 1);
        topBottomRaySpacing = bounds.size.y / (raysAcrossTop - 1);
    }

    public struct RaycastOrigins
    {
        public Vector2 topLeft, topRight, bottomLeft, bottomRight;
    }
}

public struct RaycastControllerResult
{
    public bool hit;
    public float distance;
    public int hitNumber;
    public Vector2 normal;
}
