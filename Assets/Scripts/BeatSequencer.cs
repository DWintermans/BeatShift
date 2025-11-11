using UnityEngine;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using System.Collections;
using UnityEngine.SceneManagement;

public class BeatSequencer : MonoBehaviour
{
    public CutsceneController cutsceneController;

    [Header("CSV Files")]
    public TextAsset[] beatCSVs;
    public int selectedBeatIndex = 0;

    [Header("Drum Audio Sources")]
    public AudioSource kickSource, snareSource, hihatSource, stickSource, bellSource;

    [Header("Visualizer")]
    public BeatVisualizer visualizer;

    [Header("Tempo")]
    public float bpm = 124f;

    [Header("Electricity Audio Sources")]
    public AudioSource electricitySource;
    public AudioClip electricity1;
    public AudioClip electricity2;
    public AudioClip electricity3;
    public AudioClip electricity4;
    public AudioClip electricSwitch;


    [Header("Bass Audio Sources")]
    public AudioSource bassSource;
    public AudioClip E0, E1, E2, E3, E4, E5,
                 A1, A2, A3, A4, A5,
                 D1, D2, D3, D4, D5,
                 G1, G2, G3, G4, G5, G6, G7, G8, G9, G10, G11, G12, G13, G14;

    private Dictionary<string, AudioClip> bassClips;

    //instrument name + bool
    private Dictionary<string, string[]> beatMap;
    private int gridPos = 0;
    private float stepDuration;
    private float timer;

    private int lastBeatIndex = -1;
    private float lastBpm;
    private bool bpmChanged = false;

    private Queue<QueuedBeat> beatQueue = new Queue<QueuedBeat>();

    public struct QueuedBeat
    {
        public int beatIndex;
        public float bpm;

        public QueuedBeat(int beatIndex, float bpm)
        {
            this.beatIndex = beatIndex;
            this.bpm = bpm;
        }
    }
    private bool switchBeat = false;
    private bool PreparingTransition = false;

    public void SwitchBeat()
    {
        switchBeat = true;
    }

    private bool IsReadyToVisualize = false;

    public static BeatSequencer Instance;
    private WindowManager windowManager;
    private bool electricityState = true;

