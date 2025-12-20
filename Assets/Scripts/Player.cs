using UnityEditor.Experimental.GraphView;
using UnityEngine;

[RequireComponent(typeof(BoxCollider2D))]
[RequireComponent(typeof(RaycastController))]
public class Player : MonoBehaviour
{
    private RaycastController raycastController;
    public Vector2 move;
    public float moveSpeed = 1f;
    public LayerMask collidableLayers;
    private BoxCollider2D collider;
    private Rigidbody2D rb;
    void OnEnable()
    {
        GameController.Instance.Input.MovePressed += OnMovePressed;
    }

    void OnDisable()
    {
        GameController.Instance.Input.MovePressed -= OnMovePressed;
    }
    void Awake()
    {
        raycastController = GetComponent<RaycastController>();
        collider = GetComponent<BoxCollider2D>();
        rb = GetComponent<Rigidbody2D>();
    }
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {
        // transform.rotation = Quaternion.FromToRotation(Vector3.right, move);
    }

    void FixedUpdate()
    {

        RaycastControllerResult result = raycastController.CastRays(move, moveSpeed);
        if (result.hit)
        {
            //Check and adjust if barely hitting wall
            Vector2 shiftAmmount = raycastController.GetOffsetCorrection(move, moveSpeed);
            if (shiftAmmount != Vector2.zero)
            {
                rb.MovePosition(rb.position + shiftAmmount + (move * moveSpeed));
            }
            else
            {
                rb.MovePosition(rb.position + (move * result.distance));
                move = Vector2.zero;
            }
        }
        else
        {
            rb.MovePosition(rb.position + (move * moveSpeed));
        }
    }

    void OnMovePressed(Vector2 moveInput)
    {
        if (Vector2.Dot(move, moveInput) < 0f) return;
        move = moveInput;
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
