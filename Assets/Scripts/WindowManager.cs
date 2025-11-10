using UnityEngine;

public class WindowManager : MonoBehaviour
{
    private WindowController[] windowControllers;
    private void Start()
    {
        windowControllers = FindObjectsByType<WindowController>(FindObjectsSortMode.None);
    }

    public void SetAllWindowsActive(bool active)
    {
        foreach (var controller in windowControllers)
        {
            controller.SetWindowRenderersActive(active);
        }
    }
}
