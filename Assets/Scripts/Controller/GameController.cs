using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;

[DefaultExecutionOrder(-100)]
[RequireComponent(typeof(InputController))]
public class GameController : MonoBehaviour
{
    public static GameController Instance { get; private set; }
    [HideInInspector] public InputController Input;
    [HideInInspector] public PathfindingGridController PathfindingGrid;

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            Input = GetComponent<InputController>();
            PathfindingGrid = GetComponent<PathfindingGridController>();
            PathfindingGrid.Build();
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        SetActiveRoomBasedOnPlayerPosition();
    }

    // Update is called once per frame
    void Update()
    {
        if (Keyboard.current.escapeKey.wasPressedThisFrame)
        {
            Application.Quit();
            #if UNITY_EDITOR
            UnityEditor.EditorApplication.isPlaying = false;
            #endif
        }

        if (Keyboard.current.rKey.wasPressedThisFrame)
        {
            SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
        }
    }

    public void SetActiveRoom()
    {
        
    }

    public void SetActiveRoomBasedOnPlayerPosition()
    {
        GameObject playerGameObject = GameObject.FindGameObjectWithTag("Player");

        if(playerGameObject  == null)
        {
            Debug.LogWarning("Player Not Found!");
            return;
        }
        Room[] rooms = FindObjectsByType<Room>(FindObjectsSortMode.None);
        int defaultLayer = LayerMask.NameToLayer("Default");
        int enemyBlockedLayer = LayerMask.NameToLayer("EnemyCanNotPass");
        foreach (Room room in rooms)
        {   
            room.DeactivateChildObjects();
            room.gameObject.layer = enemyBlockedLayer;
        }
        Collider2D[] hits = Physics2D.OverlapPointAll(playerGameObject.transform.position);
        foreach (Collider2D hit in hits)
        {
            Room room = hit.GetComponent<Room>();
            if (room != null)
            {
                room.gameObject.layer = defaultLayer;
                room.ActivateChildObjects();
            }
        }
        PathfindingGrid.Build();
    }
}
