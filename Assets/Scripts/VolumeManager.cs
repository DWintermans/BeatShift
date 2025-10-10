using UnityEngine;

public class VolumeManager : MonoBehaviour
{
    [Range(0f, 1f)] public float mainVolume = 0.5f;
    [Range(0f, 1f)] public float drumVolume = 0.5f;
    [Range(0f, 1f)] public float bassVolume = 0.5f;

    private static VolumeManager _instance;
    public static VolumeManager Instance
    {
        get
        {
            if (_instance == null)
            {
                _instance = FindFirstObjectByType<VolumeManager>();
                if (_instance == null)
                {
                    var go = new GameObject("VolumeManager");
                    _instance = go.AddComponent<VolumeManager>();
                    DontDestroyOnLoad(go);
                }
            }
            return _instance;
        }
    }

    void Awake()
    {
        if (_instance != null && _instance != this)
        {
            Destroy(gameObject);
            return;
        }
        _instance = this;
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