using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class ShopUI : MonoBehaviour, IUIScreen
{
    public static ShopUI Instance { get; private set; }

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            gameObject.SetActive(false);
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    public void Show()
    {
        var ui = UIManager.Instance;
        if (ui != null && ui.OpenedScreen != this) ui.ToggleScreen(this);
    }

    public void Open()
    {
        gameObject.SetActive(true);
        Time.timeScale = 0f;

        var button = GetComponentInChildren<Button>();
        if (button != null && EventSystem.current != null)
            EventSystem.current.SetSelectedGameObject(button.gameObject);
    }

    public void Hide()
    {
        if (UIManager.Instance != null && UIManager.Instance.OpenedScreen == this)
            UIManager.Instance.CloseCurrentScreen();
    }

    public void Close()
    {
        gameObject.SetActive(false);
        Time.timeScale = 1f;
    }

    
}
