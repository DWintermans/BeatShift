using UnityEngine;

public class BeatVisualizer : MonoBehaviour
{
    public GameObject[] kickPlatformsList;
    public GameObject[] snarePlatformsList;
    public GameObject[] hihatPlatformsList;

    private bool isKickOpaque = false;
    private bool isSnareOpaque = false;
    private bool isHihatOpaque = false;

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
    }

    private void ToggleTransparency(GameObject[] platforms, bool isOpaque)
    {
        float alpha = isOpaque ? 1f : 0.3f;

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
}
