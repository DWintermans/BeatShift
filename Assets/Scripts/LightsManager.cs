using System.Linq;
using UnityEngine;

public class LightsManager : MonoBehaviour
{
    private WindowController[] windowControllers;
    private GameObject[] advertisementBoards;
    private GameObject[] advertisementTexts;

    private void Start()
    {
        windowControllers = FindObjectsByType<WindowController>(FindObjectsSortMode.None);
        advertisementBoards = GameObject.FindGameObjectsWithTag("AdvertisementBoard");

        var advertisementTextsParent = GameObject.Find("AdvertisementTexts");
        if (advertisementTextsParent != null)
        {
            advertisementTexts = advertisementTextsParent.GetComponentsInChildren<Transform>(true)
                                    .Where(t => t.gameObject != advertisementTextsParent)
                                    .Select(t => t.gameObject)
                                    .ToArray();
        }
        else
        {
            advertisementTexts = new GameObject[0];
        }
    }

    public void SetAllLightsActive(bool active)
    {
        SetAllWindowsActive(active);
        SetAllAdvertisementBoardLights(active);
        SetAllAdvertisementTextsActive(active);
    }

    private void SetAllWindowsActive(bool active)
    {
        foreach (var controller in windowControllers)
        {
            controller.SetWindowRenderersActive(active);
        }
    }

    private void SetAllAdvertisementBoardLights(bool active)
    {
        foreach (var advertisementBoard in advertisementBoards)
        {
            GameObject[] children = advertisementBoard.GetComponentsInChildren<Transform>(true)
                     .Where(t => t.gameObject != advertisementBoard)
                     .Select(t => t.gameObject)
                     .ToArray();

            foreach (var child in children)
            {
                Renderer childRenderer = child.GetComponent<Renderer>();
                if (child.CompareTag("Advertisement"))
                {
                    Color baseMap = active ? new Color(1, 1, 1) : new Color(0.3f, 0.3f, 0.3f);
                    childRenderer.material.color = baseMap;
                }
                else
                {
                    childRenderer.material.DisableKeyword("_EMISSION");
                }
            }
        }
    }

    private void SetAllAdvertisementTextsActive(bool active)
    {
        foreach (var advertisementText in advertisementTexts)
        {
            Renderer renderer = advertisementText.GetComponent<Renderer>();
            if (renderer != null)
            {
                renderer.enabled = active;
            }
        }
    }
}