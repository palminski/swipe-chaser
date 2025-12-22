using Unity.VisualScripting;
using UnityEngine;

[RequireComponent(typeof(BoxCollider2D))]
[RequireComponent(typeof(RaycastController))]
public class Player : MonoBehaviour
{
    private RaycastController raycastController;
    public Vector2 move;
    public float moveSpeed = 1f;
    public LayerMask collidableLayers;
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
        rb = GetComponent<Rigidbody2D>();
    }
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {

    }

    // Update is called once per frame
    // void Update()
    // {
    //     float distance = moveSpeed*Time.deltaTime;
    //     RaycastControllerResult result = raycastController.CastRays(move, distance);
    //     if (result.hit)
    //     {
    //         //Check and adjust if barely hitting wall
    //         Vector2 shiftAmmount = raycastController.GetOffsetCorrection(move, distance);
    //         if (shiftAmmount != Vector2.zero)
    //         {
    //             transform.Translate(shiftAmmount, Space.World);
    //         }
    //         else
    //         {    
    //             transform.Translate(move * result.distance, Space.World);
    //             move = Vector2.zero;
    //         }
    //     }
    //     else
    //     {
    //         transform.Translate(move * distance, Space.World);
    //     }
    // }

    void FixedUpdate()
    {
        if (move == Vector2.zero) return;
        Vector2 translateAmount = Vector2.zero;
        raycastController.UpdateRaycastOrigins();

        // Check For Wall Correction
        Vector2 shiftAmmount = raycastController.GetOffsetCorrection(move, moveSpeed);
        if (shiftAmmount != Vector2.zero)
        {
            translateAmount = shiftAmmount;
            raycastController.TranslateRaycastOrigins(translateAmount);
        }

        //Collision Check For Wall
        RaycastControllerResult result = raycastController.CastRays(move, moveSpeed);
        if (result.hit)
        {
            //Handle Angled Walls
            if (Vector2.Dot(result.normal, move) > -0.95f)
            {
                translateAmount += move * result.distance;
                raycastController.TranslateRaycastOrigins(move * result.distance);

                //Calculate tangent to slide across
                Vector2 normal = result.normal;
                Vector2 tangent = new Vector2(-normal.y, normal.x);
                if (Vector2.Dot(tangent, move) < 0f) tangent = -tangent;
                
                //Check to see if we will hit a wall when sliding across tangent
                RaycastControllerResult tangentCastResult = raycastController.CastRays(tangent, moveSpeed, move);
                if (tangentCastResult.hit)
                {
                    translateAmount += tangent * tangentCastResult.distance;
                }
                //Check to make sure we snap back to ground if we would slide over the ground
                else
                {
                    translateAmount += tangent * moveSpeed;
                    raycastController.TranslateRaycastOrigins(tangent * moveSpeed);
                    Vector2 snapDirection = -normal;
                    RaycastControllerResult snapResults = raycastController.CastRays(snapDirection, 0.2f, move);
                    if(snapResults.hit)
                    {
                        translateAmount += snapDirection * snapResults.distance;
                    }
                }
            }
            // No Slope
            else
            {
                translateAmount += move * result.distance;
                move = Vector2.zero;
            }
        }
        // No Wall
        else
        {
            translateAmount += move * moveSpeed;
        }
        //Move RB
        rb.MovePosition(rb.position + translateAmount);
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
