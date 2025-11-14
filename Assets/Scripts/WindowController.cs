using UnityEngine;
using System.Linq;

public class WindowController : MonoBehaviour
{
    [Range(0f, 100f)]
    public float randomDeletePercentage = 50f;
    public Transform WindowSide;

    void Awake()
    {
        RandomlyDeleteWindows();
    }

    private void RandomlyDeleteWindows()
    {
        if (randomDeletePercentage <= 0f) return;

        var windows = WindowSide.GetComponentsInChildren<Transform>()
                                  .Where(t => t != WindowSide)
                                  .Select(t => t.gameObject)
                                  .ToList();

        int countToDelete = Mathf.RoundToInt(windows.Count * (randomDeletePercentage / 100f));

        var randomWindows = windows.OrderBy(_ => Random.value).Take(countToDelete);

        foreach (var window in randomWindows)
        {
            Destroy(window);
        }
    }

    public void SetWindowRenderersActive(bool active)
    {
        var windowRenderers = WindowSide.GetComponentsInChildren<Renderer>()
                                  .Where(t => t != WindowSide)
                                  .ToList();

        foreach (var renderer in windowRenderers)
        {
            renderer.enabled = active;
        }
    }
}
