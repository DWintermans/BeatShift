using UnityEngine;

public class VolumeManager : MonoBehaviour
{
    [Range(0f, 1f)] public float mainVolume = 0.5f;
    [Range(0f, 1f)] public float drumVolume = 0.5f;
    [Range(0f, 1f)] public float bassVolume = 0.5f;

    public static VolumeManager Instance { get; private set; }

    void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        DontDestroyOnLoad(gameObject);
    }

    public void SetMainVolume(float value)
    {
        mainVolume = Mathf.Clamp01(value);
    }

    public void SetDrumVolume(float value)
    {
        drumVolume = Mathf.Clamp01(value);
    }

    public void SetBassVolume(float value)
    {
        bassVolume = Mathf.Clamp01(value);
    }
}
