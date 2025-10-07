using UnityEngine;
using UnityEngine.UIElements;
using UnityEngine.SceneManagement;

public class StartMenuController : MonoBehaviour
{
    private VisualElement mainPanel, settingsPanel;
    private Button playButton, settingsButton, exitButton, backButton;
    private (SliderInt slider, Label label, System.Action<float> setter)[] volumeControls;

    void OnEnable()
    {
        var root = GetComponent<UIDocument>().rootVisualElement;

        //panels
        mainPanel = root.Q<VisualElement>("MAIN");
        settingsPanel = root.Q<VisualElement>("SETTINGS");

        //buttons
        playButton = root.Q<Button>("PlayButton");
        settingsButton = root.Q<Button>("SettingsButton");
        exitButton = root.Q<Button>("ExitButton");
        backButton = root.Q<Button>("BackButton");

        playButton.clicked += () => SceneManager.LoadScene("Prototype");
        settingsButton.clicked += ShowSettings;
        exitButton.clicked += () => { Application.Quit(); Debug.Log("Exit game"); };
        backButton.clicked += ShowMain;

        //volume control
        volumeControls = new[]
        {
            (root.Q<SliderInt>("MainVolume"), root.Q<Label>("MainVolumeLabel"), (System.Action<float>)VolumeManager.Instance.SetMainVolume),
            (root.Q<SliderInt>("DrumVolume"), root.Q<Label>("DrumVolumeLabel"), (System.Action<float>)VolumeManager.Instance.SetDrumVolume),
            (root.Q<SliderInt>("BassVolume"), root.Q<Label>("BassVolumeLabel"), (System.Action<float>)VolumeManager.Instance.SetBassVolume)
        };

        InitializeVolumeControls();

        ShowMain();
    }

    private void InitializeVolumeControls()
    {
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

    private void ShowSettings()
    {
        mainPanel.style.display = DisplayStyle.None;
        settingsPanel.style.display = DisplayStyle.Flex;

        //refresh label values
        foreach (var (slider, label, _) in volumeControls)
            label.text = slider.value.ToString();
    }

    private void ShowMain()
    {
        mainPanel.style.display = DisplayStyle.Flex;
        settingsPanel.style.display = DisplayStyle.None;
    }
}