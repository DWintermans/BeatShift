using UnityEngine;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using System.Collections;

public class BeatSequencer : MonoBehaviour
{
    [Header("CSV Files")]
    public TextAsset[] beatCSVs;
    public int selectedBeatIndex = 0;

    [Header("Drum Audio Sources")]
    public AudioSource kickSource, snareSource, hihatSource;

    [Header("Visualizer")]
    public BeatVisualizer visualizer;

    [Header("Tempo")]
    public float bpm = 160f;

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

    void Start()
    {
        visualizer = FindFirstObjectByType<BeatVisualizer>();
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

        //run changes after loop ends
        if (gridPos == totalSteps - 1)
        {
            if (selectedBeatIndex != lastBeatIndex)
                LoadSelectedBeat();

            if (bpmChanged)
            {
                ApplyBPM();
                bpmChanged = false;
            }
        }

        timer += Time.deltaTime;
        if (timer >= stepDuration)
        {
            timer -= stepDuration;
            PlayStep(gridPos);
            gridPos = (gridPos + 1) % totalSteps;
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
                    visualizer?.OnHihat();
                }
                else if (instrument == "snare")
                {
                    snareSource.PlayOneShot(snareSource.clip, drumVolume);
                    visualizer?.OnSnare();
                }
                else if (instrument == "kick")
                {
                    kickSource.PlayOneShot(kickSource.clip, drumVolume);
                    visualizer?.OnKick();
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
        }
    }

    private IEnumerator FadeInBass(AudioSource source, float duration)
    {
        float targetVolume = VolumeManager.Instance.bassVolume * VolumeManager.Instance.mainVolume;
        source.volume = 0f;
        source.Play();

        float elapsed = 0f;
        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
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
            elapsed += Time.deltaTime;
            source.volume = Mathf.Lerp(startVolume, 0f, elapsed / duration);
            yield return null;
        }

        source.volume = 0f;
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

        // foreach (var kvp in dictionary)
        // {
        //     string patternStr = string.Join(",", kvp.Value);
        //     Debug.Log($"{kvp.Key}: {patternStr}");
        // }

        return dictionary;
    }
}
