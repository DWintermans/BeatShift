using UnityEngine;
using UnityEngine.UIElements;
using System.Collections;
using System.Collections.Generic;

public class CutsceneController : MonoBehaviour
{
    private VisualElement introPanel, endingPanel, chargingPanel, blackPanel, bat1Panel, bat2Panel, bat3Panel, bat4Panel;
    private List<VisualElement> allPanels;
    private List<VisualElement> allBatPanels;
    private Coroutine currentFade;

    public static CutsceneController Instance;

    void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
    }

    void OnEnable()
    {
        var root = GetComponent<UIDocument>().rootVisualElement;
        InitPanels(root);
    }

    public void PlayCutScene(CutsceneAction action)
    {
        switch (action)
        {
            case CutsceneAction.HideAllPanels:
                HideAllPanels();
                break;
            case CutsceneAction.ShowBlackPanel:
                ShowBlackPanel();
                break;
            case CutsceneAction.ShowIntroPanel:
                ShowIntroPanel();
                break;
            case CutsceneAction.FadeOutOfBlackPanelShort:
                FadeOutOfBlackPanel(2f);
                break;
            case CutsceneAction.FadeOutOfBlackPanel:
                FadeOutOfBlackPanel(4f);
                break;
            case CutsceneAction.FadeToBlackPanelShort:
                FadeToBlackPanel(2f);
                break;
            case CutsceneAction.FadeToBlackPanel:
                FadeToBlackPanel(4f);
                break;
            case CutsceneAction.HideAllImagePanels:
                HideAllImagePanels();
                break;
            case CutsceneAction.ShowChargingPanel:
                ShowChargingPanel();
                break;
            case CutsceneAction.ShowEndingPanel:
                ShowEndingPanel();
                break;
            case CutsceneAction.ShowBAT1:
                ShowBat1Panel();
                break;
            case CutsceneAction.ShowBAT2:
                ShowBat2Panel();
                break;
            case CutsceneAction.ShowBAT3:
                ShowBat3Panel();
                break;
            case CutsceneAction.ShowBAT4:
                ShowBat4Panel();
                break;
            default:
                break;
        }
    }

    private void InitPanels(VisualElement root)
    {
        introPanel = root.Q<VisualElement>("INTRO");
        endingPanel = root.Q<VisualElement>("ENDING");
        chargingPanel = root.Q<VisualElement>("CHARGING");
        blackPanel = root.Q<VisualElement>("BLACK");
        bat1Panel = root.Q<VisualElement>("BAT_1");
        bat2Panel = root.Q<VisualElement>("BAT_2");
        bat3Panel = root.Q<VisualElement>("BAT_3");
        bat4Panel = root.Q<VisualElement>("BAT_4");

        allPanels = new List<VisualElement> { introPanel, endingPanel, chargingPanel, blackPanel, bat1Panel, bat2Panel, bat3Panel, bat4Panel };

        allBatPanels = new List<VisualElement> { bat1Panel, bat2Panel, bat3Panel, bat4Panel };

        blackPanel.style.display = DisplayStyle.None;
        blackPanel.style.opacity = 0f;
    }

    private void Show(VisualElement panelToShow)
    {
        panelToShow.style.display = DisplayStyle.Flex;
    }

    public void ShowBat1Panel()
    {
        HideAllBatPanels();
        Show(bat1Panel);
    }
    public void ShowBat2Panel()
    {
        HideAllBatPanels();
        Show(bat2Panel);
    }
    public void ShowBat3Panel()
    {
        HideAllBatPanels();
        Show(bat3Panel);
    }
    public void ShowBat4Panel()
    {
        HideAllBatPanels();
        Show(bat4Panel);
    }

    public void HideAllBatPanels()
    {
        foreach (var panel in allBatPanels)
        {
            panel.style.display = DisplayStyle.None;
        }
    }
    

    public void HideAllImagePanels()
    {
        foreach (var panel in allPanels)
        {
            if (panel != blackPanel)
                panel.style.display = DisplayStyle.None;
        }
    }

    public void HideAllPanels()
    {
        foreach (var panel in allPanels)
        {
            panel.style.display = DisplayStyle.None;
        }
    }

    public void ShowIntroPanel()
    {
        Show(introPanel);
    }

    public void ShowEndingPanel()
    {
        Show(endingPanel);
    }

    public void ShowChargingPanel()
    {
        Show(chargingPanel);
    }

    public void ShowBlackPanel()
    {
        Show(blackPanel);
        blackPanel.style.opacity = 1f;
    }

    #region Black Screen Fades

    public void FadeToBlackPanel(float fadeDuration = 1f)
    {
        if (currentFade != null) StopCoroutine(currentFade);
        currentFade = StartCoroutine(FadeBlackCoroutine(0f, 1f, fadeDuration));
    }

    public void FadeOutOfBlackPanel(float fadeDuration = 1f)
    {
        if (currentFade != null) StopCoroutine(currentFade);
        currentFade = StartCoroutine(FadeBlackCoroutine(1f, 0f, fadeDuration, hideAfter: true));
    }

    private IEnumerator FadeBlackCoroutine(float start, float end, float duration, bool hideAfter = false)
    {
        blackPanel.style.display = DisplayStyle.Flex;
        float timer = 0f;

        while (timer < duration)
        {
            timer += Time.deltaTime;
            blackPanel.style.opacity = Mathf.Lerp(start, end, timer / duration);
            yield return null;
        }

        blackPanel.style.opacity = end;

        if (hideAfter && end == 0f)
            blackPanel.style.display = DisplayStyle.None;

        currentFade = null;
    }

    #endregion
}
