using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class SettingsMenuUI : MonoBehaviour, IUIScreen
{
    public static SettingsMenuUI Instance { get; private set; }

    [SerializeField] private GameObject settingsPanel;
    [SerializeField] private Slider musicSlider;
    [SerializeField] private Slider sfxSlider;
    [SerializeField] private GameObject settingsContent;
    [SerializeField] private Button controlsButton;

    [SerializeField] private SoundMixerManager soundMixerManager;
    private ControlsMenuUI controlsMenu;

    private const string MusicKey = "MusicVolume";
    private const string SFXKey = "SoundFXVolume";



    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        if (transform.parent == null) DontDestroyOnLoad(gameObject);
    }

    private void Start()
    {
        if (settingsPanel != null) settingsPanel.SetActive(false);
        LoadSavedVolume();

        if (musicSlider != null) musicSlider.onValueChanged.AddListener(OnMusicSliderChanged);
        if (sfxSlider != null) sfxSlider.onValueChanged.AddListener(OnSFXSliderChanged);
    }

    public void Show()
    {
        if (settingsPanel != null && UIManager.Instance != null) UIManager.Instance.ToggleScreen(this);
    }

    public void Open()
    {
        settingsPanel.SetActive(true);
        Time.timeScale = 0f;

        var resumeButton = settingsPanel.GetComponentInChildren<Button>();
        if (resumeButton != null && EventSystem.current != null)
            EventSystem.current.SetSelectedGameObject(resumeButton.gameObject);
    }

    public void Close()
    {
        settingsPanel.SetActive(false);
        if (controlsMenu != null) controlsMenu.Hide();
        Time.timeScale = 1f;
    }

    public void ShowControls()
    {
        if (controlsMenu == null)
            controlsMenu = ControlsMenuUI.Create((RectTransform)settingsPanel.transform, settingsContent, controlsButton);
        controlsMenu.Show();
    }

    public bool TryHandleControlsCancel()
    {
        return controlsMenu != null && controlsMenu.TryHandleCancel();
    }

    public void ResumeGame()
    {
        if (UIManager.Instance != null && UIManager.Instance.OpenedScreen == this)
            UIManager.Instance.CloseCurrentScreen();
    }

    public void QuitGame()
    {
        ResumeGame();
        DontDestroyCleaner.ClearDDOL();
        SceneManager.LoadScene("Main Menu");
    }

    private void OnMusicSliderChanged(float value)
    {
        if (soundMixerManager != null) soundMixerManager.setMusicVolume(value);
        PlayerPrefs.SetFloat(MusicKey, value);
        //PlayerPrefs.Save();
    }

    private void OnSFXSliderChanged(float value)
    {
        if (soundMixerManager != null) soundMixerManager.setSoundFXVolume(value);
        PlayerPrefs.SetFloat(SFXKey, value);
        //PlayerPrefs.Save();
    }

    private void LoadSavedVolume()
    {
        float musicVolume = PlayerPrefs.GetFloat(MusicKey, 0.75f);  // default: 75%
        float sfxVolume = PlayerPrefs.GetFloat(SFXKey, 0.75f);

        if (musicSlider != null) musicSlider.value = musicVolume;
        if (sfxSlider != null) sfxSlider.value = sfxVolume;

        if (soundMixerManager != null)
        {
            soundMixerManager.setMusicVolume(musicVolume);
            soundMixerManager.setSoundFXVolume(sfxVolume);
        }
    }

}
