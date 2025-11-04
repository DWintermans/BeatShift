using System.Linq;
using UnityEngine;

public class BeatVisualizer : MonoBehaviour
{
    //cache retrieved platforms
    private GameObject[] kickPlatformsList;
    private GameObject[] snarePlatformsList;
    private GameObject[] hihatPlatformsList;
    private MovingPlatform[] kickMovingPlatformsList;
    private MovingPlatform[] snareMovingPlatformsList;

    private bool isKickOpaque = false;
    private bool isSnareOpaque = false;
    private bool isHihatOpaque = false;

    private void Awake()
    {
        kickPlatformsList = GetChildrenOf("KickPlatforms");
        snarePlatformsList = GetChildrenOf("SnarePlatforms");
        hihatPlatformsList = GetChildrenOf("HihatPlatforms");
        kickMovingPlatformsList = GetMovingPlatformChildrenOf("KickMovingPlatforms");
        snareMovingPlatformsList = GetMovingPlatformChildrenOf("SnareMovingPlatforms");
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

    private MovingPlatform[] GetMovingPlatformChildrenOf(string parentName)
    {
        var parent = GameObject.Find(parentName);
        if (parent == null) return new MovingPlatform[0];

        return parent.GetComponentsInChildren<MovingPlatform>(true)
                     .ToArray();
    }

    public void OnKick()
    {
        isKickOpaque = !isKickOpaque;
        ToggleTransparency(kickPlatformsList, isKickOpaque);
        ToggleHitbox(kickPlatformsList, isKickOpaque);
        ToggleEmission(kickPlatformsList, isKickOpaque);
        ToggleAdvertisementBaseMap(kickPlatformsList, isKickOpaque);

        ToggleMovingPlatformDirection(kickMovingPlatformsList);
    }

    public void OnSnare()
    {
        isSnareOpaque = !isSnareOpaque;
        ToggleTransparency(snarePlatformsList, isSnareOpaque);
        ToggleHitbox(snarePlatformsList, isSnareOpaque);
        ToggleEmission(snarePlatformsList, isSnareOpaque);
        ToggleAdvertisementBaseMap(snarePlatformsList, isSnareOpaque);

        ToggleMovingPlatformDirection(snareMovingPlatformsList);
    }

    public void OnHihat()
    {
        isHihatOpaque = !isHihatOpaque;
        ToggleTransparency(hihatPlatformsList, isHihatOpaque);
        ToggleHitbox(hihatPlatformsList, isHihatOpaque);
        ToggleEmission(hihatPlatformsList, isHihatOpaque);
        ToggleAdvertisementBaseMap(hihatPlatformsList, isHihatOpaque);
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
            else
            {
                var childRenderers = obj.GetComponentsInChildren<Renderer>();
                if (childRenderers != null)
                {
                    foreach (var childRenderer in childRenderers)
                    {
                        if (childRenderer != null)
                        {
                            Color c = childRenderer.material.color;
                            c.a = alpha;
                            childRenderer.material.color = c;
                        }
                    }
                }
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
                if (obj.layer == LayerMask.NameToLayer("Platforms"))
                    collider.enabled = !isOpaque;
                else
                    collider.isTrigger = isOpaque;
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

    private void ToggleAdvertisementBaseMap(GameObject[] platforms, bool isOpaque)
    {
        Color baseMap = isOpaque ? new Color(0.3f, 0.3f, 0.3f) : new Color(1, 1, 1);

        foreach (var obj in platforms)
        {
            var childRenderers = obj.GetComponentsInChildren<Renderer>();
            foreach (var childRenderer in childRenderers)
            {
                if (childRenderer.CompareTag("Advertisement"))
                {
                    childRenderer.material.color = baseMap;
                }
            }
        }
    }

    private void ToggleMovingPlatformDirection(MovingPlatform[] platforms)
    {
        foreach (var obj in platforms)
        {
            obj.Move();
        }
    }
}