    void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(bassSource.gameObject);
        DontDestroyOnLoad(kickSource.gameObject);
        DontDestroyOnLoad(snareSource.gameObject);
        DontDestroyOnLoad(hihatSource.gameObject);
        DontDestroyOnLoad(electricitySource.gameObject);
    }

    void Start()
    {
        visualizer = FindFirstObjectByType<BeatVisualizer>();
        cutsceneController = FindFirstObjectByType<CutsceneController>();
        windowManager = FindFirstObjectByType<WindowManager>();

        ApplyBPM();
        LoadSelectedBeat();

        bassClips = new Dictionary<string, AudioClip>
        {
            { "E0", E0 },
            { "E1", E1 },
            { "E2", E2 },
            { "E3", E3 },
            { "E4", E4 },
            { "E5", E5 },
            { "A1", A1 },
            { "A2", A2 },
            { "A3", A3 },
            { "A4", A4 },
            { "A5", A5 },
            { "D1", D1 },
            { "D2", D2 },
            { "D3", D3 },
            { "D4", D4 },
            { "D5", D5 },
            { "G1", G1 },
            { "G2", G2 },
            { "G3", G3 },
            { "G4", G4 },
            { "G5", G5 },
            { "G6", G6 },
            { "G7", G7 },
            { "G8", G8 },
            { "G9", G9 },
            { "G10", G10 },
            { "G11", G11 },
            { "G12", G12 },
            { "G13", G13 },
            { "G14", G14 }
        };
    }

    void Update()
    {
        if (!Mathf.Approximately(bpm, lastBpm))
            bpmChanged = true;

        int totalSteps = beatMap.Values.First().Length;

        float delta = SceneManager.GetActiveScene().name == "MainMenu" ? Time.unscaledDeltaTime : Time.deltaTime;

        timer += delta;
        if (timer >= stepDuration)
        {
            timer -= stepDuration;
            PlayStep(gridPos);
            gridPos = (gridPos + 1) % totalSteps;

            //run changes after loop ends
            if (gridPos == 0)
            {
                while (beatQueue.Count > 0)
                {
                    QueuedBeat nextBeat = beatQueue.Dequeue();

                    //marker
                    if (nextBeat.bpm == 2000f)
                    {
                        IsReadyToVisualize = true;
                        continue;
                    }
                    //next level loader triggering after a beat ends
                    else if (nextBeat.bpm == 4000f)
                    {
                        FindFirstObjectByType<LevelManager>().LoadNextLevel();
                        break;
                    }
                    //play electric switch sound
                    else if (nextBeat.bpm == 5000f)
                    {
                        electricitySource.PlayOneShot(electricSwitch, VolumeManager.Instance.drumVolume);
                        continue;
                    }
                    //cutscene
                    else if (nextBeat.bpm >= (float)CutsceneAction.HideAllPanels)
                    {
                        if (cutsceneController != null)
                        {
                            CutsceneAction action = (CutsceneAction)(int)nextBeat.bpm;
                            cutsceneController.PlayCutScene(action);
                        }

                        continue;
                    }
                    //normal playable beat
                    else
                    {
                        selectedBeatIndex = nextBeat.beatIndex;
                        bpm = nextBeat.bpm;
                        bpmChanged = true;
                        break;
                    }
                }

                if (beatQueue.Count == 0 && !PreparingTransition)
                {
                    FillBeatQueue();
                }

                if (selectedBeatIndex != lastBeatIndex)
                    LoadSelectedBeat();

                if (bpmChanged)
                {
                    ApplyBPM();
                    bpmChanged = false;
                }
            }
        }
    }

    private void ApplyBPM()
    {
        //60sec / bpm / 4th for 16th note
        stepDuration = 60f / bpm / 4f;
        lastBpm = bpm;
    }

    public void LoadSelectedBeat()
    {
        if (beatCSVs.Length == 0)
            return;

        selectedBeatIndex = Mathf.Clamp(selectedBeatIndex, 0, beatCSVs.Length - 1);
        beatMap = LoadBeat(beatCSVs[selectedBeatIndex]);

        //restart beat at beginning
        gridPos = 0;
        lastBeatIndex = selectedBeatIndex;
    }

    private void PlayStep(int index)
    {
        foreach (var kvp in beatMap)
        {
            string instrument = kvp.Key;
            string cell = kvp.Value[index];

            //drums
            if (cell.ToLower() == "x")
            {
                float drumVolume = VolumeManager.Instance.drumVolume * VolumeManager.Instance.mainVolume;

                if (instrument == "hihat")
                {
                    hihatSource.PlayOneShot(hihatSource.clip, drumVolume);
                    if (IsReadyToVisualize) visualizer?.OnHihat();
                }
                else if (instrument == "snare")
                {
                    snareSource.PlayOneShot(snareSource.clip, drumVolume);
                    if (IsReadyToVisualize) visualizer?.OnSnare();
                }
                else if (instrument == "kick")
                {
                    kickSource.PlayOneShot(kickSource.clip, drumVolume);
                    if (IsReadyToVisualize) visualizer?.OnKick();
                }
                else if (instrument == "stick")
                {
                    stickSource.PlayOneShot(stickSource.clip, drumVolume);
                }
                else if (instrument == "bell")
                {
                    bellSource.PlayOneShot(bellSource.clip, drumVolume);
                }
            }

            //bass
            if (instrument == "bass")
            {
                if (cell == "-")
                {
                    if (bassSource.isPlaying)
                        StartCoroutine(FadeOutBass(bassSource, 0.1f));
                }
                else if (bassClips.ContainsKey(cell))
                {
                    if (bassSource.clip != bassClips[cell] || !bassSource.isPlaying)
                    {
                        bassSource.clip = bassClips[cell];
                        bassSource.loop = true;
                        StartCoroutine(FadeInBass(bassSource, 0.1f));
                    }
                }
            }

            //electricity
            if (instrument == "electricity" && cell != "-" && PreparingTransition)
            {
                switch (cell)
                {
                    case "1":
                        electricitySource.PlayOneShot(electricity1, VolumeManager.Instance.drumVolume);
                        break;
                    case "2":
                        electricitySource.PlayOneShot(electricity2, VolumeManager.Instance.drumVolume);
                        break;
                    case "3":
                        electricitySource.PlayOneShot(electricity3, VolumeManager.Instance.drumVolume);
                        break;
                    case "4":
                        electricitySource.PlayOneShot(electricity4, VolumeManager.Instance.drumVolume);
                        break;
                }

                electricityState = !electricityState;
                windowManager.SetAllWindowsActive(electricityState);

                if (cell == "4" && cutsceneController != null)
                    cutsceneController.PlayCutScene(CutsceneAction.FadeToBlackPanelShort);
            }
        }
    }

    private IEnumerator FadeInBass(AudioSource source, float duration)
    {
        float targetVolume = VolumeManager.Instance.bassVolume * VolumeManager.Instance.mainVolume;
        source.volume = 0.1f;
        source.Play();

        float elapsed = 0f;
        while (elapsed < duration)
        {
            float delta = SceneManager.GetActiveScene().name == "MainMenu" ? Time.unscaledDeltaTime : Time.deltaTime;
            elapsed += delta;
            source.volume = Mathf.Lerp(0f, targetVolume, elapsed / duration);
            yield return null;
        }

        source.volume = targetVolume;
    }

    private IEnumerator FadeOutBass(AudioSource source, float duration)
    {
        float startVolume = source.volume;
        float elapsed = 0f;

        while (elapsed < duration)
        {
            float delta = SceneManager.GetActiveScene().name == "MainMenu" ? Time.unscaledDeltaTime : Time.deltaTime;
            elapsed += delta;
            source.volume = Mathf.Lerp(startVolume, 0f, elapsed / duration);
            yield return null;
        }

        source.volume = 0.1f;
        source.Stop();
    }

    private Dictionary<string, string[]> LoadBeat(TextAsset csv)
    {
        var dictionary = new Dictionary<string, string[]>();
        var lines = csv.text.Split('\n');

        foreach (string line in lines)
        {
            if (string.IsNullOrWhiteSpace(line)) continue;

            string[] cells = line.Trim().Split(';');
            string instrument = cells[0];

            string[] pattern = new string[cells.Length - 1];
            for (int i = 1; i < cells.Length; i++)
                pattern[i - 1] = cells[i].Trim();

            dictionary[instrument] = pattern;
        }

        return dictionary;
    }

    private void EnqueueBeat(int beatIndex, float bpm)
    {
        beatQueue.Enqueue(new QueuedBeat(beatIndex, bpm));
    }

    public void ClearQueue()
    {
        beatQueue.Clear();
    }

    void OnEnable()
    {
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    void OnDisable()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        //find the visualizer in new scene
        visualizer = FindFirstObjectByType<BeatVisualizer>();
        cutsceneController = FindFirstObjectByType<CutsceneController>();
        windowManager = FindFirstObjectByType<WindowManager>();

        SetBeatForScene(scene.name);
    }

    private void SetBeatForScene(string sceneName)
    {
        ClearQueue();
        cutsceneController.ShowBlackPanel();
        cutsceneController.HideAllImagePanels();
        IsReadyToVisualize = false;
        switchBeat = false;
        PreparingTransition = false;
        electricityState = true;

        if (!sceneName.Contains("MainMenu") || !sceneName.Contains("Level 1"))
        {
            EnqueueBeat(11, (float)CutsceneAction.FadeOutOfBlackPanelShort);
        }

        //bridgers + beat
        if (sceneName.Contains("MainMenu"))
        {
            //beat
            EnqueueBeat(0, 124f);
            EnqueueBeat(1, 124f);
        }
        else if (sceneName.Contains("Tutorial"))
        {
            //marker
            EnqueueBeat(13, 2000f);

            //beat
            EnqueueBeat(2, 124f);
        }
        else if (sceneName.Contains("Level 1"))
        {
            EnqueueBeat(11, (float)CutsceneAction.ShowIntroPanel);
            EnqueueBeat(11, (float)CutsceneAction.FadeOutOfBlackPanelShort);

            //heartbeat
            EnqueueBeat(6, 120f);
            EnqueueBeat(6, 100f);
            EnqueueBeat(6, 80f);
            EnqueueBeat(6, 50f);

            //1 sec pause
            EnqueueBeat(11, 180f);
            EnqueueBeat(11, (float)CutsceneAction.ShowBlackPanel);
            EnqueueBeat(11, (float)CutsceneAction.HideAllImagePanels);

            //2 sec pause
            EnqueueBeat(11, 120f);
            EnqueueBeat(11, (float)CutsceneAction.FadeOutOfBlackPanel);

            //beat buildup from 124 to 160 bpm
            EnqueueBeat(7, 136f);
            EnqueueBeat(7, 136f);
            EnqueueBeat(7, 142f);
            EnqueueBeat(7, 142f);

            EnqueueBeat(7, 148f);
            EnqueueBeat(7, 148f);
            EnqueueBeat(7, 154f);
            EnqueueBeat(7, 154f);

            EnqueueBeat(8, 160f);
            EnqueueBeat(8, 160f);
            EnqueueBeat(8, 160f);
            EnqueueBeat(8, 160f);

            EnqueueBeat(9, 160f);
            EnqueueBeat(9, 160f);
            EnqueueBeat(9, 160f);
            EnqueueBeat(9, 160f);

            //marker
            EnqueueBeat(13, 2000f);

            //beat
            EnqueueBeat(4, 160f);
            EnqueueBeat(5, 160f);
            EnqueueBeat(4, 160f);
            EnqueueBeat(10, 160f);
        }
        else if (sceneName.Contains("Level 2"))
        {
            //marker
            EnqueueBeat(13, 2000f);

            //beat
            EnqueueBeat(3, 124f);
            EnqueueBeat(19, 124f);
            EnqueueBeat(20, 124f);
            EnqueueBeat(21, 124f);
        }
        else if (sceneName.Contains("Level 3"))
        {
            //marker
            EnqueueBeat(13, 2000f);

            //beat
            EnqueueBeat(15, 60);
            EnqueueBeat(17, 60);
            EnqueueBeat(17, 60);
            EnqueueBeat(18, 60);
        }
    }

    private void FillBeatQueue()
    {
        string sceneName = SceneManager.GetActiveScene().name;

        // loop beat
        if (sceneName.Contains("MainMenu"))
        {
            EnqueueBeat(0, 124f);
            EnqueueBeat(1, 124f);
        }
        else if (sceneName.Contains("Tutorial"))
        {
            EnqueueBeat(2, 124f);
        }
        else if (sceneName.Contains("Level 1"))
        {
            EnqueueBeat(4, 160f);
            EnqueueBeat(5, 160f);
            EnqueueBeat(4, 160f);
            EnqueueBeat(10, 160f);
        }
        else if (sceneName.Contains("Level 2"))
        {
            if (!switchBeat)
            {
                //normal beat
                EnqueueBeat(3, 124f);
                EnqueueBeat(19, 124f);
                EnqueueBeat(20, 124f);
                EnqueueBeat(21, 124f);
            }
            else
            {
                //updated beat
                EnqueueBeat(12, 124f);
            }
        }
        else if (sceneName.Contains("Level 3"))
        {
            if (!switchBeat)
            {
                //normal beat
                EnqueueBeat(17, 60);
                EnqueueBeat(17, 60);
                EnqueueBeat(17, 60);
                EnqueueBeat(18, 60);
            }
            else
            {
                //updated beat
                EnqueueBeat(16, 120);
            }
        }
    }

    //triggers when landing on platform marked as Finish.
    public void PrepareSceneTransition(string currentScene)
    {
        //prevent double calling of function
        if (PreparingTransition)
            return;

        ClearQueue();
        IsReadyToVisualize = false;
        switchBeat = false;
        PreparingTransition = true;

        //transition to level 1
        if (currentScene.Contains("Tutorial"))
        {
            EnqueueBeat(11, (float)CutsceneAction.HideAllImagePanels);
            EnqueueBeat(11, (float)CutsceneAction.FadeToBlackPanelShort);

            //3 sec pause
            EnqueueBeat(11, 120f);
            EnqueueBeat(11, 180f);

            //load next level
            EnqueueBeat(11, 4000f);
        }
        //transition to level 2
        else if (currentScene.Contains("Level 1"))
        {
            ChargingCutscene();
        }
        //transition to level 3
        else if (currentScene.Contains("Level 2"))
        {
            ChargingCutscene();
        }
        //transition to ending
        else if (currentScene.Contains("Level 3"))
        {
            ChargingCutscene(false);
            EnqueueBeat(11, (float)CutsceneAction.FadeToBlackPanel);
            EnqueueBeat(11, 60f);
            EnqueueBeat(11, (float)CutsceneAction.ShowEndingPanel);
            EnqueueBeat(11, (float)CutsceneAction.FadeOutOfBlackPanelShort);
            EnqueueBeat(11, 60f);
            EnqueueBeat(11, 60f);

            //back to menu
            EnqueueBeat(11, 4000f);
        }
    }

    private void ChargingCutscene(bool AutoLoadNextLevel = true)
    {
        EnqueueBeat(11, (float)CutsceneAction.FadeToBlackPanelShort);
        EnqueueBeat(11, 120f);

        EnqueueBeat(11, (float)CutsceneAction.HideAllImagePanels);
        EnqueueBeat(11, (float)CutsceneAction.ShowChargingPanel);

        EnqueueBeat(11, (float)CutsceneAction.FadeOutOfBlackPanelShort);
        EnqueueBeat(11, 120f);

        EnqueueBeat(11, 5000f);

        EnqueueBeat(11, 120f);

        EnqueueBeat(11, (float)CutsceneAction.HideAllImagePanels);

        //play electricty beat + turn windows on/off on beat, automatically does FadeToBlackPanelShort on 4th note
        EnqueueBeat(14, 100f);

        EnqueueBeat(11, 120f);

        //load next level
        if (AutoLoadNextLevel)
            EnqueueBeat(11, 4000f);
    }
}
