using UnityEngine;
using System;

/// <summary>
/// An interface that allows behavior defind for when something is occluded and revealed.
/// </summary>
public interface IOccludable
{
    void OnOccluded();
    void OnRevealed();
}
