using UnityEngine;
using UnityEngine.UIElements;
using UnityEngine.SceneManagement;
using UnityEngine.InputSystem;

public class StartMenuController : MonoBehaviour
{
    private VisualElement mainPanel, settingsPanel, levelsPanel;
    private Button playButton, settingsButton, levelsButton, exitButton, settingsBackButton, levelsBackButton;
    private (SliderInt slider, Label label, System.Action<float> setter)[] volumeControls;
    public InputAction MenuAction;

    private bool IsMainMenu => SceneManager.GetActiveScene().name == "MainMenu";

    void OnEnable()
    {
        var root = GetComponent<UIDocument>().rootVisualElement;

        InitPanels(root);
        InitButtons(root);
        InitVolumeControls(root);

        MenuAction.Enable();
        MenuAction.performed += OnMenuPressed;

        if (IsMainMenu) ShowMainMenu();
        else HideMainMenu();
    }

    void OnDisable()
    {
        MenuAction.performed -= OnMenuPressed;
        MenuAction.Disable();
    }

    #region Initialization
    private void InitPanels(VisualElement root)
    {
        mainPanel = root.Q<VisualElement>("MAIN");
        settingsPanel = root.Q<VisualElement>("SETTINGS");
        levelsPanel = root.Q<VisualElement>("LEVELS");
    }

    private void InitButtons(VisualElement root)
    {
        playButton = root.Q<Button>("PlayButton");
        settingsButton = root.Q<Button>("SettingsButton");
        levelsButton = root.Q<Button>("LevelsButton");
        exitButton = root.Q<Button>("ExitButton");

        settingsBackButton = root.Q<Button>("SettingsBackButton");
        levelsBackButton = root.Q<Button>("LevelsBackButton");

        var levelButtons = root.Query<Button>(className: "level-button").ToList();
        foreach (var btn in levelButtons)
        {
            btn.clicked += () => OnLevelButtonClicked(btn.name);
        }

        //set text based on scene
        exitButton.text = IsMainMenu ? "Exit Game" : "Exit to Main Menu";

        playButton.clicked += OnPlayClicked;
        settingsButton.clicked += ShowSettings;
        levelsButton.clicked += ShowLevels;
        settingsBackButton.clicked += ShowMainMenu;
        levelsBackButton.clicked += ShowMainMenu;
        exitButton.clicked += OnExitClicked;
    }

    private void InitVolumeControls(VisualElement root)
    {
        volumeControls = new[]
        {
            (root.Q<SliderInt>("MainVolume"), root.Q<Label>("MainVolumeLabel"), (System.Action<float>)VolumeManager.Instance.SetMainVolume),
            (root.Q<SliderInt>("DrumVolume"), root.Q<Label>("DrumVolumeLabel"), (System.Action<float>)VolumeManager.Instance.SetDrumVolume),
            (root.Q<SliderInt>("BassVolume"), root.Q<Label>("BassVolumeLabel"), (System.Action<float>)VolumeManager.Instance.SetBassVolume)
        };

        foreach (var (slider, label, setter) in volumeControls)
        {
            float initialValue = slider.name switch
            {
                "MainVolume" => VolumeManager.Instance.mainVolume,
                "DrumVolume" => VolumeManager.Instance.drumVolume,
                "BassVolume" => VolumeManager.Instance.bassVolume,
                _ => 0.5f
            };

            slider.value = Mathf.RoundToInt(initialValue * 100);
            label.text = slider.value.ToString();

            slider.RegisterValueChangedCallback(evt =>
            {
                setter(evt.newValue / 100f);
                label.text = evt.newValue.ToString();
            });
        }
    }
    #endregion

    #region Button Callbacks
    private void OnPlayClicked()
    {
        if (IsMainMenu)
            SceneManager.LoadScene("Tutorial");
        else
            HideMainMenu();
    }

    private void OnExitClicked()
    {
        if (IsMainMenu)
        {
            Application.Quit();
            Debug.Log("Exit game");
        }
        else
        {
            SceneManager.LoadScene("MainMenu");
        }
    }

    private void OnLevelButtonClicked(string buttonName)
    {
        string sceneName = buttonName.Replace("Button", "");

        if (sceneName.Contains("Level"))
            sceneName = sceneName.Replace("Level", "Level ");

        Debug.Log($"Loading scene: {sceneName}");
        
        SceneManager.LoadScene(sceneName);
    }
    #endregion

    private void OnMenuPressed(InputAction.CallbackContext context)
    {
        if (settingsPanel.style.display == DisplayStyle.Flex)
        {
            ShowMainMenu();
            return;
        }

        if (mainPanel.style.display == DisplayStyle.Flex)
            HideMainMenu();
        else
            ShowMainMenu();
    }

    #region Menu Display
    private void ShowSettings()
    {
        mainPanel.style.display = DisplayStyle.None;
        settingsPanel.style.display = DisplayStyle.Flex;
        levelsPanel.style.display = DisplayStyle.None;

        //refresh labels
        foreach (var (slider, label, _) in volumeControls)
            label.text = slider.value.ToString();
    }

    private void ShowLevels()
    {
        mainPanel.style.display = DisplayStyle.None;
        settingsPanel.style.display = DisplayStyle.None;
        levelsPanel.style.display = DisplayStyle.Flex;
        Time.timeScale = 0f;
    }

    private void ShowMainMenu()
    {
        mainPanel.style.display = DisplayStyle.Flex;
        settingsPanel.style.display = DisplayStyle.None;
        levelsPanel.style.display = DisplayStyle.None;
        Time.timeScale = 0f;
    }

    private void HideMainMenu()
    {
        mainPanel.style.display = DisplayStyle.None;
        settingsPanel.style.display = DisplayStyle.None;
        levelsPanel.style.display = DisplayStyle.None;
        Time.timeScale = 1f;
    }
    #endregion
}
