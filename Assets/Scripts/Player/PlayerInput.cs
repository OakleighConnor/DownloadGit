using UnityEngine;
using UnityEngine.Events;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;

public class PlayerInputScript : MonoBehaviour
{
    public Vector2 movementInput;
    public bool jumpInput;
    public bool actionInput;

    public UnityEvent pauseGameEvent;

    void Awake()
    {
        if (pauseGameEvent == null)
        {
            pauseGameEvent = new UnityEvent();
        }

        if (SceneManager.GetActiveScene().name == "Level")
        {
            Debug.Log("Level found");
            PauseMenuHandler pauseMenu = FindAnyObjectByType<PauseMenuHandler>();
            pauseGameEvent.AddListener(pauseMenu.TogglePanel);
        }
    }
    void OnEnable()
    {
        SceneManager.sceneLoaded += OnSceneLoaded;
    }
    void OnSceneLoaded(Scene scene, LoadSceneMode mode) // Adds the ToggleSettings method as a listener of pauseGameEvent
    {
        Awake();
    }
    public void OnMove(InputAction.CallbackContext context)
    {
        movementInput = context.ReadValue<Vector2>();
    }

    public void OnJump(InputAction.CallbackContext context)
    {
        jumpInput = context.ReadValueAsButton();
    }

    public void OnAction(InputAction.CallbackContext context)
    {
        actionInput = context.ReadValueAsButton();
    }

    public void OnPause(InputAction.CallbackContext context)
    {
        Debug.Log("Pause");
        pauseGameEvent.Invoke();
    }
}
