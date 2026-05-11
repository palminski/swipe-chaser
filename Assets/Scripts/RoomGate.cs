using System.Collections.Generic;
using UnityEngine;

public class RoomGate : MonoBehaviour
{
    private int defaultLayer;
    private int groundLayer; 

    private List<Enemy> enemiesTracked = new();

    private SpriteRenderer sr;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        sr = GetComponent<SpriteRenderer>();
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

    void OnEnable()
    {
        GameController.Instance.OnEnemyKilled += OnEnemyKilled;
    }

    void OnDisable()
    {
        GameController.Instance.OnEnemyKilled -= OnEnemyKilled;
    }

    void OnTriggerExit2D(Collider2D collision)
    {
        RoomGate[] gates = transform.parent.GetComponentsInChildren<RoomGate>();
        foreach (RoomGate gate in gates)
        {
            gate.SetToGroundLayer();
        }
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
        gameObject.layer = defaultLayer;
        if (sr != null) sr.enabled = false;   
    }

    public void SetToGroundLayer()
    {
        gameObject.layer = groundLayer;
        sr.enabled = true;   
    }
}
