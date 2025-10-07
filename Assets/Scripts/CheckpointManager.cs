using System;
using System.Linq;
using UnityEngine;

public class CheckpointManager : MonoBehaviour
{
    private Checkpoint[] checkpoints;

    private void Awake()
    {
        checkpoints = GetChildrenOf("Checkpoints");
        checkpoints = checkpoints.OrderByDescending(c => c.Priority).ToArray();
    }

    private Checkpoint[] GetChildrenOf(string parentName)
    {
        var parent = GameObject.Find(parentName);
        if (parent == null) return new Checkpoint[0];

        return parent.GetComponentsInChildren<Checkpoint>(true)
                     .Where(t => t.gameObject != parent)
                     .Select(t => t)
                     .ToArray();
    }

    public Checkpoint GetLatestCheckpoint()
    {
        foreach (var checkpoint in checkpoints)
        {
            if (checkpoint.Activated)
            {
                return checkpoint;
            }
        }
        throw new ArgumentException("no checkpoint activated");
    }
}
