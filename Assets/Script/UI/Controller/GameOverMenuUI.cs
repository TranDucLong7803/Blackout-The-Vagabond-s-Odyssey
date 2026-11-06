using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;

public class GameOverMenuUI : MonoBehaviour
{
    public static GameOverMenuUI Instance { get; private set; }

    [SerializeField] GameObject gameoverMenu;
    [SerializeField] TextMeshProUGUI stageLevelText;

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else Destroy(gameObject);
    }

    void Start()
    {
        gameoverMenu.SetActive(false);
    }

    public void Show()
    {
        if (UIManager.Instance != null) UIManager.Instance.CloseCurrentScreen();
        if (AimPoint.Instance != null) AimPoint.Instance.DisableAim();
        stageLevelText.text = $"You made it to Stage: {GameManager.Instance.currentStage}, Level: {GameManager.Instance.currentLevel}";
        gameoverMenu.SetActive(true);
        Time.timeScale = 0f;
    }

    public void BackToHome()
    {
        Time.timeScale = 1f;
        DontDestroyCleaner.ClearDDOL();
        SceneManager.LoadScene("Home");
        gameObject.SetActive(false);
    }

}
