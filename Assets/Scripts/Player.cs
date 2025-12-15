using UnityEditor.Experimental.GraphView;
using UnityEngine;

public class Player : MonoBehaviour
{
    public Vector2 move;
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
        
    }

    // Update is called once per frame
    void Update()
    {
        transform.rotation = Quaternion.FromToRotation(Vector3.right, move);
    }

    void OnMovePressed(Vector2 moveInput)
    {
        if(Vector2.Dot(move, moveInput) < 0f) return;
        move = moveInput;
    print($"Move Changed To: {moveInput}");
    }
}
