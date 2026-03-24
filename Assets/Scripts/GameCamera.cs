using UnityEngine;

public class GameCamera : MonoBehaviour
{
    private Transform target;
    private GameObject playerGameObject;
    private Vector3 velocity = Vector3.zero;
    [Header("Easing")]
    [SerializeField] private float easeTimeX = 0.2f;
    [SerializeField] private float easeTimeY = 0.2f;

    
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        playerGameObject = GameObject.FindGameObjectWithTag("Player");
        SetTargetRoomBasedOnPlayerPosition();
    }

    // Update is called once per frame
    void Update()
    {
        if(target != null)
        {
            EaseToTarget();
        }
    }

    private void EaseToTarget()
    {
        Vector3 targetPosition = target.position;

        Vector3 xVector = new(transform.position.x, 0, 0);
        Vector3 targetXVector = new(targetPosition.x, 0, 0);

        Vector3 yVector = new(0, transform.position.y, 0);
        Vector3 targetYVector = new(0, targetPosition.y, 0);

        float xTarget = Vector3.SmoothDamp(xVector, targetXVector, ref velocity, easeTimeX).x;
        float yTarget = Vector3.SmoothDamp(yVector, targetYVector, ref velocity, easeTimeY).y;


        transform.position = new(xTarget, yTarget, transform.position.z);
    }

    public void SetTargetRoomBasedOnPlayerPosition()
    {
        if(playerGameObject  == null)
        {
            Debug.LogWarning("Player Not Found!");
            return;
        }
        Collider2D[] hits = Physics2D.OverlapPointAll(playerGameObject.transform.position);
        foreach (Collider2D hit in hits)
        {
            Room room = hit.GetComponent<Room>();
            if (room != null)
            {
                SetTarget(room.transform);
            }
        }
    }

    public void SetTarget(Transform transform)
    {
        target = transform;
    }
}
