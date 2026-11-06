using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class MainMenuController : MonoBehaviour
{
    [SerializeField] GameObject tutorialPanel;
    [SerializeField] private GameObject mainMenuPanel;
    [SerializeField] private Button settingsButton;
    private ControlsMenuUI controlsMenu;
    private static bool isGameStarted = false;

    void Start()
    {
        if(!isGameStarted)
        {
            isGameStarted = true;
            DataManager.Load();
        }
        //settingsMenu.SetActive(false); 
        tutorialPanel.SetActive(false);
        //PlayerPrefs.SetInt("HasWatchedCutscene", 0);
    }

    public void PlayGame()
    {
        if (PlayerPrefs.GetInt("HasWatchedCutscene", 0) == 0)
        {
            
            SceneManager.LoadScene("Cutscene_BeforeBlackout");
        }
        else
        {
            Destroy(GameObject.Find("HomeBGM"));
            SceneManager.LoadScene("Home");
        }
    }

    public void Exit()
    {
        Application.Quit();
    }

    public void OpenSettings()
    {
        if (controlsMenu == null)
            controlsMenu = ControlsMenuUI.Create((RectTransform)transform, mainMenuPanel, settingsButton);
        controlsMenu.Show();
    }

    private void Update()
    {
        if (controlsMenu != null && InputManager.Instance.playerInput.Menu.Cancel.WasPressedThisFrame())
            controlsMenu.TryHandleCancel();
    }

    void OnApplicationQuit()
    {
        DataManager.Save();
    }

}
