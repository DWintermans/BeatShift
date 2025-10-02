using UnityEngine;
using System.IO;
using System.Collections.Generic;
using System.Linq;

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
    public AudioClip C2, D2, E2, F2, G2, A2, B2, C3, D3, E3, F3, G3, A3, B3, C4;
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
            { "C2", C2 },
            { "D2", D2 },
            { "E2", E2 },
            { "F2", F2 },
            { "G2", G2 },
            { "A2", A2 },
            { "B2", B2 },
            { "C3", C3 },
            { "D3", D3 },
            { "E3", E3 },
            { "F3", F3 },
            { "G3", G3 },
            { "A3", A3 },
            { "B3", B3 },
            { "C4", C4 }
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
                if (instrument == "hihat")
                {
                    hihatSource.PlayOneShot(hihatSource.clip);
                    visualizer?.OnHihat();
                }
                else if (instrument == "snare")
                {
                    snareSource.PlayOneShot(snareSource.clip);
                    visualizer?.OnSnare();
                }
                else if (instrument == "kick")
                {
                    kickSource.PlayOneShot(kickSource.clip);
                    visualizer?.OnKick();
                }
            }

            //bass
            if (instrument == "bass")
            {
                if (cell == "-")
                {
                    if (bassSource.isPlaying) bassSource.Stop();
                }
                else if (bassClips.ContainsKey(cell))
                {
                    if (bassSource.clip != bassClips[cell] || !bassSource.isPlaying)
                    {
                        bassSource.clip = bassClips[cell];
                        bassSource.loop = true;
                        bassSource.Play();
                    }
                }
            }
        }
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
