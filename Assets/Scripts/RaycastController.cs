using UnityEngine;

public class RaycastController : MonoBehaviour
{
    private BoxCollider2D boxCollider;
    [SerializeField] private float skinWidth = 0.001f;
    [SerializeField] private float offsetCorrection = 0.1f;
    public RaycastOrigins raycastOrigins;
    public float topBottomRaySpacing;
    public float sideRaySpacing;
    public LayerMask collidableLayers;
    [SerializeField] private bool shouldDrawRaysForDebug = false;
    [SerializeField] private bool shouldDrawRaysForDebug2 = false;

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

    public RaycastControllerResult CastRays(Vector2 direction, float distance)
    {
        UpdateRaycastOrigins();
        UpdateRaySpacing();

        RaycastControllerResult result = new RaycastControllerResult();
        result.distance = float.MaxValue;
        result.hit = false;

        float raySpacing = 0;
        Vector2 rayOrigin = Vector2.zero;
        Vector2 offsetStep = Vector2.zero;

        int rayCount = direction.x == 0 ? raysAcrossTop : raysAcrossSide;

        if (direction == Vector2.up)
        {
            offsetStep = Vector2.right;
            raySpacing = topBottomRaySpacing;
            rayOrigin = raycastOrigins.topLeft;
        }
        else if (direction == Vector2.right)
        {
            offsetStep = Vector2.down;
            raySpacing = sideRaySpacing;
            rayOrigin = raycastOrigins.topRight;
        }
        else if (direction == Vector2.left)
        {
            offsetStep = Vector2.up;
            raySpacing = sideRaySpacing;
            rayOrigin = raycastOrigins.bottomLeft;
        }
        else if (direction == Vector2.down)
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
                    result.distance = hit.distance - skinWidth;
                }
            }
        }
        return result;
    }

    public Vector2 CheckSlightMisalignment(Vector2 direction, float distance)
    {
        
        Vector2 rayOriginA = Vector2.zero;
        Vector2 rayOriginAOffset = Vector2.zero;
        Vector2 rayOriginB = Vector2.zero;
        Vector2 rayOriginBOffset = Vector2.zero;

        //Set Raycasts based on direction
        if (direction == Vector2.up)
        {
            rayOriginA = raycastOrigins.topLeft;
            rayOriginAOffset = raycastOrigins.topLeft + new Vector2(offsetCorrection, 0);

            rayOriginB = raycastOrigins.topRight;
            rayOriginBOffset = raycastOrigins.topRight - new Vector2(offsetCorrection, 0);

        }
        else if (direction == Vector2.right)
        {
            rayOriginA = raycastOrigins.topRight;
            rayOriginAOffset = raycastOrigins.topRight - new Vector2(0, offsetCorrection);

            rayOriginB = raycastOrigins.bottomRight;
            rayOriginBOffset = raycastOrigins.bottomRight + new Vector2(0, offsetCorrection);
        }
        else if (direction == Vector2.left)
        {
            rayOriginA = raycastOrigins.bottomLeft;
            rayOriginAOffset = raycastOrigins.bottomLeft + new Vector2(0, offsetCorrection);

            rayOriginB = raycastOrigins.topLeft;
            rayOriginBOffset = raycastOrigins.topLeft - new Vector2(0, offsetCorrection);
        }
        else if (direction == Vector2.down)
        {
            rayOriginA = raycastOrigins.bottomRight;
            rayOriginAOffset = raycastOrigins.bottomRight - new Vector2(offsetCorrection, 0);

            rayOriginB = raycastOrigins.bottomLeft;
            rayOriginBOffset = raycastOrigins.bottomLeft + new Vector2(offsetCorrection, 0);
        }

        RaycastHit2D hitA = Physics2D.Raycast(rayOriginA, direction, distance, collidableLayers);
        RaycastHit2D hitAOffset = Physics2D.Raycast(rayOriginAOffset, direction, distance, collidableLayers);


        RaycastHit2D hitB = Physics2D.Raycast(rayOriginB, direction, distance, collidableLayers);   
        RaycastHit2D hitBOffset = Physics2D.Raycast(rayOriginBOffset, direction, distance, collidableLayers);

        if (hitA && !hitAOffset)
        {
            print("SHOULD ADJUST A");
            Vector2 raycastStart = rayOriginAOffset + direction.normalized * distance;
            
            Vector2 raycastDirection = (rayOriginA - rayOriginAOffset).normalized;
            RaycastHit2D hit = Physics2D.Raycast(raycastStart, raycastDirection, offsetCorrection, collidableLayers);
            if (hit)
            {
                float distanceToShift = offsetCorrection - hit.distance + skinWidth;
                if (direction == Vector2.up) return new Vector2(distanceToShift, 0);
                if (direction == Vector2.right) return new Vector2(0, -distanceToShift);
                if (direction == Vector2.left) return new Vector2(0, distanceToShift);
                if (direction == Vector2.down) return new Vector2(-distanceToShift, 0);
            }
        }
        if (hitB && !hitBOffset)
        {
            print("SHOULD ADJUST B");
            Vector2 raycastStart = rayOriginBOffset + direction.normalized * distance;
            
            Vector2 raycastDirection = (rayOriginB - rayOriginBOffset).normalized;
            RaycastHit2D hit = Physics2D.Raycast(raycastStart, raycastDirection, offsetCorrection, collidableLayers);
            if (hit)
            {
                float distanceToShift = offsetCorrection - hit.distance + skinWidth;
                if (direction == Vector2.up) return new Vector2(-distanceToShift, 0);
                if (direction == Vector2.right) return new Vector2(0, distanceToShift);
                if (direction == Vector2.left) return new Vector2(0, -distanceToShift);
                if (direction == Vector2.down) return new Vector2(distanceToShift, 0);
            }
        }
        return Vector2.zero;
    }



    // private float FindMinShiftToClear(
    //     Vector2 origin,
    //     Vector2 originOffset,
    //     Vector2 castDir,
    //     float castDist,
    //     LayerMask mask,
    //     Collider2D colliderToClear
    // )
    // {
    //     Vector2 shiftVec = originOffset - origin;
    //     float maxShift = shiftVec.magnitude;
    //     if (maxShift <= 0f)
    //     {
    //         return 0f;
    //     }
    //     Vector2 shiftDir = shiftVec / maxShift;

    //     float low = 0.1f;
    //     float high = maxShift;

    //     RaycastHit2D startHit = Physics2D.Raycast(origin, castDir, castDist, mask);
    // }

    public void UpdateRaycastOrigins()
    {
        Bounds bounds = boxCollider.bounds;
        bounds.Expand(skinWidth * -2);

        raycastOrigins.topRight = new Vector2(bounds.max.x, bounds.max.y);
        raycastOrigins.topLeft = new Vector2(bounds.min.x, bounds.max.y);
        raycastOrigins.bottomLeft = new Vector2(bounds.min.x, bounds.min.y);
        raycastOrigins.bottomRight = new Vector2(bounds.max.x, bounds.min.y);


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
}
