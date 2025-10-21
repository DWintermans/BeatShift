using System.Linq;
using UnityEngine;

public class BeatVisualizer : MonoBehaviour
{
    //cache retrieved platforms
    private GameObject[] kickPlatformsList;
    private GameObject[] snarePlatformsList;
    private GameObject[] hihatPlatformsList;

    private bool isKickOpaque = false;
    private bool isSnareOpaque = false;
    private bool isHihatOpaque = false;

    private void Awake()
    {
        kickPlatformsList = GetChildrenOf("KickPlatforms");
        snarePlatformsList = GetChildrenOf("SnarePlatforms");
        hihatPlatformsList = GetChildrenOf("HihatPlatforms");
    }

    private GameObject[] GetChildrenOf(string parentName)
    {
        var parent = GameObject.Find(parentName);
        if (parent == null) return new GameObject[0];

        return parent.GetComponentsInChildren<Transform>(true)
                     .Where(t => t.gameObject != parent)
                     .Select(t => t.gameObject)
                     .ToArray();
    }

    public void OnKick()
    {
        isKickOpaque = !isKickOpaque;
        ToggleTransparency(kickPlatformsList, isKickOpaque);
        ToggleHitbox(kickPlatformsList, isKickOpaque);
    }

    public void OnSnare()
    {
        isSnareOpaque = !isSnareOpaque;
        ToggleTransparency(snarePlatformsList, isSnareOpaque);
        ToggleHitbox(snarePlatformsList, isSnareOpaque);
    }

    public void OnHihat()
    {
        isHihatOpaque = !isHihatOpaque;
        ToggleTransparency(hihatPlatformsList, isHihatOpaque);
        ToggleHitbox(hihatPlatformsList, isHihatOpaque);
        ToggleEmission(hihatPlatformsList, isHihatOpaque);
    }

    private void ToggleTransparency(GameObject[] platforms, bool isOpaque)
    {
        float alpha = isOpaque ? 0.3f : 1f;

        foreach (var obj in platforms)
        {
            var renderer = obj.GetComponent<Renderer>();
            if (renderer != null)
            {
                Color c = renderer.material.color;
                c.a = alpha;
                renderer.material.color = c;
            }
        }
    }

    private void ToggleHitbox(GameObject[] platforms, bool isOpaque)
    {
        foreach (var obj in platforms)
        {
            if (obj == null)
                continue;
            var collider = obj.GetComponent<Collider>();
            if (collider != null)
            {
                collider.enabled = !isOpaque;
            }
        }
    }

    private void ToggleEmission(GameObject[] platforms, bool isOpaque)
    {
        foreach (var obj in platforms)
        {
            var renderer = obj.GetComponent<Renderer>();
            if (renderer != null)
            {
                if (isOpaque)
                {
                    renderer.material.DisableKeyword("_EMISSION");
                }
                else
                {
                    renderer.material.EnableKeyword("_EMISSION");
                }
            }
        }
    }
}
