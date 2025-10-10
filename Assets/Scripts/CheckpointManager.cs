using System;
using System.Linq;
using UnityEngine;

public class CheckpointManager : MonoBehaviour
{
    private Checkpoint[] checkpoints;

    private void Awake()
    {
        checkpoints = GetComponentsInChildren<Checkpoint>(true)
                     .Where(t => t.gameObject != this)
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
