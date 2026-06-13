using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class SettingsPopupController : MonoBehaviour
{
    [Header("UI Root")]
    [SerializeField] private GameObject popupRoot;

    [Header("Volume Sliders")]
    [SerializeField] private Slider musicSlider;
    [SerializeField] private Slider sfxSlider;

    [Header("Buttons")]
    [SerializeField] private Button continueButton;
    [SerializeField] private Button mainMenuButton;
    [SerializeField] private Button resetButton;

    public static bool IsOpen { get; private set; }

    private void Awake()
    {
        // Bind button actions
        if (continueButton != null) continueButton.onClick.AddListener(Continue);
        if (mainMenuButton != null) mainMenuButton.onClick.AddListener(GoToMainMenu);
        if (resetButton != null) resetButton.onClick.AddListener(ResetData);

        // Bind slider actions
        if (musicSlider != null) musicSlider.onValueChanged.AddListener(OnMusicVolumeChanged);
        if (sfxSlider != null) sfxSlider.onValueChanged.AddListener(OnSFXVolumeChanged);
    }

    private void OnEnable()
    {
        IsOpen = true;
        InitializeVolumeSliders();
    }

    private void OnDisable()
    {
        IsOpen = false;
    }

    private void InitializeVolumeSliders()
    {
        if (AudioManager.Instance != null)
        {
            if (musicSlider != null)
            {
                // Temporarily disable listener to prevent triggering event during initialization
                musicSlider.onValueChanged.RemoveListener(OnMusicVolumeChanged);
                musicSlider.value = AudioManager.Instance.musicVolume;
                musicSlider.onValueChanged.AddListener(OnMusicVolumeChanged);
            }

            if (sfxSlider != null)
            {
                sfxSlider.onValueChanged.RemoveListener(OnSFXVolumeChanged);
                sfxSlider.value = AudioManager.Instance.sfxVolume;
                sfxSlider.onValueChanged.AddListener(OnSFXVolumeChanged);
            }
        }
    }

    public void Open()
    {
        IsOpen = true;
        if (popupRoot != null)
        {
            popupRoot.SetActive(true);
            InitializeVolumeSliders();
        }
        else
        {
            gameObject.SetActive(true);
        }
    }

    public void Close()
    {
        IsOpen = false;
        if (popupRoot != null)
        {
            popupRoot.SetActive(false);
        }
        else
        {
            gameObject.SetActive(false);
        }
    }

    private void OnMusicVolumeChanged(float value)
    {
        if (AudioManager.Instance != null)
        {
            AudioManager.Instance.SetMusicVolume(value);
        }
    }

    private void OnSFXVolumeChanged(float value)
    {
        if (AudioManager.Instance != null)
        {
            AudioManager.Instance.SetSFXVolume(value);
        }
    }

    public void Continue()
    {
        Close();
    }

    public void GoToMainMenu()
    {
        if (SceneManager.GetActiveScene().name == "MainMenu")
        {
            Close(); // Nếu đã ở sẵn MainMenu, chỉ cần ẩn bảng Settings đi
        }
        else
        {
            if (SceneController.Instance != null)
            {
                SceneController.Instance.LoadScene("MainMenu");
            }
            else
            {
                SceneManager.LoadScene("MainMenu");
            }
        }
    }

    public void ResetData()
    {
        PlayerPrefs.DeleteKey("UnlockedChapter");
        PlayerPrefs.Save();
        if (Inventory.Instance != null)
        {
            Inventory.Instance.ClearAll();
        }
        if (PuzzleManager.Instance != null)
        {
            PuzzleManager.Instance.ClearAllStates();
        }
        Scene activeScene = SceneManager.GetActiveScene();
        if (SceneController.Instance != null)
        {
            SceneController.Instance.LoadScene(activeScene.name);
        }
        else
        {
            SceneManager.LoadScene(activeScene.name);
        }
    }
}
