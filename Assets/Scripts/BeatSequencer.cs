using UnityEngine;
using System.IO;
using System.Collections.Generic;
using System.Linq;

public class BeatSequencer : MonoBehaviour
{
    [Header("CSV Files")]
    public TextAsset[] beatCSVs; 
    public int selectedBeatIndex = 0;

    [Header("Audio Sources")]
    public AudioSource kickSource, snareSource, hihatSource;

    [Header("Visualizer")]
    public BeatVisualizer visualizer;

    //instrument name + bool
    private Dictionary<string, bool[]> beatMap;
    private int gridPos = 0;
    public float bpm = 160f;
    private float stepDuration;
    private float timer;

    void Start()
    {
        //60sec / bpm / 4th for 16th note
        stepDuration = 60f / bpm / 4f;
        LoadSelectedBeat();
        visualizer = FindFirstObjectByType<BeatVisualizer>();
    }

    void Update()
    {
        timer += Time.deltaTime;
        if (timer >= stepDuration)
        {
            timer -= stepDuration;
            PlayStep(gridPos);
            int totalSteps = beatMap.Values.First().Length; 
            gridPos = (gridPos + 1) % totalSteps;
        }
    }

    public void LoadSelectedBeat()
    {
        if (beatCSVs.Length == 0)
            return;
        selectedBeatIndex = Mathf.Clamp(selectedBeatIndex, 0, beatCSVs.Length - 1);
        beatMap = LoadBeat(beatCSVs[selectedBeatIndex]);
        
        //restart beat at beginning
        gridPos = 0; 
    }

    private void PlayStep(int index)
    {
        foreach (var kvp in beatMap)
        {
            bool[] pattern = kvp.Value;
            if (pattern[index])
            {
                if (kvp.Key == "hihat")
                {
                    hihatSource.PlayOneShot(hihatSource.clip);
                    visualizer?.OnHihat();
                }

                if (kvp.Key == "snare")
                {
                    snareSource.PlayOneShot(snareSource.clip);
                    visualizer?.OnSnare();
                }

                if (kvp.Key == "kick")
                {
                    kickSource.PlayOneShot(kickSource.clip);
                    visualizer?.OnKick();
                }  
            }
        }
    }

    private Dictionary<string, bool[]> LoadBeat(TextAsset csv)
    {
        var dictionary = new Dictionary<string, bool[]>();
        var lines = csv.text.Split('\n');

        foreach (string line in lines)
        {
            if (string.IsNullOrWhiteSpace(line)) continue;
            
            string[] cells = line.Trim().Split(';');

            string instrument = cells[0];

            bool[] pattern = new bool[cells.Length - 1];

            for (int i = 1; i < cells.Length; i++)
            {
                pattern[i - 1] = cells[i].Trim().ToLower() == "x";
            }
            dictionary[instrument] = pattern;
        }

        // foreach (var kvp in dictionary)
        // {
        //     string pattern = string.Join(",", kvp.Value.Select(b => b ? "x" : "-"));
        //     Debug.Log($"{kvp.Key}: {pattern}");
        // }

        return dictionary;
    }
}
