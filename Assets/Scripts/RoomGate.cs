using System.Collections.Generic;
using UnityEngine;

public class RoomGate : MonoBehaviour
{
    private int defaultLayer;
    private int groundLayer; 

    private List<Enemy> enemiesTracked = new();
    private Collider2D col;
    private SpriteRenderer sr;

    void Awake()
    {
        sr = GetComponent<SpriteRenderer>();
        col = GetComponent<Collider2D>();
        sr.enabled = false;
        defaultLayer = LayerMask.NameToLayer("Default");
        groundLayer = LayerMask.NameToLayer("Ground");

        Enemy[] enemiesInRoom = transform.parent.GetComponentsInChildren<Enemy>();
        foreach (Enemy enemy in enemiesInRoom)
        {
            if (enemy.vulnerableFrom != HittableDirections.None)
            {
                enemiesTracked.Add(enemy);
            }
        }
    }
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    void OnEnable()
    {
        GameController.Instance.OnEnemyKilled += OnEnemyKilled;
        if(IsPlayerInGate()) return;
        SetToGroundLayer();
    }

    void OnDisable()
    {
        GameController.Instance.OnEnemyKilled -= OnEnemyKilled;
        ResetToDefaultLayer();
    }

    void OnTriggerExit2D(Collider2D collision)
    {
        if (collision.CompareTag("Player")) SetToGroundLayer();
    }

    private void OnEnemyKilled(Enemy enemy)
    {
        enemiesTracked.Remove(enemy);
        if (enemiesTracked.Count <= 0)
        {
            Destroy(gameObject);
        }
    }

    public void ResetToDefaultLayer()
    {
        print("OFF");
        gameObject.layer = defaultLayer;
        if (sr != null) sr.enabled = false;   
    }

    public void SetToGroundLayer()
    {
        print("ON");
        gameObject.layer = groundLayer;
        sr.enabled = true;   
    }

    private bool IsPlayerInGate()
   {
      
       Collider2D[] hits = Physics2D.OverlapBoxAll(
           col.bounds.center,
           col.bounds.size,
           0f
       );


       foreach (Collider2D hit in hits)
       {
           if (hit.CompareTag("Player")) return true;
       }


       return false;
   }

}
