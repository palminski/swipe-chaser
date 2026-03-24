using UnityEngine;
using UnityEngine.InputSystem;
using System;

public class InputController : MonoBehaviour
{
    
    public Vector2 Move {get; private set;}
    // public event Action<Vector2> MoveChanged;
    public event Action<Vector2> MovePressed;
    public event Action<Vector2> MoveReleased;
    public event Action AttackPressed;
    public event Action ChargePressed;

    // public void OnMove(InputValue value)
    // {
    //     Move = value.Get<Vector2>();
    //     MoveChanged?.Invoke(Move);
    // }

    public void OnUp(InputValue input)
    {
        if (input.isPressed) MovePressed?.Invoke(Vector2.up);
    }
    public void OnDown(InputValue input)
    {
        if (input.isPressed) MovePressed?.Invoke(Vector2.down);
    }
    public void OnLeft(InputValue input)
    {
        if (input.isPressed) MovePressed?.Invoke(Vector2.left);
    }
    public void OnRight(InputValue input)
    {
        if (input.isPressed) MovePressed?.Invoke(Vector2.right);
    }
    public void OnAttack(InputValue input)
    {
        if (input.isPressed) AttackPressed?.Invoke();
    }

    public void OnCharge(InputValue input)
    {
        if (input.isPressed) ChargePressed?.Invoke();
    }

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
