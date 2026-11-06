using UnityEngine;
using UnityEngine.InputSystem;

public class InputManager : MonoBehaviour
{
    public PlayerControls playerInput { get; private set; }
    public static InputManager Instance { get; private set; }

    private bool suppressAttackUntilReleased;
    private int lastInteractionFrame = -1;

    public static bool GameplayBlocked => Time.timeScale <= 0f
        || (UIManager.Instance != null && UIManager.Instance.OpenedScreen != null);

    public bool IsAttackHeld => !suppressAttackUntilReleased && playerInput.Ingame.Attack.IsPressed();

    public static Vector2 PointerPosition => Pointer.current != null
        ? Pointer.current.position.ReadValue()
        : new Vector2(Screen.width, Screen.height) * 0.5f;

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
    private static void Initialize()
    {
        if (Instance == null)
            new GameObject(nameof(InputManager)).AddComponent<InputManager>();
    }

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        DontDestroyOnLoad(gameObject);
        playerInput = new PlayerControls();
    }

    private void OnEnable()
    {
        if (playerInput == null) return;

        playerInput.Ingame.Enable();
        playerInput.Menu.Enable();
    }

    private void Update()
    {
        if (suppressAttackUntilReleased && !playerInput.Ingame.Attack.IsPressed())
            suppressAttackUntilReleased = false;
    }

    public void SuppressHeldAttack()
    {
        suppressAttackUntilReleased = playerInput.Ingame.Attack.IsPressed();
    }

    public bool TryConsumeInteract()
    {
        if (GameplayBlocked || lastInteractionFrame == Time.frameCount
            || !playerInput.Ingame.Interact.WasPressedThisFrame()) return false;

        lastInteractionFrame = Time.frameCount;
        return true;
    }

    private void OnDisable()
    {
        playerInput?.Disable();
    }

    private void OnDestroy()
    {
        if (Instance != this) return;

        Instance = null;
        playerInput.Dispose();
    }
}
