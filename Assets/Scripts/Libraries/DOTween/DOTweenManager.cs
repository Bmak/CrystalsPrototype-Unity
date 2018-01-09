using DG.Tweening;
using UnityEngine;
using System.Collections;

/// <summary>
/// Manages the DOTween plugin.
/// For documentation: http://dotween.demigiant.com/documentation.php
/// </summary>
public class DOTweenManager : IInitializable, ILifecycleAware
{
    private const bool RECYCLE_ALL_BY_DEFAULT = false;
    private const bool USE_SAFE_MODE = true;

    [Inject]
    private Client _client;

    public void Initialize(InstanceInitializedCallback initializedCallback = null)
    {
        DOTween.Init(RECYCLE_ALL_BY_DEFAULT, USE_SAFE_MODE, GetLogBehaviour());

        if(initializedCallback != null) {
            initializedCallback(this);
        }
    }

    public void Reset()
    {
        DOTween.Clear(destroy:true);
    }

    private LogBehaviour GetLogBehaviour()
    {
        if(!_client.IsDebug()) {
            return LogBehaviour.ErrorsOnly;
        } else if(_client.GetLogLevel() >= LogLevel.INFO) {
            return LogBehaviour.Verbose;
        } else if(_client.GetLogLevel() >= LogLevel.INFO) {
            return LogBehaviour.Default;
        } else {
            return LogBehaviour.ErrorsOnly;
        }
    }
}
