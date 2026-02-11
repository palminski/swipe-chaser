using System.Collections;
using Unity.VisualScripting;
using UnityEngine;

[RequireComponent(typeof(BoxCollider2D))]
[RequireComponent(typeof(CastHandler))]
public class Player : MonoBehaviour
{
    private RaycastController raycastController;
    private CastHandler castHandler;
    public Vector2 move;
    public float moveSpeed = 1f;
    public LayerMask collidableLayers;
    private Rigidbody2D rb;
    [SerializeField] private GameObject[] attackObjects;
    private int attackObjectIndex = 0;
    private Coroutine attackCoroutine;
    private Coroutine refractoryCoroutine;
    private bool canAttack = true;

    void OnEnable()
    {
        GameController.Instance.Input.MovePressed += OnMovePressed;
        GameController.Instance.Input.AttackPressed += OnAttackPressed;
    }

    void OnDisable()
    {
        GameController.Instance.Input.MovePressed -= OnMovePressed;
        GameController.Instance.Input.AttackPressed -= OnAttackPressed;
    }
    void Awake()
    {
        castHandler = GetComponent<CastHandler>();
        rb = GetComponent<Rigidbody2D>();
    }
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {

    }



    void FixedUpdate()
    {
        if (move == Vector2.zero) return;

        Vector2 desiredDelta = move.magnitude * moveSpeed * move.normalized;

        Vector2 resolved = ResolveWIthSliding(desiredDelta, 2, out bool hitWall);
        // print(resolved.magnitude);
        if (hitWall) move = Vector2.zero;
        rb.MovePosition(rb.position + resolved);
    }

    Vector2 ResolveWIthSliding(Vector2 delta, int maxIterations, out bool hitWall)
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
            print(oppose);
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
        if (Vector2.Dot(move, moveInput) < 0f) return;
        move = moveInput;
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
        refractoryCoroutine = null;
    }

    // ====================================



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
