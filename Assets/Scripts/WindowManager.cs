using UnityEngine;
using System.Linq;

public class WindowDestroyer : MonoBehaviour
{
    [Range(0f, 100f)]
    public float randomDeletePercentage = 50f;
    public Transform WindowSideZ;
    public Transform WinowSideNegativeZ;

    void Awake()
    {
        if (!DestroyWindowsOnBackside(WindowSideZ, Vector3.right, Vector3.forward))
            RandomlyDeleteWindows(WindowSideZ, randomDeletePercentage);
        if (!DestroyWindowsOnBackside(WinowSideNegativeZ, Vector3.left, Vector3.back))
            RandomlyDeleteWindows(WinowSideNegativeZ, randomDeletePercentage);        
    }

    private bool DestroyWindowsOnBackside(Transform windowsGroup, Vector3 xAxisDirection, Vector3 zAxisDirection)
    {
        if (!IsVisibleFromAxis(xAxisDirection) && !IsVisibleFromAxis(zAxisDirection))
        {
            Destroy(windowsGroup.gameObject);
            return true;
        }

        return false;
    }

    private bool IsVisibleFromAxis(Vector3 axisDirection)
    {
        return Vector3.Dot(transform.forward, axisDirection) > 0.5f;
    }

    private void RandomlyDeleteWindows(Transform windowsGroup, float percentage)
    {
        if (percentage <= 0f) return;

        var windows = windowsGroup.GetComponentsInChildren<Transform>()
                                  .Where(t => t != windowsGroup)
                                  .Select(t => t.gameObject)
                                  .ToList();

        int countToDelete = Mathf.RoundToInt(windows.Count * (percentage / 100f));

        var randomWindows = windows.OrderBy(_ => Random.value).Take(countToDelete);

        foreach (var window in randomWindows)
        {
            Destroy(window);
        }
    }
}
