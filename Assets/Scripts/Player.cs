using UnityEditor.Experimental.GraphView;
using UnityEngine;

[RequireComponent(typeof(BoxCollider2D))]
public class Player : MonoBehaviour
{
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
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        collider = GetComponent<BoxCollider2D>();
        rb = GetComponent<Rigidbody2D>();
    }

    // Update is called once per frame
    void Update()
    {
        transform.rotation = Quaternion.FromToRotation(Vector3.right, move);
    }

    void FixedUpdate()
    {
        RaycastHit2D hit = Physics2D.Raycast(
            (Vector2)collider.bounds.center + (collider.bounds.extents * move.normalized),
            move.normalized,
            moveSpeed ,
            collidableLayers
        );

        if (hit.collider != null)
        {
            rb.MovePosition(rb.position + (move * hit.distance));
            move = Vector2.zero;
        }
        else
        {
            rb.MovePosition(rb.position + (move * moveSpeed));
        }
    }

    void OnMovePressed(Vector2 moveInput)
    {
        if(Vector2.Dot(move, moveInput) < 0f) return;
        move = moveInput;
    }
}
