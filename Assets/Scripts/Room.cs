using UnityEngine;

public class Room : MonoBehaviour
{
    private GameCamera gameCamera;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        SpriteRenderer spriteRenderer = GetComponent<SpriteRenderer>();
        spriteRenderer.enabled = false;
        gameCamera = Camera.main.GetComponent<GameCamera>();
        if(gameCamera == null)
        {
            Debug.LogWarning("GameCamera Not Found!");
            
        }
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    void OnTriggerExit2D(Collider2D collision)
    {
        if(gameCamera != null && collision.CompareTag("Player"))
        {
            gameCamera.SetTargetRoomBasedOnPlayerPosition();
            GameController.Instance.SetActiveRoomBasedOnPlayerPosition();
        }
    }

    public void ActivateChildObjects()
    {
        foreach (Transform child in transform)
        {
            child.gameObject.SetActive(true);
        }
    }

    public void DeactivateChildObjects()
    {
        foreach (Transform child in transform)
        {
            child.gameObject.SetActive(false);
        }
    }
}
