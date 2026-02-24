using System.Collections;
using Unity.VisualScripting;
using UnityEngine;

[RequireComponent(typeof(BoxCollider2D))]
[RequireComponent(typeof(CastHandler))]
public class Player : MonoBehaviour
{
    private CastHandler castHandler;
    public Vector2 move;
    public float moveSpeed = 1f;
    public float chargeSpeed = 1f;
    public LayerMask collidableLayers;
    [HideInInspector] public Rigidbody2D rb;
    private TrailRenderer trailRenderer;
    private bool canCharge = true;
    [HideInInspector] public bool isCharging = false;
    [SerializeField] private float chargePressedBuffer = 0.2f;
    [SerializeField] private float chargePressedBufferInMotion = 0.05f;
    private float chargePressedCountdown = 0;
    [SerializeField] private float movePressedBuffer = 0.1f;
    private float movePressedBufferCountdown = 0;
    private Vector2 lastMovePressed = Vector2.zero;

    private Vector2 startPosition;

    void OnEnable()
    {
        GameController.Instance.Input.MovePressed += OnMovePressed;
        GameController.Instance.Input.ChargePressed += OnChargePressed;
    }

    void OnDisable()
    {
        GameController.Instance.Input.MovePressed -= OnMovePressed;
        GameController.Instance.Input.ChargePressed -= OnChargePressed;
    }
    void Awake()
    {
        castHandler = GetComponent<CastHandler>();
        rb = GetComponent<Rigidbody2D>();
        trailRenderer = GetComponent<TrailRenderer>();
        trailRenderer.time = 0.15f;
    }
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        startPosition = transform.position;
    }

    void Update()
    {
        if (chargePressedCountdown > 0) chargePressedCountdown -= Time.deltaTime;
        if (movePressedBufferCountdown > 0)
        {
            movePressedBufferCountdown -= Time.deltaTime;
        }
        else
        {
            movePressedBufferCountdown = 0;
            lastMovePressed = Vector2.zero;
        }
    }

    void FixedUpdate()
    {
        float speed = isCharging ? chargeSpeed : moveSpeed;
        Vector2 desiredDelta = move.magnitude * speed * Time.fixedDeltaTime * move.normalized;

        Vector2 offsetCorrection = castHandler.GetOffsetCorrection(move.normalized, desiredDelta.magnitude);
        // if(offsetCorrection != Vector2.zero) print(offsetCorrection);
        desiredDelta = desiredDelta + offsetCorrection;

        Vector2 resolved = ResolveWithSliding(desiredDelta, 2, out bool hitWall);
        // print(resolved.magnitude);
        if (hitWall && offsetCorrection==Vector2.zero)
        {
            move = Vector2.zero;
            isCharging = false;
            trailRenderer.time = 0.15f;

            canCharge = true;
        }
        rb.MovePosition(rb.position + resolved);
    }

    Vector2 ResolveWithSliding(Vector2 delta, int maxIterations, out bool hitWall)
    {
        hitWall = false;
        Vector2 remaining = delta;
        Vector2 totalMoved = Vector2.zero;
        Vector2 virtualCenter = castHandler.boxCollider.bounds.center;
        for (int i = 0; i < maxIterations; i++)
        {
            if (remaining.sqrMagnitude < 1e-10f) break;

            Vector2 direction = remaining.normalized;
            float distance = remaining.magnitude;

            CastControllerResult hit = castHandler.CastBox(virtualCenter, direction, distance);

            if (!hit.hit)
            {
                totalMoved += remaining;
                break;
            }

            //Check if we hit a wall
            float oppose = Vector2.Dot(delta.normalized, hit.normal);

            if (oppose <= -0.9 || oppose == 0) hitWall = true;

            Vector2 moveToHit = direction * hit.distance;
            totalMoved += moveToHit;
            virtualCenter += moveToHit;

            Vector2 leftover = remaining - moveToHit;

            remaining = leftover - hit.normal * Vector2.Dot(leftover, hit.normal);

            if (remaining.sqrMagnitude < 1e-8f) break;

        }
        return totalMoved;
    }

    void Charge()
    {
        canCharge = false;
        isCharging = true;
        trailRenderer.time = 0.2f;


    }

    public void RefundCharge()
    {
        canCharge = true;
    }

    public void SnapToPosition(Vector2 position)
    {
        rb.position = position;
    }

    void OnMovePressed(Vector2 moveInput)
    {
        movePressedBufferCountdown = movePressedBuffer;
        lastMovePressed = moveInput;

        if (
            Vector2.Dot(move, moveInput) < 0f ||
            isCharging && move != Vector2.zero && chargePressedCountdown <= 0
            )
        {
            return;
        }

        move = moveInput;
        if (chargePressedCountdown > 0)
        {
            Charge();
        }
    }

    void OnChargePressed()
    {
        if (!canCharge) return;

        // Buffer For Pressing Charge and then Move
        chargePressedCountdown = move == Vector2.zero ? chargePressedBuffer : chargePressedBufferInMotion;

        if (move == Vector2.zero)
        {
            return;
        }
        else if (isCharging && lastMovePressed != Vector2.zero)
        {
            chargePressedCountdown = chargePressedBuffer;
            if (Vector2.Dot(move, lastMovePressed) >= 0f) move = lastMovePressed;
            if (chargePressedCountdown > 0)
            {
                isCharging = true;
                canCharge = false;
            }
            return;
        }
        Charge();
    }

    // ====================================

    public void DebugKillPlayer()
    {
        trailRenderer.Clear();;
        transform.position = startPosition;
        isCharging = false;

        canCharge = true;
        move = Vector2.zero;
    }



    public struct CollisionInfo
    {
        public bool above, below, left, right;

        public void Reset()
        {
            above = false;
            below = false;
            left = false;
            right = false;
        }
    }
}
