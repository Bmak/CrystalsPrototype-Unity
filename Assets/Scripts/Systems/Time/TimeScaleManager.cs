using UnityEngine;

/// <summary>
/// This class is responsible for managing changes to Unity's time scale
/// while remembering the previous time scale and resetting to that previous
/// time scale
/// </summary>
public class TimeScaleManager : ILoggable
{
    private float _defaultTimescale = 1.0f;

    public void SetTimeScale(float timeScale)
    {
        Time.timeScale = timeScale;
    }

    public void ResetTimeScale()
    {
        Time.timeScale = _defaultTimescale;
    }
}
