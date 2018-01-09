using System;
using UnityEngine;
using System.Collections.Generic;

/// <summary>
/// This class provides utility functions for managing and playing vfx
/// </summary>
public static class VFXUtils
{
    public static void PlayVFX(GameObject vfxObject)
    {
        PlayVFX(vfxObject.GetComponent<ParticleSystem>());
    }

    public static void PlayVFXWithoutRestart(ParticleSystem particleSystem)
    {
        if (!particleSystem.isPlaying) {
            PlayVFX(particleSystem);
        }
    }

    public static void PlayVFX(ParticleSystem particleSystem)
    {
        particleSystem.Simulate(0);
        particleSystem.Play();
    }
        
    /// <summary>
    /// In Unity, ParticleSystems don't scale with their parent. This
    /// utility will search the specified GameObject for ParticleSystems
    /// and size it to match it's scale within the object heirarchy.
    /// 
    /// Returns a VFXScaleRestorer object which should be restored to
    /// restore the ParticleSystem to it's original values.
    /// </summary>
    public static VFXScaleRestorer ScaleVFX(GameObject vfxObject)
    {
        ParticleSystem[] particles = vfxObject.GetComponentsInChildren<ParticleSystem>(includeInactive:true);
        List<VFXScaleRestorePoint> restorePoints = new List<VFXScaleRestorePoint>();


        foreach (ParticleSystem particle in particles)
        {
			ParticleSystem.MainModule main = particle.main;

            VFXScaleRestorePoint restorePoint = new VFXScaleRestorePoint();
            restorePoint.Source = particle;
			restorePoint.OriginalStartSize = main.startSize;
			restorePoint.OriginalStartSpeed = main.startSize;
            restorePoints.Add(restorePoint);

            Vector3 scale = particle.transform.lossyScale;

			//Need fixed

//            float averageScale = (scale.x + scale.y + scale.z) / 3.0f;
//			particle.startSize *= averageScale;
//            particle.startSpeed *= averageScale;
        }

        return new VFXScaleRestorer(restorePoints);
    }

    public static void EnableKeyword(this Material material, string keyword, bool enabled)
    {
        if (enabled) {
            material.EnableKeyword(keyword);
        } else {
            material.DisableKeyword(keyword);
        }
    }
}

/// <summary>
/// Used to restore ParticleSystems to their original values
/// after being scaled using VFXUtil.ScaleVFX(GameObject).
/// </summary>
public class VFXScaleRestorer
{
    private IEnumerable<VFXScaleRestorePoint> _restorePoints;
    private bool _hasBeenRestored;

    public bool HasBeenRestored { get { return _hasBeenRestored; } }

    public VFXScaleRestorer(IEnumerable<VFXScaleRestorePoint> restorePoints)
    {
        _restorePoints = restorePoints;
    }

    public void Restore()
    {
        if (_hasBeenRestored)
        {
            return;
        }

        foreach (VFXScaleRestorePoint restorePoint in _restorePoints)
        {
			ParticleSystem.MainModule main = restorePoint.Source.main;
			
            main.startSize = restorePoint.OriginalStartSize;
			main.startSpeed = restorePoint.OriginalStartSpeed;
        }
        _hasBeenRestored = true;
    }
}

public struct VFXScaleRestorePoint
{
    public ParticleSystem Source;
	public ParticleSystem.MinMaxCurve OriginalStartSize;
	public ParticleSystem.MinMaxCurve OriginalStartSpeed;
}