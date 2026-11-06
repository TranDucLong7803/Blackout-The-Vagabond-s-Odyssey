using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.Events;
using UnityEngine.SceneManagement;

public class UIManager : MonoBehaviour
{
    private IUIScreen openedScreen;
    public IUIScreen OpenedScreen => openedScreen is Object obj && obj == null ? null : openedScreen;
    public UnityEvent<float,float> healthBarEvent;
    public UnityEvent<float,float> shieldBarEvent;

    #region Singleton
    public static UIManager Instance {get; private set;}
    private void Awake()
    {
        if(Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        DontDestroyOnLoad(gameObject);

        if(healthBarEvent == null) healthBarEvent = new();
        if(shieldBarEvent == null) shieldBarEvent = new();
    }
    #endregion

    public void ToggleScreen(IUIScreen screen)
    {
        if (screen == null || (screen is Object obj && obj == null)) return;

        if (OpenedScreen == screen)
        {
            CloseCurrentScreen();
            return;
        }

        if (Time.timeScale <= 0f) return;

        CloseCurrentScreen();
        openedScreen = screen;
        screen.Open();
        if (AimPoint.Instance != null) AimPoint.Instance.DisableAim();
    }

    public void CloseCurrentScreen()
    {
        var screen = OpenedScreen;
        openedScreen = null;
        if (screen == null) return;

        if (InputManager.Instance != null) InputManager.Instance.SuppressHeldAttack();
        if (EventSystem.current != null) EventSystem.current.SetSelectedGameObject(null);
        screen.Close();
        if (AimPoint.Instance != null) AimPoint.Instance.EnableAim();
    }

    private void OnEnable()
    {
        if (Instance == this) SceneManager.sceneLoaded += OnSceneLoaded;
    }

    private void OnDisable()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        CloseCurrentScreen();
    }
}
