using System.Collections;
using Unity.VisualScripting;
using UnityEngine;

[RequireComponent(typeof(BoxCollider2D))]
[RequireComponent(typeof(CastHandler))]
public class PlayerAttackBoss : MonoBehaviour
{
    private CastHandler castHandler;
    public Vector2 move;
    public float moveSpeed = 1f;
    public float chargeSpeed = 1f;
    public LayerMask collidableLayers;
    private Rigidbody2D rb;
    [SerializeField] private GameObject[] attackObjects;
    private int attackObjectIndex = 0;
    private Coroutine attackCoroutine;
    // private Coroutine refractoryCoroutine;
    private bool canAttack = true;
    private bool canCharge = true;
    [HideInInspector] public bool isCharging = false;
    [SerializeField] private float chargePressedBuffer = 0.2f;
    private float chargePressedCountdown = 0;

    [SerializeField] private float movePressedBuffer = 0.1f;
    private float movePressedBufferCountdown = 0;
    private Vector2 lastMovePressed = Vector2.zero;


    private Vector2 startPosition;

    void OnEnable()
    {
        GameController.Instance.Input.MovePressed += OnMovePressed;
        GameController.Instance.Input.AttackPressed += OnAttackPressed;
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
        Vector2 desiredDelta = move.magnitude * speed * move.normalized;

        Vector2 resolved = ResolveWithSliding(desiredDelta, 2, out bool hitWall);
        // print(resolved.magnitude);
        if (hitWall)
        {
            move = Vector2.zero;
            isCharging = false;
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

    void OnMovePressed(Vector2 moveInput)
    {
        movePressedBufferCountdown = movePressedBuffer;
        lastMovePressed = moveInput;
        if (chargePressedCountdown <= 0 && Vector2.Dot(move, moveInput) < 0f || isCharging && move != Vector2.zero) return;

        move = moveInput;
        if (chargePressedCountdown > 0)
        {
            isCharging = true;
            canCharge = false;
        }
    }
    void OnChargePressed()
    {

        if (!canCharge) return;
        // Buffer For Pressing Charge and then Move
        if (move == Vector2.zero)
        {
            chargePressedCountdown = chargePressedBuffer;
            return;
        }
        else if (isCharging && lastMovePressed != Vector2.zero)
        {
            chargePressedCountdown = chargePressedBuffer;
            move = lastMovePressed;
            if (chargePressedCountdown > 0)
            {
                isCharging = true;
                canCharge = false;
            }
            return;
        }
        canCharge = false;
        isCharging = true;
    }
    void OnAttackPressed()
    {
        if (!canAttack || attackObjectIndex >= attackObjects.Length) return;
        if (attackCoroutine != null)
        {
            StopCoroutine(attackCoroutine);
            attackCoroutine = null;
        }
        foreach (GameObject attackObject in attackObjects)
        {
            attackObject.SetActive(false);
        }
        attackObjects[attackObjectIndex].SetActive(true);
        attackObjectIndex++;
        attackCoroutine = StartCoroutine(AttackCoroutine());
    }

    private IEnumerator AttackCoroutine()
    {
        yield return new WaitForSeconds(0.25f);
        foreach (GameObject attackObject in attackObjects)
        {
            attackObject.SetActive(false);
        }
        // if (refractoryCoroutine == null) refractoryCoroutine = StartCoroutine(RefractoryCoroutine());
        attackObjectIndex = 0;
    }

    private IEnumerator RefractoryCoroutine()
    {
        canAttack = false;
        yield return new WaitForSeconds(0.3f);
        canAttack = true;
        // refractoryCoroutine = null;
    }

    // ====================================

    public void DebugKillPlayer()
    {
        transform.position = startPosition;
        isCharging = false;
        canCharge = true;
        move = Vector2.zero;
    }

    public void AllowChargeDirectionChange(Vector2 position)
    {
        transform.position = position;
        canCharge = true;

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
